using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Types;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    public partial class HTMSerializationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("working")]
        public void Test()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Cell[] cells = new Cell[2];
            cells[0] = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cells[0], 1, 2, 2, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cells[0], 44, 24, 34, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg2);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test)}_123.txt"))
            {
                HtmSerializer2.Serialize(cells, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test)}_123.txt"))
            {
                var c = HtmSerializer2.Deserialize<Cell[]>(sr);
            }
        }

        [TestMethod]
        public void Test1()
        {
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test1)}_123.txt"))
            {
                HtmSerializer2.Serialize(new List<string> { "bla" }, null, sw);
            }

        }
        [TestMethod]
        [TestCategory("working")]
        public void Test2()
        {
            var dict = new Dictionary<string, Cell>();

            var cell1 = new Cell(12, 14, 16, new CellActivity());
            var distSeg1 = new DistalDendrite(cell1, 1, 2, 2, 1.0, 100);
            var distSeg2 = new DistalDendrite(cell1, 2, 2, 12, 1.0, 100);
            cell1.DistalDendrites.Add(distSeg1);
            cell1.DistalDendrites.Add(distSeg2);
            dict.Add("1", cell1);

            var cell2 = new Cell(12, 14, 16, new CellActivity());
            var distSeg3 = new DistalDendrite(cell2, 44, 24, 34, 1.0, 102);
            cell2.DistalDendrites.Add(distSeg3);
            dict.Add("2", cell2);


            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test2)}_123.txt"))
            {
                HtmSerializer2.Serialize(dict, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test2)}_123.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test2)}_123.txt"))
            {
                var dict1 = HtmSerializer2.Deserialize<Dictionary<string, Cell>>(sr);

                foreach (var key in dict.Keys)
                {
                    dict1.TryGetValue(key, out Cell cell);
                    Assert.IsTrue(dict[key].Equals(cell));
                }
            }

        }

        [TestMethod]
        [TestCategory("working")]
        public void Test3()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            var cell1 = new Cell(12, 14, 16, new CellActivity());
            var cell2 = new Cell(1, 1, 1, new CellActivity());

            DistalDendrite[] dd = new DistalDendrite[2];

            dd[0] = new DistalDendrite(cell1, 1, 2, 2, 1.0, 100);

            dd[1] = new DistalDendrite(cell2, 44, 24, 34, 1.0, 100);

            cell1.DistalDendrites.Add(dd[0]);
            cell2.DistalDendrites.Add(dd[1]);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test)}_123.txt"))
            {
                HtmSerializer2.Serialize(dd, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test)}_123.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test)}_123.txt"))
            {
                var d = HtmSerializer2.Deserialize<DistalDendrite[]>(sr);

                for (int i = 0; i < dd.Length; i++)
                {
                    Assert.IsTrue(dd[i].Equals(d[i]));
                }
            }
        }

        [TestMethod]
        public void Test3_1()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            DistalDendrite[] dd = new DistalDendrite[2];
            dd[0] = new DistalDendrite(null, 1, 2, 2, 1.0, 100);

            dd[1] = new DistalDendrite(null, 44, 24, 34, 1.0, 100);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test)}_123.txt"))
            {
                HtmSerializer2.Serialize1(dd, null, sw, new Dictionary<Type, Action<StreamWriter, string, object>>
                {
                    {
                        typeof(DistalDendrite), (sw, name, obj) => DistalDendrite.Serialize1(sw, obj, null)
                    }
                });
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test)}_123.txt"))
            {
                var d = HtmSerializer2.Deserialize<DistalDendrite[]>(sr);

                for (int i = 0; i < dd.Length; i++)
                {
                    Assert.IsTrue(dd[i].Equals(d[i]));
                }
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void Test4()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Dictionary<string, Bla> dict = new Dictionary<string, Bla>
            {
                {"1", new Bla{ Id = 1, Name = "real", In = new List<Internal>() } },
                {"2", new Bla{ Id = 21, Name = "real1", In = new List<Internal>{ new Internal{ Risk = 0.1f } } } },
            };

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test4)}_123.txt"))
            {
                HtmSerializer2.Serialize(dict, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test4)}_123.txt"))
            {
                var d = HtmSerializer2.Deserialize<Dictionary<string, Bla>>(sr);
            }
        }

        private class Bla
        {
            public string Name { get; set; }
            public int Id { get; set; }

            public List<Internal> In { get; set; }
        }

        private class Internal
        {
            public float Risk { get; set; }
        }

        [TestMethod]
        [TestCategory("working")]
        public void DeserializationArrayTest()
        {
            var array = new int[] { 45, 35 };

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(array, null, sw);
            }
            var reader = new StreamReader($"{TestContext.TestName}.txt");

            var res = HtmSerializer2.Deserialize<int[]>(reader);

            Assert.IsTrue(array.SequenceEqual(res));
        }

        [TestMethod]
        [TestCategory("working")]
        public void DeserializeIEnumerableTest()
        {
            var array = new List<int> { 45, 34 };

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(array, null, sw);
            }
            var reader = new StreamReader($"{TestContext.TestName}.txt");

            var res = HtmSerializer2.Deserialize<List<int>>(reader);

            Assert.IsTrue(array.SequenceEqual(res));
        }

        [TestMethod]
        [TestCategory("working")]
        public void DeserializeIEnumerable1Test()
        {
            var array = new List<int> { 45, 34 };

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(array, null, sw);
            }
            var reader = new StreamReader($"{TestContext.TestName}.txt");

            var res = HtmSerializer2.Deserialize<int[]>(reader);

            Assert.IsTrue(array.SequenceEqual(res));
        }

        [TestMethod]
        [TestCategory("working")]
        public void DeserializeHtmConfigTest()
        {
            int cellsPerColumnL4 = 20;
            int numColumnsL4 = 500;
            int cellsPerColumnL2 = 20;
            int numColumnsL2 = 500;
            int inputBits = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            double max = 20;
            int inputsL2 = numColumnsL4 * cellsPerColumnL4;
            HtmConfig htmConfig_L2 = new HtmConfig(new int[] { inputsL2 }, new int[] { numColumnsL2 })
            {
                Random = new NeoCortexApi.ThreadSafeRandom(42),

                CellsPerColumn = cellsPerColumnL2,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.1 * numColumnsL2,
                PotentialRadius = inputsL2, // Every columns 
                //InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.05 * numColumnsL2),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(htmConfig_L2, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var htmConfig = HtmSerializer2.Deserialize<HtmConfig>(sr);

                Assert.IsTrue(htmConfig_L2.Equals(htmConfig));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void DeserializeConnectionsTest()
        {
            int cellsPerColumnL4 = 20;
            int numColumnsL4 = 500;
            int cellsPerColumnL2 = 20;
            int numColumnsL2 = 500;
            int inputBits = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            double max = 20;
            int inputsL2 = numColumnsL4 * cellsPerColumnL4;
            HtmConfig htmConfig_L2 = new HtmConfig(new int[] { inputsL2 }, new int[] { numColumnsL2 })
            {
                Random = new NeoCortexApi.ThreadSafeRandom(42),

                CellsPerColumn = cellsPerColumnL2,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.1 * numColumnsL2,
                PotentialRadius = inputsL2, // Every columns 
                //InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.05 * numColumnsL2),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            var mem = new Connections(htmConfig_L2);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(mem, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var connection = HtmSerializer2.Deserialize<Connections>(sr);
                Assert.IsTrue(mem.Equals(connection));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void DeserializeHomeostaticPlasticityTest()
        {
            int cellsPerColumnL4 = 20;
            int numColumnsL4 = 500;
            int cellsPerColumnL2 = 20;
            int numColumnsL2 = 500;
            int inputBits = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            double max = 20;
            int inputsL2 = numColumnsL4 * cellsPerColumnL4;
            HtmConfig htmConfig_L2 = new HtmConfig(new int[] { inputsL2 }, new int[] { numColumnsL2 })
            {
                Random = new NeoCortexApi.ThreadSafeRandom(42),

                CellsPerColumn = cellsPerColumnL2,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.1 * numColumnsL2,
                PotentialRadius = inputsL2, // Every columns 
                //InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.05 * numColumnsL2),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            var numInputs = 8;

            var memL2 = new Connections(htmConfig_L2);

            HomeostaticPlasticityController hpa_sp_L2 = new HomeostaticPlasticityController(memL2, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {

            }, numOfCyclesToWaitOnChange: 50);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(hpa_sp_L2, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var hpa = HtmSerializer2.Deserialize<HomeostaticPlasticityController>(sr);

                hpa.OnStabilityStatusChanged = (isStable, numPatterns, actColAvg, seenInputs) =>
                {

                };
                Assert.IsTrue(hpa_sp_L2.Equals(hpa));
            }

        }

        [TestMethod]
        [TestCategory("working")]
        public void ConnectionInitSparseObjectMatrixTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);

            SpatialPooler sp = new SpatialPoolerMT();
            sp.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(mem.HtmConfig.Memory, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var memory = HtmSerializer2.Deserialize<SparseObjectMatrix<Column>>(sr);
                Assert.IsTrue((mem.HtmConfig.Memory as SparseObjectMatrix<Column>).Equals(memory));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void ConnectionInitHtmConfigTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);

            SpatialPooler sp = new SpatialPoolerMT();
            sp.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                //HtmSerializer2.Serialize(mem, null, sw);
                HtmSerializer2.Serialize(mem.HtmConfig, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var htmConfig1 = HtmSerializer2.Deserialize<HtmConfig>(sr);
                Assert.IsTrue(mem.HtmConfig.Equals(htmConfig1));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void ConnectionInitSPTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);

            SpatialPooler sp = new SpatialPoolerMT();
            sp.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(mem, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var connections = HtmSerializer2.Deserialize<Connections>(sr);
                Assert.IsTrue(mem.Equals(connections));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void SPTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);
            var numInputs = 8;
            HomeostaticPlasticityController hpa_sp_L2 = new HomeostaticPlasticityController(mem, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {

            }, numOfCyclesToWaitOnChange: 50);
            SpatialPooler sp = new SpatialPooler(hpa_sp_L2);
            sp.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                //HtmSerializer2.Serialize(mem, null, sw);
                HtmSerializer2.Serialize(sp, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                //var connection = HtmSerializer2.Deserialize<Connections>(sr);
                var sp1 = HtmSerializer2.Deserialize<SpatialPooler>(sr);
                Assert.IsTrue(sp.Equals(sp1));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void SPMTTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);
            var numInputs = 8;
            HomeostaticPlasticityController hpa_sp_L2 = new HomeostaticPlasticityController(mem, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {

            }, numOfCyclesToWaitOnChange: 50);
            SpatialPoolerMT sp = new SpatialPoolerMT(hpa_sp_L2);
            sp.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                //HtmSerializer2.Serialize(mem, null, sw);
                HtmSerializer2.Serialize(sp, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                //var connection = HtmSerializer2.Deserialize<Connections>(sr);
                var sp1 = HtmSerializer2.Deserialize<SpatialPoolerMT>(sr);
                Assert.IsTrue(sp.Equals(sp1));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void ConnectionInitTMTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);

            TemporalMemory tm = new TemporalMemory();
            tm.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(mem, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var connections = HtmSerializer2.Deserialize<Connections>(sr);
                Assert.IsTrue(mem.Equals(connections));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void TMTest()
        {
            HtmConfig htmConfig = SetupHtmConfigParameters();
            Connections mem = new Connections(htmConfig);

            TemporalMemory tm = new TemporalMemory();
            tm.Init(mem);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(tm, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var tm1 = HtmSerializer2.Deserialize<TemporalMemory>(sr);
                Assert.IsTrue(tm.Equals(tm1));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void EncoderTest()
        {
            int inputBits = 100;
            double max = 20;
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            ScalarEncoder encoder = new ScalarEncoder(settings);
            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(encoder, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var scalarEncoder = HtmSerializer2.Deserialize<ScalarEncoder>(sr);
                Assert.IsTrue(encoder.Equals(scalarEncoder));
            }
        }

        [TestMethod]
        [TestCategory("working")]
        public void CortexLayerTest()
        {
            int inputBits = 100;
            double max = 20;
            int numColumns = 1024;
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            EncoderBase encoder = new ScalarEncoder(settings);
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
            };
            var mem = new Connections(cfg);
            bool isInStableState = false;

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            //TemporalMemory tm = new TemporalMemory();

            // For more information see following paper: https://www.scitepress.org/Papers/2021/103142/103142.pdf
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, 10 * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                {
                    // Event should be fired when entering the stable state.
                    //Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                }
                else
                {
                    // Ideal SP should never enter unstable state after stable state.
                    //Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                }

                // We are not learning in instable state.
                isInStableState = isStable;

                // Clear active and predictive cells.
                //tm.Reset(mem);
            }, numOfCyclesToWaitOnChange: 50);


            SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
            sp.Init(mem);
            //tm.Init(mem);

            // Please note that we do not add here TM in the layer.
            // This is omitted for practical reasons, because we first eneter the newborn-stage of the algorithm
            // In this stage we want that SP get boosted and see all elements before we start learning with TM.
            // All would also work fine with TM in layer, but it would work much slower.
            // So, to improve the speed of experiment, we first ommit the TM and then after the newborn-stage we add it to the layer.
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(layer1, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var cortexLayer = HtmSerializer2.Deserialize<CortexLayer<object, object>>(sr);
                Assert.IsTrue(layer1.Equals(cortexLayer));
            }
        }


        private HtmConfig SetupHtmConfigParameters()
        {
            var htmConfig = new HtmConfig(new int[] { 5 }, new int[] { 5 })
            {
                PotentialRadius = 5,
                PotentialPct = 0.5,
                GlobalInhibition = false,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 3.0,
                StimulusThreshold = 0.0,
                SynPermInactiveDec = 0.01,
                SynPermActiveInc = 0.1,
                SynPermConnected = 0.1,
                MinPctOverlapDutyCycles = 0.1,
                MinPctActiveDutyCycles = 0.1,
                DutyCyclePeriod = 10,
                MaxBoost = 10,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42),
            };

            return htmConfig;
        }

        [TestMethod]
        [DataRow(12)]
        public void TestThreadSafeRandom(int counter)
        {
            var rand1 = new ThreadSafeRandom(42);
            var rand2 = new ThreadSafeRandom(42, counter);

            for (int i = 0; i < counter; i++)
            {
                rand1.NextDouble();
            }

            Assert.AreEqual(rand2.Next(), rand1.Next());

        }


    }
}
