using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public  class CorticalAreaTests
    {
        [TestMethod]
        public void CreateCorticalAreaTest()
        {
            HtmConfig cfg = new HtmConfig() 
            {
                CellsPerColumn = 10, 
                InputDimensions = new int[] { 100 }, 
                ColumnDimensions = new int[] { 32, 32 }
            };

           // CorticalArea area = new CorticalArea(cfg);

            //Assert.IsTrue(area.Columns.Count == 32 * 32);

            //foreach (var item in area.Columns)
            //{
            //    Assert.IsNotNull(item.Cells);
            //    Assert.IsTrue(item.Cells.Length == cfg.CellsPerColumn);
            //}
        }
    }
}
