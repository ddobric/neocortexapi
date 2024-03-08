// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    public class HtmSparsityTest
    {
        /// <summary>
        ///Htm Sparsity is the ratio between Width and InputBits(W/N). This unit test runs in a loop and saves cycle at which
        ///we get 100% match for the first time at Sparsity=0.18. W and N can be changed but the ratio must be 0.18.
        ///This program has 2 loops (loop inside a loop), the parent loop/outer loop is defined keeping in mind how many
        ///readings are wanted in the result. The child loop/inner loop has 460 cycle, but is ended as soon as we get 100%
        ///match i.e. for max=10 10 out 10 matches. Then the parent loop is incremented and it continues for the number of
        ///loops defined (in our case we used 1000 - 10000 loops).
        ///"We found out that, for max=10 ideal HTM Sparsity is 0.18"
        /// </summary>

        [TestMethod]
        [TestCategory("NetworkTests")]
        [DataRow(9, 50, 1000)]   //Sparsity - 0.18
        [DataRow(27, 150, 1000)] //Sparsity - 0.18
        [DataRow(45, 250, 1000)] //Sparsity - 0.18
        [DataRow(81, 450, 1000)] //Sparsity - 0.18
        //[DataRow(9, 90, 1000)] //Sparsity - 0.10
        //[DataRow(9, 70, 1000)] //Sparsity - 0.128
        //[DataRow(9, 60, 1000)] //Sparsity - 0.15
        //[DataRow(9, 45, 1000)] //Sparsity - 0.20
        //[DataRow(9, 40, 1000)] //Sparsity - 0.225
        //[DataRow(9, 36, 1000)] //Sparsity - 0.25
        //[DataRow(9, 30, 1000)] //Sparsity - 0.30

        public void HtmSparsity_1(int W, int InputB, int loop)
        {
            string filename = "Sparsity_" + W + "." + InputB + ".csv";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                Debug.WriteLine($"Learning Cycles: {460}");
                Debug.WriteLine("Cycle;Similarity");

                //Parent loop / Outer loop
                //This loop defines the number of times the experiment will run for the given data
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
                            var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;
                        }
                    }

                    // Here we add TM module to the layer.
                    layer1.HtmModules.Add("tm", tm1);

                    int cycle = 0;
                    int matches = 0;

                    double lastPredictedValue = 0;
                    //
                    // Now, training with SP+TM. SP is pretrained on pattern.
                    //Child loop / Inner loop

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

                        Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {(double)matches / (double)inputs.Length * 100.0}%");
                        if ((double)matches / (double)inputs.Length == 1)
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

        /// <summary>
        ///Htm Sparsity is the ratio between Width and InputBits(W/N). This unit test runs in a loop and saves cycle at which
        ///we get 100% match for the first time at Sparsity=0.18. W and N can be changed but the ratio must be 0.18.
        ///This program has 2 loops (loop inside a loop), the parent loop/outer loop is defined keeping in mind how many
        ///readings are wanted in the result. The child loop/inner loop has 460 cycle, but is ended as soon as we get 100%
        ///match i.e. for max=10 10 out 10 matches. Then the parent loop is incremented and it continues for the number of
        ///loops defined (in our case we used 1000 - 10000 loops).
        ///"We found out that, for max=10 ideal HTM Sparsity is 0.18"
        /// </summary>

        [TestMethod]
        [TestCategory("NetworkTests")]
        [DataRow(9, 50, 10, 1000)] //Sparsity - 0.18
        [DataRow(9, 50, 15, 1000)] //Sparsity - 0.18
        [DataRow(9, 40, 15, 1000)] //Sparsity - 0.225
        [DataRow(9, 60, 15, 1000)] //Sparsity - 0.15

        public void HtmSparsityAndMax(int W, int InputB, int max1, int loop)
        {
            string filename = "Relation_" + W + "." + InputB + "with" + max1 + ".csv";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                Debug.WriteLine($"Learning Cycles: {460}");
                Debug.WriteLine("Cycle;Similarity");

                //Parent loop / Outer loop
                //This loop defines the number of times the experiment will run for the given data
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

                    double max = max1;

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
                            var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;
                        }
                    }

                    // Here we add TM module to the layer.
                    layer1.HtmModules.Add("tm", tm1);

                    int cycle = 0;
                    int matches = 0;

                    double lastPredictedValue = 0;
                    //
                    // Now, training with SP+TM. SP is pretrained on pattern.
                    //Child loop / Inner loop

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

                        Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {(double)matches / (double)inputs.Length * 100.0}%");
                        if ((double)matches / (double)inputs.Length == 1)
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