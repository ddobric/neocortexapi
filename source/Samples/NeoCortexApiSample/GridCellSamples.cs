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
            int walk = 20;
            int arenaSize = 50;

            var spatial = new SpatialNavigation(arenaSize);
            spatial.RandomNavigation(walk);

            var gridConfig = new GridConfig(new Tuple<int, int>(spatial.txx.Count, spatial.tyy.Count));
            var grid = new Grid(gridConfig);
            var simulation = new Simulation(grid, spatial.txx, spatial.tyy);
            simulation.Run();
        }
    }
}
