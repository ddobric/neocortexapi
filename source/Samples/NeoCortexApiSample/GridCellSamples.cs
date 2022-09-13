using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridCell;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace NeoCortexApiSample
{
    public class GridCellSamples
    {
        public void Run()
        {
            int arenaSize = 50;
            var spatial = new SpatialNavigation(arenaSize);
            spatial.RandomNavigation(10);
            spatial.Plot();

            var config = new GridConfig();
            config.SetGridConfigDefaultParameters(new Tuple<int, int>(spatial.txx.Count, spatial.tyy.Count));

            var grid = new Grid(config);
            var simulation = new Simulation(grid, spatial.txx, spatial.tyy);
            simulation.Run();

           //setOfActCells = grid(x,y);

            grid.CalculateActiveCells(10);

            Console.WriteLine(grid.GridCells);
        }
    }
}
