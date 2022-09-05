using System;
using NumSharp;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace GridCell
{
    public class Spikes
    {
        private NDArray logGridCells;

        private List<List<Cell>> gridcells = new();
        public List<List<Cell>> GridCells {
            get
            {
                return gridcells;
            }
        }

        public Spikes(NDArray logGridCells)
        {
            this.logGridCells = logGridCells;
        }


        public void run(int level)
        {
            gridcells.Clear();

            for (int cellNum = 0; cellNum < level; cellNum++) {
                var celula = logGridCells[Slice.All, cellNum];
                var max = (celula.max() * .9).GetDouble();

                var i = 0;
                var activeGridCells = new List<Cell>();
                foreach (float val in celula)
                {
                    if (val > max)
                    {
                        activeGridCells.Add(new Cell(0, i, 0, CellActivity.ActiveCell));
                    }
                }

                gridcells.Add(activeGridCells);
            }
        }
    }
}

