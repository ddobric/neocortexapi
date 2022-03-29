using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Diagnostics;
using TimeSeriesSequence.HelperMethods;

namespace TimeSeriesSequence
{
    public class MultiSequenceTaxiPassanger
    {
        string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, $"DataSet/");

        /// <summary>
        /// Prediction of taxi passangers based on data set
        /// </summary>
        public void RunPassangerTimeSeriesSequenceExperiment()
        {
            int inputBits = 88;
            int maxCycles = 80;
            int numColumns = 1024;

            // Read the taxi data set and write into new processed csv with reuired column
            var taxiData = CSVPRocessingMethods.ProcessExistingDatafromCSVfile(path);

            // Encode the processed taxi passanger data
            List<Dictionary<string, int[]>> trainTaxiData = DateTimeEncoders.EncodePassengerData(taxiData);

            // Encode with multi encoder
            EncoderBase encoder = DateTimeEncoders.FetchDateTimeEncoder();

            // Run experiment with trained taxi passanger data
            var learningExpResult = RunExperiment(inputBits, maxCycles, numColumns, encoder, trainTaxiData);

            // Predic no of passanger based on user Input
            PredictPassanagerWithUserInput(learningExpResult);
        }

        /// <summary>
        /// Taking user input and sending for prediction
        /// </summary>
        /// <param name="learningExpResult"></param>
        private void PredictPassanagerWithUserInput(HtmPredictionEngine learningExpResult)
        {
            Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
            var userInputDateTime = Console.ReadLine();

            while (!userInputDateTime!.Equals("e") && userInputDateTime != "E")
            {
                RunPassangerPrediction(userInputDateTime, learningExpResult);

                Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
                userInputDateTime = Console.ReadLine();
            }
        }

        /// <summary>
        /// Predict the no of passanger based on user Input Date time
        /// </summary>
        /// <param name="userInputDateTime"></param>
        /// <param name="learningExpResult"></param>
        private void RunPassangerPrediction(string userInputDateTime, HtmPredictionEngine learningExpResult)
        {
            if (userInputDateTime != null)
            {
                DateTime userInput = DateTime.Parse(userInputDateTime);
                // Encode the user input and return the SDR
                var sdr = DateTimeEncoders.GetSDRofDateTime(userInput);
                var predictedValuesForUserInput = learningExpResult.Predict(sdr);
                foreach (var predictedVal in predictedValuesForUserInput)
                {
                    Console.WriteLine("PASSANGER SIMILARITY " + predictedVal.Similarity + " PREDICTED PASSANGER NO :" + predictedVal.PredictedInput);
                }
            }

            else
                Console.WriteLine("Enter valid Date Time for Passanger Predicting");
        }
        
        /// <summary>
        /// Run experiment based on the taxi passanger data
        /// </summary>
        /// <param name="inputBits"></param>
        /// <param name="maxCycles"></param>
        /// <param name="numColumns"></param>
        /// <param name="encoder"></param>
        /// <param name="trainTaxiData"></param>
        /// <returns></returns>
        private static HtmPredictionEngine RunExperiment(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string, int[]>> trainTaxiData)
        {
            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            Stopwatch sw = new Stopwatch();

            trainTaxiData = trainTaxiData.Take(50).ToList();

            sw.Start();

            //var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);
            var htmConfig = new HtmConfig(new int[] { inputBits }, new int[] { numColumns });

            var mem = new Connections(htmConfig);

            bool isInStableState = false;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numUniqueInputs = trainTaxiData.Count;

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
            //layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            //double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];
            bool learn = true;
            int cycle = 0;
            var lastPredictedValues = new List<string>(new string[] { "0" });

            // Training SP to get stable. New-born stage.
            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var sequence in trainTaxiData)
                {
                    foreach (var element in sequence)
                    {
                        string[] splitKey = element.Key.Split(",");
                        var observationClass = splitKey[0]; // OBSERVATION LABEL || SEQUENCE LABEL
                        var elementSDR = element.Value; // ALL ELEMENT IN ONE SEQUENCE

                        Console.WriteLine($"-------------- {observationClass} ---------------");

                        var lyrOut = layer1.Compute(elementSDR, true);     /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = TRUE */

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

            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();

            // Loop over all sequences.
            foreach (var sequence in trainTaxiData)
            {
                int SequencesMatchCount = 0; // NUMBER OF MATCHES
                var tempLOGFILE = new Dictionary<int, string>();
                var tempLOGGRAPH = new Dictionary<int, double>();
                double SaturatedAccuracyCount = 0;

                for (int i = 0; i < maxCycles; i++)
                {
                    List<string> ElementWiseClasses = new List<string>();

                    int elementMatches = 0;

                    foreach (var Elements in sequence)
                    {
                        string[] splitKey = Elements.Key.Split(",");
                        var observationLabel = splitKey[0];

                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(Elements.Value, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut!.ActivColumnIndicies));

                        List<Cell> actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());

                        if (lastPredictedValues.Contains(observationLabel))
                        {
                            elementMatches++;
                            Debug.WriteLine($"Match. Actual value: {observationLabel} - Predicted value: {lastPredictedValues.FirstOrDefault(observationLabel)}.");
                        }
                        else
                            Debug.WriteLine($"Missmatch! Actual value: {observationLabel} - Predicted values: {String.Join(',', lastPredictedValues)}");


                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        if (learn == false)
                            Debug.WriteLine($"Inference mode");

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Debug.WriteLine($"Current Input: {Elements} \t| Predicted Input: {item.PredictedInput} - {item.Similarity}");
                            }

                            lastPredictedValues = predictedInputValues.Select(v => v.PredictedInput).ToList();
                        }
                        else
                        {
                            Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValues = new List<string>();
                        }
                    }

