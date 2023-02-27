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
using GemBox.Spreadsheet.Drawing;

namespace UnitTestsProject
{
    [TestClass]
    public class NAATests
    {
        [TestMethod]
        [TestCategory("Prod")]
        [TestCategory("NAA")]
        public void CreateAreaTest()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(100, 1024);

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
            CorticalArea areaY = new CorticalArea(1, "Y", numCellsInArea);

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

                for (int y = 0; y < i + 1; y++)
                {
                    var synapse = new Synapse(preSynapticNeuron, segment.SegmentIndex, segment.Synapses.Count, 0.5);

                    segment.Synapses.Add(synapse);

                    preSynapticNeuron.ReceptorSynapses.Add(synapse);
                }

                segments.Add(segment);
            }

            var maxSeg = HtmCompute.GetSegmentWithHighesPotential(segments);

            Assert.AreEqual(numSegments - 1, maxSeg.SegmentIndex);

            Assert.IsTrue(numSegments == maxSeg.Synapses.Count);
        }


        /// <summary>
        /// This unit tests creates two areas X and Y and it creates one SDR in each area.
        /// Both SDRs represent tha set of active cells that will be associated with each other.
        /// </summary>
        /// <param name="numCells">NUmber of cells in the Y area, that will learn associationas.</param>
        /// <param name="numActCellsPct">Number of active cells in percent in the area Y.</param>
        [TestMethod]
        [TestCategory("NAA")]
        [DataRow(100, 0.02)]
        [DataRow(100, 0.04)]
        [DataRow(200, 0.01)]
        [DataRow(100, 0.05)]
        [DataRow(500, 0.01)]
        public void AssociateAreasTest(int numCells, double numActCellsPct)
        {
            var cfg = UnitTestHelpers.GetHtmConfig(100, 1024);

            cfg.MaxNewSynapseCount = 5;

            CorticalArea areaX = new CorticalArea(1, "X", 1024);

            CorticalArea areaY = new CorticalArea(2, "Y", 100);

            areaX.ActiveCellsIndicies = UnitTestHelpers.CreateRandomSdr(1024, 0.02);

            areaY.ActiveCellsIndicies = UnitTestHelpers.CreateRandomSdr(numCells, numActCellsPct);

            Naa naa = new Naa(cfg, areaY);

            Debug.WriteLine(naa.TraceState());

            //
            // We train the same association between X and Y 10 times.
            for (int i = 0; i < 10; i++)
            {
                naa.Compute(areaX, true);
                Debug.WriteLine(naa.TraceState());
            }

            AssertAssociations(areaX.ActiveCellsIndicies.Length, numCells, numActCellsPct, areaY, areaY, naa);
        }

        [TestMethod]
        [TestCategory("NAA")]
        [DataRow(100, 0.1)]
        [DataRow(100, 0.04)]
        [DataRow(200, 0.01)]
        [DataRow(100, 0.05)]
        [DataRow(500, 0.01)]
        public void AssociatePopilationssInAreasTest(int numCells, double numActCellsPct)
        {
            var cfg = UnitTestHelpers.GetHtmConfig(100, 1024);

            cfg.MaxNewSynapseCount = 5;

            CorticalArea areaX = new CorticalArea(1, "X", 1024);

            CorticalArea areaY = new CorticalArea(2, "Y", 100);

            Naa naa = new Naa(cfg, areaY);

            // Random SDRs in X area, that will be associated with random SDRs in Y area.
            List<long[]> srcSdrsInX = new List<long[]>();

            // Random SDRs in Y area, that will be associated with random SDRs in Y area.
            List<long[]> destSdrsY = new List<long[]>();

            //
            // Create populations that will be associated.
            for (int i = 0; i < 100; i++)
            {
                srcSdrsInX.Add(UnitTestHelpers.CreateRandomSdr(1024, 0.02));
                destSdrsY.Add(UnitTestHelpers.CreateRandomSdr(numCells, numActCellsPct));
            }

            //
            // Step trough all populations.
            for (int n = 0; n < srcSdrsInX.Count; n++)
            {
                Debug.WriteLine(naa.TraceState());

                areaX.ActiveCellsIndicies = srcSdrsInX[n];
                areaY.ActiveCellsIndicies = destSdrsY[n];

                //
                // We train the same association between X and Y 10 times.
                for (int i = 0; i < 10; i++)
                {
                    naa.Compute(areaX, true);
                    Debug.WriteLine(naa.TraceState());
                    // AssertApicalSynapsePermanences(areaY, cfg.InitialPermanence + (i) * cfg.PermanenceIncrement);
                }

                AssertAssociations(srcSdrsInX[n].Length, numCells, numActCellsPct, areaY, areaX, naa);
            }
        }


        /// <summary>
        /// Approves if the NAA works as designed.
        /// </summary>
        /// <param name="numAssociatingActCells">Number of cells in area X that will be assiciated with the active cells in area Y.</param>
        /// <param name="numCells">Total number of cells in the learning area Y.</param>
        /// <param name="numActCellsPct">The percent number of active cells in area Y. </param>
        /// <param name="areaY">The area that will be associated with associating cells currentlly active in area X.</param>
        /// <param name="naa">The algorithm keeping the learned state.</param>
        private static void AssertAssociations(int numAssociatingActCells, int numCells, double numActCellsPct, CorticalArea areaY, CorticalArea areaX, Naa naa)
        {
            // The Y area of NAA must not have any Inactive segment for the current set of active cells.
            Assert.IsTrue(naa.InactiveApicalSegments.Count == 0);

            // The Y area of NAA must not have any cell without segment for the current set of active cells.
            Assert.IsTrue(naa.ActiveCellsWithoutApicalSegments.Count == 0);

            // The Y area of NAA must not have any matching segment for the current set of active cells.
            Assert.IsTrue(naa.MatchingApicalSegments.Count == 0);

            // The Y area of NAA must not have 2 active segments for the current set of active cells.
            Assert.IsTrue(naa.ActiveApicalSegments.Count == numCells * numActCellsPct);

            // Make sure that all permanences ar maximally learned.
            AssertApicalSynapsePermanences(areaY, 1.0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="areaY"></param>
        /// <param name="expectedPermanence"></param>
        private static void AssertApicalSynapsePermanences(CorticalArea areaY, double expectedPermanence)
        {
            foreach (var activeCell in areaY.ActiveCells)
            {
                // Currentlly, NAA between two different areas use only ApicalSegments.
                Assert.IsTrue(activeCell.DistalDendrites.Count == 0);

                foreach (var seg in activeCell.ApicalDendrites)
                {
                    foreach (var syn in seg.Synapses)

                        Assert.IsTrue(syn.Permanence == expectedPermanence);
                }
            }
        }
    }

}
