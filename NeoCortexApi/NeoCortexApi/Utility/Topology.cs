using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoCortexApi.Utility
{
    [Serializable]
    public class Topology : Coordinator //implements Serializable
    {
        /** keep it simple */
        private static readonly long serialVersionUID = 1L;

        private IntGenerator[] igs;
        private int[] centerPosition;


        /**
         * Constructs a new {@link AbstractFlatMatrix} object to be configured with specified
         * dimensions and major ordering.
         * @param shape  the dimensions of this matrix 
         */
        public Topology(int[] shape) : base(shape, false)
        {

        }

        /**
         * Translate an index into coordinates, using the given coordinate system.
         * 
         * @param index     The index of the point. The coordinates are expressed as a single index by
         *                  using the dimensions as a mixed radix definition. For example, in dimensions
         *                  42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
         * @return          A array of coordinates of length len(dimensions).
         */
        public int[] GetCoordinatesFromIndex(int index)
        {
            return computeCoordinates(index);
        }

        /**
         * Translate coordinates into an index, using the given coordinate system.
         * 
         * @param coordinates       A array of coordinates of length dimensions.size().
         * @param shape             The coordinate system.
         * @return                  The index of the point. The coordinates are expressed as a single index by
         *                          using the dimensions as a mixed radix definition. For example, in dimensions
         *                          42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
         */
        public int GndexFromCoordinates(int[] coordinates)
        {
            return computeIndex(coordinates);
        }

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
        public int[] GetNeighborhood(int centerIndex, int radius)
        {
            centerPosition = GetCoordinatesFromIndex(centerIndex);

            IntGenerator[] intGens = new IntGenerator[dimensions.Length];
            for (int i = 0; i < dimensions.Length; i++)
            {
                intGens[i] = new IntGenerator(Math.Max(0, centerPosition[i] - radius),
                    Math.Min(dimensions[i] - 1, centerPosition[i] + radius) + 1);
            }

            List<List<int>> result = new List<List<int>>();

            result.Add(new List<int>());

            List<List<int>> interim = new List<List<int>>();

            foreach (IntGenerator gen in intGens)
            {
                interim.Clear();
                interim.AddRange(result);
                result.Clear();

                foreach (var lx in interim)
                {
                    gen.reset();

                    for (int y = 0; y < gen.size(); y++)
                    {
                        int py = gen.next();
                        List<int> tl = new List<int>();
                        tl.AddRange(lx);
                        tl.Add(py);
                        result.Add(tl);
                    }
                }
            }

            return result.Select((tl) => GndexFromCoordinates(tl.ToArray())).ToArray();
        }

        /**
         * Like {@link #neighborhood(int, int)}, except that the neighborhood isn't truncated when it's
         * near an edge. It wraps around to the other side.
         * 
         * @param centerIndex       The index of the point. The coordinates are expressed as a single index by
         *                          using the dimensions as a mixed radix definition. For example, in dimensions
         *                          42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
         * @param radius            The radius of this neighborhood about the centerIndex.
         * @return  The points in the neighborhood, including centerIndex.
         */
        public int[] wrappingNeighborhood(int centerIndex, int radius)
        {
            int[] cp = GetCoordinatesFromIndex(centerIndex);

            // Dims of columns
            IntGenerator[] intGens = new IntGenerator[dimensions.Length];
            for (int i = 0; i < dimensions.Length; i++)
            {
                intGens[i] = new IntGenerator(cp[i] - radius,
                    Math.Min((cp[i] - radius) + dimensions[i] - 1, cp[i] + radius) + 1);
            }

            List<List<int>> result = new List<List<int>>();

            result.Add(new List<int>());

            List<List<int>> interim = new List<List<int>>();

            int k = 0;
            foreach (IntGenerator gen in intGens)
            {
                interim.Clear();
                interim.AddRange(result);
                result.Clear();

                foreach (var lx in interim)
                {
                    gen.reset();

                    for (int y = 0; y < gen.size(); y++)
                    {
                        int py = ArrayUtils.modulo(gen.next(), dimensions[k]);
                        //int py = gen.next() % dimensions[k];

                        List<int> tl = new List<int>();
                        tl.AddRange(lx);
                        tl.Add(py);
                        result.Add(tl);
                    }
                }

                k++;
            }

            return result.Select((tl) => GndexFromCoordinates(tl.ToArray())).ToArray();
        }
    }
}
