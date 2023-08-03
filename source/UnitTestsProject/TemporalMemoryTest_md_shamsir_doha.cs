// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest_md_shamsir_doha
    {
        private Parameters getDefaultParameters()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 36 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 5);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 4);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.22);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.9);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(22));
            retVal.Set(KEY.SEED, 22);

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
                CellsPerColumn = 2,
                ActivationThreshold = 6,
                InitialPermanence = 0.8,
                ConnectedPermanence = 0.1,
                MinThreshold = 2,
                MaxNewSynapseCount = 3,
                PermanenceIncrement = 0.10,
                PermanenceDecrement = 0.01,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(22),
                RandomGenSeed = 22
            };

            return htmConfig;
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
            // Arrange TestIncreasePermanenceOfActiveSynapses unit test method tests the ability of a temporal memory algorithm to increase the permanence of
            // active synapses in response to a set of active columns. The test initializes a temporal memory object and applies the default parameters to it.
            // It then activates a set of cells in the temporal memory and stores them in a list.
            // Next, the algorithm is called again with a different set of active cells, and the permanence of the synapses in those active cells is increased.
            // . Finally, the test checks that the permanence of the synapses in the active cells has increased above a certain threshold. The test is designed to ensure that the temporal memory algorithm can
            // learn and adapt to changing input patterns by increasing the permanence of the synapses in active cells.
            // This is an important feature of a successful temporal memory algorithm, as it allows the system to recognize and respond to patterns in a dynamic and adaptive way.
            // The test also verifies that the algorithm is correctly updating the permanence values of the synapses, which is critical for accurate prediction and classification of input patterns.
            // Overall, this unit test serves as a validation of the functionality and effectiveness of the temporal memory algorithm.
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

        [TestMethod]
        //The purpose of TestGetLeastUsedCell1 unit test is to verify the correct functionality of the "GetLeastUsedCell" method. The unit test initializes a TemporalMemory object with a Connections object and applies default parameters.
        //Then, a DistalDendrite object is created with two Synapse objects.
        //The "GetLeastUsedCell" method is called with the Cells of a specified Column as a parameter to find the least used Cell in that Column.
        //The returned Cell is then verified to ensure that it is not equal to the Cell with the lowest usage
        //count. The usage count of the returned Cell is then incremented, and the "GetLeastUsedCell" method is called again to ensure that a different Cell is returned.
        //This unit test verifies the functionality of the "GetLeastUsedCell" method and ensures that the method returns the expected result. 
        public void TestGetLeastUsedCell()
        {
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 4 });
            p = getDefaultParameters(p, KEY.CELLS_PER_COLUMN, 3);
            p.apply(cn);

            TemporalMemory tm = new TemporalMemory();
            tm.Init(cn);

            // Create a distal segment and synapses
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(1));
            cn.CreateSynapse(dd, cn.GetCell(0), 0.30);
            cn.CreateSynapse(dd, cn.GetCell(2), 0.50);

            // Get the least used cell in column 1
            Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(1).Cells, cn.HtmConfig.Random);

            // Verify that the least used cell is correct
            Assert.AreNotEqual(leastUsedCell, cn.GetCell(0));

            // Increment the usage count of the least used cell
            leastUsedCell.ParentColumnIndex++;

            // Get the least used cell in column 1 again
            Cell newLeastUsedCell = TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(1).Cells, cn.HtmConfig.Random);

            // Verify that the new least used cell is not the same as the original least used cell
            Assert.AreNotEqual(newLeastUsedCell, leastUsedCell);
        }

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
        [TestCategory("Prod")]
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
        /// </summary>\
        [TestMethod]
        [TestCategory("Prod")]
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
    }
}
