using System;
using System.Linq;
using System.Collections.Generic;
using NumSharp;
using HexagonalLib;

namespace GridCell.encoder
{
    public class GridCellEncoder
    {
        private int size;

        // Argument periods is a list of distances. The period of a module is the
        // distance between the centers of a grid cells receptive fields.The
        // length of this list defines the number of distinct modules.
        private double[] periods = new double[5];

        private List<int[]> partitions = new();
        private NDArray offsets_;

        private List<Tuple<double, double>[]> rotationMatrix = new();


        // Argument sparsity is fraction of bits which this encoder activates in
        // the output SDR.
        private double sparsity;

        public GridCellEncoder(int size, double sparsity = 0.15)
        {
            this.size = size;
            this.sparsity = sparsity;

            // should be passed as input
            for (var i = 0; i < 5; i++)
            {
                periods[i] = 6 * Math.Pow(Math.Pow(2, 0.5), i);
            }

            var partitions = np.linspace(0, size, periods.Length + 1);
            for (var i = 0; i < partitions.size - 1; i++)
            {
                this.partitions.Add(new int[2] {
                    (int) ((float) partitions[i].GetValue()), (int) ((float)partitions[i+1].GetValue()),
                });
            }

            // Assign each module a random offset and orientation.
            var rng = np.random.RandomState(23);
            offsets_ = rng.uniform(0, periods.Max() * 9, new int[] { size, 2 });

            foreach (var period in periods)
            {
                var angle = rng.uniform(0, 1, np.float32) * 2 * Math.PI;
                var c = Math.Cos(angle);
                var s = Math.Sin(angle);

                Tuple<double, double>[] sdist = {
                    Tuple.Create(c, -s),
                    Tuple.Create(s, c)
                };

                rotationMatrix.Add(sdist);
            }

        }


        /**
         * Transform a 2-D coordinate into an SDR.
         */
        public List<int> Encode(double locX, double locY)
        {
            var location = np.ndarray((1,2));
            location[0][0] = locX;
            location[0][1] = locY;


            // Find the distance from the location to each grid cells nearest
            // receptive field center.
            // Convert the units of location to hex grid with angle 0, scale 1, offset 0.
            var displacement = location - offsets_;
            

            for (var i = 0; i < partitions.Count; i++) {
                var start = partitions[i][0];
                var stop = partitions[i][1];

                var r = rotationMatrix[i];

                var rr = np.ndarray((2,2));
                rr[0][0] = r[0].Item1;
                rr[0][1] = r[0].Item2;
                rr[1][0] = r[1].Item1;
                rr[1][1] = r[1].Item2;

                displacement[$"{start}:{stop}"] = rr.dot(displacement[$"{start}:{stop}"].T).T;
            }


            var radius = np.empty(size);
            for (var i = 0; i < periods.Length; i++) {
                var start = partitions[i][0];
                var stop = partitions[i][1];

                radius[$"{start}:{stop}"] = periods[i] / 2;
            }

            // Convert into and out of hexagonal coordinates, which rounds to the
            // nearest hexagons center.
            NDArray nearestArray = np.ndarray((size, 2));
            for (var i = 0; i < size; i++)
            {
                var grid = new HexagonalGrid(HexagonalGridType.PointyEven, (float)radius[i].GetValue<Double>());
                var x = displacement[i][0].GetValue<Double>();
                var y = displacement[i][1].GetValue<Double>();
                //var offset = new HexagonalLib.Coordinates.Offset((int)x, (int)y);

                var nearest = grid.ToPoint2(grid.ToCubic(x, y));
                nearestArray[i][0] = nearest.X;
                nearestArray[i][1] = nearest.Y;
            }

            var nearestMinusDisplacement = nearestArray - displacement;

            //var distances = np.ndarray((100, 1));
            //Find the distance between the location and the RF center.
            var distances = new double[size];
            for (var i = 0; i < size; i++)
            {
                var x = nearestMinusDisplacement[i][0];
                var y = nearestMinusDisplacement[i][1];
                var hypot = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                distances[i] = hypot;
            }


            // Activate the closest grid cells in each module.
            var activatedCells = new List<int>();
            foreach (var partition in partitions)
            {
                var start = partition[0];
                var stop = partition[1];
                var z = (int) (Math.Round(sparsity * (stop - start)));
                

                var indexes = distances[start..stop]
                .Select((x, i) => new KeyValuePair<double, int>(x, i))
                .OrderBy(x => x.Key)
                .Take(z)
                .Select((x, i) => x.Value + start)
                .ToList();

                activatedCells.AddRange(indexes);
            }

            activatedCells.Sort();
            return activatedCells;
        }
    }
}