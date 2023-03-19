using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApi.Experiments
{
    [TestClass]
    public class SpatialSimilarityExperiment
    {
        [TestMethod]
        public void SpatialSimilarityExperimentTest()
        {
            Console.WriteLine($"Hello {nameof(SpatialSimilarityExperiment)} experiment.");

            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represent an input vector (pattern).
            int inputBits = 200;

            // We will build a slice of the cortex with the given number of mini-columns
            int numColumns = 2048;

            //
            // This is a set of configuration parameters used in the experiment.
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = 0.5,
                ActivationThreshold = 10,
                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42)
            };

            double max = 100;

            //
            // This dictionary defines a set of typical encoder parameters.
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

            //
            // We create here 100 random input values.
            List<int[]> inputValues = GetTrainingvectors(0, inputBits);

            RunExperiment(cfg, encoder, inputValues);
        }


        /// <summary>
        /// Creates training vectors.
        /// </summary>
        /// <param name="experimentCode"></param>
        /// <param name="inputBits"></param>
        /// <returns></returns>
        private List<int[]> GetTrainingvectors(int experimentCode, int inputBits)
        {
            if (experimentCode == 0)
            {
                //
                // We create here 2 vectors.
                List<int[]> inputValues = new List<int[]>();

                inputValues.Add(NeoCortexUtils.CreateVector(inputBits, 5, 15 + 5));
                inputValues.Add(NeoCortexUtils.CreateVector(inputBits, 3, 15 + 5 + 3));

                return inputValues;
            }
            else
                throw new ApplicationException("Invalid experimentCode");
        }

        /// <summary>
        /// Implements the experiment.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="encoder"></param>
        /// <param name="inputValues"></param>
        private static void RunExperiment(HtmConfig cfg, EncoderBase encoder, List<int[]> inputValues)
        {
            // Creates the htm memory.
            var mem = new Connections(cfg);

            bool isInStableState = false;

            //
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPoolerMT(hpa);

            // Initializes the 
            sp.Init(mem);

            // Will hold the SDR of every inputs.
            Dictionary<string, int[]> prevActiveCols = new Dictionary<string, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<string, double> prevSimilarity = new Dictionary<string, double>();

            //
            // Initiaize start similarity to zero.
            for (int i = 0; i < inputValues.Count; i++)
            {
                string inputKey = GetInputGekFromIndex(i);
                prevSimilarity.Add(inputKey, 0.0);
                prevActiveCols.Add(inputKey, new int[0]);
            }

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 1000;

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

                //
                // This trains the layer on input pattern.
                for (int inputIndx = 0; inputIndx < inputValues.Count; inputIndx++)
                {
                    string inputKey = GetInputGekFromIndex(inputIndx);
                    int[] input = inputValues[inputIndx];

                    double similarity;

                    int[] activeColumns = new int[(int)cfg.NumColumns];

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    sp.compute(input, activeColumns, true);

                    List<int[,]> twoDimArrays = new List<int[,]>();
                    int[,] twoDimInpArray = ArrayUtils.Make2DArray<int>(input, (int)(Math.Sqrt(input.Length) + 0.5), (int)(Math.Sqrt(input.Length) + 0.5));
                    twoDimArrays.Add(twoDimInpArray = ArrayUtils.Transpose(twoDimInpArray));
                    int[,] twoDimOutArray = ArrayUtils.Make2DArray<int>(activeColumns, (int)(Math.Sqrt(cfg.NumColumns) + 0.5), (int)(Math.Sqrt(cfg.NumColumns) + 0.5));
                    twoDimArrays.Add(twoDimInpArray = ArrayUtils.Transpose(twoDimOutArray));

                    NeoCortexUtils.DrawBitmaps(twoDimArrays, $"{inputKey}.png", Color.Yellow, Color.Gray, 1024, 1024);

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[inputKey]);

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[inputKey] = activeColumns;
                    prevSimilarity[inputKey] = similarity;
                }
            }
        }

        private static string GetInputGekFromIndex(int i)
        {
            return $"I-{i.ToString("D2")}";
        }
    }
}
