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

            var gc = new GridCellEncoder(100);

            var coordinates = new double[,] {
                {1, 1},
                {5, 6},
                {10, 10},
                {50, 50}
            };

            for (int i = 0; i < coordinates.GetLength(0); i++)
            {
                var activatedCells = gc.Encode(coordinates[i,0], coordinates[i,1]);

                Console.WriteLine(Helpers.StringifyVector(activatedCells.ToArray()));
            }

        }
    }
}
