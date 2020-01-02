using NeoCortexApi.Entities;
using System.Collections.Generic;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class NeuroModel
    {
        public List<Area> Areas { get; set; }

        public static List<Cell> Cells { get; set; }

        public static List<Synapse> Synapse { get; set; }

        public static Settings CellsPerColumn { get; set; }

        public NeuroModel(int[] areaLevels, long[,] colDims, int numCells)
        {

            Cells = new List<Cell>();
            Synapse = new List<Synapse>();
            CellsPerColumn = new Settings();
            CellsPerColumn.NumberOfCells = numCells;

            Areas = new List<Area>(areaLevels.Length);
            //Areas = new Area[areaLevels.Length];
            
            for (int levelIndex = 0; levelIndex < areaLevels.Length; levelIndex++)//levelIndex == AreaID
            {
                // Areas[levelIndex] = new Area(areaId, colDims, numLayers, 0, 0, 0);
                Areas.Insert(levelIndex, new Area(levelIndex, colDims));

            }

        }
    }
    public class Settings
    {
        public int NumberOfCells { get; set; }
    }

    public class Area
    {
        public MiniColumn[,] MiniColumnsBlock { get; set; }
        public int AreaId { get; set; }
        public int Level { get; set; }


        public Area(int areaId, long[,] colDims)
        {
            MiniColumnsBlock = new MiniColumn[colDims.GetLength(0), colDims.GetLength(1)];


            double overlap = 0; // Where do I get overlap values
            for (int colDim0 = 0; colDim0 < colDims.GetLength(0); colDim0++)
            {
               // MiniColumn[] row = new MiniColumn[colDims.GetLength(1)]; // a row contains MiniMini Columns
                List<MiniColumn> row = new List<MiniColumn>(colDims.GetLength(1));

                int colDim1;
                for (colDim1 = 0; colDim1 < colDims.GetLength(1); colDim1++)
                {
                    //row[colDim0] = new MiniColumn(areaId, overlap, colDim0, colDim1);
                    row.Insert(colDim1, new MiniColumn(areaId, overlap, colDim0, colDim1));

                    // row[colDim0] = new MiniColumn(areaId, overlap, new long[colDim0, colDim1]);
                    //Minicolumn(settings, areaId, [this.id, miniColDim0, miniColDim1], randomOverlap, (miniColDim0 + X), (miniColDim1 + Z)))
                }

                MiniColumnsBlock[colDim0, colDim1-1] = row[row.Count-1];
            }

        }
    }

    public class MiniColumn
    {
        public Cell[] Cells { get; set; }
        public double Overlap { get; set; }
        public int AreaId { get; set; }


        public MiniColumn(int areaId, double overlap, int colDim0, int colDim1)//areaId, overlap, colDim0, colDim1
        //    public MiniColumn(int areaId, double overlap, long[,] columnDims)//areaId, overlap, colDim0, colDim1
        {
           
            AreaId = areaId;

            //int parentColumnIndx = columnDims.GetLength(0); // Can I set dimO/X is equal to parentColumnIndex??
            int parentColumnIndx = colDim0; // Can I set dimO/X is equal to parentColumnIndex??
            //Dim1 remains in this case always 0/1 , because of 2-Dimensional cells

           // Cells = new Cell[Model.CellsPerCol];
            Cells = new Cell[NeuroModel.CellsPerColumn.NumberOfCells];
            



            for (int numberOfCells = 0; numberOfCells < Cells.Length; numberOfCells++)
            {

                Cell cell = new Cell(areaId, parentColumnIndx, numberOfCells);
                Cells[numberOfCells] = cell;

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
        // public int AreaId { get; set; }

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
