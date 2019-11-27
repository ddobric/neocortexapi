using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class NeuroModel
    {
        public string msgType { get; set; }
    }

    public class ColumnData
    {
        public long[,] ColDims { get; set; }
        public Cell[] cell;
        public double Overlap { get; set; } 
        public string msgType { get; set; }
    }
    public class SynapseData
    {
       // public int AreaId { get; set; }

        public CellType SegmentCellType;

        public Synapse Synapse { get; set; }
        public Cell preCell { get; set; }
        public Cell postCell { get; set; }
        public string msgType { get; set; }
    }
    public class Area
    {
        public ColumnData[,] miniColumn { get; set; }
        public int areaId { get; set; }
        public int level { get; set; }

    }

    public enum CellType
    {
        ActiveCell,

        PredictiveCell,

        WinnerCell,
    }
}
