// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;

namespace NeoCortexApi.Network
{
    /// <summary>
    /// Classifier implementation which memorize all seen values.
    /// </summary>
    /// <typeparam name="TIN"></typeparam>
    /// <typeparam name="TOUT"></typeparam>
    public class HtmClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private List<TIN> inputSequence = new List<TIN>();

        private Dictionary<int[], TIN> activeMap = new Dictionary<int[], TIN>();

        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            throw new NotImplementedException();
        }

        public void ClearState()
        {
            this.activeMap.Clear();
            this.inputSequence.Clear();
        }

        /// <summary>
        /// Assotiate specified input to the given set of predictive cells.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="predictedOutput"></param>
        public void Learn(TIN input, Cell[] output)
        {
            this.inputSequence.Add(input);

            var celIndicies = GetCellIndicies(output);
            Debug.WriteLine($"CellState: {Helpers.StringifyVector(celIndicies)}");
            this.activeMap.Add(celIndicies, input);
        }

        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="predictiveCells">The list of predictive cells.</param>
        /// <returns></returns>
        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            // bool x = false;
            double maxSameBits = 0;
            TIN predictedValue = default(TIN);
            int[] arr = new int[predictiveCells.Length];
            for (int i = 0; i < predictiveCells.Length; i++)
            {
                arr[i] = predictiveCells[i].Index;
            }

            if (predictiveCells.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Debug.WriteLine($"Item length: {predictiveCells.Length}\t Items: {this.activeMap.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();

                //
                // This loop peeks the best input
                foreach (var pair in this.activeMap)
                {
                    //
                    // We compare only outputs which are similar in the length.
                    // This is important, because some outputs, which are not related to the comparing output
                    // might have much mode cells (length) than the current output. With this, outputs with much more cells
                    // would be declared as matching outputs even if they are not.
                    if (Math.Abs(arr.Length / pair.Key.Length) > 0.9)
                    {
                        double numOfSameBitsPct = (double)((double)(pair.Key.Intersect(arr).Count() / (double)arr.Length));
                        if (numOfSameBitsPct > maxSameBits)
                        {
                            Debug.WriteLine($"indx:{n}\tbits/arrbits: {pair.Key.Length}/{arr.Length}\t{pair.Value} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Key)}");
                            maxSameBits = numOfSameBitsPct;
                            predictedValue = pair.Value;
                            indxOfMatchingInp = n;
                        }

                        //if (maxSameBits > 0.9)
                        //{
                        //    sortedMatches.Add(n);
                        //    // We might have muliple matchin candidates.
                        //    // For example: Let the matchin input be i1
                        //    // I1 - c1, c2, c3, c4
                        //    // I2 - c1, c2, c3, c4, c5, c6

                        //    Debug.WriteLine($"cnt:{n}\t{pair.Value} = bits {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Key)}");
                        //}
                    }
                    n++;
                }

                foreach (var item in sortedMatches)
                {

                }

                Debug.Write("[ ");
                for (int i = Math.Max(0, indxOfMatchingInp - 3); i < Math.Min(indxOfMatchingInp + 3, this.activeMap.Keys.Count); i++)
                {
                    if (i == indxOfMatchingInp) Debug.Write("* ");
                    Debug.Write($"{this.inputSequence[i]}");
                    if (i == indxOfMatchingInp) Debug.Write(" *");

                    Debug.Write(", ");
                }
                Debug.WriteLine(" ]");

                return predictedValue;
                //return activeMap[ComputeHash(FlatArray(output))];
            }
            return default(TIN);
        }


        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public void TraceState(string fileName = null)
        {

            List<TIN> processedValues = new List<TIN>();

            foreach (var item in activeMap.Values)
            {
                if (processedValues.Contains(item) == false)
                {
                    StreamWriter sw = null;

                    if (fileName != null)
                        sw = new StreamWriter(fileName.Replace(".csv", $"_Digit_{item}.csv"));

                    Debug.WriteLine("");
                    Debug.WriteLine($"{item}");

                    foreach (var inp in this.activeMap.Where(i => EqualityComparer<TIN>.Default.Equals((TIN)i.Value, item)))
                    {
                        Debug.WriteLine($"{Helpers.StringifyVector(inp.Key)}");

                        if (sw != null)
                            sw.WriteLine($"{Helpers.StringifyVector(inp.Key)}");
                    }

                    if (sw != null)
                    {
                        sw.Flush();
                        sw.Close();
                    }

                    processedValues.Add(item);
                }
            }
        }




        private string ComputeHash(byte[] rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(rawData);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private static byte[] FlatArray(Cell[] output)
        {
            byte[] arr = new byte[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = (byte)output[i].Index;
            }
            return arr;
        }

        private static int[] GetCellIndicies(Cell[] output)
        {
            int[] arr = new int[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = output[i].Index;
            }
            return arr;
        }

        private int predictNextValue(int[] activeArr, int[] predictedArr)
        {
            var same = predictedArr.Intersect(activeArr);

            return same.Count();
        }


    }
}
