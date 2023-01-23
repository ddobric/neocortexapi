using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Net.Http.Headers;
using Naa = NeoCortexApi.NeuralAssociationAlgorithm;
using System.Diagnostics;

namespace UnitTestsProject
{
    [TestClass]
    public class NAATests
    {
        private HtmConfig GetHtmConfig(int inpBits, int columns)
        {
            var htmConfig = new HtmConfig(new int[] { inpBits }, new int[] { columns })
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
                ActivationThreshold = 10,
                MinThreshold= 6,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42),
            };

            return htmConfig;
        }

        [TestMethod]
        [TestCategory("Prod")]
        [TestCategory("NAA")]
        public void CreateAreaTest()
        {
            var cfg = GetHtmConfig(100, 1024);

            MinicolumnArea caX = new MinicolumnArea("X", cfg);

            Assert.IsTrue(1024 == caX.Columns.Count);

            Assert.IsTrue(caX.AllCells.Count == 1024 * cfg.CellsPerColumn);

        }


        [TestMethod]
        [TestCategory("NAA")]
        [TestCategory("Prod")]
        [DataRow(1024, 0.02)]
        [DataRow(2048, 0.02)]
        [DataRow(10000, 0.03)]
        [DataRow(10000, 0.05)]
        public void CreateCoCreateRandonSdrTest(int numCells, double sparsity)
        {
            var sdr1 = UnitTestHelpers.CreateRandomSdr(numCells, sparsity);

            Assert.IsTrue(sdr1.Length == (int)(numCells * sparsity));
        }

        /// <summary>
        /// Makes sure that only active cells are materialized inside the sparse area.
        /// </summary>
        /// <param name="numCellsInArea">Virtual total number of cells in the area.</param>
        /// <param name="numSdrCells">Size of SDR.</param>
        /// <param name="sparsity">Sparsity of SDR.</param>
        [TestMethod]
        [TestCategory("Prod")]
        [TestCategory("NAA")]
        [DataRow(1024, 100, 0.2)]
        public void InitActiveCellsTest(int numCellsInArea, int numSdrCells, double sparsity)
        {
            CorticalArea areaY = new CorticalArea("Y", numCellsInArea);

            areaY.ActiveCellsIndicies = UnitTestHelpers.CreateRandomSdr(numSdrCells, sparsity);

            Assert.IsTrue(areaY.ActiveCellsIndicies.Length == (int)(numSdrCells * sparsity));
            Assert.IsTrue(areaY.ActiveCells.Count == areaY.ActiveCellsIndicies.Length);
        }


        [TestMethod]
        [TestCategory("Prod")]
        [TestCategory("NAA")]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(300)]
        [DataRow(1000)]
        public void GetSegmentWithHighestPotentialTest(int numSegments)
        {
            Cell testNeuron = new Cell();
            Cell preSynapticNeuron = new Cell();

            List<Segment> segments = new List<Segment>();

            for (int i = 0; i < numSegments; i++)
            {
                ApicalDendrite segment = new ApicalDendrite(testNeuron, i, 0.5);

                for (int y = 0; y < i+1; y++)
                {
                    var synapse = new Synapse(preSynapticNeuron, segment.SegmentIndex, segment.Synapses.Count, 0.5);

                    segment.Synapses.Add(synapse);

                    preSynapticNeuron.ReceptorSynapses.Add(synapse);
                }

                segments.Add(segment);
            }

            var maxSeg = HtmCompute.GetSegmentWithHighesPotential(segments);

            Assert.AreEqual(numSegments-1, maxSeg.SegmentIndex);

            Assert.IsTrue(numSegments==maxSeg.Synapses.Count);
        }

        [TestMethod]
        [TestCategory("NAA")]
        public void CreateAreaTest2()
        {
            var cfg = GetHtmConfig(100, 1024);

            CorticalArea areaX = new CorticalArea("X", 1024);

            CorticalArea areaY = new CorticalArea("Y", 100);

            areaX.ActiveCellsIndicies = UnitTestHelpers.CreateRandomSdr(1024, 0.02);

            areaY.ActiveCellsIndicies = UnitTestHelpers.CreateRandomSdr(100, 0.02);

            Naa naa = new Naa(cfg, areaY);

            for (int i = 0; i < 100; i++)
            {
                naa.Compute(areaX, true);
                Debug.WriteLine(naa.TraceState());
            }
        }
    }
}
