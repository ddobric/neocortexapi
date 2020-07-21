using Akka.Event;
using Akka.Pattern;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi
{
    public class HomeostaticPlasticityActivator
    {
        private Connections htmMemory;

        private int cycle = 0;

        private int numLearnedElems = 0;

        private int minCycles;

        private int lastNumOfLernedElems = 0;

        private int numOfUnchangedNumOfLearnedElems = 0;

        private int[] numOfActiveCols;

        /// <summary>
        /// Number of cycles to repeat patter to make sure that its SDR is not changed.
        /// Counting starts after boosting is disabled by neborn effact.
        /// </summary>
        private int requiredNumOfStableCycles;

        /// <summary>
        /// List of hashes. [key, val] = [hash(input), hash(output)]
        /// </summary>
        private Dictionary<string, int[]> map = new Dictionary<string, int[]>();

        /// <summary>
        /// Action to be invoked when the SP become stable or instable.
        /// <is stable, num of seen patterns, derivation of active column function, cycle>
        /// </summary>
        private Action<bool, int, int, int> onStabilityStatusChanged;

        /// <summary>
        /// Set on true when SP deactivates boosting and enter the stable state.
        /// Once SP enters the stable state and it becomes instable agein, this value is ste on false.
        /// </summary>
        private bool isStable = false;

        public HomeostaticPlasticityActivator(Connections htmMemory, int minCycles, Action<bool, int, int, int> onStabilityStatusChanged, int numOfCyclesToWaitOnChange = 5)
        {
            this.onStabilityStatusChanged = onStabilityStatusChanged;
            this.htmMemory = htmMemory;
            this.minCycles = minCycles;
            this.requiredNumOfStableCycles = numOfCyclesToWaitOnChange;
        }

        public bool Compute(int[] input, int[] output)
        {
            bool res = false;

            var avgDerivation = -1;

            //
            // This value is set when the neborn effect starts. Means, when the boosting is disabled.
            // In this case we want to make sure that all learned inputs have the same number of active columns.
            // SP sometimes generates SDRs with different number of active columns. When that happen,
            // we will continue learning until all SDRs have same number of active columns.
            if (numOfActiveCols != null)
            {
                var indx = cycle % numOfActiveCols.Length;

                //
                // Here we track the number of active columns for every cycle.
                // We want that this number for every input is approximately the same.
                numOfActiveCols[indx] = output.Count(c => c == 1);

                int sum = 0;

                //
                // We calculate here the derivation of the the active column function across 
                // all inputs.
                for (int i = 0; i < numOfActiveCols.Length - 1; i++)
                {
                    // Sum of derivations
                    sum += Math.Abs(numOfActiveCols[i] - numOfActiveCols[i + 1]);
                }

                avgDerivation = sum / numOfActiveCols.Length;
            }

            var h1 = GetHash(input);
            //var h2 = GetHash(output);

            //
            // If the pattern appears for the first time, add it to dictionary of seen patterns.
            if (!map.ContainsKey(h1))
            {
                map.Add(h1, output);
            }
            else
            {
                var similarity = Correlate(map[h1], output);
                if (similarity > 0.96)
                {
                    // We replace the existing value with the new one.
                    map[h1] = output;

                    numLearnedElems++;

                    //
                    // Once the SP has learned all elements we start counting the derivation
                    // over the number of active columns.
                    if (numLearnedElems == map.Keys.Count)
                    {
                        numOfActiveCols = new int[numLearnedElems];
                    }

                    if (numLearnedElems >= map.Keys.Count && cycle >= this.minCycles)
                    {
                        this.htmMemory.setMaxBoost(0.0);
                        this.htmMemory.updateMinPctOverlapDutyCycles(0.0);

                        // If no new element learned, we will wait for next few cycles
                        if ((map.Keys.Count * 10) == lastNumOfLernedElems)
                        {
                            numOfUnchangedNumOfLearnedElems++;
                            if (numOfUnchangedNumOfLearnedElems >= requiredNumOfStableCycles * map.Keys.Count && avgDerivation == 0)
                            {
                                // We fire event when changed from instable to stable.
                                if (!isStable)
                                    this.onStabilityStatusChanged(true, map.Keys.Count, avgDerivation, cycle);

                                isStable = true;
                                res = true;
                            }
                            else
                            {
                                //
                                // If SP has entered the stable state and average derivation is not a zero,
                                // the SP started changed SDRs again. This means SP becomes unstable.
                                if (isStable)
                                {
                                    isStable = false;
                                    this.onStabilityStatusChanged(false, map.Keys.Count, avgDerivation, cycle);
                                }
                            }
                        }
                        else
                            lastNumOfLernedElems++;
                    }
                }
                else
                {
                    if (isStable)
                    {

                    }

                    // We replace the previous SDR with the new one.
                    // This is the common way of iterative learning of SP.
                    map[h1] = output;
                }
            }

            this.cycle++;

            return res;
        }

        /// <summary>
        /// Calculates the correlation of bits in ywo arrays.
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>Similarity of two arrays.</returns>
        public static double Correlate(int[] data1, int[] data2)
        {
            double min = Math.Min(data1.Length, data2.Length);
            double max = Math.Max(data1.Length, data2.Length);

            double sum = 0;

            for (int i = 0; i < min; i++)
            {
                if (data1[i] == data2[i])
                    sum++;
            }

            return sum / max;
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

        //internal static string ArrayToString(int[] input)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var item in input)
        //    {
        //        sb.Append(item == 1 ? "1" : "0");
        //    }

        //    return sb.ToString();
        //}

        //internal static int[] ToIntArray(string data)
        //{
        //    int[] val = new int[data.Length];

        //    for (int i = 0; i < val.Length; i++)
        //    {
        //        if (data[i] == '1')
        //            val[i] = 1;
        //        else
        //            val[i] = 0;
        //    }

        //    return val;
        //}
    }
}
