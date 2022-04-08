// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using LearningFoundation;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MLPerceptron;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using NeuralNet.MLPerceptron;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;


namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerMnistTests
    {
        private const int OutImgSize = 1024;

        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSizes">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow("MnistPng28x28\\training", "3", new int[] { 28 }, new int[] { 32, 64, 128 }, PoolerMode.Multicore)]
        public void TrainSingleMnistImageTest(string trainingFolder, string digit, int[] imageSizes, int[] topologies, PoolerMode poolerMode)
        {
            string TestOutputFolder = $"Output-{nameof(TrainSingleMnistImageTest)}";

            var trainingImages = Directory.GetFiles(Path.Combine(trainingFolder, digit));

            //if (Directory.Exists(TestOutputFolder))
            //    Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

            // Topology loop
            for (int imSizeIndx = 0; imSizeIndx < imageSizes.Length; imSizeIndx++)
            {
                for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
                {
                    int counter = 0;
                    var numOfActCols = topologies[topologyIndx] * topologies[topologyIndx];
                    var parameters = GetDefaultParams();
                    //parameters.Set(KEY.DUTY_CYCLE_PERIOD, 20);
                    //parameters.Set(KEY.MAX_BOOST, 1);
                    //parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
                    //parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    //parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);
                    parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * 64 * 64/*imageSizes[imSizeIndx] * imageSizes[imSizeIndx]*/);
                    parameters.Set(KEY.POTENTIAL_RADIUS, imageSizes[imSizeIndx] * imageSizes[imSizeIndx]/*(int)0.5 * imageSizes[imSizeIndx]*/);
                    parameters.Set(KEY.POTENTIAL_PCT, 1.0);
                    parameters.Set(KEY.GLOBAL_INHIBITION, true);

                    parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);       //***
                    parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);   //***
                    parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);      //***

                    //parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);       //***
                    //parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.0);   //***
                    //parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.0);      //***

                    parameters.Set(KEY.INHIBITION_RADIUS, (int)0.025 * imageSizes[imSizeIndx] * imageSizes[imSizeIndx]);

                    parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);
                    parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
                    parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
                    parameters.Set(KEY.MAX_BOOST, 100);
                    parameters.Set(KEY.WRAP_AROUND, true);
                    parameters.Set(KEY.SEED, 1956);
                    parameters.setInputDimensions(new int[] { imageSizes[imSizeIndx], imageSizes[imSizeIndx] });
                    parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });

                    SpatialPooler sp = UnitTestHelpers.CreatePooler(poolerMode);

                    var mem = new Connections();

                    parameters.apply(mem);

                    UnitTestHelpers.InitPooler(poolerMode, sp, mem, parameters);

                    int actiColLen = numOfActCols;

                    int[] activeArray = new int[actiColLen];

                    string outFolder = $"{TestOutputFolder}\\{digit}\\{topologies[topologyIndx]}x{topologies[topologyIndx]}";

                    Directory.CreateDirectory(outFolder);

                    string outputHamDistFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_hamming.txt";

                    string outputActColFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_activeCol.txt";

                    using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
                    {
                        using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                        {
                            foreach (var mnistImage in trainingImages)
                            {
                                FileInfo fI = new FileInfo(mnistImage);

                                string outputImage = $"{outFolder}\\digit_{digit}_cycle_{counter}_{topologies[topologyIndx]}_{fI.Name}";

                                string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                                string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                                //Read input csv file into array
                                int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                                int numIterationsPerImage = 5;
                                int[] oldArray = new int[activeArray.Length];
                                List<double[,]> overlapArrays = new List<double[,]>();
                                List<double[,]> bostArrays = new List<double[,]>();

                                for (int k = 0; k < numIterationsPerImage; k++)
                                {
                                    sp.compute(inputVector, activeArray, true);

                                    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                    var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                                    swHam.WriteLine($"{counter++}|{distance} ");

                                    oldArray = new int[actiColLen];
                                    activeArray.CopyTo(oldArray, 0);

                                    //var mem = sp.GetMemory(layer);
                                    overlapArrays.Add(ArrayUtils.Make2DArray<double>(ArrayUtils.ToDoubleArray(mem.Overlaps), topologies[topologyIndx], topologies[topologyIndx]));
                                    bostArrays.Add(ArrayUtils.Make2DArray<double>(mem.BoostedOverlaps, topologies[topologyIndx], topologies[topologyIndx]));
                                }

                                var activeStr = Helpers.StringifyVector(activeArray);
                                swActCol.WriteLine("Active Array: " + activeStr);

                                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, topologies[topologyIndx], topologies[topologyIndx]);
                                twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
                                List<int[,]> arrays = new List<int[,]>();
                                arrays.Add(twoDimenArray);
                                arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length))));

                                NeoCortexUtils.DrawBitmaps(arrays, outputImage, Color.Yellow, Color.Gray, OutImgSize, OutImgSize);
                                NeoCortexUtils.DrawHeatmaps(overlapArrays, $"{outputImage}_overlap.png", 1024, 1024, 150, 50, 5);
                                NeoCortexUtils.DrawHeatmaps(bostArrays, $"{outputImage}_boost.png", 1024, 1024, 150, 50, 5);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This test does spatial pooling training of set of specified images with variable radius.
        /// Radius is incremented from 10 to 100% with step of 10%.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSizes">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow("MnistPng28x28_smallerdataset\\training", "5", new int[] { 28 }, new int[] { 32 /*, 64, 128 */})]
        public void TrainSingleMnistImageWithVariableRadiusTest(string trainingFolder, string digit, int[] imageSizes, int[] topologies)
        {
            string testOutputFolder = $"Output-{nameof(TrainSingleMnistImageWithVariableRadiusTest)}";

            var trainingImages = Directory.GetFiles(Path.Combine(trainingFolder, digit));

            Directory.CreateDirectory($"{testOutputFolder}\\{digit}");

            if (Directory.Exists(testOutputFolder))
                Directory.Delete(testOutputFolder, true);

            Directory.CreateDirectory(testOutputFolder);

            Directory.CreateDirectory($"{testOutputFolder}\\{digit}");

            for (int test = 0; test <= 10; test++)
            {
                double radius = (double)test * 0.1;

                // Topology loop
                for (int imSizeIndx = 0; imSizeIndx < imageSizes.Length; imSizeIndx++)
                {
                    for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
                    {
                        int counter = 0;
                        var numOfCols = topologies[topologyIndx] * topologies[topologyIndx];
                        var parameters = GetDefaultParams();

                        parameters.setInputDimensions(new int[] { imageSizes[imSizeIndx], imageSizes[imSizeIndx] });
                        parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                        parameters.setNumActiveColumnsPerInhArea(0.1 * numOfCols);
                        parameters.Set(KEY.POTENTIAL_RADIUS, (int)(radius * imageSizes[imSizeIndx]));
                        parameters.Set(KEY.POTENTIAL_PCT, 1.0);
                        parameters.Set(KEY.GLOBAL_INHIBITION, false);
                        parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
                        parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * imageSizes[imSizeIndx]);
                        parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
                        parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * numOfCols);
                        parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
                        parameters.Set(KEY.MAX_BOOST, 5);

                        var sp = new SpatialPoolerMT();
                        var mem = new Connections();

                        parameters.apply(mem);

                        sp.Init(mem);

                        int actiColLen = numOfCols;

                        int[] activeArray = new int[actiColLen];

                        string outFolder = $"{testOutputFolder}\\{digit}\\{topologies[topologyIndx]}x{topologies[topologyIndx]}";

                        Directory.CreateDirectory(outFolder);

                        string outputHamDistFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_hamming.txt";

                        string outputActColFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_activeCol.txt";

                        using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
                        {
                            using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                            {
                                foreach (var mnistImage in trainingImages)
                                {
                                    FileInfo fI = new FileInfo(mnistImage);

                                    string outputImage = $"{outFolder}\\digit_{digit}_cycle_{counter}_topology_boost_5_radius_{radius}_{topologies[topologyIndx]}_{fI.Name}";

                                    string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                                    string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                                    //Read input csv file into array
                                    int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                                    int numIterationsPerImage = 5;
                                    int[] oldArray = new int[activeArray.Length];

                                    for (int k = 0; k < numIterationsPerImage; k++)
                                    {
                                        sp.compute(inputVector, activeArray, true);

                                        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                        var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                                        swHam.WriteLine($"{counter++}|{distance} ");

                                        oldArray = new int[actiColLen];
                                        activeArray.CopyTo(oldArray, 0);
                                    }

                                    var activeStr = Helpers.StringifyVector(activeArray);
                                    swActCol.WriteLine("Active Array: " + activeStr);

                                    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, topologies[topologyIndx], topologies[topologyIndx]);
                                    twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                                    int[,] twoDimImgArray = ArrayUtils.Make2DArray<int>(inputVector, imageSizes[imSizeIndx], imageSizes[imSizeIndx]);
                                    twoDimImgArray = ArrayUtils.Transpose(twoDimImgArray);

                                    var arr = new List<int[,]> { twoDimImgArray, twoDimenArray };

                                    NeoCortexUtils.DrawBitmaps(arr, outputImage, Color.Gray, Color.Yellow,
                                        OutImgSize, OutImgSize);

                                    break;
                                }
                            }
                        }
                    }
                }

                //calcCrossOverlapsPerDigit(testOutputFolder, fileActCols);
                //calcCrossOverlapsBetweenImages(testOutputFolder, fileActCols);
            }
        }

        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSizes">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        /// <param name="maxNumOfTrainingImages">-1 to train all available images. Positive number specifies number of training images to be included
        /// in the training process. I.E. If you want to train 10 images only, set this parameter to 10.</param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" },
            new int[] { 28 }, new int[] { /*32,*/ 64 /*, 128 */}, 10)]
        public void TrainMultilevelImageTest(string trainingFolder, string testingFolder, string[] digits, int[] imageSizes,
            int[] topologies, int maxNumOfTrainingImages = 10)
        {
            List<string> resultFiles = new List<string>();

            //int[] poolerTopology = new int[] { 28, 16 };

            // Index of layer, which is used for supervised prediction.
            // This test can create more layers with the goal to analyse result.
            // However sparse representation of specific layer can be used for supervised
            // prediction.
            int targetLyrIndx = 0;

            string testOutputFolder = $"Output-{nameof(TrainMultilevelImageTest)}";
            if (Directory.Exists(testOutputFolder))
                Directory.Delete(testOutputFolder, true);

            Directory.CreateDirectory(testOutputFolder);

            // Topology loop
            for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
            {
                for (int imSizeIndx = 0; imSizeIndx < imageSizes.Length; imSizeIndx++)
                {
                    //var numOfCols = topologies[topologyIndx] * topologies[topologyIndx];
                    var parameters = GetDefaultParams();
                    parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
                    parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * 64 * 64/*imageSizes[imSizeIndx] * imageSizes[imSizeIndx]*/);
                    parameters.Set(KEY.POTENTIAL_RADIUS, imageSizes[imSizeIndx] * imageSizes[imSizeIndx]/*(int)0.5 * imageSizes[imSizeIndx]*/);
                    parameters.Set(KEY.POTENTIAL_PCT, 1.0);
                    parameters.Set(KEY.GLOBAL_INHIBITION, true);
                    parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
                    parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0);
                    parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0);

                    //parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);       //***
                    //parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);   //***
                    //parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);      //***

                    parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);
                    parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
                    parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
                    parameters.Set(KEY.MAX_BOOST, 1);
                    parameters.Set(KEY.WRAP_AROUND, true);
                    parameters.Set(KEY.SEED, 1956);
                    parameters.setInputDimensions(new int[] { 28 * 28 });
                    parameters.setColumnDimensions(new int[] { 64 * 64 });

                    //parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * imageSizes[imSizeIndx]);
                    //parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
                    //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * 64 *64/*imageSizes[imSizeIndx] * imageSizes[imSizeIndx]*/);
                    //parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10000000);
                    //parameters.Set(KEY.MAX_BOOST, 2);
                    //parameters.setInputDimensions(new int[] { 28*28 });
                    //parameters.setColumnDimensions(new int[] { 64*64 });

                    var sp = new HtmModuleNet(new Parameters[] { parameters });

                    List<string> trainingFiles = new List<string>();

                    // Active columns of every specific file.
                    Dictionary<string, Dictionary<string, int[]>> fileActCols = new Dictionary<string, Dictionary<string, int[]>>();

                    foreach (var digit in digits)
                    {
                        // Contains cross overlaps between oll samples of a single digit.
                        //int[,] overlapMatrix = new float[trainingFiles.Count, trainingFiles.Count];

                        string digitFolder = Path.Combine(trainingFolder, digit);

                        if (!Directory.Exists(digitFolder))
                            continue;

                        var trainingImages = Directory.GetFiles(digitFolder);

                        Directory.CreateDirectory($"{testOutputFolder}\\{digit}");

                        int counter = 0;

                        //int actiColLen = numOfCols;

                        string outFolder = $"{testOutputFolder}\\{digit}\\{topologies[topologyIndx]}x{topologies[topologyIndx]}";

                        Directory.CreateDirectory(outFolder);

                        string outputHamDistFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_hamming.txt";

                        string outputActColFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_activeCol.txt";

                        trainingFiles.Add(outputActColFile);

                        using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
                        {
                            using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                            {
                                int trainedImages = 0;

                                foreach (var mnistImage in trainingImages)
                                {
                                    if (maxNumOfTrainingImages > 0 && trainedImages++ > maxNumOfTrainingImages)
                                        break;

                                    FileInfo fI = new FileInfo(mnistImage);

                                    string outputImage = $"{outFolder}\\digit_{digit}_cycle_{counter}_{topologies[topologyIndx]}_{fI.Name}";

                                    string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                                    string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                                    //Read input csv file into array
                                    int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                                    int numIterationsPerImage = 10;
                                    int[] oldArray = new int[sp.GetActiveColumns(targetLyrIndx).Length];

                                    var activeArray = sp.GetActiveColumns(targetLyrIndx);

                                    for (int k = 0; k < numIterationsPerImage; k++)
                                    {
                                        sp.Compute(inputVector, true);

                                        var distance = MathHelpers.GetHammingDistance(oldArray, sp.GetActiveColumns(targetLyrIndx));
                                        swHam.WriteLine($"{counter++}|{distance} ");

                                        oldArray = new int[sp.GetActiveColumns(targetLyrIndx).Length];
                                        sp.GetActiveColumns(targetLyrIndx).CopyTo(oldArray, 0);
                                    }

                                    //
                                    // Copy result of the file to list of all results.
                                    var activeColumns = sp.GetActiveColumns(targetLyrIndx);
                                    var copyResult = new int[activeColumns.Length];
                                    activeColumns.CopyTo(copyResult, 0);
                                    fileActCols.TryAdd(digit, new Dictionary<string, int[]>());
                                    fileActCols[digit].Add(fI.Name, copyResult);

                                    Debug.WriteLine(copyResult.IndexWhere(x => x == 1));

                                    var activeStr = Helpers.StringifyVector(activeArray);
                                    swActCol.WriteLine($"{digit}, " + activeStr);

                                    List<int[,]> bmpArrays = new List<int[,]>();
                                    List<double[,]> overlapArrays = new List<double[,]>();
                                    List<double[,]> bostArrays = new List<double[,]>();

                                    for (int layer = 0; layer < sp.Layers; layer++)
                                    {
                                        int size = (int)Math.Sqrt(sp.GetActiveColumns(layer).Length);
                                        int[,] arr = ArrayUtils.Make2DArray<int>(sp.GetActiveColumns(layer), size, size);
                                        arr = ArrayUtils.Transpose(arr);
                                        bmpArrays.Add(arr);

                                        var mem = sp.GetMemory(layer);
                                        overlapArrays.Add(ArrayUtils.Make2DArray<double>(ArrayUtils.ToDoubleArray(mem.Overlaps), size, size));
                                        bostArrays.Add(ArrayUtils.Make2DArray<double>(mem.BoostedOverlaps, size, size));
                                    }

                                    int[,] twoDimInputArray = ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length));
                                    twoDimInputArray = ArrayUtils.Transpose(twoDimInputArray);

                                    bmpArrays.Add(twoDimInputArray);

                                    NeoCortexUtils.DrawBitmaps(bmpArrays, outputImage, OutImgSize, OutImgSize);

                                    //NeoCortexUtils.DrawHeatmaps(bostArrays, outputImage + ".boost.png", OutImgSize, OutImgSize, 200, 50, 10);
                                    //NeoCortexUtils.DrawHeatmaps(overlapArrays, outputImage + ".overlapp.png", OutImgSize, OutImgSize);
                                }
                            }
                        }

                        // This part of code is doing prediction.

                        //foreach (var mnistImage in trainingImages)
                        //{
                        //    FileInfo fI = new FileInfo(mnistImage);

                        //    string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSize[imSizeIndx]}";

                        //    string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imageSize[imSizeIndx], testName);

                        //    int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                        //    sp.Compute(mem, inputVector, true);

                        //    List<int[,]> bmpArrays = new List<int[,]>();

                        //    for (int layer = 0; layer < sp.Layers; layer++)
                        //    {
                        //        int[,] arr = ArrayUtils.Make2DArray<int>(sp.GetActiveColumns(layer), (int)Math.Sqrt(sp.GetActiveColumns(layer).Length), (int)Math.Sqrt(sp.GetActiveColumns(layer).Length));
                        //        arr = ArrayUtils.Transpose(arr);
                        //        bmpArrays.Add(arr);
                        //    }

                        //    int[,] twoDimInputArray = ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length));
                        //    twoDimInputArray = ArrayUtils.Transpose(twoDimInputArray);

                        //    bmpArrays.Add(twoDimInputArray);

                        //    string outputImage = $"{outFolder}\\digit_{digit}_PREDICT_{topologies[topologyIndx]}_{fI.Name}";

                        //    NeoCortexUtils.DrawBitmaps(bmpArrays, outputImage, OutImgSize, OutImgSize);
                        //}

                    }

                    // Use LearningAPI
                    //var api = DoSupervisedLearning(1000, 0.1, 25, new int[] { 3 }, 1, trainingFiles);
                    //PredictSupervizedLearning(digits, testingFolder, sp, imageSizes[imSizeIndx], mem, targetLyrIndx, api);

                    string sdrResults = MergeFiles(testOutputFolder, trainingFiles);

                    //DoSupervisedLearningWithMLNet(topologies[topologyIndx], sdrResults);
                    calcCrossOverlapsPerDigit(testOutputFolder, fileActCols);
                    calcCrossOverlapsBetweenImages(testOutputFolder, fileActCols);
                    ValidateResults(testOutputFolder, sp, testingFolder, imageSizes[imSizeIndx], digits, targetLyrIndx, fileActCols);
                }
            }
        }


        /// <summary>
        /// It traverses through all test files, digit by digit. 
        /// Prediction result (set of active columns) is compared with results collected during training process of SP.
        /// Resulting file contains all compares and highited best match.
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <param name="mem"></param>
        /// <param name="trainedSpatialPooler"></param>
        /// <param name="testingFolder"></param>
        /// <param name="imgSize"></param>
        /// <param name="digits"></param>
        /// <param name="targetLyrIndx"></param>
        /// <param name="results"></param>
        private void ValidateResults(string outputFolder, HtmModuleNet trainedSpatialPooler,
            string testingFolder, int imgSize, string[] digits, int targetLyrIndx,
            Dictionary<string, Dictionary<string, int[]>> results)
        {
            using (StreamWriter predictionsWriter = new StreamWriter($"{outputFolder}/sparse-predictions.csv"))
            {
                // This is where we write result file, and does prediction.
                using (StreamWriter sw = new StreamWriter($"{outputFolder}/validation-results.csv"))
                {
                    foreach (var digit in digits)
                    {
                        sw.WriteLine($"-------- Trying test images for digit {digit} -----------");
                        Debug.WriteLine($"-------- Trying test images for digit {digit} -----------");

                        var tstImgsForDigit = Directory.GetFiles(Path.Combine(testingFolder, digit));

                        foreach (var testImage in tstImgsForDigit)
                        {
                            FileInfo fI = new FileInfo(testImage);

                            string testName = $"{outputFolder}\\digit_{digit}_{fI.Name}_{imgSize}";

                            string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{testImage}", imgSize, testName);

                            // Read input csv file into array
                            int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                            trainedSpatialPooler.Compute(inputVector, false);

                            // Write out predicted columns.
                            var activeArray = trainedSpatialPooler.GetActiveColumns(targetLyrIndx);
                            var activeStr = Helpers.StringifyVector(activeArray);
                            predictionsWriter.WriteLine($"{digit}, " + activeStr);

                            var bestMatch = lookupBestMatch(results, activeArray, sw);

                            sw.WriteLine($"Test image {testImage} matches to digit '{bestMatch.winnerDigit}' with hamDist = {bestMatch.maxHammingDistane}");
                            sw.WriteLine();
                            Debug.WriteLine($"Test image {testImage} matches to digit '{bestMatch.winnerDigit}' with hamDist = {bestMatch.maxHammingDistane}");
                        }
                    }
                }
            }
        }


        private static (double maxHammingDistane, string winnerDigit) lookupBestMatch(Dictionary<string, Dictionary<string, int[]>> results, int[] activeArray, StreamWriter sw)
        {
            string winnerDigit = String.Empty;
            double maxHamDist = 0.0;

            foreach (var digitResults in results)
            {
                foreach (var digitRes in digitResults.Value.Values.ToArray())
                {
                    var distance = MathHelpers.GetHammingDistance(digitRes, activeArray);
                    sw.WriteLine(distance);
                    if (maxHamDist < distance)
                    {
                        maxHamDist = distance;
                        winnerDigit = digitResults.Key;
                    }
                }
            }

            return (maxHammingDistane: maxHamDist, winnerDigit: winnerDigit);
        }

        private void calcCrossOverlapsPerDigit(string outputFolder, Dictionary<string, Dictionary<string, int[]>> results)
        {
            using (StreamWriter sw = new StreamWriter($"{outputFolder}/cross-overlapps-perimage.csv"))
            {
                foreach (var digit in results.Keys)
                {
                    var arr = results[digit].Values.ToArray();

                    sw.Write($"{digit}, ");

                    double min = int.MaxValue, max = 0, sum = 0, cnt = 0;

                    for (int i = 0; i < arr.Length; i++)
                    {
                        for (int j = i + 1; j < arr.Length; j++)
                        {
                            Debug.WriteLine(Helpers.StringifyVector(ArrayUtils.IndexWhere(arr[i], x => x == 1)));
                            Debug.WriteLine(Helpers.StringifyVector(ArrayUtils.IndexWhere(arr[j], x => x == 1)));
                            Debug.WriteLine("");

                            var distance = MathHelpers.GetHammingDistance(arr[i], arr[j]);
                            sw.WriteLine($"{distance}, ");

                            cnt++;
                            sum += distance;

                            if (distance < min)
                                min = distance;

                            if (distance > max)
                                max = distance;
                        }
                    }

                    sw.WriteLine("----------------------");
                    sw.WriteLine($"Digit: {digit}, Avg:{sum / cnt}, Min:{min}, Max:{max}, Compares:{cnt}");
                }
            }
        }

        private void calcCrossOverlapsBetweenImages(string outputFolder, Dictionary<string, Dictionary<string, int[]>> fileActCols)
        {
            List<int[]> digitResults = new List<int[]>();
            foreach (var digit in fileActCols.Keys)
            {
                digitResults.Add(fileActCols[digit].Values.Last());
            }

            double min = int.MaxValue, max = 0, sum = 0, cnt = 0;

            using (StreamWriter sw = new StreamWriter($"{outputFolder}/cross-overlapps-betweendigits.csv"))
            {
                for (int i = 0; i < digitResults.Count; i++)
                {
                    for (int j = i; j < digitResults.Count; j++)
                    {
                        var distance = MathHelpers.GetHammingDistance(digitResults[i], digitResults[j]);
                        sw.WriteLine($"{i} vs. {j} = {distance}, ");

                        cnt++;
                        sum += distance;

                        if (distance < min)
                            min = distance;

                        if (distance > max)
                            max = distance;
                    }
                }

                sw.WriteLine("----------------------");
                sw.WriteLine($"Avg:{sum / cnt}, Min:{min}, Max:{max}, Compares:{cnt}");
            }
        }


        [TestMethod]
        [TestCategory("LongRunning")]
        //[DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" },
        //  new int[] { 28 }, new int[] { /*32,*/ 64 /*, 128 */})]
        [DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "2" },
            new int[] { 28 }, new int[] { 32 })]
        //[DataRow("MnistPng28x28\\training", new string[] { "x", },
        //    new int[] { 28 }, new int[] { 64, /*64, 128 */})]
        //[DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "y", },
        //    new int[] { 28 }, new int[] { 128 })]
        public void GenerateSparsityImageTest(string trainingFolder, string testingFolder, string[] digits,
            int[] imageSizes, int[] topologies)
        {
            List<string> resultFiles = new List<string>();

            int[] poolerTopology = new int[] { 28, 20, 8, 4 };

            // Index of layer, which is used for supervised prediction.
            // This test can create more layers with the goal to analyse result.
            // However sparse representation of specific layer can be used for supervised
            // prediction.
            //int targetLyrIndx = 2;

            string TestOutputFolder = $"Output-{nameof(GenerateSparsityImageTest)}";
            //if (Directory.Exists(TestOutputFolder))
            //    Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            // Topology loop
            for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
            {
                for (int imSizeIndx = 0; imSizeIndx < imageSizes.Length; imSizeIndx++)
                {
                    var numOfCols = topologies[topologyIndx] * topologies[topologyIndx];
                    //var parameters = GetDefaultParams();
                    //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * numOfCols);
                    //parameters.Set(KEY.POTENTIAL_RADIUS, imageSizes[imSizeIndx]* imageSizes[imSizeIndx]);
                    //parameters.Set(KEY.POTENTIAL_PCT, 1.0);
                    //parameters.Set(KEY.GLOBAL_INHIBITION, true);

                    //parameters.Set(KEY.INHIBITION_RADIUS, (int)0.025 * imageSizes[imSizeIndx] * imageSizes[imSizeIndx]);

                    //parameters.Set(KEY.STIMULUS_THRESHOLD, 50);
                    //parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);   
                    //parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
                    ////parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);

                    //parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);
                    //parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
                    //parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
                    //parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
                    //parameters.Set(KEY.MAX_BOOST, 100);
                    //parameters.Set(KEY.WRAP_AROUND, true);
                    //parameters.Set(KEY.SEED, 1956);

                    //parameters.setInputDimensions(new int[] { imageSizes[imSizeIndx], imageSizes[imSizeIndx] });
                    //parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    //parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);

                    var parameters = GetDefaultParams();
                    //parameters.Set(KEY.DUTY_CYCLE_PERIOD, 20);
                    //parameters.Set(KEY.MAX_BOOST, 1);
                    //parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
                    //parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    //parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);
                    parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * numOfCols);
                    parameters.Set(KEY.POTENTIAL_RADIUS, imageSizes[imSizeIndx] * imageSizes[imSizeIndx]/*(int)0.5 * imageSizes[imSizeIndx]*/);
                    parameters.Set(KEY.POTENTIAL_PCT, 1.0);
                    parameters.Set(KEY.GLOBAL_INHIBITION, true);

                    parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);       //***
                    parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);   //***
                    parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);      //***

                    parameters.Set(KEY.INHIBITION_RADIUS, (int)0.025 * imageSizes[imSizeIndx] * imageSizes[imSizeIndx]);

                    parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);
                    parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
                    parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
                    parameters.Set(KEY.MAX_BOOST, 100);
                    parameters.Set(KEY.WRAP_AROUND, true);
                    parameters.Set(KEY.SEED, 1956);
                    parameters.setInputDimensions(new int[] { imageSizes[imSizeIndx], imageSizes[imSizeIndx] });
                    parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });

                    List<string> trainingFiles = new List<string>();

                    foreach (var digit in digits)
                    {
                        string digitFolder = Path.Combine(trainingFolder, digit);

                        if (!Directory.Exists(digitFolder))
                            continue;

                        var trainingImages = Directory.GetFiles(digitFolder);

                        Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

                        string outFolder = $"{TestOutputFolder}\\{digit}\\{topologies[topologyIndx]}x{topologies[topologyIndx]}";

                        Directory.CreateDirectory(outFolder);

                        string outputHamDistFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_hamming.txt";

                        string outputActColFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_activeCol.txt";

                        trainingFiles.Add(outputActColFile);

                        // using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                        //{
                        int imgCnt = 0;

                        foreach (var mnistImage in trainingImages)
                        {
                            var mem = new Connections();

                            parameters.apply(mem);

                            var sp = new SpatialPoolerMT();
                            sp.Init(mem);

                            int[] activeArray = new int[topologies[topologyIndx] * topologies[topologyIndx]];

                            if (imgCnt > 10) break;

                            imgCnt++;

                            FileInfo fI = new FileInfo(mnistImage);

                            string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                            string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                            //Read input csv file into array
                            int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                            int numIterationsPerImage = 5;

                            int[] oldArray = new int[activeArray.Length];

                            Debug.WriteLine($"\r\n{digit}");

                            for (int k = 0; k < numIterationsPerImage; k++)
                            {
                                sp.compute(inputVector, activeArray, true);

                                var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);

                                Debug.WriteLine(distance);

                                oldArray = new int[activeArray.Length];

                                activeArray.CopyTo(oldArray, 0);

                                int[,] twoDimInputArray = ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length));
                                twoDimInputArray = ArrayUtils.Transpose(twoDimInputArray);

                                int[,] winArray = ArrayUtils.Make2DArray<int>(activeArray, (int)Math.Sqrt(activeArray.Length), (int)Math.Sqrt(activeArray.Length));
                                winArray = ArrayUtils.Transpose(winArray);

                                string outputImage = $"{outFolder}\\digit_{digit}_img_{imgCnt}_epoch_{numIterationsPerImage}_{topologies[topologyIndx]}_{fI.Name}";

                                NeoCortexUtils.DrawBitmaps(new List<int[,]> { twoDimInputArray, winArray }, outputImage, OutImgSize, OutImgSize);
                            }

                            //var activeStr = Helpers.StringifyVector(activeArray);
                            //swActCol.WriteLine($"{digit}, " + activeStr);

                            //List<int[,]> bmpArrays = new List<int[,]>();

                            //for (int layer = 0; layer < sp.Layers; layer++)
                            //{
                            //    int[,] arr = ArrayUtils.Make2DArray<int>(sp.GetActiveColumns(layer), (int)Math.Sqrt(sp.GetActiveColumns(layer).Length), (int)Math.Sqrt(sp.GetActiveColumns(layer).Length));
                            //    arr = ArrayUtils.Transpose(arr);
                            //    bmpArrays.Add(arr);
                            //}

                            //int[,] twoDimInputArray = ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length));
                            //twoDimInputArray = ArrayUtils.Transpose(twoDimInputArray);

                            //bmpArrays.Add(twoDimInputArray);

                            //NeoCortexUtils.DrawBitmaps(bmpArrays, outputImage, OutImgSize, OutImgSize);
                        }
                        //}
                    }

                }
            }
        }


        /// <summary>
        /// Merge all SDR results of all digits into a single SDR resulting file.
        /// </summary>
        /// <param name="trainingFiles"></param>
        private static string MergeFiles(string outputPath, List<string> trainingFiles)
        {
            string sparseFileName = $"{outputPath}\\sparse-results.csv";
            if (File.Exists(sparseFileName))
                File.Delete(sparseFileName);

            using (var writer = new StreamWriter(sparseFileName, append: false))
            {
                foreach (var file in trainingFiles)
                {
                    using (var reader = new StreamReader(file))
                    {
                        writer.Write(reader.ReadToEnd());
                    }
                }
            }

            return sparseFileName;
        }

        private void DoSupervisedLearningWithMLNet(int numSparseBits, string trainDataPath)
        {
            trainDataPath = "sparse-results.csv";

            MLContext mLContext = new MLContext();

            var trainData = mLContext.Data.LoadFromTextFile(path: trainDataPath,
                     columns: new[]
                     {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Double, 1, numSparseBits),
                            new TextLoader.Column("Number", DataKind.Int16, 0)
                     },
                     hasHeader: false,
                     separatorChar: ','
                     );


            var testData = mLContext.Data.LoadFromTextFile(path: trainDataPath,
                    columns: new[]
                    {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Double, 1, numSparseBits),
                            new TextLoader.Column("Number", DataKind.Int16, 0)
                    },
                    hasHeader: false,
                    separatorChar: ','
                    );

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mLContext.Transforms.Concatenate(DefaultColumnNames.Features, nameof(InputData.PixelValues)).AppendCacheCheckpoint(mLContext);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder
            var trainer = mLContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumnName: "Number", featureColumnName: DefaultColumnNames.Features);
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("=============== Training the model ===============");

            ITransformer trainedModel = trainingPipeline.Fit(trainData);
            long elapsedMs = watch.ElapsedMilliseconds;
            Debug.WriteLine($"***** Training time: {elapsedMs / 1000} seconds *****");

            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testData);
            var metrics = mLContext.MulticlassClassification.Evaluate(data: predictions, label: "Number", score: DefaultColumnNames.Score);

            PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

            using (var fs = new FileStream("ml-model", FileMode.Create, FileAccess.Write, FileShare.Write))
                mLContext.Model.Save(trainedModel, fs);

        }

        private static void PrintMultiClassClassificationMetrics(string name, MultiClassClassifierMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*    Metrics for {name} multi-class classification model   ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better");
            Console.WriteLine($"************************************************************");
        }

        private void PredictSupervizedLearning(string[] digits, string testingFolder, HtmModuleNet sp, int imgSize, Connections mem, int targetLyrIndx, LearningApi learningPpi)
        {
            foreach (var digit in digits)
            {
                List<string> trainingFiles = new List<string>();

                string digitFolder = Path.Combine(testingFolder, digit);

                if (!Directory.Exists(digitFolder))
                    continue;

                var testingImages = Directory.GetFiles(digitFolder);

                foreach (var mnistImage in testingImages)
                {
                    string testName = $"PREDICT_digit_{digit}_{new FileInfo(mnistImage).Name}_{imgSize}";

                    string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{mnistImage}", imgSize, testName);

                    //Read input csv file into array
                    int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                    sp.Compute(inputVector, false);

                    var activeColumns = sp.GetActiveColumns(targetLyrIndx);

                    double[] sdrOut = new double[activeColumns.Length + 10];

                    activeColumns.CopyTo(sdrOut, 0);

                    MLPerceptronResult res = learningPpi.Algorithm.Predict(new double[][] { sdrOut }, learningPpi.Context) as MLPerceptronResult;

                    var predictedDigitResult = res.results[int.Parse(digit)];

                    string marker = predictedDigitResult > 0.90 ? "OK" : "ERR";
                    Debug.WriteLine($"{digit} - res={predictedDigitResult} - {marker}");
                }
            }
        }


        private LearningApi DoSupervisedLearning(int iterations, double learningrate, int batchSize,
            int[] hiddenLayerNeurons, int iterationnumber, List<string> trainingFiles)
        {
            LearningApi api = new LearningApi();

            api.UseActionModule<object, double[][]>((notUsed, ctx) =>
            {
                List<double[]> rows = new List<double[]>();

                ctx.DataDescriptor = new LearningFoundation.DataDescriptor();

                //var trainingFiles = Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\MLPerceptron\\TestFiles\\Sdr");
                int rowCnt = 0;
                foreach (var file in trainingFiles)
                {
                    using (var reader = new StreamReader(file))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var tokens = line.Split(",");
                            List<string> newTokens = new List<string>();
                            foreach (var token in tokens)
                            {
                                if (token != " ")
                                    newTokens.Add(token);
                            }

                            tokens = newTokens.ToArray();

                            if (rowCnt == 0)
                            {
                                ctx.DataDescriptor.Features = new LearningFoundation.DataMappers.Column[tokens.Length - 1];
                                for (int i = 1; i < tokens.Length; i++)
                                {
                                    ctx.DataDescriptor.Features[i - 1] = new LearningFoundation.DataMappers.Column
                                    {
                                        Id = i,
                                        Index = i,
                                        Type = LearningFoundation.DataMappers.ColumnType.BINARY
                                    };
                                }
                                ctx.DataDescriptor.LabelIndex = -1;
                            }

                            // We have 65 features and digit number in file. to encode digits 0-9. 
                            // Digits can be represented as 9 bits.
                            double[] row = new double[tokens.Length - 1 + 10];
                            for (int i = 0; i < tokens.Length; i++)
                            {
                                row[i] = double.Parse(tokens[i], CultureInfo.InvariantCulture);
                            }

                            //
                            // This code encodes 9 digit classes as last 9 bits of training vector.
                            for (int k = 0; k < 10; k++)
                            {
                                if (double.Parse(tokens[0], CultureInfo.InvariantCulture) == k)
                                    row[tokens.Length - 1 + k] = 1;
                                else
                                    row[tokens.Length - 1 + k] = 0;
                            }

                            rows.Add(row);
                        }
                    }

                    rowCnt++;
                }

                return rows.ToArray();
            });

            //int[] hiddenLayerNeurons = { 6 };
            // Invoke the MLPerecptronAlgorithm with a specific learning rate, number of iterations
            api.UseMLPerceptron(learningrate, iterations, batchSize, iterationnumber, hiddenLayerNeurons);

            MLPerceptronAlgorithmScore score = api.Run() as MLPerceptronAlgorithmScore;

            api.Save("SdrMnistModel");

            return api;
        }


        #region Private Helpers

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
            //parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);
            //int r = parameters.Get<int>(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA);

            /*
            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 16);
            parameters.Set(KEY.POTENTIAL_PCT, 0.85);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);
            //int r = parameters.Get<int>(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA);
            */
            return parameters;
        }

        #endregion

    }

    class InputData
    {
        [ColumnName("PixelValues")]
        [VectorType(64)]
        public Boolean[] PixelValues;
    }
}
