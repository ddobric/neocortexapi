using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class NeuroModel
    {
    }

    public class ColumnData
    {
        public long[] ColDims{ get; set; }

        public float Overlapp { get; set; } 
    }
    public class SynapseData
    {
        public int AreaId { get; set; }

        public CellType SegmentCellType;

        public Synapse Synapse { get; set; }    
    }


    public enum CellType
    {
        ActiveCell,

        PredictiveCell,

        WinnerCell,
    }
}
