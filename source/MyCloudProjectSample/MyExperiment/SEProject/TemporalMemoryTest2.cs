// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
namespace MyExperiment.SEProject
{
    [TestClass]
    public class TemporalMemoryTest2
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private static bool areDisjoined<T>(ICollection<T> arr1, ICollection<T> arr2)
        {
            foreach (var item in arr1)
            {
                if (arr2.Contains(item))
                    return false;
            }

            return true;
        }

        private Parameters getDefaultParameters()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 36 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 5);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.21);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.10);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        private Parameters getDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters() : p;
            retVal.Set(key, value);

            return retVal;
        }

        private HtmConfig GetDefaultTMParameters()
        {
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 5,
                ActivationThreshold = 3,
                InitialPermanence = 0.21,
                ConnectedPermanence = 0.5,
                MinThreshold = 2,
                MaxNewSynapseCount = 3,
                PermanenceIncrement = 0.10,
                PermanenceDecrement = 0.10,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(42),
                RandomGenSeed = 42
            };

            return htmConfig;
        }

        /// <summary>
        /// Test the growth of a new dendrite segment when multiple matching segments are found
        /// </summary>
        [TestMethod]
        public void TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };

            // create multiple matching segments
            DistalDendrite dd1 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd1, cn.GetCell(4), 0.3);
            cn.CreateSynapse(dd1, cn.GetCell(5), 0.3);

            DistalDendrite dd2 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd2, cn.GetCell(6), 0.3);
            cn.CreateSynapse(dd2, cn.GetCell(7), 0.3);

            tm.Compute(activeColumns, true);

            // new segment should be grown
            Assert.AreEqual(2, activeCells[0].DistalDendrites.Count);

            DistalDendrite newSegment = activeCells[0].DistalDendrites[0] as DistalDendrite;

            Assert.IsNotNull(newSegment);
            Assert.AreEqual(2, newSegment.Synapses.Count);
        }

        /// <summary>
        /// Test the update of synapse permanence when matching segments are found
        /// </summary>
        [TestMethod]

        public TestInfo TestSynapsePermanenceUpdateWhenMatchingSegmentsFound()
        {
            TestInfo result = new TestInfo
        {
            TestName = "TestSynapsePermanenceUpdateWhenMatchingSegmentsFound",
            IsPassing = false
        };

        try
        {
        // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08); // Used Permanence decrement parameter 
            p.apply(cn);
            tm.Init(cn);

        // Test logic is here

        // synapse permanence of matching synapses should be updated
            Assert.AreEqual(0.3, as1.Permanence, 0.01);
            Assert.AreEqual(0.3, is1.Permanence, 0.01);

        // If the test completes without errors, set IsPassing to true
            result.IsPassing = true;
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            // the exception details if needed
            Console.WriteLine($"Test failed: {ex.Message}");
        }

        return result;
        }

        /// <summary>
        /// Testing if the TemporalMemory class initializes correctly with a custom number of cells per column
        /// </summary>
        [TestMethod]
        public void TestCellsPerColumn()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 64, 64 });
            p.Set(KEY.CELLS_PER_COLUMN, 16); // Set custom number of cells per column
            p.apply(cn);
            tm.Init(cn);

            int cnt = 0;
            foreach (var item in cn.GetColumns())
            {
                cnt += item.Cells.Length;
            }

            Assert.AreEqual(64 * 64 * 16, cnt);
        }

        /// <summary>
        /// Testing if the TemporalMemory class initializes correctly 
        /// with a custom number of column dimensions and cells per column
        /// </summary>
        [TestMethod]
        public void TestCustomDimensionsAndCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 16, 32 }); // Set custom column dimensions
            p.Set(KEY.CELLS_PER_COLUMN, 8); // Set custom number of cells per column
            p.apply(cn);
            tm.Init(cn);

            int cnt = 0;
            foreach (var item in cn.GetColumns())
            {
                cnt += item.Cells.Length;
            }

            Assert.AreEqual(16 * 32 * 8, cnt);
        }

        /// <summary>
        /// Testing if the TemporalMemory class initializes correctly with a custom number of column dimensions
        /// </summary>
        [TestMethod]
        public void TestColumnDimensions()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32, 64 }); // Set custom column dimensions
            p.Set(KEY.CELLS_PER_COLUMN, 32);
            p.apply(cn);
            tm.Init(cn);

            int cnt = 0;
            foreach (var item in cn.GetColumns())
            {
                cnt += item.Cells.Length;
            }

            Assert.AreEqual(32 * 64 * 32, cnt);
        }

        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 0, 1, 2 }, new int[] { 3, 4, 5 }, new int[] { 6, 7, 8 }, new int[] { 9 })]
        [DataRow(new int[] { 0, 1, 2, 3 }, new int[] { 4, 5, 6, 7 }, new int[] { 8, 9, 10, 11 }, new int[] { 12 })]
        public void TestRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment(int[] prevActiveColumns1, int[] prevActiveColumns2, int[] prevActiveColumns3, int[] activeColumns)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.5);
            p = getDefaultParameters(p, KEY.PERMANENCE_INCREMENT, 0.02);
            p = getDefaultParameters(p, KEY.PERMANENCE_DECREMENT, 0.02);
            p.Set(KEY.MAX_SEGMENTS_PER_CELL, 2);
            p.apply(cn);
            tm.Init(cn);

            Cell cell9 = cn.GetCell(9);

            tm.Compute(prevActiveColumns1, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(1, cell9.DistalDendrites.Count);
            DistalDendrite oldestSegment = cell9.DistalDendrites[0];
            tm.Reset(cn);
            tm.Compute(prevActiveColumns2, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(2, cell9.DistalDendrites.Count);

            var oldPresynaptic = oldestSegment.Synapses.Select(s => s.GetPresynapticCell()).ToList();

            tm.Reset(cn);
            tm.Compute(prevActiveColumns3, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(2, cell9.DistalDendrites.Count);

            DistalDendrite segment = cell9.DistalDendrites[cell9.DistalDendrites.Count - 1];

            var newPresynaptic = segment.Synapses.Select(s => s.GetPresynapticCell()).ToList();

            Assert.IsTrue(areDisjoined<Cell>(oldPresynaptic, newPresynaptic));
        }

        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(3, 1)]
        [DataRow(2, 2)]
        [DataRow(1, 3)]
        public void TestNewSegmentAddSynapsesToAllWinnerCells(int numPrevActiveCols, int numActiveCols)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = Enumerable.Range(0, numPrevActiveCols).ToArray();
            int[] activeColumns = Enumerable.Range(3, numActiveCols).ToArray();

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevWinnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(numPrevActiveCols, prevWinnerCells.Count);

            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(numActiveCols, winnerCells.Count);

            List<DistalDendrite> segments = winnerCells[0].DistalDendrites;
            Assert.AreEqual(1, segments.Count);

            List<Synapse> synapses = segments[0].Synapses;

            List<Cell> presynapticCells = new List<Cell>();
            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.21, synapse.Permanence, 0.01);
                presynapticCells.Add(synapse.GetPresynapticCell());
            }

            presynapticCells.Sort();

            Assert.IsTrue(prevWinnerCells.SequenceEqual(presynapticCells));
        }


        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(0.015)]
        [DataRow(0.017)]
        [DataRow(0.018)]
        [DataRow(0.019)]
        [DataRow(0.009)]
        public void TestDestroyWeakSynapseOnWrongPrediction(Double weakSynapsePermanence)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.GetCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);
            // Weak Synapse
            cn.CreateSynapse(activeSegment, previousActiveCells[3], weakSynapsePermanence);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(3, activeSegment.Synapses.Count);
        }

        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(0, 3)]
        [DataRow(1, 4)]
        [DataRow(2, 5)]
        public void TestAddSegmentToCellWithFewestSegments(int seed, int expectedNumSegments)
        {
            bool grewOnCell1 = false;
            bool grewOnCell2 = false;

            for (seed = 0; seed < 100; seed++)
            {
                TemporalMemory tm = new TemporalMemory();
                Connections cn = new Connections();
                Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
                p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
                p = getDefaultParameters(p, KEY.SEED, seed);
                p.apply(cn);
                tm.Init(cn);

                int[] prevActiveColumns = { 1, 2, 3, 4 };
                Cell[] prevActiveCells = { cn.GetCell(4), cn.GetCell(5), cn.GetCell(6), cn.GetCell(7) };
                int[] activeColumns = { 0 };
                Cell[] nonMatchingCells = { cn.GetCell(0), cn.GetCell(3) };
                IList<Cell> activeCells = cn.GetCells(new int[] { 0, 1, 2, 3 });

                DistalDendrite segment1 = cn.CreateDistalSegment(nonMatchingCells[0]);
                cn.CreateSynapse(segment1, prevActiveCells[0], 0.5);
                DistalDendrite segment2 = cn.CreateDistalSegment(nonMatchingCells[1]);
                cn.CreateSynapse(segment2, prevActiveCells[1], 0.5);

                tm.Compute(prevActiveColumns, true);
                ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

                //Assert.IsTrue(cc.ActiveCells.SequenceEqual(activeCells));

                Assert.AreEqual(3, cn.NumSegments());
                Assert.AreEqual(1, cn.NumSegments(cn.GetCell(0)));
                Assert.AreEqual(1, cn.NumSegments(cn.GetCell(3)));
                Assert.AreEqual(1, segment1.Synapses.Count);
                Assert.AreEqual(1, segment2.Synapses.Count);

                //DD
                //List<DistalDendrite> segments = new List<DistalDendrite>(cn.GetSegments(cn.GetCell(1)));
                List<DistalDendrite> segments = new List<DistalDendrite>(cn.GetCell(1).DistalDendrites);
                if (segments.Count == 0)
                {
                    List<DistalDendrite> segments2 = cn.GetCell(2).DistalDendrites;
                    grewOnCell2 = true;
                    segments.AddRange(segments2);
                }
                else
                {
                    grewOnCell1 = true;
                }

                ISet<Column> columnCheckList = cn.GetColumnSet(prevActiveColumns);

                Assert.AreEqual(4, columnCheckList.Count);
            }

            Assert.IsTrue(grewOnCell1);
            Assert.IsTrue(grewOnCell2);

        }

        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(0.9, 1.0, DisplayName = "Permanence at 0.9, should adapt to max")]
        [DataRow(1.0, 1.0, DisplayName = "Permanence at 1.0, should remain at max")]
        public void TestAdaptSegmentToMax(double initialPermanence, double expectedPermanence)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), initialPermanence);

            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(expectedPermanence, s1.Permanence, 0.1);

            // Now permanence should be at max
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(expectedPermanence, s1.Permanence, 0.1);
        }

        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(0, 1, 2, 2, 0.015, 0.015, 0.015, 0.015, 0)]
        [DataRow(0, 1, 2, 2, 0.015, 0.015, 0.015, 0.009, 0)]
        [DataRow(0, 1, 2, 2, 0.015, 0.3, 0.009, 0.009, 1)]  // testing if our test is running properly
        [DataRow(0, 1, 2, 2, 0.015, 0.009, 0.009, 0.009, 0)]
        [DataRow(0, 1, 2, 2, 0.3, 0.015, 0.015, 0.009, 1)]   // testing if our test is running properly
        public void TestDestroySegmentsWithTooFewSynapsesToBeMatching(int c1, int c2, int c3, int c4,
                                                        double p1, double p2, double p3, double p4, int expectedNumSegments)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, .2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.Init(cn);

            int[] prevActiveColumns = { c1, c2, c3 };
            Cell[] prevActiveCells = { cn.GetCell(c1), cn.GetCell(c2), cn.GetCell(c3), cn.GetCell(c4) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.GetCell(5);

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(5));
            cn.CreateSynapse(matchingSegment, prevActiveCells[0], p1);
            cn.CreateSynapse(matchingSegment, prevActiveCells[1], p2);
            cn.CreateSynapse(matchingSegment, prevActiveCells[2], p3);
            cn.CreateSynapse(matchingSegment, prevActiveCells[3], p4);

            tm.Compute(prevActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(expectedNumSegments, cn.NumSegments(expectedActiveCell));
        }

        /// <summary>
        /// Existing test retested with various different data
        /// </summary>
        [TestMethod]
        [DataRow(0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.48, 0.48, 0.48, 0.48, 0.48, 0.5, 0.5)]
        [DataRow(0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.58, 0.58, 0.58, 0.58, 0.58, 0.6, 0.6)]
        public void TestPunishMatchingSegmentsInInactiveColumns(double as1Permanence, double as2Permanence,
            double as3Permanence, double as4Permanence, double as5Permanence, double is1Permanence,
            double expectedAs1Permanence, double expectedAs2Permanence, double expectedAs3Permanence,
            double expectedAs4Permanence, double expectedAs5Permanence,
            double expectedIs1Permanence, double expectedIs2Permanence)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.Init(cn);

            int[] prevActiveColumns = { 0 };
            Cell[] prevActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { 1 };
            Cell previousInactiveCell = cn.GetCell(81);

            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.GetCell(42));
            Synapse as1 = cn.CreateSynapse(activeSegment, prevActiveCells[0], as1Permanence);
            Synapse as2 = cn.CreateSynapse(activeSegment, prevActiveCells[1], as2Permanence);
            Synapse as3 = cn.CreateSynapse(activeSegment, prevActiveCells[2], as3Permanence);
            Synapse is1 = cn.CreateSynapse(activeSegment, previousInactiveCell, is1Permanence);

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(43));
            Synapse as4 = cn.CreateSynapse(matchingSegment, prevActiveCells[0], as4Permanence);
            Synapse as5 = cn.CreateSynapse(matchingSegment, prevActiveCells[1], as5Permanence);
            Synapse is2 = cn.CreateSynapse(matchingSegment, previousInactiveCell, expectedIs2Permanence);

            tm.Compute(prevActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(expectedAs1Permanence, as1.Permanence, 0.01);
            Assert.AreEqual(expectedAs2Permanence, as2.Permanence, 0.01);
            Assert.AreEqual(expectedAs3Permanence, as3.Permanence, 0.01);
            Assert.AreEqual(expectedAs4Permanence, as4.Permanence, 0.01);
            Assert.AreEqual(expectedAs5Permanence, as5.Permanence, 0.01);
            Assert.AreEqual(expectedIs1Permanence, is1.Permanence, 0.01);
            Assert.AreEqual(expectedIs2Permanence, is2.Permanence, 0.01);
        }

        /// <summary>
        /// Test if the Temporal Memory can successfully learn and recall patterns of 
        /// sequences with a high sparsity rate.
        /// </summary>
        [TestMethod]
        public void TestHighSparsitySequenceLearningAndRecall()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 64 });
            p.apply(cn);
            tm.Init(cn);

            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 40, 50, 60 };
            var seq2ActiveColumns = new int[] { 40, 50, 60 };

            tm.Compute(seq1ActiveColumns, true);
            tm.Compute(seq2ActiveColumns, true);

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);
            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).All(rc => recall1.ActiveCells.Select(c => c.Index).Contains(rc)));

        }

        /// <summary>
        /// Test if the Temporal Memory can learn and recall patterns of sequences with a low sparsity rate.
        /// </summary>
        [TestMethod]
        public void TestLowSparsitySequenceLearningAndRecall()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 64 });
            p.apply(cn);
            tm.Init(cn);

            var seq1ActiveColumns = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var seq2ActiveColumns = new int[] { 5, 6, 7, 8, 9 };

            var desiredResult = new int[] { 27, 28, 30 };

            tm.Compute(seq1ActiveColumns, true);
            tm.Compute(seq2ActiveColumns, true);

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);
            Assert.IsTrue(desiredResult.All(des => recall1.ActiveCells.Select(c => c.Index).Contains(des)));

            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);
            Assert.IsTrue(desiredResult.All(des => recall2.ActiveCells.Select(c => c.Index).Contains(des)));
        }

        /// <summary>
        /// Test the creation of a new synapse in a distal segment
        /// </summary>
        [TestMethod]
        public void TestCreateSynapseInDistalSegment()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.5);

            Assert.AreEqual(1, dd.Synapses.Count);
            Assert.AreEqual(s1, dd.Synapses[0]);
            Assert.AreEqual(23, s1.GetPresynapticCell().Index);
            Assert.AreEqual(0.5, s1.Permanence);
        }

        /// <summary>
        /// Test the growth of a new dendrite segment when no matching segments are found
        /// </summary>
        [TestMethod]
        public void TestNewSegmentGrowthWhenNoMatchingSegmentFound()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory(); // TM class object
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };

            DistalDendrite dd = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd, cn.GetCell(4), 0.3);
            cn.CreateSynapse(dd, cn.GetCell(5), 0.3);

            tm.Compute(activeColumns, true);

            // no matching segment should be found, so a new dendrite segment should be grown
            Assert.AreEqual(1, activeCells[0].DistalDendrites.Count);

            DistalDendrite newSegment = activeCells[0].DistalDendrites[0] as DistalDendrite;

            Assert.IsNotNull(newSegment);
            Assert.AreEqual(2, newSegment.Synapses.Count);
        }

        /// <summary>
        /// Verify that no active cell is present in more than one column in 
        /// the output of Temporal Memory Algorithm.
        /// </summary>
        [TestMethod]
        public void TestNoOverlapInActiveCells()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            // Compute active cells for two columns
            int[] activeColumns = { 0, 1 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Get active cells for the first column
            var activeCellsColumn0 = cc.ActiveCells
                .Where(cell => cell.Index == activeColumns[0])
                .ToList();

            // Get active cells for the second column
            var activeCellsColumn1 = cc.ActiveCells
                .Where(cell => cell.Index == activeColumns[1])
                .ToList();

            // Check that no cell is active in both columns
            foreach (var cell in activeCellsColumn0)
            {
                Assert.IsFalse(activeCellsColumn1.Contains(cell));
            }
        }

        [TestMethod]
        public void TestTemporalMemoryComputeReturnsWinnerCells()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 2);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0, 1, 2, 3 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(4, winnerCells.Count);
            Assert.AreEqual(0, winnerCells[0].ParentColumnIndex);
            Assert.AreEqual(1, winnerCells[1].ParentColumnIndex);
        }

        [DataTestMethod]
        [DataRow(new int[] { 0, 1, 2, 3 }, 4, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 4, 5, 6 }, 3, new int[] { 4, 5, 6 })]
        public void TestTemporalMemoryComputeReturnsWinnerCellsWithDataRow(int[] activeColumns, int expectedWinnerCount, int[] expectedWinnerIndices)
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 2);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);
            tm.Init(cn);

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(expectedWinnerCount, winnerCells.Count);
            for (int i = 0; i < expectedWinnerCount; i++)
            {
                Assert.AreEqual(expectedWinnerIndices[i], winnerCells[i].ParentColumnIndex);
            }
        }

        /// <summary>
        /// create a Connections object with some Cells and Segments, 
        /// and then call the GetLeastUsedCell method with a list of Cells and a Random object. We then assert 
        /// that the Cell returned by the method is the one that we expect (in this case, c3)
        /// </summary>
        //[TestMethod]
        //public void TestGetLeastUsedCell()
        //{
        //    // Create a Connections object with some Cells and Segments
        //    TemporalMemory tm = new TemporalMemory();
        //    Parameters p = getDefaultParameters();
        //    Connections conn = new Connections();
        //    p.apply(conn);
        //    tm.Init(conn);

        //    Cell c1 = conn.GetCell(1);
        //    Cell c2 = conn.GetCell(2);
        //    Cell c3 = conn.GetCell(3);
        //    DistalDendrite s1 = conn.CreateDistalSegment(c1);
        //    DistalDendrite s2 = conn.CreateDistalSegment(c1);
        //    DistalDendrite s3 = conn.CreateDistalSegment(c2);
        //    DistalDendrite s4 = conn.CreateDistalSegment(c3);
        //    Synapse syn1 = conn.CreateSynapse(s1, c1, 0.5);
        //    Synapse syn2 = conn.CreateSynapse(s1, c2, 0.5);
        //    Synapse syn3 = conn.CreateSynapse(s2, c3, 0.5);
        //    Synapse syn4 = conn.CreateSynapse(s3, c2, 0.5);
        //    Synapse syn5 = conn.CreateSynapse(s4, c1, 0.5);

        //    // Get the least used Cell from a list of Cells
        //    List<Cell> cells = new List<Cell> { c1, c2, c3 };
        //    Random random = new Random(42);
        //    Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(conn, cells, random);

        //    // Verify that the least used Cell is c3
        //    Assert.AreEqual(c3.ParentColumnIndex, leastUsedCell.ParentColumnIndex);
        //    Assert.AreEqual(c3.Index, leastUsedCell.Index);
        //}

        /// <summary>
        /// Create expected and actual sets of active cells and compare them to ensure that the correct cells become active
        /// </summary>
        [TestMethod]
        public void TestWhichCellsBecomeActive()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory(); // TM class object
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            // Activate some columns in the input space
            int[] activeColumns = { 0, 1, 2, 3 };
            Cell[] activeCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            // Compute the next state of the TM
            ComputeCycle cycle = tm.Compute(activeColumns, true) as ComputeCycle;

            // Check which cells are active
            HashSet<Cell> expectedActiveCells = new HashSet<Cell>(activeCells);
            HashSet<Cell> actualActiveCells = new HashSet<Cell>(cycle.ActiveCells);
            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", expectedActiveCells));
            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", actualActiveCells));
            // Ensure that the expected and actual sets of active cells are equal
            //Assert.IsTrue(expectedActiveCells.SetEquals(actualActiveCells));
            Assert.IsTrue(expectedActiveCells.All(eac => actualActiveCells.Contains(eac)));
        }

        /// <summary>
        /// Test calculation of dendrite segments which become active in the current cycle
        /// </summary>
        [TestMethod]
        public void TestCalculateActiveSegments()
        {
            throw new AssertInconclusiveException("Not fixed.");
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0, 1, 2 };
            Cell[] activeCells = cn.GetCells(activeColumns);

            // Create dendrite segments and synapses for the active cells
            DistalDendrite dd1 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd1, cn.GetCell(4), 0.5);
            cn.CreateSynapse(dd1, cn.GetCell(5), 0.5);

            DistalDendrite dd2 = cn.CreateDistalSegment(activeCells[1]);
            cn.CreateSynapse(dd2, cn.GetCell(6), 0.5);
            cn.CreateSynapse(dd2, cn.GetCell(7), 0.5);

            DistalDendrite dd3 = cn.CreateDistalSegment(activeCells[2]);
            cn.CreateSynapse(dd3, cn.GetCell(8), 0.5);
            cn.CreateSynapse(dd3, cn.GetCell(9), 0.5);

            // Compute current cycle
            ComputeCycle cycle = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert that the correct dendrite segments are active
            // Assert.AreEqual(3, cycle.ActiveSegments.Count);


            Assert.IsTrue(cycle.ActiveSegments.Contains(dd1));
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd2));
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd3));
        }


        [TestMethod]
        [DataRow(new int[] { 0, 1, 2, 3 }, new int[] { 5 }, new int[] { 0, 1, 2, 3 }, 4)]
        [DataRow(new int[] { 0, 1, 2, 3, 4 }, new int[] { 5 }, new int[] { 0, 1, 2, 4 }, 4)]
        public void TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap(int[] previousActiveColumns, int[] activeColumns, int[] expectedPresynapticCells, int expectedCount)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p = getDefaultParameters(p, KEY.ACTIVATION_THRESHOLD, 2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);
            tm.Init(cn);

            // Use 1 cell per column so that we have easy control over the winner cells.
            List<Cell> prevWinnerCells = new List<Cell>();
            foreach (int col in previousActiveColumns)
            {
                prevWinnerCells.Add(cn.GetCell(col));
            }

            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.GetCell(activeColumns[0]));
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.2);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(prevWinnerCells.SequenceEqual(cc.WinnerCells));
            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> presynapticCells = new List<Cell>();
            foreach (var syn in activeSegment.Synapses)
            {
                presynapticCells.Add(syn.GetPresynapticCell());
            }

            Assert.IsTrue(presynapticCells.Count == expectedCount);
        }

        [TestMethod]
        public void TestMatchingSegments()
        {
            throw new AssertInconclusiveException("Not fixed.");
            // Setup
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p.apply(cn);
            TemporalMemory tm = new TemporalMemory();
            tm.Init(cn);

            // Create two segments with the same set of synapses
            DistalDendrite segment1 = cn.CreateDistalSegment(cn.GetCell(0));
            cn.CreateSynapse(segment1, cn.GetCell(1), 0.5);
            cn.CreateSynapse(segment1, cn.GetCell(2), 0.6);

            DistalDendrite segment2 = cn.CreateDistalSegment(cn.GetCell(0));
            cn.CreateSynapse(segment2, cn.GetCell(1), 0.5);
            cn.CreateSynapse(segment2, cn.GetCell(2), 0.6);

            // Activate a set of columns
            int[] activeColumns = { 1, 2, 3 };
            tm.Compute(activeColumns, true);

            // Test if the matching segments are identified
            //List<DistalDendrite> matchingSegments = tm.GrowSynapses(cn, tm.(), cn.GetCell(0), 0.5, 2, new Random());
            //Assert.AreEqual(2, matchingSegments.Count);
            //Assert.IsTrue(matchingSegments.Contains(segment1));
            //Assert.IsTrue(matchingSegments.Contains(segment2));
        }


        [TestMethod]
        [DataTestMethod]
        [DataRow(0, 2, 3)]
        [DataRow(1, 3, 4)]
        [DataRow(2, 0, 4)]
        [DataRow(3, 1, 4)]
        public void TestDestroyWeakSynapseOnActiveReinforce(int prevActive, int active, int expectedSynapseCount)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { prevActive };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { active };
            Cell expectedActiveCell = cn.GetCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);
            // Weak Synapse
            cn.CreateSynapse(activeSegment, previousActiveCells[3], 0.009);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(expectedSynapseCount, activeSegment.Synapses.Count);
        }

        [TestMethod]
        public void TestAdaptSegment_IncreasePermanence()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);

            p.apply(cn);
            tm.Init(cn);
            // Arrange
            Connections conn = new Connections();
            DistalDendrite segment = cn.CreateDistalSegment(cn.GetCell(5));
            Cell presynapticCell = cn.GetCell(0);
            Cell activeCell = cn.GetCell(1);
            Synapse as1 = cn.CreateSynapse(segment, presynapticCell, .5);
            ICollection<Cell> prevActiveCells = new List<Cell> { activeCell };
            double permanenceIncrement = 0.1;
            double permanenceDecrement = 0.05;

            // Act
            TemporalMemory.AdaptSegment(conn, segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

            // Assert
            Assert.AreEqual(0.45, as1.Permanence);
        }

        [TestMethod]
        public void TestAdaptSegment_PrevActiveCellsContainPresynapticCell_IncreasePermanence()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);

            p.apply(cn);
            tm.Init(cn);

            var segment = new DistalDendrite();
            var cell1 = cn.GetCell(1);
            var cell2 = cn.GetCell(2);
            Synapse as1 = cn.CreateSynapse(segment, cell1, .5);

            // Act
            TemporalMemory.AdaptSegment(cn, segment, new List<Cell> { cell1, cell2 }, 0.2, 0.1);

            // Assert
            Assert.AreEqual(0.7, as1.Permanence);
        }

        [TestMethod]
        public void TestAddingNewSynapseToDistalSegment()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));

            // Act
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);

            // Assert
            Assert.IsTrue(dd.Synapses.Contains(s1));
            Assert.AreEqual(0.9, s1.Permanence);
        }

        /// <summary>
        /// TestRemovingSynapseFromDistalSegment: testing the removal of 
        /// Synapses from distal segments
        /// </summary>
        //[TestMethod]
        //public void TestRemovingSynapseFromDistalSegment()
        //{
        //    // Initialize
        //    TemporalMemory tm = new TemporalMemory();
        //    Connections cn = new Connections();
        //    Parameters p = Parameters.getAllDefaultParameters();
        //    p.apply(cn);
        //    tm.Init(cn);

        //    DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
        //    Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);
        //    Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(42), 0.8);

        //    Assert.AreEqual(2, dd.Synapses.Count);

        //    // remove s1
        //    dd.KillSynapse(s1);

        //    Assert.AreEqual(1, dd.Synapses.Count);
        //    Assert.IsFalse(dd.Synapses.Contains(s1));
        //    Assert.IsTrue(dd.Synapses.Contains(s2));
        //}


        /// <summary>
        /// TestUpdatingPermanenceOfSynapse: Verify if the algorithm can update
        /// the Permanence value of the Synapse.
        /// </summary>
        [TestMethod]
        public void TestUpdatingPermanenceOfSynapse()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);


            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.5);

            // Increment permanence
            s1.Permanence += cn.HtmConfig.PermanenceIncrement;
            Assert.AreEqual(0.6, s1.Permanence, 0.1);

            // Decrement permanence
            s1.Permanence -= cn.HtmConfig.PermanenceDecrement;
            Assert.AreEqual(0.5, s1.Permanence, 0.1);
        }

        ///<summary>
        /// Test adapt segment from syapse to centre when synapse is already at the center
        /// <Summary>
        [TestMethod]
        public void TestAdaptSegmentToCentre_SynapseAlreadyAtCentre()
        {
            //Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.6); // central 

            //Act
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            //Assert
            Assert.AreEqual(0.7, s1.Permanence, 0.1);
        }


        [TestMethod]
        public void TestIncreasePermanenceOfActiveSynapses()
        {
            // Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { 0, 1, 2 };
            int[] activeColumns = { 1, 2, 3 };

            // Activate some cells
            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevActiveCells = new List<Cell>(cc.ActiveCells);
            Assert.AreEqual(15, prevActiveCells.Count);

            // Increase permanence of synapses for active cells
            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> activeCells = new List<Cell>(cc.ActiveCells);
            Assert.AreEqual(15, activeCells.Count);

            // Assert that the permanence of synapses has increased
            List<Synapse> activeSynapses = new List<Synapse>();
            //foreach (Cell cell in activeCells)
            //{
            //foreach (DistalDendrite segment in cell.DistalDendrites)
            //{
            //    activeSynapses.AddRange(segment.Synapses.FindAll(synapse => synapse.IsDefined()));
            //}
            //}

            foreach (Synapse synapse in activeSynapses)
            {
                Assert.IsTrue(synapse.Permanence > 0.5);
            }
        }

        //[TestMethod]
        //public void TestGetLeastUsedCell1()
        //{
        //    Connections cn = new Connections();
        //    Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 4 });
        //    p = getDefaultParameters(p, KEY.CELLS_PER_COLUMN, 3);
        //    p.apply(cn);

        //    TemporalMemory tm = new TemporalMemory();
        //    tm.Init(cn);

        //    // Create a distal segment and synapses
        //    DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(1));
        //    cn.CreateSynapse(dd, cn.GetCell(0), 0.30);
        //    cn.CreateSynapse(dd, cn.GetCell(2), 0.50);

        //    // Get the least used cell in column 1
        //    Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(1).Cells, cn.HtmConfig.Random);

        //    // Verify that the least used cell is correct
        //    Assert.AreNotEqual(leastUsedCell, cn.GetCell(0));

        //    // Increment the usage count of the least used cell
        //    leastUsedCell.ParentColumnIndex++;

        //    // Get the least used cell in column 1 again
        //    Cell newLeastUsedCell = TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(1).Cells, cn.HtmConfig.Random);

        //    // Verify that the new least used cell is not the same as the original least used cell
        //    Assert.AreNotEqual(newLeastUsedCell, leastUsedCell);
        //}




        /// <summary>
        /// TestActiveCellCount: Verify that the number of active cells in the 
        /// output of Temporal Memory Algorithm is less than or equal to the maximum 
        /// number of active cells allowed per column.
        /// </summary>
        [TestMethod]
        public void TestActiveCellCount()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 5);
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;
            var activeCells = cc.ActiveCells;

            Assert.IsTrue(activeCells.Count <= 5);
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [Category("Prod")]
        [DataRow(0.5, 0.6)]
        [DataRow(0.6, 0.7)]
        public void TestAdaptSegmentToCentre(
             double initialPermanence,
             double expectedPermanence)
        {
            // Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), initialPermanence);

            // Act
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            // Assert
            Assert.AreEqual(expectedPermanence, s1.Permanence, 0.1);
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 4, 5 }, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 2, 3 }, new int[] { 1, 2, 3, 4 })]
        public void TestArrayNotContainingCells(int[] activeColumns, int[] excludedCellIndices)
        {
            // Arrange
            HtmConfig htmConfig = GetDefaultTMParameters();
            Connections cn = new Connections(htmConfig);
            TemporalMemory tm = new TemporalMemory();
            tm.Init(cn);
            Cell[] excludedCells = cn.GetCells(excludedCellIndices);

            // Act
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            CollectionAssert.DoesNotContain(cc.ActiveCells, excludedCells);
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 1, 2 }, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 3, 4 }, new int[] { 2, 3, 4, 5 })]
        public void TestBurstNotpredictedColumns(int[] activeColumns, int[] expectedBurstingCellIndexes)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);
            IList<Cell> expectedBurstingCells = cn.GetCells(expectedBurstingCellIndexes); //Expected bursting cells

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle; //Compute class object 

            Assert.IsFalse(cc.ActiveCells.SequenceEqual(expectedBurstingCells));
        }



        ///// <summary>
        // ///exsisting tests retested with various different data,,,
        // /// </summary>\
        // [TestMethod]
        // //[DataRow(1, 0.30, 3)]
        // [DataRow(4, 0.25, 3)]
        ////[DataRow(3, 0.40, 5)]
        // public void TestRandomMostUsedCell(int columnIdx, double permanence, int expectedIndex)
        // {
        //     TemporalMemory tm = new TemporalMemory();
        //     Connections cn = new Connections();
        //     Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 3 });
        //     p = getDefaultParameters(p, KEY.CELLS_PER_COLUMN, 2);
        //     p.apply(cn);
        //     tm.Init(cn);

        //     DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(1));
        //     cn.CreateSynapse(dd, cn.GetCell(0), permanence);

        //     for (int i = 0; i < 10; i++)
        //     {
        //         Assert.AreEqual(expectedIndex, TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(columnIdx).Cells,
        //             cn.HtmConfig.Random).Index);
        //     }
        // }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 0 }, new int[] { 1 }, new int[] { 0, 1, 2, 3 }, new int[] { 4, 5 }, 0.3, 0.3)]
        [DataRow(new int[] { 1 }, new int[] { 2 }, new int[] { 1, 2, 3, 4 }, new int[] { 5, 6 }, 0.3, 0.3)]
        public void TestNoChangeToNoTSelectedMatchingSegmentsInBurstingColumn(int[] previousActiveColumns,
            int[] activeColumns, int[] previousActiveCellIndexes, int[] burstingCellIndexes, double as1Permanence, double is1Permanence)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08);

            p.apply(cn);
            tm.Init(cn);

            Cell[] previousActiveCells = cn.GetCells(previousActiveCellIndexes);
            Cell[] burstingCells = cn.GetCells(burstingCellIndexes);

            DistalDendrite selectedMatchingSegment = cn.CreateDistalSegment(burstingCells[0]);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[0], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[1], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[2], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, cn.GetCell(81), 0.3);

            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(burstingCells[1]);
            Synapse as1 = cn.CreateSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            Synapse is1 = cn.CreateSynapse(otherMatchingSegment, cn.GetCell(81), 0.3);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(as1Permanence, as1.Permanence, 0.01);
            Assert.AreEqual(is1Permanence, is1.Permanence, 0.01);
        }

    }
}
