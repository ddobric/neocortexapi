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

        //public List<Cell> ActiveCells
        //{
        //    get
        //    {
        //        List<Cell> activeCells = new List<Cell>();

        //        foreach (var item in Cells)
        //        {
        //            if(item.)
        //        }
        //    }
        //}

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

        public TSeg GetSegmentFromIndex<TSeg>(int segIndx) where TSeg : Segment
        {
            return (TSeg)_segmentMap[segIndx];
        }
    }
}
