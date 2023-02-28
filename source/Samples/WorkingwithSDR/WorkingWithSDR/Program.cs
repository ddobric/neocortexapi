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

namespace WorkingWithSDR
{
    public class Program
    {
        public static void Main()
        {
            var outFolder = @"EncoderOutputImages\ScalerEncoderOutput";

            int[] d = new int[] { 1, 4, 5, 7, 8, 9 };
            ScalarEncoderTest(d);

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

        private static void ScalarEncoderTest(int[] inputs)
        {
            var outFolder = @"..\..\..\NEWTestFiles\NEWScalarEncoderResults";

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

        private static void SimilarityResult(int arr1, int arr2, Dictionary<double, int[]> sdrs, String folder)                // Function to check similarity between Inputs 
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
    }
}
    