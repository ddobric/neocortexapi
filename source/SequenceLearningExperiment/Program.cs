// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SequenceLearningExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            MusicNotesExperiment();
        }

        public static void MusicNotesExperiment()
        {
            int inputBits = 100;
            int numColumns = 2048;
            List<double> inputValues = inputValues = new List<double>(new double[] { });
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

            EncoderBase encoder = new ScalarEncoder(settings);

            // Stable and reached 100% accuracy with 2577 cycles
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 });

            // Stable and reached 100% accuracy with 2554 cycles
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 0.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 0.0, 1.0});

            // Stable and reached 100% accuracy with 3560 cycles
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0 });

            // Stable and reached 100% accuracy with 3401 cycles 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0 });

            // Stable and 100% accuracy reached in with 2040 cycles 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0 });

            // Stable and 100% accuracy reached in with 55 cycles 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 12.0 });

            // Stable and reached 100% accuracy with 112 cycles. 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 7.0, 5.0 });

            // Stable and reached 100% accuracy with 70 cycles.
            // var inputValues = new List<double>(new double[] {1.0,2.0,3.0,1.0,5.0,1.0,6.0});

            // Music Notes C-0, D-1, E-2, F-3, G-4, H-5
            // http://sea-01.cit.frankfurt-university.de:32224/?dmVyPTEuMDAxJiYwYzg1N2MyNmFmMzIyMjc0OD02MDlGQkRBOF83Njg0Nl8xNTU0N18xJiZmYjFlMzlhNWZmYWYyNDE9MTIzMyYmdXJsPWh0dHBzJTNBJTJGJTJGd3d3JTJFYmV0aHNub3Rlc3BsdXMlMkVjb20lMkYyMDEzJTJGMDglMkZ0d2lua2xlLXR3aW5rbGUtbGl0dGxlLXN0YXIlMkVodG1sJTBE
            // 1Stable and reached 100% accuracy with 1024 cycles.
            // var inputValues = new List<double>( new double[] { 0.0, 0.0, 4.0, 4.0, 5.0, 5.0, 4.0, 3.0, 3.0, 2.0, 2.0, 1.0, 1.0, 0.0 });

            // Stable and reached 100% accuracy with 531 cycles.
            // var inputValues = new List<double>(new double[] {0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0}

            // Calling Method to input values
            inputValues = InputSequence(inputValues);

            // Sequence with multiple possibilties
            // Stable and reached 100% accuracy with 542 cycles.
            //var inputValues = new List<double>(new double[] {1.0, 2.0, 3.0, 4.0, 3.0, 2.0, 4.0, 5.0, 6.0, 1.0, 7.0});

            // Stable and reached 100% accuracy with 650 cycles.
            // var inputValues = new List<double>(new double[] { 2.0, 3.0, 2.0, 5.0, 2.0, 6.0, 2.0, 6.0, 2.0, 5.0, 2.0, 3.0, 2.0, 3.0, 2.0, 5.0, 2.0, 6.0 });

            RunExperiment(inputBits, cfg, encoder, inputValues);
        }



        /// <summary>
        ///
        /// </summary>
        private static void RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, List<double> inputValues)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;
            bool learn = true;

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            var mem = new Connections(cfg);
            bool isInStableState;

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numInputs = inputValues.Distinct<double>().ToList().Count;

            TemporalMemory tm1 = new TemporalMemory();

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, numInputs * 55, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                if (numPatterns != numInputs)
                    throw new InvalidOperationException("Stable state must observe all input patterns");

                isInStableState = true;
                cls.ClearState();

                tm1.Reset(mem);
            }, numOfCyclesToWaitOnChange: 25);


            SpatialPoolerMT sp1 = new SpatialPoolerMT(hpa);
            sp1.Init(mem, new DistributedMemory()
            {
                ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1),
            });

            tm1.Init(mem);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);
            layer1.HtmModules.Add("tm", tm1);

            double[] inputs = inputValues.ToArray();

            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            string lastPredictedValue = "0";
            String prediction = null;

            Dictionary<double, List<List<int>>> activeColumnsLst = new Dictionary<double, List<List<int>>>();

            foreach (var input in inputs)
            {

                if (activeColumnsLst.ContainsKey(input) == false)
                    activeColumnsLst.Add(input, new List<List<int>>());
            }

            int maxCycles = 3500;
            int maxPrevInputs = inputValues.Count - 1;
            List<string> previousInputs = new List<string>();
            previousInputs.Add("-1.0");

            //
            // Now training with SP+TM. SP is pretrained on the given input pattern.
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                foreach (var input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    var activeColumns = layer1.GetResult("sp") as int[];

                    activeColumnsLst[input].Add(activeColumns.ToList());

                    previousInputs.Add(input.ToString());
                    if (previousInputs.Count > (maxPrevInputs + 1))
                        previousInputs.RemoveAt(0);

                    string key = GetKey(previousInputs, input);

                    cls.Learn(key, lyrOut.ActiveCells.ToArray());

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                    Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(lyrOut.ActiveCells.Select(c => c.Index).ToArray())}");

                    if (key == lastPredictedValue)
                    {
                        matches++;
                        Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValue}");
                    }
                    else
                        Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted value: {lastPredictedValue}");

                    if (lyrOut.PredictiveCells.Count > 0)
                    {
                        var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                        Debug.WriteLine($"Current Input: {input}");
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
                    else
                    {
                        Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                        lastPredictedValue = String.Empty;
                    }
                }


                double accuracy = (double)matches / (double)inputs.Length * 100.0;

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {accuracy}%");

                if (accuracy == 100.0)
                {
                    maxMatchCnt++;
                    Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");
                    if (maxMatchCnt >= 30)
                    {
                        sw.Stop();
                        Debug.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                        learn = false;
                        break;
                    }
                }
                else if (maxMatchCnt > 0)
                {
                    Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                    maxMatchCnt = 0;
                }
            }

            Debug.WriteLine("---- cell state trace ----");

            cls.TraceState($"cellState_MinPctOverlDuty-{cfg.MinPctOverlapDutyCycles}_MaxBoost-{cfg.MaxBoost}.csv");

            Debug.WriteLine("---- Spatial Pooler column state  ----");

            foreach (var input in activeColumnsLst)
            {
                using (StreamWriter colSw = new StreamWriter($"ColumState_MinPctOverlDuty-{cfg.MinPctOverlapDutyCycles}_MaxBoost-{cfg.MaxBoost}_input-{input.Key}.csv"))
                {
                    Debug.WriteLine($"------------ {input.Key} ------------");

                    foreach (var actCols in input.Value)
                    {
                        Debug.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                        colSw.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                    }
                }
            }

            Debug.WriteLine("------------ END ------------");

            Console.WriteLine("\n Please enter a number that has been learnt");
            int inputNumber = Convert.ToInt16(Console.ReadLine());
            Inference(inputNumber, false, layer1, cls);

        }

        private static List<double> InputSequence(List<double> inputValues)
        {
            Console.WriteLine("HTM Classifier is ready");
            Console.WriteLine("Please enter a sequence to be learnt");
            string userValue = Console.ReadLine();
            var numbers = userValue.Split(',');
            double sequence;
            foreach (var number in numbers)
            {
                if (double.TryParse(number, out sequence))
                {
                    inputValues.Add(sequence);
                }
            }

            return inputValues;
        }

        private static void Inference(int input, bool learn, CortexLayer<object, object> layer1, HtmClassifier<string, ComputeCycle> cls)
        {
            var result = layer1.Compute(input, false) as ComputeCycle;
            var predresult = cls.GetPredictedInputValues(result.PredictiveCells.ToArray(), 3);
            Console.WriteLine("\n The predictions are:");
            foreach (var ans in predresult)
            {
                Console.WriteLine($"Predicted Input: {string.Join(", ", ans.PredictedInput)}," +
                                  $"\tSimilarity Percentage: {string.Join(", ", ans.Similarity)}, " +
                                  $"\tNumber of Same Bits: {string.Join(", ", ans.NumOfSameBits)}");
            }
        }

        private static string GetKey(List<string> prevInputs, double input)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return key;
        }
    }
}