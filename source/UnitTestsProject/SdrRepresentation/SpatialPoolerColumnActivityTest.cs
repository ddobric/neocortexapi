using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace UnitTestsProject.Sdr
{
    /// <summary>
    /// Describe how to create SDRs
    /// </summary>
    [TestClass]
    public class SpatialPoolerColumnActivityTest
    {
        private const int OutImgSize = 1024;

        [TestMethod]
        public void CreateSdrsTest()
        {
            var colDims = new int[] { 64, 64 };
            int numOfCols = 64 * 64;

            string trainingFolder = @"..\..\..\TestFiles\Sdr";

            int imgSize = 28;

            var trainingImages = Directory.GetFiles(trainingFolder, "*.jpeg");

            Directory.CreateDirectory($"{nameof(CreateSdrsTest)}");

            int counter = 0;

            bool isInStableState = false;


            // HTM parameters
            HtmConfig htmConfig = new HtmConfig(new int[] { imgSize, imgSize }, new int[] { 64, 64 })
            {
                PotentialRadius = 10,
                PotentialPct = 1,
                GlobalInhibition = true,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 0.02 * numOfCols,
                StimulusThreshold = 0.0,
                SynPermInactiveDec = 0.008,
                SynPermActiveInc = 0.05,
                SynPermConnected = 0.10,
                MinPctOverlapDutyCycles = 1.0,
                MinPctActiveDutyCycles = 0.001,
                DutyCyclePeriod = 100,
                MaxBoost = 10.0,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42)

            };

            Connections connections = new Connections(htmConfig);

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(connections, trainingImages.Length * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                isInStableState = true;
                Debug.WriteLine($"Entered STABLE state: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
            });

            SpatialPooler sp = new SpatialPoolerMT(hpa);

            sp.Init(connections);

            string outFolder = nameof(CreateSdrsTest);
            Directory.CreateDirectory(outFolder);

            while (true)
            {
                counter++;

                Dictionary<string, int[]> sdrs = new Dictionary<string, int[]>();

                Dictionary<string, int[]> inputVectors = new Dictionary<string, int[]>();

                foreach (var trainingImage in trainingImages)
                {
                    FileInfo fI = new FileInfo(trainingImage);

                    string outputHamDistFile = $"{outFolder}\\image-{fI.Name}_hamming.txt";
                    string outputActColFile = $"{outFolder}\\image{fI.Name}_activeCol.txt";
                    string outputActColFile1 = $"{outFolder}\\image{fI.Name}_activeCol.csv";

                    using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                    {
                        using (StreamWriter swActCol1 = new StreamWriter(outputActColFile1))
                        {
                            int[] activeArray = new int[numOfCols];

                            string testName = $"{outFolder}\\{fI.Name}";

                            string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImage}", imgSize, testName);

                            // Read input csv file into array
                            int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                            List<double[,]> overlapArrays = new List<double[,]>();
                            List<double[,]> bostArrays = new List<double[,]>();

                            sp.compute(inputVector, activeArray, true);

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                            if (isInStableState)
                            {
                                CalculateResult(sdrs, inputVectors, numOfCols, activeCols, outFolder, trainingImage, inputVector);

                                overlapArrays.Add(ArrayUtils.Make2DArray<double>(ArrayUtils.ToDoubleArray(connections.Overlaps), colDims[0], colDims[1]));

                                bostArrays.Add(ArrayUtils.Make2DArray<double>(connections.BoostedOverlaps, colDims[0], colDims[1]));

                                var activeStr = Helpers.StringifyVector(activeArray);
                                swActCol.WriteLine("Active Array: " + activeStr);

                                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, colDims[0], colDims[1]);
                                twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
                                List<int[,]> arrays = new List<int[,]>();
                                arrays.Add(twoDimenArray);
                                arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length))));

                                //Calculating the max value of the overlap in the OverlapArray
                                int max = SdrRepresentation.TraceColumnsOverlap(overlapArrays, swActCol1, fI.Name);

                                int red = Convert.ToInt32(max * 0.80);        // Value above this threshould would be red and below this will be yellow 
                                int green = Convert.ToInt32(max * 0.50);      // Value above this threshould would be yellow and below this will be green

                                string outputImage = $"{outFolder}\\cycle-{counter}-{fI.Name}";

                                NeoCortexUtils.DrawBitmaps(arrays, outputImage, Color.Yellow, Color.Gray, OutImgSize, OutImgSize);
                                NeoCortexUtils.DrawHeatmaps(overlapArrays, $"{outputImage}_overlap.png", 1024, 1024, red, red, green);

                                if (sdrs.Count == trainingImages.Length)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }


        // <summary>
        /// Calculate all required results.
        /// 1. Overlap and Union of the Spatial Pooler SDRs of two Images as Input
        ///    It cross compares the 1st SDR with it self and all the Tranings Images.
        /// 2. Creates bitmaps of the overlaping and non-overlaping regions of the Comparing SDRs.
        /// 3. Also generate HeatMaps of the SDRs during Spatial Pooler learning Phase.
        /// </summary>
        /// <param name="sdrs"></param>
        private void CalculateResult(Dictionary<string, int[]> sdrs, Dictionary<string, int[]> inputVectors, int numOfCols, int[] activeCols, string outFolder, string trainingImage, int[] inputVector)
        {
            int[] CompareArray = new int[numOfCols];
            int[] ActiveArray = new int[numOfCols];

            ActiveArray = SdrRepresentation.GetIntArray(activeCols, 4096);

            sdrs.Add(trainingImage, activeCols);
            inputVectors.Add(trainingImage, inputVector);
            int[] FirstSDRArray = new int[81];
            if (sdrs.First().Key == null)
            {
                FirstSDRArray = new int[sdrs.First().Value.Length];

            }

            FirstSDRArray = sdrs.First().Value;

            CompareArray = SdrRepresentation.GetIntArray(FirstSDRArray, 4096);

            var Array = SdrRepresentation.OverlapArraFun(ActiveArray, CompareArray);
            int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
            int[,] twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
            NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{outFolder}\\Overlap_{sdrs.Count}.png", Color.PaleGreen, Color.Red, text: $"Overlap.png");

            Array = ActiveArray.Union(CompareArray).ToArray();
            int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
            int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);
            NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{outFolder}\\Union_{sdrs.Count}.png", Color.PaleGreen, Color.Green, text: $"Union.png");

            // Bitmap Intersection Image of two bit arrays selected for comparison
            SdrRepresentation.DrawIntersections(twoDimArray3, twoDimArray1, 10, $"{outFolder}\\Intersection_{sdrs.Count}.png", Color.Black, Color.Gray, text: $"Intersection.png");

            return;
        }

        /// <summary>
        /// Vaildate method <see cref="SdrRepresentation.OverlapArraFun(int[], int[])"/>
        /// </summary>
        [TestMethod]
        public void OverlapArraFunTest()
        {
            int[] a1 = new int[] { 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1 };
            int[] a2 = new int[] { 0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0 };

            Assert.ThrowsException<IndexOutOfRangeException>(() => SdrRepresentation.OverlapArraFun(a1, a2));
            var res = SdrRepresentation.OverlapArraFun(a2, a1);

            Assert.IsNotNull(res);
        }



    }


}