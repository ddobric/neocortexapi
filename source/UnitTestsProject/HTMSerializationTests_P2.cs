using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Types;
using NeoCortexApi.Utility;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    public partial class HTMSerializationTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            HtmSerializer2.Reset();
        }

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

                Assert.IsTrue(cells.Where(c => c != null).TryIfSequenceEqual(c));
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

            var cell2 = new Cell(12, 15, 16, new CellActivity());
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

            var content = reader.ReadToEnd();

            reader = new StreamReader($"{TestContext.TestName}.txt");

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
        //[TestCategory("working")]
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
                //Assert.IsTrue(hpa_sp_L2.Equals(hpa));
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
                HtmSerializer2.Serialize(mem.Memory, null, sw);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var content = sr.ReadToEnd();
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var memory = HtmSerializer2.Deserialize<SparseObjectMatrix<Column>>(sr);
                Assert.IsTrue((mem.Memory as SparseObjectMatrix<Column>).Equals(memory));
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
        //[TestCategory("working")]
        public void CortexLayerTest()
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();
            sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
            sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));

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

            TemporalMemory tm = new TemporalMemory();

            SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
            sp.Init(mem);
            tm.Init(mem);

            // Please note that we do not add here TM in the layer.
            // This is omitted for practical reasons, because we first eneter the newborn-stage of the algorithm
            // In this stage we want that SP get boosted and see all elements before we start learning with TM.
            // All would also work fine with TM in layer, but it would work much slower.
            // So, to improve the speed of experiment, we first ommit the TM and then after the newborn-stage we add it to the layer.
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            var lastPredictedValues = new List<string>(new string[] { "0" });

            int maxCycles = 3500;

            //
            // Training SP to get stable. New-born stage.
            //

            for (int i = 0; i < maxCycles && isInStableState == false; i++)
            {
                matches = 0;

                cycle++;

                //Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (var inputs in sequences)
                {
                    foreach (var input in inputs.Value)
                    {
                        //Debug.WriteLine($" -- {inputs.Key} - {input} --");

                        var lyrOut = layer1.Compute(input, true);

                        if (isInStableState)
                            break;
                    }

                    if (isInStableState)
                        break;
                }
            }
            int maxMatchCnt = 0;
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            cls.ClearState();

            // We activate here the Temporal Memory algorithm.
            layer1.HtmModules.Add("tm", tm);

            //
            // Loop over all sequences.
            foreach (var sequenceKeyPair in sequences)
            {
                Debug.WriteLine($"-------------- Sequences {sequenceKeyPair.Key} ---------------");

                int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1.0");

                //
                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    matches = 0;

                    cycle++;

                    Debug.WriteLine("");

                    Debug.WriteLine($"-------------- Cycle {cycle} ---------------");
                    Debug.WriteLine("");

                    foreach (var input in sequenceKeyPair.Value)
                    {
                        Debug.WriteLine($"-------------- {input} ---------------");

                        var lyrOut = layer1.Compute(input, true) as ComputeCycle;

                        var activeColumns = layer1.GetResult("sp") as int[];

                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > (maxPrevInputs + 1))
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input, sequenceKeyPair.Key);

                        List<Cell> actCells;

                        if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                        {
                            actCells = lyrOut.ActiveCells;
                        }
                        else
                        {
                            actCells = lyrOut.WinnerCells;
                        }

                        cls.Learn(key, actCells.ToArray());

                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        //
                        // If the list of predicted values from the previous step contains the currently presenting value,
                        // we have a match.
                        if (lastPredictedValues.Contains(key))
                        {
                            matches++;
                            Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValues.FirstOrDefault(key)}.");
                        }
                        else
                            Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted values: {String.Join(',', lastPredictedValues)}");

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {item.PredictedInput} - {item.Similarity}");
                            }

                            lastPredictedValues = predictedInputValues.Select(v => v.PredictedInput).ToList();
                        }
                        else
                        {
                            Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValues = new List<string>();
                        }
                    }

                    // The first element (a single element) in the sequence cannot be predicted
                    double maxPossibleAccuraccy = (double)((double)sequenceKeyPair.Value.Count - 1) / (double)sequenceKeyPair.Value.Count * 100.0;

                    double accuracy = (double)matches / (double)sequenceKeyPair.Value.Count * 100.0;

                    Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {sequenceKeyPair.Value.Count}\t {accuracy}%");

                    if (accuracy >= maxPossibleAccuraccy)
                    {
                        maxMatchCnt++;
                        Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");

                        //
                        // Experiment is completed if we are 30 cycles long at the 100% accuracy.
                        if (maxMatchCnt >= 30)
                        {
                            sw.Stop();
                            Debug.WriteLine($"Sequence learned. The algorithm is in the stable state after 30 repeats with with accuracy {accuracy} of maximum possible {maxMatchCnt}. Elapsed sequence {sequenceKeyPair.Key} learning time: {sw.Elapsed}.");
                            break;
                        }
                    }
                    else if (maxMatchCnt > 0)
                    {
                        Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with accuracy {accuracy}. This indicates instable state. Learning will be continued.");
                        maxMatchCnt = 0;
                    }

                    // This resets the learned state, so the first element starts allways from the beginning.
                    tm.Reset(mem);
                }
            }

            Debug.WriteLine("------------ END ------------");

            using (var swrt = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(layer1, null, swrt);
            }
            using (var sr = new StreamReader($"{TestContext.TestName}.txt"))
            {
                var cortexLayer = HtmSerializer2.Deserialize<CortexLayer<object, object>>(sr);
                Assert.IsTrue(layer1.Equals(cortexLayer));
            }
        }
        private static string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return $"{sequence}_{key}";
        }

        [TestMethod]
        public void ActiveColumnsTest()
        {
            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represent an input vector (pattern).
            int inputBits = 100;

            // We will build a slice of the cortex with the given number of mini-columns
            int numColumns = 1024;

            //
            // This is a set of configuration parameters used in the experiment.
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,

                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10,
            };

            double max = 50;

            //
            // This dictionary defines a set of typical encoder parameters.
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

            //
            // We create here 100 random input values.
            List<double> inputValues = new List<double>();

            var rand = new Random();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            List<double> inputTrainValues = new List<double>();

            for (int i = 0; i < (int)(max * 0.2); i++)
            {
                var value = rand.Next(0, (int)max);
                while (inputTrainValues.Contains(value))
                {
                    value = rand.Next(0, (int)max);
                }

                inputTrainValues.Add(value);
            }

            var outs = new List<int[]>();
            foreach (var input in inputValues)
            {
                var output = encoder.Encode(input);
                outs.Add(output);
            }


            // Creates the htm memory.
            var mem = new Connections(cfg);

            bool isInStableState = false;

            //
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPoolerMT sp = new SpatialPoolerMT(hpa);

            // Initializes the 
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // mem.TraceProximalDendritePotential(true);

            // It creates the instance of the neo-cortex layer.
            // Algorithm will be performed inside of that layer.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Add encoder as the very first module. This model is connected to the sensory input cells
            // that receive the input. Encoder will receive the input and forward the encoded signal
            // to the next module.
            cortexLayer.HtmModules.Add("encoder", encoder);

            // The next module in the layer is Spatial Pooler. This module will receive the output of the
            // encoder.
            cortexLayer.HtmModules.Add("sp", sp);

            double[] inputs = inputTrainValues.ToArray();

            // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            //
            // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 3500;

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");
                if (isInStableState)
                    break;
                //
                // This trains the layer on input pattern.
                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    // 
                    var lyrOut = cortexLayer.Compute((object)input, true) as int[];

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    //Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }
            }

            using (var swrt = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(cortexLayer, null, swrt);
            }

            using var sr = new StreamReader($"{TestContext.TestName}.txt");

            var cortexLayer1 = HtmSerializer2.Deserialize<CortexLayer<object, object>>(sr);
            Assert.IsTrue(cortexLayer.Equals(cortexLayer1));

            var testInputs = inputValues.Except(inputTrainValues).ToList();

            foreach (var input in inputTrainValues)
            {
                var expectedResult = (int[])cortexLayer.Compute(input, false);
                var result = (int[])cortexLayer1.Compute(input, false);
                Assert.IsTrue(expectedResult.SequenceEqual(result));
            }

            foreach (var input in testInputs)
            {
                var expectedResult = (int[])cortexLayer.Compute(input, false);
                var result = (int[])cortexLayer1.Compute(input, false);
                Assert.IsTrue(expectedResult.SequenceEqual(result));
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

            Assert.IsTrue(rand2.Equals(rand1));

        }
    }
}
