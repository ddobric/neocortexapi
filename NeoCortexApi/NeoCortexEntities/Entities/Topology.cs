// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Utility;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Work in progress.
    /// </summary>

    public class Topology //: Coordinator 
    {
        /// <summary>
        /// Dimension shape.
        /// </summary>
        protected int[] dimensions;

        /// <summary>
        /// Used to calculate the flat intex of multidimensional array.
        /// </summary>
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
         * Constructs a new {@link AbstractFlatMatrix} object to be configured with specified
         * dimensions and major ordering.
         * @param shape  the dimensions of this matrix 
         */


        /// <summary>
        /// TODO to be added
        /// </summary>
        public Topology(int[] shape, bool useColumnMajorOrdering = false) //: base(shape, false)
        {
            if (shape == null)
                return;

            this.dimensions = shape;
            this.numDimensions = shape.Length;
            this.dimensionMultiples = InitDimensionMultiples(
                useColumnMajorOrdering ? HtmCompute.Reverse(shape) : shape);
            isColumnMajor = useColumnMajorOrdering;
        }

        /// <summary>
        /// Initializes internal helper array which is used for multidimensional index computation.
        /// </summary>
        /// <param name="dimensions">matrix dimensions</param>
        /// <returns>array for use in coordinates to flat index computation.</returns>
        protected int[] InitDimensionMultiples(int[] dimensions)
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

        /**
         * Translate an index into coordinates, using the given coordinate system.
         * 
         * @param index     The index of the point. The coordinates are expressed as a single index by
         *                  using the dimensions as a mixed radix definition. For example, in dimensions
         *                  42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
         * @return          A array of coordinates of length len(dimensions).
         */
        //public int[] GetCoordinatesFromIndex(int index)
        //{
        //    return calcCoordinatesFrmFlatIndex(index, new HtmModuleTopology(this.dimensions, this.isColumnMajor));
        //}

        /**
         * Translate coordinates into an index, using the given coordinate system.
         * 
         * @param coordinates       A array of coordinates of length dimensions.size().
         * @param shape             The coordinate system.
         * @return                  The index of the point. The coordinates are expressed as a single index by
         *                          using the dimensions as a mixed radix definition. For example, in dimensions
         *                          42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
         */
        //public int GetIndexFromCoordinates(int[] coordinates)
        //{
        //    return computeIndex(coordinates);
        //}

        /**
         * Get the points in the neighborhood of a point.
         *
         * A point's neighborhood is the n-dimensional hypercube with sides ranging
         * [center - radius, center + radius], inclusive. For example, if there are two
         * dimensions and the radius is 3, the neighborhood is 6x6. Neighborhoods are
         * truncated when they are near an edge.
         * 
         * @param centerIndex       The index of the point. The coordinates are expressed as a single index by
         *                          using the dimensions as a mixed radix definition. For example, in dimensions
         *                          42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
         * @param radius            The radius of this neighborhood about the centerIndex.
         * @return  The points in the neighborhood, including centerIndex.
         */
        //public int[] GetNeighborhood(int centerIndex, int radius)
        //{
        //    var centerPosition = HtmCompute.GetCoordinatesFromIndex(centerIndex, this.HtmTopology);

        //    IntGenerator[] intGens = new IntGenerator[dimensions.Length];
        //    for (int i = 0; i < dimensions.Length; i++)
        //    {
        //        intGens[i] = new IntGenerator(Math.Max(0, centerPosition[i] - radius),
        //            Math.Min(dimensions[i] - 1, centerPosition[i] + radius) + 1);
        //    }

        //    List<List<int>> result = new List<List<int>>();

        //    result.Add(new List<int>());

        //    List<List<int>> interim = new List<List<int>>();

        //    foreach (IntGenerator gen in intGens)
        //    {
        //        interim.Clear();
        //        interim.AddRange(result);
        //        result.Clear();

        //        foreach (var lx in interim)
        //        {
        //            gen.reset();

        //            for (int y = 0; y < gen.size(); y++)
        //            {
        //                int py = gen.next();
        //                List<int> tl = new List<int>();
        //                tl.AddRange(lx);
        //                tl.Add(py);
        //                result.Add(tl);
        //            }
        //        }
        //    }

        //    return result.Select((tl) => GetIndexFromCoordinates(tl.ToArray())).ToArray();
        //}


    }
}
