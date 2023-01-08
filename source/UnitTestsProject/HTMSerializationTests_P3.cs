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
    //DONE: All test should have a prefix 'Serializationtest_TESTNAME'
    //DONE:[DataRow] Added more different params to be sure that the serialization works well.
    //DONE: Implement HtmConfigTests, HtmModuleTopologyTests, ProximalDentriteTests, TopologyTests, ComputeCycleTests that make sure the .Equals() Method works well.
    [TestClass]
    public class HTMSerializationTests_P3
    {
        /// <summary>
        /// Test the serialization of Column
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(1, 2, 1.0, 1)]
        [DataRow(2, 5, 18.3, 20)]
        [DataRow(10, 25, 12.0, 100)]
        [DataRow(12, 14, 18.7, 1000)]
        public void Serializationtest_COLUMN(int numCells, int colIndx, double synapsePermConnected, int numInputs)
        {
            HtmSerializer serializer = new HtmSerializer();

            HtmConfig config = new HtmConfig(new int[] { 5 }, new int[] { 5 }) {CellsPerColumn = numCells, SynPermConnected = synapsePermConnected, NumInputs = numInputs };

            Synapse synapse = QuickSetupSynapse();

            Column column = new Column(config.CellsPerColumn, colIndx, config.SynPermConnected, config.NumInputs);

            column.CreatePotentialPool(config, new int[] { 1, 2, 3 }, -1);

            column.SetPermanences(config, new double[] {1.0, 3.0, 7.0});            

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_COLUMN)}_column.txt"))
            {
                HtmSerializer.Serialize(column, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_COLUMN)}_column.txt"))
            {
                var columnD = HtmSerializer.Deserialize<Column>(sr);
                Assert.IsTrue(column.Equals(columnD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 100, 100 }, true)]
        [DataRow(new int[] { 10, 100, 1000 }, true)]
        [DataRow(new int[] { 12, 14, 16, 18 }, false)]
        [DataRow(new int[] { 100, 1000, 10000, 100000, 1000000 }, false)]
        public void Serializationtest_SPARSEBINARYMATRIXS(int[] dimensions, bool useColumnMajorOrdering)
        {
            HtmSerializer serializer = new HtmSerializer();

            SparseBinaryMatrix matrix = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);

            for (int i = 0; i < 20; i++)
            {
                matrix.set(7, new int[] { i, 1 });
            }
        
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_SPARSEBINARYMATRIXS)}_sbmatrix.txt"))
            {
                HtmSerializer.Serialize(matrix, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SPARSEBINARYMATRIXS)}_sbmatrix.txt"))
            {
                var matrixD = HtmSerializer.Deserialize<SparseBinaryMatrix>(sr);
                Assert.IsTrue(matrix.Equals(matrixD));
                Assert.IsTrue(matrix.get(new int[] { 1, 2 }).Value == 7);
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 100, 100 }, new int[] { 10, 10 }, 12, 14, 16, 1, 2, 2, 2.0, 100)]
        [DataRow(new int[] { 100, 100 }, new int[] { 10, 10 }, 100, 256, 1000, 10, 20, 20, 1.0, 100)]
        [DataRow(new int[] { 2, 4, 8 }, new int[] { 128, 256, 512 }, 12, 14, 16, 1, 4, 8, 4.0, 1000)]
        [DataRow(new int[] { 2, 4, 8 }, new int[] { 128, 256, 512 }, 1, 1, 2, 1, 2, 2, 2.0, 100)]
        public void Serializationtest_CONNECTIONS(int[] inputDims, int[] columnDims, int parentColumnIndx, int colSeq, int numCellsPerColumn,
            int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs)
        {
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(parentColumnIndx, colSeq, numCellsPerColumn, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            connections.ActiveSegments.Add(distDend);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_CONNECTIONS)}_connections.txt"))
            {
                HtmSerializer.Serialize(connections, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_CONNECTIONS)}_connections.txt"))
            {
                Connections connectionsD = HtmSerializer.Deserialize<Connections>(sr);
                Assert.IsTrue(connections.Equals(connectionsD));
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 8000 }, new int[] { 100 })]
        [DataRow(new int[] { 100, 100 }, new int[] { 10, 10 })]
        [DataRow(new int[] { 2, 4, 8 }, new int[] { 128, 256, 512 })]
        [DataRow(new int[] { 256 }, new int[] { 10, 15 })]
        public void Serializationtest_HTMCONFIG(int[] inputDims, int[] columnDims)
        {
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_HTMCONFIG)}_config.txt"))
            {
                HtmSerializer.Serialize(config, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_HTMCONFIG)}_config.txt"))
            {
                HtmConfig configD = HtmSerializer.Deserialize<HtmConfig>(sr);
                Assert.IsTrue(config.Equals(configD));
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(1, 2, 4, 1, 2, 2, 2.0, 100)]
        [DataRow(11, 12, 22, 10, 20, 20, 1.0, 100)]
        [DataRow(12, 14, 16, 1, 4, 8, 4.0, 1000)]
        [DataRow(100, 200, 400, 10, 20, 20, 20.0, 1000)]
        public void Serializationtest_DISTALDENDRITE(int parentColumnIndx, int colSeq, int numCellsPerColumn, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs)
        {
            Cell cell = new Cell(parentColumnIndx, colSeq, numCellsPerColumn, new CellActivity());
            DistalDendrite distalDendrite = new DistalDendrite(cell, flatIdx, lastUsedIteration, ordinal, synapsePermConnected, numInputs);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_DISTALDENDRITE)}_dd.txt"))
            {
                HtmSerializer.Serialize(distalDendrite, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_DISTALDENDRITE)}_dd.txt"))
            {
                DistalDendrite distalDendriteD = HtmSerializer.Deserialize<DistalDendrite>(sr);
                Assert.IsTrue(distalDendrite.Equals(distalDendriteD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(1, 1, 1.0, 1)]
        [DataRow(2, 5, 8.3, 2)]
        [DataRow(10, 25, 10.0, 100)]
        [DataRow(12, 14, 8.7, 1000)]
        public void Serializationtest_DISTRIBUTEDMEMORY(int numCells, int colIndx, double synapsePermConnected, int numInputs)
        {
            Column column = new Column(numCells, colIndx, synapsePermConnected, numInputs);

            DistributedMemory distributedMemory = new DistributedMemory();

            distributedMemory.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(1);
            distributedMemory.ColumnDictionary.Add(1, column);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_DISTRIBUTEDMEMORY)}_dm.txt"))
            {
                HtmSerializer.Serialize(distributedMemory, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_DISTRIBUTEDMEMORY)}_dm.txt"))
            {
                DistributedMemory distributedMemoryD = HtmSerializer.Deserialize<DistributedMemory>(sr);
                Assert.IsTrue(distributedMemory.Equals(distributedMemoryD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 1, 2, 4 }, true)]
        [DataRow(new int[] { 10, 12, 14 }, false)]
        [DataRow(new int[] { 1028 }, true)]
        [DataRow(new int[] { 100, 1000, 10000, 100000 }, false)]
        public void Serializationtest_HTMMODULETOPOLOGY(int[] dimension, bool isMajorOrdering)
        {

            HtmModuleTopology topology = new HtmModuleTopology(dimension, isMajorOrdering);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_HTMMODULETOPOLOGY)}_topology.txt"))
            {
                HtmSerializer.Serialize(topology, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_HTMMODULETOPOLOGY)}_topology.txt"))
            {
                HtmModuleTopology topologyD = HtmSerializer.Deserialize<HtmModuleTopology>(sr);
                Assert.IsTrue(topology.Equals(topologyD));
            }
        }


        //Currently fail because the created proDent's Synapses is an empty list (after added Pool). The Deserialize object is correct.
        //Equal() method tested.
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
                HtmSerializer.Serialize(proDend, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_PROXIMALDENTRITE)}_prodent.txt"))
            {
                ProximalDendrite proDendD = HtmSerializer.Deserialize<ProximalDendrite>(sr);
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
                HtmSerializer.Serialize(synapse, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SYNAPSE)}_synapse.txt"))
            {
                Synapse synapseD = HtmSerializer.Deserialize<Synapse>(sr);
                Assert.IsTrue(synapse.Equals(synapseD));
            }
        }


        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 1, 2, 4 }, true)]
        [DataRow(new int[] { 10, 12, 14 }, false)]
        [DataRow(new int[] { 1028 }, true)]
        [DataRow(new int[] { 100, 1000, 10000, 100000 }, false)]
        public void Serializationtest_TOPOLOGY(int[] shape, bool useColumnMajorOrdering)
        {
            Topology topology = new Topology(shape, useColumnMajorOrdering);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_TOPOLOGY)}_topology.txt"))
            {
                HtmSerializer.Serialize(topology, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_TOPOLOGY)}_topology.txt"))
            {
                Topology topologyD = HtmSerializer.Deserialize<Topology>(sr);
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
                HtmSerializer.Serialize(controller, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_HOMEOSTATICPLASTICITYCONTROLLER)}_hpc.txt"))
            {
                HomeostaticPlasticityController controllerD = HtmSerializer.Deserialize<HomeostaticPlasticityController>(sr);
                Assert.IsTrue(controller.Equals(controllerD));
            }
        }

        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_SPARSEOBJECTMATRIX()
        {
            // This test must initialize a full set of columns.

            /*for (int i = 0; i < numColumns; i++)
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

            //Setup SparseObjectMatrix
            int[] dimensions = { 20, 10 };
            bool useColumnMajorOrdering = false;

            SparseObjectMatrix<Column> matrix = new SparseObjectMatrix<Column>(dimensions, useColumnMajorOrdering, dict: null);

            //Setup connection
            int[] inputDims = { 20, 10 };
            int[] columnDims = { 10, 100 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);
            Connections connections = new Connections(config);

            //Setup columns
            int numColumns = 20;
            int cellsPerColumn = 10;

            for (int i = 0; i < numColumns; i++)
            {
                Column column = new Column(cellsPerColumn, i, connections.HtmConfig.SynPermConnected, connections.HtmConfig.NumInputs);
                matrix.set(i, column);
            }

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_SPARSEOBJECTMATRIX)}_hpc.txt"))
            {
                HtmSerializer.Serialize(matrix, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SPARSEOBJECTMATRIX)}_hpc.txt"))
            {
                SparseObjectMatrix<Column> matrixD = HtmSerializer.Deserialize<SparseObjectMatrix<Column>>(sr);
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
                HtmSerializer.Serialize(pool, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_POOL)}_pool.txt"))
            {
                Pool poolD = HtmSerializer.Deserialize<Pool>(sr);
                Assert.IsTrue(pool.Equals(poolD));
            }
        }

        //Test passed. Equal method of ComputeCycle object fixed.
        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 100, 100 }, new int[] { 10, 10 }, 11, 12, 22, 10, 20, 20, 1.0, 100)]
        [DataRow(new int[] { 2, 14, 128 }, new int[] { 8, 8 }, 2, 4, 6, 100, 256, 256, 4.0, 1000)]
        [DataRow(new int[] { 10 }, new int[] { 12 }, 1, 1, 1, 2, 2, 2, 4.0, 40)]
        [DataRow(new int[] { 12, 14, 16 }, new int[] { 10, 100 }, 12, 14, 16, 18, 20, 20, 8.9, 10)]
        public void Serializationtest_COMPUTECYCLE(int[] inputDims, int[] columnDims, int parentColumnIndx, int colSeq, int numCellsPerColumn, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs)
        {

            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(parentColumnIndx, colSeq, numCellsPerColumn, new CellActivity());

            var distDend = new DistalDendrite(cell, flatIdx, lastUsedIteration, ordinal, synapsePermConnected, numInputs);

            connections.ActiveSegments.Add(distDend);

            ComputeCycle computeCycle = new ComputeCycle(connections);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_COMPUTECYCLE)}_compute.txt"))
            {
                HtmSerializer.Serialize(computeCycle, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_COMPUTECYCLE)}_compute.txt"))
            {
                ComputeCycle computeCycleD = HtmSerializer.Deserialize<ComputeCycle>(sr);
                Assert.IsTrue(computeCycle.Equals(computeCycleD));
            }
        }


        #region Helper
        private HtmConfig QuickSetupHtmConfig()
        {
            var htmConfig = new HtmConfig(new int[] { 5 }, new int[] { 5 })
            {
            // Temporal Memory parameters
            CellsPerColumn = 32,
            ActivationThreshold = 10,
            LearningRadius = 10,
            MinThreshold = 9,
            MaxNewSynapseCount = 20,
            MaxSynapsesPerSegment = 225,
            MaxSegmentsPerCell = 225,
            InitialPermanence = 0.21,
            ConnectedPermanence = 0.5,
            PermanenceIncrement = 0.10,
            PermanenceDecrement = 0.10,
            PredictedSegmentDecrement = 0.1,

            // Spatial Pooler parameters

            PotentialRadius = 15,
            PotentialPct = 0.75,
            GlobalInhibition = true,
            LocalAreaDensity = -1.0,
            NumActiveColumnsPerInhArea = 0.02 * 2048,
            StimulusThreshold = 5.0,
            SynPermInactiveDec = 0.008,
            SynPermActiveInc = 0.05,
            SynPermConnected = 0.1,
            SynPermBelowStimulusInc = 0.01,
            SynPermTrimThreshold = 0.05,
            MinPctOverlapDutyCycles = 0.001,
            MinPctActiveDutyCycles = 0.001,
            DutyCyclePeriod = 1000,
            MaxBoost = 10.0,
            WrapAround = true,
            Random = new ThreadSafeRandom(42),
        };

            return htmConfig;
        } 

        private Synapse QuickSetupSynapse()
        {
            Cell cell = new Cell(parentColumnIndx: 1, colSeq: 20, numCellsPerColumn: 16, new CellActivity());
            Cell presynapticCell = new Cell(parentColumnIndx: 8, colSeq: 36, numCellsPerColumn: 46, new CellActivity());

            DistalDendrite dd = new DistalDendrite(parentCell: cell, flatIdx: 10, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            cell.DistalDendrites.Add(dd);

            Synapse synapse = new Synapse(presynapticCell: cell, distalSegmentIndex: dd.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            presynapticCell.ReceptorSynapses.Add(synapse);

            return synapse;
        }
        #endregion

    }
}
