using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;

namespace LabelPrediction
{
    public class MultiSequenceLearning
    {
        static readonly string PowerConsumptionCSV = Path.GetFullPath(System.AppDomain.CurrentDomain.BaseDirectory + @"\Dataset\rec-center-hourly-og.csv");
        static readonly string PowerConsumptionCSV_Exp = Path.GetFullPath(System.AppDomain.CurrentDomain.BaseDirectory + @"\Dataset\rec-center-hourly-exp.csv");


        public MultiSequenceLearning()
        {
            //needs no implementation
        }

        public void StartLearning()
        {
            int inputBits = 100;
            int maxCycles = 15;
            int numColumns = 2048;

            Console.WriteLine("Reading CSV File..");
            var csvData = HelperMethods.ReadPowerConsumptionDataFromCSV(PowerConsumptionCSV_Exp);
            Console.WriteLine("Completed reading CSV File..");

            Console.WriteLine("Encoding data read from CSV...");
            var encodedData = HelperMethods.EncodePowerConsumptionData(csvData);

            EncoderBase encoderDateTime = HelperMethods.FetchDateTimeEncoder();

            Console.WriteLine("Started Learning...");
            var trainedHTMmodel = Run(inputBits, maxCycles, numColumns, encoderDateTime, encodedData);
            var trainedCortexLayer = trainedHTMmodel.Keys.ElementAt(0);
            var trainedClassifier  = trainedHTMmodel.Values.ElementAt(0);
            Console.WriteLine("Done Learning");

            Debug.WriteLine("PLEASE ENTER DATE FOR PREDICTING PASSENGER COUNT:      *note format->dd-mm-yyyy");
            Console.WriteLine("PLEASE ENTER DATE FOR PREDICTING PASSENGER COUNT:      *note format->dd-mm-yyyy");
            var userInput = Console.ReadLine();

            while (!userInput.Equals("q") && userInput != "Q")
            {
                var sdr = HelperMethods.EncodeSingleInput(userInput);
                var userLayerOutput = trainedCortexLayer.Compute(sdr, false) as ComputeCycle;
                var predictedValuesForUserInput = trainedClassifier.GetPredictedInputValues(userLayerOutput.PredictiveCells.ToArray(), 5);
                foreach (var predictedVal in predictedValuesForUserInput)
                {
                    Console.WriteLine("SIMILARITY " + predictedVal.Similarity + " PREDICTED VALUE :" + predictedVal.PredictedInput);
                }
                Console.WriteLine("TAKING USERINPUT FOR CHECKING PREDICTED PASSENGER COUNT");
                userInput = Console.ReadLine();
            }
        }

        public Dictionary<CortexLayer<object,object>, HtmClassifier<string, ComputeCycle>> Run(int inputBits, int maxCycles, int numColumns, EncoderBase encoder, List<Dictionary<string,int[]>> sequences)
        {
            var htmConfig = HelperMethods.FetchHTMConfig(inputBits, numColumns);

            var mem = new Connections(htmConfig);

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            bool isInStableState = false;

            bool learn = true;

            int newbornCycle = 0;

            var OUTPUT_LOG_LIST = new List<Dictionary<int, string>>();
            var OUTPUT_LOG = new Dictionary<int, string>();
            var OUTPUT_trainingAccuracy_graph = new List<Dictionary<int, double>>();

            int numUniqueInputs = sequences.Count;

            // For more information see following paper: https://www.scitepress.org/Papers/2021/103142/103142.pdf
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                learn = isInStableState = isStable;

                // Clear active and predictive cells.
                //tm.Reset(mem);
            }, numOfCyclesToWaitOnChange: 50);

            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);

            TemporalMemory tm = new TemporalMemory();
            tm.Init(mem);

            //layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            // CONTRAINER FOR Previous Active Columns
            int[] prevActiveCols = new int[0];

            int computeCycle = 0;
            int maxComputeCycles = maxCycles;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //
            // Training SP to get stable. New-born stage.
            //

            for (int i = 0; i < maxComputeCycles && isInStableState == false; i++)
            {
                computeCycle++;
                newbornCycle++;
                Debug.WriteLine($"-------------- Newborn Cycle {newbornCycle} ---------------");

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

            // We activate here the Temporal Memory algorithm.
            layer1.HtmModules.Add("tm", tm);

            string lastPredictedValue = "-1";
            List<string> lastPredictedValueList = new List<string>();
            double lastCycleAccuracy = 0;
            double accuracy = 0;

            List<List<string>> possibleSequence = new List<List<string>>();

            foreach (var sequence in sequences)
            {
                int SequencesMatchCount = 0; // NUMBER OF MATCHES
                var tempLOGFILE = new Dictionary<int, string>();
                var tempLOGGRAPH = new Dictionary<int, double>();
                double SaturatedAccuracyCount = 0;

                for (int i = 0; i < maxCycles; i++)
                {
                    List<string> ElementWiseClasses = new List<string>();

                    int ElementMatches = 0;

                    foreach (var Elements in sequence)
                    {
                        var observationLabel = Elements.Key;
                        // ELEMENT SDR LIST FOR A SINGLE SEQUENCE
                        var ElementSdr = Elements.Value;

                        List<Cell> actCells = new List<Cell>();
                        var lyrOut = new ComputeCycle();

                        lyrOut = layer1.Compute(ElementSdr, learn) as ComputeCycle;
                        Debug.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));

                        actCells = (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count) ? lyrOut.ActiveCells : lyrOut.WinnerCells;

                        cls.Learn(observationLabel, actCells.ToArray());

                        if (lastPredictedValue == observationLabel && lastPredictedValue != "")
                        {
                            ElementMatches++;
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
            string filename = now.ToString("g");

            filename = "PowwerConsumptionPredictionExperiment" + filename.Split(" ")[0] + "_" + now.Ticks.ToString() + ".txt";
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "\\TrainingLogs\\" + filename;

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

        
    }

}
