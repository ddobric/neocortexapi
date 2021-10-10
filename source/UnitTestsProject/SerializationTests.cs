using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTestsProject
{
    [TestClass]
    public class SerializationTests
    {

        #region Serialization
        /// <summary>
        /// It learns SP and shows the convergence of SDR for the given input.
        /// In contrast to Experiment_1, the new feature called 'New Born' effect is activated.That means, SP is learning as usual, with activated column boosting.
        /// In contrast to Experiment2, this experiment uses newborn effect as built-in feature of SP. It uses the homeostatic plasticity activator.
        /// /// </summary>
        [TestMethod]
        [TestCategory("Experiment")]
        public void SerializationTest()
        {
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;
            int inputBits = 200;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.MAX_BOOST, maxBoost);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minOctOverlapCycles);

            // Global inhibition
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            p.Set(KEY.POTENTIAL_RADIUS, (int)(0.8 * inputBits));
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.
            //p.setInhibitionRadius( Automatically set on the columns pace in a case of global inhibition.);

            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));
            double max = 100;

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

            //  List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 });

            // We create here 100 random input values.
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            RunSerializationExperiment(maxBoost, minOctOverlapCycles, inputBits, p, encoder, inputValues);
        }

        private void RunSerializationExperiment(double maxBoost, double minOverlapCycles, int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            string path = nameof(RunSerializationExperiment);

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

            var mem = new Connections();

            bool isInStableState = false;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 15, (isStable, numPatterns, actColAvg, seenInputs) =>
            {

                Assert.IsTrue(numPatterns == inputValues.Count);

                // Event should only be fired when entering the stable state.
                // Ideal SP should never enter unstable state after stable state.
                if (isStable == false)
                {
                    isInStableState = false;
                    Debug.WriteLine($"UNSTABLE!: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                }
                else
                {
                    //Assert.IsTrue(isStable);

                    isInStableState = true;
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                }
            });

            SpatialPooler sp1 = new SpatialPooler(hpa);

            p.apply(mem);
            sp1.Init(mem, UnitTestHelpers.GetMemory());

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

            int maxSPLearningCycles = 5000;

            List<(double Element, (int Cycle, double Similarity)[] Oscilations)> oscilationResult = new List<(double Element, (int Cycle, double Similarity)[] Oscilations)>();

            Debug.WriteLine($"Learning Cycles: {maxSPLearningCycles}");
            Debug.WriteLine($"MAX_BOOST={p[KEY.MAX_BOOST]}, DUTY ={p[KEY.DUTY_CYCLE_PERIOD]}");

            HtmSerializer ser = new HtmSerializer();

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                if (isInStableState)
                    Debug.WriteLine($"STABILITY entered at cycle {cycle}.");

                Debug.WriteLine($"Cycle  ** {cycle} **");

                List<(int Cycle, double Similarity)> elementOscilationResult = new List<(int Cycle, double Similarity)>();

                //
                // This trains SP on input pattern.
                // It performs some kind of unsupervised new-born learning.
                foreach (var input in inputs)
                {
                    double similarity;

                    using (StreamWriter similarityWriter = new StreamWriter(Path.Combine(path, $"Oscilations_MaxBoost_{maxBoost}_MinOverl_{minOverlapCycles}_{input}.csv"), true))
                    {
                        using (StreamWriter sdrWriter = new StreamWriter(Path.Combine(path, $"ActiveColumns_Boost_{minOverlapCycles}_{input}.csv"), true))
                        {
                            using (StreamWriter sdrPlotlyWriter = new StreamWriter(Path.Combine(path, $"ActiveColumns_MaxBoost_{maxBoost}_MinOverl_{minOverlapCycles}_{input}_plotly-input.csv"), true))
                            {
                                Debug.WriteLine("Cycle;Similarity");

                                Debug.WriteLine($"Input: {input}");

                                var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;

                                var activeColumns = layer1.GetResult("sp") as int[];

                                var actCols = activeColumns.OrderBy(c => c).ToArray();

                                // TODO. When you enter here you can start serialization/sdeserialization code.
                                //if(isInStableState)
                                //    DrawBitmaps(encoder, input, activeColumns, 2048);
                                //ser.Serialize(sp1, "abc.json");

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


        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationDistalSegmentTest()
        {
            Dictionary<Cell, List<DistalDendrite>> distalSegments = new Dictionary<Cell, List<DistalDendrite>>();
            distalSegments.Add(new Cell(), new List<DistalDendrite>() { new DistalDendrite(new Cell(), 1, 1, 1, 1.1, 100) { } });

            var x = new { DistalSegments = distalSegments };

            HtmSerializer ser = new HtmSerializer();
            ser.Serialize(x, "distalsegment.json");
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationAbstractSparseBinaryMatrixTest()
        {

        }


        private void DrawBitmaps(EncoderBase encoder, double input, int[] activeArrayIndxes, int columnTopology)
        {
            var inputVector = encoder.Encode(input);

            int[] activeArray = ArrayUtils.FillAtIndexes(activeArrayIndxes, columnTopology, 1);

            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 1 + (int)Math.Sqrt(columnTopology), 1 + (int)Math.Sqrt(columnTopology));

            twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
            List<int[,]> arrays = new List<int[,]>();
            arrays.Add(twoDimenArray);
            arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length))));

            const int OutImgSize = 1024;
            NeoCortexUtils.DrawBitmaps(arrays, $"Input_{input}.png", Color.Yellow, Color.Gray, OutImgSize, OutImgSize);
        }
        #endregion
    }
}
