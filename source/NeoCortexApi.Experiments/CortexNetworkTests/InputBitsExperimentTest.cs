// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeoCortexApi.Experiments
{
    /// <summary>
    /// Check out student paper in the following URL: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/ML-19-20_20-5.4_ParameterChangeExperiment_Paper.pdf
    /// </summary>
    [TestClass]
    public class InputBitsExperimentTest
    {
        /// <summary>
        ///The purpose of this Unit test is to test learning at different values of InputBits N. The program runs for "loop"
        ///number of times and saves the cycle at which it gets 100% match for the first time in a excel file.
        //////This program has 2 loops (loop inside a loop), the parent loop/outer loop is defined keeping in mind how many
        ///readings are wanted in the result. The child loop/inner loop has 460 cycle, but is ended as soon as we get 100%
        ///match i.e. for max=10 10 out 10 matches. Then the parent loop is incremented and it continues for the number of
        ///loops defined (in our case we used 100 - 10000 loops).
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        // These are the data which will be given to the program. First Variable is Width, second is Input Bits and the third is Number of iterations
        [DataRow(9, 10, 1)]
        [DataRow(9, 25, 1)]
        [DataRow(9, 50, 1000)]
        [DataRow(9, 75, 1000)]
        [DataRow(9, 100, 1000)]
        [DataRow(9, 150, 1000)]
        [DataRow(9, 200, 1000)]
        [DataRow(9, 250, 1000)]
        [DataRow(9, 300, 1000)]
        [DataRow(9, 400, 1000)]
        [DataRow(9, 500, 1000)]
        public void InputBitsExperiment(int W, int InputB, int loop)
        {
            string filename = "InputBits" + InputB + ".csv";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                Debug.WriteLine($"Learning Cycles: {460}");
                Debug.WriteLine("Cycle;Similarity");
                //Parent Loop
                for (int j = 0; j < loop; j++)
                {
                    int inputBits = InputB;
                    bool learn = true;
                    Parameters p = Parameters.getAllDefaultParameters();
                    p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
                    p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
                    p.Set(KEY.CELLS_PER_COLUMN, 5);
                    p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 500 });

                    CortexNetwork net = new CortexNetwork("my cortex");
                    List<CortexRegion> regions = new List<CortexRegion>();
                    CortexRegion region0 = new CortexRegion("1st Region");

                    regions.Add(region0);

                    SpatialPoolerMT sp1 = new SpatialPoolerMT();
                    TemporalMemory tm1 = new TemporalMemory();
                    var mem = new Connections();
                    p.apply(mem);
                    sp1.Init(mem, UnitTestHelpers.GetMemory());
                    tm1.Init(mem);

                    Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", W},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
               // { "MaxVal", 20.0 },
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            };

                    double max = 10;

                    List<double> lst = new List<double>();

                    for (double i = max - 1; i >= 0; i--)
                    {
                        lst.Add(i);
                    }

                    settings["MaxVal"] = max;
                    EncoderBase encoder = new ScalarEncoder(settings);
                    CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
                    //
                    // NewBorn learning stage.
                    region0.AddLayer(layer1);
                    layer1.HtmModules.Add("encoder", encoder);
                    layer1.HtmModules.Add("sp", sp1);

                    HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

                    double[] inputs = lst.ToArray();

                    //
                    // This trains SP.
                    foreach (var input in inputs)
                    {
                        Debug.WriteLine($" ** {input} **");
                        for (int i = 0; i < 3; i++)
                        {
                            var lyrOut = layer1.Compute(input, learn) as ComputeCycle;
                        }
                    }

                    // Here we add TM module to the layer.
                    layer1.HtmModules.Add("tm", tm1);

                    int cycle = 0;
                    int matches = 0;

                    double lastPredictedValue = 0;
                    //
                    // Now, training with SP+TM. SP is pretrained on pattern.
                    //Child loop

                    for (int i = 0; i < 460; i++)
                    {
                        matches = 0;
                        cycle++;
                        foreach (var input in inputs)
                        {
                            var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                            cls.Learn(input, lyrOut.ActiveCells.ToArray());

                            Debug.WriteLine($"-------------- {input} ---------------");

                            if (learn == false)
                                Debug.WriteLine($"Inference mode");

                            Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                            Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.PredictiveCells.Select(c => c.Index).ToArray())}");

                            var predictedValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                            Debug.WriteLine($"Current Input: {input} \t| - Predicted value in previous cycle: {lastPredictedValue} \t| Predicted Input for the next cycle: {predictedValue}");

                            if (input == lastPredictedValue)
                            {
                                matches++;
                                Debug.WriteLine($"Match {input}");
                            }
                            else
                                Debug.WriteLine($"Missmatch Actual value: {input} - Predicted value: {lastPredictedValue}");

                            lastPredictedValue = predictedValue;
                        }

                        if (i == 500)
                        {
                            Debug.WriteLine("Stop Learning From Here. Entering inference mode.");
                            learn = false;
                        }

                        //tm1.reset(mem);

                        Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {matches / (double)inputs.Length * 100.0}%");
                        if (matches / (double)inputs.Length == 1)
                        {
                            writer.WriteLine($"{cycle}");
                            break;
                        }

                    }
                }
                Debug.WriteLine("New Iteration");
            }
            //cls.TraceState();
            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }
    }
}