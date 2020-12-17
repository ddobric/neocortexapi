using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;

namespace NeoCortexApiSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello NeocortexApi!");

            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;
            int inputBits = 200;
            int numColumns = 2048;

            HtmConfig cfg = new HtmConfig()
            {
                InputDimensions = new int[] { inputBits },
                ColumnDimensions = new int[] { numColumns },
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = true,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.8 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),
            };

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

            // We create here 100 random input values.
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            RunExperiment(cfg, encoder, inputValues);
        }

        private static void RunExperiment(HtmConfig cfg, EncoderBase encoder, List<double> inputValues)
        {
            bool learn = true;

            var mem = new Connections(cfg);

            bool isInStableState = false;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 15,
                (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                // Event should only be fired when entering the stable state.
                // Ideal SP should never enter unstable state after stable state.
                if (isStable == false)
                {
                    isInStableState = false;
                }
                else
                {
                    isInStableState = true;
                }
            });

            SpatialPoolerMT sp = new SpatialPoolerMT(hpa);

            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");
          
            cortexLayer.HtmModules.Add("encoder", encoder);
            
            cortexLayer.HtmModules.Add("sp", sp);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            double[] inputs = inputValues.ToArray();
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            int maxSPLearningCycles = 1000;

            List<(double Element, (int Cycle, double Similarity)[] Oscilations)> oscilationResult = new List<(double Element, (int Cycle, double Similarity)[] Oscilations)>();

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                if (isInStableState)
                    Debug.WriteLine($"STABILITY entered at cycle {cycle}.");

                Debug.WriteLine($"Cycle  ** {cycle} **");

                //
                // This trains SP on input pattern.
                // It performs some kind of unsupervised new-born learning.
                foreach (var input in inputs)
                {
                    double similarity;

                    Debug.WriteLine("Cycle;Similarity");

                    Debug.WriteLine($"Input: {input}");

                    var lyrOut = cortexLayer.Compute((object)input, learn) as ComputeCycle;

                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    Debug.WriteLine($" {cycle.ToString("D4")} Active Columns: [{actCols.Length}/{similarity}] - {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }
            }
        }
    }
}
