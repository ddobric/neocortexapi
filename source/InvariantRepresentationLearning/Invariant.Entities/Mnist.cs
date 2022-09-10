using Invariant.Entities;
using MNIST.IO;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Invariant.Entities
{
    public class Mnist
    {
        /// <summary>
        /// Generate Mnist Data from the provided package
        /// </summary>
        /// <param name="NoSample">Number of Samples</param>
        /// <returns></returns>
        public static void DataGen(string MnistFolder, string outputPath, int NoSample)
        {
            IEnumerable<TestCase> data = InitMnistSetAndDirectories(MnistFolder, outputPath);

            int[] checkArray = new int[10];

            foreach (var a in data)
            {
                //Visualize(a);
                if (checkArray[(int)a.Label] < NoSample)
                {
                    checkArray[(int)a.Label] += 1;
                    SaveImage(a, outputPath, checkArray[(int)a.Label]);
                }
                if (EnoughSample(ref checkArray, NoSample))
                {
                    return;
                }
            }
        }

        private static IEnumerable<TestCase> InitMnistSetAndDirectories(string MnistFolder, string outputPath)
        {
            var data = FileReaderMNIST.LoadImagesAndLables
                            (
                            Path.Combine(MnistFolder, "train-labels-idx1-ubyte.gz"),
                            Path.Combine(MnistFolder, "train-images-idx3-ubyte.gz")
                            );

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            for (int i = 0; i < 10; i++)
            {
                if (!Directory.Exists(Path.Combine(outputPath, $"{i}")))
                {
                    Directory.CreateDirectory(Path.Combine(outputPath, $"{i}"));
                }
            }

            return data;
        }

        /// <summary>
        /// Generate all Mnist Dataset
        /// </summary>
        /// <param name="MnistFolder"></param>
        /// <param name="outputPath"></param>
        public static void DataGenAll(string MnistFolder, string outputPath)
        {
            IEnumerable<TestCase> data = InitMnistSetAndDirectories(MnistFolder, outputPath);
            int[] indexArray = new int[10];
            foreach (var a in data)
            {
                indexArray[(int)a.Label] += 1;
                SaveImage(a, outputPath, indexArray[(int)a.Label]);
            }
        }

        /// <summary>
        /// Generate Invariant Representation data from Mnist Dataset
        /// </summary>
        public static void TestDataGen(string MnistFolder, string outputPath, int NoSample)
        {
            var data = FileReaderMNIST.LoadImagesAndLables
                (
                Path.Combine(MnistFolder, "t10k-labels-idx1-ubyte.gz"),
                Path.Combine(MnistFolder, "t10k-images-idx3-ubyte.gz")
                );

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            for (int i = 0; i < 10; i++)
            {
                if (!Directory.Exists(Path.Combine(outputPath, $"{i}")))
                {
                    Directory.CreateDirectory(Path.Combine(outputPath, $"{i}"));
                }
            }

            int[] checkArray = new int[10];

            Random random = new Random();

            foreach (var a in data)
            {
                //Visualize(a);
                TestCase invA = GenerateInvFrame(a,random.Next());
                if (checkArray[(int)a.Label] < NoSample)
                {
                    checkArray[(int)a.Label] += 1;
                    SaveImage(invA, outputPath, checkArray[(int)a.Label]);
                }
                if (EnoughSample(ref checkArray, NoSample))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Generate test case. Settings found now in function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="nextInt64"></param>
        /// <returns></returns>
        private static TestCase GenerateInvFrame(TestCase a, int nextInt64)
        {
            var InvA = new TestCase();

            // assigning label
            InvA.Label = a.Label;

            byte[,] newbyte = new byte[100, 100];

            var framelist = Frame.GetConvFramesbyPixel(newbyte.GetLength(1), newbyte.GetLength(0), a.Image.GetLength(0), a.Image.GetLength(1));
            

            Random random = new Random(nextInt64);
            var selectedFrame = framelist[random.Next() % framelist.Count];


            for(int i = 0; i < newbyte.GetLength(0); i += 1)
            {
                for(int j = 0; j < newbyte.GetLength(1); j += 1)
                {
                    if(i>= selectedFrame.tlY && i<=selectedFrame.brY && j>= selectedFrame.tlX && j <= selectedFrame.brX)
                    {
                        newbyte[i, j] = a.Image[i - selectedFrame.tlY, j - selectedFrame.tlX];
                    }
                }
            }

            InvA.Image = newbyte;

            return InvA;
        }

        /// <summary>
        /// Debug print the element to output debug
        /// </summary>
        /// <param name="a"></param>
        private static void Visualize(TestCase a)
        {
            Debug.WriteLine(a.Label);
            for (int i = 0; i < a.Image.GetLength(0); i++)
            {
                for (int j = 0; j < a.Image.GetLength(1); j++)
                {
                    Debug.Write(a.Image[i, j]);
                }
                Debug.WriteLine("");
            }
        }

        /// <summary>
        /// Save a MNIST image as png
        /// </summary>
        /// <param name="a">the output element from MNIST.IO nupkg</param>
        /// <param name="outputfolder">output training folder</param>
        /// <param name="index">current index of the number type</param>
        private static void SaveImage(TestCase a, string outputfolder, int index)
        {
            SKBitmap outputBitmap = new SKBitmap(a.Image.GetLength(0), a.Image.GetLength(1));

            for (int y = 0; y < outputBitmap.Height; y++)
            {
                for (int x = 0; x < outputBitmap.Width; x++)
                {
                    int grey = (int)a.Image[y, x];
                    outputBitmap.SetPixel(x, y, new SKColor((byte)(grey), (byte)(grey), (byte)(grey)));
                }
            }

            string imageName = $"{index}.png";
            string outputfile = Path.Combine(outputfolder, a.Label.ToString(), imageName);

            using (var image = SKImage.FromBitmap(outputBitmap))
            {
                string encodingFormat = "Png";
                SKEncodedImageFormat frm = (SKEncodedImageFormat)Enum.Parse(typeof(SKEncodedImageFormat), encodingFormat);


                using (var data = image.Encode(frm, 80))
                {
                    // save the data to a stream
                    using (var stream = File.OpenWrite($"{outputfile}"))
                    {
                        data.SaveTo(stream);
                    }
                }
            }
        }

        /// <summary>
        /// Check if there are already enough sample for the training set
        /// </summary>
        /// <param name="checkArray">the array which shows number of element read for each number</param>
        /// <param name="noSample">number of max sample</param>
        /// <returns></returns>
        private static bool EnoughSample(ref int[] checkArray, int noSample)
        {
            for(int i = 0; i < checkArray.Length; i++)
            {
                if (checkArray[i] < noSample)
                {
                    return false;
                }
            }
            return true;
        }
    }
}