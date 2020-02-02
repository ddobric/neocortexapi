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
            sp1.init(mem, UnitTestHelpers.GetMemory());
            tm1.init(mem);
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
                    cls.Learn(input, lyrOut.ActiveCells.ToArray(), lyrOut.predictiveCells.ToArray());
                    Debug.WriteLine($"Current Input: {input}");
                    if (learn == false)
                    {
                        Debug.WriteLine($"Predict Input When Not Learn: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                    }
                    else
                    {
                        Debug.WriteLine($"Predict Input: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
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
            sp1.init(mem, UnitTestHelpers.GetMemory());
            tm1.init(mem);

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

                    cls.Learn(input, lyrOut.ActiveCells.ToArray(), lyrOut.predictiveCells.ToArray());

                    Debug.WriteLine($"-------------- {input} ---------------");

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.predictiveCells.Select(c => c.Index).ToArray())}");

                    Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                }

                if (i == 50)
                {
                    Debug.WriteLine("Stop Learning From Here. Entering inference mode.");
                    learn = false;
                }

                tm1.reset(mem);
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
            sp1.init(mem, UnitTestHelpers.GetMemory());
            tm1.init(mem);

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

                    cls.Learn(input, lyrOut.ActiveCells.ToArray(), lyrOut.predictiveCells.ToArray());

                    Debug.WriteLine($"-------------- {input} ---------------");

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.predictiveCells.Select(c => c.Index).ToArray())}");

                    var predictedValue = cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray());

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
            }

            cls.TraceState();

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }



        /// <summary>
        ///
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        [TestCategory("Experiment")]
        public void MusicNotesExperiment()
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

            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 0.0, 1.0, 2.0, 0.0, 1.0, 2.0, 2.0, 0.0, 0.1, 2.0 });
            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0 });
            // C-0, D-1, E-2, F-3, G-4, H-5
            //var inputValues = new double[] { 0.0, 0.0, 4.0, 4.0, 5.0, 5.0, 4.0, 3.0, 3.0, 2.0, 2.0, 1.0, 1.0, 0.0 };

            //inputValues = new List<double>(new double[] { 1.0, 2.0, 3.0, 1.0, 5.0, 1.0, 6.0, });

            // RunExperiment(inputBits, p, encoder, inputValues);
            RunExperiment(inputBits, p, encoder, inputValues);
        }


        /// <summary>
        ///
        /// </summary>
        private void RunExperiment(int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //INeuroVisualizer vis = new WSNeuroVisualizer();
            //vis.InitModelAsync(new NeuroModel(null, (new long [10, 0]), 6));
            int maxMatchCnt = 0;
            bool learn = true;
            //INeuroVisualizer vis = new WSNeuroVisualizer();
            //GenerateNeuroModel model = new GenerateNeuroModel();

            //vis.InitModel(model.CreateNeuroModel(new int[] { 1}, (long[,])p[KEY.COLUMN_DIMENSIONS], (int)p[KEY.CELLS_PER_COLUMN]));

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.init(mem, UnitTestHelpers.GetMemory());
            tm1.init(mem);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            //
            // NewBorn learning stage.
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];

            int maxSPLearningCycles = 5;
            List<(double Element, (int Cycle, double Similarity)[] Oscilations)> oscilationResult = new List<(double Element, (int Cycle, double Similarity)[] Oscilations)>();

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

                    Debug.WriteLine($" {i.ToString("D4")} SP-OUT: [{actCols.Length}/{MathHelpers.CalcArraySimilarity(prevActiveCols, actCols)}] - {Helpers.StringifyVector(actCols)}");
                }
            }


            // Here we add TM module to the layer.
            layer1.HtmModules.Add("tm", tm1);

            int cycle = 0;
            int matches = 0;

            double lastPredictedValue = 0;

            //
            // Now training with SP+TM. SP is pretrained on the given input pattern.
            for (int i = 0; i < 1460; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                foreach (var input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    cls.Learn(input, lyrOut.ActiveCells.ToArray(), lyrOut.predictiveCells.ToArray());
                    //vis.UpdateSynapsesAsync();

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.predictiveCells.Select(c => c.Index).ToArray())}");

                    if (input == lastPredictedValue)
                    {
                        matches++;
                        Debug.WriteLine($"Match {input}");
                    }
                    else
                        Debug.WriteLine($"Missmatch Actual value: {input} - Predicted value: {lastPredictedValue}");

                    if (lyrOut.predictiveCells.Count > 0)
                    {
                        var predictedInputValue = cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray());

                        Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {predictedInputValue}");

                        lastPredictedValue = predictedInputValue;
                    }
                    else
                        Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
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
                        double predictedInputValue = inputValues[new Random().Next(0, inputValues.Count - 1)];

                        List<double> predictedValues = new List<double>();

                        while (--cnt > 0)
                        {
                            var lyrOut = layer1.Compute(predictedInputValue, learn) as ComputeCycle;
                            predictedInputValue = cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray());
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

            cls.TraceState();

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }



        ///
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
            p.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);
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

            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0});
            //List<double> inputValues = new List<double>(new double[] { 0.0, });

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
        public void Abc()
        {
            INeuroVisualizer vis = new WSNeuroVisualizer();

            //vis.InitModel();
            //vis.UpdateColumnOverlaps
            List<MiniColumn> colData = new List<MiniColumn>();
            MiniColumn updateOverlap = new MiniColumn(0, 0, 0, 0);
            updateOverlap.Overlap = 0.9;

            colData.Add(updateOverlap);
            // vis.UpdateColumnOverlapsAsync(colData);
        }
    }
}
