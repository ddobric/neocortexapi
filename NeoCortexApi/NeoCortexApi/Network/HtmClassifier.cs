using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NeoCortexApi.Entities;

namespace NeoCortexApi.Network
{
    public class HtmClassifier<TIN, TOUT>
    {
        private Dictionary<string, TIN> outputMap = new Dictionary<string, TIN>();

        private Dictionary<string, TIN> predictMap = new Dictionary<string, TIN>();

        public void Learn(TIN input, Cell output, Cell predictedOutput)
        {
            this.outputMap.Add(ComputeHash(flatArray(output)), input);

            this.predictMap.Add(ComputeHash(flatArray(predictedOutput)), input);
        }

        /// <summary>
        /// Get corresponding input value for current cycle.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public TIN GetInputValue(int[,] output)
        {
            return default(TIN);
        }


        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public TIN GetPredictedInputValue(int[,] output)
        {
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

        private static byte[] flatArray(int[,] output)
        {
            byte[] arr = new byte[output.LongLength];
            var lenX = output.GetLength(0);
            var lenY = output.GetLength(1);
            for (int x = 0; x < lenX; x += 1)
            {
                for (int y = 0; y < lenY; y += 1)
                {
                   arr[lenX*x+y] = (byte)output[x, y];
                }
            }

            return arr;
        }
    }
}
