// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi.Entities;
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

        public static string StringifyVector(string[] vector)
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
        /// Stringifies the vector by using of custom conversdion function.
        /// </summary>
        /// <param name="vector">The vector to be stringified.</param>
        /// <param name="fnc">Conversion function.</param>
        /// <returns></returns>
        public static string StringifyVector<T>(T[] vector, Func<int, T, string> fnc)
        {
            StringBuilder sb = new StringBuilder();
            int indx = 0;
            foreach (var vectorBit in vector)
            {
                sb.Append(fnc(indx++, vectorBit));
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


        public static List<string> DefaultNodeList
        {
            get
            {
                var nodes = new List<string>()
                {
                      "akka.tcp://HtmCluster@localhost:8081",
                     // "akka.tcp://HtmCluster@localhost:8082"
                };

                //var nodes = new List<string>()
                //{
                //      "akka.tcp://HtmCluster@htm-node1.westeurope.azurecontainer.io:8081",
                //      "akka.tcp://HtmCluster@htm-node2.westeurope.azurecontainer.io:8081"
                //};

                //var nodes = new List<string>()
                //{
                //     "akka.tcp://HtmCluster@phd-node1.westeurope.cloudapp.azure.com:8080",
                //     "akka.tcp://HtmCluster@phd-node2.westeurope.cloudapp.azure.com:8080"

                //};


                return nodes;
            }
        }

        ///// <summary>
        ///// Gets default sparse dictionary configuration.
        ///// </summary>
        //public static HtmSparseIntDictionaryConfig DefaultHtmSparseIntDictionaryConfig
        //{
        //    get
        //    {
        //        return new HtmSparseIntDictionaryConfig()
        //        {
        //            Nodes = DefaultNodeList,
        //            PartitionsPerNode = 200,
        //            ProcessingBatch = 10
        //        };
        //    }
        //}


        public static void F1()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }

            F2();
        }

        public static void F2()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
            F3();
        }

        public static void F3()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
            F4();
        }

        public static void F4()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
            F5();
        }

        public static void F5()
        {
            double x = 1.233;
            for (int i = 0; i < 100000000; i++)
            {
                x += 1.2732;
            }
        }
    }
}
