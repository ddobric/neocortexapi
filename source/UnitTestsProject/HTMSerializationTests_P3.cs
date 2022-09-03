using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;


namespace UnitTestsProject
{
    public partial class HTMSerializationTests
    {

        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_Column()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Column column = new Column(2,12,12.2,2);
            
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_Column)}_column.txt"))
            {
                HtmSerializer2.Serialize(column, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_Column)}_column.txt"))
            {
                var columnD = HtmSerializer2.Deserialize<Column>(sr);
                Assert.IsTrue(column.Equals(columnD));
            }
        }


        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_SparseBinaryMatrix()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            int[] dimension = { 100, 100 };

            SparseBinaryMatrix matrix = new SparseBinaryMatrix(dimension,false);            

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_SparseBinaryMatrix)}_sbmatrix.txt"))
            {
                HtmSerializer2.Serialize(matrix, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_SparseBinaryMatrix)}_sbmatrix.txt"))
            {
                var matrixD = HtmSerializer2.Deserialize<SparseBinaryMatrix>(sr);
                Assert.IsTrue(matrix.Equals(matrixD));
            }
        }

  
        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_Connections()
        {
            int[] inputDims = { 100,100 };
            int[] columnDims = { 10,10 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            connections.ActiveSegments.Add(distDend);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_Connections)}_connections.txt"))
            {
                HtmSerializer2.Serialize(connections, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_Connections)}_connections.txt"))
            {
                Connections connectionsD = HtmSerializer2.Deserialize<Connections>(sr);
                Assert.IsTrue(connections.Equals(connectionsD));
            }
        }

        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_HtmConfig()
        {
            int[] inputDims = { 10, 12,14 };
            int[] columnDims = { 1, 2, 3 };

            HtmConfig config = new HtmConfig(inputDims, columnDims);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_HtmConfig)}_config.txt"))
            {
                HtmSerializer2.Serialize(config, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_HtmConfig)}_config.txt"))
            {
                HtmConfig configD = HtmSerializer2.Deserialize<HtmConfig>(sr);
                Assert.IsTrue(config.Equals(configD));
            }
        }      

        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_DistalDendrite()
        {
           
            Cell cell = new Cell(12, 14, 16, new CellActivity());

            DistalDendrite distalDendrite = new DistalDendrite(cell, 1, 2, 2, 2.0, 100);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_DistalDendrite)}_dd.txt"))
            {
                HtmSerializer2.Serialize(distalDendrite, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_DistalDendrite)}_dd.txt"))
            {
                DistalDendrite distalDendriteD = HtmSerializer2.Deserialize<DistalDendrite>(sr);
                Assert.IsTrue(distalDendrite.Equals(distalDendriteD));
            }
        }


        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_DistributedMemory()
        {

            Column column = new Column(2, 12, 12.2, 2);

            DistributedMemory distributedMemory = new DistributedMemory();

            distributedMemory.ColumnDictionary.Add(1, column);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_DistributedMemory)}_dm.txt"))
            {
                HtmSerializer2.Serialize(distributedMemory, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_DistributedMemory)}_dm.txt"))
            {
                DistributedMemory distributedMemoryD = HtmSerializer2.Deserialize<DistributedMemory>(sr);
                Assert.IsTrue(distributedMemory.Equals(distributedMemoryD));
            }
        }

        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_HtmModuleTopology()
        {
            int[] dimension = { 10, 12, 14 };

            HtmModuleTopology topology = new HtmModuleTopology(dimension, true);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_HtmModuleTopology)}_topology.txt"))
            {
                HtmSerializer2.Serialize(topology, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_HtmModuleTopology)}_topology.txt"))
            {
                HtmModuleTopology topologyD = HtmSerializer2.Deserialize<HtmModuleTopology>(sr);
                Assert.IsTrue(topology.Equals(topologyD));
            }
        }

        //Currently fail because the created proDent's Synapses is an empty list (after added Pool). The Deserialize object is correct.
        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_ProximalDentrite()
        {
            Pool rfPool = new Pool(size: 2, numInputs: 100);

            Cell cell = new Cell(parentColumnIndx: 1, colSeq: 20, numCellsPerColumn: 16, new CellActivity());
            Cell preSynapticCell = new Cell(parentColumnIndx: 2, colSeq: 22, numCellsPerColumn: 26, new CellActivity());

            DistalDendrite dd = new DistalDendrite(parentCell: cell, flatIdx: 10, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            cell.DistalDendrites.Add(dd);

            Synapse synapse = new Synapse(presynapticCell: cell, distalSegmentIndex: dd.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            preSynapticCell.ReceptorSynapses.Add(synapse);

            rfPool.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            rfPool.m_SynapsesBySourceIndex.Add(1, synapse);

            int colIndx = 10;
            double synapsePermConnected = 20.5;
            int numInputs = 30;

            ProximalDendrite proDend = new ProximalDendrite(colIndx, synapsePermConnected, numInputs);
            proDend.RFPool = rfPool;

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_ProximalDentrite)}_prodent.txt"))
            {
                HtmSerializer2.Serialize(proDend, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_ProximalDentrite)}_prodent.txt"))
            {
                ProximalDendrite proDendD = HtmSerializer2.Deserialize<ProximalDendrite>(sr);
                Assert.IsTrue(proDend.Equals(proDendD));
            }

        }


    }
}
