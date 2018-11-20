using System;
using System.Collections.Generic;
using System.Text;

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
        public static double[] GetHammingDistance(this double[][] originArray, double[][] comparingArray)
        {
            double[][] hDistance = new double[originArray.Length][];
            double[] h = new double[originArray.Length];
            double[] accuracy = new double[originArray.Length];

            for (int i = 0; i < originArray.Length; i++)
            {
                if (originArray[i].Length != comparingArray[i].Length)
                {
                    throw new Exception("Data must be equal length");
                }

                int sum = 0;
                for (int j = 0; j < originArray[i].Length; j++)
                {
                    if (originArray[i][j] == comparingArray[i][j])
                    {
                        sum = sum + 0;
                    }
                    else
                    {
                        sum = sum + 1;
                    }
                }

                h[i] = sum;

                accuracy[i] = ((originArray[i].Length - sum) * 100 / originArray[i].Length);
            }

            return accuracy;
        }

    }
}
