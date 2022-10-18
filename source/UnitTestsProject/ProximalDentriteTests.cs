using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using NeoCortexEntities.NeuroVisualizer;


namespace UnitTestsProject
{
    /// <summary>
    /// Tests for ProximalDentrite equal method.
    /// </summary>
    [TestClass]
    public class ProximalDentriteTests
    {
        [TestMethod]
        public void CompareProximalDentrites()
        {
            Pool rfPool1 = new Pool(10, 100);
            Pool rfPool2 = new Pool(2, 1000);

            Cell cell = new Cell(1, 20, 16, new CellActivity());
            Cell preSynapticCell = new Cell(2, 22, 26, new CellActivity());

            DistalDendrite dd = new DistalDendrite(cell, 10, 20, 10, 15, 100);
            cell.DistalDendrites.Add(dd);

            Synapse synapse = new Synapse(cell, dd.SegmentIndex, 23, 1.0);
            preSynapticCell.ReceptorSynapses.Add(synapse);

            //first RFPool
            rfPool1.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            rfPool1.m_SynapsesBySourceIndex.Add(1, synapse);

            //second RFPool
            rfPool2.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            rfPool2.m_SynapsesBySourceIndex.Add(1, synapse);

            //first parameter set
            int colIndx1 = 10;
            double synapsePermConnected1 = 20.5;
            int numInputs1 = 30;

            //second parameter set
            int colIndx2 = 2;
            double synapsePermConnected2 = 17.5;
            int numInputs2 = 300;

            ProximalDendrite proDend1 = new ProximalDendrite(colIndx1, synapsePermConnected1, numInputs1);
            proDend1.RFPool = rfPool1;

            ProximalDendrite proDend2 = new ProximalDendrite(colIndx1, synapsePermConnected1, numInputs1);
            proDend2.RFPool = rfPool1;

            ProximalDendrite proDend3 = new ProximalDendrite(colIndx2, synapsePermConnected2, numInputs2);
            proDend3.RFPool = rfPool1;

            ProximalDendrite proDend4 = new ProximalDendrite(colIndx1, synapsePermConnected2, numInputs2);
            proDend4.RFPool = rfPool1;

            ProximalDendrite proDend5 = new ProximalDendrite(colIndx1, synapsePermConnected1, numInputs2);
            proDend5.RFPool = rfPool1;

            ProximalDendrite proDend6 = new ProximalDendrite(colIndx1, synapsePermConnected1, numInputs1);
            proDend6.RFPool = rfPool2;

            //Not same by reference
            Assert.IsFalse(proDend1 == proDend2);

            //proDend1 and proDend2 are same by value
            Assert.IsTrue(proDend1.Equals(proDend2));

            //proDend1 and proDend3 are NOT same by value (different parameter sets)
            Assert.IsFalse(proDend1.Equals(proDend3));

            //proDend1 and proDend4 are NOT same by value (different parameters)
            Assert.IsFalse(proDend1.Equals(proDend4));

            //proDend1 and proDend5 are NOT same by value (different parameters)
            Assert.IsFalse(proDend1.Equals(proDend5));

            //proDend1 and proDend6 are NOT same by value (different RFPool)
            Assert.IsFalse(proDend1.Equals(proDend6));
        }

    }
}
