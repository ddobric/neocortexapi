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
        public TestInfo TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound()
        {
            // Initialize
            TestInfo result = new TestInfo
            {
                TestName = "TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound",
                IsPassing = true
            };

            try
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
            catch (Exception ex)
            {
                // Handle any exceptions
                // You can log the exception details if needed
                result.IsPassing = false;
                Console.WriteLine($"Test failed: {ex.Message}");
            }

            return result;
        }

        public TestInfo TestSynapsePermanenceUpdateWhenMatchingSegmentsFound()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestSynapsePermanenceUpdateWhenMatchingSegmentsFound",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };

            try
            {
        // Initialize
                TemporalMemory tm = new TemporalMemory();
                Connections cn = new Connections();
                Parameters p = getDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08);
                p.apply(cn);
                tm.Init(cn);

                // Test logic is here
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

                // Verify the synapse permanence
                Assert.AreEqual(0.3, as1.Permanence, 0.01);
                Assert.AreEqual(0.3, is1.Permanence, 0.01);

        // If the test completes without errors, set IsPassing to true
            result.IsPassing = true;
            }
            catch (Exception ex)
            {
                result.IsPassing = false; // Test failed
            }

            return result;
        }

        public TestInfo TestColumnDimensions()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestColumnDimensions",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try {
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

                // Assert the test condition
                Assert.AreEqual(32 * 64 * 32, cnt);
            } catch (Exception ex) { 
                result.IsPassing = false;
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                // You can log the exception details if needed
                Console.WriteLine($"Test failed: {ex.Message}");
            }

            return result;
        }

        public TestInfo TestRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment(int[] prevActiveColumns1, int[] prevActiveColumns2, int[] prevActiveColumns3, int[] activeColumns)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try {
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
            catch(Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestNewSegmentAddSynapsesToAllWinnerCells(int numPrevActiveCols, int numActiveCols)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestNewSegmentAddSynapsesToAllWinnerCells",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try {
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
            catch(Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }


        public TestInfo TestDestroyWeakSynapseOnWrongPrediction(Double weakSynapsePermanence)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestDestroyWeakSynapseOnWrongPrediction",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try {
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
            catch(Exception ex)
            {
                result.IsPassing= false;
            }
            return result;
        }

        public TestInfo TestAddSegmentToCellWithFewestSegments(int seed, int expectedNumSegments)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestAddSegmentToCellWithFewestSegments",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestAdaptSegmentToMax(double initialPermanence, double expectedPermanence)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestAdaptSegmentToMax",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestDestroySegmentsWithTooFewSynapsesToBeMatching(int c1, int c2, int c3, int c4,
                                                        double p1, double p2, double p3, double p4, int expectedNumSegments)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestDestroySegmentsWithTooFewSynapsesToBeMatching",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestPunishMatchingSegmentsInInactiveColumns(double as1Permanence, double as2Permanence,
            double as3Permanence, double as4Permanence, double as5Permanence, double is1Permanence,
            double expectedAs1Permanence, double expectedAs2Permanence, double expectedAs3Permanence,
            double expectedAs4Permanence, double expectedAs5Permanence,
            double expectedIs1Permanence, double expectedIs2Permanence)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestPunishMatchingSegmentsInInactiveColumns",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestHighSparsitySequenceLearningAndRecall()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestHighSparsitySequenceLearningAndRecall",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestLowSparsitySequenceLearningAndRecall()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestLowSparsitySequenceLearningAndRecall",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestCreateSynapseInDistalSegment()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestCreateSynapseInDistalSegment",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        
        public TestInfo TestNewSegmentGrowthWhenNoMatchingSegmentFound()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestNewSegmentGrowthWhenNoMatchingSegmentFound",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestNoOverlapInActiveCells()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestNoOverlapInActiveCells",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap(int[] previousActiveColumns, int[] activeColumns, int[] expectedPresynapticCells, int expectedCount)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestDestroyWeakSynapseOnActiveReinforce(int prevActive, int active, int expectedSynapseCount)
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestDestroyWeakSynapseOnActiveReinforce",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestAdaptSegment_IncreasePermanence()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestAdaptSegment_IncreasePermanence",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

        public TestInfo TestAdaptSegment_PrevActiveCellsContainPresynapticCell_IncreasePermanence()
        {
            TestInfo result = new TestInfo
            {
                TestName = "TestAdaptSegment_PrevActiveCellsContainPresynapticCell_IncreasePermanence",
                IsPassing = true // Assume passing, modify based on the actual test logic
            };
            try
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
            catch (Exception ex)
            {
                result.IsPassing = false;
            }
            return result;
        }

    }

    public class TestInfo
    {
        public string TestName { get; set; }
        public bool IsPassing { get; set; }
    }


}
