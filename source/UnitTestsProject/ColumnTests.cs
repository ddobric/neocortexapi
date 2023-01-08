using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to Column.
    /// </summary>
    [TestClass]
    public class ColumnTests
    {
        /// <summary>
        /// Column Equal method tests.
        /// TODO: Compare ProximalDendrite and ConnectedInputCounterMatrix
        /// </summary>
        [TestMethod]
        public void CompareColumn()
        {
            HtmConfig config = new HtmConfig(new int[] { 5 }, new int[] { 5 });

            Column column1 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            Column column2 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            Column column3 = new Column(config.CellsPerColumn, 2, config.SynPermConnected, config.NumInputs);

            //Not same by reference
            Assert.IsFalse(column1 == column2);
            Assert.IsFalse(column1 == column3);

            //column1 and column2 are same by value
            Assert.IsTrue(column1.Equals(column2));

            //column1 and column3 are NOT same by value (different column index)
            Assert.IsFalse(column1.Equals(column3));

        }
    }
}
