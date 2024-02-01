using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridCell;
using GridCell.encoder;
using GridCell.js;
using NeoCortexApi;

namespace NeoCortexApiSample
{
    public class GridCellSamples
    {
        //public void Run()
        //{
        //    int walk = 20;
        //    int arenaSize = 50;

        //    var spatial = new SpatialNavigation(arenaSize);
        //    spatial.RandomNavigation(walk);

        //    var gridConfig = new GridConfig(arenaSize);
        //    var grid = new Grid(gridConfig);

        //    var simulation = new Simulation(grid, spatial.txx, spatial.tyy);
        //    simulation.Run();
        //}

        public void Run()
        {
            //int arenaSize = 100;

            //var gridConfig = new GConfig(arenaSize);
            //var grid = new GCell(gridConfig);

            //var simulation = new GSimulation(grid);
            //simulation.RunContinuous();

            var size = 100;
            var sdr = new GridCellSDR(size);
            var gc = new GridCellEncoder(size, sparsity: 0.25);

            var coordinates = new double[,] {
                {100, 100},
                {100, 101},
                {10, 10},
                {50, 50},
                {1000, 50},
                {5000, 9999},
                {100, 100},
            };


            Dictionary<string, int[]> encodedValues = new ();
            for (int i = 0; i < coordinates.GetLength(0); i++)
            {
                var activatedCells = gc.Encode(coordinates[i,0], coordinates[i,1]);

                Console.WriteLine(Helpers.StringifyVector(activatedCells.ToArray()));

                sdr.SetSparse(activatedCells.ToArray());

                Console.WriteLine(Helpers.StringifyVector(sdr.GetDense()));
                //Console.WriteLine(Helpers.StringifyVector(sdr.GetSparse()));

                //var indexes = sdr.CalculateActiveCells().Select(cell => cell.Index).ToArray();

                //Console.WriteLine(Helpers.StringifyVector(indexes));
                Console.WriteLine("---------------------------------------");

                //sdr.saveSDRImage($"x_{coordinates[i, 0]}_y_{coordinates[i, 1]}");

                encodedValues.Add($"x_{coordinates[i, 0]}_y_{coordinates[i, 1]}_({i})", (int[]) sdr.GetDense().Clone());
            }

            Console.WriteLine(Helpers.TraceSimilarities(encodedValues));
        }
    }
}
