using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Network;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace TimeSeriesSequence
{
    public class MultiSequenceLearning_TaxiPassanger
    {
        /// <summary>
        /// Prediction of taxi passangers based on data set
        /// </summary>
        public void RunPassangerTimeSeriesSequenceExperiment()
        {
            int inputBits = 72;
            int maxCycles = 15;
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

            //Read the taxi data set and write into new processed csv with reuired column
            var taxiData = ProcessExistingDatafromCSVfile();

            var trainTaxiData = (Dictionary<string, List<double>>) HelperMethods.EncodePassengerData(taxiData);

            EncoderBase encoder = HelperMethods.FetchDateTimeEncoder();

            // var trained_HTM_model = Run(inputBits, maxCycles, numColumns, trainingDataProcessed, false);
            //var trained_HTM_model1 =  
            RunExperiment(inputBits, maxCycles, numColumns, cfg, encoder, trainTaxiData);
        }

        private static void RunExperiment(int inputBits, int maxCycles, int numColumns, HtmConfig cfg, EncoderBase encoder, Dictionary<string, List<double>> trainTaxiData)
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;

            var mem = new Connections(cfg);

            bool isInStableState = false;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numUniqueInputs = GetNumberOfInputs(trainTaxiData);

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

            // int maxCycles = 3500;

            //
            // Training SP to get stable. New-born stage.
            //

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var inputs in trainTaxiData)
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
            foreach (var sequenceKeyPair in trainTaxiData)
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
                }
            }
        }

        /// <summary>
        /// Read the datas from taxi data set and process it
        /// </summary>
        private static List<object> ProcessExistingDatafromCSVfile()
        {
            List<TaxiData> taxiDatas = new List<TaxiData>();
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"DataSet\");

            using (StreamReader sr = new StreamReader(path + "2021_Green.csv"))
            {
                string line = string.Empty;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    string[] strRow = line.Split(','); ;
                    TaxiData taxiData = new TaxiData();
                    if (strRow[7] != "")
                    {
                        taxiData.lpep_pickup_datetime = Convert.ToDateTime(strRow[1]);
                        taxiData.passenger_count = Convert.ToInt32(strRow[7]);
                        taxiDatas.Add(taxiData);
                    }
                }
            }

            var processedTaxiData = CreateProcessedCSVFile(taxiDatas, path);

            return processedTaxiData;
        }

        /// <summary>
        /// Create the processed CSV file with required column
        /// </summary>
        /// <param name="taxiDatas"></param>
        /// <param name="path"></param>
        private static List<object> CreateProcessedCSVFile(List<TaxiData> taxiDatas, string path)
        {
            List<ProcessedData> processedTaxiDatas = new List<ProcessedData>();

            foreach (var item in taxiDatas)
            {
                var pickupTime = item.lpep_pickup_datetime.ToString("HH:mm");
                Slot result = GetSlot(pickupTime, HelperMethods.GetSlots());

                ProcessedData processedData = new ProcessedData();
                processedData.Date = item.lpep_pickup_datetime.Date;
                processedData.TimeSpan = result.StartTime.ToString() + " - " + result.EndTime.ToString();
                processedData.Segment = result.Segment;
                processedData.Passanger_count = item.passenger_count;
                processedTaxiDatas.Add(processedData);
            }

            var accumulatedPassangerData = processedTaxiDatas.GroupBy(c => new
            {
                c.Date,
                c.Segment
            }).Select(
                        g => new
                        {
                            Date = g.First().Date,
                            TimeSpan = g.First().TimeSpan,
                            Segment = g.First().Segment,
                            Passsanger_Count = g.Sum(s => s.Passanger_count),
                        }).AsEnumerable()
                          .Cast<dynamic>();


            StringBuilder csvcontent = new StringBuilder();
            csvcontent.AppendLine("Pickup_Date,TimeSpan,Segment,Passenger_count");
            foreach (var taxiData in accumulatedPassangerData)
            {
                var newLine = string.Format("{0},{1},{2},{3}", taxiData.Date, taxiData.TimeSpan, taxiData.Segment, taxiData.Passsanger_Count);
                csvcontent.AppendLine(newLine);
            }

            // Delete the existing file to avoid duplicate records.
            if (File.Exists(path + "2021_Green_Processed.csv"))
            {
                File.Delete(path + "2021_Green_Processed.csv");
            }

            // Save processed CSV data
            File.AppendAllText(path + "2021_Green_Processed.csv", csvcontent.ToString());

            return accumulatedPassangerData.ToList();
        }

        /// <summary>
        /// Get the slot based on pick up time
        /// </summary>
        /// <param name="pickupTime"></param>
        /// <param name="timeSlots"></param>
        /// <returns></returns>
        private static Slot GetSlot(string pickupTime, List<Slot> timeSlots)
        {
            var time = TimeSpan.Parse(pickupTime);
            Slot slots = timeSlots.FirstOrDefault(x => x.EndTime >= time && x.StartTime <= time);

            return slots;
        }

        /// <summary>
        /// Gets the number of all unique inputs.
        /// </summary>
        /// <param name="sequences">Alle sequences.</param>
        /// <returns></returns>
        private static int GetNumberOfInputs(Dictionary<string, List<double>> sequences)
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
    }
}
