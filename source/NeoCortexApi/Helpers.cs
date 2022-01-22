// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public string TraceSdrsMethod(List<int[]> indexesOfNoneZeros, string paddingTxt = " ", bool renderSimilarityMatrix = false)
        { 
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates string representation from one dimensional vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="separator">The separator used between bits.</param>
        /// <returns></returns>
        public static string StringifyVector(int[] vector, string separator=", ")
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                if(separator != null)
                    sb.Append(separator);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates string representation from one dimensional vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="separator">The separator used between bits.</param>
        /// <returns></returns>

        public static string StringifyVector(double[] vector, string separator = ", ")
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                if (separator != null)
                    sb.Append(separator);
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


        /// <summary>
        /// Calculates and renders the matrix of cross-similarities between a set of values.
        /// </summary>
        /// <param name="values">The list of values that will are correlated to eacher.</param>
        /// <param name="similarities">The two-dimensional array of already calculated cross-similarities.</param>
        /// <returns></returns>
        public static string RenderSimilarityMatrix(List<string> values, double[,] similarities)
        {
            string[,] matrix = new string[values.Count, values.Count];

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < values.Count; i++)
            {
                var str = String.Join(';', similarities.GetRow(0));
                sb.AppendLine(str);

                for (int j = 0; j < values.Count; j++)
                {
                    matrix[i, j] = similarities[i, j].ToString("0.##");
                }
            }

            var results = Helpers.RenderSimilarityMatrix(values.ToArray(), matrix);

            return results;
        }


        /// <summary>
        /// Renders the similarity matrix into the readable format. 
        /// </summary>
        /// <param name="values">The list of values that will be compared.</param>
        /// <param name="matrix">Two-dimensianal matrix of cross-similarities between given values. Values are already rendered as string.</param>
        /// <returns></returns>

        public static string RenderSimilarityMatrix( string[] values, string[,] matrix)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            
            sw.Write($"{string.Format(" {0,-15}", "")} |");

            for (int k = 0; k < values.Length; k++)
            {
                string st = String.Format(" {0,-15} |", values[k]);
                sw.Write($"{st}");
            }

            sw.WriteLine("");

            for (int k = 0; k <= values.Length; k++)
            {
                string st = String.Format(" {0,-15} |", "---------------");
                sw.Write($"{st}");
            }

            sw.WriteLine("");

            for (int i = 0; i < values.Length; i++)
            {
                sw.Write(String.Format(" {0,-15} |", values[i]));

                for (int j = 0; j < values.Length; j++)
                {
                    string st = String.Format(" {0,-15} |", matrix[i, j]);
                    sw.Write(st);
                }

                sw.WriteLine("");
            }

            sw.Flush();

            var result = sw.GetStringBuilder().ToString();

            return result;
        }


        /// <summary>
        /// Traceout similarities between the list of provided values.
        /// </summary>
        /// <param name="encodedValues">Dictionary of encoded values. Key is the input value and the value is encoded input value as SDR.</param>
        /// <param name="traceValues"></param>
        /// <returns></returns>
        public static string TraceSimilarities(Dictionary<string, int[]> encodedValues, bool traceValues = true)
        {
            Dictionary<string, int[]> sdrMap = new Dictionary<string, int[]>();
            List<string> inpVals = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (var pair in encodedValues)
            {
                sdrMap.Add($"{pair.Key}", ArrayUtils.IndexWhere(pair.Value, (el) => el == 1));
                inpVals.Add($"{pair.Key}");

                if (traceValues)
                {
                    sb.AppendLine($"{pair.Key} - {Helpers.StringifyVector(pair.Value, separator: null)}");
                }
            }

            sb.AppendLine();

            var similarities = MathHelpers.CalculateSimilarityMatrix(sdrMap);

            var results = Helpers.RenderSimilarityMatrix(inpVals, similarities);

            return sb.ToString() + results;
        }
    }
}
