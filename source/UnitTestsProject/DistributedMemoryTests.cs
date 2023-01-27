using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to distributed memory.
    /// </summary>
    [TestClass]
    public class DistributedMemoryTests
    {
        /// <summary>
        /// DistributedMemory Equal method tests.
        /// </summary>
        [TestMethod]
        public void CompareDistributedMemory()
        {
            Column column = new Column(numCells: 1, colIndx: 1, synapsePermConnected: 1.0, numInputs: 1);
            Column column2 = new Column(numCells: 15, colIndx: 2, synapsePermConnected: 1.0, numInputs: 3);
            Column column3 = new Column(numCells: 12, colIndx: 3, synapsePermConnected: 1.0, numInputs: 5);

            Column column4 = new Column(numCells: 1, colIndx: 1, synapsePermConnected: 1.0, numInputs: 1);
            Column column5 = new Column(numCells: 15, colIndx: 2, synapsePermConnected: 1.0, numInputs: 3);
            Column column6 = new Column(numCells: 12, colIndx: 3, synapsePermConnected: 1.0, numInputs: 5);

            Column column7 = new Column(numCells: 2, colIndx: 2, synapsePermConnected: 1.0, numInputs: 1);
            Column column8 = new Column(numCells: 11, colIndx: 4, synapsePermConnected: 2.0, numInputs: 1);
            Column column9 = new Column(numCells: 12, colIndx: 6, synapsePermConnected: 2.0, numInputs: 1);


            #region Compare two DistributedMemory with ColumnDictionary that have same columns

            DistributedMemory distributedMemory1 = new DistributedMemory();
            DistributedMemory distributedMemory2 = new DistributedMemory();

            distributedMemory1.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(3);
            distributedMemory1.ColumnDictionary.Add(1, column);
            distributedMemory1.ColumnDictionary.Add(2, column2);
            distributedMemory1.ColumnDictionary.Add(3, column3);

            distributedMemory2.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(3);
            distributedMemory2.ColumnDictionary.Add(1, column);
            distributedMemory2.ColumnDictionary.Add(2, column2);
            distributedMemory2.ColumnDictionary.Add(3, column3);

            //Not same by reference
            Assert.IsFalse(distributedMemory1 == distributedMemory2);

            //distributedMemory1 and distributedMemory2 are same by value
            Assert.IsTrue(distributedMemory1.Equals(distributedMemory2));

            //Check number of columns inside ColumnDictionary
            Assert.IsTrue(distributedMemory1.ColumnDictionary.Count.Equals(3));
            Assert.IsTrue(distributedMemory2.ColumnDictionary.Count.Equals(3));

            //Check if 2 ColumnDictionary equal
            Assert.IsTrue(distributedMemory1.ColumnDictionary.Equals(distributedMemory2.ColumnDictionary));
            Assert.IsTrue(distributedMemory1.ColumnDictionary.ElementsEqual(distributedMemory2.ColumnDictionary));
            #endregion


            #region Compare two DistributedMemory with two set similar columns

            //This test is same with the first test, but this time we use 2 set of columns that are similar  
            DistributedMemory distributedMemory3 = new DistributedMemory();
            DistributedMemory distributedMemory4 = new DistributedMemory();

            distributedMemory3.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(1);
            distributedMemory3.ColumnDictionary.Add(1, column);
            distributedMemory3.ColumnDictionary.Add(2, column2);
            distributedMemory3.ColumnDictionary.Add(3, column3);

            distributedMemory4.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(1);
            distributedMemory4.ColumnDictionary.Add(1, column4);
            distributedMemory4.ColumnDictionary.Add(2, column5);
            distributedMemory4.ColumnDictionary.Add(3, column6);

            //Not same by reference
            Assert.IsFalse(distributedMemory3 == distributedMemory4);

            //distributedMemory3 and distributedMemory4 are same by value
            Assert.IsTrue(distributedMemory3.Equals(distributedMemory4));

            //Check number of columns inside ColumnDictionary
            Assert.IsTrue(distributedMemory3.ColumnDictionary.Count.Equals(3));
            Assert.IsTrue(distributedMemory4.ColumnDictionary.Count.Equals(3));

            //Check if 2 ColumnDictionary equal
            Assert.IsTrue(distributedMemory3.ColumnDictionary.Equals(distributedMemory4.ColumnDictionary));
            Assert.IsTrue(distributedMemory3.ColumnDictionary.ElementsEqual(distributedMemory4.ColumnDictionary));
            #endregion

            #region Compare two DistributedMemory with two different ColumnDictionary
            //This test is same with the first test, but this time we use 2 set of columns that are similar  
            DistributedMemory distributedMemory5 = new DistributedMemory();
            DistributedMemory distributedMemory6 = new DistributedMemory();

            distributedMemory5.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(3);
            distributedMemory5.ColumnDictionary.Add(1, column);
            distributedMemory5.ColumnDictionary.Add(2, column2);
            distributedMemory5.ColumnDictionary.Add(3, column3);

            distributedMemory6.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(2);
            distributedMemory6.ColumnDictionary.Add(1, column7);
            distributedMemory6.ColumnDictionary.Add(2, column8);


            //distributedMemory5 and distributedMemory6 are NOT same by value
            Assert.IsFalse(distributedMemory5.Equals(distributedMemory6));

            //Check number of columns inside ColumnDictionary
            Assert.IsTrue(distributedMemory5.ColumnDictionary.Count.Equals(3));
            Assert.IsTrue(distributedMemory6.ColumnDictionary.Count.Equals(2));

            //Check if 2 ColumnDictionary equal
            Assert.IsFalse(distributedMemory5.ColumnDictionary.Equals(distributedMemory6.ColumnDictionary));
            Assert.IsFalse(distributedMemory5.ColumnDictionary.ElementsEqual(distributedMemory6.ColumnDictionary));

            #endregion


        }
    }
}
