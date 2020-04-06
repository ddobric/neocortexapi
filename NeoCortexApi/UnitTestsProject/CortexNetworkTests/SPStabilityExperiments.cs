// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using NeoCortexEntities.NeuroVisualizer;
using WebSocketNeuroVisualizer;
using NeoCortexApi.Utility;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.WebSockets;

namespace UnitTestsProject
{
    [TestClass]
    public class SPStabilityExperiments
    {

        #region Experiment 1
        /// <summary>
        /// It learns SP and shows the convergence of SDR for the given input.
        /// It repeats the learning process for every input many times (i.e.: 10000+ cycles) and writes out
        /// the state of SDR. Then it counts number or instabe states. It means once SP generates stable SDR, it 
        /// can hapen that in some hihg cycle SDR changes. NUmber of such instable cycles is counted and outputed as a result.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        [TestCategory("Experiment")]
        public void SpatialPooler_Stability_Experiment_1()
        {
            int inputBits = 100;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.MAX_BOOST, 0.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.0);

            // Local inhibition
            // Stops the bumping of inactive columns.
            //p.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true); Obsolete.use KEY.MIN_PCT_OVERLAP_DUTY_CYCLES = 0;
            p.Set(KEY.POTENTIAL_RADIUS, 50);
            p.Set(KEY.GLOBAL_INHIBITION, false);
            p.setInhibitionRadius(15);

            // Global inhibition
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            //p.Set(KEY.GLOBAL_INHIBITION, true);
            //p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            //p.Set(KEY.POTENTIAL_RADIUS, inputBits);
            //p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.
            //p.setInhibitionRadius( Automatically set on the columns pace in a case of global inhibition.);

            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);

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

            RunSpStabilityExperiment_1(inputBits, p, encoder, inputValues);
        }


        private void RunSpStabilityExperiment_1(int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            string path = nameof(SpatialPooler_Stability_Experiment_1);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            while (true)
            {                
                Directory.CreateDirectory(path);
                if (Directory.Exists(path) == false)
                    Thread.Sleep(300);
                else
                    break;
            }
            
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
            sp1.init(mem, UnitTestHelpers.GetMemory());

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            //
            // NewBorn learning stage.
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];

            int maxSPLearningCycles = 10000;

            List<(double Element, (int Cycle, double Similarity)[] Oscilations)> oscilationResult = new List<(double Element, (int Cycle, double Similarity)[] Oscilations)>();

            //
            // This trains SP on input pattern.
            // It performs some kind of unsupervised new-born learning.
            foreach (var input in inputs)
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(path, $"Oscilations_Boost_10_{input}.csv")))
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

                        writer.Flush();
                    }

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

        #endregion

        #region Experiment 2
        /// <summary>
        /// It learns SP and shows the convergence of SDR for the given input.
        /// It repeats the learning process for every input many times (i.e.: 10000+ cycles) and writes out
        /// the state of SDR. Then it counts number or instabe states. It means once SP generates stable SDR, it 
        /// can hapen that in some hihg cycle SDR changes. NUmber of such instable cycles is counted and outputed as a result.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        [TestCategory("Experiment")]
        public void SpatialPooler_Stability_Experiment2()
        {
            int inputBits = 100;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            double minverlapCycles = 0.1;

            p.Set(KEY.MAX_BOOST, 0.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minverlapCycles);
            // Stops the bumping of inactive columns.
            //p.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true); Obsolete.use KEY.MIN_PCT_OVERLAP_DUTY_CYCLES = 0;

            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);
            
            // Local inhibition
            //p.Set(KEY.POTENTIAL_RADIUS, 50);
            //p.Set(KEY.GLOBAL_INHIBITION, false);
            //p.setInhibitionRadius(15);

            // Global inhibition
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            //p.Set(KEY.GLOBAL_INHIBITION, true);
            //p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            //p.Set(KEY.POTENTIAL_RADIUS, inputBits);
            //p.Set(KEY.LOCAL_AREA_DENSITY, -1);
            //p.setInhibitionRadius( Automatically set on the columns pace in a case of global inhibition.)


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

            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 });
            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, });
            RunSpStabilityExperiment2(minverlapCycles, inputBits, p, encoder, inputValues);
        }

        private void RunSpStabilityExperiment2(double minOverlapCycles, int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            string path = nameof(SpatialPooler_Stability_Experiment2);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);

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
            sp1.init(mem, UnitTestHelpers.GetMemory());

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            double[] inputs = inputValues.ToArray();
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            int maxSPLearningCycles = 25000;

            List<(double Element, (int Cycle, double Similarity)[] Oscilations)> oscilationResult = new List<(double Element, (int Cycle, double Similarity)[] Oscilations)>();

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                //if (cycle >= 300)
                //    mem.updateMinPctOverlapDutyCycles(0.0);

                Debug.WriteLine($"Cycle  ** {cycle} **");

                List<(int Cycle, double Similarity)> elementOscilationResult = new List<(int Cycle, double Similarity)>();

                //
                // This trains SP on input pattern.
                // It performs some kind of unsupervised new-born learning.
                foreach (var input in inputs)
                {
                    double similarity;

                    using (StreamWriter similarityWriter = new StreamWriter(Path.Combine(path, $"Oscilations_Boost_{minOverlapCycles}_{input}.csv"), true))
                    {
                        using (StreamWriter sdrWriter = new StreamWriter(Path.Combine(path, $"ActiveColumns_Boost_{minOverlapCycles}_{input}.csv"), true))
                        {
                            using (StreamWriter sdrPlotlyWriter = new StreamWriter(Path.Combine(path, $"ActiveColumns_Boost_{minOverlapCycles}_{input}_plotly-input.csv"), true))
                            {
                                Debug.WriteLine($"Learning Cycles: {maxSPLearningCycles}");
                                Debug.WriteLine($"MAX_BOOST={p[KEY.MAX_BOOST]}, DUTY ={p[KEY.DUTY_CYCLE_PERIOD]}");
                                Debug.WriteLine("Cycle;Similarity");

                                Debug.WriteLine($"Input: {input}");

                                var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;

                                var activeColumns = layer1.GetResult("sp") as int[];

                                var actCols = activeColumns.OrderBy(c => c).ToArray();

                                Debug.WriteLine($" {cycle.ToString("D4")} SP-OUT: [{actCols.Length}/{MathHelpers.CalcArraySimilarity(prevActiveCols[input], actCols)}] - {Helpers.StringifyVector(actCols)}");
                                sdrWriter.WriteLine($"{cycle.ToString("D4")} [{actCols.Length}/{MathHelpers.CalcArraySimilarity(prevActiveCols[input], actCols)}] - {Helpers.StringifyVector(actCols)}");
                                sdrPlotlyWriter.WriteLine($"{Helpers.StringifyVector(actCols)}");

                                similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                                if (similarity < 60.0)
                                {
                                    var tp = (Cycle: cycle, Similarity: similarity);

                                    elementOscilationResult.Add(tp);
                                }

                                prevActiveCols[input] = activeColumns;
                                prevSimilarity[input] = similarity;

                                similarityWriter.WriteLine($"{cycle};{similarity}");

                                sdrPlotlyWriter.Flush();
                            }
                            sdrWriter.Flush();
                        }

                        similarityWriter.Flush();
                    }

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
        #endregion

    }
}

