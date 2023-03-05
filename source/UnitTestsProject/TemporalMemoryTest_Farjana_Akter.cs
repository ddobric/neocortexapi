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
        // Test 1
        [TestMethod]
        public void TestBurstUnpredictedColumns1()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0 };
            var burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4 });

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
        }

        
        //Test 2
        [TestMethod]
        public void TestBurstUnpredictedColumns2()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 2 };
            var burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4 });

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            Assert.IsFalse(cc.ActiveCells.SequenceEqual(burstingCells));
        }
        //Test 3
        [TestMethod]
        public void TestZeroActiveColumns02()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { 0 };
            Cell cell4 = cn.GetCell(4);

            DistalDendrite activeSegment = cn.CreateDistalSegment(cell4);
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(3), 0.5);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsFalse(cc.ActiveCells.Count == 0);
            Assert.IsFalse(cc.WinnerCells.Count == 0);
            Assert.IsFalse(cc.PredictiveCells.Count == 0);

            int[] zeroColumns = new int[0];
            ComputeCycle cc2 = tm.Compute(zeroColumns, true) as ComputeCycle;
            Assert.IsTrue(cc2.ActiveCells.Count == 0);
            Assert.IsTrue(cc2.WinnerCells.Count == 0);
            Assert.IsTrue(cc2.PredictiveCells.Count == 0);
        }
     
  

        [TestMethod]
        public void Compute_ActiveColumns_ReturnsExpectedResult()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);
            // Arrange

            var activeColumns = new int[] { 1, 2, 3 };
            var learn = true;

            // Act
            var result = tm.Compute(activeColumns, learn);

            // Assert
            Assert.IsNotNull(result);
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
            Assert.IsFalse(recall1.ActiveCells.Select(c => c.Index).SequenceEqual(sequence1));

            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);
            Assert.IsFalse(recall2.ActiveCells.Select(c => c.Index).SequenceEqual(sequence2));
        }
    }
}
