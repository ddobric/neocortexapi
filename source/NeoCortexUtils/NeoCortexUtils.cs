// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Daenet.ImageBinarizerLib;
using Daenet.ImageBinarizerLib.Entities;
using SkiaSharp;
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
        /// Binarize image to the file with the test name.
        /// </summary>
        /// <param name="mnistImage"></param>
        /// <param name="imageSize"></param>
        /// <param name="testName"></param>
        /// <returns></returns>
        public static string BinarizeImage(string mnistImage, int imageSize, string testName)
        {
            string binaryImage;

            binaryImage = $"{testName}.txt";

            if (File.Exists(binaryImage))
                File.Delete(binaryImage);

            ImageBinarizer imageBinarizer = new ImageBinarizer(new BinarizerParams { RedThreshold = 200, GreenThreshold = 200, BlueThreshold = 200, ImageWidth = imageSize, ImageHeight = imageSize, InputImagePath = mnistImage, OutputImagePath = binaryImage });

            imageBinarizer.Run();

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
        /// Combines heatmap and normalized permanence representations into a single image with title.
        /// This Drwaitng Function is used to Visulalization of the Permanence Values.
        /// </summary>
        /// <param name="heatmapData">List of arrays representing the heatmap data.</param>
        /// <param name="normalizedData">List of arrays representing normalized data below the heatmap.</param>
        /// <param name="encodedData">List of arrays of original Encoded data encoded by the scaler encoder.</param>
        /// <param name="filePath">Output image path for saving the combined image.</param>
        /// <param name="bmpWidth">Width of the heatmap bitmap (default is 1024).</param>
        /// <param name="bmpHeight">Height of the heatmap bitmap (default is 1024).</param>
        /// <param name="redStart">Threshold for values above which pixels are red (default is 200).</param>
        /// <param name="yellowMiddle">Threshold for values between which pixels are yellow (default is 127).</param>
        /// <param name="greenStart">Threshold for values below which pixels are green (default is 20).</param>
        /// <param name="enlargementFactor">Factor by which the image is enlarged for better visualization (default is 4).</param>
        public static void Draw1dHeatmap(List<double[]> heatmapData, List<int[]> normalizedData, List<int[]> encodedData, String filePath,
        int bmpWidth = 1024,
        int bmpHeight = 1024,
        decimal redStart = 200, decimal yellowMiddle = 127, decimal greenStart = 20,
        int enlargementFactor = 4)
        {
            int height = heatmapData.Count;
            int maxLength = heatmapData.Max(arr => arr.Length);

            if (maxLength > bmpWidth || height > bmpHeight)
                throw new ArgumentException("Size of all included arrays must be less than specified 'bmpWidth' and 'bmpHeight'");

            // Calculate target width and height based on the enlargement factor
            int targetWidth = bmpWidth * enlargementFactor;
            // Include space for the title and labels
            int targetHeight = bmpHeight * enlargementFactor + 40;

            // Create a new bitmap for the heatmap and text row with background
            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(targetWidth, targetHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(myBitmap))
            {
                // Set the background color to LightSkyBlue
                g.Clear(Color.LightSkyBlue);

                // Draw title
                string title = "HeatMap Image";
                Font titleFont = new Font("Arial", 12);
                SizeF titleSize = g.MeasureString(title, titleFont);
                float titleX = (targetWidth - titleSize.Width) / 2;
                // Move the title further up (adjust the value as needed)
                float titleY = 0;
                g.DrawString(title, titleFont, Brushes.Black, new PointF(titleX, titleY));

                // Calculate scale factors for width and height based on the target dimensions
                var scaleX = (double)targetWidth / bmpWidth;
                // Exclude the space for the title and labels from scaleY
                var scaleY = (double)(targetHeight - 40) / bmpHeight;

                // Leave a gap between sections
                float labelY = 30;

                // Draw heatmap
                for (int i = 0; i < height; i++)
                {
                    var heatmapArr = heatmapData[i];

                    int w = heatmapArr.Length;

                    for (int Xcount = 0; Xcount < w; Xcount++)
                    {
                        for (int padX = 0; padX < scaleX; padX++)
                        {
                            for (int padY = 0; padY < scaleY; padY++)
                            {
                                myBitmap.SetPixel((int)(i * scaleX) + (int)(Xcount * scaleX) + padX, (int)(padY) + (int)labelY, GetColor(redStart, yellowMiddle, greenStart, (Decimal)heatmapArr[Xcount]));
                            }
                        }
                    }
                    // Draw normalized representation below the heatmap
                    using (var font = new Font("Arial", 12))
                    {
                        string normalizedLabel = "Normalized Permanence (Reconstructed Inputs)";
                        Font normalizedLabelFont = new Font("Arial", 10);
                        SizeF normalizedLabelSize = g.MeasureString(normalizedLabel, normalizedLabelFont);
                        float normalizedLabelX = (targetWidth - normalizedLabelSize.Width) / 2;
                        // Leave a gap before drawing the label
                        labelY += 130;
                        // Adjust the vertical position down by 10 units (you can modify this value)
                        labelY += 70;
                        g.DrawString(normalizedLabel, normalizedLabelFont, Brushes.Black, new PointF(normalizedLabelX, labelY));

                        var normalizedArr = normalizedData[i];
                        for (int Xcount = 0; Xcount < normalizedArr.Length; Xcount++)
                        {
                            // Format the integer as string
                            string formattedNumber = normalizedArr[Xcount].ToString();
                            // Adjusted position for top middle
                            float textX = (float)(i * scaleX) + (float)(Xcount * scaleX) + (float)(scaleX / 2) - 5;
                            // Adjusted vertical position for label
                            float textY = (float)(bmpHeight * scaleY) + 25;
                            g.DrawString(formattedNumber, font, Brushes.Black, new PointF(textX, textY));

                            // Draw a line from the top middle of the number to the corresponding heatmap pixel
                            // Adjusted starting point for the line
                            float lineStartX = textX + 5;
                            // Adjusted starting point for the line
                            float lineStartY = textY - 20;
                            float lineEndX = (float)(i * scaleX) + (float)(Xcount * scaleX) + (float)(scaleX / 2);
                            float lineEndY = 300;
                            g.DrawLine(Pens.Black, lineStartX, lineStartY, lineEndX, lineEndY);

                        }
                        // Draw the label for encoded values
                        string encodedLabel = "Encoded Inputs";
                        Font encodedLabelFont = new Font("Arial", 10);
                        SizeF encodedLabelSize = g.MeasureString(encodedLabel, encodedLabelFont);
                        float encodedLabelX = (targetWidth - encodedLabelSize.Width) / 2;
                        // Leave a gap before drawing the label
                        labelY = 120;
                        // Adjust the vertical position down by 10 units (you can modify this value)
                        labelY += -50;
                        g.DrawString(encodedLabel, encodedLabelFont, Brushes.Black, new PointF(encodedLabelX, labelY));

                        // Draw encoded values
                        var encodedArr = encodedData[i];
                        for (int Xcount = 0; Xcount < encodedArr.Length; Xcount++)
                        {
                            // Format the integer as string
                            string formattedNumber = encodedArr[Xcount].ToString();
                            // Adjusted position for top middle
                            float textX = (float)(i * scaleX) + (float)(Xcount * scaleX) + (float)(scaleX / 2) - 5;
                            float textY = 175; // Adjusted vertical position for label
                            g.DrawString(formattedNumber, font, Brushes.Black, new PointF(textX, textY));
                            // Draw a line from the top middle of the number to the corresponding heatmap pixel
                            // Adjusted starting point for the line
                            float lineStartX = textX + 5;
                            // Adjusted starting point for the line
                            float lineStartY = textY - 20;
                            float lineEndX = (float)(i * scaleX) + (float)(Xcount * scaleX) + (float)(scaleX / 2);
                            // Adjusted ending point for the line
                            float lineEndY = 100;
                            g.DrawLine(Pens.Black, lineStartX, lineStartY, lineEndX, lineEndY);
                        }
                    }
                }
            }

            // Save the combined image with heatmap and text row
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

                var scale = Math.Max(1, ((bmpWidth) / twoDimArrays.Count) / (w + 1));// +1 is for offset between pictures in X dim.

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

        /// <summary>
        /// Draws a combined similarity plot based on the given list of similarity values.
        /// This graph can Visulaze the Similarity Bar graph of multiple inputs between the Encoded inputs
        /// and the Reconsturced Inputs using Reconstruct Method.
        /// </summary>
        /// <param name="similarities">The list of similarity values to be plotted.</param>
        /// <param name="filePath">The file path where the plot image will be saved.</param>
        /// <param name="imageWidth">Width of the graph.</param>
        /// <param name="imageHeight">Height of the graph.</param>
        /// <remarks>
        /// The plot includes bars representing similarity values, indexed from left to right. Each bar's height corresponds to its similarity value.
        /// Axis labels, a title, a scale indicating similarity values, and text indicating the similarity range are added to the plot.
        /// </remarks>

        public static void DrawCombinedSimilarityPlot(List<double> similarities, string filePath, int imageWidth, int imageHeight)
        {
            // Create a new bitmap
            Bitmap bitmap = new Bitmap(imageWidth, imageHeight);

            // Create a graphics object from the bitmap
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // Clear the bitmap with a white background
                graphics.Clear(Color.White);

                // Define the maximum similarity value
                double maxSimilarity = similarities.Max();

                // Calculate the maximum bar height based on the plot height and scale
                // Adjusted for title position
                int maxBarHeight = imageHeight - 200;

                // Determine the number of bars
                int barCount = similarities.Count;

                // Calculate the total width occupied by bars and spacing
                // minimum bar width is 10 pixels
                int totalBarWidth = barCount * 10;
                //20 pixels of spacing between bars
                int totalSpacing = 20 * (barCount + 1);

                // Calculate the maximum available width for bars (excluding margins)
                // Adjusted for margins
                int maxAvailableWidth = imageWidth - totalSpacing - 200;

                // Calculate the bar width based on the available space and number of bars
                // Minimum width for each bar
                int minBarWidth = 20;
                int barWidth = Math.Max(minBarWidth, maxAvailableWidth / barCount);

                // Define the width of the scale
                int scaleWidth = 100;

                // Draw each bar
                for (int i = 0; i < barCount; i++)
                {
                    // Calculate the height of the bar based on the similarity value
                    int barHeight = (int)(similarities[i] / maxSimilarity * maxBarHeight);

                    // Determine the position and size of the bar
                    // Adjusted x position and spacing between bars
                    int x = scaleWidth + (i + 1) * 20 + i * barWidth;
                    // Adjusted for title position and space at the bottom for labels
                    int y = imageHeight - barHeight - 100;

                    // Draw the bar with a minimum width of 1 pixel to avoid disappearance
                    // Subtracting 1 to leave a small gap between bars
                    int w = Math.Max(1, barWidth - 1);

                    // Determine the color based on the similarity level
                    Color color = GetColorForSimilarity(similarities[i]);

                    // Create a solid brush with the determined color
                    Brush brush = new SolidBrush(color);

                    // Draw the bar
                    graphics.FillRectangle(brush, x, y, w, barHeight);

                    // Add labels for each bar
                    // Format the similarity value
                    string label = similarities[i].ToString("0.0");
                    Font font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
                    SizeF labelSize = graphics.MeasureString(label, font);
                    // Draw the label above the bar
                    graphics.DrawString(label, font, Brushes.Black, x + (barWidth - labelSize.Width) / 2, y - 20);
                    // Draw input label below the bar
                    graphics.DrawString($"{i + 1}", font, Brushes.Black, x + (barWidth - labelSize.Width) / 2, imageHeight - 50);
                }
                // Add axis labels
                Font axisFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
                graphics.DrawString("X - Axis (Input) Index", axisFont, Brushes.Black, scaleWidth + (imageWidth - scaleWidth) / 2, imageHeight - 20);
                // Add a title
                string title = "Similarity Graph";
                Font titleFont = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold);
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                // Adjusted title position
                graphics.DrawString(title, titleFont, Brushes.Black, (imageWidth - titleSize.Width) / 2, 20);

                // Add a scale indicating values from 0 to 1
                Font scaleFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
                // Draw 11 tick marks
                for (int i = 0; i <= 10; i++)
                {
                    double value = i / 10.0;
                    // Invert value and map to plot height
                    int y = (int)((1 - value) * maxBarHeight) + 100;
                    // Draw tick mark
                    graphics.DrawLine(Pens.Black, scaleWidth - 10, y, scaleWidth, y);
                    // Draw value label
                    graphics.DrawString(value.ToString("0.0"), scaleFont, Brushes.Black, 0, y - 8);
                }

                // Add text indicating the similarity test
                string similarityText = "Y axis-Similarity Range";
                // Larger and bold font for similarity text
                Font similarityFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
                SizeF similaritySize = graphics.MeasureString(similarityText, similarityFont);
                graphics.DrawString(similarityText, similarityFont, Brushes.Black, 50, imageHeight / 2 - similaritySize.Height / 2, new StringFormat { FormatFlags = StringFormatFlags.DirectionVertical });
            }

            // Save the bitmap to a file as PNG format
            bitmap.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// Determines the color based on the given similarity level.
        /// </summary>
        /// <param name="similarity">The similarity level to determine the color for (range: 0 to 1).</param>
        /// <returns>The color corresponding to the similarity level, ranging from light gray to dark orange.</returns>

        private static Color GetColorForSimilarity(double similarity)
        {
            // Define the color range
            // Light gray
            int minColorValue = 100;
            // Dark orange
            int maxColorValue = 255;

            // Map the similarity value to the color range
            int colorValue = (int)(minColorValue + (maxColorValue - minColorValue) * similarity);

            // Ensure the color value is within the valid range
            colorValue = Math.Max(minColorValue, Math.Min(maxColorValue, colorValue));

            // Create a color with the determined value
            // Orange gradient
            return Color.FromArgb(colorValue, colorValue / 2, 0);
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
        /// Determines the color of a bar based on the given similarity level.
        /// </summary>
        /// <param name="similarity">The similarity level to determine the color for.</param>
        /// <returns>The color corresponding to the similarity level.</returns>

        private static Color GetBarColor(double similarity)
        {
            // Assign color based on similarity level
            // High similarity (90% or higher)
            if (similarity >= 0.9)
                return Color.DarkOrange;
            // Medium similarity (70% or higher)
            else if (similarity >= 0.7)
                return Color.Orange;
            // Low similarity (50% or higher)
            else if (similarity >= 0.5)
                return Color.LightSalmon;
            // Very low similarity (below 50%)
            else
                return Color.LightGray;
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
            int[] inputVector = new int[widthBits * heightBits];

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


        /// <summary>
        /// Creates the dence array of permancences from sparse array.
        /// </summary>
        /// <param name="array">A dense array of permancences. Ever permanence value is a sum of permanence valus of
        /// active mini-columns to the input neuron with the given index.</param>
        /// <param name="numInputNeurons">Number of input neurons connected from mini-columns at the proximal segment.</param>
        private static double[] CreateDenseArray(Dictionary<int, double> array, int numInputNeurons)
        {
            // Creates the dense array of permanences.
            // Every permanence value for a single input neuron.
            double[] res = new double[numInputNeurons];

            for (int i = 0; i < numInputNeurons; i++)
            {
                if (array.ContainsKey(i))
                    res[i] = array[i];
                else
                    res[i] = 0.0;
            }

            return res;
        }


        /// <summary>
        /// Calculates the softmax function.
        /// </summary>
        /// <param name="sparseArray">The array if indicies of active mini-columns or cells.</param>
        /// <param name="numInputNeurons">The number of existing input neurons.</param>
        /// <returns></returns>
        public static double[] Softmax(Dictionary<int, double> sparseArray, int numInputNeurons)
        {
            var denseArr = CreateDenseArray(sparseArray, numInputNeurons);

            var res = Softmax(denseArr);

            return res;
        }


        /// <summary>
        /// Calculates the softmax of the input array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The softmax array.</returns>

        public static double[] Softmax(double[] input)
        {
            double[] exponentials = input.Select(x => Math.Exp(x)).ToArray();

            double sum = exponentials.Sum();

            return exponentials.Select(x => x / sum).ToArray();
        }
    }
}