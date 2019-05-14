using ImageBinarizer;
using NeoCortexApi.DistributedCompute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NeoCortexApi
{
    public class Helpers
    {
        /// <summary>
        /// Creates random vector of specified dimension.
        /// </summary>
        /// <param name="numOfBits"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static int[] GetRandomVector(int numOfBits, Random rnd = null)
        {
            if (rnd == null)
                rnd = new Random();

            int[] vector = new int[numOfBits];

            for (int i = 0; i < numOfBits; i++)
            {
                vector[i] = rnd.Next(0, 2);
            }

            return vector;
        }

        /// <summary>
        /// Creates string representation from one dimensional vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static string StringifyVector(int[] vector)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                sb.Append(", ");
            }

            return sb.ToString();
        }

        public static string StringifyVector(double[] vector)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                sb.Append(", ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates string representation from one dimensional vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static string StringifyVector(double[][] vector)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                sb.Append(", ");
            }

            return sb.ToString();
        }


        public static List<string> Nodes
        {
            get
            {
                var nodes = new List<string>()
                {
                      "akka.tcp://HtmCluster@localhost:8081",
                };

                return nodes;
            }
        }

        /// <summary>
        /// Gets default sparse dictionary configuration.
        /// </summary>
        public static HtmSparseIntDictionaryConfig DefaultHtmSparseIntDictionaryConfig
        {
            get
            {
                return new HtmSparseIntDictionaryConfig()
                {
                     ActorConfig = new DistributedComputeLib.ActorConfig()
                     {
                          
                     }, 
                    Nodes = Nodes,
                };
            }
        }


        /// <summary>
        /// Binarize image to binarizedImage.
        /// </summary>
        /// <param name="mnistImage"></param>
        /// <param name="imageSize"></param>
        /// <param name="testName"></param>
        /// <returns></returns>
        public static string BinarizeImage(string mnistImage, int imageSize, string testName)
        {
            string binaryImage;

            Binarizer imageBinarizer = new Binarizer(200, 200, 200, imageSize, imageSize);
            binaryImage = $"{testName}.txt";
            if (File.Exists(binaryImage))
                File.Delete(binaryImage);

            imageBinarizer.CreateBinary(mnistImage, binaryImage);

            return binaryImage;
        }


        public static void f1()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }

            f2();
        }

        public static void f2()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
            f3();
        }

        public static void f3()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
            f4();
        }

        public static void f4()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
            f5();
        }

        public static void f5()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
        }
    }
}
