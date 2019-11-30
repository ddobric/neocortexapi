using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class NeuroModel
    {
         public string MsgType { get; set; }
         public Area[] Areas { get; set; }
         public Cell[] Cells { get; set; }

       public NeuroModel(int[] areaLevels, long [,] colDims, int numLayers)
        {
            Areas = new Area[areaLevels.Length];
            int areaId = 0;
            for (int levelIndex = 0; levelIndex < areaLevels.Length; levelIndex++)
            {
                areaId = levelIndex;
                Areas[levelIndex] = new Area(areaId, colDims, 0, 0, 0);


            }

        }
  
    }
    public class CreateModel
    {
        //public NeuroModel GetModel()
        //{
        //    NeuroModel neuroModel = new NeuroModel();
   
        //    return neuroModel;
        //}

    }

    public class MiniColumn
    {
        public long[,] ColDims { get; set; }//dont need


        public Cell[] Cells;
        public double Overlap { get; set; } 
        public string MsgType { get; set; }
        
    }
    public class SynapseData
    {
       // public int AreaId { get; set; }

        public CellType SegmentCellType;

        public Synapse Synapse { get; set; }
        public Cell PreCell { get; set; }
        public Cell PostCell { get; set; }
        public string MsgType { get; set; }
    }
    public class Area
    {
        public MiniColumn[,] MiniColumns { get; set; }
        public int AreaId { get; set; }
        public int Level { get; set; }

        // Area(areaId, areaLevels[levelIndex], 0, 0, 0);
      public Area(int areaId, long[,] colDims, int x, int y, int z )
        {
            for (int colDim0 = 0; colDim0 < colDims.GetLength(0); colDim0++)
            {
                // MiniColumn[] row 
                    
                for (int colDim1 = 0; colDim1 < colDims.GetLength(1); colDim1++)
                {


                }

            }

        }
    }

    public enum CellType
    {
        ActiveCell,

        PredictiveCell,

        WinnerCell,
    }

}
