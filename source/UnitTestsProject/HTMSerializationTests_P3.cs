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
    // All test shouyld have a prefix 'Serializationtest_TESTNAME'
    [TestClass]
    public class HTMSerializationTests_P3
    {
        /// <summary>
        /// TODO: ALL TEST MUST BE WELL COMMENTED
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_COLUMN()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Column column = new Column(2,12,12.2,2);
            
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_COLUMN)}_column.txt"))
            {
                HtmSerializer2.Serialize(column, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_COLUMN)}_column.txt"))
            {
                var columnD = HtmSerializer2.Deserialize<Column>(sr);
                Assert.IsTrue(column.Equals(columnD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_SPARSEBINARYMATRIXS()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            int[] dimension = { 100, 100 };

            SparseBinaryMatrix matrix = new SparseBinaryMatrix(dimension,false);            

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_SPARSEBINARYMATRIXS)}_sbmatrix.txt"))
            {
                HtmSerializer2.Serialize(matrix, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SPARSEBINARYMATRIXS)}_sbmatrix.txt"))
            {
                var matrixD = HtmSerializer2.Deserialize<SparseBinaryMatrix>(sr);
                Assert.IsTrue(matrix.Equals(matrixD));
            }
        }

  
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_CONNECTIONS()
        {
            int[] inputDims = { 100,100 };
            int[] columnDims = { 10,10 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            connections.ActiveSegments.Add(distDend);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_CONNECTIONS)}_connections.txt"))
            {
                HtmSerializer2.Serialize(connections, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_CONNECTIONS)}_connections.txt"))
            {
                Connections connectionsD = HtmSerializer2.Deserialize<Connections>(sr);
                Assert.IsTrue(connections.Equals(connectionsD));
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_HTMCONFIG()
        {
            int[] inputDims = { 10, 12,14 };
            int[] columnDims = { 1, 2, 3 };

            HtmConfig config = new HtmConfig(inputDims, columnDims);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_HTMCONFIG)}_config.txt"))
            {
                HtmSerializer2.Serialize(config, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_HTMCONFIG)}_config.txt"))
            {
                HtmConfig configD = HtmSerializer2.Deserialize<HtmConfig>(sr);
                Assert.IsTrue(config.Equals(configD));
                //TODO: Implement HtmConfigTests, that make sure the HtmConfig.Equals() Method works well. Implement many CompareMethods that make sure that ALL HtmConfig parameters are used in th eequal method.
            }
        }      

        [TestMethod]
        [TestCategory("serialization")]
        //TODO: [DataRow] We need many more different params to be sure that the serialization works well. You use currentlly single set of params = cell, 1, 2, 2, 2.0, 100
        public void Serializationtest_DISTALDENDRITE()
        {
           
            Cell cell = new Cell(12, 14, 16, new CellActivity());

            DistalDendrite distalDendrite = new DistalDendrite(cell, 1, 2, 2, 2.0, 100);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_DISTALDENDRITE)}_dd.txt"))
            {
                HtmSerializer2.Serialize(distalDendrite, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_DISTALDENDRITE)}_dd.txt"))
            {
                DistalDendrite distalDendriteD = HtmSerializer2.Deserialize<DistalDendrite>(sr);
                Assert.IsTrue(distalDendrite.Equals(distalDendriteD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_DISTRIBUTEDMEMORY()
        {
            //TODO: Need more parameter sets. use DataRow.
            Column column = new Column(2, 12, 12.2, 2);

            DistributedMemory distributedMemory = new DistributedMemory();

            distributedMemory.ColumnDictionary.Add(1, column);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_DISTRIBUTEDMEMORY)}_dm.txt"))
            {
                HtmSerializer2.Serialize(distributedMemory, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_DISTRIBUTEDMEMORY)}_dm.txt"))
            {
                DistributedMemory distributedMemoryD = HtmSerializer2.Deserialize<DistributedMemory>(sr);
                Assert.IsTrue(distributedMemory.Equals(distributedMemoryD));
            }
        }

        // TODO: Implement HtmModuleTopologyTests class that tests Equals() method.
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_HTMMODULETOPOLOGY()
        {
            int[] dimension = { 10, 12, 14 };

            HtmModuleTopology topology = new HtmModuleTopology(dimension, true);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_HTMMODULETOPOLOGY)}_topology.txt"))
            {
                HtmSerializer2.Serialize(topology, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_HTMMODULETOPOLOGY)}_topology.txt"))
            {
                HtmModuleTopology topologyD = HtmSerializer2.Deserialize<HtmModuleTopology>(sr);
                Assert.IsTrue(topology.Equals(topologyD));
            }
        }

        // TODO: Implement ProximalDentriteTests class that tests Equals() method.
        //Currently fail because the created proDent's Synapses is an empty list (after added Pool). The Deserialize object is correct.
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_PROXIMALDENTRITE()
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

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_PROXIMALDENTRITE)}_prodent.txt"))
            {
                HtmSerializer2.Serialize(proDend, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_PROXIMALDENTRITE)}_prodent.txt"))
            {
                ProximalDendrite proDendD = HtmSerializer2.Deserialize<ProximalDendrite>(sr);
                Assert.IsTrue(proDend.Equals(proDendD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_SYNAPSE()
        {  
            Cell cell = new Cell(parentColumnIndx: 1, colSeq: 20, numCellsPerColumn: 16, new CellActivity());
            Cell presynapticCell = new Cell(parentColumnIndx: 8, colSeq: 36, numCellsPerColumn: 46, new CellActivity());

            DistalDendrite dd = new DistalDendrite(parentCell: cell, flatIdx: 10, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            cell.DistalDendrites.Add(dd);

            Synapse synapse = new Synapse(presynapticCell: cell, distalSegmentIndex: dd.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            presynapticCell.ReceptorSynapses.Add(synapse);
           
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_SYNAPSE)}_synapse.txt"))
            {
                HtmSerializer2.Serialize(synapse, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SYNAPSE)}_synapse.txt"))
            {
                Synapse synapseD = HtmSerializer2.Deserialize<Synapse>(sr);
                Assert.IsTrue(synapse.Equals(synapseD));
            }
        }

        //TODO: See previous comments.
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_TOPOLOGY()
        {
            int[] shape = { 1, 2, 3 };
            bool useColumnMajorOrdering = true;
            Topology topology = new Topology(shape, useColumnMajorOrdering);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_TOPOLOGY)}_topology.txt"))
            {
                HtmSerializer2.Serialize(topology, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_TOPOLOGY)}_topology.txt"))
            {
                Topology topologyD = HtmSerializer2.Deserialize<Topology>(sr);
                Assert.IsTrue(topology.Equals(topologyD));
            }
        }


        //Currently fail. Deserialize object is not correct.
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_HOMEOSTATICPLASTICITYCONTROLLER()
        {
            int[] inputDims = { 100, 100 };
            int[] columnDims = { 10, 10 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections htmMemory = new Connections();
            int minCycles = 50;
            Action<bool, int, double, int> onStabilityStatusChanged = (isStable, numPatterns, actColAvg, seenInputs) => { };
            int numOfCyclesToWaitOnChange = 50;
            double requiredSimilarityThreshold = 0.97;

            HomeostaticPlasticityController controller = new HomeostaticPlasticityController(htmMemory, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange, requiredSimilarityThreshold);
                        
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_HOMEOSTATICPLASTICITYCONTROLLER)}_hpc.txt"))
            {
                HtmSerializer2.Serialize(controller, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_HOMEOSTATICPLASTICITYCONTROLLER)}_hpc.txt"))
            {
                HomeostaticPlasticityController controllerD = HtmSerializer2.Deserialize<HomeostaticPlasticityController>(sr);
                Assert.IsTrue(controller.Equals(controllerD));
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_SPARSEOBJECTMATRIX()
        {
            int[] dimensions = { 10, 20, 30 };
            bool useColumnMajorOrdering = false;

            SparseObjectMatrix<Column> matrix = new SparseObjectMatrix<Column>(dimensions, useColumnMajorOrdering, dict: null);

            // TODO: This test must initialize a full set of columns.
            /*
            for (int i = 0; i < numColumns; i++)
            {
                Column column = colZero == null ?
                    new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs) : matrix.GetObject(i);

                for (int j = 0; j < cellsPerColumn; j++)
                {
                    cells[i * cellsPerColumn + j] = column.Cells[j];
                }
                //If columns have not been previously configured
                if (colZero == null)
                    matrix.set(i, column);

            }*/

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_SPARSEOBJECTMATRIX)}_hpc.txt"))
            {
                HtmSerializer2.Serialize(matrix, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SPARSEOBJECTMATRIX)}_hpc.txt"))
            {
                SparseObjectMatrix<Column> matrixD = HtmSerializer2.Deserialize<SparseObjectMatrix<Column>>(sr);
                Assert.IsTrue(matrix.Equals(matrixD));
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_POOL()
        {
            Pool pool = new Pool(size: 1, numInputs: 200);

            Cell cell = new Cell(parentColumnIndx: 1, colSeq: 20, numCellsPerColumn: 16, new CellActivity());
            Cell preSynapticCell = new Cell(parentColumnIndx: 2, colSeq: 22, numCellsPerColumn: 26, new CellActivity());

            DistalDendrite dd = new DistalDendrite(parentCell: cell, flatIdx: 10, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            cell.DistalDendrites.Add(dd);

            Synapse synapse = new Synapse(presynapticCell: cell, distalSegmentIndex: dd.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            preSynapticCell.ReceptorSynapses.Add(synapse);

            pool.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            pool.m_SynapsesBySourceIndex.Add(2, synapse);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_POOL)}_pool.txt"))
            {
                HtmSerializer2.Serialize(pool, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_POOL)}_pool.txt"))
            {
                Pool poolD = HtmSerializer2.Deserialize<Pool>(sr);
                Assert.IsTrue(pool.Equals(poolD));
            }
        }

        //Test failed. Possible cause: equal method of ComputeCycle object. ActiveCells.equal checks for reference equality, so it'll return false everytime. (even if both list are empty)
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_COMPUTECYCLE()
        {
            int[] inputDims = { 100, 100 };
            int[] columnDims = { 10, 10 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            connections.ActiveSegments.Add(distDend);

            ComputeCycle computeCycle = new ComputeCycle(connections);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_COMPUTECYCLE)}_compute.txt"))
            {
                HtmSerializer2.Serialize(computeCycle, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_COMPUTECYCLE)}_compute.txt"))
            {
                ComputeCycle computeCycleD = HtmSerializer2.Deserialize<ComputeCycle>(sr);
                Assert.IsTrue(computeCycle.Equals(computeCycleD)); //TODO: Implement ComputeCycleTest.cs
            }
        }

        
    }
}
