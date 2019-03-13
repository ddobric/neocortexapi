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
        [TestMethod]
        public void StableOutputOnSameInputTest()
        {         
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32,32 });
            parameters.setColumnDimensions(new int[] { 64,64 });
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

            parameters.setInputDimensions(new int[] { 1000});
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

            parameters.setInputDimensions(new int[] { 32,32 });
            parameters.setColumnDimensions(new int[] { 32,32 });
            parameters.setNumActiveColumnsPerInhArea(0.2*32*32);
            parameters.setGlobalInhibition(false);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            int[] inputVector = ArrayUtils.ReadCsvFileTest("Testfiles\\digit8_binary_32bit.txt").ToArray();
            var inputString = Helpers.StringifyVector(inputVector);
            Debug.WriteLine("Input Array: " + inputString);
            //int[] inputVector = new int[] { 1, 0, 0, 0, 1, 1, 1, 0, 1, 1};
            int[] activeArray = new int[32*32];
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
        /// This test generate image of active column in sparse space and hamming distance between iterations of different image size and topology.
        /// @param mnistImage directory of original input image.
        /// </summary>
        [TestMethod]
        [DataRow("MnistTestImages\\digit7.png")]
        public void CalculateSpeedOfLearningTest(string mnistImage)
        {
            //define image size
            int[] imageSize = new int[] {128};
            int[] topology = new int[] {30};

            //Extract the name of image
            int index1 = mnistImage.IndexOf("\\") + 1;
            int index2 = mnistImage.IndexOf(".");
            string sub1 = mnistImage.Substring(0, index2);
            string sub2 = mnistImage.Substring(0, index1);
            string name = mnistImage.Substring(index1, sub1.Length - sub2.Length);
            string testName = null;
            string outputHamDistFile = $"Output\\{testName}.txt";
            string outputImage = $"Output\\{testName}.png";
            string inputBinaryImageFile = $"Output\\BinaryImages\\{testName}.txt";
            for (int i = 0; i < imageSize.Length; i++)
            {
                //binarize image to binarizedImage

                /* commented because we already had binary image
                Binarizer imageBinarizer = new Binarizer(200, 200, 200, imageSize[i], imageSize[i]);
                testName = name + "_" + imageSize[i];
                inputBinaryImageFile = $"Output\\BinaryImages\\{testName}.txt";
                imageBinarizer.CreateBinary(mnistImage, inputBinaryImageFile);
                */
                testName = name + "_" + imageSize[i];
                inputBinaryImageFile = $"Output\\BinaryImages\\{testName}.txt";
                for (int j = 0; j < topology.Length; j++)
                {

                    testName = name + "_" + imageSize[i] + "_Topology" + topology[j];
                    outputHamDistFile = $"Output\\{testName}.txt";
                    outputImage = $"Output\\{testName}.png";
                    
                    int numOfActCols = 0;

                    var sw = new Stopwatch();

                    // Load mnistImage from original file.

                    // Binarize image to specific sizes.
                    // foreach(32x32, 64x64, 128x128, 256,256, 1024x1024)..
                    // {
                    //      foreach(topology) = {10x10, 20x20, 30,30, 60x60, 100x100, 500x500}
                    //{
                    //      img = create bin image of size
                    //      compute
                    //      h = getHamDist()
                    //      writeHamDist
                    //      createPngfromActCols
                    // }
                    //}
                    using (StreamWriter stream = new StreamWriter(outputHamDistFile))
                    {
                        numOfActCols = topology[j] * topology[j];
                        var parameters = GetDefaultParams();

                        parameters.setInputDimensions(new int[] { imageSize[i], imageSize[i] });
                        parameters.setColumnDimensions(new int[] { topology[j], topology[j] });
                        parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);
                        var sp = new SpatialPooler();
                        var mem = new Connections();
                        parameters.apply(mem);
                        sw.Start();
                        sp.init(mem);
                        sw.Stop();

                        //stream.Write($"in=32x32, col=64x64 \tInit execution time={(double)sw.ElapsedMilliseconds / (double)1000} sec.");

                        int actiColLen = numOfActCols;
                        int[] activeArray = new int[actiColLen];
                        //Read input csv file into array
                        int[] inputVector = ArrayUtils.ReadCsvFileTest(inputBinaryImageFile).ToArray();
                        sw.Restart();
                        int iterations = 100;
                        int[] oldArray = new int[activeArray.Length];
                        for (int k = 0; k < iterations; k++)
                        {
                            sp.compute(mem, inputVector, activeArray, true);

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                            var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                            stream.WriteLine(distance + "\n");
                            var str = Helpers.StringifyVector(activeCols);
                            //Debug.WriteLine($"{distance} - {str}");
                            oldArray = new int[actiColLen];
                            activeArray.CopyTo(oldArray, 0);
                        }
                        var activeStr = Helpers.StringifyVector(activeArray);
                        stream.WriteLine("Active Array: " + activeStr);
                        //Debug.WriteLine("active Array:" + activeStr);
                        sw.Stop();
                        //stream.WriteLine($"Compute execution time per iteration ={(double)sw.ElapsedMilliseconds / (double)1000 / iterations} sec. Compute execution time={(double)sw.ElapsedMilliseconds / (double)1000} sec.");
                        NeoCortexUtils.DrawBitmap(activeArray, topology[j],topology[j], outputImage);
                    }
                }
            }
        }

        /// <summary>
        /// This test generate a text file of binary image from original image.The text file is 
        /// </summary>
        [TestMethod]
        [DataRow("TestFiles\\digit8.png")]
        public void BinarizeImageTest(String sourcePath, String destinationPath)
        {
            Binarizer imageBinarizer = new Binarizer(200,200,200,32,32);
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
                for (int j = 0; j < sub.Length-1; j++)
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
            parameters.Set(KEY.POTENTIAL_RADIUS,10);
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
