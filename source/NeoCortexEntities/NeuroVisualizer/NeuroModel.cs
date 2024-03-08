// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexEntities.NeuroVisualizer
{
    public class GenerateNeuroModel
    {

        public NeuroModel CreateNeuroModel(int[] areaLevels, long[,] colDims, int numCells)
        {
            NeuroModel model = new NeuroModel();
            model.Areas = new List<Area>(areaLevels.Length);
            model.CellsPerColumn = numCells;

            for (int levelIndex = 0; levelIndex < areaLevels.Length; levelIndex++)//levelIndex == AreaID
            {

                //Areas.Insert(levelIndex, new Area(levelIndex, colDims));
                model.Areas.Insert(levelIndex, CreateArea(model, levelIndex, colDims, numCells));

                for (int i = 0; i < 20; i++)//How to get the total number of synapses, for now 20
                {
                    SynapseData synap = new SynapseData
                    {
                        Permanence = new Random().NextDouble(),//for now generatin random permanences ranging 0 to 1
                        PreSynapticCellIndex = model.Cells[new Random().Next(0, (model.Cells.Count()))].Index,//selecting random cell from already generated cells 
                        PostSynapticCellIndex = model.Cells[new Random().Next(0, (model.Cells.Count()))].Index,//selecting random cell from already generated cells 
                        // SourceCell = cell id
                        // destination Cell = cell id
                    };
                    model.Synapse.Insert(i, synap);
                }

            }


            return model;
        }
        private Area CreateArea(NeuroModel model, int areaID, long[,] colDims, int cellsPerColumn)
        {
            Area area = new Area(areaID, colDims);


            double overlap = 0; // Where do I get overlap values
            for (int colDim0 = 0; colDim0 < colDims.GetLength(0); colDim0++)
            {
                for (int colDim1 = 0; colDim1 < colDims.GetLength(1); colDim1++)
                {

                    area.MiniColumns[colDim0, colDim1] = new MiniColumn(areaID, overlap, colDim0, colDim1, ColumnActivity.Inactive);
                    Cell[] cells = CreateCells(cellsPerColumn, areaID, colDim0, colDim1);
                    for (int i = 0; i < cells.Length; i++)
                    {
                        area.MiniColumns[colDim0, colDim1].Cells.Add(cells[i]);
                        model.Cells.Add(cells[i]);

                    }



                }

            }


            return area;

        }

        private Cell[] CreateCells(int cellsPerColumn, int areaID, int colDim0, int colDim1)
        {
            Cell[] cells = new Cell[cellsPerColumn];
            int parentColumnIndx = 0;
            int cellId = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                cellId += 1;
                Cell cell = new Cell(areaID, i, parentColumnIndx, CellActivity.PredictiveCell);

                cells[i] = cell;
            }

            return cells;
        }


    }

    public class NeuroModel
    {
        public List<Area> Areas { get; set; }

        public List<Cell> Cells { get; set; }

        public List<SynapseData> Synapse { get; set; }

        public int CellsPerColumn { get; set; }


        public NeuroModel()
        {

            Cells = new List<Cell>();
            Synapse = new List<SynapseData>();

        }




    }

    public class Area
    {
        public MiniColumn[,] MiniColumns { get; set; }//Till now two dimensinal matrix to save the mini colums
        public int AreaId { get; set; }
        public int Level { get; set; }

        public Area(int areaID, long[,] colDims)
        {
            AreaId = areaID;
            MiniColumns = new MiniColumn[colDims.GetLength(0), colDims.GetLength(1)];

        }

    }

    public class MiniColumn
    {
        public List<Cell> Cells { get; set; }
        public double Overlap { get; set; }
        public int AreaId { get; set; }
        public int MiniColDim0 { get; set; }
        public int MiniColDim1 { get; set; }




        public MiniColumn(int areaId, double overlap, int miniColDim0, int miniColDim1, ColumnActivity colActivity)//areaId, overlap, colDim0, colDim1
        {

            AreaId = areaId;
            Overlap = overlap;
            MiniColDim0 = miniColDim0;
            MiniColDim1 = miniColDim1;
            Cells = new List<Cell>();
            //int parentColumnIndx = columnDims.GetLength(0); // Can I set dimO/X is equal to parentColumnIndex??
            // int parentColumnIndx = colDim0; // Can I set dimO/X is equal to parentColumnIndex??
            //Dim1 remains in this case always 0/1 , because of 2-Dimensional cells

            //Cells = new Cell[NeuroModel.CellsPerColumn];



        }


    }
    public class SynapseData
    {
        //public CellType SegmentCellType;
        public int PreSynapticCellIndex { get; set; }
        public int PostSynapticCellIndex { get; set; }
        public double Permanence { get; set; }

    }

    public enum ColumnActivity
    {
        Active,

        Inactive
    }


    public enum CellActivity
    {
        ActiveCell,

        PredictiveCell,

        WinnerCell,
    }


}
