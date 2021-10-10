// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketNeuroVisualizer;

namespace UnitTestsProject
{
    [TestClass]
    public class NetworkTests
    {

        /// <summary>
        /// It learns the sequence of elements.
        /// We provide a sequence A,B,C,D and repeat it. 
        /// By calling .Reset() we indicate end of sequence.
        /// We first start learning of region with SP algorithm only. This is called NewBorn-stage. In this stage
        /// SP simply learn patterns to enter a stable stage. To eneter stable stage SP needs 2-3 iterations.
        /// After NewBorn-stage we add TM as second algorithm and start learning sequences.
        /// We do learn sequences 10 iterations with learn=true. When starting the 11th iteration (experimentaly detected) learn is set to false
        /// and inference mode is entered to predict newxt step.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        public void CategorySequenceExperiment()
        {
            bool learn = true;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { 100 });
            p.Set(KEY.CELLS_PER_COLUMN, 30);
            string[] categories = new string[] { "A", "B", "C", "D" };
            //string[] categories = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K", "L" , "M", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Ö" };
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
            Dictionary<string, object> settings = new Dictionary<string, object>();
            //settings.Add("W", 25);
            settings.Add("N", 100);
            //settings.Add("Radius", 1);

            EncoderBase encoder = new CategoryEncoder(categories, settings);
            //encoder.Encode()
            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);
            //layer1.HtmModules.Add(tm1);
            //layer1.Compute();

            //IClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            HtmUnionClassifier<string, ComputeCycle> cls1 = new HtmUnionClassifier<string, ComputeCycle>();
            //string[] inputs = new string[] { "A", "B", "C", "D" };
            string[] inputs = new string[] { "A", "B", "C", "D" };

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

