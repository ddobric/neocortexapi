using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{

    /// <summary>
    /// Transforms coordinates from multidimensional space into the single dimensional space of flat indexes.
    /// </summary>
    /// <remarks>
    /// Authors:
    /// cogmission, Damir Dobric.
    /// </remarks>
    public class Coordinator
    {

        protected int[] dimensions;
        protected int[] dimensionMultiples;

        /// <summary>
        /// 
        /// </summary>
        protected bool isColumnMajor;


        protected int numDimensions;


        public HtmModuleTopology HtmTopology
        {
            get
            {
                return new HtmModuleTopology(this.dimensions, this.isColumnMajor);
            }
        }

        /**
         * Constructs a new {@link Coordinator} object to be configured with specified
         * dimensions and major ordering.
         * @param shape  the dimensions of this matrix 
         */
        public Coordinator(int[] shape) : this(shape, false)
        {

        }

        /**
         * Constructs a new {@link Coordinator} object to be configured with specified
         * dimensions and major ordering.
         * 
         * @param shape                     the dimensions of this sparse array 
         * @param useColumnMajorOrdering    flag indicating whether to use column ordering or
         *                                  row major ordering. if false (the default), then row
         *                                  major ordering will be used. If true, then column major
         *                                  ordering will be used.
         */
        public Coordinator(int[] shape, bool useColumnMajorOrdering)
        {
            this.dimensions = shape;
            this.numDimensions = shape.Length;
            this.dimensionMultiples = initDimensionMultiples(
                useColumnMajorOrdering ? HtmCompute.Reverse(shape) : shape);
            isColumnMajor = useColumnMajorOrdering;
        }

        ///**
        // * Returns a flat index computed from the specified coordinates
        // * which represent a "dimensioned" index.
        // * 
        // * @param   coordinates     an array of coordinates
        // * @return  a flat index
        // */
        //public int computeIndex(int[] coordinates)
        //{
        //    int[] localMults = isColumnMajor ? HtmCompute.Reverse(dimensionMultiples) : dimensionMultiples;
        //    int baseNum = 0;
        //    for (int i = 0; i < coordinates.Length; i++)
        //    {
        //        baseNum += (localMults[i] * coordinates[i]);
        //    }
        //    return baseNum;
        //}


        ///// <summary>
        ///// Calculates multidimensional coordinates from flat array index.
        ///// </summary>
        ///// <param name="index">Flat index.</param>
        ///// <returns>Coordinates in multidimensional space.</returns>
        //public static int[] calcCoordinatesFrmFlatIndex(int index, HtmModuleTopology topology)
        //{
        //    int[] returnVal = new int[topology.NumDimensions];
        //    int baseNum = index;
        //    for (int i = 0; i < topology.DimensionMultiplies.Length; i++)
        //    {
        //        int quotient = baseNum / topology.DimensionMultiplies[i];
        //        baseNum %= topology.DimensionMultiplies[i];
        //        returnVal[i] = quotient;
        //    }
        //    return topology.IsMajorOrdering ? Reverse(returnVal) : returnVal;
        //}



        /**
         * Initializes internal helper array which is used for multidimensional
         * index computation.
         * @param dimensions matrix dimensions
         * @return array for use in coordinates to flat index computation.
         */
        protected int[] initDimensionMultiples(int[] dimensions)
        {
            int holder = 1;
            int len = dimensions.Length;
            int[] dimensionMultiples = new int[numDimensions];
            for (int i = 0; i < len; i++)
            {
                holder *= (i == 0 ? 1 : dimensions[len - i]);
                dimensionMultiples[len - 1 - i] = holder;
            }
            return dimensionMultiples;
        }

     
    }
}
