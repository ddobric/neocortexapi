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
        [TestCategory("NAA")]
        public void CreateAreaTest()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(100, 1024); 

            Cell cell1 = new Cell(-1, 0);

            MinicolumnArea caX = new MinicolumnArea("X", cfg);

            Assert.IsTrue(1024 == caX.Columns.Count);

            Assert.IsTrue(caX.AllCells.Count == 1024 * cfg.CellsPerColumn);

        }

    }
}
