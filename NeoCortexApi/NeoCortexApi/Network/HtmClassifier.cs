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

        private Dictionary<int[], int> inputSequenceMap = new Dictionary<int[], int>();

        private Dictionary<int[], TIN> activeMap = new Dictionary<int[], TIN>();

        private Dictionary<TIN, List<int[]>> m_AllInputs = new Dictionary<TIN, List<int[]>>();

        private Dictionary<TIN, int[]> m_ActiveMap2 = new Dictionary<TIN, int[]>();

        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            throw new NotImplementedException();
        }

        public void ClearState()
        {
            //this.activeMap.Clear();
            this.m_ActiveMap2.Clear();
           // this.inputSequence.Clear();
        }

        /// <summary>
        /// Assotiate specified input to the given set of predictive cells.
        /// </summary>
        /// <param name="input">Any kind of input.</param>
        /// <param name="output">The SDR of the input as calculated by SP.</param>
        /// <param name="predictedOutput"></param>
        public void Learn(TIN input, Cell[] output)
        {
           // this.inputSequence.Add(input);

            var cellIndicies = GetCellIndicies(output);
        
            if (m_AllInputs.ContainsKey(input) == false)
                m_AllInputs.Add(input, new List<int[]>());
            else
                m_AllInputs[input].Add(cellIndicies);

            if (this.m_ActiveMap2.ContainsKey(input))
            {
                if (!this.m_ActiveMap2[input].SequenceEqual(cellIndicies))
                {
                    // double numOfSameBitsPct = (double)(((double)(this.activeMap2[input].Intersect(cellIndicies).Count()) / Math.Max((double)cellIndicies.Length, this.activeMap2[input].Length)));
                    // double numOfSameBitsPct = (double)(((double)(this.activeMap2[input].Intersect(cellIndicies).Count()) / (double)this.activeMap2[input].Length));
                    var numOfSameBitsPct = this.m_ActiveMap2[input].Intersect(cellIndicies).Count();
                    Debug.WriteLine($"Prev/Now/Same={this.m_ActiveMap2[input].Length}/{cellIndicies.Length}/{numOfSameBitsPct}");
                }

                this.m_ActiveMap2[input] = cellIndicies;
            }
            else
                this.m_ActiveMap2.Add(input, cellIndicies);
        }

        public void Learn(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            this.inputSequence.Add(input);

            this.inputSequenceMap.Add(GetCellIndicies(output), this.inputSequence.Count - 1);

            if (!activeMap.ContainsKey(GetCellIndicies(output)))
            {
                this.activeMap.Add(GetCellIndicies(output), input);
            }
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
            TIN predictedValue = default;
          
            if (predictiveCells.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Debug.WriteLine($"Item length: {predictiveCells.Length}\t Items: {this.m_ActiveMap2.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();
               
                var celIndicies = GetCellIndicies(predictiveCells);

                Debug.WriteLine($"Predictive cells: {celIndicies.Length} \t {Helpers.StringifyVector(celIndicies)}");

                foreach (var pair in this.m_ActiveMap2)
                {
                    if (pair.Value.SequenceEqual(celIndicies))
                    {
                        Debug.WriteLine($">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length}\tsimilarity 100pct\t {Helpers.StringifyVector(pair.Value)}");
                        return pair.Key;
                    }

                    // Tried following:
                    //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(arr).Count()) / Math.Max(arr.Length, pair.Value.Count())));
                    //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(celIndicies).Count()) / (double)pair.Value.Length));// ;
                    var numOfSameBitsPct = pair.Value.Intersect(celIndicies).Count();
                    if (numOfSameBitsPct > maxSameBits)
                    {
                        Debug.WriteLine($">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");
                        maxSameBits = numOfSameBitsPct;
                        predictedValue = pair.Key;
                        indxOfMatchingInp = n;
                    }
                    else
                        Debug.WriteLine($"<indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");

                    n++;
                }
            }

            return predictedValue;
        }
        /*
        //
        // This loop peeks the best input
        foreach (var pair in this.activeMap)
        {
            //
            // We compare only outputs which are similar in the length.
            // This is important, because some outputs, which are not related to the comparing output
            // might have much mode cells (length) than the current output. With this, outputs with much more cells
            // would be declared as matching outputs even if they are not.
            if ((Math.Min(arr.Length, pair.Key.Length) / Math.Max(arr.Length, pair.Key.Length)) > 0.9)
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
    }*/


        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public void TraceState(string fileName = null)
        {
            StreamWriter sw = null;
            if (fileName != null)
                sw = new StreamWriter(fileName);
            else
                sw = new StreamWriter(fileName.Replace(".csv", "HtmClassifier.state.csv"));

            List<TIN> processedValues = new List<TIN>();

            foreach (var item in m_ActiveMap2)
            {
                Debug.WriteLine("");
                Debug.WriteLine($"{item.Key}");
                Debug.WriteLine($"{Helpers.StringifyVector(item.Value)}");

                sw.WriteLine("");
                sw.WriteLine($"{item.Key}");
                sw.WriteLine($"{Helpers.StringifyVector(item.Value)}");
            }

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }

            Debug.WriteLine("........... Cell State .............");

            using (var cellStateSw = new StreamWriter(fileName.Replace(".csv", "HtmClassifier.fullstate.csv")))
            {
                foreach (var item in m_AllInputs)
                {
                    Debug.WriteLine("");
                    Debug.WriteLine($"{item.Key}");

                    cellStateSw.WriteLine("");
                    cellStateSw.WriteLine($"{item.Key}"); 
                    foreach (var cellState in item.Value)
                    {
                        var str = Helpers.StringifyVector(cellState);
                        Debug.WriteLine(str);
                        cellStateSw.WriteLine(str);
                    }
                }
            }
        }



        /*
    /// <summary>
    /// Traces out all cell indicies grouped by input value.
    /// </summary>
    public void TraceState2(string fileName = null)
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
     */


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

        private int PredictNextValue(int[] activeArr, int[] predictedArr)
        {
            var same = predictedArr.Intersect(activeArr);

            return same.Count();
        }


    }
}
