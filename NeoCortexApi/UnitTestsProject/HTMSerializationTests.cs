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
        public void SerializeArrayInt()
        {
            HtmSerializer2 htm = new HtmSerializer2();
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
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
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
            HtmSerializer2 htm = new HtmSerializer2();
            Cell[] cells = new Cell[2];
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeArrayCell)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                cells[0] = new Cell(1, 1, 1, 1, new CellActivity());

                cells[0].DistalDendrites = new List<DistalDendrite>();

                cells[0].DistalDendrites.Add(new DistalDendrite(cells[0], 1, 2, 2, 1.0, 100));
                cells[0].DistalDendrites.Add(new DistalDendrite(cells[0], 44, 24, 34, 1.0, 100));

                cells[0].ReceptorSynapses = new List<Synapse>();

                cells[0].ReceptorSynapses.Add(new Synapse(cells[0], 1, 23, 1.0));
                cells[0].ReceptorSynapses.Add(new Synapse(cells[0], 3, 27, 1.0));

                cells[1] = new Cell(1, 1, 1, 3, new CellActivity());

                cells[1].DistalDendrites = new List<DistalDendrite>();

                cells[1].DistalDendrites.Add(new DistalDendrite(cells[1], 1, 3, 5, 1.0, 100));
                cells[1].DistalDendrites.Add(new DistalDendrite(cells[1], 4, 24, 3, 1.0, 100));

                cells[1].ReceptorSynapses = new List<Synapse>();

                cells[1].ReceptorSynapses.Add(new Synapse(cells[1], 21, 23, 1.0));
                cells[1].ReceptorSynapses.Add(new Synapse(cells[1], 3, 7, 1.0));

                htm.SerializeValue(cells, sw);

                htm.SerializeEnd("UnitTest", sw);
            }
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDictionaryStringint()
        {
            HtmSerializer2 htm = new HtmSerializer2();
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
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
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
            HtmSerializer2 htm = new HtmSerializer2();
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
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
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
            HtmSerializer2 htm = new HtmSerializer2();
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
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
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
        //[DataRow(1, 1, 1, 1)]
        [DataRow(1111, 22221, 11111, 2221)]
        //[DataRow(1211212121, 2121212121, 212121211, 3232132131)]
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
                //Cell cell1 = Cell.Deserialize(sr);

                //Assert.IsTrue(cell1.Equals(cell));
            }
        }

        ///<summary>
        ///Test List of DistalDendrite
        ///</summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDistalDendriteList()
        {
            HtmSerializer2 htm = new HtmSerializer2();
            Cell cell = new Cell(1, 1, 1, 1, new CellActivity());
            List<DistalDendrite> distals = new List<DistalDendrite>();
            distals.Add(new DistalDendrite(cell, 1, 2, 2, 1.0, 100));
            distals.Add(new DistalDendrite(cell, 44, 24, 34, 1.0, 100));
            List<DistalDendrite> distals2 = new List<DistalDendrite>();
            distals2.Add(new DistalDendrite(cell, 1, 2, 2, 1.0, 100));
            distals2.Add(new DistalDendrite(cell, 44, 24, 34, 1.0, 100));
            Assert.IsTrue(distals.SequenceEqual(distals2));
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeDistalDendriteList)}"))
            {
                htm.SerializeBegin("UnitTest", sw);
                htm.SerializeValue(distals, sw);
                htm.SerializeEnd("UnitTest", sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeDistalDendriteList)}"))
            {
                List<DistalDendrite> distals1 = new List<DistalDendrite>();
                while(sr.Peek() > 0)
                {
                    string str = sr.ReadLine();
                    if (str == htm.ReadBegin(nameof(DistalDendrite)))
                    {
                        distals1.Add(DistalDendrite.Deserialize(sr));
                    }
                }
                
            }
        }
        /// <summary>
        /// Test DistalDendrite.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeDistalDendrite()
        {
            Cell cell = new Cell(1, 1, 1, 1, new CellActivity());
            DistalDendrite distal = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);
            //distal.Synapses.Add(new Synapse(cell, 1, 23, 1.0));
            //distal.Synapses.Add(new Synapse(cell, 3, 27, 1.0));
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeDistalDendrite)}.txt"))
            {
                distal.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeDistalDendrite)}.txt"))
            {
                DistalDendrite distal1 = DistalDendrite.Deserialize(sr);

                Assert.IsTrue(distal1.Equals(distal));
            }
        }

        /// <summary>
        /// Test List of Synapse.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        public void SerializeSynapseList()
        {
            HtmSerializer2 htm = new HtmSerializer2();
            Cell cells = new Cell(1, 1, 1, 1, new CellActivity());
            List<Synapse> synapses = new List<Synapse>();
            synapses.Add(new Synapse(cells, 1, 23, 2.0));
            synapses.Add(new Synapse(cells, 3, 27, 1.0));
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSynapseList)}.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                //htm.SerializeValue(synapses, sw);

                htm.SerializeEnd("UnitTest", sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeSynapseList)}.txt"))
            {
                List<Synapse> synapses1 = new List<Synapse>();
                while(sr.Peek() > 0)
                {
                    string data = sr.ReadLine();
                    if (data == String.Empty || data == htm.ReadBegin("UnitTest") )
                    {
                        continue;
                    }
                    else if (data == htm.ReadBegin(nameof(Synapse)))
                    {
                        //synapses1.Add(Synapse.Deserialize(sr));
                    }
                }
                Assert.IsTrue(synapses.SequenceEqual(synapses1));
            }
        }

        /// <summary>
        /// Test Synapse.
        /// </summary>
        [TestMethod]
        [TestCategory("Serialization")]
        [DataRow(34, 24, 24.65)]
        //[DataRow(13, 87, 22.45)]
        //[DataRow(1000, 3400, 4573.623)]

        public void SerializeSynapseTest(int segmentindex,int synapseindex,double permanence)
        {
            Cell cell = new Cell(12, 14, 16, 18, new CellActivity());
            //Cell cell = new Cell();
            cell.DistalDendrites = new List<DistalDendrite>();

            cell.DistalDendrites.Add(new DistalDendrite(cell, 1, 2, 2, 1.0, 100));
            cell.DistalDendrites.Add(new DistalDendrite(cell, 44, 24, 34, 1.0, 100));

            cell.ReceptorSynapses = new List<Synapse>();

            cell.ReceptorSynapses.Add(new Synapse(cell, 1, 23, 1.0));
            cell.ReceptorSynapses.Add(new Synapse(cell, 3, 27, 1.0));
            Synapse synapse = new Synapse(cell, segmentindex, synapseindex, permanence);
            Synapse synapse1 = null;
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSynapseTest)}_{synapseindex}.txt"))
            {
                synapse.Serialize(sw);

                using (StreamWriter streamWriter = new StreamWriter($"ser_{nameof(SerializeCellTest)}_{cell.Index}.txt") )
                {
                    cell.Serialize(streamWriter);
                    for(int j = 0; j < cell.DistalDendrites.Count; j++)
                    {
                        if(!File.Exists($"Users / mouni.kolisetty / neocortexapi / NeoCortexApi / UnitTestsProject / bin / Debug / net5.0 / ser_SerializeSynapseTest_{cell.DistalDendrites[j].Ordinal}.txt"))
                            using (StreamWriter swLD = new StreamWriter($"ser_{nameof(SerializeDistalDendrite)}_{cell.DistalDendrites[j].Ordinal}.txt"))
                            {
                                cell.DistalDendrites[j].Serialize(swLD);
                            }
                    }
                    for (int i = 0; i < cell.ReceptorSynapses.Count; i++)
                    {
                        if (!File.Exists($"Users / mouni.kolisetty / neocortexapi / NeoCortexApi / UnitTestsProject / bin / Debug / net5.0 / ser_SerializeSynapseTest_{cell.ReceptorSynapses[i].SynapseIndex}.txt"))
                            using (StreamWriter swLS = new StreamWriter($"ser_{nameof(SerializeSynapseTest)}_{cell.ReceptorSynapses[i].SynapseIndex}.txt"))
                            {
                                cell.ReceptorSynapses[i].Serialize(swLS);
                            }
                    }
                }
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeSynapseTest)}_{synapseindex}.txt"))
            {
                 synapse1 = Synapse.Deserialize(sr);
            }
            using (StreamReader streamreader = new StreamReader($"ser_SerializeCellTest_{synapse1.SourceCell.Index}.txt"))
            {
                var mvcell = Cell.Deserialize(streamreader);
                synapse1.SourceCell = mvcell;
                foreach(Synapse s in synapse1.SourceCell.ReceptorSynapses)
                {
                    s.SourceCell = mvcell;
                }
                foreach (DistalDendrite dd in synapse1.SourceCell.DistalDendrites)
                {
                    dd.ParentCell = mvcell;
                }

            }
            
            Assert.IsTrue(synapse1.SourceCell.ReceptorSynapses.SequenceEqual(synapse.SourceCell.ReceptorSynapses));
            //using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeSynapseTest)}.txt"))
            //{
            //    Synapse synapse1 = Synapse.Deserialize(sr);

            //    Assert.IsTrue(synapse1.Equals(synapse));
            //}
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
