using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Assotiate specified input to the given set of predictive cells.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="predictedOutput"></param>
        public void Learn(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            this.inputSequence.Add(input);

            this.inputSequenceMap.Add(GetCellIndicies(output), this.inputSequence.Count -1);

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
            bool x = false;
            int maxSameBits = 0;
            TIN charOutput = default(TIN);
            int[] arr = new int[predictiveCells.Length];
            for (int i = 0; i < predictiveCells.Length; i++)
            {
                arr[i] = predictiveCells[i].Index;
            }

            if (predictiveCells.Length != 0)
            {
                int indx = 0;
                Debug.WriteLine($"Item length: {predictiveCells.Length}\t Items: {this.activeMap.Keys.Count}");
                int n = 0;
                //foreach (TIN inputVal in activeArray.Keys)
                foreach (var pair in this.activeMap)
                {
                    int numOfSameBits = pair.Key.Intersect(arr).Count();
                    if (numOfSameBits > maxSameBits)
                    {
                        Debug.WriteLine($"cnt:{n}\t{pair.Value} = bits {numOfSameBits}\t {Helpers.StringifyVector(pair.Key)}");
                        maxSameBits = numOfSameBits;
                        charOutput = pair.Value;
                        indx = n;
                    }

                    n++;
                }

                Debug.Write("[ ");
                for (int i = Math.Max(0, indx - 3); i < Math.Min(indx + 3, this.activeMap.Keys.Count); i++)
                {
                    if (i == indx) Debug.Write("* ");
                    Debug.Write($"{this.inputSequence[i]}");
                    if (i == indx) Debug.Write(" *");

                    Debug.Write(", ");
                }
                Debug.WriteLine(" ]");

                return charOutput;
                //return activeMap[ComputeHash(FlatArray(output))];
            }
            return default(TIN);
        }


        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public void TraceState()
        {
            List<TIN> processedValues = new List<TIN>();

            foreach (var item in activeMap.Values)
            {
                if (processedValues.Contains(item) == false)
                {
                    Debug.WriteLine("");
                    Debug.WriteLine($"{item}");

                    foreach (var inp in this.activeMap.Where(i => EqualityComparer<TIN>.Default.Equals((TIN)i.Value, item)))
                    {
                        Debug.WriteLine($"{Helpers.StringifyVector(inp.Key)}");                        
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
