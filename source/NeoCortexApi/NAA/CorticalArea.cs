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
        private ConcurrentDictionary<long, Cell> CurrActiveCells { get; set; } = new ConcurrentDictionary<long, Cell>();

        /// <summary>
        /// Sparse map of cells that have been involved in learning. Their indexes in the virtual sparse array.
        /// </summary>
        private ConcurrentDictionary<long, Cell> AllCellsSparse { get; set; } = new ConcurrentDictionary<long, Cell>();

        /// <summary>
        /// The index of the area.
        /// </summary>
        //public int Index { get; set; }

        /// <summary>
        /// The name of the _area. It must be unique in the application.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get the list of active cells from indicies.
        /// </summary>
        public ICollection<Cell> ActiveCells
        {
            get
            {
                var actCells = CurrActiveCells.Values;

                return actCells;
            }
        }

        public long[] ActiveCellsIndicies
        {
            get
            {
                return CurrActiveCells.Keys.ToArray();
            }
            set
            {
                CurrActiveCells = new ConcurrentDictionary<long, Cell>();

                int indx = 0;
              
                foreach (var item in value)
                {
                    CurrActiveCells.TryAdd(item, new Cell(-1, indx++));
                }
            }
        }


        /// <summary>
        /// Gets the number of outgoing synapses of all active cells.
        /// </summary>
        public int NumOutgoingSynapses
        {
            get
            {
                return this.ActiveCells.SelectMany(el => el.ReceptorSynapses).Count();
            }
        }

        /// <summary>
        /// Gets the number of incoming synapses at apical segments.
        /// </summary>
        public int NumIncomingApicalSynapses
        {
            get
            {
                return this.ActiveCells.SelectMany(cell => cell.ApicalDendrites).SelectMany(aSeg => aSeg.Synapses).Count();                
            }
        }

        public CorticalArea(int index, string name, int numCells)
        {
            this.Name = name;

            this._numCells = numCells;

            CurrActiveCells = new ConcurrentDictionary<long, Cell>();
        }

        public override string ToString()
        {
            return $"{Name} - Cells: {_numCells} - Active Cells : {CurrActiveCells.Count}";
        }
              
    }
}