                    accuracy = ((double)elementMatches / (sequence.Count)) * 100;
                    Debug.WriteLine($"Cycle : {i} \t Accuracy:{accuracy}");
                    tempLOGGRAPH.Add(i, accuracy);
                    
                    if (accuracy >= 100)
                    {
                        SequencesMatchCount++;
                        if (SequencesMatchCount >= 30)
                        {
                            tempLOGFILE.Add(i, $"Cycle : {i} \t  Accuracy:{accuracy} \t Number of times repeated {SequencesMatchCount}");
                            break;
                        }
                        tempLOGFILE.Add(i, $"Cycle : {i} \t  Accuracy:{accuracy} \t Number of times repeated {SequencesMatchCount}");

                    }
                    else if (lastCycleAccuracy == accuracy && accuracy != 0)
                    {
                        SaturatedAccuracyCount++;
                        if (SaturatedAccuracyCount >= 20 && lastCycleAccuracy > 70)
                        {
                            Debug.WriteLine($"NO FURTHER ACCURACY CAN BE ACHIEVED");
                            Debug.WriteLine($"Saturated Accuracy : {lastCycleAccuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                            tempLOGFILE.Add(i, $"Cycle: { i} \t Accuracy:{accuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                            break;
                        }
                        else
                        {
                            tempLOGFILE.Add(i, $"Cycle: { i} \t Saturated Accuracy : {lastCycleAccuracy} \t Number of times repeated {SaturatedAccuracyCount}");
                        }
                    }
                    else
                    {
                        SaturatedAccuracyCount = 0;
                        SequencesMatchCount = 0;
                        lastCycleAccuracy = accuracy;
                        tempLOGFILE.Add(i, $"cycle : {i} \t Accuracy :{accuracy} \t ");
                    }
                    lastPredictedValueList.Clear();

                }

                tm.Reset(mem);
                learn = true;
                OUTPUT_LOG_LIST.Add(tempLOGFILE);
            }

            sw.Stop();

            //****************DISPLAY STATUS OF EXPERIMENT
            Debug.WriteLine("-------------------TRAINING END------------------------");
            Console.WriteLine("-----------------TRAINING END------------------------");
            Debug.WriteLine("-------------------WRTING TRAINING OUTPUT LOGS---------------------");
            Console.WriteLine("-------------------WRTING TRAINING OUTPUT LOGS------------------------");
            //*****************

            DateTime now = DateTime.Now;
            string filename = now.ToString("g");

            //filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            //string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

            filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, $"TrainingLogs/{filename}");

            using (StreamWriter swOutput = File.CreateText(path))
            {
                swOutput.WriteLine($"{filename}");
                foreach (var SequencelogCycle in OUTPUT_LOG_LIST)
                {
                    swOutput.WriteLine("******Sequence Starting*****");
                    foreach (var cycleOutPutLog in SequencelogCycle)
                    {
                        swOutput.WriteLine(cycleOutPutLog.Value, true);
                    }
                    swOutput.WriteLine("****Sequence Ending*****");

                }
            }

            Debug.WriteLine("-------------------TRAINING LOGS HAS BEEN CREATED---------------------");
            Console.WriteLine("-------------------TRAINING LOGS HAS BEEN CREATED------------------------");

            var returnDictionary = new Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>>();
            returnDictionary.Add(layer1, cls);
            Console.WriteLine("Complete Learning");

            return new HtmPredictionEngine { Layer = layer1, Classifier = cls, Connections = mem };
        }
        
        public class HtmPredictionEngine
        {
            public void Reset()
            {
                var tm = this.Layer.HtmModules.FirstOrDefault(m => m.Value is TemporalMemory);
                ((TemporalMemory)tm.Value).Reset(this.Connections);
            }
            
            public List<ClassifierResult<string>> Predict(int[] input)
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
        /// HTM configuaration oblect initialization
        /// </summary>
        /// <param name="inputBits"></param>
        /// <param name="numColumns"></param>
        /// <returns></returns>
        public static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
        {
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
                PredictedSegmentDecrement = 0.1,

                //NumInputs = 88
            };

            return cfg;
        }
    }
}
