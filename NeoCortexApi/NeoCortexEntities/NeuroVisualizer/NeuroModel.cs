using NeoCortexApi.Entities;
using System.Collections.Generic;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class NeuroModel
    {
        public List<Area> Areas { get; set; }

        public static List<Cell> Cells { get; set; }

        public static List<Synapse> Synapse { get; set; }

        public static int CellsPerColumn { get; set; }

        public NeuroModel(int[] areaLevels, long[,] colDims, int numCells)
        {

            Cells = new List<Cell>();
            Synapse = new List<Synapse>();

            CellsPerColumn = numCells;

            Areas = new List<Area>(areaLevels.Length);
            //Areas = new Area[areaLevels.Length];
            
            for (int levelIndex = 0; levelIndex < areaLevels.Length; levelIndex++)//levelIndex == AreaID
            {
                // Areas[levelIndex] = new Area(areaId, colDims, numLayers, 0, 0, 0);
                Areas.Insert(levelIndex, new Area(levelIndex, colDims));

            }

            SynapseData synap = new SynapseData()
            {
                
            };

        }
    }

    public class Area
    {
        public MiniColumn[,] MiniColumns { get; set; }//Till now two dimensinal matrix to save the mini colums
        public int AreaId { get; set; }
        public int Level { get; set; }


        public Area(int areaId, long[,] colDims)
        {
            MiniColumns = new MiniColumn[colDims.GetLength(0), colDims.GetLength(1)];
            int NumCol = 0;

            double overlap = 0; // Where do I get overlap values
            for (int colDim0 = 0; colDim0 < colDims.GetLength(0); colDim0++)
            {
                for (int colDim1 = 0; colDim1 < colDims.GetLength(1); colDim1++)
                {
               
                    MiniColumns[colDim0, colDim1] = new MiniColumn(areaId, overlap, colDim0, colDim1);
                    
                }

            }
            AreaId = areaId;

        }
    }

    public class MiniColumn
    {
        public Cell[] Cells { get; set; }
        public double Overlap { get; set; }
        public int AreaId { get; set; }


        public MiniColumn(int areaId, double overlap, int colDim0, int colDim1)//areaId, overlap, colDim0, colDim1
        {
           
            AreaId = areaId;

            //int parentColumnIndx = columnDims.GetLength(0); // Can I set dimO/X is equal to parentColumnIndex??
            int parentColumnIndx = colDim0; // Can I set dimO/X is equal to parentColumnIndex??
            //Dim1 remains in this case always 0/1 , because of 2-Dimensional cells

            Cells = new Cell[NeuroModel.CellsPerColumn];
            



            for (int i = 0; i < Cells.Length; i++)
            {

                Cell cell = new Cell(areaId, parentColumnIndx, i);

                Cells[i] = cell;

            }
            saveCells(Cells);

        }
     
        private void saveCells(Cell[] cell)
        {
            
            for (int i = 0; i < cell.Length; i++)
            {
                NeuroModel.Cells.Add(cell[i]);

            }

        }

    }
    public class SynapseData
    {

        public CellType SegmentCellType;
        public Synapse Synapse { get; set; }
        public Cell PreCell { get; set; }
        public Cell PostCell { get; set; }

        public void insertSynapses()
        {
                double permanence = 0;
           

            for (int i = 0; i < 100; i++)//How to get the total number of synapses
            {
                // How to create a synapse
                Synapse synapse = new Synapse
                {
                    Permanence = permanence,
                    // SourceCell = cell id
                    // destination Cell = cell id
                };

                NeuroModel.Synapse.Insert(i, synapse);
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
