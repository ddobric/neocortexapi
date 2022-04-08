// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace UnitTestsProject.NoiseExperiments
{
    [TestClass]
    public class NoiseUnitTests
    {
        private const int OutImgSize = 1024;

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void SpatialSequenceLearningExperiment()
        {
            var parameters = GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, 64 * 64);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
            parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * 64 * 64);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * 64 * 64);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
            parameters.Set(KEY.MAX_BOOST, 5);

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            double[] inputSequence = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };

            var inputVectors = GetEncodedSequence(inputSequence, 0.0, 100.0);

            parameters.apply(mem);

            sp.Init(mem, UnitTestHelpers.GetMemory());

            foreach (var inputVector in inputVectors)
            {
                for (int i = 0; i < 3; i++)
                {
                    var activeIndicies = sp.Compute(inputVector, true, true) as int[];
                    var activeArray = sp.Compute(inputVector, true, false) as int[];

                    Debug.WriteLine(Helpers.StringifyVector(activeArray));
                    Debug.WriteLine(Helpers.StringifyVector(activeIndicies));
                }
            }
        }


        public List<int[]> GetEncodedSequence(double[] inputSequence, double min, double max)
        {
            List<int[]> sdrList = new List<int[]>();

            string outFolder = nameof(NoiseUnitTests);

            Directory.CreateDirectory(outFolder);

            DateTime now = DateTime.Now;

            ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 21},
                { "N", 1024},
                { "Radius", -1.0},
                { "MinVal", min},
                { "MaxVal", max},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            });

            foreach (var i in inputSequence)
            {
                var result = encoder.Encode(i);

                sdrList.Add(result);

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{i}.png", Color.Yellow, Color.Black, text: i.ToString());
            }

            return sdrList;
        }

        internal static Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 50.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);

            return parameters;
        }


        #region Noise Experiment Paper

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void NoiseExperimentTest()
        {
            const int colDimSize = 64;

            const int noiseStepPercent = 5;

            var parameters = GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, 32 * 32);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
            parameters.Set(KEY.INHIBITION_RADIUS, (int)0.01 * colDimSize * colDimSize);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * colDimSize * colDimSize);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
            parameters.Set(KEY.MAX_BOOST, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.01);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.0);

            parameters.Set(KEY.SEED, 42);

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { colDimSize, colDimSize });

            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            parameters.apply(mem);
            sp.Init(mem);

            List<int[]> inputVectors = new List<int[]>();

            inputVectors.Add(getInputVector1());
            inputVectors.Add(getInputVector2());

            int vectorIndex = 0;

            int[][] activeColumnsWithZeroNoise = new int[inputVectors.Count][];

            foreach (var inputVector in inputVectors)
            {
                var x = getNumBits(inputVector);

                Debug.WriteLine("");
                Debug.WriteLine($"----- VECTOR {vectorIndex} ----------");

                // Array of active columns with zero noise. The reference (ideal) output.
                activeColumnsWithZeroNoise[vectorIndex] = new int[colDimSize * colDimSize];

                int[] activeArray = null;

                for (int j = 0; j < 25; j += noiseStepPercent)
                {
                    Debug.WriteLine($"--- Vector {0} - Noise Iteration {j} ----------");

                    int[] noisedInput;

                    if (j > 0)
                    {
                        noisedInput = ArrayUtils.FlipBit(inputVector, (double)((double)j / 100.00));
                    }
                    else
                        noisedInput = inputVector;

                    // TODO: Try CalcArraySimilarity
                    var d = MathHelpers.GetHammingDistance(inputVector, noisedInput, true);
                    Debug.WriteLine($"Input with noise {j} - HamDist: {d}");
                    Debug.WriteLine($"Original: {Helpers.StringifyVector(inputVector)}");
                    Debug.WriteLine($"Noised:   {Helpers.StringifyVector(noisedInput)}");

                    for (int i = 0; i < 10; i++)
                    {
                        activeArray = sp.Compute(noisedInput, true, returnActiveColIndiciesOnly: false) as int[];

                        if (j > 0)
                            Debug.WriteLine($"{ MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true)} -> {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");
                    }

                    if (j == 0)
                    {
                        Array.Copy(activeArray, activeColumnsWithZeroNoise[vectorIndex], activeColumnsWithZeroNoise[vectorIndex].Length);
                    }

                    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                    var d2 = MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true);
                    Debug.WriteLine($"Output with noise {j} - Ham Dist: {d2}");
                    Debug.WriteLine($"Original: {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeColumnsWithZeroNoise[vectorIndex], (el) => el == 1))}");
                    Debug.WriteLine($"Noised:   {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");

                    List<int[,]> arrays = new List<int[,]>();

                    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 64, 64);
                    twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                    arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(noisedInput, 32, 32)));
                    arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(activeArray, 64, 64)));

                    //   NeoCortexUtils.DrawHeatmaps(bostArrays, $"{outputImage}_boost.png", 1024, 1024, 150, 50, 5);
                    NeoCortexUtils.DrawBitmaps(arrays, $"Vector_{vectorIndex}_Noise_{j * 10}.png", Color.Yellow, Color.Gray, OutImgSize, OutImgSize);
                }

                vectorIndex++;
            }

            vectorIndex = OutputPredictionResult(sp, inputVectors, activeColumnsWithZeroNoise);
        }


        /// <summary>
        ///  Prediction code.
        ///  This method takes a single sample of every input vector and adds
        ///  some noise to it. Then it predicts it.<param name="sp"></param>
        ///  Calculated hamming distance (percent overlap) between predicted output and output <param name="inputVectors"></param>
        ///  trained without noise is final result, which should be higher than 95% (realistic guess).
        /// </summary>
        /// <param name="activeColumnsWithZeroNoise"></param>
        /// <returns></returns>
        private static int OutputPredictionResult(SpatialPoolerMT sp, List<int[]> inputVectors, int[][] activeColumnsWithZeroNoise)
        {
            int vectorIndex = 0;
            foreach (var inputVector in inputVectors)
            {
                for (int tstIndx = 0; tstIndx < 100; tstIndx++)
                {
                    double noise = ((double)(new Random().Next(5, 25))) / 100.0;

                    var noisedInput = ArrayUtils.FlipBit(inputVector, noise);

                    var distIn = MathHelpers.GetHammingDistance(inputVector, noisedInput, true);

                    int[] activeArray = new int[64 * 64];

                    sp.compute(noisedInput, activeArray, false);

                    var distOut = MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true);

                    Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1));
                    Helpers.StringifyVector(ArrayUtils.IndexWhere(activeColumnsWithZeroNoise[vectorIndex], (el) => el == 1));

                    Debug.WriteLine($"Result for vector {vectorIndex} with noise {noise} - DistIn: {distIn} - DistOut: {distOut}");

                    Debug.WriteLine("------------");

                    //Assert.IsTrue(distOut >= 90);
                }

                vectorIndex++;
            }

            return vectorIndex;
        }

        private static int[] getInputVector1()
        {
            int[] inputVector = new int[1024];

            for (int i = 0; i < 31; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (i > 2 && i < 18 && j > 2 && j < 19)
                        inputVector[i * 32 + j] = 1;
                    else
                        inputVector[i * 32 + j] = 0;
                }
            }

            return inputVector;
        }

        private static int[] getInputVector2()
        {
            int[] inputVector = new int[1024];

            for (int i = 0; i < 31; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (i > 12 && i < 24 && j > 19 && j < 30)
                        inputVector[i * 32 + j] = 1;
                    else
                        inputVector[i * 32 + j] = 0;
                }
            }

            return inputVector;
        }

        private static int getNumBits(int[] data)
        {
            int cnt = 0;
            foreach (var item in data)
            {
                if (item == 1)
                    cnt++;
            }

            return cnt;
        }
        #endregion
    }
}
