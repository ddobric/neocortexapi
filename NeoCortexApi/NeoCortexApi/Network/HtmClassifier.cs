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
    public class HtmClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private List<TIN> inputSequence = new List<TIN>();

        private Dictionary<int[], int> inputSequenceMap = new Dictionary<int[], int>();

        private Dictionary<int[], TIN> activeMap = new Dictionary<int[], TIN>();

        //private Dictionary<int[], TIN> predictMap = new Dictionary<int[], TIN>();

        //private Dictionary<TIN, int[]> activeArray = new Dictionary<TIN, int[]>();

        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            throw new NotImplementedException();
        }

        public TOUT Inference(Cell[] activeCells)
        {
            throw new NotImplementedException();
        }

        public void Learn1(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            var outIndicies = GetCellIndicies(output);

          
            if (!activeMap.ContainsKey(GetCellIndicies(output)))
            {
                this.activeMap.Add(GetCellIndicies(output), input);
            }

            //if (!activeArray.ContainsKey(input))
            //{
            //    this.activeArray.Add(input, GetCellIndicies(output));
            //}

            //if (!predictMap.ContainsKey(GetCellIndicies(predictedOutput)))
            //{
            //    this.predictMap.Add(GetCellIndicies(predictedOutput), input);
            //}
        }

        public void Learn(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            this.inputSequence.Add(input);

            this.inputSequenceMap.Add(GetCellIndicies(output), this.inputSequence.Count -1);

            if (!activeMap.ContainsKey(GetCellIndicies(output)))
            {
                this.activeMap.Add(GetCellIndicies(output), input);
            }

            //if (!activeArray.ContainsKey(input))
            //{
            //    this.activeArray.Add(input, GetCellIndicies(output));
            //}

            //if (!predictMap.ContainsKey(GetCellIndicies(predictedOutput)))
            //{
            //    this.predictMap.Add(GetCellIndicies(predictedOutput), input);
            //}
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

        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public TIN GetPredictedInputValue(Cell[] output)
        {
            bool x = false;
            int maxSameBits = 0;
            TIN charOutput = default(TIN);
            int[] arr = new int[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = output[i].Index;
            }

            if (output.Length != 0)
            {
                int indx = 0;
                Debug.WriteLine($"Item length: {output.Length}\t Items: {this.activeMap.Keys.Count}");
                int n = 0;
                //foreach (TIN inputVal in activeArray.Keys)
                foreach (var pair in this.activeMap)
                {
                    //int numOfSameBits = predictNextValue(arr, activeArray[inputVal]);
                    int numOfSameBits = pair.Key.Intersect(arr).Count();
                    //int numOfSameBits = predictNextValue(arr, activeArray[inputVal]);
                    if (numOfSameBits > maxSameBits)
                    {
                        Debug.WriteLine($"cnt:{n}\t{n}\t{pair.Value} = bits {numOfSameBits}");
                        maxSameBits = numOfSameBits;
                        charOutput = pair.Value;
                        indx = n;
                    }

                    n++;
                }

                Debug.Write("[ ");
                for (int i = Math.Max(0, indx-3); i < Math.Min(indx + 3, this.activeMap.Keys.Count); i++)
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
