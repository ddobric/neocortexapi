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
        /// </summary>
        [TestMethod]
        public void CompareColumn()
        {
            
            // Two set of configuration parameters with different values.
            HtmConfig config = new HtmConfig(inputDims: new int[] { 5 }, columnDims: new int[] { 5 })
            {
                NumInputs = 1
            };

            HtmConfig config2 = new HtmConfig(inputDims: new int[] { 5 }, columnDims: new int[] { 5 })
            {
                NumInputs = 2
            };


            #region Test with different column index     
            //Test with different column index 
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

            #endregion

            #region Test with different PotentialPool
            //Test with different PotentialPool
            Column column4 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            column4.CreatePotentialPool(config, new int[] { 1, 2, 3 }, -1);

            Column column5 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            column5.CreatePotentialPool(config, new int[] { 1, 2, 3 }, -1);

            Column column6 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            column6.CreatePotentialPool(config, new int[] { 0, 2, 2 }, -1);

            //column4 and column5 are same by value 
            Assert.IsTrue(column4.Equals(column5));

            //column4 and column6 are NOT same by value (different Potential Pool)
            Assert.IsFalse(column4.Equals(column6));
            #endregion

            #region Test with different connectedInputCounter 
            //Test with different connectedInputCounter 
            Column column7 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            Column column8 = new Column(config.CellsPerColumn, 1, config.SynPermConnected, config.NumInputs);
            Column column9 = new Column(config2.CellsPerColumn, 1, config2.SynPermConnected, config2.NumInputs);

            //Compare connectedInputCounter
            Assert.IsTrue(column7.connectedInputCounter.Equals(column8.connectedInputCounter));
            Assert.IsFalse(column7.connectedInputCounter.Equals(column9.connectedInputCounter));

            //column7 and column8 are same by value 
            Assert.IsTrue(column7.Equals(column8));

            //column7 and column9 are NOT same by value (different config)
            Assert.IsFalse(column7.Equals(column9)); 
            #endregion

        }
    }
}
