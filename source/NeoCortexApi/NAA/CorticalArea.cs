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

        public Cell[] Cells { get; set; }

        public string Name { get; private set; }


        /// <summary>
        /// Get the list of active cells from indicies.
        /// </summary>
        public List<Cell> ActiveCells
        {
            get
            {
                var actCells = Cells.Where(c=> ActiveCellsIndicies.Contains(c.Index));

                return actCells.ToList();
            }
        }

        ///// <summary>
        ///// Get the list of active cells from indicies.
        ///// </summary>
        //public ICollection<Cell> WinnerCells
        //{
        //    get
        //    {
        //        var actCells = Cells.Where(c => WinnerCellsIndicies.Contains(c.Index));

        //        return actCells.ToList();
        //    }
        //}

        public List<long> ActiveCellsIndicies { get; set; } = new List<long>();

        public List<long> WinnerCellsIndicies { get; set; } = new List<long>();


        public CorticalArea(string name, int numCells)
        {
            this.Name = name;

            Cells = new Cell[numCells];
        }


        public override string ToString()
        {
            return $"{Name} - Cells: {this.Cells.Length}";
        }

        public Segment[] AllDistalDendrites
        {
            get
            {
                return Cells.SelectMany(c => c.DistalDendrites).ToArray();
            }
        }

        //public TSeg GetSegmentFromIndex<TSeg>(int segIndx) where TSeg : Segment
        //{
        //    return (TSeg)_segmentMap[segIndx];
        //}

    }
}
