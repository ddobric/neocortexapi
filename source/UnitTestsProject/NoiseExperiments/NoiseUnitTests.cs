// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Azure.Documents.Spatial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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


        #region Noise Experiments Paper and PhD

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void NoiseExperimentTest()
        {
            Helpers.CreateTestFolder(nameof(NoiseExperimentTest));

            int inpDimSize = 32;
            int colDimSize = 64;
            int potRad = -1;

            // If this value is increased, the memorizing of the seen pattern increases.
            // Higher PotentionPct means that more synapses are connected to the input and the pattern keeps
            // recognized for larger portion of the noiseLevel. 
            double potRadPtc = 0.5;

            Parameters parameters = InitConfigParams(new int[] { colDimSize, colDimSize }, new int[] { inpDimSize, inpDimSize }, potRad, potRadPtc);

            var sp = new SpatialPooler();
            var mem = new Connections();

            parameters.apply(mem);
            sp.Init(mem);

            List<int[]> inputVectors = GetBoxVectors();

            int vectorIndex = 0;

            int[][] activeColumnsWithZeroNoise = new int[inputVectors.Count][];

            foreach (var inputVector in inputVectors)
            {
                using (StreamWriter sw = new StreamWriter($"Noise_experiment_result_vector{vectorIndex}_PotRad{potRad}_potRadPtc{potRadPtc}.txt"))
                {
                    var x = getNumBits(inputVector);

                    Debug.WriteLine("");
                    Debug.WriteLine($"----- VECTOR {vectorIndex} ----------");

                    // Array of active columns with zero noiseLevel. The reference (ideal) output.
                    activeColumnsWithZeroNoise[vectorIndex] = new int[colDimSize * colDimSize];

                    int[] activeArray = null;

                    for (int noiseLevel = 0; noiseLevel < 100; noiseLevel += 5)
                    {
                        Debug.WriteLine($"--- Vector {0} - Noise Iteration {noiseLevel} ----------");

                        int[] noisedInput;

                        if (noiseLevel > 0)
                        {
                            noisedInput = ArrayUtils.FlipBit(inputVector, (double)((double)noiseLevel / 100.00));
                        }
                        else
                            noisedInput = inputVector;

                        // TODO: Try CalcArraySimilarity
                        var inpDist = MathHelpers.GetHammingDistance(inputVector, noisedInput, true);
                        Debug.WriteLine($"Input with noiseLevel {noiseLevel} - HamDist: {inpDist}");
                        Debug.WriteLine($"Original: {Helpers.StringifyVector(inputVector)}");
                        Debug.WriteLine($"Noised:   {Helpers.StringifyVector(noisedInput)}");

                        for (int i = 0; i < 10; i++)
                        {
                            activeArray = sp.Compute(noisedInput, true, returnActiveColIndiciesOnly: false) as int[];

                            if (noiseLevel > 0)
                                Debug.WriteLine($"{MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true)} -> {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");
                        }

                        if (noiseLevel == 0)
                        {
                            Array.Copy(activeArray, activeColumnsWithZeroNoise[vectorIndex], activeColumnsWithZeroNoise[vectorIndex].Length);
                        }

                        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                        var outputDistance = MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true);
                        Debug.WriteLine($"Output with noiseLevel {noiseLevel} - Ham Dist: {outputDistance}");
                        Debug.WriteLine($"Original: {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeColumnsWithZeroNoise[vectorIndex], (el) => el == 1))}");
                        Debug.WriteLine($"Noised:   {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");

                        List<int[,]> arrays = new List<int[,]>();

                        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 64, 64);
                        twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                        arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(noisedInput, 32, 32)));
                        arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(activeArray, 64, 64)));

                        var overlap = sp.CalculateOverlap(mem, noisedInput);

                        sp.TraceColumnPermenances("perms.txt");

                        sw.WriteLine($"{noiseLevel};{inpDist};{outputDistance}");

                        CreateOutput(noiseLevel, nameof(NoiseExperimentTest), colDimSize, parameters, vectorIndex, noiseLevel, arrays, overlap, activeCols, sp);
                    }

                    vectorIndex++;
                }
            }


            //vectorIndex = OutputPredictionResult(sp, inputVectors, activeColumnsWithZeroNoise);
        }

        


        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void NoiseExperimentPhdTest()
        {
            Helpers.CreateTestFolder(nameof(NoiseExperimentPhdTest));

            int colDimSize = 64;
            
            int numVects = 30;

            int potRad = -1;
           

            // If this value is increased, the memorizing of the seen pattern increases.
            // Higher PotentionPct means that more synapses are connected to the input and the pattern keeps
            // recognized for larger portion of the noiseLevel. 
            double potRadPtc = 0.8;

            int noiseLevelPct = 5;

            //List<int[]> inputVectors = GetRandomVectors(5, noiseLevelPct);
            List<int[]> inputVectors = GetBoxVectors();

            Parameters parameters = InitConfigParams(new int[] { 64, 64 }, new int[] { 32, 32 }, potRad, potRadPtc);

            var mem = new Connections();

            SpatialPooler sp = RunNewBornStage(mem, inputVectors, parameters);

            int vectorIndex = 0;

            int[][] activeColumnsWithZeroNoise = new int[inputVectors.Count][];

            string fileName = $"Boxed_Noise_experiment_result_for_level_{noiseLevelPct}_vector{vectorIndex}_PotRad{potRad}_potRadPtc{potRadPtc}.txt";

            //
            // Opens the file for writing the results. It writes to CSV file "{noiseLevel};{inpDist};{outputDistance}".
            using (StreamWriter sw = new StreamWriter($"{Path.Combine(nameof(NoiseExperimentPhdTest), fileName)}"))
            {
                //
                // Looping through all input vectors
                foreach (var inputVector in inputVectors)
                {
                    Debug.WriteLine("");
                    Debug.WriteLine($"----- VECTOR {vectorIndex} ----------");

                    // Array of active columns with zero noiseLevel. The reference (ideal) output.
                    activeColumnsWithZeroNoise[vectorIndex] = new int[colDimSize * colDimSize];

                    int[] activeArray = null;

                    // With this value we repeat the creating of sdr 100 times for every noised vector (noiseLevel>0)
                    // The idea is to observe the change of the SDR of noised vectors f the same level.
                    int noiseRepeats = 0;

                    int noiseLevel = 0;

                    // 
                    // It makes sure that only two noie levels are used in the training.
                    // The first one is with zero noiseLevel and the second one is with the noiseLevel defined in the noiseLevelPct variable.
                    while (noiseRepeats <= 100)
                    {
                        //for (int noiseLevel = 0; noiseLevel <= noiseLevelPct; noiseLevel += noiseLevelPct)
                        //{
                        if (noiseRepeats == 0)
                            noiseLevel = 0;
                        else
                            noiseLevel = noiseLevelPct;

                        noiseRepeats++;

                        Debug.WriteLine($"--- Vector {0} - Noise Level = {noiseLevel} ----------");

                        int[] noisedInput;

                        if (noiseLevel > 0)
                        {
                            noisedInput = ArrayUtils.FlipBit(inputVector, (double)((double)noiseLevel / 100.00));
                        }
                        else
                            noisedInput = inputVector;

                        // TODO: Try CalcArraySimilarity
                        var inpDist = MathHelpers.GetHammingDistance(inputVector, noisedInput, true);
                        Debug.WriteLine($"Input with noiseLevel {noiseLevel} - HamDist: {inpDist}");
                        Debug.WriteLine($"Original: {Helpers.StringifyVector(ArrayUtils.IndexWhere(inputVector, (el) => el == 1))}");
                        Debug.WriteLine($"Noised:   {Helpers.StringifyVector(ArrayUtils.IndexWhere(noisedInput, (el) => el == 1))}");

                        for (int i = 0; i < 10; i++)
                        {
                            activeArray = sp.Compute(noisedInput, true, returnActiveColIndiciesOnly: false) as int[];

                            if (noiseLevel > 0)
                                Debug.WriteLine($"{MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true)} -> {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");
                        }

                        if (noiseLevel == 0)
                        {
                            Array.Copy(activeArray, activeColumnsWithZeroNoise[vectorIndex], activeColumnsWithZeroNoise[vectorIndex].Length);
                        }

                        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                        var outputDistance = MathHelpers.GetHammingDistance(activeColumnsWithZeroNoise[vectorIndex], activeArray, true);
                        Debug.WriteLine($"Output with noiseLevel {noiseLevel} - Ham Dist: {outputDistance}");
                        Debug.WriteLine($"Original: {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeColumnsWithZeroNoise[vectorIndex], (el) => el == 1))}");
                        Debug.WriteLine($"Noised:   {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");

                        List<int[,]> arrays = new List<int[,]>();

                        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 64, 64);
                        twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                        arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(noisedInput, 32, 32)));
                        arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(activeArray, 64, 64)));

                        var overlap = sp.CalculateOverlap(mem, noisedInput);

                        sp.TraceColumnPermenances("perms.txt");

                        if (noiseLevel != 0)
                            sw.WriteLine($"{noiseLevel};{inpDist};{outputDistance}");

                        CreateOutput(noiseLevel, nameof(NoiseExperimentPhdTest), colDimSize, parameters, vectorIndex, noiseLevel, arrays, overlap, activeCols, sp, noiseRepeats);
                    }

                    vectorIndex++;
                }
            }
        }


        private static SpatialPooler RunNewBornStage(Connections mem, List<int[]> inpVectors, Parameters parameters)
        {
            bool isInStableState = false;

            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, inpVectors.Count * 55,
             (isStable, numPatterns, actColAvg, seenInputs) =>
             {
                 if (isStable)
                 {
                     isInStableState = true;

                     // Event should be fired when entering the stable state.
                     Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                 }
                 else
                     // Ideal SP should never enter unstable state after stable state.
                     Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // Assert.IsTrue(numPatterns == inpVectors.Count);

             }, numOfCyclesToWaitOnChange: 25);

            SpatialPooler sp = new SpatialPooler(hpc);

            parameters.apply(mem);
            
            sp.Init(mem);

            for (int cycle = 0; cycle < 100000 && isInStableState == false; cycle++)
            {
                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                int indx = 0;

                foreach (var input in inpVectors)
                {
                    Debug.WriteLine($" -- {indx++} --");

                    var actColsNotUsed = sp.Compute(input, true);

                    if (isInStableState)
                        break;
                }

                if (isInStableState)
                    break;

            }

            return sp;
        }


        // Reads the text file with the prediction results and outputs the prediction accuracy.


        private static List<int[]> GetBoxVectors()
        {
            List<int[]> inputVectors = new List<int[]>();

            inputVectors.Add(getInputVector1());
            inputVectors.Add(getInputVector2());
            //inputVectors.Add(getInputVector3());
            //inputVectors.Add(Helpers.GetRandomVector(1024, 5));
            //inputVectors.Add(Helpers.GetRandomVector(1024, 10));
            //inputVectors.Add(Helpers.GetRandomVector(1024, 15));
            //inputVectors.Add(Helpers.GetRandomVector(1024, 20));

            return inputVectors;
        }

        // This method calculates the sum of elements in the array.



        private static List<int[]> GetRandomVectors(int numVects, int noiseLevel)
        {
            int vectorSize = 1024;

            List<int[]> inputVectors = new List<int[]>();

            for (int i = 0; i < numVects; i++)
            {
                inputVectors.Add(Helpers.GetRandomVector(vectorSize, (int)((double)noiseLevel / (double)100 * (double)vectorSize)));
            }

            return inputVectors;
        }

        private static Parameters InitConfigParams(int[] colDims, int[] inpDims, int potRad, double potRadPtc)
        {
            Parameters parameters = GetDefaultParams();
            parameters.Set(KEY.GLOBAL_INHIBITION, true);

            // Set the RF to the entire input space.
            parameters.Set(KEY.POTENTIAL_RADIUS, potRad);

            // If this value is increased, the memorizing of the seen pattern increases.
            // Higher PotentionPct means that more synapses are connected to the input and the pattern keeps
            // recognized for larger portion of the noiseLevel. 
            parameters.Set(KEY.POTENTIAL_PCT, potRadPtc);

            parameters.Set(KEY.STIMULUS_THRESHOLD, 5);
            // parameters.Set(KEY.INHIBITION_RADIUS, (int)0.01 * colDimSize * colDimSize);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * colDims[0] * colDims[1]);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
            parameters.Set(KEY.MAX_BOOST, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.005);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.005);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.0);

            parameters.Set(KEY.SEED, 42);

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(colDims);

            return parameters;
        }


        private void CreateOutput(int noiseLevel, string outFolfer, int colDimSize, Parameters parameters, int vectorIndex, int j, List<int[,]> arrays, int[] overlaps, int[] activeCols, SpatialPooler sp, int repeat = 0)
        {
            List<int> actOverlaps = new List<int>();

            for (int i = 0; i < activeCols.Length; i++)
            {
                actOverlaps.Add(overlaps[i]);
            }

            int actColThreshold = actOverlaps.Count > 0 ? actOverlaps.Min() : 0;

            var twodOverlapArray = PrepareOverlapArray(overlaps, new int[] { colDimSize, colDimSize });

            SdrRepresentation.TraceInGraphFormat(new List<int[,]>() { twodOverlapArray },
                new int[] { 1, colDimSize * colDimSize },
                actColThreshold/* parameters.Get<int>(KEY.STIMULUS_THRESHOLD)*/, Color.Red, Color.Green,
                pngFileName: Path.Combine(outFolfer, $"Overlap_{vectorIndex}_Noise_{j}.png"));

            NeoCortexUtils.DrawBitmaps(arrays, $"Vector_{vectorIndex}_Noise_{j}_Repeat({repeat}).png", Color.Yellow, Color.Gray, OutImgSize, OutImgSize);

            #region Draw Heatmap

            var allColsPerm = sp.GetColumnPermenances();

            DrawPermanencesHeatmap(noiseLevel, vectorIndex, allColsPerm);

            //DrawPermanencesGraph(noiseLevel, colDimSize, vectorIndex, allColsPerm);

            #endregion
        }

        //private void DrawPermanencesGraph(int noiseLevel, int colDimSize, int vectorIndex, List<List<double>> allColsPerm)
        //{
        //    NOT IMPLEMENTED!!
        //    var arr = PreperePermArray(allColsPerm, colDimSize * colDimSize, 1024);
        //    SdrRepresentation.TraceInGraphFormat(new List<int[,]>() { arr },
        //      new int[] { colDimSize * colDimSize, 1024 },
        //      -1, Color.Green, Color.Green, width: 4096, height: 1300,
        //      pngFileName: Path.Combine(nameof(NoiseExperimentTest), $"Permanences_Inp_{vectorIndex}_Noise_{noiseLevel}.png"));
        //}

        private List<List<double>> DrawPermanencesHeatmap(int noiseLevel, int vectorIndex, List<List<double>> allColsPerm)
        {
            NeoCortexUtils.DrawHeatmaps(new List<double[,]>() { PrepareHeatmapArray(allColsPerm) },
                $"Heat_Vector_{vectorIndex}_Noise_{noiseLevel}.png", 1024, 4096, 100, 200, 300);

            return allColsPerm;
        }

        private double[,] PrepareHeatmapArray(List<List<double>> permanences)
        {
            double[,] arr = new double[permanences[0].Count, permanences.Count];

            for (int colIndex = 0; colIndex < permanences.Count; colIndex++)
            {
                for (int inpIndex = 0; inpIndex < permanences[colIndex].Count; inpIndex++)
                {
                    arr[inpIndex, colIndex] = 100 * permanences[colIndex][inpIndex];
                }
            }

            return arr;
        }

        private T[,] PrepareOverlapArray<T>(T[] overlaps, int[] colDims)
        {
            T[,] arr = new T[1, colDims[1] * colDims[0]];

            for (int colIndex = 0; colIndex < colDims[0] * colDims[1]; colIndex++)
            {
                arr[0, colIndex] = overlaps[colIndex];
            }

            return arr;
        }

        private int[,] PreperePermArray(List<List<double>> allColPerms, int cols, int inputs)
        {
            int offs = 3;
            int singlePermRange = 10;

            int[,] arr = new int[cols, inputs];

            for (int colIndex = 0; colIndex < 200 /*allColPerms.Count*/; colIndex++)
            {
                for (int inpIndex = 0; inpIndex < 1000; inpIndex++)
                {
                    //int scaledPermVal = (offs + inpIndex * singlePermRange) + (int)(100.00 * allColPerms[colIndex][inpIndex]);
                    int scaledPermVal = (offs + inpIndex * singlePermRange) + 0;
                    arr[inpIndex, colIndex] = 512;
                }
            }

            return arr;
        }

        /// <summary>
        ///  Prediction code.
        ///  This method takes a single sample of every input vector and adds
        ///  some noiseLevel to it. Then it predicts it.<param name="sp"></param>
        ///  Calculated hamming distance (percent permanences) between predicted output and output <param name="inputVectors"></param>
        ///  trained without noiseLevel is final result, which should be higher than 95% (realistic guess).
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

                    Debug.WriteLine($"Result for vector {vectorIndex} with noiseLevel {noise} - DistIn: {distIn} - DistOut: {distOut}");

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

        private static int[] getInputVector3()
        {
            int[] inputVector = new int[1024];

            for (int i = 0; i < 31; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (i > 14 && i < 18 && j > 20 && j < 24)
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
