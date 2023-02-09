using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to Column.
    /// </summary>
    [TestClass]
    public class PoolTests
    {
        /// <summary>
        /// Column Equal method tests.
        /// </summary>
        [TestMethod]
        public void ComparePools()
        {           
            Pool pool1 = new Pool(size: 1, numInputs: 200);   
            Pool pool2 = new Pool(size: 1, numInputs: 200);
            Pool pool3 = new Pool(size: 2, numInputs: 300);

            #region Compare 3 empty pools
            //Not same by reference
            Assert.IsFalse(pool1 == pool2);
            Assert.IsFalse(pool1 == pool3);

            //pool1 and pool2 are same by value
            Assert.IsTrue(pool1.Equals(pool2));

            //pool1 and pool3 are NOT same by value
            Assert.IsFalse(pool1.Equals(pool3));
            #endregion

            #region Compare 3 pools with same/different SynapsesBySourceIndex
            Cell cell1 = new Cell(parentColumnIndx: 1, colSeq: 20, numCellsPerColumn: 16, new CellActivity());
            Cell cell2 = new Cell(parentColumnIndx: 2, colSeq: 21, numCellsPerColumn: 16, new CellActivity());
            Cell preSynapticCell = new Cell(parentColumnIndx: 3, colSeq: 22, numCellsPerColumn: 26, new CellActivity());

            DistalDendrite dd1 = new DistalDendrite(parentCell: cell1, flatIdx: 10, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            DistalDendrite dd2 = new DistalDendrite(parentCell: cell2, flatIdx: 11, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);

            cell1.DistalDendrites.Add(dd1);
            cell2.DistalDendrites.Add(dd2);

            Synapse synapse1 = new Synapse(presynapticCell: cell1, distalSegmentIndex: dd1.SegmentIndex, synapseIndex: 23, permanence: 2.0);
            Synapse synapse2 = new Synapse(presynapticCell: cell2, distalSegmentIndex: dd2.SegmentIndex, synapseIndex: 22, permanence: 2.0);

            preSynapticCell.ReceptorSynapses.Add(synapse1);
            preSynapticCell.ReceptorSynapses.Add(synapse2);

            pool1.m_SynapsesBySourceIndex.Add(2, synapse1);
            pool2.m_SynapsesBySourceIndex.Add(2, synapse1);
            pool3.m_SynapsesBySourceIndex.Add(2, synapse2);

            //pool1 and pool2 are same by value
            Assert.IsTrue(pool1.Equals(pool2));

            //pool1 and pool3 are NOT same by value
            Assert.IsFalse(pool1.Equals(pool3));
            #endregion

            #region Update Pool and compare pool1 and pool2 again.
            Synapse synapse3 = new Synapse(presynapticCell: cell2, distalSegmentIndex: dd2.SegmentIndex, synapseIndex: 21, permanence: 1.0);

            pool1.UpdatePool(synPermConnected: 1.0, synapse3, permanence: 2.0);

            //pool1 and pool2 are NOT same by value after new synapse is added to pool1
            Assert.IsFalse(pool1.Equals(pool2));
            #endregion

            pool2.UpdatePool(synPermConnected: 1.0, synapse3, permanence: 2.0);

            //pool1 and pool2 are same by value after update pool2 
            Assert.IsTrue(pool1.Equals(pool2));

            //Compare values of pool1 and pool2
            Assert.IsTrue(pool1.GetSparsePotential().ElementsEqual(pool2.GetSparsePotential()));
            Assert.IsTrue(pool1.GetSparsePermanences().ElementsEqual(pool2.GetSparsePermanences()));
            Assert.IsTrue(pool1.GetDenseConnected().ElementsEqual(pool2.GetDenseConnected()));
            Assert.IsTrue(pool1.GetSynapseForInput(2).Equals(pool2.GetSynapseForInput(2)));

        }
    }
}
