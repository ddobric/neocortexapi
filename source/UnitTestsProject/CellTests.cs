using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Net.Http.Headers;
using Naa = NeoCortexApi.NeuralAssociationAlgorithm;
using System.Diagnostics;


namespace UnitTestsProject
{
    /// <summary>
    /// UnitTests for the Cell.
    /// </summary>
    [TestClass]
    public class CellTests
    {
        [TestMethod]
        [TestCategory("Prod")]
        public void CellCompareTest()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(100, 1024); 

            List<Cell> cellsArea1= new List<Cell>();

            List<Cell> cellsArea2 = new List<Cell>();

            for (int i = 0; i < 100; i++)
            {
                cellsArea1.Add(new Cell(1, i));
                cellsArea2.Add(new Cell(2, i));
            }

            for (int i = 0; i < cellsArea1.Count; i++)
            {
                // Same cells must be equal.
                Assert.IsTrue(cellsArea1[i] == cellsArea1[i]);

                for (int j = 0; j < cellsArea2.Count; j++)
                {
                    Assert.IsTrue(cellsArea2[j] == cellsArea2[j]);

                    Assert.IsTrue(cellsArea1[i] != cellsArea2[j]);
                }
            }
        }

    }
}
