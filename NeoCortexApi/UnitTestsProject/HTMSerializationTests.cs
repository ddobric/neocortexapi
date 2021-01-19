using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace UnitTestsProject
{
    [TestClass]
    public class HTMSerializationTests
    {
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeValueTest()
        {
            HtmSerializer2 htm = new HtmSerializer2();
            
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
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
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
            HtmSerializer2 htm = new HtmSerializer2();
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
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
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
        /// Test empty SpatialPooler.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeEmptySpatialPooler()
        {

            SpatialPooler spatial = new SpatialPooler();

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeEmptySpatialPooler)}.txt"))
            {
                spatial.Serialize(sw);
            }
            //using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeEmptySpatialPooler)}.txt"))

            //{
            //    SpatialPooler spatial1 = SpatialPooler.Deserialize(sr);

            //    Assert.IsTrue(spatial1.Equals(spatial));
            //}


        }

        /// <summary>
        /// Test empty HomeostaticPlasticityController.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeEmptyHomeostaticPlasticityController()
        {

            HomeostaticPlasticityController homeostatic = new HomeostaticPlasticityController();

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeEmptyHomeostaticPlasticityController)}.txt"))
            {
                homeostatic.Serialize(sw);
            }
            //using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeEmptyHomeostaticPlasticityController)}.txt"))

            //{
            //    SpatialPooler homeostatic1 = SpatialPooler.Deserialize(sr);

            //    Assert.IsTrue(homeostatic1.Equals(homeostatic));
            //}


        }

        /// <summary>
        /// Test empty DistalDendrite.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeEmptyDistalDendrite()
        {

            DistalDendrite distal = new DistalDendrite();

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeEmptyDistalDendrite)}.txt"))
            {
                distal.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeEmptyDistalDendrite)}.txt"))

            {
                DistalDendrite distal1 = DistalDendrite.Deserialize(sr);

                Assert.IsTrue(distal1.Equals(distal));
            }


        }

        
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeSegmentTest()
        {
            //TODO
            Cell cell = new Cell(12, 14, 16, 18, new CellActivity());

            Segment seg = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSegmentTest)}.txt"))
            {
                seg.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeSegmentTest)}.txt"))
            {
                //Segment segment1 = Segment.Deserialize(sr);

                // Assert.IsTrue(segment1.Equals(seg));
            }
        }

        /// <summary>
        /// Test empty SegmentActivity.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeSegmentActivityTest()
        {
            SegmentActivity segment = new SegmentActivity();

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

        /// <summary>
        /// Test Cell.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(1, 1, 1, 1)]
        [DataRow(1111, 22221, 11111, 2221)]
        [DataRow(1211212121, 2121212121, 212121211, 3232132131)]
        public void SerializeCellTest(int parentIndx, int colSeq, int cellsPerCol, int cellId)
        {
            Cell cell = new Cell(parentIndx, colSeq, cellsPerCol, cellId, new CellActivity());

            cell.DistalDendrites = new List<DistalDendrite>();

            cell.DistalDendrites.Add(new DistalDendrite(cell, 1, 2, 2, 1.0, 100));
            cell.DistalDendrites.Add(new DistalDendrite(cell, 44, 24, 34, 1.0, 100));

            cell.ReceptorSynapses = new List<Synapse>();

            cell.ReceptorSynapses.Add(new Synapse(cell, 1, 23, 1.0));
            cell.ReceptorSynapses.Add(new Synapse(cell, 3, 27, 1.0));

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeCellTest)}.txt"))
            {
                cell.Serialize(sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeCellTest)}.txt"))
            {
                Cell cell1 = Cell.Deserialize(sr);

                Assert.IsTrue(cell1.Equals(cell));
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

    }
}
