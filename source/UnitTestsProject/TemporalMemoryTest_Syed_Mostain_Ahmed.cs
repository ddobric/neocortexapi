// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest_Syed_Mostain_Ahmed
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
        public void Compute_ExternalPredictiveInputsActive_ReturnsExpectedResult()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            var activeColumns = new int[] { 1, 2, 3 };
            var learn = true;
            var externalPredictiveInputsActive = new int[] { 4, 5, 6 };
            var externalPredictiveInputsWinners = new int[] { 7, 8, 9 };

            var result = tm.Compute(activeColumns, learn, externalPredictiveInputsActive, externalPredictiveInputsWinners);
            Console.WriteLine(result);
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// TestActiveCellCount: Verify that the number of active cells in the 
        /// output of Temporal Memory Algorithm is less than or equal to the maximum 
        /// number of active cells allowed per column.
        /// </summary>
        [TestMethod]
        public void TestActiveCellCount()
        {
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

            //var sequence1 = new int[] { 0, 10, 20, 30, 40, 50, 60 };
            //var sequence2 = new int[] { 5, 15, 25, 35, 45, 55 };

            //var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 40, 50, 60 };
            //var seq2ActiveColumns = new int[] { 5, 15, 25, 35, 45, 55 };

            var sequence1 = new int[] { 0, 1, 2, 3 };
            var sequence2 = new int[] { 4, 5, 6, 7 };

            var seq1ActiveColumns = new int[] { 0 };
            var seq2ActiveColumns = new int[] { 1 };

            // Learn the sequences multiple times
            for (int i = 0; i < 10; i++)
            {
                tm.Compute(seq1ActiveColumns, true);
                tm.Compute(seq2ActiveColumns, true);
            }

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);
            TestContext.WriteLine("recall1 ===>>>>> "+string.Join(",", recall1.ActiveCells.Select(c => c.Index)));
            TestContext.WriteLine("sequence1  >>>>>>>>>>>>  " + string.Join(",", sequence1));
            Assert.IsTrue(recall1.ActiveCells.Select(c => c.Index).SequenceEqual(sequence1));

            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);
            TestContext.WriteLine("recall2 ===>>>>> " + string.Join(",", recall2.ActiveCells));
            TestContext.WriteLine("sequence1 ==>>>>> " + string.Join(",", sequence2));
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).SequenceEqual(sequence2));
        }

        /// <summary>
        /// Test the growth of a new dendrite segment when no matching segments are found
        /// </summary>
        [TestMethod]
        public void TestNewSegmentGrowthWhenNoMatchingSegmentFound()
        {
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

    }
}
