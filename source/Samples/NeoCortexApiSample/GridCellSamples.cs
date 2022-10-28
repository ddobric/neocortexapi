using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridCell;
using GridCell.js;

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
            int arenaSize = 100;

            var gridConfig = new GConfig(arenaSize);
            var grid = new GCell(gridConfig);

            var simulation = new GSimulation(grid);
            simulation.RunContinuous();
        }
    }
}
