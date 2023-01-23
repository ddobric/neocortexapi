using NeoCortexApi.DataMappers;
using NeoCortexApi.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Cortical column that consists of cells. It does not contain mini-columns.
    /// </summary>
    public class CorticalArea
    {
        protected ConcurrentDictionary<int, Segment> _segmentMap = new ConcurrentDictionary<int, Segment>();

        /// <summary>
        /// Number of cells in the _area.
        /// </summary>
        private int _numCells;

        /// <summary>
        /// Map of active cells and their indexes in the virtual sparse array.
        /// </summary>
        private ConcurrentDictionary<long, Cell> Cells { get; set; } = new ConcurrentDictionary<long, Cell>();

        /// <summary>
        /// The name of the _area.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get the list of active cells from indicies.
        /// </summary>
        public ICollection<Cell> ActiveCells
        {
            get
            {
                var actCells = Cells.Values;

                return actCells;
            }
        }

        public long[] ActiveCellsIndicies
        {
            get
            {
                return Cells.Keys.ToArray();
            }
            set
            {
                Cells = new ConcurrentDictionary<long, Cell>();

                int indx = 0;

                foreach (var item in value)
                {
                    Cells.TryAdd(item, new Cell(0, indx++, _numCells, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell));
                }
            }
        } 



        public CorticalArea(string name, int numCells)
        {
            this.Name = name;

            this._numCells = numCells;

            Cells = new ConcurrentDictionary<long, Cell>();
        }

        public override string ToString()
        {
            return $"{Name} - Cells: {_numCells} - Active Cells : {Cells.Count}";
        }
              
    }
}
