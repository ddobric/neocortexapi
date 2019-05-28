using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public static class HtmCompute
    {
        /// <summary>
        /// Calculates multidimensional coordinates from flat array index.
        /// </summary>
        /// <param name="index">Flat index.</param>
        /// <returns>Coordinates in multidimensional space.</returns>
        public static int[] GetCoordinatesFromIndex(int index, HtmModuleTopology topology)
        {
            int[] returnVal = new int[topology.NumDimensions];
            int baseNum = index;
            for (int i = 0; i < topology.DimensionMultiplies.Length; i++)
            {
                int quotient = baseNum / topology.DimensionMultiplies[i];
                baseNum %= topology.DimensionMultiplies[i];
                returnVal[i] = quotient;
            }
            return topology.IsMajorOrdering ? Reverse(returnVal) : returnVal;
        }

        /// <summary>
        /// Reurns reverse array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int[] Reverse(int[] input)
        {
            int[] retVal = new int[input.Length];
            for (int i = input.Length - 1, j = 0; i >= 0; i--, j++)
            {
                retVal[j] = input[i];
            }
            return retVal;
        }
    }
}
