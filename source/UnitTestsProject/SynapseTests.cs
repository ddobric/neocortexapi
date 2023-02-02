using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to synapse.
    /// </summary>
    [TestClass]
    public class SynapseTests
    {
        /// <summary>
        /// Test equal method.
        /// </summary>
        [TestMethod]
        public void CompareSynapse()
        {

            Cell cell1 = new Cell(parentColumnIndx: 1, colSeq: 1, numCellsPerColumn: 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);
            Cell cell2 = new Cell(parentColumnIndx: 8, colSeq: 36, numCellsPerColumn: 46, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);
            Cell cell3 = new Cell(parentColumnIndx: 9, colSeq: 10, numCellsPerColumn: 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);

            DistalDendrite distalDendrite = new DistalDendrite(parentCell: cell1, flatIdx: 1, lastUsedIteration: 2, ordinal: 10, synapsePermConnected: 0.5, numInputs: 10);
            cell1.DistalDendrites.Add(distalDendrite);

            Synapse synapse1 = new Synapse(presynapticCell: cell1, distalSegmentIndex: distalDendrite.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            cell2.ReceptorSynapses.Add(synapse1);

            Synapse synapse2 = new Synapse(presynapticCell: cell1, distalSegmentIndex: distalDendrite.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            cell2.ReceptorSynapses.Add(synapse2);

            Synapse synapse3 = new Synapse(presynapticCell: cell3, distalSegmentIndex: distalDendrite.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            cell2.ReceptorSynapses.Add(synapse3);

            //Not same by reference
            Assert.IsFalse(synapse1 == synapse2);
            Assert.IsFalse(synapse1 == synapse3);

            //column1 and column2 are same by value
            Assert.IsTrue(synapse1.Equals(synapse2));
            Assert.IsTrue(synapse1.SourceCell.Equals(synapse2.SourceCell));

            //column1 and column3 are NOT same by value (different column index)
            Assert.IsFalse(synapse1.Equals(synapse3));

        }
    }
}
