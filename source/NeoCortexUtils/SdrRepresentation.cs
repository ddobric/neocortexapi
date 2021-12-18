using GemBox.Spreadsheet;
using GemBox.Spreadsheet.Charts;
using GemBox.Spreadsheet.Drawing;
//using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NeoCortex
{
    /// <summary>
    /// Sparse distributed representations 
    /// </summary>
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
            // TODO: why do we need to assign arr1 and arr2 to new array???

            int[] nw = arr1;
            int[] old = arr2;
            int[] ovrlap = new int[arr1.Length]; //Math.Min(arr1.Length, arr2.Length);
            for (int i = 0; i < arr1.Length; i++)
            {
                //
                // TODO: what happen if arr1 has more elements than arr2: throw IndexOutOfRangeException
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
        /// <returns></returns>
        public static int[] ArrayUnion(List<int[]> arrList)
        {
            // The length of the Union array is the length of the maximum length of array in the list
            int arrayLengthUnion = 0;
            
            foreach(int[] arr in arrList)
            {
                if (arr.Length > arrayLengthUnion)
                {
                    arrayLengthUnion = arr.Length;
                }
            }
            int[] unionArray = new int[arrayLengthUnion];
            foreach (int[] arr in arrList)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    unionArray[i] = ((unionArray[i] + arr[i]) >= 1) ? 1 : 0;
                }
            }
            return unionArray;
        }


        /// <summary>
        /// Converts active columns index array into to binary array 
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
        /// Show SDR in "Column/Overlap Ratio" and generate its Excel file 
        /// </summary>
        /// <param overlapArrays="Contains the overlaps of the columns during SDR learning of Spatial Pooler"></param>
        /// <param swActCol1="to write in Excel file"></param>
        /// <returns></returns>

        public static int TraceColumnsOverlap(List<double[,]> overlapArrays, StreamWriter swActCol1, string testName)
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

                        //
                        // Add data which will be used by the Excel chart.
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

            workbook.Save($"{testName}.xlsx");

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

        // To trace out the column Overlap values in Excel, Line trace and Graph format
        /// <summary>
        /// <br>values: Text, Excel or Graph</br>
        /// </summary>
        public enum TraceFormat
        {
            Text, 
            Excel, 
            Graph,
        }
        /// <summary>
        /// <br>Choice function for ease of switching between the modes</br>
        /// <br>The functions called inside can also be called independently</br>
        /// <br>TraceColumnsOverlap()</br>
        /// <br>TraceInLineFormat()</br>
        /// <br>TraceInGraphFormat()</br>
        /// </summary>
        /// <param name="overlapArrays"></param>
        /// <param name="colDims"></param>
        /// <param name="formatTypes"></param>
        /// <param name="threshold"></param>
        /// <param name="aboveThreshold"></param>
        /// <param name="belowThreshold"></param>
        public static void TraceColumnsOverlap(List<double[,]> overlapArrays, int[] colDims, TraceFormat formatTypes, double threshold, Color aboveThreshold, Color belowThreshold)  //To Trace out the Columns /Overlap count
        {
            switch (formatTypes)
            {
                case TraceFormat.Text:
                    TraceInLineFormat(overlapArrays, colDims);
                    break;

                case TraceFormat.Graph:
                    TraceInGraphFormat(overlapArrays, colDims, threshold, aboveThreshold, belowThreshold);
                    break;

                case TraceFormat.Excel:
                    TraceInExcelFormat(overlapArrays, colDims);
                    break;

                default:
                    //Debug.WriteLine($"Not A correct Trace Format! ");
                    break;
            }

        }

        /// <summary>
        /// <br>To trace out the column Overlap values and output them to a txt file.</br>
        /// <br>at SdrRepresentation_Output/TraceInLineFormat.txt </br>
        /// <br>Needs to Include link md to description</br>
        /// </summary>
        /// <param name="overlapArrays">overlap result from calling an object of type Connection.OverLaps</param>
        /// <param name="colDims">dimension in [width, length] array</param>        
        public static void TraceInLineFormat(List<double[,]> overlapArrays, int[] colDims)
        {
            List<string> vs = new List<string>();
            int column = 0;
            foreach (var entry in overlapArrays)
            {
                for (int i = 0; i < colDims[0]; ++i)
                {
                    for (int j = 0; j < colDims[1]; ++j)
                    {
                        vs.Add($"{column},{entry[i, j]}");
                        column++;
                    }
                }
            }
            if (!Directory.Exists("SdrRepresentation_Output"))
            {
                Directory.CreateDirectory("SdrRepresentation_Output");
            }
            File.WriteAllLines("SdrRepresentation_Output/TraceInLineFormat.txt", vs.ToArray());
        }

        /// <summary>
        /// <br>To represent the column overlap values in graph format</br>
        /// <br>The current default output size of the graph is 500, 524</br>
        /// <br>Needs to Include link md to description</br>
        /// </summary>
        /// <param name="overlapArrays">overlap result from calling an object of type Connection.OverLaps</param>
        /// <param name="colDims">dimension in [width, length] array</param>
        /// <param name="threshold"></param>
        /// <param name="aboveThreshold"></param>
        /// <param name="belowThreshold"></param>
        public static void TraceInGraphFormat(List<double[,]> overlapArrays, int[] colDims, double threshold, Color aboveThreshold, Color belowThreshold, int width = 500, int height = 524)
        {
            int row = 0;

            Image image = new Bitmap(width, height);

            Graphics graph = Graphics.FromImage(image);

            graph.Clear(Color.Azure);
            Pen pen = new Pen(belowThreshold);

            foreach (var entry in overlapArrays)
            {
                for (int i = 0; i < colDims[0]; ++i)
                {
                    for (int j = 0; j < colDims[1]; ++j)
                    {
                        var overlapValue = (entry[i, j]);
                        if (overlapValue > threshold)
                        {
                            Pen penthreshold = new Pen(aboveThreshold);
                            graph.DrawLines(penthreshold, new Point[] { new Point(row, (int)entry[i, j]), new Point(row, 0) });
                        }
                        else
                        {
                            graph.DrawLines(pen, new Point[] { new Point(row, (int)entry[i, j]), new Point(row, 0) });
                        }
                        row++;
                    }
                }
            }
            if (!Directory.Exists("SdrRepresentation_Output"))
            {
                Directory.CreateDirectory("SdrRepresentation_Output");
            }
            image.Save("SdrRepresentation_Output/trace_col_overlap_graph.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        /// <summary>
        /// <br>To export the column overlap values to excel</br>
        /// <br>Needs to Include link md to description</br>
        /// </summary>
        /// <param name="overlapArrays">overlap result from calling an object of type Connection.OverLaps</param>
        /// <param name="colDims">dimension in [width, length] array</param>       
        public static void TraceInExcelFormat(List<double[,]> overlapArrays, int[] colDims)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            SpreadsheetInfo.FreeLimitReached += (sender, e) => e.FreeLimitReachedAction = FreeLimitReachedAction.ContinueAsTrial;
            var workbook = new ExcelFile();
            var worksheet = workbook.Worksheets.Add("TraceColumnOverlap");
            worksheet.Cells["A1"].Value = "Tracing out all the values for Column overlap:";
            worksheet.Cells["A2"].Value = "Column";
            worksheet.Cells["B2"].Value = "Overlap Value";

            // Write header data to Excel cells.
            int colval = 2;
            int indexVal = 0;
            foreach (var entry in overlapArrays)
            {
                for (int i = 0; i < colDims[0]; ++i)
                {
                    for (int j = 0; j < colDims[1]; ++j)
                    {
                        worksheet.Cells[colval, 0].SetValue(indexVal);
                        worksheet.Cells[colval, 1].SetValue(entry[i, j]);
                        colval++;
                        indexVal++;
                    }
                }
            }
            if (!Directory.Exists("SdrRepresentation_Output"))
            {
                Directory.CreateDirectory("SdrRepresentation_Output");
            }
            workbook.Save("SdrRepresentation_Output/ColumnOverlapTracing.xlsx");
        }

    }
}

