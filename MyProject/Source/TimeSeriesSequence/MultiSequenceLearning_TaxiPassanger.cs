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
using static TimeSeriesSequence.MultisequenceLearningTest;

namespace TimeSeriesSequence
{
    public class MultiSequenceLearning_TaxiPassanger
    {

        string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"DataSet\");

        /// <summary>
        /// Prediction of taxi passangers based on data set
        /// </summary>
        public void RunPassangerTimeSeriesSequenceExperiment()
        {

            int inputBits = 88;
            int maxCycles = 15;
            int numColumns = 1024;

            //Read the taxi data set and write into new processed csv with reuired column
            var taxiData = HelperMethods.ProcessExistingDatafromCSVfile(path);

            List<Dictionary<string, int[]>> trainTaxiData = HelperMethods.EncodePassengerData(taxiData);

            EncoderBase encoder = HelperMethods.FetchDateTimeEncoder();


            Console.WriteLine("Starting to learn Taxi Passanger data");

            var learningExpResult = RunExperiment(inputBits, maxCycles, numColumns, encoder, trainTaxiData);

            CortexLayer<object, object> trainedCortexLayer = learningExpResult.Keys.ElementAt(0);
            HtmClassifier<string, ComputeCycle>  trainedClassifier = learningExpResult.Values.ElementAt(0);

            Console.WriteLine("Complete Learning");

            Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
           
            var userInputDateTime = Console.ReadLine();

            RunPassangerPrediction(userInputDateTime, trainedCortexLayer, trainedClassifier);
        }

        /// <summary>
        /// Predict the no of passanger based on user Input Date time
        /// </summary>
        /// <param name="userInputDateTime"></param>
        /// <param name="trainedPassangerCortexLayer"></param>
        /// <param name="trainedPassangerClassifier"></param>
        private void RunPassangerPrediction(string? userInputDateTime, CortexLayer<object, object> trainedPassangerCortexLayer, HtmClassifier<string, ComputeCycle> trainedPassangerClassifier)
        {
            if (userInputDateTime != null)
            {
                DateTime userInput = DateTime.Parse(userInputDateTime);

                var sdr = HelperMethods.GetSDRofDateTime(userInput);
                var userLayerOutput = trainedPassangerCortexLayer.Compute(sdr, false) as ComputeCycle;
                var predictedValuesForUserInput = trainedPassangerClassifier.GetPredictedInputValues(userLayerOutput.PredictiveCells.ToArray(), 5);
                foreach (var predictedVal in predictedValuesForUserInput)
                {
                    Console.WriteLine("PASSANGER SIMILARITY " + predictedVal.Similarity + " PREDICTED PASSANGER NO :" + predictedVal.PredictedInput);
                }
            }

            else
                Console.WriteLine("Enter valid Date Time for Passanger Predicting");

        }

        private static Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>> RunExperiment(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string, int[]>> trainTaxiData)
        {
            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;

            var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);

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

                foreach (var sequence in trainTaxiData)
                {
                    //foreach (var input in data)
                    //{
                    //    Debug.WriteLine($" -- {data.} - {input} --");

                    //    var lyrOut = layer1.Compute(input, true);

                    //    if (isInStableState)
                    //        break;
                    //}

                    foreach (var element in sequence)
                    {
                        var observationClass = element.Key; // OBSERVATION LABEL || SEQUENCE LABEL
                        var elementSDR = element.Value; // ALL ELEMENT IN ONE SEQUENCE

                        Console.WriteLine($"-------------- {observationClass} ---------------");

                        var lyrOut = layer1.Compute(elementSDR, true);     /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = TRUE */
                        //var lyrOut = layer1.Compute(elementSDR, learn);    /* CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = if TRUE */

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

            string lastPredictedValue = "-1";
            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();

            //
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
                        var observationLabel = Elements.Key;

                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(Elements.Value, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));

                        List<Cell> actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());

                        if (lastPredictedValue == observationLabel && lastPredictedValue != "")
                        {
                            elementMatches++;
                            Debug.WriteLine($"Match. Actual value: {observationLabel} - Predicted value: {lastPredictedValue}");
                        }
                        else
                        {
                            Debug.WriteLine($"Mismatch! Actual value: {observationLabel} - Predicted values: {lastPredictedValue}");
                        }

                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        if (learn == false)
                            Debug.WriteLine($"Inference mode");

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            Debug.WriteLine($"Current Input: {observationLabel}");
                            Debug.WriteLine("The predictions with similarity greater than 50% are");

                            foreach (var t in predictedInputValue)
                            {

                                if (t.Similarity >= (double)50.00)
                                {
                                    Debug.WriteLine($"Predicted Input: {string.Join(", ", t.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", t.Similarity)}, \tNumber of Same Bits: {string.Join(", ", t.NumOfSameBits)}");
                                }
                            }

                            lastPredictedValue = predictedInputValue.First().PredictedInput;

                        }
                    }

                    accuracy = ((double)elementMatches / (sequence.Count)) * 100;
                    Debug.WriteLine($"Cycle : {i} \t Accuracy:{accuracy}");
                    tempLOGGRAPH.Add(i, accuracy);
                    if (accuracy == 100)
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
            string filename = now.ToString("g"); //

            filename = "TaxiPassangerPredictionExperiment" + filename.Split(" ")[0] + "_" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

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

            return returnDictionary;

            // return new HtmPredictionEngine { Layer = layer1, Classifier = cls, Connections = mem };

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
        /// Read the datas from taxi data set and process it
        /// </summary>

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
