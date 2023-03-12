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
        //<summary>
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
        public void TestWinnerCells_top()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            p.apply(cn);
            tm.Init(cn);

            int[] zeroColumns = { };
            int[] activeColumns = { 0 };

            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(2), cn.GetCell(1), cn.GetCell(2) };
            tm.Compute(zeroColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(0, cn.NumSegments(), 0);
        }

        [TestMethod]

       
        public void TestPredictedActiveCellsAreAlwaysWinners2()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(2), cn.GetCell(1), cn.GetCell(2) };
            List<Cell> expectedWinnerCells = new List<Cell>(cn.GetCells(new int[] { 4, 9 }));

            DistalDendrite activeSegment1 = cn.CreateDistalSegment(expectedWinnerCells[0]);
            cn.CreateSynapse(activeSegment1, previousActiveCells[0], 0.8);
            cn.CreateSynapse(activeSegment1, previousActiveCells[1], 0.8);
            cn.CreateSynapse(activeSegment1, previousActiveCells[2], 0.8);

            DistalDendrite activeSegment2 = cn.CreateDistalSegment(expectedWinnerCells[1]);
            cn.CreateSynapse(activeSegment2, previousActiveCells[0], 0.8);
            cn.CreateSynapse(activeSegment2, previousActiveCells[1], 0.8);
            cn.CreateSynapse(activeSegment2, previousActiveCells[2], 0.8);

            ComputeCycle cc = tm.Compute(previousActiveColumns, false) as ComputeCycle; // learn=false
            cc = tm.Compute(activeColumns, false) as ComputeCycle; // learn=false

            Assert.IsFalse(cc.WinnerCells.SequenceEqual(new LinkedHashSet<Cell>(expectedWinnerCells)));
        }
    }
}
// files
