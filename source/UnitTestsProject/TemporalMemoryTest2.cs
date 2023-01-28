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
    public class TemporalMemoryTest2
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

    }
}
