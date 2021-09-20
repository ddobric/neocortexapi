// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using ImageBinarizer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace NeoCortex
{
    /// <summary>
    /// Set of helper methods.
    /// </summary>
    public class NeoCortexUtils
    {
        /// <summary>
        /// Binarize image to binarizedImage.
        /// </summary>
        /// <param name="mnistImage"></param>
        /// <param name="imageSize"></param>
        /// <param name="testName"></param>
        /// <returns></returns>
        public static string BinarizeImage(string mnistImage, int imageSize, string testName)
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
        /// Draws the bitmap from array of active columns.
        /// </summary>
        /// <param name="twoDimArray">Array of active columns.</param>
        /// <param name="width">Output width.</param>
        /// <param name="height">Output height.</param>
        /// <param name="filePath">The bitmap PNG filename.</param>
        /// <param name="text">Text to be written.</param>
        public static void DrawBitmap(int[,] twoDimArray, int width, int height, String filePath, string text = null)
        {
            DrawBitmap(twoDimArray, width, height, filePath, Color.Black, Color.Green, text);
        }

        /// <summary>
        /// Draws the bitmap from array of active columns.
        /// </summary>
        /// <param name="twoDimArray">Array of active columns.</param>
        /// <param name="width">Output width.</param>
        /// <param name="height">Output height.</param>
        /// <param name="filePath">The bitmap PNG filename.</param>
        /// <param name="inactiveCellColor"></param>
        /// <param name="activeCellColor"></param>
        /// <param name="text">Text to be written.</param>
        public static void DrawBitmap(int[,] twoDimArray, int width, int height, String filePath, Color inactiveCellColor, Color activeCellColor, string text = null)
        {
            int w = twoDimArray.GetLength(0);
            int h = twoDimArray.GetLength(1);

            if (w > width || h > height)
                throw new ArgumentException("Requested width/height must be greather than width/height inside of array.");

            var scale = width / w;

            if (scale * w < width)
                scale++;

            DrawBitmap(twoDimArray, scale, filePath, inactiveCellColor, activeCellColor, text);

        }

        /// <summary>
        /// Draws the bitmap from array of active columns.
        /// </summary>
        /// <param name="twoDimArray">Array of active columns.</param>
        /// <param name="scale">Scale of bitmap. If array of active columns is 10x10 and scale is 5 then output bitmap will be 50x50.</param>
        /// <param name="filePath">The bitmap filename.</param>
        /// <param name="activeCellColor"></param>
        /// <param name="inactiveCellColor"></param>
        /// <param name="text">Text to be written.</param>
        public static void DrawBitmap(int[,] twoDimArray, int scale, String filePath, Color inactiveCellColor, Color activeCellColor, string text = null)
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
                            if (twoDimArray[Xcount, Ycount] == 1)
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Yellow); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, activeCellColor); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Black); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, inactiveCellColor); // HERE IS YOUR LOGIC
                                k++;
                            }
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
        /// TODO: add comment
        /// </summary>
        /// <param name="twoDimArrays"></param>
        /// <param name="filePath"></param>
        /// <param name="bmpWidth"></param>
        /// <param name="bmpHeight"></param>
        public static void DrawBitmaps(List<int[,]> twoDimArrays, String filePath, int bmpWidth = 1024, int bmpHeight = 1024)
        {
            DrawBitmaps(twoDimArrays, filePath, Color.DarkGray, Color.Yellow, bmpWidth, bmpHeight);
        }


        /// <summary>
        /// Drawas bitmaps from list of arrays.
        /// </summary>
        /// <param name="twoDimArrays">List of arrays to be represented as bitmaps.</param>
        /// <param name="filePath">Output image path.</param>
        /// <param name="inactiveCellColor">Color of inactive bit.</param>
        /// <param name="activeCellColor">Color of active bit.</param>
        /// <param name="bmpWidth">The width of the bitmap.</param>
        /// <param name="bmpHeight">The height of the bitmap.</param>
        public static void DrawBitmaps(List<int[,]> twoDimArrays, String filePath, Color inactiveCellColor, Color activeCellColor, int bmpWidth = 1024, int bmpHeight = 1024)
        {
            int widthOfAll = 0, heightOfAll = 0;

            foreach (var arr in twoDimArrays)
            {
                widthOfAll += arr.GetLength(0);
                heightOfAll += arr.GetLength(1);
            }

            if (widthOfAll > bmpWidth || heightOfAll > bmpHeight)
                throw new ArgumentException("Size of all included arrays must be less than specified 'bmpWidth' and 'bmpHeight'");

            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(bmpWidth, bmpHeight);
            int k = 0;

            for (int n = 0; n < twoDimArrays.Count; n++)
            {
                var arr = twoDimArrays[n];

                int w = arr.GetLength(0);
                int h = arr.GetLength(1);

                var scale = ((bmpWidth) / twoDimArrays.Count) / (w + 1);// +1 is for offset between pictures in X dim.

                //if (scale * (w + 1) < (bmpWidth))
                //    scale++;

                for (int Xcount = 0; Xcount < w; Xcount++)
                {
                    for (int Ycount = 0; Ycount < h; Ycount++)
                    {
                        for (int padX = 0; padX < scale; padX++)
                        {
                            for (int padY = 0; padY < scale; padY++)
                            {
                                if (arr[Xcount, Ycount] == 1)
                                {
                                    myBitmap.SetPixel(n * (bmpWidth / twoDimArrays.Count) + Xcount * scale + padX, Ycount * scale + padY, activeCellColor); // HERE IS YOUR LOGIC
                                    k++;
                                }
                                else
                                {
                                    myBitmap.SetPixel(n * (bmpWidth / twoDimArrays.Count) + Xcount * scale + padX, Ycount * scale + padY, inactiveCellColor); // HERE IS YOUR LOGIC
                                    k++;
                                }
                            }
                        }
                    }
                }
            }

            myBitmap.Save(filePath, ImageFormat.Png);
        }


        /// <summary>
        /// Drawas bitmaps from list of arrays.
        /// </summary>
        /// <param name="twoDimArrays">List of arrays to be represented as bitmaps.</param>
        /// <param name="filePath">Output image path.</param>
        /// <param name="bmpWidth">The width of the bitmap.</param>
        /// <param name="bmpHeight">The height of the bitmap.</param>
        /// <param name="greenStart">ALl values below this value are by defaut green.
        /// Values higher than this value transform to yellow.</param>
        /// <param name="yellowMiddle">The middle of the heat. Values lower than this value transforms to green.
        /// Values higher than this value transforms to red.</param>
        /// <param name="redStart">Values higher than this value are by default red. Values lower than this value transform to yellow.</param>
        public static void DrawHeatmaps(List<double[,]> twoDimArrays, String filePath,
            int bmpWidth = 1024,
            int bmpHeight = 1024,
            decimal redStart = 200, decimal yellowMiddle = 127, decimal greenStart = 20)
        {
            int widthOfAll = 0, heightOfAll = 0;

            foreach (var arr in twoDimArrays)
            {
                widthOfAll += arr.GetLength(0);
                heightOfAll += arr.GetLength(1);
            }

            if (widthOfAll > bmpWidth || heightOfAll > bmpHeight)
                throw new ArgumentException("Size of all included arrays must be less than specified 'bmpWidth' and 'bmpHeight'");

            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(bmpWidth, bmpHeight);
            int k = 0;

            for (int n = 0; n < twoDimArrays.Count; n++)
            {
                var arr = twoDimArrays[n];

                int w = arr.GetLength(0);
                int h = arr.GetLength(1);

                var scale = ((bmpWidth) / twoDimArrays.Count) / (w + 1);// +1 is for offset between pictures in X dim.

                for (int Xcount = 0; Xcount < w; Xcount++)
                {
                    for (int Ycount = 0; Ycount < h; Ycount++)
                    {
                        for (int padX = 0; padX < scale; padX++)
                        {
                            for (int padY = 0; padY < scale; padY++)
                            {
                                myBitmap.SetPixel(n * (bmpWidth / twoDimArrays.Count) + Xcount * scale + padX, Ycount * scale + padY, GetColor(redStart, yellowMiddle, greenStart, (Decimal)arr[Xcount, Ycount]));
                                k++;
                            }
                        }
                    }
                }
            }

            myBitmap.Save(filePath, ImageFormat.Png);
        }



        private static Color GetColor(decimal redStartVal, decimal yellowStartVal, decimal greenStartVal, decimal val)
        {
            // color points
            int[] Red = new int[] { 255, 0, 0 }; //{ 252, 191, 123 }; // #FCBF7B
            int[] Yellow = new int[] { 254, 255, 132 }; // #FEEB84
            int[] Green = new int[] { 99, 190, 123 };  // #63BE7B
            //int[] Green = new int[] { 0, 0, 255 };  // #63BE7B
            int[] White = new int[] { 255, 255, 255 }; // #FFFFFF

            // value that corresponds to the color that represents the tier above the value - determined later
            Decimal highValue = 0.0M;
            // value that corresponds to the color that represents the tier below the value
            Decimal lowValue = 0.0M;
            // next higher and lower color tiers (set to corresponding member variable values)
            int[] highColor = null;
            int[] lowColor = null;

            // 3-integer array of color values (r,g,b) that will ultimately be converted to hex
            int[] rgb = null;


            // If value lower than green start value, it must be green.
            if (val <= greenStartVal)
            {
                rgb = Green;
            }
            // determine if value lower than the baseline of the red tier
            else if (val >= redStartVal)
            {
                rgb = Red;
            }

            // if not, then determine if value is between the red and yellow tiers
            else if (val > yellowStartVal)
            {
                highValue = redStartVal;
                lowValue = yellowStartVal;
                highColor = Red;
                lowColor = Yellow;
            }

            // if not, then determine if value is between the yellow and green tiers
            else if (val > greenStartVal)
            {
                highValue = yellowStartVal;
                lowValue = greenStartVal;
                highColor = Yellow;
                lowColor = Green;
            }
            // must be green
            else
            {
                rgb = Green;
            }

            // get correct color values for values between dark red and green
            if (rgb == null)
            {
                rgb = GetColorValues(highValue, lowValue, highColor, lowColor, val);
            }

            // return the hex string
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }

        private static int[] GetColorValues(decimal highBound, decimal lowBound, int[] highColor, int[] lowColor, decimal val)
        {

            // proportion the val is between the high and low bounds
            decimal ratio = (val - lowBound) / (highBound - lowBound);
            int[] rgb = new int[3];
            // step through each color and find the value that represents the approriate proportional value 
            // between the high and low colors
            for (int i = 0; i < 3; i++)
            {
                int hc = (int)highColor[i];
                int lc = (int)lowColor[i];
                // high color is lower than low color - reverse the subtracted vals
                bool reverse = hc < lc;

                reverse = false;

                // difference between the high and low values
                int diff = reverse ? lc - hc : hc - lc;
                // lowest value of the two
                int baseVal = reverse ? hc : lc;
                rgb[i] = (int)Math.Round((decimal)diff * ratio) + baseVal;
            }
            return rgb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<int> ReadCsvIntegers(String path)
        {
            string fileContent = File.ReadAllText(path);
            string[] integerStrings = fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> intList = new List<int>();
            for (int n = 0; n < integerStrings.Length; n++)
            {
                String s = integerStrings[n];
                char[] sub = s.ToCharArray();
                for (int j = 0; j < sub.Length; j++)
                {
                    intList.Add(int.Parse(sub[j].ToString()));
                }
            }
            return intList;
        }

        private static Random rnd = new Random(42);

        /// <summary>
        /// Creates the random vector.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="nonZeroPct"></param>
        /// <returns></returns>
        public static int[] CreateRandomVector(int bits, int nonZeroPct)
        {
            int[] inputVector = new int[bits];

            var nonZeroBits = (float)(nonZeroPct / 100.0) * bits;

            while (inputVector.Count(k => k == 1) < nonZeroBits)
            {
                inputVector[rnd.Next(bits)] = 1;
            }

            return inputVector;
        }

        /// <summary>
        /// Calculate mean value of array of numbers. 
        /// </summary>
        /// <param name="colData"> array of values </param>
        /// <returns>calculated mean</returns>
        public static double MeanOf(double[] colData)
        {
            if (colData == null || colData.Length < 2)
                throw new ArgumentException("'coldData' cannot be null or empty!");

            //calculate summ of the values
            double sum = 0;
            for (int i = 0; i < colData.Length; i++)
                sum += colData[i];

            //calculate mean
            double retVal = sum / colData.Length;

            return retVal;
        }

        /// <summary>
        /// Calculates Pearson correlation coefficient of two data sets
        /// </summary>
        /// <param name="data1"> first data set</param>
        /// <param name="data2">second data set </param>
        /// <returns></returns>
        public static double CorrelationPearson(double[] data1, double[] data2)
        {
            if (data1 == null || data1.Length < 2)
                throw new ArgumentException("'xData' cannot be null or empty!");

            if (data2 == null || data2.Length < 2)
                throw new ArgumentException("'yData' cannot be null or empty!");

            if (data1.Length != data2.Length)
                throw new ArgumentException("Both datasets must be of the same size!");

            //calculate average for each dataset
            double aav = MeanOf(data1);
            double bav = MeanOf(data2);

            double corr = 0;
            double ab = 0, aa = 0, bb = 0;
            for (int i = 0; i < data1.Length; i++)
            {
                var a = data1[i] - aav;
                var b = data2[i] - bav;

                ab += a * b;
                aa += a * a;
                bb += b * b;
            }

            corr = ab / Math.Sqrt(aa * bb);

            return corr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<int> ReadCsvFileTest(String path)
        {
            string fileContent = File.ReadAllText(path);
            string[] integerStrings = fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> intList = new List<int>();
            for (int n = 0; n < integerStrings.Length; n++)
            {
                String s = integerStrings[n];
                char[] sub = s.ToCharArray();
                for (int j = 0; j < sub.Length; j++)
                {
                    intList.Add(int.Parse(sub[j].ToString()));
                }
            }
            return intList;
        }


        /// <summary>
        /// Creates the 2D box vector.
        /// </summary>
        /// <param name="heightBits">The heght of the vector.</param>
        /// <param name="widthBits">The width of the vector.</param>
        /// <param name="nonzeroBitStart">Position of the first nonzero bit.</param>
        /// <param name="nonZeroBitEnd">Position of the last nonzero bit.</param>
        /// <returns>The two dimensional box.</returns>
        public static int[] Create2DVector(int widthBits, int heightBits, int nonzeroBitStart, int nonZeroBitEnd)
        {
            int[] inputVector = new int[widthBits*heightBits];

            for (int i = 0; i < widthBits; i++)
            {
                for (int j = 0; j < heightBits; j++)
                {
                    if (i > nonzeroBitStart && i < nonZeroBitEnd && j > nonzeroBitStart && j < nonZeroBitEnd)
                        inputVector[i * widthBits + j] = 1;
                    else
                        inputVector[i * 32 + j] = 0;
                }
            }

            return inputVector;
        }

        /// <summary>
        /// Creates the 1D vector.
        /// </summary>
        /// <param name="bits">The number of bits vector.</param>
        /// <param name="nonzeroBitStart">Position of the first nonzero bit.</param>
        /// <param name="nonZeroBitEnd">Position of the last nonzero bit.</param>
        /// <returns>The one dimensional vector.</returns>
        public static int[] CreateVector(int bits, int nonzeroBitStart, int nonZeroBitEnd)
        {
            int[] inputVector = new int[bits];

            for (int j = 0; j < bits; j++)
            {
                if (j > nonzeroBitStart && j < nonZeroBitEnd)
                    inputVector[j] = 1;
                else
                    inputVector[j] = 0;
            }

            return inputVector;
        }
    }
}