            //
            // Now, training with SP+TM. SP is pretrained on pattern.
            for (int i = 0; i < 200; i++)
            {
                foreach (var input in inputs)
                {
                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;
                    //cls1.Learn(input, lyrOut.activeCells.ToArray(), learn);
                    //Debug.WriteLine($"Current Input: {input}");
                    cls.Learn(input, lyrOut.ActiveCells.ToArray());
                    Debug.WriteLine($"Current Input: {input}");
                    if (learn == false)
                    {
                        Debug.WriteLine($"Predict Input When Not Learn: {cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray())}");
                    }
                    else
                    {
                        Debug.WriteLine($"Predict Input: {cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray())}");
                    }

                    Debug.WriteLine("-----------------------------------------------------------\n----------------------------------------------------------");
                }


                if (i == 10)
                {
                    Debug.WriteLine("Stop Learning From Here----------------------------");
                    learn = false;
                }

                // tm1.reset(mem);
            }

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
            /*
            learn = false;
            for (int i = 0; i < 19; i++)
            {
                foreach (var input in inputs)
                {
                    layer1.Compute((object)input, learn);
                }
            }
            */

        }

        /// <summary>
        /// It learns the sequence of elements of type double. Useful to test sequence stability in dependence on cells, num of elements
        /// topology etc.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        public void LongerSequenceExperiment()
        {
            int inputBits = 1024;

            bool learn = true;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 2048 });

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
                { "W", 21},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
               // { "MaxVal", 20.0 },
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            };

            double max = 50;
            List<double> lst = new List<double>();
            for (double i = 0; i < max; i++)
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

            //
            // Now, training with SP+TM. SP is pretrained on pattern.
            for (int i = 0; i < 200; i++)
            {
                foreach (var input in inputs)
                {
                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    cls.Learn(input, lyrOut.ActiveCells.ToArray());

                    Debug.WriteLine($"-------------- {input} ---------------");

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.PredictiveCells.Select(c => c.Index).ToArray())}");

                    Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray())}");
                }

                if (i == 50)
                {
                    Debug.WriteLine("Stop Learning From Here. Entering inference mode.");
                    learn = false;
                }

                tm1.Reset(mem);
            }

            cls.TraceState();

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }


        /// <summary>
        ///
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        public void SimpleSequenceExperiment()
        {
            int inputBits = 50;
            //int inputBits = 5;

            bool learn = true;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            //p.Set(KEY.CELLS_PER_COLUMN, 100);
            p.Set(KEY.CELLS_PER_COLUMN, 5);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 500 });
            //p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
            //p.setStimulusThreshold(1);
            //p.setMinThreshold(1);

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
                { "W", 7},
                //{ "W", 1},
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

                    Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                    Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(lyrOut.ActiveCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.PredictiveCells.Select(c => c.Index).ToArray())}");

                    var predictedValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                    Debug.WriteLine($"Current Input: {input} \t| - Predicted value in previous cycle: {lastPredictedValue} \t| Predicted Input for the next cycle: {predictedValue}");

                    if (input == lastPredictedValue)
                    {
                        matches++;
                        Debug.WriteLine($"Match. Actual value: {input} - Predicted value: {lastPredictedValue}");
                    }
                    else
                        Debug.WriteLine($"Missmatch! Actual value: {input} - Predicted value: {lastPredictedValue}");

                    lastPredictedValue = predictedValue;
                }

                if (i == 500)
                {
                    Debug.WriteLine("Stop Learning From Here. Entering inference mode.");
                    learn = false;
                }

                //tm1.reset(mem);

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {(double)matches / (double)inputs.Length * 100.0}%");
            }

            cls.TraceState();

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }



        /// <summary>
        ///
        /// </summary>
        private async Task RunExperimentNeuroVisualizer(int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;
            bool learn = true;
            INeuroVisualizer vis = new WSNeuroVisualizer();
            GenerateNeuroModel model = new GenerateNeuroModel();

            await vis.ConnectToWSServerAsync();
            await vis.InitModelAsync(model.CreateNeuroModel(new int[] { 1 }, (long[,])p[KEY.COLUMN_DIMENSIONS], (int)p[KEY.CELLS_PER_COLUMN]));

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

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            //
            // NewBorn learning stage.
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);

            //HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];

            int maxSPLearningCycles = 50;

            //
            // This trains SP on input pattern.
            // It performs some kind of unsupervised new-born learning.
            foreach (var input in inputs)
            {
                List<(int Cycle, double Similarity)> elementOscilationResult = new List<(int Cycle, double Similarity)>();

                Debug.WriteLine($"Learning  ** {input} **");

                for (int i = 0; i < maxSPLearningCycles; i++)
                {
                    var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;

                    var activeColumns = layer1.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    var similarity = MathHelpers.CalcArraySimilarity(prevActiveCols, actCols);
                    await vis.UpdateColumnAsync(GetColumns(actCols));

                    Debug.WriteLine($" {i.ToString("D4")} SP-OUT: [{actCols.Length}/{similarity.ToString("0.##")}] - {Helpers.StringifyVector(actCols)}");

                    prevActiveCols = activeColumns;
                }
            }

            // Here we add TM module to the layer.
            layer1.HtmModules.Add("tm", tm1);

            int cycle = 0;
            int matches = 0;

            string lastPredictedValue = "0";

            Dictionary<double, List<List<int>>> activeColumnsLst = new Dictionary<double, List<List<int>>>();

            foreach (var input in inputs)
            {
                if (activeColumnsLst.ContainsKey(input) == false)
                    activeColumnsLst.Add(input, new List<List<int>>());
            }

            int maxCycles = 3500;

            //
            // Now training with SP+TM. SP is pretrained on the given input pattern.
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                string prevInput = "-1.0";

                //
                // Activate the 'New - Born' effect.
                //if (i == 300)
                //{
                //    mem.setMaxBoost(0.0);
                //    mem.updateMinPctOverlapDutyCycles(0.0);
                //    cls.ClearState();
                //}

                foreach (var input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    var activeColumns = layer1.GetResult("sp") as int[];

                    activeColumnsLst[input].Add(activeColumns.ToList());

                    //cls.Learn(input, lyrOut.ActiveCells.ToArray());
                    cls.Learn(GetKey(prevInput, input), lyrOut.ActiveCells.ToArray());

                    List<Synapse> synapses = new List<Synapse>();
                    Cell cell = new Cell(0, 1, 6, 0, CellActivity.ActiveCell);// where to get all these values
                    Synapse synap = new Synapse(cell, 1, 1, 0.78);// here is just supposed to update the permanence, all other values remains same; where do we get all other values
                    synapses.Add(synap);
                    await vis.UpdateSynapsesAsync(synapses);//update Synapse or add new ones

                    await vis.UpdateCellsAsync(GetCells(lyrOut.ActiveCells));

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    if (GetKey(prevInput, input) == lastPredictedValue)
                    {
                        matches++;
                        Debug.WriteLine($"Match {input}");
                    }
                    else
                        Debug.WriteLine($"Missmatch Actual value: {GetKey(prevInput, input)} - Predicted value: {lastPredictedValue}");

                    if (lyrOut.PredictiveCells.Count > 0)
                    {
                        var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                        Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {predictedInputValue}");

                        lastPredictedValue = predictedInputValue;
                    }
                    else
                        Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");

                    prevInput = input.ToString();
                }

                //tm1.reset(mem);

                double accuracy = (double)matches / (double)inputs.Length * 100.0;

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {accuracy}%");

                if (accuracy == 100.0)
                {
                    maxMatchCnt++;
                    Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");
                    if (maxMatchCnt >= 20)
                    {
                        sw.Stop();
                        Debug.WriteLine($"Exit experiment in the stable state after 10 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                        learn = false;
                        //var testInputs = new double[] { 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 0.0, 1.0 };

                        // C-0, D-1, E-2, F-3, G-4, H-5
                        //var testInputs = new double[] { 0.0, 0.0, 4.0, 4.0, 5.0, 5.0, 4.0, 3.0, 3.0, 2.0, 2.0, 1.0, 1.0, 0.0 };

                        //// Traverse the sequence and check prediction.
                        //foreach (var input in inputValues)
                        //{
                        //    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;
                        //    predictedInputValue = cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray());
                        //    Debug.WriteLine($"I={input} - P={predictedInputValue}");
                        //}

                        //
                        // Here we let the HTM predict seuence five times on its own.
                        // We start with last predicted value.
                        int cnt = 5 * inputValues.Count;

                        Debug.WriteLine("---- Start Predicting the Sequence -----");

                        // We take a random value to start somwhere in the sequence.
                        var predictedInputValue = inputValues[new Random().Next(0, inputValues.Count - 1)].ToString();

                        List<string> predictedValues = new List<string>();

                        while (--cnt > 0)
                        {
                            //var lyrOut = layer1.Compute(predictedInputValue, learn) as ComputeCycle;
                            var lyrOut = layer1.Compute(double.Parse(predictedInputValue[predictedInputValue.Length - 1].ToString()), learn) as ComputeCycle;
                            predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            predictedValues.Add(predictedInputValue);
                        };

                        foreach (var item in predictedValues)
                        {
                            Debug.Write(item);
                            Debug.Write(" ,");
                        }
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

            cls.TraceState($"cellState_MinPctOverlDuty-{p[KEY.MIN_PCT_OVERLAP_DUTY_CYCLES]}_MaxBoost-{p[KEY.MAX_BOOST]}.csv");

            Debug.WriteLine("---- column state trace ----");

            foreach (var input in activeColumnsLst)
            {
                using (StreamWriter colSw = new StreamWriter($"ColumState_MinPctOverlDuty-{p[KEY.MIN_PCT_OVERLAP_DUTY_CYCLES]}_MaxBoost-{p[KEY.MAX_BOOST]}_input-{input.Key}.csv"))
                {
                    Debug.WriteLine($"------------ {input} ------------");

                    foreach (var actCols in input.Value)
                    {
                        Debug.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                        colSw.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                    }
                }
            }

            Debug.WriteLine("------------ END ------------");

        }

        private List<Cell> GetCells(IList<Cell> activeCells)
        {
            //throw new NotImplementedException();
            List<Cell> cells = new List<Cell>();

            //foreach (var cell in actCells)
            //{        // cellIndex1, CellActivit1, ellIndex1, CellActivit1,, perm

            //    MiniColumn mC = new MiniColumn(0, 0.0, col, 0, ColumnActivity.Active);
            //}
            return cells;
        }

        private List<MiniColumn> GetColumns(int[] actCols)
        {
            List<MiniColumn> miniColumns = new List<MiniColumn>();
            foreach (var col in actCols)
            {
                MiniColumn mC = new MiniColumn(0, 0.0, col, 0, ColumnActivity.Active);
                miniColumns.Add(mC);
            }

            return miniColumns;
        }

        private static string GetKey(string prevInput, double input)
        {
            return $"{prevInput}-{input.ToString()}";
        }




        /// <summary>
        /// It learns SP and shows the convergence of SDR for the given input.
        /// It repeats the learning process for every input many times (i.e.: 10000+ cycles) and writes out
        /// the state of SDR. Then it counts number or instabe states. It means once SP generates stable SDR, it 
        /// can hapen that in some hihg cycle SDR changes. NUmber of such instable cycles is counted and outputed as a result.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        [TestCategory("Experiment")]
        public void SpatialPooler_Stability_Experiment()
        {
            int inputBits = 100;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });
            p.Set(KEY.MAX_BOOST, 1.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100000);

            // Stops the bumping of inactive columns.
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0);
            //p.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);

            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);
            p.setInhibitionRadius(15);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));
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

            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 });

            RunSpStabilityExperiment(inputBits, p, encoder, inputValues);
        }

        /// <summary>
        ///
        /// </summary>
        private void RunSpStabilityExperiment(int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool learn = true;

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            SpatialPooler sp1 = new SpatialPooler();
            var mem = new Connections();
            p.apply(mem);
            sp1.Init(mem, UnitTestHelpers.GetMemory());

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            //
            // NewBorn learning stage.
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];

            int maxSPLearningCycles = 25000;


            List<(double Element, (int Cycle, double Similarity)[] Oscilations)> oscilationResult = new List<(double Element, (int Cycle, double Similarity)[] Oscilations)>();

            //
            // This trains SP on input pattern.
            // It performs some kind of unsupervised new-born learning.
            foreach (var input in inputs)
            {
                using (StreamWriter writer = new StreamWriter($"Oscilations_Boost_10_{input}.csv"))
                {
                    Debug.WriteLine($"Learning Cycles: {maxSPLearningCycles}");
                    Debug.WriteLine($"MAX_BOOST={p[KEY.MAX_BOOST]}, DUTY ={p[KEY.DUTY_CYCLE_PERIOD]}");
                    Debug.WriteLine("Cycle;Similarity");

                    double similarity = 0;
                    double prevSimilarity = 0;

                    List<(int Cycle, double Similarity)> elementOscilationResult = new List<(int Cycle, double Similarity)>();

                    Debug.WriteLine($"Learning  ** {input} **");

                    for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
                    {
                        var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;

                        var activeColumns = layer1.GetResult("sp") as int[];

                        var actCols = activeColumns.OrderBy(c => c).ToArray();

                        Debug.WriteLine($" {cycle.ToString("D4")} SP-OUT: [{actCols.Length}/{MathHelpers.CalcArraySimilarity(prevActiveCols, actCols)}] - {Helpers.StringifyVector(actCols)}");

                        similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols);

                        if (similarity < 60.0)
                        {
                            var tp = (Cycle: cycle, Similarity: similarity);

                            elementOscilationResult.Add(tp);
                        }

                        prevActiveCols = activeColumns;
                        prevSimilarity = similarity;

                        writer.WriteLine($"{cycle};{similarity}");
                    }

                    writer.Flush();
                    oscilationResult.Add((Element: input, Oscilations: elementOscilationResult.ToArray()));
                }
            }

            foreach (var item in oscilationResult)
            {
                int oscilationPeeks = 0;
                int lastCycle = -1;
                StringBuilder sb = new StringBuilder();

                foreach (var o in item.Oscilations)
                {
                    sb.Append($"({o.Cycle}/{o.Similarity})");

                    if (lastCycle + 1 != o.Cycle)
                        oscilationPeeks++;

                    lastCycle = o.Cycle;
                }

                Debug.WriteLine($"{item.Element};{oscilationPeeks};{item.Oscilations.Length};[{sb.ToString()}]");
            }

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }


        private void PlaySong(double[] notes)
        {
            // C-0, D-1, E-2, F-3, G-4, H-5
        }

        [TestMethod]
        public async Task Abc()
        {
            INeuroVisualizer vis = new WSNeuroVisualizer();

            List<MiniColumn> colData = new List<MiniColumn>();
            MiniColumn minCol = new MiniColumn(0, 0, 0, 0, ColumnActivity.Active);
            MiniColumn minCol1 = new MiniColumn(0, 01, 0, 0, ColumnActivity.Active);

            colData.Add(minCol);
            colData.Add(minCol1);

            await vis.UpdateColumnAsync(colData);
        }

        [TestMethod]
        public async Task TestModel()
        {
            INeuroVisualizer vis = new WSNeuroVisualizer();
            int[] areas = new int[] { 1 };
            GenerateNeuroModel model = new GenerateNeuroModel();
            // vis.InitModelAsync(new NeuroModel(areas, (new long[10, 1]), 6));
            // vis.InitModelAsync(new NeuroModel(areas, (new long[10, 5]), 8));


            await vis.ConnectToWSServerAsync();
            await vis.InitModelAsync(model.CreateNeuroModel(areas, (new long[10, 1]), 6));

        }
        [TestMethod]
        public async Task updateOrAddSynapse()
        {
            INeuroVisualizer vis = new WSNeuroVisualizer();
            int[] areas = new int[] { 1 };
            GenerateNeuroModel model = new GenerateNeuroModel();
            await vis.ConnectToWSServerAsync();
            await vis.InitModelAsync(model.CreateNeuroModel(areas, (new long[10, 1]), 6));


            List<Synapse> synapses = new List<Synapse>();
            Cell cell = new Cell(0, 1, 6, 1, CellActivity.PredictiveCell);
            Synapse synap = new Synapse(cell, 1, 1, 0.75);
            synapses.Add(synap);

            await vis.UpdateSynapsesAsync(synapses);
        }

        [TestMethod]
        public async Task updateOverlap()
        {
            INeuroVisualizer vis = new WSNeuroVisualizer();
            int[] areas = new int[] { 1 };
            GenerateNeuroModel model = new GenerateNeuroModel();
            await vis.ConnectToWSServerAsync();
            await vis.InitModelAsync(model.CreateNeuroModel(areas, (new long[10, 1]), 6));

            List<MiniColumn> columnList = new List<MiniColumn>();
            MiniColumn minCol = new MiniColumn(0, 0.80, 8, 0, ColumnActivity.Active);
            columnList.Add(minCol);
            await vis.UpdateColumnAsync(columnList);
        }

        [TestMethod]
        public async Task TestConectivity()
        {
            INeuroVisualizer vis = new WSNeuroVisualizer();

            await vis.ConnectToWSServerAsync();
            //await  vis.TestMethod("Testing phase", ws2);

        }
    }
}

