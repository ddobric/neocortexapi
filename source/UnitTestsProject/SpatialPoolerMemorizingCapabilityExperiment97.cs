using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
/*
* Version 1.0.0:
* |DONE|
*    final commit, releasing 1st draft for testing
*    Hammingdim : Hammingdim^2 is the number of Elements/values/patterns used in the range
* |TODO|
*    Testing
*/

namespace UnitTestsProject
{
    /******************************************************************************
     *               _    _ _______ _____                                         *
     *              | |  | |__   __/ ____|                                        *
     *              | |__| |  | | | |  __ _ __ ___  _   _ _ __                    *
     *              |  __  |  | | | | |_ | '__/ _ \| | | | '_ \                   *
     *              | |  | |  | | | |__| | | | (_) | |_| | |_) |                  *
     *              |_|  |_|  |_|  \_____|_|  \___/ \__,_| .__/                   *
     *                                                   | |                      *
     *                                                   |_|                      *
     ******************************************************************************
     * Topic : Validating memorizing capabilities of Spatial Pooler               *
     * TASK:                                                                      *
     * Learning k different patterns/inputs N times                               *
     * TODO:                                                                      *
     * Making a Test that compare the learning scenarios between                  *
     *     CSI : Continuous Same Inputs feeding      N*L1 -> N*L2 -> ... -> N*Lk  *
     *     SPI : Sequential periodic Inputs feeding  N* ( L1 -> L2 -> ...-> Lk )  *
     *   https://raw.githubusercontent.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2019-2020/HTGroup/MyProject/MyAlgorithm/MemVal/SpatialPoolerValidatingMemorizingCapability.cs?token=AANM5R6PUGCPRFVJ3W3O3CS62TBA4                                                                         *
     *****************************************************************************/


    [TestClass]
    public class SpatialPoolerValidatingMemorizingCapability
    {

        private const int OutImgSize = 1024;
        /********************************************************************************************
         * File output directories */

