// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NeoCortexApi
{
    public class Helpers
    {
        /// <summary>
        /// Create szthe test folder.
        /// </summary>
        /// <param name="folderName"></param>
        public static void CreateTestFolder(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                while (!Directory.Exists(folderName))
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Creates random vector of specified dimension.
        /// </summary>
        /// <param name="numOfBits"></param>
        /// <param name="activeBits">How many bits should be active. <=0 means 50% of 'numOfBits' will be active.</param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static int[] GetRandomVector(int numOfBits, int activeBits = -1, Random rnd = null)
        {
            if (rnd == null)
                rnd = new Random();

            if (activeBits <= 0)
                activeBits = numOfBits/2;


            int[] vector = new int[numOfBits];

            for (int i = 0; i < activeBits; i++)
            {
                while (true)
                {
                    int index = rnd.Next(0, numOfBits);
                    if (vector[index] == 0)
                    {
                        vector[index] = 1;
                        break;
                    }
                }                
            }

            return vector;
        }

        #region TraceSDR

        /// <summary>
        /// Creates string representation from one dimensional value. 
        /// </summary>
        /// <see cref=""/>
        /// <param name="sdrs">the SDR sets</param>
        /// <returns>string of traced output SDRs</returns>
        public static string StringifySdr(List<int[]> sdrs, string separator = " | " )
        {
            //List of string of arrays for SDR set
            var heads = new List<int>(new int[sdrs.Count]);

            //The count for SDR starting from initial position [0,0]
            var outputs = new StringBuilder[sdrs.Count];

            while (true)
            {
                //We set the minimum value as initial value of SDRs can be 0
                int minActiveColumn = -1;

                minActiveColumn = ArrangeSdr(sdrs, heads, minActiveColumn);

                if (minActiveColumn == -1)
                {
                    //Stores a mutable string of characters.
                    var result = new StringBuilder();

                    foreach (var output in outputs)
                    {
                        result.AppendLine(output.ToString());
                    }
                    return result.ToString();
                }

                AppendActiveColumn(sdrs, heads, outputs, minActiveColumn, separator);
            }
        }


        /// <summary>
        /// Stores the SDR values from both sets and arrange them.
        /// </summary>
        /// <param name="sdrs">The SDR values as index bit of active neurons</param>
        /// <param name="heads">The two representations taken as input.</param>
        /// <param name="outputs">Represents every index bit of representations.</param>
        /// <param name="minActiveColumn">Stores the similar semantic active and inactive bits for comparison.</param>
        /// <summary>
        private static void AppendActiveColumn(List<int[]> sdrs, List<int> heads, StringBuilder[] outputs, int minActiveColumn, string separator = " | ")
        {
            for (int i = 0; i < sdrs.Count; i++)
            {
                if (outputs[i] == null)
                {
                    outputs[i] = new StringBuilder();
                }
                var head = heads[i];
                var sdr = sdrs[i];

                if (head < sdr.Length && sdr[head] == minActiveColumn)
                {
                    outputs[i].Append(minActiveColumn);
                    outputs[i].Append(separator);
                    heads[i] = head + 1;
                }
                else
                {
                    //Creates a padding or spacing on the indices of inactive bits
                    var numOfSpaces = minActiveColumn.ToString().Length;
                    for (var j = 0; j < numOfSpaces; j++)
                    {
                        outputs[i].Append(" ");
                    }

                    outputs[i].Append(separator);
                }
            }
        }

        /// <summary>
        /// Creates the results of SDRs in a well arrangement
        /// </summary>
        private static int ArrangeSdr(List<int[]> sdrs, List<int> heads, int minActiveColumn)
        {
            for (int i = 0; i < sdrs.Count; i++)
            {
                var head = heads[i];
                var sdr = sdrs[i];

                if (heads[i] > sdr.Length - 1)
                {
                    continue;
                }

                var activeColumn = sdr[head];
                if (minActiveColumn == -1)
                {
                    minActiveColumn = activeColumn;
                }
                else
                {
                    if (activeColumn < minActiveColumn)
                    {
                        minActiveColumn = activeColumn;
                    }
                }
            }

            return minActiveColumn;
        }

        #endregion
        /// <summary>
        /// Creates string representation from one dimensional vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="separator">The separator used between bits.</param>
        /// <returns></returns>
        public static string StringifyVector(int[] vector, string separator = ", ")
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

        public static string RenderSimilarityMatrix(string[] values, string[,] matrix)
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

        /// <summary>
        /// Gets the cells from Apical Segment inside the population that are synaptically connected from the population defined by sourceCells.
        /// </summary>
        /// <param name="sourceCells"></param>
        /// <param name="destinationCells"></param>
        /// <returns></returns>
        public static List<Cell> GetApicalConnectedCells(IList<Cell> sourceCells, IList<Cell> destinationCells)
        {
            List<Cell> cennectedCells = new List<Cell>();

            foreach (var cell in sourceCells)
            {
                cennectedCells.AddRange(GetApicalConnectedCells(cell, destinationCells));
            }

            return cennectedCells;
        }


        /// <summary>
        /// Gets the cells from Apical Segment inside the population that are synaptically connected from the cell.
        /// </summary>
        /// <param name="cell">Cell from X that connets to the populatiopn in Y</param>
        /// <param name="population"></param>
        /// <returns>Cpnnetced cells from the population.</returns>
        public static List<Cell> GetApicalConnectedCells(Cell cell, IList<Cell> population)
        {
            List<Cell> connectedCells = new List<Cell>();

            // Get all synapses in the population.
            var populationSynapses = population.SelectMany(c => c.ApicalDendrites.SelectMany(s => s.Synapses)).ToList();

            // Gets intersecting synapses.
            var intersected = cell.ReceptorSynapses.Intersect(populationSynapses).ToList();

            intersected.ForEach((syn) => connectedCells.Add(population.FirstOrDefault(cell => cell.ApicalDendrites.Count(seg => seg.SegmentIndex == syn.SegmentIndex && cell.Index == syn.SegmentParentCellIndex) > 0)));


            //foreach (var syn in cell.ReceptorSynapses)
            //{
            //    foreach (var popCell in population)
            //    {
            //        foreach (var seg in popCell.ApicalDendrites)
            //        {
            //            foreach (var popSyn in seg.Synapses)
            //            {
            //                if (popSyn == syn)
            //                { 
            //                    connectedCells.Add(popCell);
            //                }
            //            }
            //        }
            //    }
            //}

            return connectedCells;
        }

        /// <summary>
        /// Gets the cells from Distal Segment inside the population that are synaptically connected from the population defined by sourceCells.
        /// </summary>
        /// <param name="sourceCells"></param>
        /// <param name="destinationCells"></param>
        /// <returns></returns>
        public static List<Cell> GetDistalConnectedCells(IList<Cell> sourceCells, IList<Cell> destinationCells)
        {
            List<Cell> cennectedCells = new List<Cell>();

            foreach (var cell in sourceCells)
            {
                cennectedCells.AddRange(GetDistalConnectedCells(cell, destinationCells));
            }

            return cennectedCells;
        }





        /// <summary>
        /// Gets the cells from Distal Segment inside the population that are synaptically connected from the cell.
        /// </summary>
        /// <param name="cell">Cell from X that connets to the populatiopn in Y</param>
        /// <param name="population"></param>
        /// <returns>Cpnnetced cells from the population.</returns>
        public static List<Cell> GetDistalConnectedCells(Cell cell, IList<Cell> population)
        {          
            List<Cell> connectedCells = new List<Cell>();

            var populationSynapses = population.SelectMany(c => c.DistalDendrites.SelectMany(s => s.Synapses)).ToList();

            var intersected = cell.ReceptorSynapses.Intersect(populationSynapses).ToList();

            intersected.ForEach((s) => connectedCells.Add(population.FirstOrDefault(c => c.DistalDendrites.Count(d => d.SegmentIndex == s.SegmentIndex && c.Index == s.SegmentParentCellIndex) == 1)));


            //foreach (var syn in cell.ReceptorSynapses)
            //{
            //    foreach (var popCell in population)
            //    {
            //        foreach (var seg in popCell.DistalDendrites)
            //        {
            //            foreach (var popSyn in seg.Synapses)
            //            {
            //                if (popSyn == syn)
            //                {
            //                    connectedCells.Add(popCell);
            //                }
            //            }
            //        }
            //    }
            //}

            return connectedCells;
        }
    }
}
