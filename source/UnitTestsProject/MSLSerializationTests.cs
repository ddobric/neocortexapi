using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi.Types;
using NeoCortexApi.Utility;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Spatial;
using System.Runtime.CompilerServices;

namespace UnitTestsProject
{
    [TestClass]
    public partial class MSLSerializationTests
    {
        public TestContext TestContext { get; set; }

        private string fileName;

        // We will use 100 bits to represent an input vector( pattern).
        private int inputBits = 100;
        // We will build a slice of the cortex with the given number of mini-columns.
        private int numColumns = 1024;

        private int cellsPerColumn = 25;

        private HtmClassifier<string, ComputeCycle> htmClassifier;

        private Connections mem;

        private ScalarEncoder encoder;

        private SpatialPoolerMT sp;

        private TemporalMemory tm;

        private Predictor predictor;

        private CortexLayer<object, object> layer;
        
        private Dictionary<string, List<double>> sequences;
        
        private List<Cell> lastActiveCells = new List<Cell>();

        private double max = 20;




        [TestInitialize]
        public void TestInit()
        {
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
            // Initialize instance of class Connection
            mem = new Connections(cfg);
            // Initialize instance of class ScalarEncoder with the pre-defined setting values. 
            encoder = new ScalarEncoder(settings);

            sp = new SpatialPoolerMT();
            tm = new TemporalMemory();

            sp.Init(mem);

            tm.Init(mem);

            // Initialize the CortexLayer
            layer = new CortexLayer<object, object>("L");

            // Add SP and TM objects to CortexLayer, initialize the values (null) 
            layer.HtmModules.Add("encoder", encoder);
            layer.HtmModules.Add("sp", sp);
            layer.HtmModules.Add("tm", tm);

            // Create new instance of class HtmClassifier.
            // HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

             htmClassifier= new HtmClassifier<string, ComputeCycle>();

            sequences = new Dictionary<string, List<double>>();
            sequences.Add("S1", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, }));
            sequences.Add("S2", new List<double>(new double[] { 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0 }));
            LearnHtmClassifier();

            predictor = new Predictor(null, null, null);

            predictor.layer = layer;
            predictor.connections = mem;
            predictor.classifier = htmClassifier;

            fileName = $"{TestContext.TestName}.txt";
            HtmSerializer.Reset();
        }
        [TestCleanup]

        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void SerializePredictorTest()
        {

            using (var sw = new StreamWriter(fileName))
            {
                predictor.Serialize(predictor, null, sw);
                sw.Close();
            }
            using (var sr = new StreamReader(fileName))
            {
                var PredictExp = Predictor.Deserialize<Predictor>(sr, null);
                sr.Close();
                if (PredictExp is Predictor predictor1)
                {
                    var mem1 = predictor1.connections;
                    var layer1 = predictor1.layer;
                    var tm1 = (TemporalMemory)layer1.HtmModules["tm"];
                    var sp1 = (SpatialPooler)layer1.HtmModules["sp"];
                    var htmClassifier1 = predictor1.classifier;
                    
                    Assert.IsTrue(predictor.connections.Equals(mem1));
                    Assert.IsTrue(tm.Equals(tm1));
                    Assert.IsTrue(sp.Equals(sp1));
                    Assert.IsTrue(htmClassifier.Equals(htmClassifier1));
                }

            }
        }

        private void LearnHtmClassifier()
        {
            int maxCycles = 100;

            foreach (var sequenceKeyPair in sequences)
            {
                int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1.0");

                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    foreach (var input in sequenceKeyPair.Value)
                    {
                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > maxPrevInputs + 1)
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the same SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input, sequenceKeyPair.Key);
                        List<Cell> actCells = getMockCells(CellActivity.ActiveCell);
                        htmClassifier.Learn(key, actCells.ToArray());
                    }
                }
            }
        }

        private string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = string.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += prevInputs[i];
            }

            return $"{sequence}_{key}";
        }

        /// <summary>
        /// Mock the cells data that we get from the Temporal Memory
        /// </summary>
        private List<Cell> getMockCells(CellActivity cellActivity)
        {
            List<Cell> cells = new List<Cell>();
            for (int k = 0; k < Random.Shared.Next(5, 20); k++)
            {
                int parentColumnIndx = Random.Shared.Next(0, numColumns);
                int numCellsPerColumn = Random.Shared.Next(0, cellsPerColumn);
                int colSeq = Random.Shared.Next(0, cellsPerColumn);

                cells.Add(new Cell(parentColumnIndx, colSeq, numCellsPerColumn, cellActivity));
            }

            if (cellActivity == CellActivity.ActiveCell)
            {
                lastActiveCells = cells;
            }
            else if (cellActivity == CellActivity.PredictiveCell)
            {
                // Append one of the cell from lastActiveCells to the randomly generated preditive cells to have some similarity
                cells.AddRange(lastActiveCells.GetRange
                    (
                        Random.Shared.Next(lastActiveCells.Count), 1
                    )
                );
            }

            return cells;
        }
    }
}
