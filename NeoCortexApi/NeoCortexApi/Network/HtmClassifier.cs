using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NeoCortexApi.Entities;

namespace NeoCortexApi.Network
{
    public class HtmClassifier<TIN, TOUT>
    {
        private Dictionary<string, TIN> activeMap = new Dictionary<string, TIN>();

        private Dictionary<string, TIN> predictMap = new Dictionary<string, TIN>();

        public void Learn(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            if (!activeMap.ContainsKey(ComputeHash(flatArray(output))))
            {
                this.activeMap.Add(ComputeHash(flatArray(output)), input);
            }
            
            if (!predictMap.ContainsKey(ComputeHash(flatArray(predictedOutput))))
            {
                this.predictMap.Add(ComputeHash(flatArray(predictedOutput)), input);
            }
        }

        /// <summary>
        /// Get corresponding input value for current cycle.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public TIN GetInputValue(Cell[] output)
        {
            if (output.Length != 0 && activeMap.ContainsKey(ComputeHash(flatArray(output))))
            {
                return activeMap[ComputeHash(flatArray(output))];
            }
            return default(TIN);
        }


        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public TIN GetPredictedInputValue(Cell[] output)
        {
            if (output.Length != 0 && activeMap.ContainsKey(ComputeHash(flatArray(output))))
            {
                return activeMap[ComputeHash(flatArray(output))];
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
        

        
        private static byte[] flatArray(Cell[] output)
        {
            byte[] arr = new byte[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = (byte)output[i].Index;
            }
            return arr;
        }
        
    }
}
