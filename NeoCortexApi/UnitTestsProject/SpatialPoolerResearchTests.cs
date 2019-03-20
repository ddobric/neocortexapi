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

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerResearchTests
    {
        private const int OutImgSize = 1024;

        [TestMethod]
        public void StableOutputOnSameInputTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPooler();
            var mem = new Connections();
            List<int> intList = ArrayUtils.ReadCsvFileTest("TestDigit\\digit1_binary_32bit.txt");
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[64 * 64];
            int[] inputVector = intList.ToArray();
            for (int i = 0; i < 200; i++)
            {
                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str);
            }

        }

        /// <summary>
        /// Creates data which shows how columns are connected to sensory input.
        /// </summary>
        [TestMethod]
        public void CollSynapsesToInput()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32 });
            parameters.setColumnDimensions(new int[] { 128 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 128);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[128];

            int[] inputVector = Helpers.GetRandomVector(32, parameters.Get<Random>(KEY.RANDOM));

            for (int i = 0; i < 100; i++)
            {
                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str);
            }
        }

        /// <summary>
        /// Corresponds to git\nupic\examples\sp\sp_tutorial.py
        /// </summary>
        [TestMethod]
        public void SPTutorialTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 1000 });
            parameters.setColumnDimensions(new int[] { 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
            parameters.setGlobalInhibition(false);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[2048];

            int[] inputVector = Helpers.GetRandomVector(1000, parameters.Get<Random>(KEY.RANDOM));

            sp.compute(mem, inputVector, activeArray, true);

            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

            var str = Helpers.StringifyVector(activeCols);

            Debug.WriteLine(str);

        }


        /// <summary>
        /// This test generates neghborhood cells for different radius and center of cell topoogy 64 single dimensional cell array.
        /// Reults are stored in CSV file, which is loaded by Python code 'neighborhood-test.py'to create a plotly diagram.
        /// </summary>
        [TestMethod]
        public void NeighborhoodTest()
        {
            var parameters = GetDefaultParams();

            int cellsDim1 = 64;
            int cellsDim2 = 64;

            parameters.setInputDimensions(new int[] { 32 });
            parameters.setColumnDimensions(new int[] { cellsDim1 });

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            for (int rad = 1; rad < 10; rad++)
            {
                using (StreamWriter sw = new StreamWriter($"neighborhood-test-rad{rad}-center-from-{cellsDim1}-to-{0}.csv"))
                {
                    sw.WriteLine($"{cellsDim1}|{cellsDim2}|{rad}|First column defines center of neiborhood. All other columns define indicies of neiborhood columns");

                    for (int center = 0; center < 64; center++)
                    {
                        var nbs = mem.getColumnTopology().GetNeighborhood(center, rad);

                        StringBuilder sb = new StringBuilder();

                        sb.Append(center);
                        sb.Append('|');

                        foreach (var neighobordCellIndex in nbs)
                        {
                            sb.Append(neighobordCellIndex);
                            sb.Append('|');
                        }

                        string str = sb.ToString();

                        sw.WriteLine(str.TrimEnd('|'));
                    }
                }
            }
        }


        /// <summary>
        /// Generates result of inhibition
        /// </summary>
        [TestMethod]
        public void SPInhibitionTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 32, 32 });
            parameters.setNumActiveColumnsPerInhArea(0.2 * 32 * 32);
            parameters.setGlobalInhibition(false);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            int[] inputVector = ArrayUtils.ReadCsvFileTest("Testfiles\\digit8_binary_32bit.txt").ToArray();
            var inputString = Helpers.StringifyVector(inputVector);
            Debug.WriteLine("Input Array: " + inputString);
            //int[] inputVector = new int[] { 1, 0, 0, 0, 1, 1, 1, 0, 1, 1};
            int[] activeArray = new int[32 * 32];
            //int iteration = -1;
            String str = "";
            for (int i = 0; i < 100; i++)
            {

                var overlaps = sp.calculateOverlap(mem, inputVector);
                var strOverlaps = Helpers.StringifyVector(overlaps);

                var inhibitions = sp.inhibitColumns(mem, ArrayUtils.toDoubleArray(overlaps));
                var strInhibitions = Helpers.StringifyVector(inhibitions);

                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                var strActiveArr = Helpers.StringifyVector(activeArray);
                Debug.WriteLine("Active array: " + strActiveArr);
                var strActiveCols = Helpers.StringifyVector(activeCols);
                Debug.WriteLine("Number of Active Column: " + activeCols.Length);
                str = strActiveCols;
                Debug.WriteLine($"{i} - {strActiveCols}");
            }
            var strOutput = Helpers.StringifyVector(activeArray);
            Debug.WriteLine("Output: " + strOutput);
        }


        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSize">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [DataRow("MnistTestImages\\digit7.png", new int[] { 32, 64 }, new int[] { 10, 20 })]
        //[DataRow("MnistTestImages\\digit7.png", 128, 30)]
        public void CalculateSpeedOfLearningTest(string mnistImage, int[] imageSize, int[] topologies)
        {
            int index1 = mnistImage.IndexOf("\\") + 1;
            int index2 = mnistImage.IndexOf(".");
            string sub1 = mnistImage.Substring(0, index2);
            string sub2 = mnistImage.Substring(0, index1);
            string name = mnistImage.Substring(index1, sub1.Length - sub2.Length);
            for (int imSizeIndx = 0; imSizeIndx < imageSize.Length; imSizeIndx++)
            {
                string testName = $"{name}_{imageSize[imSizeIndx]}";
                string outputSpeedFile = $"Output\\{testName}_speed.txt";
                string inputBinaryImageFile = BinarizeImage("Output\\" + mnistImage, imageSize[imSizeIndx], testName);
                for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
                {
                    string finalName = $"{testName}_{topologies[topologyIndx]}";
                    string outputHamDistFile = $"Output\\{finalName}_hamming.txt";
                    string outputActColFile = $"Output\\{finalName}_activeCol.txt";

                    string outputImage = $"Output\\{finalName}.png";

                    int numOfActCols = 0;
                    var sw = new Stopwatch();
                    using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
                    {
                        using (StreamWriter swSpeed = new StreamWriter(outputSpeedFile, true))
                        {
                            using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                            {
                                numOfActCols = topologies[topologyIndx] * topologies[topologyIndx];
                                var parameters = GetDefaultParams();
                                parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
                                parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                                parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);

                                var sp = new SpatialPooler();
                                var mem = new Connections();

                                parameters.apply(mem);
                                sw.Start();

                                sp.init(mem);

                                sw.Stop();
                                swSpeed.WriteLine($"{topologies[topologyIndx]}|{(double)sw.ElapsedMilliseconds / (double)1000}");

                                int actiColLen = numOfActCols;
                                int[] activeArray = new int[actiColLen];

                                //Read input csv file into array
                                int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();
                                sw.Restart();

                                int iterations = 2;
                                int[] oldArray = new int[activeArray.Length];
                                for (int k = 0; k < iterations; k++)
                                {
                                    sp.compute(mem, inputVector, activeArray, true);

                                    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                    var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                                    swHam.WriteLine(distance + "\n");
                                    var str = Helpers.StringifyVector(activeCols);

                                    oldArray = new int[actiColLen];
                                    activeArray.CopyTo(oldArray, 0);
                                }

                                var activeStr = Helpers.StringifyVector(activeArray);
                                swActCol.WriteLine("Active Array: " + activeStr);

                                sw.Stop();

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
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSize">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        //[DataRow("MnistPng28x28\\training", "7", new int[] { 28 }, new int[] { 32, /*64, 128 */ })]
        //[DataRow("MnistPng28x28\\training", new string[] {"0", "1", "2", "3", "4", "5", "6", "7"},
        //    new int[] { 28 }, new int[] { 32, /*64, 128 */})]
        //[DataRow("MnistPng28x28\\training", new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" },
        //    new int[] { 28 }, new int[] { 32, /*64, 128 */})]
        [DataRow("MnistPng28x28\\training", new string[] { "x", },
            new int[] { 28 }, new int[] { 128, /*64, 128 */})]
        //[DataRow("MnistPng28x28\\training", new string[] { "y", },
        //    new int[] { 28 }, new int[] { 128})]
        public void TrainMultilevelImageTest(string trainingFolder, string[] digits, int[] imageSize, int[] topologies)
        {
            const string TestOutputFolder = "Output";
            //if (Directory.Exists(TestOutputFolder))
            //    Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            // Topology loop
            for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
            {
                for (int imSizeIndx = 0; imSizeIndx < imageSize.Length; imSizeIndx++)
                {
                    var numOfActCols = topologies[topologyIndx] * topologies[topologyIndx];
                    var parameters = GetDefaultParams();
                    parameters.Set(KEY.POTENTIAL_RADIUS, (int)0.3 * imageSize[imSizeIndx]);
                    parameters.Set(KEY.POTENTIAL_PCT, 0.75);
                    parameters.Set(KEY.GLOBAL_INHIBITION, false);
                    parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
                    parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * imageSize[imSizeIndx]);
                    parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
                    parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * numOfActCols);
                    parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
                    parameters.Set(KEY.MAX_BOOST, 5);
                    parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
                    parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
                    //parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);

                    var mem = new Connections();

                    parameters.apply(mem);

                    var sp = new HtmModuleNet(parameters, new int[] { 28, 32, 16, 7, 4 });

                    foreach (var digit in digits)
                    {
                        string digitFolder = Path.Combine(trainingFolder, digit);

                        if (!Directory.Exists(digitFolder))
                            continue;

                        var trainingImages = Directory.GetFiles(digitFolder);

                        Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

                        int counter = 0;

                        int actiColLen = numOfActCols;

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

                                    int numIterationsPerImage = 10;
                                    int[] oldArray = new int[sp.GetActiveColumns(2).Length];

                                    var activeArray = sp.GetActiveColumns(2);

                                    for (int k = 0; k < numIterationsPerImage; k++)
                                    {
                                        sp.Compute(mem, inputVector, true);

                                        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                        var distance = MathHelpers.GetHammingDistance(oldArray, sp.GetActiveColumns(2));
                                        swHam.WriteLine($"{counter++}|{distance} ");

                                        oldArray = new int[actiColLen];
                                        sp.GetActiveColumns(2).CopyTo(oldArray, 0);
                                    }

                                    var activeStr = Helpers.StringifyVector(activeArray);
                                    swActCol.WriteLine("Active Array: " + activeStr);

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

                        foreach (var mnistImage in trainingImages)
                        {
                            FileInfo fI = new FileInfo(mnistImage);

                            string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{imageSize[imSizeIndx]}";

                            string inputBinaryImageFile = BinarizeImage($"{mnistImage}", imageSize[imSizeIndx], testName);

                            int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                            sp.Compute(mem, inputVector, true);

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

                            string outputImage = $"{outFolder}\\digit_{digit}_PREDICT_{topologies[topologyIndx]}_{fI.Name}";

                            NeoCortexUtils.DrawBitmaps(bmpArrays, outputImage, OutImgSize, OutImgSize);
                        }
                    }
                }
            }
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

        /// <summary>
        /// This test generate a text file of binary image from original image.The text file is 
        /// </summary>
        [TestMethod]
        [DataRow("TestFiles\\digit8.png")]
        public void BinarizeImageTest(String sourcePath, String destinationPath)
        {
            Binarizer imageBinarizer = new Binarizer(200, 200, 200, 32, 32);
            imageBinarizer.CreateBinary(sourcePath, destinationPath);
        }

        /// <summary>
        /// This test read csv file and save data in the file as List of integer value.
        /// @return List<int> list of integer value after reading file.
        /// </summary>
        [TestMethod]
        [DataRow("TestDigit\\digit1_binary_32bit.txt")]
        public List<int> ReadCsvFileTest(String sourcePath)
        {
            string fileContent = File.ReadAllText(sourcePath);
            string[] integerStrings = fileContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> intList = new List<int>();
            for (int n = 0; n < integerStrings.Length; n++)
            {
                String s = integerStrings[n];
                char[] sub = s.ToCharArray();
                for (int j = 0; j < sub.Length - 1; j++)
                {
                    intList.Add(int.Parse(sub[j].ToString()));
                }
            }
            return intList;
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
}
