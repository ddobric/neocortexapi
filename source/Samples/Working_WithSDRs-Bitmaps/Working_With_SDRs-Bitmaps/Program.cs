using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexEntities;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using NeoCortex;

namespace WorkingWithSDR
{
    public class Program
    {
        public static void Main()
        {
            /// User can directly compair two scalar values between 0 to 99.

            var outFolder = @"EncoderOutputImages\ScalerEncoderOutput";

            //int[] d = new int[] { 1, 4, 5, 7, 8, 9 };     
            Console.WriteLine("Welcome to the SDR Representation project. Please enter two numbers (0-99) to find SDR as Indices and Text, Bitmaps, Overlap, Union and Intersection");
            Console.Write("Please enter First Number: ");
            int ch1 = Convert.ToInt16(Console.ReadLine());
            Console.Write("Please enter Second Number: ");
            int ch2 = Convert.ToInt16(Console.ReadLine());


            int[] d = new int[] { ch1, ch2 };
            ScalarEncoderTest(d, ch1, ch2);

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

        private static void ScalarEncoderTest(int[] inputs, int a, int b)
        {
            var outFolder1 = @"NEWTestFiles\NEWScalarEncoderResults";
            var outFolder2 = @"Overlap_Union";

            Directory.CreateDirectory(outFolder1);
            Directory.CreateDirectory(outFolder2);
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
                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder1}\\{input}.png", Color.PaleGreen, Color.Blue, text: input.ToString());

                sdrs.Add(input, result);


            }


            // <summary>
            /// Calculate all required results.
            /// 1. Overlap and Union of the Binary arrays of two scalar values.
            ///    It cross compares the binary arrays  of any of the two scalar values User enters.
            /// 2. Creates bitmaps of the overlaping and non-overlaping regions of the two binary arrays entered by the User.
            /// 3. Creates bitmaps of interestion of Overlap and Union of two values.
            /// </summary>

            //Console.WriteLine("Encoder Binary array Created");
            // Console.WriteLine("Enter the two elements you want to Compare");
            // String a = Console.ReadLine();
            //String b = Console.ReadLine();


            // SimilarityResult(Convert.ToInt32(a), Convert.ToInt32(b), sdrs, outFolder1);
            SimilarityResult(a, b, sdrs, outFolder1);
        }

        private static void SimilarityResult(int arr1, int arr2, Dictionary<double, int[]> sdrs, String folder)              // Function to check similarity between Inputs 
        {


            List<int[,]> arrayOvr = new List<int[,]>();

            int h = arr1;
            int w = arr2;

            Console.WriteLine("SDR[h] = ");

            Console.WriteLine(Helpers.StringifyVector(sdrs[h]));

            Console.WriteLine("SDR[w] = ");



            Console.WriteLine(Helpers.StringifyVector(sdrs[w]));

            var Overlaparray = SdrRepresentation.OverlapArraFun(sdrs[h], sdrs[w]);
            Console.WriteLine("SDR of Overlap = ");
            Console.WriteLine(Helpers.StringifyVector(Overlaparray));
            int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, (int)Math.Sqrt(Overlaparray.Length), (int)Math.Sqrt(Overlaparray.Length));
            int[,] twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
            NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_{h}_{w}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

            //var unionArr = sdrs[h].Union(sdrs[w]).ToArray();                              //This function was not working. so, new Union function is created.
            var unionArr = Union(sdrs[h], sdrs[w]).ToArray();
            Console.WriteLine("SDR of Union = ");
            Console.WriteLine(Helpers.StringifyVector(unionArr));
            int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(unionArr, (int)Math.Sqrt(unionArr.Length), (int)Math.Sqrt(unionArr.Length));
            int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

            NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Union_{h}_{w}.png", Color.PaleGreen, Color.Green, text: $"Union_{h}_{w}.png");
            SdrRepresentation.DrawIntersections(twoDimArray3, twoDimArray1, 100, $"{folder}\\Intersection of Union and Overlap of {h}_{w}.png", Color.Black, Color.Gray, text: $"Intersection.png");

        }

        public static int[] Union(int[] arr1, int[] arr2)                        // To find union of of the Binary arrays of two scalar values.
        {


            int[] union = new int[arr1.Length];

            for (int i = 0; i < arr1.Length; i++)
            {

                if (arr1[i] == 0 && arr2[i] == 0)
                {
                    union[i] = 0;
                }
                else
                {
                    union[i] = 1;
                }
            }
            return union;
        }

    }
}
