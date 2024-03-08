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
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    [TestCategory("Experiment")]
    public class SpatialPoolerImiageSimilarityExperiments
    {
        private const int OutImgSize = 1024;

        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="inputPrefix"></param>
        [TestMethod]
        [TestCategory("LongRunning")]
        //[DataRow("digit7")]
        //[DataRow("digit5")]
        [DataRow("Vertical")]
        //[DataRow("Box")]
        //[DataRow("Horizontal")]
        public void ImageSimilarityExperiment(string inputPrefix)
        {
            //int stableStateCnt = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            //int inputBits = 100;
            var colDims = new int[] { 64, 64 };
            int numOfCols = 64 * 64;
            //int numColumns = colDims[0];

            string trainingFolder = "Similarity\\TestFiles";
            int imgSize = 28;
            //var colDims = new int[] { 64, 64 };
            //int numOfActCols = colDims[0] * colDims[1];

            string TestOutputFolder = $"Output-{nameof(ImageSimilarityExperiment)}";

            var trainingImages = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.png");

            Directory.CreateDirectory($"{nameof(ImageSimilarityExperiment)}");

            int counter = 0;
            //var parameters = GetDefaultParams();
            ////parameters.Set(KEY.DUTY_CYCLE_PERIOD, 20);
            ////parameters.Set(KEY.MAX_BOOST, 1);
            ////parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
            ////parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
            ////parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);
            //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * 4096); // TODO. Experiment with different sizes
            //parameters.Set(KEY.POTENTIAL_RADIUS, imgSize * imgSize);
            //parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            //parameters.Set(KEY.GLOBAL_INHIBITION, true); // TODO: Experiment with local inhibition too. Note also the execution time of the experiment.

            //// Num of active synapces in order to activate the column.
            //parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);
            //parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            //parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);

            //parameters.Set(KEY.INHIBITION_RADIUS, (int)0.02 * imgSize * imgSize); // TODO. check if this has influence in a case of the global inhibition. ALso check how this parameter influences the similarity of SDR.

            //parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);
            //parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            //parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            //parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
            //parameters.Set(KEY.MAX_BOOST, 100);
            //parameters.Set(KEY.WRAP_AROUND, true);
            //parameters.Set(KEY.SEED, 1969);
            //parameters.setInputDimensions(new int[] { imgSize, imgSize });
            //parameters.setColumnDimensions(colDims);

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { imgSize, imgSize });
            p.Set(KEY.COLUMN_DIMENSIONS, colDims);
            p.Set(KEY.CELLS_PER_COLUMN, 10);

            p.Set(KEY.MAX_BOOST, maxBoost);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 50);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minOctOverlapCycles);

            // Global inhibition
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.setNumActiveColumnsPerInhArea(0.02 * numOfCols);
            p.Set(KEY.POTENTIAL_RADIUS, (int)(0.8 * imgSize * imgSize));
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.
            //p.setInhibitionRadius( Automatically set on the columns pace in a case of global inhibition.);

            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numOfCols));

            bool isInStableState = false;

            var mem = new Connections();

            p.apply(mem);

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, trainingImages.Length * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                // Event should only be fired when entering the stable state.
                // Ideal SP should never enter unstable state after stable state.
                Assert.IsTrue(isStable);
                Assert.IsTrue(numPatterns == trainingImages.Length);
                isInStableState = true;
                Debug.WriteLine($"Entered STABLE state: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
            }, requiredSimilarityThreshold: 0.975);

            SpatialPooler sp = new SpatialPoolerMT(hpa);

            sp.Init(mem, UnitTestHelpers.GetMemory());

            string outFolder = $"{TestOutputFolder}\\{inputPrefix}";

            Directory.CreateDirectory(outFolder);

            string outputHamDistFile = $"{outFolder}\\digit{inputPrefix}_hamming.txt";

            string outputActColFile = $"{outFolder}\\digit{inputPrefix}_activeCol.txt";

            using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
            {
                using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                {
                    int cycle = 0;

                    Dictionary<string, int[]> sdrs = new Dictionary<string, int[]>();
                    Dictionary<string, int[]> inputVectors = new Dictionary<string, int[]>();

                    while (true)
                    {
                        foreach (var trainingImage in trainingImages)
                        {
                            int[] activeArray = new int[numOfCols];

                            FileInfo fI = new FileInfo(trainingImage);

                            string outputImage = $"{outFolder}\\{inputPrefix}_cycle_{counter}_{fI.Name}";

                            string testName = $"{outFolder}\\{inputPrefix}_{fI.Name}";

                            string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImage}", imgSize, testName);

                            // Read input csv file into array
                            int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                            int[] oldArray = new int[activeArray.Length];
                            List<double[,]> overlapArrays = new List<double[,]>();
                            List<double[,]> bostArrays = new List<double[,]>();

                            sp.compute(inputVector, activeArray, true);

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                            Debug.WriteLine($"Cycle: {cycle++} - Input: {trainingImage}");
                            Debug.WriteLine($"{Helpers.StringifyVector(activeCols)}\n");

                            if (isInStableState)
                            {
                                if (sdrs.Count == trainingImages.Length)
                                {
                                    CalculateSimilarity(sdrs, inputVectors);
                                    return;
                                }

                                var distance = MathHelpers.GetHammingDistance(oldArray, activeArray, true);
                                //var similarity = MathHelpers.CalcArraySimilarity(oldArray, activeArray, true);
                                sdrs.Add(trainingImage, activeCols);
                                inputVectors.Add(trainingImage, inputVector);

                                swHam.WriteLine($"{counter++}|{distance} ");

                                oldArray = new int[numOfCols];
                                activeArray.CopyTo(oldArray, 0);

                                overlapArrays.Add(ArrayUtils.Make2DArray<double>(ArrayUtils.ToDoubleArray(mem.Overlaps), colDims[0], colDims[1]));
                                bostArrays.Add(ArrayUtils.Make2DArray<double>(mem.BoostedOverlaps, colDims[0], colDims[1]));

                                var activeStr = Helpers.StringifyVector(activeArray);
                                swActCol.WriteLine("Active Array: " + activeStr);

                                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, colDims[0], colDims[1]);
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
        /// This is the original code that will be worked out in the thesis.
        /// https://github.com/UniversityOfAppliedSciencesFrankfurt/thesis-htm-imgclassification-dasu/blob/67d1b65e3d13cb28c5be196b87a24ad5a7ecaa47/ImageClassification/Image_Classification_Console/Classification.cs#L237
        /// This experimental code will be replaced.
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="digit"></param>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void SimilarityExperimentWithEncoder()
        {
            int stableStateCnt = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            int inputBits = 100;
            var colDims = new int[] { 64 * 64 };
            int numOfActCols = colDims[0];
            int numColumns = colDims[0];
            string TestOutputFolder = $"Output-{nameof(ImageSimilarityExperiment)}";

            Directory.CreateDirectory($"{nameof(ImageSimilarityExperiment)}");

            //int counter = 0;
            //var parameters = GetDefaultParams();
            ////parameters.Set(KEY.DUTY_CYCLE_PERIOD, 20);
            ////parameters.Set(KEY.MAX_BOOST, 1);
            ////parameters.setInputDimensions(new int[] { imageSize[imSizeIndx], imageSize[imSizeIndx] });
            ////parameters.setColumnDimensions(new int[] { topologies[topologyIndx], topologies[topologyIndx] });
            ////parameters.setNumActiveColumnsPerInhArea(0.02 * numOfActCols);
            //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.06 * 4096); // TODO. Experiment with different sizes
            //parameters.Set(KEY.POTENTIAL_RADIUS, inputBits);
            //parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            //parameters.Set(KEY.GLOBAL_INHIBITION, true); // TODO: Experiment with local inhibition too. Note also the execution time of the experiment.

            //// Num of active synapces in order to activate the column.
            //parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);
            //parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            //parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);

            //parameters.Set(KEY.INHIBITION_RADIUS, (int)0.15 * inputBits); // TODO. check if this has influence in a case of the global inhibition. ALso check how this parameter influences the similarity of SDR.

            //parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);
            //parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 1.0);
            //parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            //parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            //parameters.Set(KEY.MAX_BOOST, 10);
            //parameters.Set(KEY.WRAP_AROUND, true);
            //parameters.Set(KEY.SEED, 1969);
            //parameters.setInputDimensions(new int[] {inputBits });
            //parameters.setColumnDimensions(colDims);

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.COLUMN_DIMENSIONS, colDims);
            p.Set(KEY.CELLS_PER_COLUMN, 10);

            p.Set(KEY.MAX_BOOST, maxBoost);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 50);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minOctOverlapCycles);

            // Global inhibition
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            p.Set(KEY.POTENTIAL_RADIUS, (int)(.7 * inputBits));
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.
            //p.setInhibitionRadius( Automatically set on the columns pace in a case of global inhibition.);

            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));
            double max = 20;

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            var encoder = new ScalarEncoder(settings);

            bool isInStableState = false;

            var mem = new Connections();

            p.apply(mem);

            var inputs = new int[] { 0, 1, 2, 3, 4, 5 };

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputs.Length * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                // Event should only be fired when entering the stable state.
                // Ideal SP should never enter unstable state after stable state.
                Assert.IsTrue(isStable);
                //Assert.IsTrue(numPatterns == inputs.Length);
                isInStableState = true;
                Debug.WriteLine($"Entered STABLE state: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
            });

            SpatialPooler sp = new SpatialPoolerMT(hpa);

            sp.Init(mem, UnitTestHelpers.GetMemory());

            string outFolder = $"{TestOutputFolder}";

            Directory.CreateDirectory(outFolder);

            string outputHamDistFile = $"{outFolder}\\hamming.txt";

            string outputActColFile = $"{outFolder}\\activeCol.txt";

            using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
            {
                using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                {
                    int cycle = 0;

                    Dictionary<string, int[]> sdrs = new Dictionary<string, int[]>();

                    while (!isInStableState)
                    {
                        foreach (var digit in inputs)
                        {
                            int[] activeArray = new int[numOfActCols];
                            int[] oldArray = new int[activeArray.Length];
                            List<double[,]> overlapArrays = new List<double[,]>();
                            List<double[,]> bostArrays = new List<double[,]>();

                            var inputVector = encoder.Encode(digit);

                            sp.compute(inputVector, activeArray, true);

                            string actColFileName = Path.Combine(outFolder, $"{digit}.actcols.txt");

                            if (cycle == 0 && File.Exists(actColFileName))
                                File.Delete(actColFileName);

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                            using (StreamWriter swCols = new StreamWriter(actColFileName, true))
                            {
                                swCols.WriteLine(Helpers.StringifyVector(activeCols));
                            }

                            Debug.WriteLine($"'Cycle: {cycle} - {digit}'");
                            Debug.WriteLine($"IN :{Helpers.StringifyVector(inputVector)}");
                            Debug.WriteLine($"OUT:{Helpers.StringifyVector(activeCols)}\n");

                            if (isInStableState)
                            {
                                if (--stableStateCnt <= 0)
                                    return;
                            }
                            /*
                            if (isInStableState)
                            {
                                swActCol.WriteLine($"\nDigit {digit}");

                                sdrs.Add(digit.ToString(), activeCols);

                                // 
                                // To be sure that same input produces the same output after entered the stable state.
                                for (int i = 0; i < 100; i++)
                                {
                                    activeArray = new int[numOfActCols];

                                    sp.compute(inputVector, activeArray, true);
                                    
                                    var distance = MathHelpers.GetHammingDistance(oldArray, activeArray, true);
                                    
                                    var actColsIndxes = ArrayUtils.IndexWhere(activeArray, i => i == 1);
                                    var oldActColsIndxes = ArrayUtils.IndexWhere(oldArray, i => i == 1);

                                    var similarity = MathHelpers.CalcArraySimilarity(actColsIndxes, oldActColsIndxes);

                                    swHam.Write($"Digit {digit}: Dist/Similarity: {distance} | {similarity}\t");
                                    Debug.Write($"Digit {digit}: Dist/Similarity: {distance} | {similarity}\t");
                                    Debug.WriteLine($"{Helpers.StringifyVector(actColsIndxes)}");

                                    if (i > 5 && similarity < 100)
                                    { 
                                    
                                    }

                                    oldArray = new int[numOfActCols];
                                    activeArray.CopyTo(oldArray, 0);
                                }
                            }

                            Debug.WriteLine($"Cycle {cycle++}");*/
                        }

                        cycle++;
                    }

                    CalculateSimilarity(sdrs, null);//todo
                }
            }
        }


        /// <summary>
        /// Calculates the similarity matrix . 
        /// It takes all output SDRs and compares all of them.
        /// Save the correlation coefficient in csv file.
        /// </summary>
        /// <param name="sdrs">Dictionary of all output SDRs fro every input.</param>
        /// <param name="inputVectors">The dictionary of corresponding inputs.</param>        
        private void CalculateSimilarity(Dictionary<string, int[]> sdrs, Dictionary<string, int[]> inputVectors, string output = "Correlation.csv")
        {
            var keyArray = sdrs.Keys.ToArray();

            StreamWriter streamWriter = new StreamWriter(output);

            for (int i = 0; i < keyArray.Length; i++)
            {
                var key1 = keyArray[i];

                streamWriter.Write("," + key1);
            }

            for (int i = 0; i < keyArray.Length; i++)
            {
                var key1 = keyArray[i];
                streamWriter.WriteLine();
                streamWriter.Write(key1);
                for (int j = 0; j < keyArray.Length; j++)
                {
                    var key2 = keyArray[j];
                    int[] sdr1 = sdrs.GetValueOrDefault<string, int[]>(key1);
                    int[] sdr2 = sdrs.GetValueOrDefault<string, int[]>(key2);
                    
                    double outputSimilarity = MathHelpers.CalcArraySimilarity(sdr1, sdr2);

                    int[] inp1 = inputVectors.GetValueOrDefault<string, int[]>(key1);
                    int[] inp2 = inputVectors.GetValueOrDefault<string, int[]>(key2);
                    double inputSimilarity = MathHelpers.CalcArraySimilarity(inp1, inp2);

                    streamWriter.Write($" | {inputSimilarity.ToString("0.0")} {outputSimilarity.ToString("0.0")} ");                    
                }
            }

            streamWriter.Close();

            return;
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
