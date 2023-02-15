using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    public class SparseObjectMatrixTests
    {
        [TestMethod]
        public void CompareSparseObjectMatrixs()
        {            

            int[] dimensions = { 20, 10 };
            int[] dimensions2 = { 20, 20 };
            bool useColumnMajorOrdering = false;
            bool useColumnMajorOrdering2 = true;

            SparseObjectMatrix<Column> matrix1 = new SparseObjectMatrix<Column>(dimensions, useColumnMajorOrdering, dict: null);
            SparseObjectMatrix<Column> matrix2 = new SparseObjectMatrix<Column>(dimensions, useColumnMajorOrdering, dict: null);
            SparseObjectMatrix<Column> matrix3 = new SparseObjectMatrix<Column>(dimensions, useColumnMajorOrdering2, dict: null);
            SparseObjectMatrix<Column> matrix4 = new SparseObjectMatrix<Column>(dimensions2, useColumnMajorOrdering, dict: null);

            #region Compare empty matrix with different input configs
            //Not same by reference
            Assert.IsFalse(matrix1 == matrix2);
            Assert.IsFalse(matrix1 == matrix3);
            Assert.IsFalse(matrix1 == matrix4);

            //matrix1 and matrix2 are same by value
            Assert.IsTrue(matrix1.Equals(matrix2));

            //matrix1 and matrix3/matrix4 are NOT same by value
            Assert.IsFalse(matrix1.Equals(matrix3));
            Assert.IsFalse(matrix1.Equals(matrix4));
            #endregion


            HtmConfig config = new HtmConfig(new int[] { 10, 10 }, new int[] { 10, 10 });
            Connections connections = new Connections(config);

            SparseObjectMatrix<Column> matrix5 = new SparseObjectMatrix<Column>(dimensions, useColumnMajorOrdering, dict: null);

            //Initialize 2 different full set of columns.
            //First set
            int numColumns = 20;
            int cellsPerColumn = 32;
            Column[] columnSet1 = new Column[numColumns];

            for (int i = 0; i < numColumns; i++)
            {
                columnSet1[i] = new Column(cellsPerColumn, i, connections.HtmConfig.SynPermConnected, connections.HtmConfig.NumInputs);

                //Setup cells for each column
                for (int j = 0; j < cellsPerColumn; j++)
                {
                    columnSet1[i].Cells[j] = new Cell(parentColumnIndx: i, colSeq: j, numCellsPerColumn: cellsPerColumn, new CellActivity());
                }

                //Set column to each matrix index
                matrix1.set(i, columnSet1[i]);
                matrix2.set(i, columnSet1[i]);
            }

            //Second set
            int numColumns2 = 10;
            int cellsPerColumn2 = 32;
            Column[] columnSet2 = new Column[numColumns2];

            for (int i = 0; i < numColumns2; i++)
            {
                columnSet2[i] = new Column(cellsPerColumn2, i, connections.HtmConfig.SynPermConnected, connections.HtmConfig.NumInputs);

                //Setup cells for each column
                for (int j = 0; j < cellsPerColumn2; j++)
                {
                    columnSet2[i].Cells[j] = new Cell(parentColumnIndx: i, colSeq: j, numCellsPerColumn: cellsPerColumn2, new CellActivity());
                }

                //Set column to each matrix index
                matrix5.set(i, columnSet2[i]);
            }

            //matrix1 and matrix2 are same by value
            Assert.IsTrue(matrix1.Equals(matrix2));

            //Check values

            for (int i = 0; i < numColumns; i++)
            {
                Assert.AreEqual(matrix1.GetColumn(i), columnSet1[i]);
                Assert.AreEqual(matrix1.GetColumn(i), matrix2.GetColumn(i));
            }

            for (int i = 0; i < numColumns2; i++)
            {
                Assert.AreEqual(matrix5.GetColumn(i), columnSet2[i]);
            }

            //matrix1 and matrix5 are NOT same by value
            Assert.IsFalse(matrix1.Equals(matrix5));

        }
    }
}
