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
    public class MultiSequenceTaxiPassanger
    {

        string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"DataSet/");

        /// <summary>
        /// Prediction of taxi passangers based on data set
        /// </summary>
        public void RunPassangerTimeSeriesSequenceExperiment()
        {
            int inputBits = 88;
            int maxCycles = 50;
            int numColumns = 1024;

            //Read the taxi data set and write into new processed csv with reuired column
            var taxiData = HelperMethods.ProcessExistingDatafromCSVfile(path);

            //Encode the processed taxi passanger data
            List<Dictionary<string, int[]>> trainTaxiData = HelperMethods.EncodePassengerData(taxiData);

            EncoderBase encoder = HelperMethods.FetchDateTimeEncoder();

            Console.WriteLine("Starting to learn Taxi Passanger data");
            //var learningExpResult = RunExperiment4(inputBits, maxCycles, numColumns, encoder, trainTaxiData);
            var learningExpResult2 = Run(inputBits, maxCycles, numColumns, trainTaxiData, false);
            //var learningExpResult1 = RunExperiment1(inputBits, numColumns, encoder, trainTaxiData);
            CortexLayer<object, object> trainedCortexLayer = learningExpResult2.Keys.ElementAt(0);
            HtmClassifier<string, ComputeCycle>  trainedClassifier = learningExpResult2.Values.ElementAt(0);
            Console.WriteLine("Complete Learning");

            //Predict Data with User input
            Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
            var userInputDateTime = Console.ReadLine();

            while (!userInputDateTime.Equals("e") && userInputDateTime != "E")
            {
                RunPassangerPrediction(userInputDateTime, trainedCortexLayer, trainedClassifier);

                Console.WriteLine("PLEASE ENTER DATE and TIME FOR PREDICTING TAXI PASSANGER:      *note format->dd/mm/yyyy hh:00");
                userInputDateTime = Console.ReadLine();
            }
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

        private static Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>> RunExperiment4(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string, int[]>> sequences)
        {
            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            Stopwatch sw = new Stopwatch();

            sequences = sequences.Take(1000).ToList();

            sw.Start();

            int maxMatchCnt = 0;

            var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);

            var mem = new Connections(htmConfig);

            bool isInStableState = false;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numUniqueInputs = sequences.Count;

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

            // Training SP to get stable. New-born stage.

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var sequence in sequences)
                {
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

            // ADDING TEMPORAL MEMEORY to CORTEX LAYER
            layer1.HtmModules.Add("tm", tm);

            string lastPredictedValue = "-1";
            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();
            // TRAINING SP+TM TOGETHER
            foreach (var sequence in sequences)  // SEQUENCE LOOP
            {
                int SequencesMatchCount = 0; // NUMBER OF MATCHES
                var tempLOGFILE = new Dictionary<int, string>();
                var tempLOGGRAPH = new Dictionary<int, double>();
                double SaturatedAccuracyCount = 0;


                for (int i = 0; i < maxCycles; i++) // MAXCYCLE LOOP 
                {
                    //var ElementWisePrediction = new List<List<HtmClassifier<string, ComputeCycle>.ClassifierResult>>();
                    List<string> ElementWiseClasses = new List<string>();

                    // ELEMENT IN SEQUENCE MATCHES COUNT
                    int ElementMatches = 0;

                    foreach (var Elements in sequence) // SEQUENCE DICTIONARY LOOP
                    {

                        // OBSERVATION LABEl
                        var observationLabel = Elements.Key;
                        // ELEMENT SDR LIST FOR A SINGLE SEQUENCE
                        var ElementSdr = Elements.Value;

                        List<Cell> actCells = new List<Cell>();
                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(ElementSdr, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));

                        // Active Cells
                        actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());


                        // CLASS VOTING IS USED FOR SEQUENCE CLASSIFICATION EXPERIMENT i.e CANCER SEQUENCE CLASSIFICATION EXPERIMENT
                       
                            if (lastPredictedValueList.Contains(observationLabel))
                            {
                                ElementMatches++;
                                lastPredictedValueList.Clear();
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

                        }

                    }

                    accuracy = ((double)ElementMatches / (sequence.Count)) * 100;
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

            filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

            //filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            // string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"TrainingLogs/{filename}");

            using (StreamWriter swOutput = File.CreateText(path + filename))
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

        private static Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>> RunExperiment(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string, int[]>> trainTaxiData)
        {
            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            Stopwatch sw = new Stopwatch();

            trainTaxiData = trainTaxiData.Take(1000).ToList();

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

            // Training SP to get stable. New-born stage.

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var sequence in trainTaxiData)
                {
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

                        //if (lyrOut.PredictiveCells.Count > 0)
                        //{
                        //    var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                        //    Debug.WriteLine($"Current Input: {observationLabel}");
                        //    Debug.WriteLine("The predictions with similarity greater than 50% are");

                        //    foreach (var t in predictedInputValue)
                        //    {

                        //        if (t.Similarity >= (double)50.00)
                        //        {
                        //            Debug.WriteLine($"Predicted Input: {string.Join(", ", t.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", t.Similarity)}, \tNumber of Same Bits: {string.Join(", ", t.NumOfSameBits)}");
                        //        }
                        //    }

                        //    lastPredictedValue = predictedInputValue.First().PredictedInput;

                        //}
                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
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
            string filename = now.ToString("g"); //

            filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

            //filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            // string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"TrainingLogs/{filename}");

            using (StreamWriter swOutput = File.CreateText(path + filename))
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

        public Dictionary<CortexLayer<object, object>, HtmClassifier<string, ComputeCycle>> Run(int inputBits, int maxCycles, int numColumns, List<Dictionary<string, int[]>> Sequences, Boolean classVotingEnabled)
        {
            //-----------HTM CONFG
            var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);
            //--------- CONNECTIONS
            var mem = new Connections(htmConfig);

            Sequences = Sequences.Take(200).ToList();

            // HTM CLASSIFIER
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            // CORTEX LAYER
            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            // HPA IS_IN_STABLE STATE FLAG
            bool isInStableState = false;
            // LEARNING ACTIVATION FLAG
            bool learn = true;

            // NUMBER OF NEW BORN CYCLES
            int newbornCycle = 0;

            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();
            // HOMOSTATICPLASTICITY CONTROLLER
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, Sequences.Count, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Console.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Console.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                learn = isInStableState = isStable;

                // Clear all learned patterns in the classifier.
                //cls.ClearState();

            }, numOfCyclesToWaitOnChange: 30);

            // SPATIAL POOLER initialization with HomoPlassiticityController using connections.
            SpatialPoolerMT sp = new SpatialPoolerMT(hpa);
            sp.Init(mem);

            // TEMPORAL MEMORY initialization using connections.
            TemporalMemory tm = new TemporalMemory();
            tm.Init(mem);

            // ADDING SPATIAL POOLER TO CORTEX LAYER
            layer1.HtmModules.Add("sp", sp);

            // CONTRAINER FOR Previous Active Columns
            int[] prevActiveCols = new int[0];

            // Starting experiment
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // TRAINING SP till STATBLE STATE IS ACHIEVED
            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                newbornCycle++;
                Console.WriteLine($"-------------- Newborn Cycle {newbornCycle} ---------------");

                foreach (var sequence in Sequences) // FOR EACH SEQUENCE IN SEQUNECS LOOP ::: LOOP - 1
                {
                    foreach (var Element in sequence) // FOR EACH dictionary containing single sequence Details LOOP ::: LOOP - 2
                    {
                        var observationClass = Element.Key; // OBSERVATION LABEL || SEQUENCE LABEL
                        var elementSDR = Element.Value; // ALL ELEMENT IN ONE SEQUENCE 

                        Console.WriteLine($"-------------- {observationClass} ---------------");
                        var result = sp.Compute(elementSDR, learn);
                        // CORTEX LAYER OUTPUT with elementSDR as INPUT and LEARN = TRUE
                        var lyrOut = layer1.Compute(elementSDR, learn);

                        // IF STABLE STATE ACHIEVED BREAK LOOP - 3
                        if (isInStableState)
                            break;

                    }

                }
                if (isInStableState)
                    break;
            }

            // ADDING TEMPORAL MEMEORY to CORTEX LAYER
            layer1.HtmModules.Add("tm", tm);

            string lastPredictedValue = "-1";
            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();
            // TRAINING SP+TM TOGETHER
            foreach (var sequence in Sequences)  // SEQUENCE LOOP
            {
                int SequencesMatchCount = 0; // NUMBER OF MATCHES
                var tempLOGFILE = new Dictionary<int, string>();
                var tempLOGGRAPH = new Dictionary<int, double>();
                double SaturatedAccuracyCount = 0;


                for (int i = 0; i < maxCycles; i++) // MAXCYCLE LOOP 
                {
                    List<string> ElementWiseClasses = new List<string>();

                    // ELEMENT IN SEQUENCE MATCHES COUNT
                    int ElementMatches = 0;

                    foreach (var Elements in sequence) // SEQUENCE DICTIONARY LOOP
                    {

                        // OBSERVATION LABEl
                        var observationLabel = Elements.Key;
                        // ELEMENT SDR LIST FOR A SINGLE SEQUENCE
                        var ElementSdr = Elements.Value;

                        List<Cell> actCells = new List<Cell>();
                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(ElementSdr, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));

                        // Active Cells
                        actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());


                        // CLASS VOTING IS USED FOR SEQUENCE CLASSIFICATION EXPERIMENT i.e CANCER SEQUENCE CLASSIFICATION EXPERIMENT
                        if (!classVotingEnabled)
                        {

                            if (lastPredictedValue == observationLabel && lastPredictedValue != "")
                            {
                                ElementMatches++;
                                Debug.WriteLine($"Match. Actual value: {observationLabel} - Predicted value: {lastPredictedValue}");
                            }
                            else
                            {
                                Debug.WriteLine($"Mismatch! Actual value: {observationLabel} - Predicted values: {lastPredictedValue}");
                            }
                        }
                        else
                        {
                            if (lastPredictedValueList.Contains(observationLabel))
                            {
                                ElementMatches++;
                                lastPredictedValueList.Clear();
                                Debug.WriteLine($"Match. Actual value: {observationLabel} - Predicted value: {lastPredictedValue}");
                            }
                            else
                            {
                                Debug.WriteLine($"Mismatch! Actual value: {observationLabel} - Predicted values: {lastPredictedValue}");
                            }
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

                                if (classVotingEnabled)
                                {
                                    lastPredictedValueList.Add(t.PredictedInput);
                                }

                            }

                            if (!classVotingEnabled)
                            {
                                lastPredictedValue = predictedInputValue.First().PredictedInput;
                            }
                        }

                    }

                    accuracy = ((double)ElementMatches / (sequence.Count)) * 100;
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

            // filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            // string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"TrainingLogs\");

            filename = "TaxiPassangerPredictionExperiment" + now.Ticks.ToString() + ".txt";
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"TrainingLogs/{filename}");

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
