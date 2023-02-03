using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace UnitTestsProject
{
    [TestClass]
    public partial class HTMSerializationTests
    {

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeValueTest()
        {
            HtmSerializer htm = new HtmSerializer();

            using (StreamWriter sw = new StreamWriter("ser.txt"))
            {             
                htm.SerializeBegin("UnitTest", sw);

                htm.SerializeValue(15, sw);
                htm.SerializeValue(12.34, sw);
                htm.SerializeValue(12345678, sw);
                htm.SerializeValue("Hello", sw);
                htm.SerializeValue(true, sw);
                htm.SerializeEnd("UnitTest", sw);
            }

            using (StreamReader sr = new StreamReader("ser.txt"))
            {
                int intfulldata;
                double doublefulldata;
                long longfulldata;
                string stringfulldata;
                bool boolfulldata;
                while (sr.Peek() > 0)
                {
                    string data = sr.ReadLine();
                    if (data == string.Empty || data == htm.ReadBegin("UnitTest"))
                    {
                        continue;
                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    {
                                        intfulldata = htm.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 1:
                                    {
                                        doublefulldata = htm.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 2:
                                    {
                                        longfulldata = htm.ReadLongValue(str[i]);
                                        break;
                                    }
                                case 3:
                                    {
                                        stringfulldata = htm.ReadStringValue(str[i]);
                                        break;
                                    }
                                case 4:
                                    {
                                        boolfulldata = htm.ReadBoolValue(str[i]);
                                        break;
                                    }
                                default:
                                    { break; }

                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeArrayDouble()
        {
            HtmSerializer htm = new HtmSerializer();
            Double[] vs = new Double[10];
            Double[] vs1 = new Double[10];
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeArrayDouble)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                for (int i = 0; i < 10; i++)
                {
                    vs[i] = i;
                }

                htm.SerializeValue(vs, sw);

                htm.SerializeEnd("UnitTest", sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeArrayDouble)}.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == String.Empty || data == htm.ReadBegin("UnitTest"))
                    {
                        continue;
                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    vs1 = htm.ReadArrayDouble(str[i]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }
            Assert.IsTrue(vs1.SequenceEqual(vs));
        }


        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeArrayInt()
        {
            HtmSerializer htm = new HtmSerializer();
            int[] vs = new int[10];
            int[] vs1 = new int[10];
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeArrayInt)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                for (int i = 0; i < 10; i++)
                {
                    vs[i] = i;
                }

                htm.SerializeValue(vs, sw);

                htm.SerializeEnd("UnitTest", sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeArrayInt)}.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == String.Empty || data == htm.ReadBegin("UnitTest"))
                    {
                        continue;
                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    vs1 = htm.ReadArrayInt(str[i]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }
            Assert.IsTrue(vs1.SequenceEqual(vs));
        }



        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeArrayCell()
        {
            HtmSerializer htm = new HtmSerializer();
            Cell[] cells = new Cell[2];
            Cell[] cells1;
            cells[0] = new Cell(12, 14, 16,  new CellActivity());

            var distSeg1 = new DistalDendrite(cells[0], 1, 2, 2, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cells[0], 44, 24, 34, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg2);

            Cell preSynapticcell = new Cell(11, 14, 16,  new CellActivity());

            var synapse1 = new Synapse(cells[0], distSeg1.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse2 = new Synapse(cells[0], distSeg2.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);

            cells[1] = new Cell(2, 4, 1, new CellActivity());

            var distSeg3 = new DistalDendrite(cells[1], 3, 4, 7, 1.0, 100);
            cells[1].DistalDendrites.Add(distSeg3);

            var distSeg4 = new DistalDendrite(cells[1], 4, 34, 94, 1.0, 100);
            cells[1].DistalDendrites.Add(distSeg4);

            Cell preSynapticcell1 = new Cell(1, 1, 6, new CellActivity());

            var synapse3 = new Synapse(cells[1], distSeg3.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse4 = new Synapse(cells[1], distSeg4.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeArrayCell)}.txt"))
            {
                htm.SerializeValue(cells, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeArrayCell)}.txt"))
            {
                //cells1 = htm.DeserializeCellArray(data,sr);
            }
            //Assert.IsTrue(cells.SequenceEqual(cells1));
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDictionaryStringint()
        {
            HtmSerializer htm = new HtmSerializer();
            Dictionary<String, int> keyValues = new Dictionary<string, int>();
            keyValues.Add("Hello", 1);
            keyValues.Add("Welcome", 2);
            keyValues.Add("Bye", 3);
            Dictionary<String, int> keyValuePairs = new Dictionary<string, int>();
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeDictionaryStringint)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                htm.SerializeValue(keyValues, sw);

                htm.SerializeEnd("UnitTest", sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeDictionaryStringint)}.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == String.Empty || data == htm.ReadBegin("UnitTest"))
                    {

                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    keyValuePairs = htm.ReadDictSIValue(str[i]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }

            Assert.IsTrue(keyValuePairs.SequenceEqual(keyValues));
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDictionaryIntint()
        {
            HtmSerializer htm = new HtmSerializer();
            Dictionary<int, int> keyValues = new Dictionary<int, int>();
            keyValues.Add(23, 1);
            keyValues.Add(24, 2);
            keyValues.Add(35, 3);
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeDictionaryIntint)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                htm.SerializeValue(keyValues, sw);

                htm.SerializeEnd("UnitTest", sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeDictionaryIntint)}.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == String.Empty || data == htm.ReadBegin("UnitTest"))
                    {

                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    keyValuePairs = htm.ReadDictionaryIIValue(str[i]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }

            Assert.IsTrue(keyValuePairs.SequenceEqual(keyValues));
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDictionarystringintA()
        {
            HtmSerializer htm = new HtmSerializer();
            Dictionary<String, int[]> keyValues = new Dictionary<String, int[]>
            {
                { "Hello", new int[] { 1, 2, 3 } },
                { "GoodMorning", new int[] { 4, 5, 6 } },
                { "Goodevening", new int[] { 7, 8, 9 } }
            };
            Dictionary<String, int[]> keyValuePairs = new Dictionary<String, int[]>();
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeDictionarystringintA)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                htm.SerializeValue(keyValues, sw);

                htm.SerializeEnd("UnitTest", sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeDictionarystringintA)}.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == String.Empty || data == htm.ReadBegin("UnitTest"))
                    {

                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    keyValuePairs = htm.ReadDictSIarray(str[i]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }

            Assert.IsTrue(keyValuePairs.SequenceEqual(keyValues));
        }



        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDictionaryTest()
        {
            //Proximal + Distal
            //Dictionary<Segment, List<Synapse>> keyValues, StreamWriter sw
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeSegmentDictionaryTest()
        {
            //Proximal + Distal
            //Dictionary<Segment, List<Synapse>> keyValues, StreamWriter sw
        }
        /// <summary>
        /// Test SpatialPooler.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeSpatialPooler()
        {
            HomeostaticPlasticityController homeostaticPlasticityActivator = new HomeostaticPlasticityController();
            SpatialPooler spatial = new SpatialPooler(homeostaticPlasticityActivator);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSpatialPooler)}.txt"))
            {
                spatial.Serialize(sw);
            }

        }

        /// <summary>
        /// Test HomeostaticPlasticityController.
        /// </summary>
        //[TestMethod]
        //[TestCategory("Serialization")]
        //[DataRow(3, 2, 6.2)]
        //public void SerializeHomeostaticPlasticityController(int minCycles, int numOfCyclesToWaitOnChange, double requiredSimilarityThreshold)
        //{
        //    int[] inputDims = { 3, 4, 5 };
        //    int[] columnDims = { 35, 43, 52 };
        //    HtmConfig matrix = new HtmConfig(inputDims, columnDims);
        //    Connections connections = new Connections(matrix);
        //    HomeostaticPlasticityController homeostatic = new HomeostaticPlasticityController(connections, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange , requiredSimilarityThreshold);

        //    using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeHomeostaticPlasticityController)}.txt"))
        //    {
        //        homeostatic.Serialize(sw);
        //    }

        //}

        [TestMethod]
        [TestCategory("Serialization")]

        public void SerializeConnectionsTest()
        {
            int[] inputDims = { 3, 4, 5 };
            int[] columnDims = { 35, 43, 52 };
            HtmConfig cfg = new HtmConfig(inputDims, columnDims);
            
            Connections connections = new Connections(cfg);

            Cell cells = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cells, 1, 2, 2, 1.0, 100);

            var distSeg2 = new DistalDendrite(cells, 44, 24, 34, 1.0, 100);

            connections.ActiveSegments.Add(distSeg1);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeConnectionsTest)}.txt"))
            {
                connections.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeConnectionsTest)}.txt"))
            {
                Connections connections1 = Connections.Deserialize(sr);
                Assert.IsTrue(connections.Equals(connections1));
            }
        }

        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(new int[] { 3, 4, 5 }, new int[] { 35, 43, 52 })]
        public void SerializeHtmConfigTest(int[] inputDims, int[] columnDims)
        {
            HtmConfig matrix = new HtmConfig(inputDims, columnDims);
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeHtmConfigTest)}.txt"))
            {
                matrix.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeHtmConfigTest)}.txt"))
            {
                HtmConfig matrix1 = HtmConfig.Deserialize(sr);
                Assert.IsTrue(matrix.Equals(matrix1));
            }

        }

        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(2, 5, 8.3, 2)]
        public void SerializeColumnTest(int numCells, int colIndx, double synapsePermConnected, int numInputs)
        {
            Column matrix = new Column(numCells, colIndx, synapsePermConnected, numInputs);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeColumnTest)}.txt"))
            {
                matrix.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeColumnTest)}.txt"))
            {
                //Column column = Column.Deserialize(sr);

                //Assert.IsTrue(matrix.Equals(column));
            }
        }


        /// <summary>
        /// Test Cell.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(1111, 22221, 11111, 2221)]
        public void SerializeCellTest(int parentIndx, int colSeq, int cellsPerCol, int cellId)
        {
            Cell cell = new Cell(12, 14, 16,  new CellActivity());

            var distSeg1 = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);
            cell.DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cell, 44, 24, 34, 1.0, 100);
            cell.DistalDendrites.Add(distSeg2);

            Cell preSynapticcell = new Cell(11, 14, 16, new CellActivity());

            var synapse1 = new Synapse(cell, distSeg1.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse2 = new Synapse(cell, distSeg2.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeCellTest)}.txt"))
            {
                cell.SerializeT(sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeCellTest)}.txt"))
            {
                HtmSerializer ser = new HtmSerializer();

                Cell cell1 = ser.DeserializeCell(sr);

                Assert.IsTrue(cell1.Equals(cell));
            }
        }
        /// <summary>
        /// Test DistalDendrite.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(1, 2, 2, 1.0, 100)]
        public void SerializeDistalDendrite(int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs)
        {
            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);
            cell.DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cell, 44, 24, 34, 1.0, 100);
            cell.DistalDendrites.Add(distSeg2);

            Cell preSynapticcell = new Cell(11, 14, 16, new CellActivity());

            var synapse1 = new Synapse(cell, distSeg1.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse2 = new Synapse(cell, distSeg2.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);

            // Serializes the segment to file.
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeDistalDendrite)}.txt"))
            {
                distSeg1.Serialize(sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeDistalDendrite)}.txt"))
            {

                HtmSerializer ser = new HtmSerializer();

                DistalDendrite distSegment1 = ser.DeserializeDistalDendrite(sr);

                Assert.IsTrue(distSegment1.Equals(distSeg1));

            }
        }
        ///<summary>
        ///Test Synapse.
        ///</summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(13, 87, 22.45)]
        public void SerializeSynapseTest(int segmentindex, int synapseindex, double permanence)
        {

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);
            cell.DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cell, 44, 24, 34, 1.0, 100);
            cell.DistalDendrites.Add(distSeg2);

            Cell preSynapticcell = new Cell(11, 14, 16, new CellActivity());

            var synapse1 = new Synapse(cell, distSeg1.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse2 = new Synapse(cell, distSeg2.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSynapseTest)}.txt"))
            {
                synapse1.Serialize(sw);

            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeSynapseTest)}.txt"))
            {
                HtmSerializer ser = new HtmSerializer();

                Synapse synapseT1 = ser.DeserializeSynapse(sr);

                Assert.IsTrue(synapse1.Equals(synapseT1));
            }
        }

        /// <summary>
        /// Test Pool.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(0, 34)]
        [DataRow(1, 28)]
        [DataRow(1000, 3426)]
        public void SerializePoolTest(int size, int numInputs)
        {
            Pool pool = new Pool(size, numInputs);

            //pool.m_SynapseConnections = new List<int>();
            //pool.m_SynapseConnections.Add(34);
            //pool.m_SynapseConnections.Add(87);
            //pool.m_SynapseConnections.Add(44);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cell, 1, 2, 3, 1.0, 100);
            cell.DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cell, 44, 24, 4, 1.0, 100);
            cell.DistalDendrites.Add(distSeg2);

            Cell preSynapticcell = new Cell(11, 14, 16, new CellActivity());

            var synapse1 = new Synapse(cell, distSeg1.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse2 = new Synapse(cell, distSeg2.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);

            pool.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            pool.m_SynapsesBySourceIndex.Add(3, synapse1);
            pool.m_SynapsesBySourceIndex.Add(67, synapse2);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializePoolTest)}.txt"))
            {
                pool.Serialize(sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializePoolTest)}.txt"))
            {
                Pool pool1 = Pool.Deserialize(sr);

                Assert.IsTrue(pool1.Equals(pool));
            }
        }
        ///<summary>
        ///Test ProximalDendrite.
        ///</summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(0, 12.34, 23)]
        public void SerializeProximalDendriteTest(int colIndx, double synapsePermConnected, int numInputs)
        {
            ProximalDendrite proximal = new ProximalDendrite(colIndx, synapsePermConnected, numInputs);
            var rfPool = new Pool(1, 28);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);
            cell.DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cell, 44, 24, 34, 1.0, 100);
            cell.DistalDendrites.Add(distSeg2);

            Cell preSynapticcell = new Cell(11, 14, 16, new CellActivity());

            var synapse1 = new Synapse(cell, distSeg1.SegmentIndex, 23, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse1);

            var synapse2 = new Synapse(cell, distSeg2.SegmentIndex, 27, 1.0);
            preSynapticcell.ReceptorSynapses.Add(synapse2);

            rfPool.m_SynapsesBySourceIndex = new Dictionary<int, Synapse>();
            rfPool.m_SynapsesBySourceIndex.Add(3, synapse1);
            rfPool.m_SynapsesBySourceIndex.Add(67, synapse2);

            proximal.RFPool = rfPool;

            //HtmSerializer2 htm = new HtmSerializer2();
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeProximalDendriteTest)}.txt"))
            {
                proximal.Serialize(sw);
            }
            //htm.indent($"ser_{nameof(SerializeProximalDendriteTest)}.txt");
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeProximalDendriteTest)}.txt"))
            {
                ProximalDendrite proximal1 = ProximalDendrite.Deserialize(sr);

                Assert.IsTrue(proximal.Equals(proximal1));
            }
        }
        ///<summary>
        ///Test HtmModuleTopology.
        ///</summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(new int[] { 3, 4, 5 }, true)]
        [DataRow(new int[] { 43, 24, 85 }, false)]
        [DataRow(new int[] { 15, 74, 25 }, true)]
        public void SerializeHtmModuleTopologyTest(int[] dimensions, bool isMajorOrdering)
        {
            HtmModuleTopology htm = new HtmModuleTopology(dimensions, isMajorOrdering);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeHtmModuleTopologyTest)}.txt"))
            {
                htm.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeHtmModuleTopologyTest)}.txt"))
            {
                HtmModuleTopology htm1 = HtmModuleTopology.Deserialize(sr);

                Assert.IsTrue(htm1.Equals(htm));
            }
        }

        /// <summary>
        /// Test integer value.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(1000)]
        [DataRow(-1200010)]
        [DataRow(int.MaxValue)]
        [DataRow(-int.MaxValue)]
        public void SerializeIntegerTest(int val)
        {
            Integer inte = new Integer(val);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeIntegerTest)}.txt"))
            {
                inte.Serialize(sw);

            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeIntegerTest)}.txt"))
            {
                Integer inte1 = Integer.Deserialize(sr);

                Assert.IsTrue(inte1.Equals(inte));

            }
        }
        /// <summary>
        /// Test SegmentActivity.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeSegmentActivityTest()
        {
            SegmentActivity segment = new SegmentActivity();

            segment.ActiveSynapses = new Dictionary<int, int>();
            segment.ActiveSynapses.Add(23, 1);
            segment.ActiveSynapses.Add(24, 2);
            segment.ActiveSynapses.Add(35, 3);
            segment.PotentialSynapses = new Dictionary<int, int>();
            segment.PotentialSynapses.Add(2, 56);
            segment.PotentialSynapses.Add(22, 6);
            segment.PotentialSynapses.Add(24, 26);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSegmentActivityTest)}.txt"))
            {
                segment.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeSegmentActivityTest)}.txt"))

            {
                SegmentActivity segment1 = SegmentActivity.Deserialize(sr);

                Assert.IsTrue(segment1.Equals(segment));
            }
        }

        #region Serialization SparseObjectMatrix<T>
        [TestMethod]
        [DataRow(1024)]
        public void SerializeSparseObjectMatrix(int num)
        {
            // Create SparseObjectMatrix
            // either by dicrect creation or running experiment
            int[] dimensions = { 10, 10 };

            //IDistributedDictionary<int, int[]> dict = new();

            SparseObjectMatrix<int[]> matrix = new(dimensions, false);
            for (int i = 0; i < num; i+=10)
            {
                matrix.set(i, new int[] { 1, 2, 3, 4, 5 });
            }
          
            // Serialize 
            using (StreamWriter sw = new StreamWriter("ser.txt"))
            {
                matrix.Serialize(sw);
            }
            // Deserizlize
            SparseObjectMatrix<int[]> matrixNew = new();
            using (StreamReader sr = new StreamReader("ser.txt"))
            {
                matrixNew = SparseObjectMatrix<int[]>.Deserialize(sr);

                HtmSerializer.IsEqual(matrix, matrixNew);
            }
        }
        #endregion

        [TestMethod]
        public void SerializeInMemoryDistributedDictionary()
        {
            InMemoryDistributedDictionary<int, int> numNodes = new InMemoryDistributedDictionary<int, int>(3);
            // There are no Serialize of Dictionary in InMemoryDistributedDictionary
            numNodes.Add(145, 29);
            numNodes.Add(123, 26);
            numNodes.Add(531, 26);
            numNodes.Add(1536, 26);
            numNodes.Add(1529, 26);
            // Serialize 
            using (StreamWriter sw = new StreamWriter("InMem.txt"))
            {
                numNodes.Serialize(sw);
            }
            // Deserizlize
            InMemoryDistributedDictionary<int, int> newTest = new InMemoryDistributedDictionary<int, int>();
            using (StreamReader sr = new StreamReader("InMem.txt"))
            {
                newTest = InMemoryDistributedDictionary<int, int>.Deserialize(sr);

                HtmSerializer.IsEqual(numNodes, newTest);
            }
        }

        [TestMethod]
        public void SerializeSparseBinaryMatrix()
        {
            // Create SParse BinarySparseMatrix
            // either by dicrect creation or running experiment
            int[] dimensions = { 100, 100 };

            //IDistributedDictionary<int, int[]> dict = new();

            SparseBinaryMatrix binaryMatrix = new(dimensions, false);

            // Serialize 
            using (StreamWriter sw = new StreamWriter("Binary.txt"))
            {
                binaryMatrix.Serialize(sw);
            }
            // Deserizlize
            SparseBinaryMatrix newBinary = new();
            using (StreamReader sr = new StreamReader("Binary.txt"))
            {
                newBinary = SparseBinaryMatrix.Deserialize(sr);

                HtmSerializer.IsEqual(binaryMatrix, newBinary);
            }
        }

        [Obsolete(" We have moved this method to HtmSerializer2.")]
        internal static bool IsEqual(object obj1, object obj2)
        {
            return false;
        }
    }
}
