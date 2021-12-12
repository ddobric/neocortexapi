using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace UnitTestProject
{
    /// <summary>
    /// Project work: https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2019-2020/tree/PRD1/PRD1%20Group%20SE%20Project(WS-2019)/NoiseTestSpatialPooler
    /// </summary>
    [TestClass]
    public class SpatialPoolerMemorizingExperiment
    {
        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest1()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 1.0, 3.0, 2.0 },
                                                      new double[] { 3.0, 1.0, 4.0 },
                                                      new double[] { 4.0, 1.0}
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(21);
            inputs.setMaxIndex(100.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(1.0);

            ProcessTestCase(inputSequences, inputs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest2()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 1.0, 1.0, 1.0},
                                                      new double[] { 2.0, 2.0 },
                                                      new double[] { 4.0, 4.0, 4.0, 4.0},
                                                      new double[] { 3.0, 2.0, 1.0 }
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(21);
            inputs.setMaxIndex(100.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(1.0);

            ProcessTestCase(inputSequences, inputs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest3()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 2.0, 1.0 , 3.0, 4.0},
                                                      new double[] { 3.0, 5.0 },
                                                      new double[] { 4.0, 4.0, 2.0, 1.0}
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(25);
            inputs.setMaxIndex(100.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(1.0);

            ProcessTestCase(inputSequences, inputs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest4()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 1.0, 1.0, 1.0 },
                                                      new double[] { 2.0, 2.0, 2.0 },
                                                      new double[] { 3.0, 3.0, 3.0 },
                                                      new double[] { 4.0, 4.0, 4.0 },
                                                      new double[] { 1.0, 1.0, 1.0 }
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(25);
            inputs.setMaxIndex(100.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(1.0);

            ProcessTestCase(inputSequences, inputs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest5()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 1.0, 2.0, 3.0, 4.0 },
                                                      new double[] { 1.0, 2.0, 3.0, 4.0 },
                                                      new double[] { 1.0, 2.0, 3.0, 4.0 },
                                                      new double[] { 1.0, 2.0, 3.0, 4.0 }
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(25);
            inputs.setMaxIndex(200.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(3.0);

            ProcessTestCase(inputSequences, inputs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest6()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 5.0, 3.0, 1.0, 4.0 },
                                                      new double[] { 3.0, 4.0, 2.0 },
                                                      new double[] { 1.0, 5.0, 3.0, 2.0, 1.0 },
                                                      new double[] { 5.0, 4.0, 1.0, 4.0 }
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(51);
            inputs.setMaxIndex(100.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(4.0);

            ProcessTestCase(inputSequences, inputs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        [TestCategory("SpatialPoolerMemorizingExperiment")]
        public void NoiseTest7()
        {
            List<double[]> inputSequences = new List<double[]>()
                                                    { new double[] { 2.0, 6.0, 1.0, 4.0 },
                                                      new double[] { 3.0, 4.0, 2.0 },
                                                      new double[] { 1.0, 5.0, 3.0, 2.0, 1.0 },
                                                      new double[] { 5.0, 4.0, 1.0, 6.0 }
                                                    };

            InputParameters inputs = new InputParameters();
            inputs.setWidth(25);
            inputs.setMaxIndex(100.0);
            inputs.setRadius(-1.0);
            inputs.setCompareNumber(6.0);

            ProcessTestCase(inputSequences, inputs);
        }

        /// <summary>
        /// Processes the test cases for Noise Test taking the necessary parameters.
        /// It's the parent method which calls other utility methods to create the input vectors and the outputs.
        /// </summary>
        /// <param name="inputSequence">An array of double, consisting the starting indexes for each input vector</param>
        /// <param name="inputs">
        /// A parameter of the class "InputParameters", which contains all the input parameters needed as properties which can be set in a test case
        /// </param>
        /// <returns>Returns nothing</returns>
        public void ProcessTestCase(List<double[]> inputSequences, InputParameters inputs)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string parentDir = path.Substring(0, path.IndexOf("bin") - 1);
            string resultDir = parentDir.Substring(0, parentDir.LastIndexOf(@"\"));

            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string outFolder = resultDir + @"\SpatialPooler_Results\" + timeStamp + @"\Output\";
            string inFolder = resultDir + @"\SpatialPooler_Results\" + timeStamp + @"\InputVectors";

            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            if (!Directory.Exists(inFolder + @"\"))
                Directory.CreateDirectory(inFolder);

            //int radius = 0;

            var parameters = GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, 64 * 64);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
            parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * 64 * 64);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * 64 * 64);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
            parameters.Set(KEY.MAX_BOOST, 5);
            Console.WriteLine("Fetched all default parameters\n");

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            parameters.apply(mem);
            Console.WriteLine("\nConfiguring the Inputs...\n");
            sp.Init(mem, GetInMemoryDictionary());
            int outFolderCount = 0;

            int compareIndex = Convert.ToInt32(inputs.getCompareNumber());
            double[][] recordOutput = null;
            double[] hammingDistance = null;

            foreach (double[] inputSequence in inputSequences)
            {
                outFolderCount++;
                double minVal = 0.0;
                for (int i = 0; i < inputSequence.Length; i++)
                {
                    if (i == 0)
                        minVal = inputSequence[i];
                    else if (inputSequence[i] < minVal)
                        minVal = inputSequence[i];
                }
                minVal -= 1.0;

                Console.WriteLine("\nGetting the Input Vectors...\n");
                var inputVectors = GetEncodedSequence(inputSequence, minVal, inputs.getMaxIndex(), inputs, inFolder);

                int count = 1;
                //string output = String.Empty;
                int max = 0;
                for (int i = 0; i < inputVectors.Count; i++)
                {
                    hammingDistance = null;
                    //output = String.Empty;
                    Console.WriteLine("Computing the Output for the vector no: " + count.ToString() + "...\n");
                    var activeArray = sp.Compute(inputVectors[i], true) as int[];

                    for (int j = 0; j < activeArray.Length; j++)
                    {
                        if (activeArray[j] > max)
                            max = activeArray[j];
                    }

                    //var str = Helpers.StringifyVector(activeArray);

                    int rows = Convert.ToInt32(Math.Ceiling(Math.Sqrt(Convert.ToDouble(max))));
                    int counter = 0;
                    int index = 0;
                    int[,] outTwoDArray = new int[rows, rows];

                    for (int j = 0; j < rows; j++)
                    {
                        for (int k = 0; k < rows; k++)
                        {
                            outTwoDArray[j, k] = 0;
                        }
                    }

                    for (int j = 0; j < rows; j++)
                    {
                        for (int k = 0; k < rows; k++)
                        {
                            counter++;
                            if (index < activeArray.Length && activeArray[index] == counter)
                            {
                                index++;
                                outTwoDArray[j, k] = 1;
                            }
                        }
                    }

                    double[][] comparingArray = new double[rows][];
                    for (int j = 0; j < rows; j++)
                    {
                        comparingArray[j] = new double[rows];
                        for (int k = 0; k < rows; k++)
                            comparingArray[j][k] = Convert.ToDouble(outTwoDArray[j, k]);
                    }

                    int[,] record2Darray = null;
                    if (inputSequence[i] == compareIndex)
                    {
                        if (recordOutput != null)
                        {
                            hammingDistance = MathHelpers.GetHammingDistance(recordOutput, comparingArray, false);
                            record2Darray = new int[recordOutput.Length, recordOutput.Length];
                            for (int j = 0; j < recordOutput.Length; j++)
                            {
                                for (int k = 0; k < recordOutput.Length; k++)
                                    record2Darray[j, k] = Convert.ToInt32(recordOutput[j][k]);
                            }
                        }

                        recordOutput = new double[rows][];

                        for (int j = 0; j < rows; j++)
                        {
                            recordOutput[j] = new double[rows];
                            for (int k = 0; k < rows; k++)
                                recordOutput[j][k] = comparingArray[j][k];
                        }
                    }

                    if (hammingDistance != null)
                    {
                        int rowHam = Convert.ToInt32(Math.Ceiling(Math.Sqrt(hammingDistance.Length)));
                        int[,] hammingArray = new int[rowHam, rowHam];
                        int limit = 0;

                        for (int j = 0; j < rowHam; j++)
                        {
                            for (int k = 0; k < rowHam; k++)
                            {
                                if (limit < hammingDistance.Length)
                                {
                                    //hj
                                    hammingArray[j, k] = Convert.ToInt32(hammingDistance[limit]);
                                    limit++;
                                }
                            }
                        }

                        int compare_no = 1;
                        if (!File.Exists($"{outFolder}\\Compare_{compareIndex}.png"))
                        {
                            DrawBitmapHamming(hammingArray, 1024, 1024, $"{outFolder}\\Compare_{compareIndex}.png", $"Compare_{compareIndex}");
                            DrawBitmapOverlap(record2Darray, outTwoDArray, 1024, 1024, $"{outFolder}\\Overlap_{compareIndex}.png", $"Overlap_{compareIndex}");
                        }
                        else
                        {
                            while (File.Exists($"{outFolder}\\Compare_{compareIndex}_{compare_no}.png"))
                                compare_no++;
                            DrawBitmapHamming(hammingArray, 1024, 1024, $"{outFolder}\\Compare_{compareIndex}_{compare_no}.png", $"Compare_{compareIndex}");
                            compare_no = 1;
                            while (File.Exists($"{outFolder}\\Overlap_{compareIndex}_{compare_no}.png"))
                                compare_no++;
                            DrawBitmapOverlap(record2Darray, outTwoDArray, 1024, 1024, $"{outFolder}\\Overlap_{compareIndex}_{compare_no}.png", $"Overlap_{compareIndex}");
                        }


                    }

                    if (!Directory.Exists(outFolder + @"\\" + outFolderCount.ToString()))
                        Directory.CreateDirectory(outFolder + @"\\" + outFolderCount.ToString());

                    int[,] out2dimArray = ArrayUtils.Transpose(outTwoDArray);
                    NeoCortexUtils.DrawBitmap(out2dimArray, 1024, 1024, $"{outFolder}\\{outFolderCount}\\{count}.png", Color.Black, Color.Green, text: inputSequence[i].ToString());

                    //File.WriteAllLines(outFolder + count.ToString() + ".txt", new string[] { str });
                    Console.WriteLine("Output is recorded in the path: " + outFolder + count.ToString() + ".txt\n");
                    count++;
                }
            }
        }

        /// <summary>
        /// To draw a bitmap png file to show the comparision between to learnings
        /// </summary>
        /// <param name="twoDimArray1">Original output array to be compared</param>
        /// <param name="twoDimArray2">New output array to be compared with</param>
        /// <param name="length">Length of the plane</param>
        /// <param name="width">Width of the plane</param>
        /// <param name="filePath">Path where the generated file to be kept</param>
        /// <param name="text">Test to be written on the top left side of the generated file</param>
        public static void DrawBitmapOverlap(int[,] twoDimArray1, int[,] twoDimArray2, int length, int width, String filePath, string text = null)
        {
            int w = twoDimArray1.GetLength(0);
            int h = twoDimArray1.GetLength(1);

            var scale = width / w;

            if (scale * w < width)
                scale++;

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
                            if (twoDimArray1[Xcount, Ycount] == 1 && twoDimArray2[Xcount, Ycount] == 1)
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Yellow); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.Blue); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray1[Xcount, Ycount] == 1)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.Yellow); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray2[Xcount, Ycount] == 1)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.Green); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Black); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.Black); // HERE IS YOUR LOGIC
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
        /// Returns the input vectors as array of integers
        /// </summary>
        /// <param name="inputSequence">An array of double, consisting the starting indexes for each input vector</param>
        /// <param name="min">Minimum index in the input vector plane</param>
        /// <param name="max">Maximum index in the input vector plane</param>
        /// <param name="inputs">
        /// </param>
        /// <param name="outFolder">The path where the input vectors are to be generated</param>
        /// <returns></returns>
        static List<int[]> GetEncodedSequence(double[] inputSequence, double min, double max, InputParameters inputs, string outFolder)
        {
            List<int[]> sdrList = new List<int[]>();

            DateTime now = DateTime.Now;

            ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", inputs.getWidth()},
                { "N", 1024},
                { "Radius", inputs.getRadius()},
                { "MinVal", min},
                { "MaxVal", max},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            });

            foreach (var i in inputSequence)
            {
                var result = encoder.Encode(i);

                sdrList.Add(result);

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                int counter = 0;
                if (File.Exists(outFolder + @"\\" + i + @".png"))
                {
                    counter = 1;
                    while (File.Exists(outFolder + @"\\" + i + @"-" + counter.ToString() + @".png"))
                    {
                        counter++;
                    }
                }

                if (counter == 0)
                    NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{i}.png", Color.Yellow, Color.Black, text: i.ToString());
                else
                    NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{i}-{counter}.png", Color.Yellow, Color.Black, text: i.ToString());
            }

            return sdrList;
        }

        /// <summary>
        /// To plot the bitmap png file for the Hamming Distance of two learnings
        /// </summary>
        /// <param name="twoDimArray">The Hamming Distance array</param>
        /// <param name="width">Width of the array</param>
        /// <param name="height">Height of the array</param>
        /// <param name="filePath">Path where the generated file to be placed</param>
        /// <param name="text">Test to be written on the top left side of the generated file</param>
        public static void DrawBitmapHamming(int[,] twoDimArray, int width, int height, String filePath, string text = null)
        {
            int w = twoDimArray.GetLength(0);
            int h = twoDimArray.GetLength(1);

            var scale = width / w;

            if (scale * w < width)
                scale++;

            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(w * scale, h * scale);
            ColorConverter convert = new ColorConverter();
            int k = 0;
            for (int Xcount = 0; Xcount < w; Xcount++)
            {
                for (int Ycount = 0; Ycount < h; Ycount++)
                {
                    for (int padX = 0; padX < scale; padX++)
                    {
                        for (int padY = 0; padY < scale; padY++)
                        {
                            if (twoDimArray[Xcount, Ycount] == 100)
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Yellow); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, Color.White); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 90)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#e6e6e6")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 80)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#d9d9d9")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 70)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#cccccc")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 60)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#bfbfbf")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 50)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#a6a6a6")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 40)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#8c8c8c")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 30)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#737373")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 20)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#595959")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else if (twoDimArray[Xcount, Ycount] >= 10)
                            {
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#333333")); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Black); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, (Color)convert.ConvertFromString("#000000")); // HERE IS YOUR LOGIC
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
        /// Sets the default parameters which are not to be provided in the test cases
        /// </summary>
        /// <returns>Returns an object of class "Parameters" which contains all the required parameters for the tests</returns>
        public Parameters GetDefaultParams()
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
            parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);

            return parameters;
        }

        /// <summary>
        /// Creates the memory distribution dictionary which is used to configure the inputs
        /// </summary>
        /// <returns>Returns an object of the class "DistributedMemory"</returns>
        public DistributedMemory GetInMemoryDictionary()
        {
            return new DistributedMemory()
            {
                ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1),
                //PoolDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Pool>(1),
            };
        }

    }


    public class InputParameters
    {
        private int width;
        private double maxIndex;
        private double radius;
        private double compareNumber;

        public void setWidth(int value)
        {
            this.width = value;
        }
        public int getWidth()
        {
            return this.width;
        }

        public void setMaxIndex(double value)
        {
            this.maxIndex = value;
        }
        public double getMaxIndex()
        {
            return this.maxIndex;
        }

        public void setRadius(double value)
        {
            this.radius = value;
        }
        public double getRadius()
        {
            return this.radius;
        }

        public void setCompareNumber(double value)
        {
            this.compareNumber = value;
        }
        public double getCompareNumber()
        {
            return this.compareNumber;
        }

    }

}
