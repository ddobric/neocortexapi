using System;
using System.Collections.Generic;
using System.Text;
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
        public static double GetHammingDistance(int[] originArray, int[] comparingArray)
        {

            double[] arr1 = ArrayUtils.toDoubleArray(originArray);
            double[] arr2 = ArrayUtils.toDoubleArray(comparingArray);
            return GetHammingDistance(new double[][] { arr1 }, new double[][] { arr2 })[0];
        }

        /// <summary>
        /// Calculates the hamming distance between arrays.
        /// </summary>
        /// <param name="originArray">Original array to compare from.</param>
        /// <param name="comparingArray">Array to compare to.</param>
        /// <returns>Hamming distance.</returns>
        public static double[] GetHammingDistance(double[][] originArray, double[][] comparingArray)
        {
            double[][] hDistance = new double[originArray.Length][];
            double[] h = new double[originArray.Length];
            double[] hammingDistance = new double[originArray.Length];

            for (int i = 0; i < originArray.Length; i++)
            {
                int len = Math.Max(originArray[i].Length, comparingArray[i].Length);
                int sum = 0;
                for (int j = 0; j < len; j++)
                {
                    if (originArray[i].Length > j && comparingArray[i].Length > j)
                    {
                        if (originArray[i][j] == comparingArray[i][j])
                        {
                            sum = sum + 0;
                        }
                        else
                        {
                            sum++;
                        }
                    }
                    else
                        sum++;
                }

                h[i] = sum;
                if (originArray[i].Length > 0)
                    hammingDistance[i] = ((originArray[i].Length - sum) * 100 / originArray[i].Length);
                else
                    hammingDistance[i] = double.PositiveInfinity;
            }

            return hammingDistance;
        }

    }
}
