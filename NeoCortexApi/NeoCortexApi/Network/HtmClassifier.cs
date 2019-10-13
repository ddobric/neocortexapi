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
        private Dictionary<int[], TIN> activeMap = new Dictionary<int[], TIN>();

        private Dictionary<int[], TIN> predictMap = new Dictionary<int[], TIN>();

        private Dictionary<TIN, int[]> activeArray = new Dictionary<TIN, int[]>();

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

            if (!activeArray.ContainsKey(input))
            {
                this.activeArray.Add(input, GetCellIndicies(output));
            }

            if (!predictMap.ContainsKey(GetCellIndicies(predictedOutput)))
            {
                this.predictMap.Add(GetCellIndicies(predictedOutput), input);
            }
        }

        public void Learn(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            if (!activeMap.ContainsKey(GetCellIndicies(output)))
            {
                this.activeMap.Add(GetCellIndicies(output), input);
            }

            if (!activeArray.ContainsKey(input))
            {
                this.activeArray.Add(input, GetCellIndicies(output));
            }

            if (!predictMap.ContainsKey(GetCellIndicies(predictedOutput)))
            {
                this.predictMap.Add(GetCellIndicies(predictedOutput), input);
            }
        }


        ///// <summary>
        ///// Get corresponding input value for current cycle.
        ///// </summary>
        ///// <param name="output"></param>
        ///// <returns></returns>
        //public TIN GetInputValue(Cell[] output)
        //{
        //    /*
        //    if (output.Length != 0 && activeMap.ContainsKey(FlatArray1(output)))
        //    {
        //        return activeMap[FlatArray1(output)];
        //    }
        //    */

        //    //int k = 0;
        //    foreach (int[] arr in activeMap.Keys)
        //    {
        //        var arr2 = GetCellIndicies(output);
        //        var rs = MathHelpers.GetHammingDistance(arr, arr2, true);
        //        //Debug.WriteLine($">> {rs}");
        //        if (arr.SequenceEqual(arr2))
        //        {
        //            return activeMap[arr];
        //        }
        //    }
        //    return default(TIN);
        //}


        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public TIN GetPredictedInputValue(Cell[] output)
        {
            int maxSameBits = 0;
            TIN charOutput = default(TIN);
            int[] arr = new int[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = output[i].Index;
            }
            if (output.Length != 0)
            {
                foreach (TIN inputVal in activeArray.Keys)
                {
                    int numOfSameBits = predictNextValue(arr, activeArray[inputVal]);
                    if (numOfSameBits > maxSameBits)
                    {
                        maxSameBits = numOfSameBits;
                        charOutput = (TIN)inputVal;
                    }
                }
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
