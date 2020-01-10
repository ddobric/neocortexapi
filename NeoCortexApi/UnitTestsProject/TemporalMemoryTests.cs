using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using NeoCortexApi.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace UnitTestsProject
{
    [TestClass]
    public class TemporalPoolerTest
    {
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


        private Parameters getDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters() : p;
            retVal.Set(key, value);

            return retVal;
        }


        private T deepCopyPlain<T>(T obj)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }

            //JsonSerializerSettings jss = new Newtonsoft.Json.JsonSerializerSettings();

            //string serObj = JsonConvert.SerializeObject(obj);
            //return JsonConvert.DeserializeObject<T>(serObj);
        }

        [TestMethod]
        public void testActivateCorrectlyPredictiveCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

            // Cell4 belongs to column with index 1.
            Cell cell4 = cn.getCell(4);

            // ISet<Cell> expectedActiveCells = Stream.of(cell4).collect(Collectors.toSet());
            ISet<Cell> expectedActiveCells = new HashSet<Cell>(new Cell[] { cell4 });

            // We add distal dentrite at column1.cell4
            DistalDendrite activeSegment = cn.CreateDistalSegment(cell4);

            //
            // We add here synapses between column0.cells[0-3] and segment.
            cn.createSynapse(activeSegment, cn.getCell(0), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(1), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(2), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(3), 0.5);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(cc.predictiveCells.SequenceEqual(expectedActiveCells));

            ComputeCycle cc2 = tm.Compute(activeColumns, true) as ComputeCycle;
            Assert.IsTrue(cc2.ActiveCells.SequenceEqual(expectedActiveCells));
        }

        [TestMethod]
        public void TestBurstUnpredictedColumns()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            int[] activeColumns = { 0 };
            ISet<Cell> burstingCells = cn.getCellSet(new int[] { 0, 1, 2, 3 });

            ComputeCycle cc = tm.Compute( activeColumns, true) as ComputeCycle;

            Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
        }


        [TestMethod]
        public void TestZeroActiveColumns()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            Cell cell4 = cn.getCell(4);

            DistalDendrite activeSegment = cn.CreateDistalSegment(cell4);
            cn.createSynapse(activeSegment, cn.getCell(0), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(1), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(2), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(3), 0.5);

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;
            Assert.IsFalse(cc.ActiveCells.Count == 0);
            Assert.IsFalse(cc.WinnerCells.Count == 0);
            Assert.IsFalse(cc.predictiveCells.Count == 0);

            int[] zeroColumns = new int[0];
            ComputeCycle cc2 = tm.Compute( zeroColumns, true) as ComputeCycle;
            Assert.IsTrue(cc2.ActiveCells.Count == 0);
            Assert.IsTrue(cc2.WinnerCells.Count == 0);
            Assert.IsTrue(cc2.predictiveCells.Count == 0);
        }

        [TestMethod]
        public void TestPredictedActiveCellsAreAlwaysWinners()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            List<Cell> expectedWinnerCells = new List<Cell>(cn.getCellSet(new int[] { 4, 6 }));

            DistalDendrite activeSegment1 = cn.CreateDistalSegment(expectedWinnerCells[0]);
            cn.createSynapse(activeSegment1, previousActiveCells[0], 0.5);
            cn.createSynapse(activeSegment1, previousActiveCells[1], 0.5);
            cn.createSynapse(activeSegment1, previousActiveCells[2], 0.5);

            DistalDendrite activeSegment2 = cn.CreateDistalSegment(expectedWinnerCells[1]);
            cn.createSynapse(activeSegment2, previousActiveCells[0], 0.5);
            cn.createSynapse(activeSegment2, previousActiveCells[1], 0.5);
            cn.createSynapse(activeSegment2, previousActiveCells[2], 0.5);

            ComputeCycle cc = tm.Compute( previousActiveColumns, false) as ComputeCycle; // learn=false
            cc = tm.Compute( activeColumns, false) as ComputeCycle; // learn=false

            Assert.IsTrue(cc.WinnerCells.SequenceEqual(new LinkedHashSet<Cell>(expectedWinnerCells)));
        }


        [TestMethod]
        public void TestReinforcedCorrectlyActiveSegments()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PERMANENCE_DECREMENT, 0.08);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            Cell activeCell = cn.getCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(activeCell);
            Synapse as1 = cn.createSynapse(activeSegment, previousActiveCells[0], 0.5);
            Synapse as2 = cn.createSynapse(activeSegment, previousActiveCells[1], 0.5);
            Synapse as3 = cn.createSynapse(activeSegment, previousActiveCells[2], 0.5);
            Synapse is1 = cn.createSynapse(activeSegment, cn.getCell(81), 0.5);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true) ;

            Assert.AreEqual(0.6, as1.getPermanence(), 0.1);
            Assert.AreEqual(0.6, as2.getPermanence(), 0.1);
            Assert.AreEqual(0.6, as3.getPermanence(), 0.1);
            Assert.AreEqual(0.42, is1.getPermanence(), 0.001);
        }

        [TestMethod]
        public void TestReinforcedSelectedMatchingSegmentInBurstingColumn()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            Cell[] burstingCells = { cn.getCell(4), cn.getCell(5) };

            DistalDendrite activeSegment = cn.CreateDistalSegment(burstingCells[0]);
            Synapse as1 = cn.createSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse as2 = cn.createSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse as3 = cn.createSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse is1 = cn.createSynapse(activeSegment, cn.getCell(81), 0.3);

            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(burstingCells[1]);
            cn.createSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            cn.createSynapse(otherMatchingSegment, previousActiveCells[1], 0.3);
            cn.createSynapse(otherMatchingSegment, cn.getCell(81), 0.3);

            tm.Compute( previousActiveColumns, true);
            tm.Compute( activeColumns, true);

            Assert.AreEqual(0.4, as1.getPermanence(), 0.01);
            Assert.AreEqual(0.4, as2.getPermanence(), 0.01);
            Assert.AreEqual(0.4, as3.getPermanence(), 0.01);
            Assert.AreEqual(0.22, is1.getPermanence(), 0.001);
        }

        [TestMethod]
        public void testNoChangeToNonSelectedMatchingSegmentsInBurstingColumn()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            Cell[] burstingCells = { cn.getCell(4), cn.getCell(5) };

            DistalDendrite selectedMatchingSegment = cn.CreateDistalSegment(burstingCells[0]);
            cn.createSynapse(selectedMatchingSegment, previousActiveCells[0], 0.3);
            cn.createSynapse(selectedMatchingSegment, previousActiveCells[1], 0.3);
            cn.createSynapse(selectedMatchingSegment, previousActiveCells[2], 0.3);
            cn.createSynapse(selectedMatchingSegment, cn.getCell(81), 0.3);

            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(burstingCells[1]);
            Synapse as1 = cn.createSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            Synapse as2 = cn.createSynapse(otherMatchingSegment, previousActiveCells[1], 0.3);
            Synapse is1 = cn.createSynapse(otherMatchingSegment, cn.getCell(81), 0.3);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(0.3, as1.getPermanence(), 0.01);
            Assert.AreEqual(0.3, as2.getPermanence(), 0.01);
            Assert.AreEqual(0.3, is1.getPermanence(), 0.01);
        }

        [TestMethod]
        public void testNoChangeToMatchingSegmentsInPredictedActiveColumn()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            Cell expectedActiveCell = cn.getCell(4);
            List<Cell> expectedActiveCells = new List<Cell>(new Cell[] { expectedActiveCell });
            Cell otherBurstingCell = cn.getCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.createSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[2], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[3], 0.5);

            DistalDendrite matchingSegmentOnSameCell = cn.CreateDistalSegment(expectedActiveCell);
            Synapse s1 = cn.createSynapse(matchingSegmentOnSameCell, previousActiveCells[0], 0.3);
            Synapse s2 = cn.createSynapse(matchingSegmentOnSameCell, previousActiveCells[1], 0.3);

            DistalDendrite matchingSegmentOnOtherCell = cn.CreateDistalSegment(otherBurstingCell);
            Synapse s3 = cn.createSynapse(matchingSegmentOnOtherCell, previousActiveCells[0], 0.3);
            Synapse s4 = cn.createSynapse(matchingSegmentOnOtherCell, previousActiveCells[1], 0.3);

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(cc.predictiveCells.SequenceEqual(expectedActiveCells));
            tm.Compute( activeColumns, true) ;

            Assert.AreEqual(0.3, s1.getPermanence(), 0.01);
            Assert.AreEqual(0.3, s2.getPermanence(), 0.01);
            Assert.AreEqual(0.3, s3.getPermanence(), 0.01);
            Assert.AreEqual(0.3, s4.getPermanence(), 0.01);
        }


        [TestMethod]
        public void testNoNewSegmentIfNotEnoughWinnerCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            p.apply(cn);
            tm.init(cn);

            int[] zeroColumns = { };
            int[] activeColumns = { 0 };

            tm.Compute(zeroColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(0, cn.numSegments(), 0);
        }

        [TestMethod]
        public void testNewSegmentAddSynapsesToSubsetOfWinnerCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 2);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0, 1, 2 };
            int[] activeColumns = { 4 };

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;

            ISet<Cell> prevWinnerCells = cc.WinnerCells;
            Assert.AreEqual(3, prevWinnerCells.Count);

            cc = tm.Compute( activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(1, winnerCells.Count);

            List<DistalDendrite> segments = winnerCells[0].getSegments(cn);
            //List<DistalDendrite> segments = winnerCells[0].Segments;
            Assert.AreEqual(1, segments.Count);

            List<Synapse> synapses = cn.getSynapses(segments[0]);
            Assert.AreEqual(2, synapses.Count);

            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.21, synapse.getPermanence(), 0.01);
                Assert.IsTrue(prevWinnerCells.Contains(synapse.getPresynapticCell()));
            }
        }

        [TestMethod]
        public void testNewSegmentAddSynapsesToAllWinnerCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0, 1, 2 };
            int[] activeColumns = { 4 };

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevWinnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(3, prevWinnerCells.Count);

            cc = tm.Compute( activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(1, winnerCells.Count);
            List<DistalDendrite> segments = winnerCells[0].getSegments(cn);
            //List<DistalDendrite> segments = winnerCells[0].Segments;
            Assert.AreEqual(1, segments.Count);
            List<Synapse> synapses = segments[0].getAllSynapses(cn);

            List<Cell> presynapticCells = new List<Cell>();
            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.21, synapse.getPermanence(), 0.01);
                presynapticCells.Add(synapse.getPresynapticCell());
            }

            presynapticCells.Sort();

            Assert.IsTrue(prevWinnerCells.SequenceEqual(presynapticCells));
        }

        [TestMethod]
        public void testMatchingSegmentAddSynapsesToSubsetOfWinnerCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0, 1, 2, 3 };
            ISet<Cell> prevWinnerCells = cn.getCellSet(new int[] { 0, 1, 2, 3 });
            int[] activeColumns = { 4 };

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.getCell(4));
            cn.createSynapse(matchingSegment, cn.getCell(0), 0.5);

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(cc.WinnerCells.SequenceEqual(prevWinnerCells));
            cc = tm.Compute( activeColumns, true) as ComputeCycle;

            List<Synapse> synapses = cn.getSynapses(matchingSegment);
            Assert.AreEqual(3, synapses.Count);

            synapses.Sort();
            foreach (Synapse synapse in synapses)
            {
                if (synapse.getPresynapticCell().Index == 0) continue;

                Assert.AreEqual(0.21, synapse.getPermanence(), 0.01);
                Assert.IsTrue(synapse.getPresynapticCell().Index == 1 ||
                           synapse.getPresynapticCell().Index == 2 ||
                           synapse.getPresynapticCell().Index == 3);
            }
        }


        [TestMethod]
        public void testMatchingSegmentAddSynapsesToAllWinnerCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0, 1 };
            ISet<Cell> prevWinnerCells = cn.getCellSet(new int[] { 0, 1 });
            int[] activeColumns = { 4 };

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.getCell(4));
            cn.createSynapse(matchingSegment, cn.getCell(0), 0.5);

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(cc.WinnerCells.SequenceEqual(prevWinnerCells));

            cc = tm.Compute( activeColumns, true) as ComputeCycle;

            List<Synapse> synapses = cn.getSynapses(matchingSegment);
            Assert.AreEqual(2, synapses.Count);

            synapses.Sort();

            foreach (Synapse synapse in synapses)
            {
                if (synapse.getPresynapticCell().Index == 0) continue;

                Assert.AreEqual(0.21, synapse.getPermanence(), 0.01);
                Assert.AreEqual(1, synapse.getPresynapticCell().Index);
            }
        }

        /**
         * When a segment becomes active, grow synapses to previous winner cells.
         *
         * The number of grown synapses is calculated from the "matching segment"
         * overlap, not the "active segment" overlap.
         */
        [TestMethod]
        public void testActiveSegmentGrowSynapsesAccordingToPotentialOverlap()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p = getDefaultParameters(p, KEY.ACTIVATION_THRESHOLD, 2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);
            tm.init(cn);

            // Use 1 cell per column so that we have easy control over the winner cells.
            int[] previousActiveColumns = { 0, 1, 2, 3, 4 };
            List<Cell> prevWinnerCells = new List<Cell>(new Cell[] { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3), cn.getCell(4) });

            int[] activeColumns = { 5 };

            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.getCell(5));
            cn.createSynapse(activeSegment, cn.getCell(0), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(1), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(2), 0.2);

            ComputeCycle cc = tm.Compute( previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(prevWinnerCells.SequenceEqual(cc.WinnerCells));
            cc = tm.Compute( activeColumns, true) as ComputeCycle;

            List<Cell> presynapticCells = new List<Cell>();
            foreach (var syn in activeSegment.getAllSynapses(cn))
            {
                presynapticCells.Add(syn.getPresynapticCell());
            }

            //= cn.getSynapses(activeSegment).stream()
            //.map(s->s.getPresynapticCell())
            //.collect(Collectors.toSet());

            Assert.IsTrue(
                presynapticCells.Count == 4 && (
                (presynapticCells.Contains(cn.getCell(0)) && presynapticCells.Contains(cn.getCell(1)) && presynapticCells.Contains(cn.getCell(2)) && presynapticCells.Contains(cn.getCell(3))) ||
                (presynapticCells.Contains(cn.getCell(0)) && presynapticCells.Contains(cn.getCell(1)) && presynapticCells.Contains(cn.getCell(2)) && presynapticCells.Contains(cn.getCell(4)))));
        }

        [TestMethod]

        public void testDestroyWeakSynapseOnWrongPrediction()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.getCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.createSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[2], 0.5);
            // Weak Synapse
            cn.createSynapse(activeSegment, previousActiveCells[3], 0.015);

            tm.Compute( previousActiveColumns, true);
            tm.Compute( activeColumns, true);

            Assert.AreEqual(3, cn.GetNumSynapses(activeSegment));
        }

        [TestMethod]
        public void testDestroyWeakSynapseOnActiveReinforce()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.getCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.createSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.createSynapse(activeSegment, previousActiveCells[2], 0.5);
            // Weak Synapse
            cn.createSynapse(activeSegment, previousActiveCells[3], 0.009);

            tm.Compute( previousActiveColumns, true);
            tm.Compute( activeColumns, true);

            Assert.AreEqual(3, cn.GetNumSynapses(activeSegment));
        }

        [TestMethod]
        public void testRecycleWeakestSynapseToMakeRoomForNewSynapse()
        {
         
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 30);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 100 });
            //p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p = getDefaultParameters(p, KEY.PERMANENCE_INCREMENT, 0.02);
            p = getDefaultParameters(p, KEY.PERMANENCE_DECREMENT, 0.02);
            p.Set(KEY.MAX_SYNAPSES_PER_SEGMENT, 3);
            p.apply(cn);
            tm.init(cn);

            Assert.AreEqual(3, cn.getMaxSynapsesPerSegment());

            int[] prevActiveColumns = { 0, 1, 2 };
            ISet<Cell> prevWinnerCells = cn.getCellSet(new int[] { 0, 1, 2 });
            int[] activeColumns = { 4 };

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.getCell(4));
            cn.createSynapse(matchingSegment, cn.getCell(81), 0.6);
            // Weakest Synapse
            cn.createSynapse(matchingSegment, cn.getCell(0), 0.11);

            ComputeCycle cc = tm.Compute( prevActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(prevWinnerCells.SequenceEqual(cc.WinnerCells));
            tm.Compute( activeColumns, true) ;

            List<Synapse> synapses = cn.getSynapses(matchingSegment);
            Assert.AreEqual(3, synapses.Count);
            //Set<Cell> presynapticCells = synapses.stream().map(s->s.getPresynapticCell()).collect(Collectors.toSet());
            List<Cell> presynapticCells = new List<Cell>();
            foreach (var syn in cn.getSynapses(matchingSegment))
            {
                presynapticCells.Add(syn.getPresynapticCell());
            }

            Assert.IsFalse(presynapticCells.Count(c => c.Index == 0) > 0);

            //Assert.IsFalse(presynapticCells.stream().mapToInt(cell->cell.getIndex()).anyMatch(i->i == 0));
        }

        [TestMethod]
        public void testRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.5);
            p = getDefaultParameters(p, KEY.PERMANENCE_INCREMENT, 0.02);
            p = getDefaultParameters(p, KEY.PERMANENCE_DECREMENT, 0.02);
            p.Set(KEY.MAX_SEGMENTS_PER_CELL, 2);
            p.apply(cn);
            tm.init(cn);

            int[] prevActiveColumns1 = { 0, 1, 2 };
            int[] prevActiveColumns2 = { 3, 4, 5 };
            int[] prevActiveColumns3 = { 6, 7, 8 };
            int[] activeColumns = { 9 };
            Cell cell9 = cn.getCell(9);

            tm.Compute( prevActiveColumns1, true);
            tm.Compute( activeColumns, true);

            Assert.AreEqual(1, cn.getSegments(cell9).Count);
            DistalDendrite oldestSegment = cn.getSegments(cell9)[0];
            tm.reset(cn);
            tm.Compute(prevActiveColumns2, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(2, cn.getSegments(cell9).Count);

            //Set<Cell> oldPresynaptic = cn.getSynapses(oldestSegment)
            //    .stream()
            //    .map(s->s.getPresynapticCell())
            //    .collect(Collectors.toSet());

            var oldPresynaptic = cn.getSynapses(oldestSegment).Select(s => s.getPresynapticCell()).ToList();

            tm.reset(cn);
            tm.Compute( prevActiveColumns3, true);
            tm.Compute( activeColumns, true);
            Assert.AreEqual(2, cn.getSegments(cell9).Count);

            // Verify none of the segments are connected to the cells the old
            // segment was connected to.

            foreach (DistalDendrite segment in cn.getSegments(cell9))
            {
                //Set<Cell> newPresynaptic = cn.getSynapses(segment)
                //    .stream()
                //    .map(s->s.getPresynapticCell())
                //    .collect(Collectors.toSet());
                var newPresynaptic = cn.getSynapses(segment).Select(s => s.getPresynapticCell()).ToList();


                Assert.IsTrue(areDisjoined<Cell>(oldPresynaptic, newPresynaptic));
            }
        }


        [TestMethod]
        public void TestDestroySegmentsWithTooFewSynapsesToBeMatching()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, .2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.init(cn);

            int[] prevActiveColumns = { 0 };
            Cell[] prevActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.getCell(5);

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.getCell(5));
            cn.createSynapse(matchingSegment, prevActiveCells[0], .015);
            cn.createSynapse(matchingSegment, prevActiveCells[1], .015);
            cn.createSynapse(matchingSegment, prevActiveCells[2], .015);
            cn.createSynapse(matchingSegment, prevActiveCells[3], .015);

            tm.Compute( prevActiveColumns, true);
            tm.Compute( activeColumns, true);

            Assert.AreEqual(0, cn.numSegments(expectedActiveCell));
        }

        [TestMethod]
        public void testPunishMatchingSegmentsInInactiveColumns()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.init(cn);

            int[] prevActiveColumns = { 0 };
            Cell[] prevActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            int[] activeColumns = { 1 };
            Cell previousInactiveCell = cn.getCell(81);

            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.getCell(42));
            Synapse as1 = cn.createSynapse(activeSegment, prevActiveCells[0], .5);
            Synapse as2 = cn.createSynapse(activeSegment, prevActiveCells[1], .5);
            Synapse as3 = cn.createSynapse(activeSegment, prevActiveCells[2], .5);
            Synapse is1 = cn.createSynapse(activeSegment, previousInactiveCell, .5);

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.getCell(43));
            Synapse as4 = cn.createSynapse(matchingSegment, prevActiveCells[0], .5);
            Synapse as5 = cn.createSynapse(matchingSegment, prevActiveCells[1], .5);
            Synapse is2 = cn.createSynapse(matchingSegment, previousInactiveCell, .5);

            tm.Compute( prevActiveColumns, true);
            tm.Compute( activeColumns, true);

            Assert.AreEqual(0.48, as1.getPermanence(), 0.01);
            Assert.AreEqual(0.48, as2.getPermanence(), 0.01);
            Assert.AreEqual(0.48, as3.getPermanence(), 0.01);
            Assert.AreEqual(0.48, as4.getPermanence(), 0.01);
            Assert.AreEqual(0.48, as5.getPermanence(), 0.01);
            Assert.AreEqual(0.50, is1.getPermanence(), 0.01);
            Assert.AreEqual(0.50, is2.getPermanence(), 0.01);
        }

        [TestMethod]
        public void testAddSegmentToCellWithFewestSegments()
        {
            bool grewOnCell1 = false;
            bool grewOnCell2 = false;

            for (int seed = 0; seed < 100; seed++)
            {
                TemporalMemory tm = new TemporalMemory();
                Connections cn = new Connections();
                Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
                p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
                p = getDefaultParameters(p, KEY.SEED, seed);
                p.apply(cn);
                tm.init(cn);

                int[] prevActiveColumns = { 1, 2, 3, 4 };
                Cell[] prevActiveCells = { cn.getCell(4), cn.getCell(5), cn.getCell(6), cn.getCell(7) };
                int[] activeColumns = { 0 };
                Cell[] nonMatchingCells = { cn.getCell(0), cn.getCell(3) };
                ISet<Cell> activeCells = cn.getCellSet(new int[] { 0, 1, 2, 3 });

                DistalDendrite segment1 = cn.CreateDistalSegment(nonMatchingCells[0]);
                cn.createSynapse(segment1, prevActiveCells[0], 0.5);
                DistalDendrite segment2 = cn.CreateDistalSegment(nonMatchingCells[1]);
                cn.createSynapse(segment2, prevActiveCells[1], 0.5);

                tm.Compute( prevActiveColumns, true);
                ComputeCycle cc = tm.Compute( activeColumns, true)as ComputeCycle;

                Assert.IsTrue(cc.ActiveCells.SequenceEqual(activeCells));

                Assert.AreEqual(3, cn.numSegments());
                Assert.AreEqual(1, cn.numSegments(cn.getCell(0)));
                Assert.AreEqual(1, cn.numSegments(cn.getCell(3)));
                Assert.AreEqual(1, cn.GetNumSynapses(segment1));
                Assert.AreEqual(1, cn.GetNumSynapses(segment2));

                List<DistalDendrite> segments = new List<DistalDendrite>(cn.getSegments(cn.getCell(1)));
                if (segments.Count == 0)
                {
                    List<DistalDendrite> segments2 = cn.getSegments(cn.getCell(2));
                    Assert.IsFalse(segments2.Count == 0);
                    grewOnCell2 = true;
                    segments.AddRange(segments2);
                }
                else
                {
                    grewOnCell1 = true;
                }

                Assert.AreEqual(1, segments.Count);
                List<Synapse> synapses = segments[0].getAllSynapses(cn);
                Assert.AreEqual(4, synapses.Count);

                ISet<Column> columnCheckList = cn.getColumnSet(prevActiveColumns);

                foreach (Synapse synapse in synapses)
                {
                    Assert.AreEqual(0.2, synapse.getPermanence(), 0.01);

                    var parentColIndx = synapse.getPresynapticCell().getParentColumnIndex();
                    Column column = cn.getMemory().GetColumn(parentColIndx);
                    Assert.IsTrue(columnCheckList.Contains(column));
                    columnCheckList.Remove(column);
                }

                Assert.AreEqual(0, columnCheckList.Count);
            }

            Assert.IsTrue(grewOnCell1);
            Assert.IsTrue(grewOnCell2);
        }

        [TestMethod]
        [TestCategory("Tests with Serialization Issue")]
        public void testConnectionsNeverChangeWhenLearningDisabled()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p = getDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.2);
            p.apply(cn);
            tm.init(cn);

            int[] prevActiveColumns = { 0 };
            Cell[] prevActiveCells = { cn.getCell(0), cn.getCell(1), cn.getCell(2), cn.getCell(3) };
            int[] activeColumns = { 1, 2 };
            Cell prevInactiveCell = cn.getCell(81);
            Cell expectedActiveCell = cn.getCell(4);

            DistalDendrite correctActiveSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.createSynapse(correctActiveSegment, prevActiveCells[0], 0.5);
            cn.createSynapse(correctActiveSegment, prevActiveCells[1], 0.5);
            cn.createSynapse(correctActiveSegment, prevActiveCells[2], 0.5);

            DistalDendrite wrongMatchingSegment = cn.CreateDistalSegment(cn.getCell(43));
            cn.createSynapse(wrongMatchingSegment, prevActiveCells[0], 0.5);
            cn.createSynapse(wrongMatchingSegment, prevActiveCells[1], 0.5);
            cn.createSynapse(wrongMatchingSegment, prevInactiveCell, 0.5);

            var r = deepCopyPlain<Synapse>(cn.getReceptorSynapseMapping().Values.First().First());
            var synMapBefore = deepCopyPlain<Dictionary<Cell, LinkedHashSet<Synapse>>>(cn.getReceptorSynapseMapping());
            var segMapBefore = deepCopyPlain<Dictionary<Cell, List<DistalDendrite>>>(cn.getSegmentMapping());

            tm.Compute( prevActiveColumns, false);
            tm.Compute( activeColumns, false);

            Assert.IsTrue(synMapBefore != cn.getReceptorSynapseMapping());
            Assert.IsTrue(synMapBefore.Keys.SequenceEqual(cn.getReceptorSynapseMapping().Keys));

            Assert.IsTrue(segMapBefore != cn.getSegmentMapping());
            Assert.IsTrue(segMapBefore.Keys.SequenceEqual( cn.getSegmentMapping().Keys));
        }

        public void testLeastUsedCell()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 2 });
            p = getDefaultParameters(p, KEY.CELLS_PER_COLUMN, 2);
            p.apply(cn);
            tm.init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.getCell(0));
            cn.createSynapse(dd, cn.getCell(3), 0.3);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(1, tm.GetLeastUsedCell(cn, cn.getColumn(0).Cells, cn.getRandom()).Index);
            }
        }

        [TestMethod]
        public void testAdaptSegment()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.getCell(0));
            Synapse s1 = cn.createSynapse(dd, cn.getCell(23), 0.6);
            Synapse s2 = cn.createSynapse(dd, cn.getCell(37), 0.4);
            Synapse s3 = cn.createSynapse(dd, cn.getCell(477), 0.9);

            tm.adaptSegment(cn, dd, cn.getCellSet(new int[] { 23, 37 }), cn.getPermanenceIncrement(), cn.getPermanenceDecrement());

            Assert.AreEqual(0.7, s1.getPermanence(), 0.01);
            Assert.AreEqual(0.5, s2.getPermanence(), 0.01);
            Assert.AreEqual(0.8, s3.getPermanence(), 0.01);
        }

        [TestMethod]
        public void testAdaptSegmentToMax()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.getCell(0));
            Synapse s1 = cn.createSynapse(dd, cn.getCell(23), 0.9);

            tm.adaptSegment(cn, dd, cn.getCellSet(new int[] { 23 }), cn.getPermanenceIncrement(), cn.getPermanenceDecrement());
            Assert.AreEqual(1.0, s1.getPermanence(), 0.1);

            // Now permanence should be at max
            tm.adaptSegment(cn, dd, cn.getCellSet(new int[] { 23 }), cn.getPermanenceIncrement(), cn.getPermanenceDecrement());
            Assert.AreEqual(1.0, s1.getPermanence(), 0.1);
        }

        [TestMethod]
        public void testAdaptSegmentToMin()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.getCell(0));
            Synapse s1 = cn.createSynapse(dd, cn.getCell(23), 0.1);
            cn.createSynapse(dd, cn.getCell(1), 0.3);

            tm.adaptSegment(cn, dd, cn.getCellSet(new int[] { }), cn.getPermanenceIncrement(), cn.getPermanenceDecrement());
            Assert.IsFalse(cn.getSynapses(dd).Contains(s1));
        }

        [TestMethod]
    public void testNumberOfColumns()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 64, 64 });
            p.Set(KEY.CELLS_PER_COLUMN, 32);
            p.apply(cn);
            tm.init(cn);

            Assert.AreEqual(64 * 64, cn.NumColumns);
        }

        [TestMethod]
        public void testNumberOfCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 64, 64 });
            p.Set(KEY.CELLS_PER_COLUMN, 32);
            p.apply(cn);
            tm.init(cn);

            Assert.AreEqual(64 * 64 * 32, cn.getCells().Length);
        }
    }
}
