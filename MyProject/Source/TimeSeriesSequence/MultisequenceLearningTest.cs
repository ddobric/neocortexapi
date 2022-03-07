using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Experiments;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;


namespace TimeSeriesSequence
{
    /// <summary>
    /// Implements an experiment that demonstrates how to learn sequences.
    /// </summary>
    public class MultisequenceLearningTest
    {
        /// <summary>
        /// Runs the learning of sequences.
        /// </summary>
        /// <param name="sequences">Dictionary of sequences. KEY is the sewuence name, the VALUE is th elist of element of the sequence.</param>
        public HtmPredictionEngine Run(Dictionary<string, List<double>> sequences)
        {
            Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(MultisequenceLearningTest)}");

            int inputBits = 100;
            int numColumns = 1024;

            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
            };

            double max = 20;

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            Dictionary<string, Dictionary<string, object>> dateEncoderSetting = dateTimeEncoderSettings();

            EncoderBase encoder = new ScalarEncoder(settings);

            DateTimeEncoder dtEncoder = new DateTimeEncoder(dateEncoderSetting, DateTimeEncoder.Precision.Hours);

            return RunExperiment(inputBits, cfg, encoder, sequences);
        }


        /// <summary>
        /// Encoder settings for date and time
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, object>> dateTimeEncoderSettings()
        {
            Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();

            encoderSettings.Add("DayOfWeekEncoder",
                new Dictionary<string, object>()
                {
                    { "W", 3},
                    { "N", 128},
                    { "MinVal", 0.0},
                    { "MaxVal", 7.0},
                    { "Periodic", false},
                    { "Name", "DayOfWeekEncoder"},
                    { "ClipInput", false},
                    { "Offset", 50},
                });

            encoderSettings.Add("WeekendEncoder", new Dictionary<string, object>()
                {
                    { "W", 3},
                    { "N", 42},
                    { "MinVal", 0.0},
                    { "MaxVal", 54.0},
                    { "Periodic", false},
                    { "Name", "WeekendEncoder"},
                    { "ClipInput", true},
                    { "Offset", 50},
                });


            encoderSettings.Add("Segment", new Dictionary<string, object>()
                {
                    { "W", 3},
                    { "N", 8640},
                     // This means 8640 hours.
                    { "MinVal", 0.0},
                    { "MaxVal", 23.0},
                    { "Periodic", false},
                    { "Name", "Segment"},
                    { "ClipInput", false},
                    { "Offset", 128},
                });
            
            return encoderSettings;
        }

        /// <summary>
        ///
        /// </summary>
        private HtmPredictionEngine RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, Dictionary<string, List<double>> sequences)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;

            var mem = new Connections(cfg);

            bool isInStableState = false;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numUniqueInputs = GetNumberOfInputs(sequences);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            TemporalMemory tm = new TemporalMemory();

            // For more information see following paper: https://www.scitepress.org/Papers/2021/103142/103142.pdf
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                isInStableState = isStable;

                // Clear active and predictive cells.
                //tm.Reset(mem);
            }, numOfCyclesToWaitOnChange: 50);


            SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
            sp.Init(mem);
            tm.Init(mem);

            // Please note that we do not add here TM in the layer.
            // This is omitted for practical reasons, because we first eneter the newborn-stage of the algorithm
            // In this stage we want that SP get boosted and see all elements before we start learning with TM.
            // All would also work fine with TM in layer, but it would work much slower.
            // So, to improve the speed of experiment, we first ommit the TM and then after the newborn-stage we add it to the layer.
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            //double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            var lastPredictedValues = new List<string>(new string[] { "0" });

            int maxCycles = 3500;

            //
            // Training SP to get stable. New-born stage.
            //

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var inputs in sequences)
                {
                    foreach (var input in inputs.Value)
                    {
                        Debug.WriteLine($" -- {inputs.Key} - {input} --");

                        var lyrOut = layer1.Compute(input, true);

                        if (isInStableState)
                            break;
                    }

                    if (isInStableState)
                        break;
                }
            }

            // Clear all learned patterns in the classifier.
            cls.ClearState();

            // We activate here the Temporal Memory algorithm.
            layer1.HtmModules.Add("tm", tm);

            //
            // Loop over all sequences.
            foreach (var sequenceKeyPair in sequences)
            {
                Debug.WriteLine($"-------------- Sequences {sequenceKeyPair.Key} ---------------");

                int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1.0");

                //
                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    matches = 0;

                    cycle++;

                    Debug.WriteLine("");

                    Debug.WriteLine($"-------------- Cycle {cycle} ---------------");
                    Debug.WriteLine("");

                    foreach (var input in sequenceKeyPair.Value)
                    {
                        Debug.WriteLine($"-------------- {input} ---------------");

                        var lyrOut = layer1.Compute(input, true) as ComputeCycle;

                        var activeColumns = layer1.GetResult("sp") as int[];

                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > (maxPrevInputs + 1))
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input, sequenceKeyPair.Key);

                        List<Cell> actCells;

                        if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                        {
                            actCells = lyrOut.ActiveCells;
                        }
                        else
                        {
                            actCells = lyrOut.WinnerCells;
                        }

                        cls.Learn(key, actCells.ToArray());

                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        //
                        // If the list of predicted values from the previous step contains the currently presenting value,
                        // we have a match.
                        if (lastPredictedValues.Contains(key))
                        {
                            matches++;
                            Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValues.FirstOrDefault(key)}.");
                        }
                        else
                            Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted values: {String.Join(',', lastPredictedValues)}");

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {item.PredictedInput} - {item.Similarity}");
                            }

                            lastPredictedValues = predictedInputValues.Select(v => v.PredictedInput).ToList();
                        }
                        else
                        {
                            Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValues = new List<string>();
                        }
                    }

                    // The first element (a single element) in the sequence cannot be predicted
                    double maxPossibleAccuraccy = (double)((double)sequenceKeyPair.Value.Count - 1) / (double)sequenceKeyPair.Value.Count * 100.0;

                    double accuracy = (double)matches / (double)sequenceKeyPair.Value.Count * 100.0;

                    Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {sequenceKeyPair.Value.Count}\t {accuracy}%");

                    if (accuracy >= maxPossibleAccuraccy)
                    {
                        maxMatchCnt++;
                        Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");

                        //
                        // Experiment is completed if we are 30 cycles long at the 100% accuracy.
                        if (maxMatchCnt >= 30)
                        {
                            sw.Stop();
                            Debug.WriteLine($"Sequence learned. The algorithm is in the stable state after 30 repeats with with accuracy {accuracy} of maximum possible {maxMatchCnt}. Elapsed sequence {sequenceKeyPair.Key} learning time: {sw.Elapsed}.");
                            break;
                        }
                    }
                    else if (maxMatchCnt > 0)
                    {
                        Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with accuracy {accuracy}. This indicates instable state. Learning will be continued.");
                        maxMatchCnt = 0;
                    }

                    // This resets the learned state, so the first element starts allways from the beginning.
                    tm.Reset(mem);
                }
            }

            Debug.WriteLine("------------ END ------------");

            return new HtmPredictionEngine { Layer = layer1, Classifier = cls, Connections = mem };
        }

        public class HtmPredictionEngine
        {
            public void Reset()
            {
                var tm = this.Layer.HtmModules.FirstOrDefault(m => m.Value is TemporalMemory);
                ((TemporalMemory)tm.Value).Reset(this.Connections);
            }
            public List<ClassifierResult<string>> Predict(double input)
            {
                var lyrOut = this.Layer.Compute(input, false) as ComputeCycle;

                List<ClassifierResult<string>> predictedInputValues = this.Classifier.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                return predictedInputValues;
            }

            public Connections Connections { get; set; }

            public CortexLayer<object, object> Layer { get; set; }

            public HtmClassifier<string, ComputeCycle> Classifier { get; set; }
        }

        /// <summary>
        /// Gets the number of all unique inputs.
        /// </summary>
        /// <param name="sequences">Alle sequences.</param>
        /// <returns></returns>
        private int GetNumberOfInputs(Dictionary<string, List<double>> sequences)
        {
            int num = 0;

            foreach (var inputs in sequences)
            {
                //num += inputs.Value.Distinct().Count();
                num += inputs.Value.Count;
            }

            return num;
        }

        /// <summary>
        /// Constracts the unique key of the element of an sequece. This key is used as input for HtmClassifier.
        /// It makes sure that alle elements that belong to the same sequence are prefixed with the sequence.
        /// The prediction code can then extract the sequence prefix to the predicted element.
        /// </summary>
        /// <param name="prevInputs"></param>
        /// <param name="input"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return $"{sequence}_{key}";
        }
        public void RunPowerPredictionExperiment()
        {
            const int inputBits = 300; /* without datetime component */ // 13420; /* with 4096 scalar bits */ // 10404 /* with 1024 bits */;

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 2048 });
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10 /* 50 */);
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.CONNECTED_PERMANENCE, 0.1);
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.setNumActiveColumnsPerInhArea(0.02 * 2048);
            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10 /*15*/);
            p.setInhibitionRadius(15);
            p.Set(KEY.MAX_BOOST, 0.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100000);
            p.setActivationThreshold(10);
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * 2048));
            p.setPermanenceIncrement(0.17);

            //p.Set(KEY.MAX_SYNAPSES_PER_SEGMENT, 32);
            //p.Set(KEY.MAX_SEGMENTS_PER_CELL, 128);
            //p.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 200);

            //p.Set(KEY.POTENTIAL_RADIUS, 700);
            //p.Set(KEY.POTENTIAL_PCT, 0.5);
            //p.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 42);

            //p.Set(KEY.LOCAL_AREA_DENSITY, -1);

            CortexRegion region0 = new CortexRegion("1st Region");

            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.Init(mem, UnitTestHelpers.GetMemory());
            tm1.Init(mem);

            Dictionary<string, object> settings = new Dictionary<string, object>();

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("sp", sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Train(inputBits, layer1, cls, true); // New born mode.
            sw.Stop();

            Debug.WriteLine($"NewBorn stage duration: {sw.ElapsedMilliseconds / 1000} s");

            layer1.AddModule("tm", tm1);

            sw.Start();

            int hunderdAccCnt = 0;
            for (int i = 0; i < 1000; i++)
            {
                float acc = Train(inputBits, layer1, cls, false);

                Debug.WriteLine($"Accuracy = {acc}, Cycle = {i}");

                if (acc == 100.0)
                    hunderdAccCnt++;

                if (hunderdAccCnt >= 10)
                {
                    break;
                }
                //tm1.reset(mem);
            }

            if (hunderdAccCnt >= 10)
            {
                Debug.WriteLine($"EXPERIMENT SUCCESS. Accurracy 100% reached.");
            }
            else
            {
                Debug.WriteLine($"Experiment FAILED!. Accurracy 100% was not reached.");
            }

            cls.TraceState();

            sw.Stop();

            Debug.WriteLine($"Training duration: {sw.ElapsedMilliseconds / 1000} s");

        }

        double lastPredictedValue = 0.0;

        private float Train(int inpBits, IHtmModule<object, object> network, HtmClassifier<double, ComputeCycle> cls, bool isNewBornMode = true)
        {
            float accurracy;

            string outFolder = nameof(RunPowerPredictionExperiment);

            Directory.CreateDirectory(outFolder);

            CortexNetworkContext ctx = new CortexNetworkContext();

            Dictionary<string, object> scalarEncoderSettings = getScalarEncoderDefaultSettings(inpBits);
            //var dateTimeEncoderSettings = getFullDateTimeEncoderSettings();

            ScalarEncoder scalarEncoder = new ScalarEncoder(scalarEncoderSettings);
            //DateTimeEncoder dtEncoder = new DateTimeEncoder(dateTimeEncoderSettings, DateTimeEncoder.Precision.Hours);

            string fileName = "TestFiles\\rec-center-hourly-short.csv";

            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                int cnt = 0;
                int matches = 0;

                using (StreamWriter sw = new StreamWriter("out.csv"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        cnt++;

                        //if (isNewBornMode && cnt > 100) break;

                        bool x = false;
                        if (x)
                        {
                            break;
                        }
                        List<int> output = new List<int>();

                        string[] tokens = line.Split(",");

                        // Encode scalar value
                        var result = scalarEncoder.Encode(tokens[1]);

                        output.AddRange(result);

                        // This part adds datetime components to the input vector.
                        //output.AddRange(new int[scalarEncoder.Offset]);
                        //DateTime dt = DateTime.Parse(tokens[0], CultureInfo.InvariantCulture);
                        // Encode date/time/hour.
                        //result = dtEncoder.Encode(new DateTimeOffset(dt, TimeSpan.FromMilliseconds(0)));
                        //output.AddRange(result);

                        // This performs a padding to the inputBits = 10404 = 102*102.
                        output.AddRange(new int[inpBits - output.Count]);

                        var outArr = output.ToArray();

                        Debug.WriteLine($"-------------- {tokens[1]} --------------");

                        if (isNewBornMode)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                // Output here are active cells.
                                var res = network.Compute(output.ToArray(), true);

                                Debug.WriteLine(Helpers.StringifyVector(((int[])res)));
                            }
                        }
                        else
                        {
                            var lyrOut = network.Compute(output.ToArray(), true) as ComputeCycle; ;

                            double input = Convert.ToDouble(tokens[1], CultureInfo.InvariantCulture);

                            if (input == lastPredictedValue)
                            {
                                matches++;
                                Debug.WriteLine($"Match {input}");
                            }
                            else
                                Debug.WriteLine($"Missmatch Actual value: {input} - Predicted value: {lastPredictedValue}");

                            cls.Learn(input, lyrOut.ActiveCells.ToArray());

                            lastPredictedValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                            sw.WriteLine($"{tokens[0]};{input.ToString(CultureInfo.InvariantCulture)};{lastPredictedValue.ToString(CultureInfo.InvariantCulture)}");

                            Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                            Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.PredictiveCells.Select(c => c.Index).ToArray())}");
                            Debug.WriteLine($"Current input: {input} Predicted Input: {lastPredictedValue}");

                            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(outArr, (int)Math.Sqrt(outArr.Length), (int)Math.Sqrt(output.Count));
                            var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
                            NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{tokens[0].Replace("/", "-").Replace(":", "-")}.png", Color.Yellow, Color.Black, text: input.ToString());

                        }

                        Debug.WriteLine($"NewBorn stage: {isNewBornMode} - record: {cnt}");

                    }
                }

                accurracy = (float)matches / (float)cnt * (float)100.0;
            }

            return accurracy;
        }




        /// <summary>
        /// The getDefaultSettings
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, object}"/></returns>
        private static Dictionary<string, object> getScalarEncoderDefaultSettings(int inputBits)
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("W", 15 /*21*/);                       //the number of bits that are set to encode a single value -the "width" of the output signal 
                                                                       //restriction: w must be odd to avoid centering problems.
            encoderSettings.Add("N", inputBits /*4096*/);                     //The number of bits in the output. Must be greater than or equal to w
            encoderSettings.Add("MinVal", (double)0.0);         //The minimum value of the input signal.
            encoderSettings.Add("MaxVal", (double)60);       //The upper bound of the input signal
                                                             //encoderSettings.Add("Radius", (double)0);         //Two inputs separated by more than the radius have non-overlapping representations.
                                                             //Two inputs separated by less than the radius will in general overlap in at least some
                                                             //of their bits. You can think of this as the radius of the input.
                                                             //encoderSettings.Add("Resolution", (double)0.15);  // Two inputs separated by greater than, or equal to the resolution are guaranteed
                                                             //to have different representations.
            encoderSettings.Add("Periodic", (bool)false);        //If true, then the input value "wraps around" such that minval = maxval
                                                                 //For a periodic value, the input must be strictly less than maxval,
                                                                 //otherwise maxval is a true upper bound.
            encoderSettings.Add("ClipInput", (bool)false);       //if true, non-periodic inputs smaller than minval or greater than maxval 
                                                                 //will be clipped to minval/maxval

            encoderSettings.Add("Offset", 108);

            return encoderSettings;
        }



        /// <summary>
        /// Values for the radius and width will be passed here. For this unit test the width = 1 and radius = 1.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, object>> getFullDateTimeEncoderSettings()
        {
            Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();

            encoderSettings.Add("SeasonEncoder",
              new Dictionary<string, object>()
              {
                    { "W", 21},
                    { "N", 128},
                    //{ "Radius", 365/4},
                    { "MinVal", 1.0},
                    { "MaxVal", 367.0},
                    { "Periodic", true},
                    { "Name", "SeasonEncoder"},
                    { "ClipInput", true},
                    { "Offset", 50},
              }
              );

            encoderSettings.Add("DayOfWeekEncoder",
                new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 128},
                    { "MinVal", 0.0},
                    { "MaxVal", 7.0},
                    { "Periodic", false},
                    { "Name", "DayOfWeekEncoder"},
                    { "ClipInput", false},
                    { "Offset", 50},
                });

            encoderSettings.Add("WeekendEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 42},
                    { "MinVal", 0.0},
                    { "MaxVal", 1.0},
                    { "Periodic", false},
                    { "Name", "WeekendEncoder"},
                    { "ClipInput", true},
                    { "Offset", 50},
                });

            encoderSettings.Add("ThreeDaysLongWeekendEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 42},
                    { "MinVal", 0.0},
                    { "MaxVal", 3.0},
                    { "Periodic", false},
                    { "Name", "ThreeDaysLongWeekendEncoder"},
                    { "ClipInput", true},
                    { "Offset", 50},
                });

            encoderSettings.Add("DateTimeEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 8640},
                     // This means 8640 hours.
                    { "MinVal", new DateTimeOffset(new DateTime(2010, 1, 1), TimeSpan.FromHours(0))},
                    { "MaxVal", new DateTimeOffset(new DateTime(2011, 1, 1), TimeSpan.FromHours(0))},
                    { "Periodic", false},
                    { "Name", "DateTimeEncoder"},
                    { "ClipInput", false},
                    { "Offset", 128},
                });

            return encoderSettings;
        }
    }
}
