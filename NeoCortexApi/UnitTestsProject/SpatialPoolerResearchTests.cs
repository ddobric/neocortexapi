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
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerResearchTests
    {
        private const int OutImgSize = 1024;


     
        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void StableOutputOnSameInputTest()
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
            var sp = new SpatialPooler();
            var mem = new Connections();
            //List<int> intList = ArrayUtils.ReadCsvFileTest("TestFiles\\digit1_binary_32bit.txt");
            //intList.Clear();

            //List<int> intList = new List<int>();


            int[] inputVector = new int[1024];

            for (int i = 0; i < 31; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (i > 2 && i < 5 && j > 2 && j < 8)
                        inputVector[i * 32 + j] = 1;
                    else
                        inputVector[i * 32 + j] = 0;
                }
            }

            parameters.apply(mem);
            sp.init(mem);

            //int[] activeArray = new int[64 * 64];

            for (int i = 0; i < 10; i++)
            {
                //sp.compute( inputVector, activeArray, true);
                var activeArray = sp.Compute(inputVector, true) as int[];

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str);
            }

        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void NoiseTest()
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
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);

            parameters.Set(KEY.SEED, 42);

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { colDimSize, colDimSize });

            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            //var rnd = new Random();

            parameters.apply(mem);
            sp.init(mem);

            List<int[]> inputVectors = new List<int[]>();

            inputVectors.Add(getInputVector1());
            inputVectors.Add(getInputVector2());

            int vectorIndex = 0;

            int[][] activeArrayWithZeroNoise = new int[inputVectors.Count][];

            foreach (var inputVector in inputVectors)
            {
                var x = getNumBits(inputVector);

                Debug.WriteLine("");
                Debug.WriteLine($"----- VECTOR {vectorIndex} ----------");

                //int[] activeArray = new int[64 * 64];
                activeArrayWithZeroNoise[vectorIndex] = new int[colDimSize * colDimSize];

                int[] activeArray = null;

                for (int j = 0; j < 25; j += noiseStepPercent)
                {
                    Debug.WriteLine($"--- Vector {0} - Noise Iteration {j} ----------");

                    int[] noisedInput;

                    if (j > 0)
                    {
                        noisedInput = ArrayUtils.flipBit(inputVector, (double)((double)j / 100.00));
                    }
                    else
                        noisedInput = inputVector;

                    var d = MathHelpers.GetHammingDistance(inputVector, noisedInput, true);
                    Debug.WriteLine($"Input with noise {j} - HamDist: {d}");
                    Debug.WriteLine($"Original: {Helpers.StringifyVector(inputVector)}");
                    Debug.WriteLine($"Noised:   {Helpers.StringifyVector(noisedInput)}");

                    for (int i = 0; i < 10; i++)
                    {
                        //sp.compute( noisedInput, activeArray, true);
                        activeArray = sp.Compute(noisedInput, true, returnActiveColIndiciesOnly: false) as int[];

                        if (j > 0)
                            Debug.WriteLine($"{ MathHelpers.GetHammingDistance(activeArrayWithZeroNoise[vectorIndex], activeArray, true)} -> {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArray, (el) => el == 1))}");
                    }

                    if (j == 0)
                    {
                        Array.Copy(activeArray, activeArrayWithZeroNoise[vectorIndex], activeArrayWithZeroNoise[vectorIndex].Length);
                    }

                    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                    var d2 = MathHelpers.GetHammingDistance(activeArrayWithZeroNoise[vectorIndex], activeArray, true);
                    Debug.WriteLine($"Output with noise {j} - Ham Dist: {d2}");
                    Debug.WriteLine($"Original: {Helpers.StringifyVector(ArrayUtils.IndexWhere(activeArrayWithZeroNoise[vectorIndex], (el) => el == 1))}");
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

            //
            // Prediction code.
            // This part of code takes a single sample of every input vector and add
            // some noise to it. Then it predicts it.
            // Calculated hamming distance (percent overlap) between predicted output and output 
            // trained without noise is final result, which should be higher than 95% (realistic guess).

            vectorIndex = 0;

            foreach (var inputVector in inputVectors)
            {
                double noise = 7;
                var noisedInput = ArrayUtils.flipBit(inputVector, noise / 100.00);

                int[] activeArray = new int[64 * 64];

                sp.compute(noisedInput, activeArray, false);

                var dist = MathHelpers.GetHammingDistance(activeArrayWithZeroNoise[vectorIndex], activeArray, true);
                Debug.WriteLine($"Result for vector {vectorIndex++} with noise {noise} - Ham Dist: {dist}");

                Assert.IsTrue(dist >= 95);
            }
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
                sp.compute(inputVector, activeArray, true);

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

            sp.compute(inputVector, activeArray, true);

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
                        var nbs = HtmCompute.GetNeighborhood(center, rad, mem.getColumnTopology().HtmTopology);

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
        [TestCategory("LongRunning")]
        public void SPInhibitionTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 10 });
            parameters.setColumnDimensions(new int[] { 1024 });
            parameters.setNumActiveColumnsPerInhArea(0.2 * 32 * 32);
            parameters.setGlobalInhibition(false);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            //int[] inputVector = NeoCortexUtils.ReadCsvFileTest("Testfiles\\digit8_binary_32bit.txt").ToArray();
            // var inputString = Helpers.StringifyVector(inputVector);
            //Debug.WriteLine("Input Array: " + inputString);
            int[] inputVector = new int[] { 1, 0, 0, 0, 1, 1, 1, 0, 1, 1 };
            int[] activeArray = new int[32 * 32];
            //int iteration = -1;
            String str = "";
            for (int i = 0; i < 10; i++)
            {

                var overlaps = sp.CalculateOverlap(mem, inputVector);
                var strOverlaps = Helpers.StringifyVector(overlaps);

                var inhibitions = sp.inhibitColumns(mem, ArrayUtils.toDoubleArray(overlaps));
                var strInhibitions = Helpers.StringifyVector(inhibitions);

                activeArray = sp.Compute(inputVector, true) as int[];

                //Debug.WriteLine(result);
                //sp.compute( inputVector, activeArray, true);

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
        [TestCategory("LongRunning")]
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
                                int[] inputVector = NeoCortexUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();
                                sw.Restart();

                                int iterations = 2;
                                int[] oldArray = new int[activeArray.Length];
                                for (int k = 0; k < iterations; k++)
                                {
                                    sp.compute(inputVector, activeArray, true);

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
        [DataRow("TestFiles\\digit8.png", "digit8_binarized.txt")]
        [DataRow("TestFiles\\digit7.png", "digit7_binarized.txt")]
        [DataRow("TestFiles\\digit1.png", "digit1_binarized.txt")]
        public void BinarizeImageTest(String sourcePath, String destinationPath)
        {
            Binarizer imageBinarizer = new Binarizer(200, 200, 200, 32, 32);

            imageBinarizer.CreateBinary(sourcePath, destinationPath);

            string res = imageBinarizer.GetBinary(sourcePath);
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



        /// <summary>
        ///TEsting the shifted image with  trined image (how much it is memorising) 
        ///predecting images with trained images, new hamming diatance given by professor is used ////////////
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSize">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow("MnistTestImages\\i(0).png", "MnistTestImages\\i(5).png", new int[] { 50 }, new int[] { 60 })]
        // [DataRow("MnistTestImages\\digit7.png", 128, 30)]
        public void LearningimageDoubleShiftStble(string mnistImage, string shiftedImage, int[] imageSize, int[] topologies)
        {

            var path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, mnistImage);
            var pathShifted = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, shiftedImage);
            Console.WriteLine("Test started");
            Console.WriteLine(mnistImage);
            Console.WriteLine(shiftedImage);
            const int OutImgSize = 1024;

            int index1 = mnistImage.IndexOf("\\") + 1;
            int index2 = mnistImage.IndexOf(".");
            string sub1 = mnistImage.Substring(0, index2);
            string sub2 = mnistImage.Substring(0, index1);
            string name = mnistImage.Substring(index1, sub1.Length - sub2.Length);

            int index1Shift = shiftedImage.IndexOf("\\") + 1;
            int index2Shift = shiftedImage.IndexOf(".");
            string sub1Shift = shiftedImage.Substring(0, index2Shift);
            string sub2Shift = shiftedImage.Substring(0, index1Shift);
            string nameShift = shiftedImage.Substring(index1Shift, sub1Shift.Length - sub2Shift.Length);

            for (int imSizeIndx = 0; imSizeIndx < imageSize.Length; imSizeIndx++)
            {
                Console.WriteLine(String.Format("Image Size: \t{0}", imageSize[imSizeIndx]));

                string testName = $"{name}_{imageSize[imSizeIndx]}";
                string outputSpeedFile = $"Output\\{testName}_speed.txt";

                string testNameShifted = $"{nameShift}_{imageSize[imSizeIndx]}";
                string outputSpeedFileShifted = $"Output\\{testNameShifted}_speed.txt";

                string inputBinaryImageFile = BinarizeImage(path, imageSize[imSizeIndx], testName);
                string inputBinaryImageShiftedFile = BinarizeImage(pathShifted, imageSize[imSizeIndx], testNameShifted);

                //string inputBinaryImageFile = BinarizeImage("Output\\" + mnistImage, imageSize[imSizeIndx], testName);
                for (int topologyIndx = 0; topologyIndx < topologies.Length; topologyIndx++)
                {
                    Console.WriteLine(String.Format("Topology: \t{0}", topologies[topologyIndx]));

                    string finalName = $"{testName}_{topologies[topologyIndx]}";
                    string outputHamDistFile = $"Output\\{finalName}_hamming.txt";
                    string outputActColFile = $"Output\\{finalName}_activeCol.txt";

                    string outputImage = $"Output\\{finalName}.png";

                    string finalNameShifted = $"{testNameShifted}_{topologies[topologyIndx]}";
                    string outputHamDistFileShifted = $"Output\\{finalNameShifted}_hamming.txt";
                    string outputActColFileShifted = $"Output\\{finalNameShifted}_activeCol.txt";

                    string outputImageShifted = $"Output\\{finalNameShifted}.png";


                    //File.Create(finalName);
                    //Directory.CreateDirectory("Output");
                    //File.Create(outputHamDistFile);
                    //File.Create(outputActColFile);

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
                                int[] inputVector = NeoCortexUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                                var inputOverlap = new List<double>();
                                var outputOverlap = new List<double>();

                                int[] newActiveArray = new int[topologies[topologyIndx] * topologies[topologyIndx]];
                                double[][] newActiveArrayDouble = new double[1][];
                                newActiveArrayDouble[0] = new double[newActiveArray.Length];

                                //Training 
                                sp.compute(inputVector, activeArray, true);

                                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                                double[][] oldActiveArray = new double[1][];
                                oldActiveArray[0] = new double[activeArray.Length];
                                for (int a = 0; a < activeArray.Length; a++)
                                {
                                    oldActiveArray[0][a] = activeArray[a];
                                }

                                int isTrained = 0;

                                while (isTrained == 0)
                                {
                                    sp.compute(inputVector, newActiveArray, true);

                                    activeCols = ArrayUtils.IndexWhere(newActiveArray, (el) => el == 1);



                                    for (int a = 0; a < newActiveArray.Length; a++)
                                    {
                                        newActiveArrayDouble[0][a] = newActiveArray[a];
                                    }

                                    if (MathHelpers.GetHammingDistance(oldActiveArray, newActiveArrayDouble, true)[0] == 100)
                                    {
                                        isTrained = 1;
                                    }
                                    else
                                    {
                                        isTrained = 0;
                                        oldActiveArray = newActiveArrayDouble;
                                    }
                                }

                                var str = Helpers.StringifyVector(activeCols);


                                int[] oldInputVector = NeoCortexUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();

                                int[] inputVectorShifted = NeoCortexUtils.ReadCsvFileTest(inputBinaryImageShiftedFile).ToArray();

                                int[] activeArrayShifted = new int[topologies[topologyIndx] * topologies[topologyIndx]];
                                double[][] activeArrayShiftedDouble = new double[1][];
                                activeArrayShiftedDouble[0] = new double[activeArrayShifted.Length];

                                sw.Restart();

                                //Prediction
                                sp.compute(inputVectorShifted, activeArrayShifted, false);

                                var resActiveColsPercent = ArrayUtils.IndexWhere(activeArrayShifted, (el) => el == 1);

                                var resStrPercent = Helpers.StringifyVector(activeArrayShifted);


                                for (int a = 0; a < activeArrayShifted.Length; a++)
                                {
                                    activeArrayShiftedDouble[0][a] = activeArrayShifted[a];
                                }



                                var distance = MathHelpers.GetHammingDistance(newActiveArrayDouble, activeArrayShiftedDouble, false)[0];
                                var distPercent = ((100 - distance) * 100) / (topologies[topologyIndx] * topologies[topologyIndx]);
                                swHam.WriteLine(distance + "\t" + (100 - distance) + "\t" + distPercent + "\t" + (1 - (distPercent / 100.0)) + "\n");

                                outputOverlap.Add(1 - (distPercent / 100.0));

                                swActCol.WriteLine(String.Format(@"Active Cols: {0}", Helpers.StringifyVector(resActiveColsPercent)));

                                Console.WriteLine(resStrPercent);
                                swActCol.WriteLine("Active Array: " + resStrPercent);

                                sw.Stop();

                                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArrayShifted, topologies[topologyIndx], topologies[topologyIndx]);
                                twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                                NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, outputImage);
                                //NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, "D:\\Project_Latest\\FromGit\\se-dystsys-2018-2019-softwareengineering\\outputs\\eight");


                                swActCol.WriteLine("inputOverlaps: " + Helpers.StringifyVector(inputOverlap.ToArray()));

                                swActCol.WriteLine("outputOverlaps: " + Helpers.StringifyVector(outputOverlap.ToArray()));
                            }
                        }


                    }
                }


            }
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
}
