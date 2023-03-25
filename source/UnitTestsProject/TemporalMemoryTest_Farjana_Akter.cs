// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest_Farjana_Akter
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

        [TestMethod]
        public void TestLowSparsitySequenceLearningAndRecall()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 64 });
            p.apply(cn);
            tm.Init(cn);

            var sequence1 = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var sequence2 = new int[] { 5, 6, 7, 8, 9 };

            var seq1ActiveColumns = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var seq2ActiveColumns = new int[] { 5, 6, 7, 8, 9 };

            // Learn the sequences multiple times
            for (int i = 0; i < 10; i++)
            {
                tm.Compute(seq1ActiveColumns, true);
                tm.Compute(seq2ActiveColumns, true);
            }

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);
            Assert.IsTrue(recall1.ActiveCells.Select(c => c.Index).SequenceEqual(sequence1));

            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).SequenceEqual(sequence2));
        }

        /// <summary>
        /// Test the update of synapse permanence when matching segments are found
        /// </summary>
        [TestMethod]
        public void TestSynapsePermanenceUpdateWhenMatchingSegmentsFound()
        {
            TemporalMemory tm = new TemporalMemory(); // TM class object
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08); // Used Permanence decrement parameter 
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            Cell[] activeCells = { cn.GetCell(4), cn.GetCell(5) };

            DistalDendrite selectedMatchingSegment = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[0], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[1], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[2], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, cn.GetCell(81), 0.3);

            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(activeCells[1]);
            Synapse as1 = cn.CreateSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            Synapse is1 = cn.CreateSynapse(otherMatchingSegment, cn.GetCell(81), 0.3);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);

            // synapse permanence of matching synapses should be updated
            Assert.AreEqual(0.3, as1.Permanence, 0.01);
            Assert.AreEqual(0.3, is1.Permanence, 0.01);
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
        /// Test if the Temporal Memory can successfully learn and recall patterns of 
        /// sequences with a high sparsity rate.
        /// </summary>
        [TestMethod]
        public void TestHighSparsitySequenceLearningAndRecall()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 4 });
            p.apply(cn);
            tm.Init(cn);

            var sequence1 = new int[] { 0, 10, 20, 30, 40, 50, 60 };
            var sequence2 = new int[] { 5, 15, 25, 35, 45, 55 };

            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 40, 50, 60 };
            var seq2ActiveColumns = new int[] { 5, 15, 25, 35, 45, 55 };

            // Learn the sequences multiple times
            for (int i = 0; i < 10; i++)
            {
                tm.Compute(seq1ActiveColumns, true);
                tm.Compute(seq2ActiveColumns, true);
            }

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);
            TestContext.WriteLine("recall1 ===>>>>> " + string.Join(",", recall1.ActiveCells.Select(c => c.Index)));
            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", sequence1));
            Assert.IsTrue(recall1.ActiveCells.Select(c => c.Index).SequenceEqual(sequence1));

            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);
            TestContext.WriteLine("recall2 ===>>>>> " + string.Join(",", recall2.ActiveCells));
            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", sequence2));
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).SequenceEqual(sequence2));
        }
    }
}
