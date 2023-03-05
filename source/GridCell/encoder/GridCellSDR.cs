using System;
using System.Collections.Generic;
using System.Linq;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace GridCell.encoder
{
    public class GridCellSDR
    {
        private int dimensions;

        private int[] sparse;
        private int[] dense;

        public GridCellSDR(int dimensions)
        {
            this.dimensions = dimensions;
            dense = new int[dimensions];
        }

        public void SetSparse(int[] sparse)
        {
            this.sparse = sparse;

            Array.Clear(dense, 0, dimensions);
            foreach (var cellIndex in sparse)
            {
                dense[cellIndex] = 1;    
            }
        }

        public int[] GetSparse()
        {
            return sparse;
        }

        public void SetDense(int[] dense)
        {
            this.dense = dense;

            var activeCellsIndexes =
                dense
                    .Select((isActive, index) => new KeyValuePair<int, int>(isActive, index))
                    .Where((x) => x.Key != 0)
                    .Select((kv) => kv.Value)
                    .ToList();

            sparse = activeCellsIndexes.ToArray();
        }

        public int[] GetDense()
        {
            return dense;
        }

        /// <summary>
        /// CalculateActiveCells
        /// </summary>
        public Cell[] CalculateActiveCells()
        {
            if (sparse == null)
            {
                throw new ArgumentNullException();
            }

            var activeCells = new Cell[sparse.Length];
            var numCellsPerColumn = (int) Math.Sqrt(dimensions);

            for (int i = 0; i < sparse.Length; i++)
            {
                var parentColIndex = sparse[i] / numCellsPerColumn;
                var colSeq = sparse[i] % numCellsPerColumn;
                activeCells[i] = new Cell(parentColIndex, colSeq, numCellsPerColumn, CellActivity.ActiveCell);
            }

            return activeCells;
        }

        public void SaveSDRImage(string fileName)
        {
            var numCellsPerColumn = (int)Math.Sqrt(dimensions);

            var data2D = new double[numCellsPerColumn, numCellsPerColumn];

            for (int i = 0; i < sparse.Length; i++)
            {
                var parentColIndex = sparse[i] / numCellsPerColumn;
                var colSeq = sparse[i] % numCellsPerColumn;
                data2D[parentColIndex, colSeq] = 1.0;
            }

            var plt = new ScottPlot.Plot(600, 600);
            plt.AddHeatmap(data2D, ScottPlot.Drawing.Colormap.Blues);

            plt.SaveFig(fileName + ".png");
        }
    }
}

