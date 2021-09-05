using GemBox.Spreadsheet;
using GemBox.Spreadsheet.Charts;
using GemBox.Spreadsheet.Drawing;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortex
{
    public class SdrRepresentation
    {

        /// <summary>
        /// Creates a vector which consists of common no of "1's" in two input vectors/SDRs
        /// </summary>
        /// <param arr1="1st array"></param>
        /// <param arr2="2nd array"></param>
        /// <returns></returns>
        public static int[] OverlapArraFun(int[] arr1, int[] arr2)
        {
            int[] nw = arr1;
            int[] old = arr2;
            int[] ovrlap = new int[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {

                if (nw[i] == 1 && old[i] == 1)
                {
                    ovrlap[i] = 1;
                }
                else
                {
                    ovrlap[i] = 0;
                }

            }
            return ovrlap;
        }

        /// <summary>
        /// Creates a vector which consists of all no of "1's" in two input vectors/SDRs
        /// </summary>
        /// <param arr1="1st array"></param>
        /// <param arr2="2nd array"></param>
        /// <returns></returns>
        public static int[] UnionArraFun(int[] arr1, int[] arr2)
        {
            int[] nw = arr1;
            int[] old = arr2;
            int[] un = new int[4096];
            for (int i = 0; i < arr1.Length; i++)
            {

                if ((nw[i] == 0 && old[i] == 1) || (nw[i] == 1 && old[i] == 0) || (nw[i] == 1 && old[i] == 1))
                {
                    un[i] = 1;
                }
                else
                {
                    un[i] = 0;
                }
            }
            return un;
        }


        /// <summary>
        /// Converts active columns array into to binary array 
        /// </summary>
        /// <param activeCols="active Columns of SDR"></param>
        /// <param numOfCols="size of the output vector"></param>
        /// <returns></returns>
        public static int[] GetIntArray(int[] activeCols, int numOfCols)
        {
            int[] a = new int[numOfCols];
            int c1 = 0;
            for (int i = 0; i < numOfCols; i++)
            {

                if (i == activeCols[c1])
                {
                    a[i] = 1;
                    c1++;
                }
                else
                {
                    a[i] = 0;
                }

                if (c1 == activeCols.Length)
                { break; }


            }

            return a;

        }




        /// <summary>
        /// shows active column in the output window
        /// </summary>
        /// <param cycle="no of cycles"></param>
        /// <param trainingImage="Input Image"></param>
        /// <param activeCols="active Columns of SDR"></param>
        /// <returns></returns>
        public static void TraceActiveColumns(int cycle, string trainingImage, int[] activeCols)
        {
            Debug.WriteLine($"Cycle: {cycle++} - Input: {trainingImage}");

            Debug.WriteLine($"ActiveColumn:{Helpers.StringifyVector(activeCols)}\n");

            // int index = 0;
            /*
            foreach (var vectorBit in activeCols)
            {

                Debug.WriteLine($"Column {index} / Overlap {vectorBit}");
                index++;
            }
            */

        }


        /// <summary>
        /// Show SDR in "Column/Overlap Ratio" and generate its Excel file 
        /// </summary>
        /// <param overlapArrays="Contains the overlaps of the columns during SDR learning of Spatial Pooler"></param>
        /// <param swActCol1="to write in Excel file"></param>
        /// <returns></returns>

        public static int TraceColumnsOverlap(List<double[,]> overlapArrays, StreamWriter swActCol1)
        {

            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            // Continue to use the component in a Trial mode when free limit is reached.
            SpreadsheetInfo.FreeLimitReached += (sender, e) => e.FreeLimitReachedAction = FreeLimitReachedAction.ContinueAsTrial;

            var workbook = new ExcelFile();
            var worksheet = workbook.Worksheets.Add("Chart");
            // var v = overlapArrays.ToArray();
            int max = 0;
            int index = 0;
            int in2 = 0;
            swActCol1.WriteLine($"Column / Overlaps");
            for (int c = 0; c < 1; c++)
            {
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        int s = (int)overlapArrays[c][i, j];


                        swActCol1.WriteLine($"Column: {index} / Overlaps: {s}");


                        //// Add data which will be used by the Excel chart.
                        worksheet.Cells["A1"].Value = "Column /";
                        worksheet.Cells["B1"].Value = "Overlap";
                        worksheet.Cells[in2, 0].SetValue(index);
                        worksheet.Cells[in2, 1].SetValue(s);


                        in2++;

                        if (s > max)
                            max = s;
                        index++;
                    }
                }
            }

            // Set header row and formatting.
            worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight;
            worksheet.Columns[0].Width = (int)LengthUnitConverter.Convert(3, LengthUnit.Centimeter, LengthUnit.ZeroCharacterWidth256thPart);

            // Make entire sheet print on a single page.
            worksheet.PrintOptions.FitWorksheetWidthToPages = 1;
            worksheet.PrintOptions.FitWorksheetHeightToPages = 1;

            // Create Excel chart and select data for it.
            var chart = worksheet.Charts.Add<LineChart>("D2", "P25");
            chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, in2, 1), true);

            // Define colors
            var backgroundColor = DrawingColor.FromName(DrawingColorName.RoyalBlue);
            var seriesColor = DrawingColor.FromName(DrawingColorName.Green);
            var textColor = DrawingColor.FromName(DrawingColorName.White);
            var borderColor = DrawingColor.FromName(DrawingColorName.Black);

            // Format chart
            chart.Fill.SetSolid(backgroundColor);

            var outline = chart.Outline;
            outline.Width = Length.From(2, LengthUnit.Point);
            outline.Fill.SetSolid(borderColor);

            // Format plot area
            chart.PlotArea.Fill.SetSolid(DrawingColor.FromName(DrawingColorName.White));

            outline = chart.PlotArea.Outline;
            outline.Width = Length.From(1.5, LengthUnit.Point);
            outline.Fill.SetSolid(borderColor);

            // Format chart title 
            var textFormat = chart.Title.TextFormat;
            textFormat.Size = Length.From(1, LengthUnit.Point);
            textFormat.Font = "Arial";
            textFormat.Font = "Arial";
            textFormat.Fill.SetSolid(textColor);

            // Format vertical axis
            textFormat = chart.Axes.Vertical.TextFormat;
            textFormat.Fill.SetSolid(textColor);
            textFormat.Italic = true;

            // Format horizontal axis
            textFormat = chart.Axes.Horizontal.TextFormat;
            textFormat.Fill.SetSolid(textColor);
            textFormat.Size = Length.From(1, LengthUnit.Point);
            textFormat.Bold = true;

            // Format vertical major gridlines
            chart.Axes.Vertical.MajorGridlines.Outline.Width = Length.From(1, LengthUnit.Point);

            workbook.Save(@"Column_Overlap_Chart.xlsx");

            return max;
        }

        /// <summary>
        /// Generate intersection between two SDRs
        /// </summary>
        /// <param twoDimArray="Overlap array of SDR"></param>
        /// <param woDimArray2="Union array of SDR"></param>
        /// <returns></returns>
        public static void DrawIntersections(int[,] twoDimArray, int[,] twoDimArray2, int scale, String filePath, Color inactiveCellColor, Color activeCellColor, string text = null)
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

                                if (twoDimArray[Xcount, Ycount] == 1 && twoDimArray2[Xcount, Ycount] == 1)
                                {
                                    myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.Red);
                                    k++;
                                }
                                else
                                {

                                    myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, activeCellColor);
                                    k++;
                                }



                            }
                            else
                            {

                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, inactiveCellColor);
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



    }
}

