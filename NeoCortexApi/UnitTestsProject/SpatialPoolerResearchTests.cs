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
            parameters.setNumActiveColumnsPerInhArea(0.02 * 32 * 32);
            parameters.setGlobalInhibition(false);

            var sp = new SpatialPooler();
            
            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            int[] inputVector = ArrayUtils.ReadCsvFileTest("Output\\BinaryImages\\digit7_32.txt").ToArray();
            //var inputString = Helpers.StringifyVector(inputVector);
            //Debug.WriteLine("Input Array: " + inputString);
            //int[] inputVector = new int[] { 1, 0, 0, 0, 1, 1, 1, 0, 1, 1};
            int[] activeArray = new int[32 * 32];
            //int iteration = -1;
            String str = "";
            for (int i = 0; i < 1; i++)
            {
                var overlaps = sp.calculateOverlap(mem, inputVector);
                var strOverlaps = Helpers.StringifyVector(overlaps);

                var inhibitions = sp.inhibitColumns(mem, ArrayUtils.toDoubleArray(overlaps));
                var strInhibitions = Helpers.StringifyVector(inhibitions);

                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                var strActiveArr = Helpers.StringifyVector(activeArray);
                //Debug.WriteLine("Active array: " + strActiveArr);
                var strActiveCols = Helpers.StringifyVector(activeCols);
                //Debug.WriteLine("Number of Active Column: " + activeCols.Length);
                //str = strActiveCols;
                Debug.WriteLine($"{i} - {strActiveCols}");
            }
            //var strOutput = Helpers.StringifyVector(activeArray);
            //Debug.WriteLine("Output: " + strOutput);
        }

        [TestMethod]
        public void InputOutputDependency()
        {
            List<double> noisePercentage = CreateRange(0, 0.95, 0.1);
            int[] inputVector = ArrayUtils.ReadCsvFileTest("Output\\BinaryImages\\digit7_32.txt").ToArray();
            string inputDistance = "Output\\hammingInput.txt";
            string outputDistance = "Output\\hammingOutput.txt";
            int[] outputVector = new int[64*64];
            var parameters = GetDefaultParams();
            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPooler();
            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            foreach (double i in noisePercentage)
            {
                var outputVectorStr = Helpers.StringifyVector(outputVector);
                //Debug.WriteLine($"Ouput Vector:{outputVectorStr}");
                int[] newInputVector = new int[32*32];
                newInputVector = ArrayUtils.flipBit(inputVector, i);
                var inputStr = Helpers.StringifyVector(newInputVector);
                Debug.WriteLine($"{i}:{inputStr}");
                using (StreamWriter inputDist = new StreamWriter(inputDistance,true))
                {
                    using (StreamWriter outputDist = new StreamWriter(outputDistance,true))
                    {
                        var distance = GetHammingDistanceTest(inputVector, newInputVector);
                        //var distance = MathHelpers.GetHammingDistance(inputVector, newInputVector);
                        inputDist.WriteLine(distance);

                        int[] activeArray = new int[64*64];
                        for (int k = 0; k < 1; k++)
                        {
                            sp.compute(mem, newInputVector, activeArray, true);

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                            //swHam.WriteLine(distance + "\n");
                            var str = Helpers.StringifyVector(activeCols);
                            Debug.WriteLine($"{str}");
                        }
                        if (i == 0)
                        {
                            outputVector = new int[64*64];
                            activeArray.CopyTo(outputVector, 0);
                        }
                        var strActiveArr = Helpers.StringifyVector(activeArray);
                        //Debug.WriteLine($"{i}:{strActiveArr}");
                        //var outDistance = GetHammingDistanceTest(outputVector, activeArray); 
                        var outDistance = MathHelpers.GetHammingDistance(outputVector, activeArray);
                        outputDist.WriteLine(outDistance);
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
        [DataRow("digit7", new int[] {32}, new int[] {32})]
        public void CalculateSpeedOfLearningTest(string mnistImage, int[] imageSize, int[] topologies)
        {
            for (int imSizeIndx = 0; imSizeIndx < imageSize.Length; imSizeIndx++)
            {
                string testName = $"{mnistImage}_{imageSize[imSizeIndx]}";
                string outputSpeedFile = $"Output\\{testName}_speed.txt";
                string inputBinaryImageFile = BinarizeImage($"MnistTestImages\\{mnistImage}.png", imageSize[imSizeIndx], testName);
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
                                sp.init(mem);
                                sw.Start();
                                int actiColLen = numOfActCols;
                                int[] activeArray = new int[actiColLen];
                                //Read input csv file into array
                                int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();
                                int iterations = 120;
                                int[] oldArray = new int[activeArray.Length];
                                for (int k = 0; k < iterations; k++)
                                {
                                    sp.compute(mem, inputVector, activeArray, true);

                                    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                    
                                    //swHam.WriteLine(distance + "\n");
                                    var str = Helpers.StringifyVector(activeCols);
                                    //Debug.WriteLine($"{distance} - {str}");
                                    oldArray = new int[actiColLen];
                                    activeArray.CopyTo(oldArray, 0);
                                }
                                var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                                swHam.WriteLine(distance + "\n");
                                var activeStr = Helpers.StringifyVector(activeArray);
                                swActCol.WriteLine("Active Array: " + activeStr);
                                //Debug.WriteLine("active Array:" + activeStr);
                                sw.Stop();
                                swSpeed.WriteLine($"{topologies[topologyIndx]}|{(double)sw.ElapsedMilliseconds / (double)1000}");
                                //int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, topologies[topologyIndx], topologies[topologyIndx]);
                                //twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
                                //Debug.WriteLine("Still Running");
                                //NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, outputImage);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void CalculatingActiveColumn()
        {
            string[] fileEntries = Directory.GetFiles("C:\\Users\\n.luu\\Study\\SE\\Project\\NeoCortexApi\\UnitTestsProject\\MnistTestImages");
            int[] oldInput = new int[28*28];
            int[] oldArray = new int[28*28];
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100; i++)
            {
                string fileName = fileEntries[i].Substring(79,10);
                string outputHamDistFile = $"Output\\hammingInput.txt";
                string outputHamDistOut = $"Output\\hammingOutput.txt";
                string outputImage = $"Output\\ActiveColImage\\{fileName}.png";
                string inputBinaryImageFile = BinarizeImage($"{fileEntries[i]}", 28, fileName);
                using (StreamWriter swHam = new StreamWriter(outputHamDistFile, true))
                {
                    using (StreamWriter swActCol = new StreamWriter(outputHamDistOut,true))
                    {
                        var parameters = GetDefaultParams();
                        parameters.setInputDimensions(new int[] { 28, 28 });
                        parameters.setColumnDimensions(new int[] { 28, 28});
                        parameters.setNumActiveColumnsPerInhArea(0.02 * 28 * 28);
                        var sp = new SpatialPooler();
                        var mem = new Connections();
                        parameters.apply(mem);
                        sp.init(mem);
                        int actiColLen = 28 * 28;
                        int[] activeArray = new int[actiColLen];
                        //Read input csv file into array
                        int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();
                        var distance = MathHelpers.GetHammingDistance(inputVector, oldInput);
                        oldInput = new int[inputVector.Length];
                        inputVector.CopyTo(oldInput, 0);
                        swHam.WriteLine(distance);
                        int iterations = 100;
                        for (int k = 0; k < iterations; k++)
                        {
                            sp.compute(mem, inputVector, activeArray, true);
                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                            var str = Helpers.StringifyVector(activeCols);
                            //Debug.WriteLine($"{distance} - {str}");
                        }
                        var activColDist = MathHelpers.GetHammingDistance(activeArray, oldArray);
                        oldArray = new int[actiColLen];
                        activeArray.CopyTo(oldArray, 0);
                        var activeStr = Helpers.StringifyVector(activeArray);
                        swActCol.WriteLine(activColDist);
                        //Debug.WriteLine("active Array:" + activeStr);
                        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 28, 28);
                        twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
                        //Debug.WriteLine("Finish " + (i+1) +" Image");
                        NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, outputImage);
                    }
                }
            }
            sw.Stop();
            Debug.WriteLine($"Total Time Elapse: {(double)sw.ElapsedMilliseconds / (double)1000}");
        }
        
        [TestMethod]
        [DataRow("digit7", new int[] {20})]
        public void CalculateChangeOfRadius(string mnistImage, int[] radius)
        {
            for (int radiusIndx = 0; radiusIndx < radius.Length; radiusIndx++)
            {
                string testName = $"{mnistImage}_Radius{radius[radiusIndx]}";
                string outputSpeedFile = $"Output\\{testName}_speed_default_noboost.txt";
                string inputBinaryImageFile = BinarizeImage($"MnistTestImages\\{mnistImage}.png", 32, testName);
                string finalName = $"{testName}_default_noboost";
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
                                numOfActCols = 20*20;
                                var parameters = GetDefaultParams();
                                parameters.setPotentialRadius(radius[radiusIndx]);
                                parameters.setInputDimensions(new int[] { 128,128 });
                                parameters.setColumnDimensions(new int[] { 20,20});
                                parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);
                                var sp = new SpatialPooler();
                                var mem = new Connections();
                                parameters.apply(mem);
                                sp.init(mem);
                                sw.Start();
                                int actiColLen = numOfActCols;
                                int[] activeArray = new int[actiColLen];
                                //Read input csv file into array
                                int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();
                                int iterations = 120;
                                int[] oldArray = new int[activeArray.Length];
                                for (int k = 0; k < iterations; k++)
                                {
                                    sp.compute(mem, inputVector, activeArray, true);

                                    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                                    var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                                    swHam.WriteLine(distance + "\n");
                                    var str = Helpers.StringifyVector(activeCols);
                                    //Debug.WriteLine($"{distance} - {str}");
                                    oldArray = new int[actiColLen];
                                    activeArray.CopyTo(oldArray, 0);
                                }
                                var activeStr = Helpers.StringifyVector(activeArray);
                                swActCol.WriteLine("Active Array: " + activeStr);
                                //Debug.WriteLine("active Array:" + activeStr);
                                sw.Stop();
                                swSpeed.WriteLine($"{20}|{(double)sw.ElapsedMilliseconds / (double)1000}");
                                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 20, 20);
                                twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
                                Debug.WriteLine("Still Running");
                                NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, outputImage);
                            }
                        }
                    }
                }
        }

        [TestMethod]
        [DataRow("000015-num7")]
        public void NewMethodTest(String mnistImage)
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 28, 28 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            parameters.setGlobalInhibition(false);
            string testName = $"{mnistImage}";
            string inputBinaryImageFile = BinarizeImage($"C:\\Users\\n.luu\\Study\\SE\\Project\\NeoCortexApi\\UnitTestsProject\\MnistTestImages\\{mnistImage}.png", 28, testName);
            var sp = new SpatialPooler();
            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            int[] inputVector = ArrayUtils.ReadCsvFileTest($"Output\\BinaryImages\\{testName}.txt").ToArray();
            var inputString = Helpers.StringifyVector(inputVector);
            int numOfActCols = 64 * 64;
            int actiColLen = numOfActCols;
            int[] activeArray = new int[actiColLen];
            int[] oldArray = new int[activeArray.Length];
            String str = "";
            for (int i = 0; i < 150; i++)
            {

                var overlaps = sp.calculateOverlap(mem, inputVector);
                var strOverlaps = Helpers.StringifyVector(overlaps);

                //var inhibitions = sp.inhibitColumns(mem, ArrayUtils.toDoubleArray(overlaps));
                //var strInhibitions = Helpers.StringifyVector(inhibitions);

                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                var strActiveArr = Helpers.StringifyVector(activeArray);
                //Debug.WriteLine("Active array: " + strActiveArr);
                var strActiveCols = Helpers.StringifyVector(activeCols);
                var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                //Debug.WriteLine("Number of Active Column: " + activeCols.Length);
                str = strActiveCols;
                oldArray = new int[actiColLen];
                activeArray.CopyTo(oldArray, 0);
                Debug.WriteLine($"{i} - {strActiveCols}");
                Debug.WriteLine($"distance: - {distance}");
            }
            var strOutput = Helpers.StringifyVector(activeArray);
            Debug.WriteLine("Output: " + strOutput);
            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, 64, 64);
            twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
            NeoCortexUtils.DrawBitmap(twoDimenArray, OutImgSize, OutImgSize, "Output//OutIm_Ori.png");
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
            binaryImage = $"Output\\BinaryImages\\{testName}.txt";
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

        [TestMethod]
        //[DataRow(new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, new int[] { 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 })]
        public double GetHammingDistanceTest(int[] arr1, int[] arr2)
        {
            int sum = 0;
            int sum1 = 0;
            int sum2 = 0;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] == 1)
                {
                    sum1++;
                }
            }
            for (int j = 0; j < arr2.Length; j++)
            {
                if (arr2[j] == 1)
                {
                    sum2++;
                }
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] == 1)
                {
                    if (arr2[i] == 1)
                    {
                        sum++;
                    }
                }
            }
            double min = Math.Min(sum1, sum2);
            var hammDistance = (double)sum*100 / min ;
            return hammDistance;
            //Debug.WriteLine($"Hamming Distance = {hammDistance}");
        }

        [TestMethod]
        public void TestCreateRange()
        {
            List<double> result = CreateRange(0, 1, 0.3);
            foreach (double i in result)
            {
                Debug.WriteLine(i);
            }
        }

        [TestMethod]
        public List<double> CreateRange(double from, double to, double step)
        {
            List<double> result = new List<double>();
            double i = from;
            while (i <= to)
            {
                result.Add(i);
                i += step;
            }
            return result;
        }
        #region Private Helpers

        private static Parameters GetDefaultParams()
        {

            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 16);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 20.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.001);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            //parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
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