        private readonly String outputFolder = "MemVal";
        private readonly String CSI = "CSI";
        private readonly String SPI = "SPI";
        private readonly String encoderOutputFolder = "encoderVal";
        private readonly String SDROuputFolder_SPI = "SDRoutVal_SPI";
        private readonly String SDROuputDiff_SPI = "SDROutputDiff_SPI";
        private readonly String FreshComputed_SPI = "FreshComputed_SPI";
        private readonly String SDROuputFolder_CSI = "SDRoutVal_CSI";
        private readonly String SDROuputDiff_CSI = "SDROutputDiff_CSI";
        private readonly String FreshComputed_CSI = "FreshComputed_CSI";
        private readonly String logOutputFolder = "logSDRcsv";
        //private readonly String mutualTest = "mutualTest";
        /*******************************************************************************************
         *                        _______ ______  _____ _______ _____                              *
         *                       |__   __|  ____|/ ____|__   __/ ____|                             *
         *                          | |  | |__  | (___    | | | (___                               *
         *                          | |  |  __|  \___ \   | |  \___ \                              *
         *                          | |  | |____ ____) |  | |  ____) |                             *
         *                          |_|  |______|_____/   |_| |_____/                              *
        *******************************************************************************************/
        [TestMethod]
        [TestCategory("LearnCSI")]
        [TestCategory("HTGroup")]
        [DataRow(1, 32, 15, 16)]
        public void LearnCSI(int iteration, int colDimSize, int inputDimSize, int Hammingdim)
        {
            FolderInit();
            Debug.WriteLine("** running learning SP with Continuous Same Inputs feeding");
            /**************************************************************************************
             * |INITIATION| |SPATIALPOOLER|
             * Setting the parameters variable 
             *changing the KEY's values  
             */
            var parameters = GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, inputDimSize * inputDimSize);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 5);//5
            parameters.Set(KEY.INHIBITION_RADIUS, (int)(0.01 * colDimSize * colDimSize));
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, (int)(0.02 * colDimSize * colDimSize));
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);//1000
            parameters.Set(KEY.MAX_BOOST, 10.0);//0
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.SEED, 42);

            parameters.setInputDimensions(new int[] { inputDimSize, inputDimSize });
            parameters.setColumnDimensions(new int[] { colDimSize, colDimSize });

            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            //var rnd = new Random();

            parameters.apply(mem);
            sp.Init(mem);
            /***************************************************************************************
             * |LOGICTEST| |CONTINOUS_PATTERN_LEARNING|
             * Using input from scalar encoder 
             */

            List<int[]> inputVectors;
            double[] inputDoubleArray = GetInputDoubleArray(-100, 100, Hammingdim * Hammingdim);
            inputVectors = ScalarEncoderDoubleArray(inputDoubleArray, inputDimSize, CSI);
            int vectorIndex = 0;
            Debug.WriteLine($"InputVectors.Count = {inputVectors.Count}");
            int[][] activeArray = new int[inputVectors.Count][];


            foreach (var inputVector in inputVectors)
            {
                activeArray[vectorIndex] = new int[colDimSize * colDimSize];

                for (int i = 0; i < iteration; i++)
                {
                    Debug.WriteLine(
                        $"compute VECTOR {vectorIndex} " +
                        $"iter: {i} " +
                        $"val: {NeoCortexApi.Helpers.StringifyVector(inputVector)} ");

                    sp.compute(inputVector, activeArray[vectorIndex], true);
                }
                sp.compute(inputVector, activeArray[vectorIndex], false);
                vectorIndex++;
            }
            vectorIndex = 0;
            foreach (var inputVector in inputVectors)
            {
                int[,] arrayDraw = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(activeArray[vectorIndex], colDimSize, colDimSize));

                NeoCortexUtils.DrawBitmap(arrayDraw,
                    OutImgSize, OutImgSize,
                    $"{outputFolder}\\{CSI}\\{SDROuputFolder_CSI}\\Vector_{vectorIndex}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(250, 100, 100),
                    $"No.{vectorIndex} val {inputDoubleArray[vectorIndex]}");
                vectorIndex++;
            }
            vectorIndex = 0;
            //************************drawing similarities*******************************************
            int[] dummyArray = new int[colDimSize * colDimSize];
            int[][] diffArray = new int[inputVectors.Count][];
            for (int i = 0; i < inputVectors.Count; i++)
            {
                sp.compute(inputVectors[i], dummyArray, false);
                int[,] drawArray = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(dummyArray, colDimSize, colDimSize));
                NeoCortexUtils.DrawBitmap(drawArray,
                    OutImgSize, OutImgSize,
                    $"{outputFolder}\\{CSI}\\{FreshComputed_CSI}\\Vector_{i}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(100, 250, 100),
                    $"No.{i} newCom, Val_{inputDoubleArray[i]}");
                diffArray[i] = AddArray(activeArray[i], dummyArray);
            }
            foreach (var inputVector in inputVectors)
            {
                int[,] arrayDraw = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(diffArray[vectorIndex], colDimSize, colDimSize));
                Debug.WriteLine($"Drawing SDRdiff index : {vectorIndex} .........");
                DrawdiffArray(arrayDraw,
                    OutImgSize / colDimSize,
                    $"{outputFolder}\\{CSI}\\{SDROuputDiff_CSI}\\Vector_{vectorIndex}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(100, 100, 250),
                    Color.FromArgb(250, 100, 100),
                    Color.FromArgb(100, 250, 100),
                    $"No.{vectorIndex} val {inputDoubleArray[vectorIndex]}");
                vectorIndex++;
            }
            //*********** Checking phase with getHammingDistance ************************************
            double[] hammingArray = GetHammingArray(inputVectors, activeArray, sp);
            LogToCSV(hammingArray, inputVectors, $"{CSI}\\{logOutputFolder}\\HammingOutput.csv");
            DrawHammingBitmap(
                ArrayUtils.Make2DArray<double>(hammingArray, Hammingdim, Hammingdim),
                50,
                $"{outputFolder}\\{CSI}\\{logOutputFolder}\\HammingBitmap.png");
        }
        //*******************************************************************************************
        [TestMethod]
        [TestCategory("LearnSPI")]
        [TestCategory("HTGroup")]
        [DataRow(1, 44, 64, 20)]
        public void LearnSPI(int iteration, int colDimSize, int inputDimSize, int Hammingdim)
        {
            FolderInit();
            Debug.WriteLine("** running learning SP with Sequential Periodic Inputs feeding **");
            /****************************************************************************************
             * |INITIATION| |SPATIALPOOLER|
             * setting the parameters variable 
             * changing the KEY's values  
             */
            var parameters = GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, inputDimSize * inputDimSize);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 5);//5
            parameters.Set(KEY.INHIBITION_RADIUS, (int)(0.01 * colDimSize * colDimSize));
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, (int)(0.02 * colDimSize * colDimSize));
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);//1000
            parameters.Set(KEY.MAX_BOOST, 10.0);//0
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.SEED, 42);

            parameters.setInputDimensions(new int[] { inputDimSize, inputDimSize });
            parameters.setColumnDimensions(new int[] { colDimSize, colDimSize });

            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            parameters.apply(mem);
            sp.Init(mem);
            /***************************************************************************************
             * |LOGICTEST| |SEQUENTIAL_PERIODIC_INPUT Learning| 
             * Using input from scalar encoder 
             */
            List<int[]> inputVectors;
            double[] inputDoubleArray = GetInputDoubleArray(-100, 100, Hammingdim * Hammingdim);
            inputVectors = ScalarEncoderDoubleArray(inputDoubleArray, inputDimSize, SPI);

            int vectorIndex = 0;

            Debug.WriteLine($"InputVectors.Count = {inputVectors.Count}");
            int[][] activeArray = new int[inputVectors.Count][];

            for (int i = 0; i < iteration; i++)
            {
                foreach (int[] inputVector in inputVectors)
                {
                    Debug.WriteLine(
                        $"compute VECTOR {vectorIndex} " +
                        $"val: {NeoCortexApi.Helpers.StringifyVector(inputVector)} " +
                        $"iter: {i}");
                    activeArray[vectorIndex] = new int[colDimSize * colDimSize];

                    sp.compute(inputVector, activeArray[vectorIndex], true);
                    if (i == iteration - 1)
                    {
                        sp.compute(inputVector, activeArray[vectorIndex], false);
                    }
                    vectorIndex++;
                }
                vectorIndex = 0;
            }
            vectorIndex = 0;
            foreach (var inputVector in inputVectors)
            {
                int[,] arrayDraw = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(activeArray[vectorIndex], colDimSize, colDimSize));
                NeoCortexUtils.DrawBitmap(arrayDraw,
                    OutImgSize, OutImgSize,
                    $"{outputFolder}\\{SPI}\\{SDROuputFolder_SPI}\\Vector_{vectorIndex}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(250, 100, 100),
                    $"No.{vectorIndex} val {inputDoubleArray[vectorIndex]}");
                vectorIndex++;
            }
            vectorIndex = 0;
            //************************drawing similarities*******************************************
            int[] dummyArray = new int[colDimSize * colDimSize];
            int[][] diffArray = new int[inputVectors.Count][];
            for (int i = 0; i < inputVectors.Count; i++)
            {
                sp.compute(inputVectors[i], dummyArray, false);
                int[,] drawArray = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(dummyArray, colDimSize, colDimSize));
                NeoCortexUtils.DrawBitmap(drawArray,
                    OutImgSize, OutImgSize,
                    $"{outputFolder}\\{SPI}\\{FreshComputed_SPI}\\Vector_{i}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(100, 250, 100),
                    $"No.{i} newCom, Val_{inputDoubleArray[i]}");
                diffArray[i] = AddArray(activeArray[i], dummyArray);
            }
            foreach (var inputVector in inputVectors)
            {
                int[,] arrayDraw = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(diffArray[vectorIndex], colDimSize, colDimSize));
                Debug.WriteLine($"Drawing SDRdiff index : {vectorIndex} .........");
                DrawdiffArray(arrayDraw,
                    OutImgSize / colDimSize,
                    $"{outputFolder}\\{SPI}\\{SDROuputDiff_SPI}\\Vector_{vectorIndex}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(100, 100, 250),
                    Color.FromArgb(100, 250, 100),
                    Color.FromArgb(250, 100, 100),
                    $"No.{vectorIndex} val {inputDoubleArray[vectorIndex]}");
                vectorIndex++;
            }
            //***********Checking phase**************************************************************
            double[] hammingArray = GetHammingArray(inputVectors, activeArray, sp);
            LogToCSV(hammingArray, inputVectors, $"{SPI}\\{logOutputFolder}\\HammingOutput.csv");
            DrawHammingBitmap(
                ArrayUtils.Make2DArray<double>(hammingArray, Hammingdim, Hammingdim),
                50,
                $"{outputFolder}\\{SPI}\\{logOutputFolder}\\HammingBitmap.png");
        }

        /*****************************************************************************
        *   _    _      _                   ______                _   _              *
        *  | |  | |    | |                 |  ____|              | | (_)             *
        *  | |__| | ___| |_ __   ___ _ __  | |__ _   _ _ __   ___| |_ _  ___  _ __   *
        *  |  __  |/ _ \ | '_ \ / _ \ '__| |  __| | | | '_ \ / __| __| |/ _ \| '_ \  *
        *  | |  | |  __/ | |_) |  __/ |    | |  | |_| | | | | (__| |_| | (_) | | | | *
        *  |_|  |_|\___|_| .__/ \___|_|    |_|   \__,_|_| |_|\___|\__|_|\___/|_| |_| *
        *                | |                                                         *
        *                |_|                                                         *
        *****************************************************************************/
        /// <summary>
        /// Default Parameters for initiating Spatial Pooler
        /// <br>code inherited from SpatialPoolerResearchTests/NoiseTest</br>
        /// </summary>
        /// <returns></returns>
        internal static Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 15);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 50.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 5);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);

            return parameters;
        }
        /// <summary>
        /// Create a List of encoded data as an array of 0 and 1
        /// <br>Scalar Encoder initiation,</br> 
        /// <br>logging the encoded data information to CSV file</br>
        /// <br>draw encoded data to bitmaps</br>
        /// </summary>
        /// <param name="inputDoubleArray"></param>
        /// <param name="inputDimSize">how many elements the ouput has</param>
        /// <returns></returns>
        internal List<int[]> ScalarEncoderDoubleArray(
            double[] inputDoubleArray,
            int inputDimSize,
            String testFolder)
        {
            List<int[]> arrays = new List<int[]>();
            int encodeDim = inputDimSize;
            CortexNetworkContext ctx = new CortexNetworkContext();
            Dictionary<string, object> encoderSettings = new Dictionary<string, object>
            {
                { "W", 25 },
                { "N", encodeDim * encodeDim },
                { "MinVal", inputDoubleArray.Min() - 0.01 },
                { "MaxVal", inputDoubleArray.Max() + 0.01 },
                { "Radius", (double)2.5 },
                { "Resolution", (double)0 },
                { "Periodic", (bool)false },
                { "ClipInput", (bool)true },
                { "Name", "TestScalarEncoder" },
                { "IsRealCortexModel", true }
            };

            ScalarEncoder encoder = new ScalarEncoder(encoderSettings);
            for (int i = 0; i < inputDoubleArray.Length; i++)
            {
                Debug.WriteLine($"Output Dimension of the Scalar Encoder : {encoder.N}");
                var result = encoder.Encode(inputDoubleArray[i]);
                Debug.WriteLine($"the input value is {inputDoubleArray[i]}");
                Debug.WriteLine($"resulting SDR: {NeoCortexApi.Helpers.StringifyVector(result)}");
                arrays.Add(result);
                int[,] arrayToDraw = ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(result, encodeDim, encodeDim));
                NeoCortexUtils.DrawBitmap(
                    arrayToDraw,
                    OutImgSize, OutImgSize,
                    $"{outputFolder}\\{testFolder}\\{encoderOutputFolder}\\encodedValnumber_{i}.png",
                    Color.FromArgb(44, 44, 44),
                    Color.FromArgb(200, 200, 250),
                    $"encodedVal of {inputDoubleArray[i]}");
            }
            LogToCSV(inputDoubleArray, arrays, $"{testFolder}\\{logOutputFolder}\\scalarEncoderOutput.csv");
            return arrays;
        }
        /// <summary>
        /// Creation of a double array
        /// <br>More double array data can be test using this function as base</br>
        /// <para>Go to function definition to change implemetation</para>
        /// </summary>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <param name="n">numbers of values</param>
        /// <returns></returns>
        internal double[] GetInputDoubleArray(
            double minVal,
            double maxVal,
            int n)
        {
            double[] doubleArray = new double[n];
            for (int i = 0; i < n; i++)
            {
                doubleArray[i] = Math.Round((maxVal - minVal) / n * i + minVal, 2, MidpointRounding.AwayFromZero);
            }
            /*
             * generate a random number of doubles 
             * in the range from minVal to maxVal
             * with length n 
             */
            //Debug.WriteLine($"the double Array is :{NeoCortexApi.Helpers.StringifyVector(doubleArray)}");
            return doubleArray;
        }
        /// <summary>
        /// writing the inputs and the encoded SDR values into a csv files
        /// <br>current implementation is only for Scalar encoder with double values</br>.
        /// </summary>
        /// <param name="inputDoubleArray">double[] array from input</param>
        /// <param name="SDRarrays">int[] SDR output array from the scalr encoder</param>
        /// <param name="fileName">the path to folder, create under memVal/ -- see variable ouputFolder. </param>
        internal void LogToCSV(
            double[] inputDoubleArray,
            List<int[]> SDRarrays,
            String fileName)
        {
            string path = $"{outputFolder}\\{fileName}";
            TextWriter tw = new StreamWriter(path);
            for (int i = 0; i < inputDoubleArray.Length; i++)
            {
                tw.WriteLine($"{inputDoubleArray[i]},{NeoCortexApi.Helpers.StringifyVector(SDRarrays[i])}");
            }
            tw.Close();
        }
        /// <summary>
        /// 
        /// Draw bitmap based on difference/similarity from one computed SDR to all other SDR values, 
        /// <br>the more the mutual active collumns, the higher the value from GetHammingDistance(), the more bias the pixel's color to activeCellColor</br>
        /// </summary>
        /// <remarks>
        /// not yet inplemented, change xml comment upon implementation
        /// </remarks>
        /// <param name="twoDimArray">input array to be drawn, current implemntation prefered square</param>
        /// <param name="scale">how big one cell is, Ex: 5x5 array with scale=10 will give a 50x50 pixel size bitmap</param>
        /// <param name="filePath"></param>
        /// <param name="inactiveCellColor">difference bias</param>
        /// <param name="activeCellColor">similarities bias</param>
        /// <param name="text">text appears in the bitmap</param>
        public static void DrawHammingBitmap(
            double[,] twoDimArray,
            int scale,
            String filePath,
            string text = null)
        {

            int w = twoDimArray.GetLength(0);
            int h = twoDimArray.GetLength(1);
            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(w * scale, h * scale);
            int k = 0;
            for (int Xcount = 0; Xcount < w; Xcount++)
            {
                for (int Ycount = 0; Ycount < h; Ycount++)
                {
                    int gain = (int)(255 * twoDimArray[Xcount, Ycount] / 100);
                    Debug.WriteLine($"cell {Xcount},{Ycount} gain : {gain}");
                    for (int padX = 0; padX < scale; padX++)
                    {
                        for (int padY = 0; padY < scale; padY++)
                        {
                            myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.FromArgb(gain, gain, gain));
                            k++;
                        }
                    }
                }
            }

            Graphics g = Graphics.FromImage(myBitmap);
            var fontFamily = new FontFamily(System.Drawing.Text.GenericFontFamilies.SansSerif);
            g.DrawString(text, new Font(fontFamily, 32), SystemBrushes.Control, new PointF(0, 0));

            myBitmap.Save(filePath, ImageFormat.Png);
        }
        /// <summary>
        /// draw the difference array during the learning of the spatial pooler
        /// <br>based on the value of the elements</br>
        /// <para>see --> AddArray()</para>
        /// </summary>
        /// <param name="twoDimArray"> diffArray</param>
        /// <param name="scale">length of one square pixel</param>
        /// <param name="filePath"></param>
        /// <param name="sameColor0">Value 0</param>
        /// <param name="sameColor1">Value 3</param>
        /// <param name="diffColorfrom1">Value 1</param>
        /// <param name="diffColorfrom2">Value 2</param>
        /// <param name="text">included text in the bitmap</param>
        public static void DrawdiffArray(
            int[,] twoDimArray,
            int scale,
            String filePath,
            Color sameColor0,
            Color sameColor1,
            Color diffColorfrom1,
            Color diffColorfrom2,
            string text = null)
        {
            int w = twoDimArray.GetLength(0);
            int h = twoDimArray.GetLength(1);

            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(w * scale, h * scale);
            int k = 0;
            for (int Xcount = 0; Xcount < w; Xcount++)
            {
                for (int Ycount = 0; Ycount < h; Ycount++)
                {
                    for (int padX = 0; padX < scale; padX++)
                    {
                        for (int padY = 0; padY < scale; padY++)
                        {
                            // If '1' in the first array.
                            if (twoDimArray[Xcount, Ycount] == 1)
                            {
                                //Coloring the pixel in case its value is 1
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, diffColorfrom1);
                                k++;
                            }
                            // If '1' in second array
                            else if (twoDimArray[Xcount, Ycount] == 2)
                            {
                                //Coloring the pixel in case its value is 2
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, diffColorfrom2);
                                k++;
                            }
                            // If same value in both arrays = overlap.
                            else if (twoDimArray[Xcount, Ycount] == 3)
                            {
                                //Coloring the pixel in case its value is 3
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, sameColor1);
                                k++;
                            }
                            else
                            {
                                //Coloring the pixel in case its value is 0
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, sameColor0);
                                k++;
                            }
                        }
                    }
                }
            }
            //inserting the text
            Graphics g = Graphics.FromImage(myBitmap);
            var fontFamily = new FontFamily(System.Drawing.Text.GenericFontFamilies.SansSerif);
            g.DrawString(text, new Font(fontFamily, 32), SystemBrushes.Control, new PointF(0, 0));

            myBitmap.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// Adding 2 original and newly computed array
        /// <para>
        /// difference array = original_recorded_array*2(value) + newly_computed_array*1
        /// <br>this gives the following values</br>
        /// <br>0 : same 0</br>
        /// <br>1 : newly_computed_array 's 1</br>
        /// <br>2 : original_recorded_array 's 1</br>
        /// <br>3 : same 1</br>
        /// </para>
        /// </summary>
        /// <param name="v">original computed SDR</param>
        /// <param name="dummyArray">newly computed SDR</param>
        /// <returns> computed diffArray </returns>
        private int[] AddArray(int[] v, int[] dummyArray)
        {
            if (v.Length != dummyArray.Length)
            {
                return new int[] { 404 };
            }
            else
            {
                int[] result = new int[v.Length];
                for (int i = 0; i < v.Length; i++)
                {
                    /*
                     */
                    result[i] = 2 * v[i] + dummyArray[i];
                }
                return result;
            }
        }
        internal List<int[]> SliceArray(List<int[]> inpArray, int startIndex, int endIndex)
        {
            List<int[]> retArray = new List<int[]>();
            for (int i = startIndex; i < endIndex; i++)
            {
                retArray.Add(inpArray[i]);
            }
            return retArray;
        }
        /// <summary>
        /// Initiating the ouput folder for the test,
        /// **must** be run prior to the code,
        /// <para>Test may crash if there are no directory</para>
        /// </summary>
        internal void FolderInit()
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            if (!Directory.Exists($"{outputFolder}\\{SPI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{SPI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{CSI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{CSI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{SPI}\\{encoderOutputFolder}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{SPI}\\{encoderOutputFolder}");
            }
            if (!Directory.Exists($"{outputFolder}\\{CSI}\\{encoderOutputFolder}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{CSI}\\{encoderOutputFolder}");
            }
            if (!Directory.Exists($"{outputFolder}\\{SPI}\\{SDROuputFolder_SPI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{SPI}\\{SDROuputFolder_SPI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{CSI}\\{SDROuputFolder_CSI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{CSI}\\{SDROuputFolder_CSI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{SPI}\\{SDROuputDiff_SPI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{SPI}\\{SDROuputDiff_SPI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{CSI}\\{SDROuputDiff_CSI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{CSI}\\{SDROuputDiff_CSI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{SPI}\\{logOutputFolder}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{SPI}\\{logOutputFolder}");
            }
            if (!Directory.Exists($"{outputFolder}\\{CSI}\\{logOutputFolder}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{CSI}\\{logOutputFolder}");
            }
            if (!Directory.Exists($"{outputFolder}\\{SPI}\\{FreshComputed_SPI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{SPI}\\{FreshComputed_SPI}");
            }
            if (!Directory.Exists($"{outputFolder}\\{CSI}\\{FreshComputed_CSI}"))
            {
                Directory.CreateDirectory($"{outputFolder}\\{CSI}\\{FreshComputed_CSI}");
            }
        }
        /// <summary>
        /// ouput an Array Percentage of different between the recorded SDR(activeArray) 
        /// <br>and the recent calculated SDR(from inputVectors and current sp)</br>
        /// </summary>
        /// <param name="inputVectors">original encoded data</param>
        /// <param name="activeArray">recorded array from learning</param>
        /// <param name="sp">the ref to the current spatial pooler</param>
        /// <returns></returns>
        internal double[] GetHammingArray(List<int[]> inputVectors, int[][] activeArray, SpatialPoolerMT sp)
        {
            int[] dummyArray = new int[activeArray[0].Length];
            double[] hammingArray = new double[inputVectors.Count];
            int matched = 0;
            double matchPercentage = 90;
            Debug.WriteLine($"hamming Array:");
            for (int i = 0; i < inputVectors.Count; i++)
            {
                sp.compute(inputVectors[i], dummyArray, false);
                Debug.WriteLine($"activeArray{i}   = {NeoCortexApi.Helpers.StringifyVector(activeArray[i])}");
                Debug.WriteLine($"computedArray{i} = {NeoCortexApi.Helpers.StringifyVector(dummyArray)}");
                hammingArray[i] = MathHelpers.GetHammingDistance(activeArray[i], dummyArray, true);
                if (hammingArray[i] > matchPercentage) matched++;
                Debug.WriteLine($"Hamming Array{i} : {hammingArray[i]}\n" +
                    $"Hamming Distance: { hammingArray[i]} ");

                Debug.WriteLine($"Input {i} = {hammingArray[i]}");
            }

            Debug.WriteLine($"matched over {matchPercentage}: {matched}/{inputVectors.Count}");

            return hammingArray;
        }
    }
}