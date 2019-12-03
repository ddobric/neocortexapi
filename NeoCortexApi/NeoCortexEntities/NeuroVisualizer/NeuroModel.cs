using NeoCortexApi.Entities;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class NeuroModel
    {
        public Area[] Areas { get; set; }
        public Cell[] Cells { get; set; }
        public Synapse[] synapse { get; set; }

        public NeuroModel(int[] areaLevels, long[,] colDims, int numLayers)
        {
            NeuroSettings sett = new NeuroSettings();
            sett.Layers = numLayers;
            Areas = new Area[areaLevels.Length];
            int areaId = 0;
            for (int levelIndex = 0; levelIndex < areaLevels.Length; levelIndex++)
            {
                areaId = levelIndex;
                Areas[levelIndex] = new Area(areaId, colDims, 0, 0, 0);


            }

        }

    }
    public class NeuroSettings
    {
        public int Layers { get; set; }

    }

    public class MiniColumn
    {
        //public long[,] ColDims { get; set; }
        public Cell[] Cells { get; set; }
        public double Overlap { get; set; }
        public int AreaId { get; set; }

        public MiniColumn(int areaId, double overlap, int colDim0, int colDim1)//areaId, overlap, colDim0, colDim1
        //    public MiniColumn(int areaId, double overlap, long[,] columnDims)//areaId, overlap, colDim0, colDim1
        {
            AreaId = areaId;
            // ColDims = columnDims;
            NeuroSettings sett = new NeuroSettings();
            //int parentColumnIndx = columnDims.GetLength(0); // Can I set dimO/X is equal to parentColumnIndex??
            int parentColumnIndx = colDim0; // Can I set dimO/X is equal to parentColumnIndex??
            //Dim1 remains in this case always 0 , because of 2-Dimensional cells

            Cells = new Cell[sett.Layers];

            for (int layer = 0; layer < sett.Layers; layer++)
            {

                Cell cell = new Cell(areaId, parentColumnIndx, layer);
                Cells[layer] = cell;

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

    }

    public class Area
    {
        public MiniColumn[,] MiniColumns { get; set; }
        public int AreaId { get; set; }
        public int Level { get; set; }

        public Area(int areaId, long[,] colDims, int x, int y, int z)
        {

            double overlap = 0; // Where do I get overlap values
            for (int colDim0 = 0; colDim0 < colDims.GetLength(0); colDim0++)
            {
                MiniColumn[] row = new MiniColumn[colDims.GetLength(0)];

                for (int colDim1 = 0; colDim1 < colDims.GetLength(1); colDim1++)
                {
                    row[colDim0] = new MiniColumn(areaId, overlap, colDim0, colDim1);
                    // row[colDim0] = new MiniColumn(areaId, overlap, new long[colDim0, colDim1]);
                    //Minicolumn(settings, areaId, [this.id, miniColDim0, miniColDim1], randomOverlap, (miniColDim0 + X), (miniColDim1 + Z)))
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
