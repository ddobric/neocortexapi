using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

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