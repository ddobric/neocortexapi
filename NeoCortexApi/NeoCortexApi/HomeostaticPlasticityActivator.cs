using Akka.Pattern;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi
{
    public class HomeostaticPlasticityActivator
    {
        private Connections htmMemory;

        private int cycle = 0;

        private int numLearnedElems = 0;

        /// <summary>
        /// List of hashes. [key, val] = [hash(input), hash(output)]
        /// </summary>
        private Dictionary<string, string> map = new Dictionary<string, string>();

        public HomeostaticPlasticityActivator(Connections htmMemory)
        {
            this.htmMemory = htmMemory;
        }

        public bool Compute(int[] input, int[] output)
        {
            bool res = false;

            var h1 = GetHash(input);
            var h2 = GetHash(output);

            if (!map.ContainsKey(h1))
            {
                map.Add(h1, h2);
            }
            else
            {
                if (map[h1] == h2 )
                {
                    numLearnedElems++;
                }

                if (numLearnedElems == map.Keys.Count)
                {
                    res = true;                    
                }
            }

            this.cycle++;

            return res;
        }

        /// <summary>
        /// Compute the hash from the array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string GetHash(int[] input)
        {
            List<byte> buff = new List<byte>();

            foreach (var item in input)
            {
                buff.AddRange(BitConverter.GetBytes(item));
            }

            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(buff.ToArray());

                return Encoding.UTF8.GetString(data);
            }
        }
    }
}
