// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Daenet.ImageBinarizerLib;
using Daenet.ImageBinarizerLib.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System.Diagnostics;
using System.IO;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerInhibitionExperimentalTests
    {
        #region Test Scenario Methods

        /// <summary>
        /// Experiment Input Size / Column Dimension
        /// This test main purpose is to inspect the impact of input vector size and output column dimension
        /// on the performance of spatial pooler.
        /// This test compares spatial pooling performance in case of global inhibition and local inhibition.
        /// Save output hamming distance, active columns array/ .png image, and speed of processing in text files in OutputTopology directory.
        /// </summary>
        /// <param name="mnistImage"> Path to the source mnist image using in the test</param>
        /// <param name="imageSize"> Size of the image (image has same width and height)</param>
        /// <param name="columnDimension"> List of sparse space size.(with same width and height)</param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow(@"Testfiles\digit7.png", new int[] { 32, 48, 64 }, new int[] { 16, 24, 32, 40, 48, 54, 64 })]
        [DataRow(@"Testfiles\digit8.png", new int[] { 32, 48, 64 }, new int[] { 32, 64, 128 })]
        public void ExperimentInputSizeAndColumnDimension(string mnistImage, int[] imageSize, int[] columnDimension)
        {
            string name = GetFileName(mnistImage);
            string outputDirectory = $@"..\..\..\GlobalLocalInhibitionResult\Output\OutputTopology\{name}";
            Directory.CreateDirectory(outputDirectory);

            for (int currentImageIndex = 0; currentImageIndex < imageSize.Length; currentImageIndex++)
            {
                string nameMainPart = $"{name}_{imageSize[currentImageIndex]}";
                string outputSpeedFile = $@"{outputDirectory}\{nameMainPart}_speed.txt";
                string inputFile = BinarizeImage(mnistImage, imageSize[currentImageIndex]);

                for (int columnDimensionIndex = 0; columnDimensionIndex < columnDimension.Length; columnDimensionIndex++)
                {
                    for (int currentInhibition = 0; currentInhibition < 2; currentInhibition++)
                    {
                        bool isGlobalInhibition = (currentInhibition == 0) ? true : false;
                        string inhibition = isGlobalInhibition ? "Global" : "Local";
                        string finalName = $@"{outputDirectory}\{nameMainPart}_{columnDimension[columnDimensionIndex]}_{inhibition}";
                        string hammingFile = $"{finalName}_hamming.txt";
                        string activeColumnFile = $"{finalName}_active.txt";
                        string outputImage = $"{finalName}.png";

                        Training(imageSize[currentImageIndex], columnDimension[columnDimensionIndex], inputFile, hammingFile, outputSpeedFile, activeColumnFile, outputImage, isGlobalInhibition);
                    }
                }
            }
        }

        /// <summary>
        /// Experiment Max Boost / Duty Cycle period 
        /// This test purpose is to compare the performance change following parameters are adjusted:
        /// max boost and duty cycle period.
        /// This Test compares performance of global and local inhibition when using different value of
        /// parameter Max Boost and Duty Cycle period.
        /// </summary>
        /// <param name="mnistImage">Path to the source mnist image using in the test</param>
        /// <param name="imageSize">Size of the image (image has same width and height)</param>
        /// <param name="columnDimension">Number of columns = columnDimension*columnDimension. </param>
        /// <param name="maxBoost"> Setup max boost parameter  </param>
        /// <param name="dutyCyclePeriod"> Setup duty cycle period parameter </param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow(@"Testfiles\digit7.png", 32, 32, 1, 10)]
        [DataRow(@"Testfiles\digit7.png", 32, 32, 1, 100)]
        [DataRow(@"Testfiles\digit7.png", 32, 32, 1, 1000)]
        [DataRow(@"Testfiles\digit7.png", 32, 32, 10, 10)]
        [DataRow(@"Testfiles\digit7.png", 32, 32, 10, 100)]
        [DataRow(@"Testfiles\digit7.png", 32, 32, 10, 1000)]
        [DataRow(@"Testfiles\digit7.png", 32, 64, 100, 1000)]
        [DataRow(@"Testfiles\digit7.png", 32, 64, 100, 1000)]
        [DataRow(@"Testfiles\digit8.png", 32, 32, 100, 10)]
        public void ExperimentMaxBoostAndDutyCyclePeriod(string mnistImage, int imageSize, int columnDimension, double maxBoost, int dutyCyclePeriod)
        {
            string name = GetFileName(mnistImage);
            string outputDirectory = $@"..\..\..\GlobalLocalInhibitionResult\Output\OutputDutyCycle\{dutyCyclePeriod}";
            Directory.CreateDirectory(outputDirectory);
            string nameMainPart = $"{name}_{columnDimension}_{maxBoost:N0}";
            string outputSpeedFile = $@"{outputDirectory}\{nameMainPart}_speed.txt";
            string inputFile = BinarizeImage(mnistImage, imageSize);

            for (int currentInhibition = 0; currentInhibition < 2; currentInhibition++)
            {
                bool isGlobalInhibition = (currentInhibition == 0) ? true : false;
                string inhibition = isGlobalInhibition ? "Global" : "Local";
                string finalName = $@"{outputDirectory}\{nameMainPart}_{inhibition}";
                string hammingFile = $"{finalName}_hamming.txt";
                string outputImage = $"{finalName}.png";

                var parameters = SetupParameters(imageSize, columnDimension, maxBoost, dutyCyclePeriod, isGlobalInhibition);

                for (int numberOfTries = 0; numberOfTries < 10; numberOfTries++)
                {
                    Training(inputFile, hammingFile, outputSpeedFile, outputImage, parameters);
                    Debug.WriteLine($"Completed {numberOfTries + 1} time(s)");
                }
            }
        }

        /// <summary>
        /// Experiment Potential Radius
        /// This test purpose is to check if potential radius parameter have any effect on the speed of spatial pooler local inhibition algorithm.
        /// This Test is for local inhibition algorithm only.
        /// </summary>
        /// <param name="mnistImage">Path to the source mnist image using in the test</param>
        /// <param name="imageSize">Size of the input image (image has same width and height)</param>
        /// <param name="columnDimension">Column dimension. Number of columns = columnDimension*columnDimension.</param>
        /// <param name="potentialRadius">The percent of the inputs, within a column's potential radius, that a column can be connected to.</param>
        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow(@"Testfiles\digit7.png", 32, 64, new int[] { 4, 8, 12, 16 })]
        [DataRow(@"Testfiles\digit8.png", 64, 64, new int[] { 4, 8, 16, 20, 24, 28, 32 })]
        public void ExperimentPotentialRadius(string mnistImage, int imageSize, int columnDimension, int[] potentialRadius)
        {
            string name = GetFileName(mnistImage);
            string outputDirectory = $@"..\..\..\GlobalLocalInhibitionResult\Output\OutputPotentialRadius\{name}";
            Directory.CreateDirectory(outputDirectory);
            string nameMainPart = $@"{outputDirectory}\{name}_{columnDimension}";
            string outputSpeedFile = $@"{nameMainPart}_speed.txt";
            string inputFile = BinarizeImage(mnistImage, imageSize);

            for (int currentRadius = 0; currentRadius < potentialRadius.Length; currentRadius++)
            {
                bool isGlobalInhibition = false;
                string hammingFile = $"{nameMainPart}_{potentialRadius[currentRadius]}_hamming.txt";
                string outputImage = $"{nameMainPart}_{potentialRadius[currentRadius]}.png";

                var parameters = SetupParameters(imageSize, columnDimension, potentialRadius[currentRadius], isGlobalInhibition);

                for (int numberOfTries = 0; numberOfTries < 10; numberOfTries++)
                {
                    Training(inputFile, hammingFile, outputSpeedFile, outputImage, parameters);
                    Debug.WriteLine($"Completed {numberOfTries + 1} time(s)");
                }
            }
        }

        /// <summary>
        /// This is a short test to test spatial pooler with different parameters.
        /// The output prints the active column index (index of column with value = 1)
        /// This test does not write any output result files.
        /// </summary>
        /// <param name="inputBinarizedFile"> Input is a image file that already been binarized.</param>
        [TestMethod]
        [TestCategory("Experiment")]
        [DataRow(@"Testfiles\digit7.txt")]
        public void ExperimentTest(string inputBinarizedFile)
        {
            var parameters = SetupParameters(32, 64, 4096, true);

            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100000);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);

            var sp = new SpatialPooler();
            var mem = new Connections();

            int[] inputVector = NeoCortexUtils.ReadCsvFileTest(inputBinarizedFile).ToArray();
            int[] activeArray = new int[4096];
            parameters.apply(mem);
            sp.Init(mem);

            for (int i = 0; i < 1000; i++)
            {
                sp.compute(inputVector, activeArray, true);
                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                var str = Helpers.StringifyVector(activeCols);
                Debug.WriteLine(str);
            }
        }
        #endregion

        #region SpatialPooler training methods

        /// <summary>
        /// This function train the input image and write result to text files in folder @"/Output"
        /// The result text files include speed comparison between global inhibition and local inhibition,
        /// the stable of the out put array (by comparing hamming distance arrays).
        /// Finally this method draw an image of active column as .png file.
        /// </summary>
        /// <param name="imageSize">Size of the image (image has same width and height)</param>
        /// <param name="columnDimension">List of sparse space size.(with same width and height)</param>
        /// <param name="inputBinarizedFile">input image after binarized</param>
        /// <param name="hammingFile">Path to hamming distance output file </param>
        /// <param name="outputSpeedFile">Path to speed comparison output file</param>
        /// <param name="activeColumnFile">Path to active column after training output file (as array text)</param>
        /// <param name="outputImage">Path to active column after training output file (as .png image file)</param>
        /// <param name="isGlobalInhibition">is using Global inhibition algorithms or not (if false using local inhibition)</param>
        private static void Training(int imageSize, int columnDimension, string inputBinarizedFile, string hammingFile, string outputSpeedFile, string activeColumnFile, string outputImage, bool isGlobalInhibition)
        {
            int outputImageSize = 1024;
            int activeColumn = columnDimension * columnDimension;
            var stopwatch = new Stopwatch();
            using (StreamWriter swHamming = new StreamWriter(hammingFile))
            {
                using (StreamWriter swSpeed = new StreamWriter(outputSpeedFile, true))
                {
                    using (StreamWriter swActiveColumn = new StreamWriter(activeColumnFile))
                    {
                        var parameters = SetupParameters(imageSize, columnDimension, isGlobalInhibition);
                        var sp = new SpatialPooler();
                        var mem = new Connections();

                        parameters.apply(mem);

                        stopwatch.Start();
                        sp.Init(mem);
                        stopwatch.Stop();

                        int actiColumnLength = activeColumn;
                        int[] activeArray = new int[actiColumnLength];

                        // Read input csv file into array
                        int[] inputVector = NeoCortexUtils.ReadCsvFileTest(inputBinarizedFile).ToArray();

                        stopwatch.Restart();

                        int iterations = 300;
                        int[] oldArray = new int[activeArray.Length];

                        for (int k = 0; k < iterations; k++)
                        {
                            sp.compute(inputVector, activeArray, true);

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                            var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                            var similarity = MathHelpers.CalcArraySimilarity(oldArray, activeArray);
                            swHamming.WriteLine($"{distance} | {similarity}");
                            var str = Helpers.StringifyVector(activeCols);
                            Debug.WriteLine(str);
                            oldArray = new int[actiColumnLength];
                            activeArray.CopyTo(oldArray, 0);
                        }

                        stopwatch.Stop();
                        var activeArrayString = Helpers.StringifyVector(activeArray);
                        swActiveColumn.WriteLine("Active Array: " + activeArrayString);

                        string inhibition = isGlobalInhibition ? "Global" : "Local";
                        double milliseconds = (double)stopwatch.ElapsedMilliseconds;
                        double seconds = milliseconds / (double)1000;
                        swSpeed.WriteLine($"Topology: {columnDimension.ToString().PadRight(5)} | Inhibition type: {inhibition.PadRight(7)} | Total time: {milliseconds:N0} milliseconds ({seconds:N2} seconds).");

                        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, columnDimension, columnDimension);
                        twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                        NeoCortexUtils.DrawBitmap(twoDimenArray, outputImageSize, outputImageSize, outputImage);
                    }
                }
            }
        }

        /// <summary>
        /// This function train the input image and write result to text files in folder @"/OutputDutyCycle"
        /// The result text files include speed comparison between global inhibition and local inhibition,
        /// the stable of the out put array (by comparing hamming distance arrays).
        /// Finally this method draw an image of active column as .png file.
        /// This training method is used for testing speed of training with different value of max boost and duty cycle
        /// </summary>
        /// <param name="inputBinarizedFile">input image after binarized</param>
        /// <param name="hammingFile">Path to hamming distance output file</param>
        /// <param name="outputSpeedFile">Path to speed comparison output file</param>
        /// <param name="outputImage">Path to active column after training output file (as .png image file)</param>
        /// <param name="parameters">Parameter setup</param>
        private static void Training(string inputBinarizedFile, string hammingFile, string outputSpeedFile, string outputImage, Parameters parameters)
        {
            int outputImageSize = 1024;
            int topology = parameters.Get<int[]>(KEY.COLUMN_DIMENSIONS)[0];
            int activeColumn = topology * topology;
            var stopwatch = new Stopwatch();
            using (StreamWriter swHamming = new StreamWriter(hammingFile))
            {
                using (StreamWriter swSpeed = new StreamWriter(outputSpeedFile, true))
                {
                    var sp = new SpatialPooler();
                    var mem = new Connections();

                    parameters.apply(mem);

                    stopwatch.Start();
                    sp.Init(mem);
                    stopwatch.Stop();

                    int actiColumnLength = activeColumn;
                    int[] activeArray = new int[actiColumnLength];

                    // Read input csv file into array
                    int[] inputVector = NeoCortexUtils.ReadCsvFileTest(inputBinarizedFile).ToArray();

                    stopwatch.Restart();

                    int iterations = 1000;
                    int[] oldArray = new int[activeArray.Length];

                    for (int k = 0; k < iterations; k++)
                    {
                        sp.compute(inputVector, activeArray, true);

                        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                        var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                        var similarity = MathHelpers.CalcArraySimilarity(oldArray, activeArray);
                        swHamming.WriteLine($"{distance} | {similarity}");
                        var str = Helpers.StringifyVector(activeCols);
                        Debug.WriteLine(str);
                        oldArray = new int[actiColumnLength];
                        activeArray.CopyTo(oldArray, 0);
                    }

                    var activeArrayString = Helpers.StringifyVector(activeArray);

                    stopwatch.Stop();

                    Debug.WriteLine("Active Array: " + activeArrayString);

                    int potentialRadius = parameters.Get<int>(KEY.POTENTIAL_RADIUS);
                    bool isGlobalInhibition = parameters.Get<bool>(KEY.GLOBAL_INHIBITION);
                    string inhibition = isGlobalInhibition ? "Global" : "Local";
                    double milliseconds = (double)stopwatch.ElapsedMilliseconds;
                    double seconds = milliseconds / (double)1000;
                    swSpeed.WriteLine($"Column dimension: {topology.ToString().PadRight(5)} |Potential Radius: {potentialRadius}| Inhibition type: {inhibition.PadRight(7)} | Total time: {milliseconds:N0} milliseconds ({seconds:N2} seconds).");

                    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, topology, topology);
                    twoDimenArray = ArrayUtils.Transpose(twoDimenArray);

                    NeoCortexUtils.DrawBitmap(twoDimenArray, outputImageSize, outputImageSize, outputImage);
                }
            }
        }

        #endregion SpatialPooler training method

        #region Private helper methods

        /// <summary>
        /// Binarize image to create an binarized image and return the path to that binarized image.
        /// </summary>
        /// <param name="mnistImage"> Path to the source mnist image</param>
        /// <param name="imageSize">Size of the image (image has same width and height)</param>
        /// <returns>Path to the output binarized image</returns>
        private static string BinarizeImage(string mnistImage, int imageSize)
        {
            string binaryImage = $@"Output\{GetFileName(mnistImage)}.txt";
           
            ImageBinarizer imageBinarizer = new ImageBinarizer(new BinarizerParams { RedThreshold = 200, GreenThreshold = 200, BlueThreshold = 200, ImageWidth = imageSize, ImageHeight = imageSize, InputImagePath = mnistImage, OutputImagePath = binaryImage });

            Directory.CreateDirectory($"Output");

            if (File.Exists(binaryImage))
            {
                File.Delete(binaryImage);
            }

            imageBinarizer.Run();

            return binaryImage;
        }

        /// <summary>
        /// Method to get file name of the input image without the extension (use for naming output folders later)
        /// </summary>
        /// <param name="mnistImage">Path to the source mnist image (e.g. "TestFiles\digit7.png")</param>
        /// <returns>File name without extension</returns>
        private static string GetFileName(string mnistImage)
        {
            string path = Path.GetFullPath(mnistImage);
            path = Path.GetFileNameWithoutExtension(path);
            return path;
        }

        #endregion Private helper methods

        #region Parameter setup methods

        public static Parameters SetupDefaultParameters()
        {
            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 32, 32 });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 64, 64 });
            parameters.Set(KEY.POTENTIAL_RADIUS, 16);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 10.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.10);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.SEED, 42);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);
            return parameters;
        }

        private static Parameters SetupParameters(int imageSize, int topology, bool isGlobalInhibition)
        {
            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0); //less than 0 if parameter is unused
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);

            parameters.setInputDimensions(new int[] { imageSize, imageSize }); // input image
            parameters.setColumnDimensions(new int[] { topology, topology }); // column dimension
            parameters.setNumActiveColumnsPerInhArea(0.02 * topology * topology); // density of the active columns (depends on topology)
            parameters.setGlobalInhibition(isGlobalInhibition);
            parameters.setDutyCyclePeriod(100);
            parameters.setMaxBoost(10.0);
            return parameters;
        }

        private static Parameters SetupParameters(int imageSize, int columnDimension, double maxBoost,
            int dutyCyclePeriod, bool isGlobalInhibition)
        {
            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);

            parameters.setInputDimensions(new int[] { imageSize, imageSize });
            parameters.setColumnDimensions(new int[] { columnDimension, columnDimension });
            parameters.setNumActiveColumnsPerInhArea(0.02 * columnDimension * columnDimension);
            parameters.setGlobalInhibition(isGlobalInhibition);
            parameters.setDutyCyclePeriod(dutyCyclePeriod);
            parameters.setMaxBoost(maxBoost);
            return parameters;
        }

        private static Parameters SetupParameters(int imageSize, int columnDimension, int potentialRadius, bool isGlobalInhibition = false)
        {
            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);

            parameters.setPotentialRadius(potentialRadius);
            parameters.setInputDimensions(new int[] { imageSize, imageSize });
            parameters.setColumnDimensions(new int[] { columnDimension, columnDimension });
            parameters.setNumActiveColumnsPerInhArea(0.02 * columnDimension * columnDimension);
            parameters.setGlobalInhibition(isGlobalInhibition);
            parameters.setDutyCyclePeriod(1000);
            parameters.setMaxBoost(10);

            return parameters;
        }

        #endregion Parameter setup methods
    }
}
