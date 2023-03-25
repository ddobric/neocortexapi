// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;

namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest_Syed_Mostain_Ahmed
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// For debugging the tests
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
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 4);
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

        private HtmConfig GetDefaultTMParameters()
        {
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 4,
                ActivationThreshold = 3,
                InitialPermanence = 0.21,
                ConnectedPermanence = 0.5,
                MinThreshold = 2,
                MaxNewSynapseCount = 3,
                PermanenceIncrement = 0.1,
                PermanenceDecrement = 0.1,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(42),
                RandomGenSeed = 42
            };

            return htmConfig;
        }


        private Parameters getDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters() : p;
            retVal.Set(key, value);

            return retVal;
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
        [TestMethod]
        public void TestRemovingSynapseFromDistalSegment()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);
            Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(42), 0.8);

            Assert.AreEqual(2, dd.Synapses.Count);

            // remove s1
            dd.KillSynapse(s1);

            Assert.AreEqual(1, dd.Synapses.Count);
            Assert.IsFalse(dd.Synapses.Contains(s1));
            Assert.IsTrue(dd.Synapses.Contains(s2));
        }

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
        /// Verify that Temporal Memory Algorithm can activate predicted columns after 
        /// burst in the previous cycle.
        /// </summary>
        [TestMethod]
        public void TestBurstPredictedColumns()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            List<Cell> expectedWinnerCells = new List<Cell>(cn.GetCells(new int[] { 4, 6 }));

            DistalDendrite activeSegment1 = cn.CreateDistalSegment(expectedWinnerCells[0]);
            cn.CreateSynapse(activeSegment1, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment1, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment1, previousActiveCells[2], 0.5);

            DistalDendrite activeSegment2 = cn.CreateDistalSegment(expectedWinnerCells[1]);
            cn.CreateSynapse(activeSegment2, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment2, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment2, previousActiveCells[2], 0.5);

            ComputeCycle cc = tm.Compute(previousActiveColumns, false) as ComputeCycle; // learn=false
            cc = tm.Compute(activeColumns, true) as ComputeCycle; // learn=true

            Assert.IsTrue(cc.WinnerCells.SequenceEqual(new LinkedHashSet<Cell>(expectedWinnerCells)));
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

        /// <summary>
        /// Test if TemporalMemory behaves correctly when given duplicate active columns
        /// </summary>
        [TestMethod]
        public void TestDuplicateActiveColumns()
        {
            HtmConfig htmConfig = GetDefaultTMParameters();
            Connections cn = new Connections(htmConfig);

            TemporalMemory tm = new TemporalMemory();

            tm.Init(cn);

            int[] activeColumns = { 4, 5, 4, 5, 6 }; // Contains duplicates
            Cell[] burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4 });

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            Assert.IsFalse(cc.ActiveCells.SequenceEqual(burstingCells));
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

        /// <summary>
        /// create a Connections object with some Cells and Segments, 
        /// and then call the GetLeastUsedCell method with a list of Cells and a Random object. We then assert 
        /// that the Cell returned by the method is the one that we expect (in this case, c3)
        /// </summary>
        [TestMethod]
        public void TestGetLeastUsedCell()
        {
            // Create a Connections object with some Cells and Segments
            TemporalMemory tm = new TemporalMemory();
            Parameters p = getDefaultParameters();
            Connections conn = new Connections();
            p.apply(conn);
            tm.Init(conn);

            Cell c1 = conn.GetCell(1);
            Cell c2 = conn.GetCell(2);
            Cell c3 = conn.GetCell(3);
            DistalDendrite s1 = conn.CreateDistalSegment(c1);
            DistalDendrite s2 = conn.CreateDistalSegment(c1);
            DistalDendrite s3 = conn.CreateDistalSegment(c2);
            DistalDendrite s4 = conn.CreateDistalSegment(c3);
            Synapse syn1 = conn.CreateSynapse(s1, c1, 0.5);
            Synapse syn2 = conn.CreateSynapse(s1, c2, 0.5);
            Synapse syn3 = conn.CreateSynapse(s2, c3, 0.5);
            Synapse syn4 = conn.CreateSynapse(s3, c1, 0.5);
            Synapse syn5 = conn.CreateSynapse(s4, c1, 0.5);

            // Get the least used Cell from a list of Cells
            List<Cell> cells = new List<Cell> { c1, c2, c3 };
            Random random = new Random(42);
            Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(conn, cells, random);

            // Verify that the least used Cell is c3
            Assert.AreEqual(c3.ParentColumnIndex, leastUsedCell.ParentColumnIndex);
            Assert.AreEqual(c3.Index, leastUsedCell.Index);
        }

    }
}
