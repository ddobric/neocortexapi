using System;
using System.Numerics;
using NumSharp;

namespace gridcells
{
    public class Simulation
    {
        private NDArray logGridCells;

        public NDArray GridCellsLog {
            get {
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

            logGridCells = np.ndarray((txx.Count - 1,  grid.mm * grid.nn * grid.gridLayers));
        }


        public void run()
        {
            for (int i = 1; i < txx.Count; i++) {
                var speedVector = new Complex((txx[i] - txx[i - 1]), (tyy[i] - tyy[i - 1]));
                grid.update(speedVector);
                logGridCells[i-1] = grid.GridActivity.flatten();
            }
        }
    }
}

