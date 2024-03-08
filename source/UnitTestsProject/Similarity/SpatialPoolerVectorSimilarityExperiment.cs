using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace NeoCortexApi.Experiments
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class SpatialPoolerVectorSimilarityExperiment
    {
        /// <summary>
        /// Generates artifficial vectors and calculates the input and output similarity.
        /// </summary>
        [TestMethod]
        public void VectorSimilarityExperimentTest()
        {
            Console.WriteLine($"Hello {nameof(SpatialPoolerVectorSimilarityExperiment)} experiment.");

            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represe nt an input vector (pattern).
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
                StimulusThreshold = 5,
                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = 0.5,
                ActivationThreshold = 10,
                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42)
            };

            int width = 15;
 
            //
            // We create here 100 random input values.
            List<int[]> inputValues = GetTrainingvectors(0, inputBits, width);

            RunExperiment(cfg, inputValues);
        }


        /// <summary>
        /// Creates training vectors.
        /// </summary>
        /// <param name="experimentCode"></param>
        /// <param name="inputBits"></param>
        /// <returns></returns>
        private List<int[]> GetTrainingvectors(int experimentCode, int inputBits, int width)
        {
            if (experimentCode == 0)
            {
                //
                // We create here 2 vectors.
                List<int[]> inputValues = new List<int[]>();

                for (int i = 0; i < 10; i += 1)
                {
                    inputValues.Add(NeoCortexUtils.CreateVector(inputBits, i, i + width));
                }


                return inputValues;
            }
            else if (experimentCode == 1)
            {
                // todo. create or load other test vectors/images here 
                // We create here 2 vectors.
                List<int[]> inputValues = new List<int[]>();

                for (int i = 0; i < 10; i += 1)
                {
                    inputValues.Add(NeoCortexUtils.CreateVector(inputBits, i, i + width));
                }

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
        private static void RunExperiment(HtmConfig cfg, List<int[]> inputValues)
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
                }, requiredSimilarityThreshold: 0.975);

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPoolerMT(hpa);

            // Initializes the 
            sp.Init(mem);

            // Holds the indicies of active columns of the SDR.
            Dictionary<string, int[]> prevActiveColIndicies = new Dictionary<string, int[]>();

            // Holds the active column SDRs.
            Dictionary<string, int[]> prevActiveCols = new Dictionary<string, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<string, double> prevSimilarity = new Dictionary<string, double>();

            //
            // Initiaize start similarity to zero.
            for (int i = 0; i < inputValues.Count; i++)
            {
                string inputKey = GetInputGekFromIndex(i);
                prevSimilarity.Add(inputKey, 0.0);
                prevActiveColIndicies.Add(inputKey, new int[0]);
            }

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 1000;

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                //Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

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
                    // DrawImages(cfg, inputKey, input, activeColumns);

                    var actColsIndicies = ArrayUtils.IndexWhere(activeColumns, c => c == 1);

                    similarity = MathHelpers.CalcArraySimilarity(actColsIndicies, prevActiveColIndicies[inputKey]);

                    Debug.WriteLine($"[i={inputKey}, cols=:{actColsIndicies.Length} s={similarity}] SDR: {Helpers.StringifyVector(actColsIndicies)}");

                    prevActiveCols[inputKey] = activeColumns;
                    prevActiveColIndicies[inputKey] = actColsIndicies;
                    prevSimilarity[inputKey] = similarity;

                    if (isInStableState)
                    {
                        GenerateResult(cfg, inputValues, prevActiveColIndicies, prevActiveCols);
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// Draws all inputs and related SDRs. It also outputs the similarity matrix.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="inputValues"></param>
        /// <param name="activeColIndicies"></param>
        /// <param name="activeCols"></param>
        private static void GenerateResult(HtmConfig cfg, List<int[]> inputValues,
            Dictionary<string, int[]> activeColIndicies, Dictionary<string, int[]> activeCols)
        {
            int inpLen = (int)(Math.Sqrt(inputValues[0].Length) + 0.5);

            Dictionary<string, int[]> inpVectorsMap = new Dictionary<string, int[]>();

            for (int k = 0; k < inputValues.Count; k++)
            {
                inpVectorsMap.Add(GetInputGekFromIndex(k), ArrayUtils.IndexWhere(inputValues[k], c => c == 1));
            }

            var outRes = MathHelpers.CalculateSimilarityMatrix(activeColIndicies);

            var inRes = MathHelpers.CalculateSimilarityMatrix(inpVectorsMap);

            string[,] matrix = new string[inpVectorsMap.Keys.Count, inpVectorsMap.Keys.Count];
            int i = 0;
            foreach (var inputKey in inpVectorsMap.Keys)
            {
                for (int j = 0; j < inpVectorsMap.Keys.Count; j++)
                {
                    matrix[i, j] = $"{inRes[i, j].ToString("0.##")}/{outRes[i, j].ToString("0.##")}";
                }

                DrawBitmaps(cfg, inputKey, inputValues[i], inpLen, activeCols[inputKey]);

                i++;
            }

            var res = Helpers.RenderSimilarityMatrix(inpVectorsMap.Keys.ToArray(), matrix);

            Console.WriteLine(res);
            Debug.WriteLine(res);
        }


        /// <summary>
        /// Draws the input and the corresponding SDR.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="inputKey"></param>
        /// <param name="input"></param>
        /// <param name="inpLen"></param>
        /// <param name="activeColumns"></param>
        private static void DrawBitmaps(HtmConfig cfg, string inputKey, int[] input, int inpLen, int[] activeColumns)
        {
            List<int[,]> twoDimArrays = new List<int[,]>();
            int[,] twoDimInpArray = ArrayUtils.Make2DArray<int>(input, inpLen, inpLen);
            twoDimArrays.Add(twoDimInpArray = ArrayUtils.Transpose(twoDimInpArray));
            int[,] twoDimOutArray = ArrayUtils.Make2DArray<int>(activeColumns, (int)(Math.Sqrt(cfg.NumColumns) + 0.5), (int)(Math.Sqrt(cfg.NumColumns) + 0.5));
            twoDimArrays.Add(twoDimInpArray = ArrayUtils.Transpose(twoDimOutArray));

            NeoCortexUtils.DrawBitmaps(twoDimArrays, $"{inputKey}.png", Color.Yellow, Color.Gray, 1024, 1024);
        }

        private static void DrawImages(HtmConfig cfg, string inputKey, int[] input, int[] activeColumns)
        {
            List<int[,]> twoDimArrays = new List<int[,]>();
            int[,] twoDimInpArray = ArrayUtils.Make2DArray<int>(input, (int)(Math.Sqrt(input.Length) + 0.5), (int)(Math.Sqrt(input.Length) + 0.5));
            twoDimArrays.Add(twoDimInpArray = ArrayUtils.Transpose(twoDimInpArray));
            int[,] twoDimOutArray = ArrayUtils.Make2DArray<int>(activeColumns, (int)(Math.Sqrt(cfg.NumColumns) + 0.5), (int)(Math.Sqrt(cfg.NumColumns) + 0.5));
            twoDimArrays.Add(twoDimInpArray = ArrayUtils.Transpose(twoDimOutArray));

            NeoCortexUtils.DrawBitmaps(twoDimArrays, $"{inputKey}.png", Color.Yellow, Color.Gray, 1024, 1024);
        }

        private static string GetInputGekFromIndex(int i)
        {
            return $"I-{i.ToString("D2")}";
        }
    }
}
