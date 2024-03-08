using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NeoCortexApi;
using NumSharp;

namespace GridCell
{
    public class Simulation
    {
        private NDArray logGridCells;

        public NDArray GridCellsLog
        {
            get
            {
                return logGridCells;
            }
        }

        private List<double> txx;
        private List<double> tyy;
        private Grid grid;

        public Simulation(Grid grid, List<double> txx, List<double> tyy)
        {
            this.txx = txx;
            this.tyy = tyy;
            this.grid = grid;

            logGridCells = np.ndarray((txx.Count - 1, grid.mm * grid.nn * grid.gridLayers));
        }

        public void Run()
        {
            for (int i = 1; i < txx.Count; i++)
            {
                var deltaMove = new Tuple<double, double>((txx[i] - txx[i - 1]), (tyy[i] - tyy[i - 1]));

                var currentActiveCells = grid.Compute(deltaMove);

                var indexes = currentActiveCells.Select(cell => cell.Index).ToArray();
                
                Console.WriteLine(Helpers.StringifyVector(indexes));
            }
        }

    }
}

