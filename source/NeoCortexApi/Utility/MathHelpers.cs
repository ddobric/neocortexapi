// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi.Utility
{
    public class MathHelpers
    {
        /// <summary>
        /// Calculates the hamming distance between arrays.
        /// </summary>
        /// <param name="originArray">Original array to compare from.</param>
        /// <param name="comparingArray">Array to compare to.</param>
        /// <returns>Hamming distance.</returns>
        public static double GetHammingDistance(int[] originArray, int[] comparingArray, bool countNoneZerosOnly = false)
        {

            double[] arr1 = ArrayUtils.ToDoubleArray(originArray);
            double[] arr2 = ArrayUtils.ToDoubleArray(comparingArray);
            return GetHammingDistance(new double[][] { arr1 }, new double[][] { arr2 }, countNoneZerosOnly)[0];
        }

        /// <summary>
        /// Calculates the hamming distance between arrays.
        /// </summary>
        /// <param name="originArray">Original array to compare from.</param>
        /// <param name="comparingArray">Array to compare to.</param>
        /// <returns>Hamming distance.</returns>
        public static double[] GetHammingDistance(double[][] originArray, double[][] comparingArray, bool countNoneZerosOnly = false)
        {
            double[][] hDistance = new double[originArray.Length][];
            double[] h = new double[originArray.Length];
            double[] hammingDistance = new double[originArray.Length];

            for (int i = 0; i < originArray.Length; i++)
            {
                int len = Math.Max(originArray[i].Length, comparingArray[i].Length);
                int numOfDifferentBits = 0;
                for (int j = 0; j < len; j++)
                {
                    if (originArray[i].Length > j && comparingArray[i].Length > j)
                    {
                        if (originArray[i][j] == comparingArray[i][j])
                        {
                            numOfDifferentBits = numOfDifferentBits + 0; //TODO meaning of this operation
                        }
                        else
                        {
                            if (countNoneZerosOnly == false)
                                numOfDifferentBits++;
                            else
                            {
                                if (originArray[i][j] == 1)
                                    numOfDifferentBits++;
                            }
                        }
                    }
                    else
                        numOfDifferentBits++;
                }

                h[i] = numOfDifferentBits;
                if (originArray[i].Length > 0 && originArray[i].Count(b => b == 1) > 0)
                {
                    //hammingDistance[i] = ((originArray[i].Length - numOfDifferentBits) * 100 / originArray[i].Length);
                    if (countNoneZerosOnly == true)
                    {
                        hammingDistance[i] = ((originArray[i].Count(b => b == 1) - numOfDifferentBits) * 100 / originArray[i].Count(b => b == 1));
                    }
                    else
                    {
                        hammingDistance[i] = ((originArray[i].Length - numOfDifferentBits) * 100 / originArray[i].Length);
                    }

                }
                else
                    hammingDistance[i] = double.NegativeInfinity;
            }

            return hammingDistance;
        }

        /// <summary>
        /// Calculates how many elements of the array are same in percents. This method is useful to compare 
        /// two arays that contains indicies of active columns. Please note that arrays must not contain elements with the same index.
        /// This method does not validate this for performance resons.
        /// </summary>
        /// <param name="originArray">Indexes of non-zero bits in the SDR.</param>
        /// <param name="comparingArray">Indexes of non-zero bits in the SDR.</param>
        /// <returns>Similarity between arrays 0.0-1.0</returns>
        public static double CalcArraySimilarity(int[] originArray, int[] comparingArray)
        {
            if (originArray.Length > 0 && comparingArray.Length > 0)
            {
                int cnt = 0;

                foreach (var item in comparingArray)
                {
                    if (originArray.Contains(item))
                        cnt++;
                }

                return ((double)cnt / (double)Math.Max(originArray.Length, comparingArray.Length)) * 100.0;
            }
            else
            {
                return -1.0;
            }
        }


        /// <summary>
        /// Calculates the similarity matrix from the list of SDRs (arrays). 
        /// It compares all given SDRs and calculate their similarity.
        /// </summary>
        /// <param name="actBitsIndexes">Dictionary of all output SDRs defined as indicies of active bits,</param>
        public static double[,] CalculateSimilarityMatrix(Dictionary<string, int[]> actBitsIndexes)
        {
            double[,] res = new double[actBitsIndexes.Count, actBitsIndexes.Count];

            var keyArray = actBitsIndexes.Keys.ToArray();

            for (int i = 0; i < keyArray.Length; i++)
            {
                var key1 = keyArray[i];

                for (int j = 0; j < keyArray.Length; j++)
                {
                    var key2 = keyArray[j];

                    int[] sdr1 = actBitsIndexes.GetValueOrDefault<string, int[]>(key1);
                    int[] sdr2 = actBitsIndexes.GetValueOrDefault<string, int[]>(key2);

                    double similarity = MathHelpers.CalcArraySimilarity(sdr1, sdr2);

                    res[i, j] = similarity;
                }
            }

            return res;
        }

        /// <summary>
        /// Calculates the memory of the SDR.
        /// </summary>
        /// <param name="w">The number of non-zero bits used to encode the value.</param>
        /// <param name="n">The total number of bits.</param>
        /// <returns>The number of possible patterns that can be encoded.</returns>
        public static double SdrMem(int w, int n)
        {
            return Factorial(n) / (Factorial(w) * (Factorial(n - w)));
        }


        /// <summary>
        /// Calculates the factorial of n.
        /// </summary>
        /// <param name="n">The value to calculate the factorial.</param>
        /// <returns></returns>
        public static double Factorial(int n)
        {
            double fact = 1;
            for (int i = 1; i <= n; i++)
            {
                fact *= i;
            }

            return fact;
        }
    }
}

