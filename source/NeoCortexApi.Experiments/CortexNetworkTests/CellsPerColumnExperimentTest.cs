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
    /// Check out student paper in the following URL: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/ML-19-20_20-5.4_CellsPerColumnExperiment_Paper.pdf
    /// </summary>
    [TestClass]
    public class CellsPerColumnExperimentTest
    {
        /// <summary>
        /// This experiment checks for the learning process in dependence on cells per column. What is the effect
        /// on Learning by varying the values of cells per column. 

        /// The basis of this experiment is SimpleSequenceExperiment. This program has 2 loops (loop inside a loop), 
        /// the parent loop/outer loop is defined keeping in mind how many readings are required in the result .
        /// The child loop/inner loop has 400 cycle, but is ended as soon as we get 100% prediction match
        /// i.e. if there are 10 input values in the input sequence, so there should be 10 out 10 matches.
        /// Then the parent loop is incremented and it continues for the number of iter defined. In this case it is 30.
        /// Iter is simply the the number of times,the whole experiment is repeated to achieve accurate results.


        /// </summary>

        [TestMethod]
        [TestCategory("Experiment")]
        [TestCategory("NetworkTests")]
        [DataRow(1, 30)]   //Cells =1 , iter = 30                         
        [DataRow(2, 30)] //Cells =2 , iter = 30
        [DataRow(3, 30)] //Cells =3 , iter = 30
        [DataRow(4, 30)] //Cells =4 , iter = 30 
        [DataRow(5, 30)] //Cells =5 , iter = 30 
        [DataRow(7, 30)] //Cells =7 , iter = 30 
        [DataRow(10, 30)] //Cells =10 , iter = 30 
        [DataRow(20, 30)] //Cells =20 , iter = 30
        public void CellPerColumn(int C, int loop)
        {
            string filename = "Cells=" + C + ".csv";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                Debug.WriteLine($"Learning Cycles: {400}");
                Debug.WriteLine("Cycle;Similarity");

                // Parent Loop
                //This loop defines the number of times the experiment will run for the given data
                for (int j = 0; j < loop; j++)
                {
                    int inputBits = 200;
                    bool learn = true;
                    Parameters p = Parameters.getAllDefaultParameters();
                    p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
                    p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });

                    p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 500 });
                    p.Set(KEY.CELLS_PER_COLUMN, C);

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
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
               // { "MaxVal", 20.0 },
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            };

                    double max = 10;

                    List<double> lst = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                    // Later we run the whole experiment for all the below input sequences

                    // List<double> lst = new List<double>() { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8 };
                    // List<double> lst = new List<double>() { 1, 2, 3, 1, 2, 4 };
                    // List<double> lst = new List<double>() { 1, 2, 3, 4, 1, 2, 3, 5 };
                    // List<double> lst = new List<double>() { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

                    settings["MaxVal"] = max;

                    EncoderBase encoder = new ScalarEncoder(settings);

                    CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

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

                    for (int i = 0; i < 400; i++)
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