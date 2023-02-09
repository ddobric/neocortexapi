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
        /// Test the serialization of Column. Equal method is tested at ColumnTest.
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(10, 1, 1.0, 1)]
        public void Serializationtest_COLUMN(int numCells, int colIndx, double synapsePermConnected, int numInputs)
        {
            HtmSerializer serializer = new HtmSerializer();

            HtmConfig config = new HtmConfig(new int[] { 5 }, new int[] { 5 })
            {
                CellsPerColumn = numCells,
                SynPermConnected = synapsePermConnected,
                NumInputs = numInputs,
                SynPermTrimThreshold = 0.05,
                SynPermMax = 1.0
            };

            Column column = new Column(config.CellsPerColumn, colIndx, config.SynPermConnected, config.NumInputs);

            // Creates connections between mini-columns and input neurons. All permanences are at the begining set to 0
            column.CreatePotentialPool(config, new int[] { 1, 7, 9 }, -1);

            //Updates the permanence matrix with a column's new permanence values
            column.UpdatePermanencesForColumnSparse(config,perm:new double[] {0.1,0.27},maskPotential: new int[] {1,7},false);

            //Serialize the column and save to a text file
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_COLUMN)}_column.txt"))
            {
                HtmSerializer.Serialize(column, null, sw);
            }

            //Deserialize the text file created above and compare with the original column
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_COLUMN)}_column.txt"))
            {
                var columnD = HtmSerializer.Deserialize<Column>(sr);
                Assert.IsTrue(column.Equals(columnD));
            }
        }

        /// <summary>
        /// Test the serialization of Sparse binary matrix. Equal method is tested at SparseBinaryMatrixsTests.
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 100, 100 }, true)]
        //[DataRow(new int[] { 10, 100, 1000 }, true)]
        //[DataRow(new int[] { 12, 14, 16, 18 }, false)]
        //[DataRow(new int[] { 100, 1000, 10000, 100000, 1000000 }, false)]
        public void Serializationtest_SPARSEBINARYMATRIXS(int[] dimensions, bool useColumnMajorOrdering)
        {
            HtmSerializer serializer = new HtmSerializer();

            //Create an empty matrix
            SparseBinaryMatrix matrix = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);

            //Sets the value to be indexed
            for (int i = 0; i < 20; i++)
            {
                matrix.set(value: 7,coordinates: new int[] { i, 1 });
            }
            for(int i = 20; i < 100; i++)
            {
                matrix.set(value: 9, coordinates: new int[] { i, 2 });
            }
        
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_SPARSEBINARYMATRIXS)}_sbmatrix.txt"))
            {
                HtmSerializer.Serialize(matrix, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_SPARSEBINARYMATRIXS)}_sbmatrix.txt"))
            {
                var matrixD = HtmSerializer.Deserialize<SparseBinaryMatrix>(sr);

                //Check if Deserialized matrix is equal with original
                Assert.IsTrue(matrix.Equals(matrixD));

                //Check if values are correct
                for (int i = 0; i < 20; i++)
                {
                    Assert.IsTrue(matrixD.get(coordinates: new int[] { i, 1 }).Value == 7);
                }

                for (int i = 20; i < 100; i++)
                {
                    Assert.IsTrue(matrixD.get(coordinates: new int[] { i, 2 }).Value == 9);
                }

            }
        }

        //TODO: Need to test more
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

            //Create connections from config
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

        /// <summary>
        /// Test the serialization of HtmConfig. Equal method is tested at HtmConfigTests.
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        //[DataRow(new int[] { 8000 }, new int[] { 100 })]
        //[DataRow(new int[] { 100, 100 }, new int[] { 10, 10 })]
        [DataRow(new int[] { 2, 4, 8 }, new int[] { 128, 256, 512 })]
        //[DataRow(new int[] { 256 }, new int[] { 10, 15 })]
        public void Serializationtest_HTMCONFIG(int[] inputDims, int[] columnDims)
        {
            //Create a HtmConfig with input parameters InputDimensions and ColumnDimensions. Other parameters are set with default value.
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            //Create ColumnTopology and InputTopology and update some of the parameters
            config.ColumnTopology = new Topology(config.ColumnDimensions);
            config.InputTopology = new Topology(config.InputDimensions);

            //Update parameters
            config.CellsPerColumn = 32;
            config.ActivationThreshold = 16;
            config.LearningRadius = 12;
            config.MinThreshold = 10;
            config.MaxNewSynapseCount = 20;
            config.MaxSynapsesPerSegment = 228;
            config.MaxSegmentsPerCell = 228;
            config.InitialPermanence = 0.21;
            config.SynPermConnected = 0.5;
            config.PermanenceIncrement = 0.15;
            config.PermanenceDecrement = 0.15;
            config.PredictedSegmentDecrement = 0.1;
            config.PotentialRadius = 15;
            config.PotentialPct = 0.75;
            config.GlobalInhibition = true;
            config.LocalAreaDensity = -1.0;
            config.NumActiveColumnsPerInhArea = 0.02 * 2048;
            config.StimulusThreshold = 5.0;
            config.SynPermInactiveDec = 0.008;
            config.SynPermActiveInc = 0.05;
            config.SynPermConnected = 0.1;
            config.SynPermBelowStimulusInc = 0.01;
            config.SynPermTrimThreshold = 0.05;
            config.MinPctOverlapDutyCycles = 0.001;
            config.MinPctActiveDutyCycles = 0.001;
            config.DutyCyclePeriod = 1000;
            config.MaxBoost = 10.0;
            config.WrapAround = true;
            config.Random = new ThreadSafeRandom(42);

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


        /// <summary>
        /// Test the serialization of DistalDendrite. Equal method is tested at DistalDendriteTests.
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(1, 2, 4, 1, 2, 2, 2.0, 100)]
        [DataRow(11, 12, 22, 10, 20, 20, 1.0, 100)]
        [DataRow(12, 14, 16, 1, 4, 8, 4.0, 1000)]
        [DataRow(100, 200, 400, 10, 20, 20, 20.0, 1000)]
        public void Serializationtest_DISTALDENDRITE(int parentColumnIndx, int colSeq, int numCellsPerColumn, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs)
        {
            //Create a DistalDendrite with a parent cell.
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

        /// <summary>
        /// Test the serialization of DistributedMemory. Equal method is tested at DistributedMemoryTests.
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        public void Serializationtest_DISTRIBUTEDMEMORY()
        {
            Column column1 = new Column(numCells: 1, colIndx: 1, synapsePermConnected: 1.0, numInputs: 1);
            Column column2 = new Column(numCells: 15, colIndx: 2, synapsePermConnected: 1.0, numInputs: 3);
            Column column3 = new Column(numCells: 12, colIndx: 3, synapsePermConnected: 1.0, numInputs: 5);

            DistributedMemory distributedMemory = new DistributedMemory();

            distributedMemory.ColumnDictionary = new InMemoryDistributedDictionary<int, Column>(3);
            distributedMemory.ColumnDictionary.Add(1, column1);
            distributedMemory.ColumnDictionary.Add(2, column2);
            distributedMemory.ColumnDictionary.Add(3, column3);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_DISTRIBUTEDMEMORY)}_dm.txt"))
            {
                HtmSerializer.Serialize(distributedMemory, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_DISTRIBUTEDMEMORY)}_dm.txt"))
            {
                DistributedMemory distributedMemoryD = HtmSerializer.Deserialize<DistributedMemory>(sr);

                //Check if Deserialized DistributedMemory is equal with original
                Assert.IsTrue(distributedMemory.Equals(distributedMemoryD));

                //Check column count in both DistributedMemory
                Assert.IsTrue(distributedMemory.ColumnDictionary.Count.Equals(3));
                Assert.IsTrue(distributedMemoryD.ColumnDictionary.Count.Equals(3));

                //Check if all columns are the same
                Assert.IsTrue(distributedMemory.ColumnDictionary.ElementsEqual(distributedMemoryD.ColumnDictionary));
            }
        }

        /// <summary>
        /// Test the serialization of HtmModuleTopology. Equal method is tested at HtmModuleTopologyTests.
        /// </summary>
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


        /// <summary>
        /// Test the serialization of Synapse. Equal method is tested at SynapseTests.
        /// </summary>
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

        /// <summary>
        /// Test the serialization of Topology. Equal method is tested at TopologyTests.
        /// </summary>
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
                //Check if Deserialized Topology is equal with original 
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
            Cell cell = new Cell(parentColumnIndx: 1, colSeq: 20, numCellsPerColumn: 16, new CellActivity());
            Cell cell2 = new Cell(parentColumnIndx: 2, colSeq: 21, numCellsPerColumn: 16, new CellActivity());
            Cell preSynapticCell = new Cell(parentColumnIndx: 3, colSeq: 22, numCellsPerColumn: 26, new CellActivity());

            DistalDendrite dd = new DistalDendrite(parentCell: cell, flatIdx: 10, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            cell.DistalDendrites.Add(dd);

            Synapse synapse = new Synapse(presynapticCell: cell, distalSegmentIndex: dd.SegmentIndex, synapseIndex: 23, permanence: 1.0);
            preSynapticCell.ReceptorSynapses.Add(synapse);

            //Create an empty Pool and Add synapse to SynapsesBySourceIndex of the pool
            Pool pool = new Pool(size: 1, numInputs: 200);

            pool.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            pool.m_SynapsesBySourceIndex.Add(2, synapse);

            //Update Pool with new synapse
            //TODO: Should UpdatePool update size of the pool? - After update, the Deserialized Pool's size is 2, the original Pool's size is 1.
            DistalDendrite dd2 = new DistalDendrite(parentCell: cell2, flatIdx: 11, lastUsedIteration: 20, ordinal: 10, synapsePermConnected: 15, numInputs: 100);
            Synapse synapse2 = new Synapse(presynapticCell: cell2, distalSegmentIndex: dd2.SegmentIndex, synapseIndex: 21, permanence: 1.0);

            pool.UpdatePool(synPermConnected: 1.0, synapse2, permanence: 2.0);
            
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_POOL)}_pool.txt"))
            {
                HtmSerializer.Serialize(pool, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_POOL)}_pool.txt"))
            {
                Pool poolD = HtmSerializer.Deserialize<Pool>(sr);
                //Check if Deserialized Pool is equal with original 
                Assert.IsTrue(pool.Equals(poolD));

                //Compare SparsePermanences of original Pool and Deserialized Pool
                Assert.IsTrue(poolD.GetSparsePermanences().ElementsEqual(pool.GetSparsePermanences()));
                Assert.IsTrue(poolD.GetSparsePermanences().ElementsEqual(pool.GetSparsePermanences()));
                Assert.IsTrue(poolD.GetDenseConnected().ElementsEqual(pool.GetDenseConnected()));
            }
        }

        /// <summary>
        /// Test the serialization of ComputeCycle. Equal method is tested at ComputeCycleTests.
        /// DONE: Fixed Equal method of ComputeCycle object.
        /// </summary>
        [TestMethod]
        [TestCategory("serialization")]
        [DataRow(new int[] { 100, 100 }, new int[] { 10, 10 })]
        [DataRow(new int[] { 2, 14, 128 }, new int[] { 8, 8 })]
        [DataRow(new int[] { 10 }, new int[] { 12 })]
        [DataRow(new int[] { 12, 14, 16 }, new int[] { 10, 100 })]
        public void Serializationtest_COMPUTECYCLE(int[] inputDims, int[] columnDims)
        {

            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(1, 12, 22, new CellActivity());
            Cell cell2 = new Cell(1, 1, 10, new CellActivity());
            Cell cell3 = new Cell(2, 4, 6, new CellActivity());
            Cell cell4 = new Cell(2, 1, 12, new CellActivity());

            Cell activeCell1 = new Cell(1, 1, 10, new CellActivity());
            Cell activeCell2 = new Cell(1, 2, 20, new CellActivity());
            Cell activeCell3 = new Cell(2, 2, 10, new CellActivity());

            Cell winnerCell1 = new Cell(10, 10, 10, new CellActivity());
            Cell winnerCell2 = new Cell(12, 22, 20, new CellActivity());
            Cell winnerCell3 = new Cell(22, 32, 30, new CellActivity());

            DistalDendrite distDend1 = new DistalDendrite(cell, flatIdx: 10, lastUsedIteration: 20, ordinal: 20, synapsePermConnected: 1.0, numInputs: 100);
            DistalDendrite distDend2 = new DistalDendrite(cell2, flatIdx: 1,lastUsedIteration: 1,ordinal: 1,synapsePermConnected: 0.5,numInputs: 10);
            DistalDendrite distDend3 = new DistalDendrite(cell3, flatIdx: 2, lastUsedIteration: 256, ordinal: 7, synapsePermConnected: 1.0, numInputs: 1);
            DistalDendrite distDend4 = new DistalDendrite(cell4, flatIdx: 3, lastUsedIteration: 12, ordinal: 2, synapsePermConnected: 0.5, numInputs: 2);

            connections.ActiveSegments.Add(distDend1);
            connections.ActiveSegments.Add(distDend2);
            connections.MatchingSegments.Add(distDend3);
            connections.MatchingSegments.Add(distDend4);

            connections.ActiveCells.Add(activeCell1);
            connections.ActiveCells.Add(activeCell2);
            connections.ActiveCells.Add(activeCell3);

            connections.WinnerCells.Add(winnerCell1);
            connections.WinnerCells.Add(winnerCell2);
            connections.WinnerCells.Add(winnerCell3);

            ComputeCycle computeCycle = new ComputeCycle(connections);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Serializationtest_COMPUTECYCLE)}_compute.txt"))
            {
                HtmSerializer.Serialize(computeCycle, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Serializationtest_COMPUTECYCLE)}_compute.txt"))
            {
                ComputeCycle computeCycleD = HtmSerializer.Deserialize<ComputeCycle>(sr);

                //Check if Deserialized ComputeCycle is equal with original 
                Assert.IsTrue(computeCycle.Equals(computeCycleD));

                //Check if values inside each ComputeCycle are the same
                Assert.IsTrue(computeCycle.ActiveCells.ElementsEqual(computeCycleD.ActiveCells));
                Assert.IsTrue(computeCycle.WinnerCells.ElementsEqual(computeCycleD.WinnerCells));
                Assert.IsTrue(computeCycle.ActiveSegments.ElementsEqual(computeCycleD.ActiveSegments));
                Assert.IsTrue(computeCycle.MatchingSegments.ElementsEqual(computeCycleD.MatchingSegments));
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
