using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

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

            
        }

    }

}