using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ImageBinarizer;
using System.Drawing;
using NeoCortex;
using NeoCortexApi.Network;
using LearningFoundation;
using System.Globalization;
using MLPerceptron;
using System.Linq;
using NeuralNet.MLPerceptron;
using Microsoft.ML;
using Microsoft.ML.Data;


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
        /// <param name="imageSize">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [DataRow("MnistPng28x28\\training", "3", new int[] { 28 }, new int[] { 32, 64, 128 })]
        public void TrainSingleMnistImageTest(string trainingFolder, string digit, int[] imageSize, int[] topologies)
        {
            const string TestOutputFolder = "Output";

            var trainingImages = Directory.GetFiles(Path.Combine(trainingFolder, digit));

            if (Directory.Exists(TestOutputFolder))
                Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

            // Topology loop
            for (int imSizeIndx = 0; imSizeIndx < imageSize.Length; imSizeIndx++)
            {
                for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
                {
                    int counter = 0;
                    var numOfActCols = topologies[topologyIndx] * topologies[topologyIndx];
                    var parameters = GetDefaultParams();
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10000);
                    parameters.Set(KEY.MAX_BOOST, 5);
                    parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
                    parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);

                    var sp = new SpatialPooler();
                    var mem = new Connections();

                    parameters.apply(mem);

                    sp.init(mem);

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

                                string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSize[imSizeIndx]}";

                                string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imageSize[imSizeIndx], testName);

                                //Read input csv file into array
                                int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                                int numIterationsPerImage = 5;
                                int[] oldArray = new int[activeArray.Length];

                                for (int k = 0; k < numIterationsPerImage; k++)
                                {
                                    sp.compute(mem, inputVector, activeArray, true);

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

                                NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, outputImage);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This test does spatial pooling training of set of specifies images with variable radius.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSizes">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [DataRow("MnistPng28x28\\training", "3", new int[] { 28 }, new int[] { 32 /*, 64, 128 */})]
        public void TrainSingleMnistImageWithVariableRadiusTest(string trainingFolder, string digit, int[] imageSizes, int[] topologies)
        {
            string TestOutputFolder = $"Output-{nameof(TrainSingleMnistImageWithVariableRadiusTest)}";

            var trainingImages = Directory.GetFiles(Path.Combine(trainingFolder, digit));

            if (Directory.Exists(TestOutputFolder))
                Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

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

                        var sp = new SpatialPooler();
                        var mem = new Connections();

                        parameters.apply(mem);

                        sp.init(mem);

                        int actiColLen = numOfCols;

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

                                    string outputImage = $"{outFolder}\\digit_{digit}_cycle_{counter}_topology_boost_5_radius_{radius}_{topologies[topologyIndx]}_{fI.Name}";

                                    string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                                    string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                                    //Read input csv file into array
                                    int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                                    int numIterationsPerImage = 5;
                                    int[] oldArray = new int[activeArray.Length];

                                    for (int k = 0; k < numIterationsPerImage; k++)
                                    {
                                        sp.compute(mem, inputVector, activeArray, true);

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
            }
        }

        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSizes">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        //[DataRow("MnistPng28x28\\training", "7", new int[] { 28 }, new int[] { 32, /*64, 128 */ })]
        //[DataRow("MnistPng28x28\\training", new string[] {"0", "1", "2", "3", "4", "5", "6", "7"},
        //    new int[] { 28 }, new int[] { 32, /*64, 128 */})]
        [DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" },
            new int[] { 28 }, new int[] { /*32,*/ 64 /*, 128 */})]
        //[DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "0", "1", "2" },
        //    new int[] { 28 }, new int[] { /*32,*/ 64 /*, 128 */})]
        //[DataRow("MnistPng28x28\\training", new string[] { "x", },
        //    new int[] { 28 }, new int[] { 64, /*64, 128 */})]
        //[DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "y", },
        //    new int[] { 28 }, new int[] { 128 })]
        public void TrainMultilevelImageTest(string trainingFolder, string testingFolder, string[] digits, int[] imageSizes, int[] topologies)
        {
            List<string> resultFiles = new List<string>();

            int[] poolerTopology = new int[] { 28, 20, 16 };

            // Index of layer, which is used for supervised prediction.
            // This test can create more layers with the goal to analyse result.
            // However sparse representation of specific layer can be used for supervised
            // prediction.
            int targetLyrIndx = 2;

            const string TestOutputFolder = "Output";
            //if (Directory.Exists(TestOutputFolder))
            //    Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            // Topology loop
            for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
            {
                for (int imSizeIndx = 0; imSizeIndx < imageSizes.Length; imSizeIndx++)
                {
                    var numOfCols = topologies[topologyIndx] * topologies[topologyIndx];
                    var parameters = GetDefaultParams();
                    parameters.Set(KEY.POTENTIAL_RADIUS, (int)0.5 * imageSizes[imSizeIndx]);
                    parameters.Set(KEY.POTENTIAL_PCT, 0.75);
                    parameters.Set(KEY.GLOBAL_INHIBITION, false);
                    parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
                    parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * imageSizes[imSizeIndx]);
                    parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
                    parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * numOfCols);
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
                    parameters.Set(KEY.MAX_BOOST, 5);
                    parameters.setInputDimensions(new int[] { imageSizes[imSizeIndx], imageSizes[imSizeIndx] });
                    parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    //parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);

                    var mem = new Connections();

                    parameters.apply(mem);

                    var sp = new HtmModuleNet(parameters, poolerTopology);

                    List<string> trainingFiles = new List<string>();

                    foreach (var digit in digits)
                    {
                        string digitFolder = Path.Combine(trainingFolder, digit);

                        if (!Directory.Exists(digitFolder))
                            continue;

                        var trainingImages = Directory.GetFiles(digitFolder);

                        Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

                        int counter = 0;

                        int actiColLen = numOfCols;

                        string outFolder = $"{TestOutputFolder}\\{digit}\\{topologies[topologyIndx]}x{topologies[topologyIndx]}";

                        Directory.CreateDirectory(outFolder);

                        string outputHamDistFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_hamming.txt";

                        string outputActColFile = $"{outFolder}\\digit{digit}_{topologies[topologyIndx]}_activeCol.txt";

                        trainingFiles.Add(outputActColFile);

                        using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
                        {
                            using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                            {
                                foreach (var mnistImage in trainingImages)
                                {
                                    FileInfo fI = new FileInfo(mnistImage);

                                    string outputImage = $"{outFolder}\\digit_{digit}_cycle_{counter}_{topologies[topologyIndx]}_{fI.Name}";

                                    string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                                    string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                                    //Read input csv file into array
                                    int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                                    int numIterationsPerImage = 10;
                                    int[] oldArray = new int[sp.GetActiveColumns(targetLyrIndx).Length];

                                    var activeArray = sp.GetActiveColumns(targetLyrIndx);

                                    for (int k = 0; k < numIterationsPerImage; k++)
                                    {
                                        sp.Compute(mem, inputVector, true);

                                        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                        var distance = MathHelpers.GetHammingDistance(oldArray, sp.GetActiveColumns(targetLyrIndx));
                                        swHam.WriteLine($"{counter++}|{distance} ");

                                        oldArray = new int[actiColLen];
                                        sp.GetActiveColumns(targetLyrIndx).CopyTo(oldArray, 0);
                                    }

                                    var activeStr = Helpers.StringifyVector(activeArray);
                                    swActCol.WriteLine($"{digit}, " + activeStr);

                                    List<int[,]> bmpArrays = new List<int[,]>();

                                    for (int layer = 0; layer < sp.Layers; layer++)
                                    {
                                        int[,] arr = ArrayUtils.Make2DArray<int>(sp.GetActiveColumns(layer), (int)Math.Sqrt(sp.GetActiveColumns(layer).Length), (int)Math.Sqrt(sp.GetActiveColumns(layer).Length));
                                        arr = ArrayUtils.Transpose(arr);
                                        bmpArrays.Add(arr);
                                    }

                                    int[,] twoDimInputArray = ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length));
                                    twoDimInputArray = ArrayUtils.Transpose(twoDimInputArray);

                                    bmpArrays.Add(twoDimInputArray);

                                    NeoCortexUtils.DrawBitmaps(bmpArrays, outputImage, OutImgSize, OutImgSize);
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

                    string sdrResults = MergeFiles(trainingFiles);

                    //DoSupervisedLearningWithMLNet(topologies[topologyIndx], sdrResults);
                }
            }
        }


        [TestMethod]
        //[DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" },
        //  new int[] { 28 }, new int[] { /*32,*/ 64 /*, 128 */})]
        [DataRow("MnistPng28x28\\training", "MnistPng28x28\\testing", new string[] { "2" },
            new int[] { 28 }, new int[] { 64 })]
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
            int targetLyrIndx = 2;

            const string TestOutputFolder = "Output";
            //if (Directory.Exists(TestOutputFolder))
            //    Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            // Topology loop
            for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
            {
                for (int imSizeIndx = 0; imSizeIndx < imageSizes.Length; imSizeIndx++)
                {
                    var numOfCols = topologies[topologyIndx] * topologies[topologyIndx];
                    var parameters = GetDefaultParams();
                    parameters.Set(KEY.POTENTIAL_RADIUS, (int)0.3 * imageSizes[imSizeIndx]);
                    parameters.Set(KEY.POTENTIAL_PCT, 0.75);
                    parameters.Set(KEY.GLOBAL_INHIBITION, false);
                    parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
                    parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * imageSizes[imSizeIndx]);
                    parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
                    parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * numOfCols);
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
                    parameters.Set(KEY.MAX_BOOST, 5);
                    parameters.setInputDimensions(new int[] { imageSizes[imSizeIndx], imageSizes[imSizeIndx] });
                    parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    //parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);


                    List<string> trainingFiles = new List<string>();

                    foreach (var digit in digits)
                    {
                        string digitFolder = Path.Combine(trainingFolder, digit);

                        if (!Directory.Exists(digitFolder))
                            continue;

                        var trainingImages = Directory.GetFiles(digitFolder);

                        Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

                        int counter = 0;

                        int actiColLen = numOfCols;

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

                            var sp = new SpatialPooler();
                            sp.init(mem);

                            int[] activeArray = new int[topologies[topologyIndx] * topologies[topologyIndx]];

                            if (imgCnt > 10) break;

                            imgCnt++;

                            FileInfo fI = new FileInfo(mnistImage);

                            string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSizes[imSizeIndx]}";

                            string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imageSizes[imSizeIndx], testName);

                            //Read input csv file into array
                            int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                            int numIterationsPerImage = 5;

                            int[] oldArray = new int[activeArray.Length];

                            Debug.WriteLine($"\r\n{digit}");

                            for (int k = 0; k < numIterationsPerImage; k++)
                            {
                                sp.compute(mem, inputVector, activeArray, true);

                                var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);

                                Debug.WriteLine(distance);

                                oldArray = new int[actiColLen];

                                activeArray.CopyTo(oldArray, 0);

                                int[,] twoDimInputArray = ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length));
                                twoDimInputArray = ArrayUtils.Transpose(twoDimInputArray);

                                int[,] winArray = ArrayUtils.Make2DArray<int>(activeArray, (int)Math.Sqrt(activeArray.Length), (int)Math.Sqrt(activeArray.Length));
                                winArray = ArrayUtils.Transpose(twoDimInputArray);

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
        private static string MergeFiles(List<string> trainingFiles)
        {
            string sparseFileName = "sparse-results.csv";
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

                    string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imgSize, testName);

                    //Read input csv file into array
                    int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                    sp.Compute(mem, inputVector, false);

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

        /// <summary>
        /// Binarize image to binarizedImage.
        /// </summary>
        /// <param name="mnistImage"></param>
        /// <param name="imageSize"></param>
        /// <param name="testName"></param>
        /// <returns></returns>
        private static string BinarizeImage(string mnistImage, int imageSize, string testName)
        {
            string binaryImage;

            Binarizer imageBinarizer = new Binarizer(200, 200, 200, imageSize, imageSize);
            binaryImage = $"{testName}.txt";
            if (File.Exists(binaryImage))
                File.Delete(binaryImage);

            imageBinarizer.CreateBinary(mnistImage, binaryImage);

            return binaryImage;
        }


        #region Private Helpers

        private static Parameters GetDefaultParams()
        {

            Random rnd = new Random(42);

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
