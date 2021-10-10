using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace UnitTestsProject.Sdr
{
    [TestClass]
    public class ScalarEncoderTestOverBitmap
    {
        /// <summary>
        /// Describe how to output SDR as text
        /// </summary>
        [TestMethod]
        public void CreateSdrAsTextTest()
        {
            var outFolder = @"EncoderOutputImages\ScalerEncoderOutput";

            int[] d = new int[] { 1, 4, 5, 7, 8, 9 };
            this.ScalarEncoderTest(d);

            Directory.CreateDirectory(outFolder);

            Console.WriteLine("SDR Representation using ScalarEncoder");


            for (int input = 1; input < (int)6; input++)
            {
                //double input = 1.10;


                ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
                    {
                        { "W", 25},
                        { "N", (int)0},
                        { "Radius", (double)2.5},
                        { "MinVal", (double)1},
                        { "MaxVal", (double)50},
                        { "Periodic", false},
                        { "Name", "Scalar Encoder"},
                        { "ClipInput", false},
                    });

                var result = encoder.Encode(input);
                Debug.WriteLine($"Input = {input}");
                Debug.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
                Debug.WriteLine($"SDR As Indices = {NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))}");

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input}.png", Color.Yellow, Color.Black, text: input.ToString());
            }
        }

        private void ScalarEncoderTest(int[] inputs)
        {
            var outFolder = @"..\..\..\TestFiles\ScalarEncoderResults";

            ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},       // 2% Approx 
                { "N", 100},
                { "MinVal", (double)0},
                { "MaxVal", (double)99},
                { "Periodic", true},
                { "Name", "Scalar Sequence"},
                { "ClipInput", true},
            });
            Dictionary<double, int[]> sdrs = new Dictionary<double, int[]>();


            foreach (double input in inputs)
            {
                int[] result = encoder.Encode(input);

                Console.WriteLine($"Input = {input}");
                Console.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
                Console.WriteLine($"SDR As Text = {NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))}");


                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
                int[,] twoDimArray = ArrayUtils.Transpose(twoDimenArray);
                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input}.png", Color.PaleGreen, Color.Blue, text: input.ToString());

                sdrs.Add(input, result);


            }


            // <summary>
            /// Calculate all required results.
            /// 1. Overlap and Union of the Binary arrays of two scalar values
            ///    It cross compares the binary arrays  of any of the two scalar values User enters.
            /// 2. Creates bitmaps of the overlaping and non-overlaping regions of the two binary arrays selected by the User.
            /// </summary>
            Console.WriteLine("Encoder Binary array Created");
            Console.WriteLine("Enter the two elements you want to Compare");
            String a = Console.ReadLine();
            String b = Console.ReadLine();

            SimilarityResult(Convert.ToInt32(a), Convert.ToInt32(b), sdrs, outFolder);

        }

        private void SimilarityResult(int arr1, int arr2, Dictionary<double, int[]> sdrs, String folder)                // Function to check similarity between Inputs 
        {

            List<int[,]> arrayOvr = new List<int[,]>();

            int h = arr1;
            int w = arr2;

            Console.WriteLine("SDR[h] = ");

            Console.WriteLine(Helpers.StringifyVector(sdrs[h]));

            Console.WriteLine("SDR[w] = ");

            Console.WriteLine(Helpers.StringifyVector(sdrs[w]));

            var Overlaparray = SdrRepresentation.OverlapArraFun(sdrs[h], sdrs[w]);
            int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, (int)Math.Sqrt(Overlaparray.Length), (int)Math.Sqrt(Overlaparray.Length));
            int[,] twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
            NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_Union\\Overlap_{h}_{w}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

            var unionArr = sdrs[h].Union(sdrs[w]).ToArray();
            int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(unionArr, (int)Math.Sqrt(unionArr.Length), (int)Math.Sqrt(unionArr.Length));
            int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

            NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Overlap_Union\\Union_{h}_{w}.png", Color.PaleGreen, Color.Green, text: $"Overlap_{h}_{w}.png");

        }

        /// <summary>
        /// Describe how to output SDR as bitmap
        /// </summary>
        [TestMethod]
        public void CreateSdrAsBitmapTest()
        {
            Object[] d1 = new Object[] { "05/02/2020 22:58:06", "06/04/2020 01:28:07", "07/09/2019 21:15:07", "08/01/2017 11:27:07" };

            this.DateTimeEncoderTest(d1);


            Object[] inputs = { "05/02/2020 22:58:07", "06/04/2020 01:28:07", "07/09/2019 21:15:07", "08/01/2018 11:27:07" };



            foreach (var input in inputs)
            {

                var outFolder = @"EncoderOutputImages\DateTimeEncoderOutput";
                Directory.CreateDirectory(outFolder);

                var now = DateTimeOffset.Now;

                Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();
                encoderSettings.Add("DateTimeEncoder", new Dictionary<string, object>()
                            {
                                { "W", 21},
                                { "N", 1024},
                                { "MinVal", now.AddYears(-10)},
                                { "MaxVal", now},
                                { "Periodic", false},
                                { "Name", "DateTimeEncoder"},
                                { "ClipInput", false},
                                { "Padding", 5},
                            });

                var encoder = new DateTimeEncoder(encoderSettings, DateTimeEncoder.Precision.Days);
                var result = encoder.Encode(DateTimeOffset.Parse(input.ToString()));

                Debug.WriteLine($"Input = {input}");
                Debug.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
                Debug.WriteLine($"SDR As Indices = {NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))}");

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 32, 32);
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input.ToString().Replace("/", "-").Replace(":", "-")}_32x32-N-{encoderSettings["DateTimeEncoder"]["N"]}-W-{encoderSettings["DateTimeEncoder"]["W"]}.png");


            }
        }

        [TestMethod]
        public void DateTimeEncoderTest(Object[] inputs)
        {
            var outFolder = @"..\..\..\..\ScalarEncoderResults";

            var now = DateTimeOffset.Now;

            Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();
            encoderSettings.Add("DateTimeEncoder", new Dictionary<string, object>()
                    {
                        { "W", 7},
                        { "N", 20},
                        { "MinVal", now.AddYears(-10)},
                        { "MaxVal", now},
                        { "Periodic", false},
                        { "Name", "DateTimeEncoder"},
                        { "ClipInput", false},
                        { "Padding", 5},
                    });

            var encoder = new DateTimeEncoder(encoderSettings, DateTimeEncoder.Precision.Days);

            Dictionary<Object, int[]> sdrs = new Dictionary<Object, int[]>();


            foreach (Object input in inputs)
            {
                int[] result = encoder.Encode(DateTimeOffset.Parse(input.ToString()));

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 32, 32);
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input.ToString().Replace("/", "-").Replace(":", "-")}_32x32-N-{encoderSettings["DateTimeEncoder"]["N"]}-W-{encoderSettings["DateTimeEncoder"]["W"]}.png");


                Console.WriteLine($"Input = {input}");
                Console.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
                Console.WriteLine($"SDR As Text = {NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))}");

                sdrs.Add(input, result);


            }


            // <summary>
            /// Calculate all required results.
            /// 1. Overlap and Union of the Binary arrays of two scalar values
            ///    It cross compares the binary arrays  of any of the two scalar values User enters.
            /// 2. Creates bitmaps of the overlaping and non-overlaping regions of the two binary arrays selected by the User.
            /// </summary>



            int[] result1 = encoder.Encode(DateTimeOffset.Parse(inputs[0].ToString()));

            Console.WriteLine($"Input 000 = {inputs[0]}");


            Console.WriteLine($"Input 33 = {inputs[3]}");

            int[] result2 = encoder.Encode(DateTimeOffset.Parse(inputs[3].ToString()));


            SimilarityResult1(result1, result2, sdrs, outFolder, inputs[0], inputs[3]);

        }

        public void SimilarityResult1(int[] arr1, int[] arr2, Dictionary<Object, int[]> sdrs, String folder, Object input0, Object input1)                // Function to check similarity between Inputs 
        {
            List<int[,]> arrayOvr = new List<int[,]>();

            Object h = input0;
            Object w = input1;

            var Overlaparray = SdrRepresentation.OverlapArraFun(arr1, arr2);

            int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, 32, 32);

            var twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);

            NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_Union\\Overlap_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

            var unionArr = arr1.Union(arr2).ToArray();
            int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(unionArr, 32, 32);
            int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

            NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Overlap_Union\\Union{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Green, text: $"Overlap_{h}_{w}.png");
        }



        [TestMethod]
        public void CategoryEncoderTest(string[] inputs)
        {



            var outFolder = @"..\..\..\..\ScalarEncoderResults";

            var now = DateTimeOffset.Now;

            Dictionary<string, object> encoderSetting = getDefaultSettings(); // creaing default constructor
            static Dictionary<string, object> getDefaultSettings()
            {
                Dictionary<String, Object> encoderSetting = new Dictionary<string, object>();
                encoderSetting.Add("W", 3);
                encoderSetting.Add("Radius", (double)1);
                return encoderSetting;
            }

            CategoryEncoder categoryEncoder = new CategoryEncoder(inputs, encoderSetting); // passing the input array here
            Dictionary<Object, int[]> sdrs = new Dictionary<Object, int[]>();


            foreach (Object input in inputs)
            {
                int[] result = categoryEncoder.Encode(input);

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 32, 32);
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 512, 512, $"{outFolder}\\{input}.png", Color.Gold, Color.Silver, text: input.ToString());

                Console.WriteLine($"Input = {input}");
                Console.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
                Console.WriteLine($"SDR As Text = {NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))}");

                sdrs.Add(input, result);

            }




            // <summary>
            /// Calculate all required results.
            /// 1. Overlap and Union of the Binary arrays of two scalar values
            ///    It cross compares the binary arrays  of any of the two scalar values User enters.
            /// 2. Creates bitmaps of the overlaping and non-overlaping regions of the two binary arrays selected by the User.
            /// </summary>


            int[] result1 = categoryEncoder.Encode(inputs[0]);


            Console.WriteLine($"Input 000 = {inputs[0]}");


            Console.WriteLine($"Input 33 = {inputs[3]}");

            int[] result2 = categoryEncoder.Encode(inputs[3]);

            SimilarityResult2(result1, result2, sdrs, outFolder, inputs[0], inputs[3]);

        }

        public void SimilarityResult2(int[] arr1, int[] arr2, Dictionary<Object, int[]> sdrs, String folder, Object input0, Object input1)                // Function to check similarity between Inputs 
        {

            List<int[,]> arrayOvr = new List<int[,]>();

            Object h = input0;
            Object w = input1;

            var Overlaparray = SdrRepresentation.OverlapArraFun(arr1, arr2);

            int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, 32, 32);

            var twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);

            NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_Union\\Overlap_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

            var Unionarray = arr1.Union(arr2).ToArray();
            int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Unionarray, 32, 32);
            int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

            NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Overlap_Union\\Union{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Green, text: $"Union_{h}_{w}.png");

            // Bitmap Intersection Image of two bit arrays selected for comparison
            SdrRepresentation.DrawIntersections(twoDimArray3, twoDimArray1, 100, $"{folder}\\Overlap_Union\\Intersection_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.Black, Color.Gray, text: $"Intersection_{h}_{w}.png");
        }








    }




}



