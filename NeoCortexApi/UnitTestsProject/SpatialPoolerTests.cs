using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerTest
    {
        private Parameters parameters;
        private SpatialPooler sp;
        private Connections mem;

        public void setupParameters()
        {
            parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 5 });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
            parameters.Set(KEY.POTENTIAL_RADIUS, 5);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
        }

        public void setupDefaultParameters()
        {
            parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 32, 32 });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 64, 64 });
            parameters.Set(KEY.POTENTIAL_RADIUS, 16);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 10.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.10);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.SEED, 42);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
        }

        private void initSP()
        {
            sp = new SpatialPoolerMT();
            mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
        }


        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void confirmSPConstruction()
        {
            setupParameters();

            initSP();

            Assert.AreEqual(5, mem.getInputDimensions()[0]);
            Assert.AreEqual(5, mem.getColumnDimensions()[0]);
            Assert.AreEqual(5, mem.getPotentialRadius());
            Assert.AreEqual(0.5, mem.getPotentialPct());//, 0);
            Assert.AreEqual(false, mem.GlobalInhibition);
            Assert.AreEqual(-1.0, mem.LocalAreaDensity);//, 0);
            Assert.AreEqual(3, mem.NumActiveColumnsPerInhArea);//, 0);
            Assert.IsTrue(Math.Abs(1 - mem.StimulusThreshold) <= 1);
            Assert.AreEqual(0.01, mem.getSynPermInactiveDec());//, 0);
            Assert.AreEqual(0.1, mem.getSynPermActiveInc());//, 0);
            Assert.AreEqual(0.1, mem.getSynPermConnected());//, 0);
            Assert.AreEqual(0.1, mem.getMinPctOverlapDutyCycles());//, 0);
            Assert.AreEqual(0.1, mem.getMinPctActiveDutyCycles());//, 0);
            Assert.AreEqual(10, mem.getDutyCyclePeriod());//, 0);
            Assert.AreEqual(10.0, mem.getMaxBoost());//, 0);
            Assert.AreEqual(42, mem.getSeed());

            Assert.AreEqual(5, mem.NumInputs);
            Assert.AreEqual(5, mem.NumColumns);
        }



        /**
         * Checks that feeding in the same input vector leads to polarized
         * permanence values: either zeros or ones, but no fractions
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testCompute1()
        {
            setupParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 9 });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
            parameters.setPotentialRadius(5);

            //This is 0.3 in Python version due to use of dense 
            // permanence instead of sparse (as it should be)
            parameters.setPotentialPct(0.5);

            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(3);
            parameters.setStimulusThreshold(1);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.1);
            parameters.setMinPctActiveDutyCycles(0.1);
            parameters.setDutyCyclePeriod(10);
            parameters.setMaxBoost(10);
            parameters.setSynPermTrimThreshold(0);

            //This is 0.5 in Python version due to use of dense 
            // permanence instead of sparse (as it should be)
            parameters.setPotentialPct(1);

            parameters.setSynPermConnected(0.1);

            sp = new SpatialPooler();
            mem = new Connections();
            parameters.apply(mem);

            SpatialPoolerMock mock = new SpatialPoolerMock(new int[] { 0, 1, 2, 3, 4 });
            mock.init(mem);

            int[] inputVector = new int[] { 1, 0, 1, 0, 1, 0, 0, 1, 1 };
            int[] activeArray = new int[] { 0, 0, 0, 0, 0 };
            for (int i = 0; i < 20; i++)
            {
                mock.compute(inputVector, activeArray, true);
            }

            for (int i = 0; i < mem.NumColumns; i++)
            {
                int[] permanences = ArrayUtils.toIntArray(mem.getColumn(i).ProximalDendrite.RFPool.getDensePermanences(mem.NumInputs));

                Assert.IsTrue(inputVector.SequenceEqual(permanences));
            }
        }

        /**
         * Checks that columns only change the permanence values for 
         * inputs that are within their potential pool
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testCompute2()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 10 });
            parameters.setColumnDimensions(new int[] { 5 });
            parameters.setPotentialRadius(3);
            parameters.setPotentialPct(0.3);
            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(3);
            parameters.setStimulusThreshold(1);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.1);
            parameters.setMinPctActiveDutyCycles(0.1);
            parameters.setDutyCyclePeriod(10);
            parameters.setMaxBoost(10);
            parameters.setSynPermConnected(0.1);

            mem = new Connections();
            parameters.apply(mem);

            SpatialPoolerMock mock = new SpatialPoolerMock(new int[] { 0, 1, 2, 3, 4 });
            mock.init(mem);

            int[] inputVector = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            int[] activeArray = new int[] { 0, 0, 0, 0, 0 };
            for (int i = 0; i < 20; i++)
            {
                mock.compute(inputVector, activeArray, true);
            }

            for (int i = 0; i < mem.NumColumns; i++)
            {
                int[] permanences = ArrayUtils.toIntArray(mem.getColumn(i).ProximalDendrite.RFPool.getDensePermanences(mem.NumInputs));
                //int[] potential = (int[])mem.getConnectedCounts().getSlice(i);
                int[] potential = (int[])mem.getColumn(i).ConnectedInputBits;
                Assert.IsTrue(permanences.SequenceEqual(potential));
            }
        }

        /**
         * When stimulusThreshold is 0, allow columns without any overlap to become
         * active. This test focuses on the global inhibition code path.
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testZeroOverlap_NoStimulusThreshold_GlobalInhibition()
        {
            int inputSize = 10;
            int nColumns = 20;
            parameters = Parameters.getSpatialDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { inputSize });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { nColumns });
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);// This makes column active even if no synapse is connected.
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.SEED, 42);

            SpatialPooler sp = new SpatialPooler();
            Connections cn = new Connections();
            parameters.apply(cn);
            sp.init(cn);

            int[] activeArray = new int[nColumns];
            sp.compute(new int[inputSize], activeArray, true);

            Assert.IsTrue(3 == activeArray.Count(i => i > 0));//, ArrayUtils.INT_GREATER_THAN_0).length);
        }

        /**
         * When stimulusThreshold is > 0, don't allow columns without any overlap to
         * become active. This test focuses on the global inhibition code path.
         */
        [TestMethod]
        [DataRow(PoolerMode.SingleThreaded)]
        [DataRow(PoolerMode.Multicore)]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testZeroOverlap_StimulusThreshold_GlobalInhibition(PoolerMode poolerMode)
        {
            int inputSize = 10;
            int nColumns = 20;
            parameters = Parameters.getSpatialDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { inputSize });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { nColumns });
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 1.0);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.SEED, 42);

            SpatialPooler sp = UnitTestHelpers.CreatePooler(poolerMode);
            Connections cn = new Connections();
            parameters.apply(cn);
            sp.init(cn);

            int[] activeArray = new int[nColumns];
            sp.compute(new int[inputSize], activeArray, true);

            Assert.IsTrue(0 == activeArray.Count(i => i > 0));//, ArrayUtils.INT_GREATER_THAN_0).length);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testZeroOverlap_NoStimulusThreshold_LocalInhibition()
        {
            int inputSize = 10;
            int nColumns = 20;
            parameters = Parameters.getSpatialDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { inputSize });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { nColumns });
            parameters.Set(KEY.POTENTIAL_RADIUS, 5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 1.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.SEED, 42);

            SpatialPooler sp = new SpatialPooler();
            Connections cn = new Connections();
            parameters.apply(cn);
            sp.init(cn);

            // This exact number of active columns is determined by the inhibition
            // radius, which changes based on the random synapses (i.e. weird math).
            // Force it to a known number.
            cn.InhibitionRadius = 2;

            int[] activeArray = new int[nColumns];
            sp.compute(new int[inputSize], activeArray, true);

            Assert.IsTrue(6 == activeArray.Count(i => i > 0));//, ArrayUtils.INT_GREATER_THAN_0).length);
        }

        /**
         * When stimulusThreshold is > 0, don't allow columns without any overlap to
         * become active. This test focuses on the local inhibition code path.
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testZeroOverlap_StimulusThreshold_LocalInhibition()
        {
            int inputSize = 10;
            int nColumns = 20;
            parameters = Parameters.getSpatialDefaultParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { inputSize });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { nColumns });
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 1.0);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.SEED, 42);

            SpatialPooler sp = new SpatialPooler();
            Connections cn = new Connections();
            parameters.apply(cn);
            sp.init(cn);

            int[] activeArray = new int[nColumns];
            sp.compute(new int[inputSize], activeArray, true);

            Assert.IsTrue(0 == activeArray.Count(i => i > 0));//, ArrayUtils.INT_GREATER_THAN_0).length);
        }

        // DD
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testOverlapsOutput()
        {
            parameters = Parameters.getSpatialDefaultParameters();
            parameters.setColumnDimensions(new int[] { 3 });
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setPotentialRadius(5);
            parameters.setNumActiveColumnsPerInhArea(5);
            parameters.setGlobalInhibition(true);
            parameters.setSynPermActiveInc(0.1);
            parameters.setSynPermInactiveDec(0.1);
            parameters.setSeed(42);
            parameters.setStimulusThreshold(4);
            parameters.setRandom(new ThreadSafeRandom(42));

            var sp = new SpatialPoolerMT();
            Connections cn = new Connections();
            parameters.apply(cn);
            sp.init(cn);

            cn.BoostFactors = (new double[] { 2.0, 2.0, 2.0 });
            int[] inputVector = { 1, 1, 1, 1, 1 };
            int[] activeArray = { 0, 0, 0 };
            int[] expOutput = { 4, 4, 4 }; // Added during implementation of parallel.
                                           /*{ 1, 1, 1 }*/
            ;
            // { 2, 1, 0 }; This was used originally on Linux with JAVA and Pyhton
            sp.compute(inputVector, activeArray, true);

            double[] boostedOverlaps = cn.BoostedOverlaps;
            int[] overlaps = cn.Overlaps;

            for (int i = 0; i < cn.NumColumns; i++)
            {
                Assert.AreEqual(expOutput[i], overlaps[i]);
                Assert.IsTrue(Math.Abs(expOutput[i] * 2 - boostedOverlaps[i]) <= 0.01);
            }
        }


        // This throws OUT OF MEMORY.
        // [TestMethod]
        // [TestCategory("LongRunning")]
        public void perfTest()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 188, 1 });
            parameters.setColumnDimensions(new int[] { 2048, 10 });
            parameters.setPotentialRadius(94);
            parameters.setPotentialPct(0.5);
            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(10);
            parameters.setNumActiveColumnsPerInhArea(40);
            parameters.setStimulusThreshold(0);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.001);
            parameters.setMinPctActiveDutyCycles(0.001);
            parameters.setDutyCyclePeriod(1000);
            parameters.setMaxBoost(10);
            initSP();

            int[] inputVector = {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
        };

            int[] activeArray = new int[2048];

            sp.compute(inputVector, activeArray, true);

        }

        /**
         * Given a specific input and initialization params the SP should return this
         * exact output.
         *
         * Previously output varied between platforms (OSX/Linux etc) == (in Python)
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testExactOutput()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 1, 188 });
            parameters.setColumnDimensions(new int[] { 2048, 1 });
            parameters.setPotentialRadius(94);
            parameters.setPotentialPct(0.5);
            parameters.setGlobalInhibition(true);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(40);
            parameters.setStimulusThreshold(0);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.001);
            parameters.setMinPctActiveDutyCycles(0.001);
            parameters.setDutyCyclePeriod(1000);
            parameters.setMaxBoost(10);
            initSP();

            int[] inputVector = {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
        };

            int[] activeArray = new int[2048];

            sp.compute(inputVector, activeArray, true);

            int[] real = activeArray.IndexWhere(i => i > 0).ToArray();

            int[] expected1 = new int[] {
             74, 203, 237, 270, 288, 317, 479, 529, 530, 622, 659, 720, 757, 790, 924, 956, 1033,
             1041, 1112, 1332, 1386, 1430, 1500, 1517, 1578, 1584, 1651, 1664, 1717, 1735, 1747,
             1748, 1775, 1779, 1788, 1813, 1888, 1911, 1938, 1958 };

            // On Windows .NET. Depends on Random.
            int[] expected2 = new int[] { 209, 212, 271, 300, 319, 402, 412, 464, 486, 530, 533, 651, 668, 742, 761, 804, 838, 865, 1412, 1421, 1456, 1460, 1510, 1546, 1628, 1671, 1729, 1749, 1796, 1826, 1837, 1914, 1941, 1953, 1971, 1982, 1983, 1984, 1991, 2047 };

            Assert.IsTrue(expected1.SequenceEqual(real) || expected2.SequenceEqual(real));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testStripNeverLearned()
        {
            setupParameters();
            parameters.setColumnDimensions(new int[] { 6 });
            parameters.setInputDimensions(new int[] { 9 });
            initSP();

            // Column [2] has dutycycle=0. It means it has never learned anything.
            mem.updateActiveDutyCycles(new double[] { 0.5, 0.1, 0, 0.2, 0.4, 0 });
            int[] activeColumns = new int[] { 0, 1, 2, 4 };
            int[] stripped = sp.stripUnlearnedColumns(mem, activeColumns);
            int[] trueStripped = new int[] { 0, 1, 4 };
            Assert.IsTrue(trueStripped.SequenceEqual(stripped));

            mem.updateActiveDutyCycles(new double[] { 0.9, 0, 0, 0, 0.4, 0.3 });
            activeColumns = ArrayUtils.Range(0, 6);
            stripped = sp.stripUnlearnedColumns(mem, activeColumns);
            trueStripped = new int[] { 0, 4, 5 };
            Assert.IsTrue(trueStripped.SequenceEqual(stripped));

            mem.updateActiveDutyCycles(new double[] { 0, 0, 0, 0, 0, 0 });
            activeColumns = ArrayUtils.Range(0, 6);
            stripped = sp.stripUnlearnedColumns(mem, activeColumns);
            trueStripped = new int[] { };
            Assert.IsTrue(trueStripped.SequenceEqual(stripped));

            mem.updateActiveDutyCycles(new double[] { 1, 1, 1, 1, 1, 1 });
            activeColumns = ArrayUtils.Range(0, 6);
            stripped = sp.stripUnlearnedColumns(mem, activeColumns);
            trueStripped = ArrayUtils.Range(0, 6);
            Assert.IsTrue(trueStripped.SequenceEqual(stripped));
        }


        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testMapColumn()
        {
            // Test 1D
            setupParameters();
            parameters.setColumnDimensions(new int[] { 4 });
            parameters.setInputDimensions(new int[] { 12 });
            initSP();

            Assert.IsTrue(1 == HtmCompute.MapColumn(0, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(4 == HtmCompute.MapColumn(1, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(7 == HtmCompute.MapColumn(2, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(10 == HtmCompute.MapColumn(3, mem.ColumnTopology, mem.InputTopology));

            // Test 1D with same dimension of columns and inputs
            setupParameters();
            parameters.setColumnDimensions(new int[] { 4 });
            parameters.setInputDimensions(new int[] { 4 });
            initSP();

            Assert.IsTrue(0 == HtmCompute.MapColumn(0, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(1 == HtmCompute.MapColumn(1, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(2 == HtmCompute.MapColumn(2, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(3 == HtmCompute.MapColumn(3, mem.ColumnTopology, mem.InputTopology));

            // Test 1D with dimensions of length 1
            setupParameters();
            parameters.setColumnDimensions(new int[] { 1 });
            parameters.setInputDimensions(new int[] { 1 });
            initSP();

            Assert.IsTrue(0 == HtmCompute.MapColumn(0, mem.ColumnTopology, mem.InputTopology));

            // Test 2D
            setupParameters();
            parameters.setColumnDimensions(new int[] { 12, 4 });
            parameters.setInputDimensions(new int[] { 36, 12 });
            initSP();

            Assert.IsTrue(13 == HtmCompute.MapColumn(0, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(49 == HtmCompute.MapColumn(4, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(52 == HtmCompute.MapColumn(5, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(58 == HtmCompute.MapColumn(7, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(418 == HtmCompute.MapColumn(47, mem.ColumnTopology, mem.InputTopology));

            // Test 2D with some input dimensions smaller than column dimensions.
            setupParameters();
            parameters.setColumnDimensions(new int[] { 4, 4 });
            parameters.setInputDimensions(new int[] { 3, 5 });
            initSP();

            Assert.IsTrue(0 == HtmCompute.MapColumn(0, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(4 == HtmCompute.MapColumn(3, mem.ColumnTopology, mem.InputTopology));
            Assert.IsTrue(14 == HtmCompute.MapColumn(15, mem.ColumnTopology, mem.InputTopology));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testMapPotential1D()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 12 });
            parameters.setColumnDimensions(new int[] { 4 });
            parameters.setPotentialRadius(2);
            parameters.setPotentialPct(1);
            parameters.Set(KEY.WRAP_AROUND, false);
            initSP();

            Assert.AreEqual(12, mem.getInputDimensions()[0]);
            Assert.AreEqual(4, mem.getColumnDimensions()[0]);
            Assert.AreEqual(2, mem.getPotentialRadius());

            // Test without wrapAround and potentialPct = 1
            int[] expected = new int[] { 0, 1, 2, 3 };
            mem.HtmConfig.IsWrapAround = false;
            mem.setWrapAround(false);
            int[] mask = HtmCompute.MapPotential(mem.HtmConfig, 0, mem.getRandom());
            Assert.IsTrue(expected.SequenceEqual(mask));

            expected = new int[] { 5, 6, 7, 8, 9 };
            mask = HtmCompute.MapPotential(mem.HtmConfig, 2, mem.getRandom());
            Assert.IsTrue(expected.SequenceEqual(mask));

            // Test with wrapAround and potentialPct = 1

            expected = new int[] { 0, 1, 2, 3, 11 };
            mem.HtmConfig.IsWrapAround = true;
            mem.setWrapAround(true);
            mask = HtmCompute.MapPotential(mem.HtmConfig, 0, mem.getRandom());
            Assert.IsTrue(expected.SequenceEqual(mask));

            expected = new int[] { 0, 8, 9, 10, 11 };
            mask = HtmCompute.MapPotential(mem.HtmConfig, 3, mem.getRandom());
            Assert.IsTrue(expected.SequenceEqual(mask));

            // Test with wrapAround and potentialPct < 1
            parameters.setPotentialPct(0.5);
            parameters.Set(KEY.WRAP_AROUND, true);
            initSP();

            int[] supersetMask = new int[] { 0, 1, 2, 3, 11 };
            mask = HtmCompute.MapPotential(mem.HtmConfig, 0, mem.getRandom());
            Assert.IsTrue(mask.Length == 3);
            List<int> unionList = new List<int>(supersetMask);
            unionList.AddRange(mask);
            int[] unionMask = ArrayUtils.unique(unionList.ToArray());
            Assert.IsTrue(unionMask.SequenceEqual(supersetMask));
        }


        /// <summary>
        /// Test for mapping of 2D columns to 2D inputs.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testMapPotential2D()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 6, 12 });
            parameters.setColumnDimensions(new int[] { 2, 4 });
            parameters.setPotentialRadius(1);
            parameters.setPotentialPct(1);
            initSP();

            //Test without wrapAround
            mem.setWrapAround(false);
            int[] mask = HtmCompute.MapPotential(mem.HtmConfig, 0, mem.getRandom());
            List<int> trueIndices = new List<int>(new int[] { 0, 1, 2, 12, 13, 14, 24, 25, 26 });
            List<int> maskSet = new List<int>(mask);
            Assert.IsTrue(trueIndices.SequenceEqual(maskSet));

            trueIndices.Clear();
            maskSet.Clear();
            trueIndices.AddRange(new int[] { 6, 7, 8, 18, 19, 20, 30, 31, 32 });
            mask = HtmCompute.MapPotential(mem.HtmConfig, 2, mem.getRandom());
            maskSet.AddRange(mask);
            Assert.IsTrue(trueIndices.SequenceEqual(maskSet));

            //Test with wrapAround
            trueIndices.Clear();
            maskSet.Clear();
            parameters.setPotentialRadius(2);
            initSP();
            trueIndices.AddRange(
                    new int[] { 0, 1, 2, 3, 11,
                        12, 13, 14, 15, 23,
                        24, 25, 26, 27, 35,
                        36, 37, 38, 39, 47,
                        60, 61, 62, 63, 71 });
            mem.setWrapAround(true);
            mask = HtmCompute.MapPotential(mem.HtmConfig, 0, mem.getRandom());
            maskSet.AddRange(mask);
            Assert.IsTrue(trueIndices.SequenceEqual(maskSet));

            trueIndices.Clear();
            maskSet.Clear();
            trueIndices.AddRange(
                    new int[] { 0, 8, 9, 10, 11,
                        12, 20, 21, 22, 23,
                        24, 32, 33, 34, 35,
                        36, 44, 45, 46, 47,
                        60, 68, 69, 70, 71 });
            mask = HtmCompute.MapPotential(mem.HtmConfig, 3, mem.getRandom());
            maskSet.AddRange(mask);
            Assert.IsTrue(trueIndices.SequenceEqual(maskSet));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testMapPotential1Column1Input()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 1 });
            parameters.setColumnDimensions(new int[] { 1 });
            parameters.setPotentialRadius(2);
            parameters.setPotentialPct(1);
            parameters.Set(KEY.WRAP_AROUND, false);
            initSP();

            //Test without wrapAround and potentialPct = 1
            int[] expectedMask = new int[] { 0 };
            int[] mask = HtmCompute.MapPotential(mem.HtmConfig, 0, mem.getRandom());
            List<int> trueIndices = new List<int>(expectedMask);
            List<int> maskSet = new List<int>(mask);

            Assert.IsTrue(trueIndices.SequenceEqual(maskSet));
            // The *position* of the one "on" bit expected. 
            // Python version returns [1] which is the on bit in the zero'th position
            //Assert.IsTrue(trueIndices.Equals(maskSet));
        }

        //////////////////////////////////////////////////////////////
        /**
         * Local test apparatus for {@link #testInhibitColumns()}
         */
        bool globalCalled = false;
        bool localCalled = false;
        double _density = 0;
        public void reset()
        {
            this.globalCalled = false;
            this.localCalled = false;
            this._density = 0;
        }
        public void setGlobalCalled(bool b)
        {
            this.globalCalled = b;
        }
        public void setLocalCalled(bool b)
        {
            this.localCalled = b;
        }
        //////////////////////////////////////////////////////////////


        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testInhibitColumns()
        {
            setupParameters();
            parameters.setColumnDimensions(new int[] { 5 });
            parameters.setInhibitionRadius(10);
            initSP();

            SpatialPoolerMock2 mock = new SpatialPoolerMock2((density) =>
            {
                setGlobalCalled(true);
                _density = density;
                return new int[1];
            },
            (density) =>
            {
                setLocalCalled(true);
                _density = density;
                return new int[2];
            });

            //        //Mocks to test which method gets called
            //        SpatialPooler inhibitColumnsGlobal = new SpatialPooler()
            //        {
            //        private static final long serialVersionUID = 1L;

            //    @Override public int[] inhibitColumnsGlobal(Connections c, double[] overlap, double density)
            //    {
            //        setGlobalCalled(true);
            //        _density = density;
            //        return new int[] { 1 };
            //    }
            //};


            double[] overlaps = ArrayUtils.sample(mem.NumColumns, mem.getRandom());
            mem.NumActiveColumnsPerInhArea = 5;
            mem.LocalAreaDensity = 0.1;
            mem.GlobalInhibition = true;
            mem.InhibitionRadius = 5;
            double trueDensity = mem.LocalAreaDensity;
            //inhibitColumnsGlobal.inhibitColumns(mem, overlaps);
            mock.inhibitColumns(mem, overlaps);
            Assert.IsTrue(globalCalled);
            Assert.IsTrue(!localCalled);
            Assert.IsTrue(Math.Abs(trueDensity - _density) <= .01d);

            //////
            reset();
            mem.setColumnDimensions(new int[] { 50, 10 });
            //Internally calculated during init, to overwrite we put after init
            mem.GlobalInhibition = false;
            mem.InhibitionRadius = 7;

            double[] tieBreaker = new double[500];
            ArrayUtils.fillArray(tieBreaker, 0);
            mem.setTieBreaker(tieBreaker);
            overlaps = ArrayUtils.sample(mem.NumColumns, mem.getRandom());
            //inhibitColumnsLocal.inhibitColumns(mem, overlaps);
            mock.inhibitColumns(mem, overlaps);
            trueDensity = mem.LocalAreaDensity;
            Assert.IsTrue(!globalCalled);
            Assert.IsTrue(localCalled);
            Assert.IsTrue(Math.Abs(trueDensity - _density) <= .01d);

            //////
            reset();
            parameters.setInputDimensions(new int[] { 100, 10 });
            parameters.setColumnDimensions(new int[] { 100, 10 });
            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(-1);
            parameters.setNumActiveColumnsPerInhArea(3);
            initSP();

            //Internally calculated during init, to overwrite we put after init
            mem.InhibitionRadius = 4;
            tieBreaker = new double[1000];
            ArrayUtils.fillArray(tieBreaker, 0);
            mem.setTieBreaker(tieBreaker);
            overlaps = ArrayUtils.sample(mem.NumColumns, mem.getRandom());
            //inhibitColumnsLocal.inhibitColumns(mem, overlaps);
            mock.inhibitColumns(mem, overlaps);
            trueDensity = 3.0 / 81.0;
            Assert.IsTrue(!globalCalled);
            Assert.IsTrue(localCalled);
            Assert.IsTrue(Math.Abs(trueDensity - _density) <= .01d);

            //////
            reset();
            mem.NumActiveColumnsPerInhArea = 7;

            //Internally calculated during init, to overwrite we put after init
            mem.InhibitionRadius = 1;
            tieBreaker = new double[1000];
            ArrayUtils.fillArray(tieBreaker, 0);
            mem.setTieBreaker(tieBreaker);
            overlaps = ArrayUtils.sample(mem.NumColumns, mem.getRandom());
            //inhibitColumnsLocal.inhibitColumns(mem, overlaps);
            mock.inhibitColumns(mem, overlaps);
            trueDensity = 0.5;
            Assert.IsTrue(!globalCalled);
            Assert.IsTrue(localCalled);
            Assert.IsTrue(Math.Abs(trueDensity - _density) <= .01d);

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testUpdateBoostFactors()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5/*Don't care*/ });
            parameters.setColumnDimensions(new int[] { 5 });
            parameters.setMaxBoost(10.0);
            parameters.setRandom(new ThreadSafeRandom(42));
            initSP();

            mem.setNumColumns(6);

            double[] minActiveDutyCycles = new double[6];
            ArrayUtils.fillArray(minActiveDutyCycles, 0.000001D);
            mem.setMinActiveDutyCycles(minActiveDutyCycles);

            double[] activeDutyCycles = new double[] { 0.1, 0.3, 0.02, 0.04, 0.7, 0.12 };
            mem.setActiveDutyCycles(activeDutyCycles);

            double[] trueBoostFactors = new double[] { 1, 1, 1, 1, 1, 1 };
            sp.UpdateBoostFactors(mem);
            double[] boostFactors = mem.BoostFactors;
            for (int i = 0; i < boostFactors.Length; i++)
            {
                Assert.IsTrue(Math.Abs(trueBoostFactors[i] - boostFactors[i]) <= 0.1D);
            }

            ////////////////
            minActiveDutyCycles = new double[] { 0.1, 0.3, 0.02, 0.04, 0.7, 0.12 };
            mem.setMinActiveDutyCycles(minActiveDutyCycles);
            ArrayUtils.fillArray(mem.BoostFactors, 0);
            sp.UpdateBoostFactors(mem);
            boostFactors = mem.BoostFactors;
            for (int i = 0; i < boostFactors.Length; i++)
            {
                Assert.IsTrue(Math.Abs(trueBoostFactors[i] - boostFactors[i]) <= 0.1D);
            }

            ////////////////
            minActiveDutyCycles = new double[] { 0.1, 0.2, 0.02, 0.03, 0.7, 0.12 };
            mem.setMinActiveDutyCycles(minActiveDutyCycles);
            activeDutyCycles = new double[] { 0.01, 0.02, 0.002, 0.003, 0.07, 0.012 };
            mem.setActiveDutyCycles(activeDutyCycles);
            trueBoostFactors = new double[] { 9.1, 9.1, 9.1, 9.1, 9.1, 9.1 };
            sp.UpdateBoostFactors(mem);
            boostFactors = mem.BoostFactors;
            for (int i = 0; i < boostFactors.Length; i++)
            {
                Assert.IsTrue(Math.Abs(trueBoostFactors[i] - boostFactors[i]) <= 0.1D);
            }

            ////////////////
            minActiveDutyCycles = new double[] { 0.1, 0.2, 0.02, 0.03, 0.7, 0.12 };
            mem.setMinActiveDutyCycles(minActiveDutyCycles);
            ArrayUtils.fillArray(activeDutyCycles, 0);
            mem.setActiveDutyCycles(activeDutyCycles);
            ArrayUtils.fillArray(trueBoostFactors, 10.0);
            sp.UpdateBoostFactors(mem);
            boostFactors = mem.BoostFactors;
            for (int i = 0; i < boostFactors.Length; i++)
            {
                Assert.IsTrue(Math.Abs(trueBoostFactors[i] - boostFactors[i]) <= 0.1D);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testUpdateInhibitionRadius()
        {
            setupParameters();
            initSP();

            //Test global inhibition case
            mem.GlobalInhibition = true;
            mem.setColumnDimensions(new int[] { 57, 31, 2 });
            // If global inhibition is set, then all columns in the row are inhibited.
            sp.updateInhibitionRadius(mem);
            Assert.IsTrue(57 == mem.InhibitionRadius);

            ////////////
            SpatialPoolerMock3 mock = new SpatialPoolerMock3(3, 4);
            //        // ((3 * 4) - 1) / 2 => round up
            //        SpatialPooler mock = new SpatialPooler()
            //        {
            //        private static final long serialVersionUID = 1L;
            //    public double avgConnectedSpanForColumnND(Connections c, int columnIndex)
            //    {
            //        return 3;
            //    }

            //    public double avgColumnsPerInput(Connections c)
            //    {
            //        return 4;
            //    }
            //};
            mem.GlobalInhibition = false;
            sp = mock;
            sp.updateInhibitionRadius(mem);
            Assert.IsTrue(6 == mem.InhibitionRadius);

            //////////////

            //Test clipping at 1.0
            mock = new SpatialPoolerMock3(0.5, 1.2);
            //            mock = new SpatialPooler()
            //    {
            //            private static final long serialVersionUID = 1L;
            //    public double avgConnectedSpanForColumnND(Connections c, int columnIndex)
            //    {
            //        return 0.5;
            //    }

            //    public double avgColumnsPerInput(Connections c)
            //    {
            //        return 1.2;
            //    }
            //};
            mem.GlobalInhibition = false;
            sp = mock;
            sp.updateInhibitionRadius(mem);
            Assert.IsTrue(1 == mem.InhibitionRadius);

            /////////////
            mock = new SpatialPoolerMock3(2.4, 2);
            //        //Test rounding up
            //        mock = new SpatialPooler()
            //        {
            //        private static final long serialVersionUID = 1L;
            //    public double avgConnectedSpanForColumnND(Connections c, int columnIndex)
            //    {
            //        return 2.4;
            //    }

            //    public double avgColumnsPerInput(Connections c)
            //    {
            //        return 2;
            //    }
            //};
            mem.GlobalInhibition = false;
            sp = mock;
            //((2 * 2.4) - 1) / 2.0 => round up
            sp.updateInhibitionRadius(mem);
            Assert.IsTrue(2 == mem.InhibitionRadius);

            //...
            sp = new SpatialPoolerMT();

            mem.GlobalInhibition = true;

            sp.updateInhibitionRadius(mem);

            // max dim of columns
            Assert.IsTrue(57 == mem.InhibitionRadius);

            // TODO..
            sp = mock;
            mem.GlobalInhibition = false;

            mem.setInputDimensions(new int[] { 5, 10, 2 });
            sp.updateInhibitionRadius(mem);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testAvgColumnsPerInput()
        {
            setupParameters();
            initSP();

            mem.setColumnDimensions(new int[] { 2, 2, 2, 2 });
            mem.setInputDimensions(new int[] { 4, 4, 4, 4 });
            Assert.IsTrue(0.5 == sp.calcAvgColumnsPerInput(mem));

            mem.setColumnDimensions(new int[] { 2, 2, 2, 2 });
            mem.setInputDimensions(new int[] { 7, 5, 1, 3 });
            double trueAvgColumnPerInput = (2.0 / 7 + 2.0 / 5 + 2.0 / 1 + 2 / 3.0) / 4.0d;
            Assert.IsTrue(trueAvgColumnPerInput == sp.calcAvgColumnsPerInput(mem));

            mem.setColumnDimensions(new int[] { 3, 3 });
            mem.setInputDimensions(new int[] { 3, 3 });
            trueAvgColumnPerInput = 1;
            Assert.IsTrue(trueAvgColumnPerInput == sp.calcAvgColumnsPerInput(mem));

            mem.setColumnDimensions(new int[] { 25 });
            mem.setInputDimensions(new int[] { 5 });
            trueAvgColumnPerInput = 5;
            Assert.IsTrue(trueAvgColumnPerInput == sp.calcAvgColumnsPerInput(mem));

            mem.setColumnDimensions(new int[] { 3, 3, 3, 5, 5, 6, 6 });
            mem.setInputDimensions(new int[] { 3, 3, 3, 5, 5, 6, 6 });
            trueAvgColumnPerInput = 1;
            Assert.IsTrue(trueAvgColumnPerInput == sp.calcAvgColumnsPerInput(mem));

            mem.setColumnDimensions(new int[] { 3, 6, 9, 12 });
            mem.setInputDimensions(new int[] { 3, 3, 3, 3 });
            trueAvgColumnPerInput = 2.5;
            Assert.IsTrue(trueAvgColumnPerInput == sp.calcAvgColumnsPerInput(mem));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testAvgConnectedSpanForColumnND()
        {
            sp = new SpatialPooler();
            mem = new Connections();

            int[] inputDimensions = new int[] { 4, 4, 2, 5 };
            mem.setInputDimensions(inputDimensions);
            mem.setColumnDimensions(new int[] { 5 });
            sp.InitMatrices(mem, null);

            List<int> connected = new List<int>();
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 1, 0, 1, 0 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 1, 0, 1, 1 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 3, 2, 1, 0 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 3, 0, 1, 0 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 1, 0, 1, 3 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 2, 2, 1, 0 }, false));
            //connected.sort(0, connected.size());
            connected = connected.OrderBy(i => i).ToList();

            //[ 45  46  48 105 125 145]
            //mem.getConnectedSynapses().set(0, connected.toArray());
            // mem.getPotentialPools().set(0, new Pool(6, mem.NumInputs));
            mem.getColumn(0).ProximalDendrite.RFPool = new Pool(6, mem.NumInputs);
            mem.getColumn(0).setProximalConnectedSynapsesForTest(mem, connected.ToArray());

            connected.Clear();
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 2, 0, 1, 0 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 2, 0, 0, 0 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 3, 0, 0, 0 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 3, 0, 1, 0 }, false));
            //connected.sort(0, connected.size());
            connected = connected.OrderBy(i => i).ToList();

            //[ 80  85 120 125]
            //mem.getConnectedSynapses().set(1, connected.toArray());
            //mem.getPotentialPools().set(1, new Pool(4, mem.NumInputs));
            mem.getColumn(1).ProximalDendrite.RFPool = new Pool(4, mem.NumInputs);
            mem.getColumn(1).setProximalConnectedSynapsesForTest(mem, connected.ToArray());

            connected.Clear();
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 0, 0, 1, 4 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 0, 0, 0, 3 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 0, 0, 0, 1 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 1, 0, 0, 2 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 0, 0, 1, 1 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 3, 3, 1, 1 }, false));
            connected = connected.OrderBy(i => i).ToList();
            //connected.sort(0, connected.size());

            //[  1   3   6   9  42 156]
            //mem.getConnectedSynapses().set(2, connected.toArray());
            //mem.getPotentialPools().set(2, new Pool(4, mem.NumInputs));
            mem.getColumn(2).ProximalDendrite.RFPool = new Pool(4, mem.NumInputs);
            mem.getColumn(2).setProximalConnectedSynapsesForTest(mem, connected.ToArray());

            connected.Clear();
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 3, 3, 1, 4 }, false));
            connected.Add(mem.getInputMatrix().computeIndex(new int[] { 0, 0, 0, 0 }, false));
            //connected.sort(0, connected.size());
            connected = connected.OrderBy(i => i).ToList();

            //[  0 159]
            //mem.getConnectedSynapses().set(3, connected.toArray());
            //mem.getPotentialPools().set(3, new Pool(4, mem.NumInputs));
            mem.getColumn(3).ProximalDendrite.RFPool = new Pool(4, mem.NumInputs);
            mem.getColumn(3).setProximalConnectedSynapsesForTest(mem, connected.ToArray());

            //[]
            connected.Clear();
            //mem.getPotentialPools().set(4, new Pool(4, mem.NumInputs));
            mem.getColumn(4).ProximalDendrite.RFPool = new Pool(4, mem.NumInputs);
            mem.getColumn(4).setProximalConnectedSynapsesForTest(mem, connected.ToArray());

            double[] trueAvgConnectedSpan = new double[] { 11.0 / 4d, 6.0 / 4d, 14.0 / 4d, 15.0 / 4d, 0d };
            for (int i = 0; i < mem.NumColumns; i++)
            {
                double connectedSpan = HtmCompute.CalcAvgSpanOfConnectedSynapses(mem.getColumn(i), mem.HtmConfig);
                Assert.IsTrue(trueAvgConnectedSpan[i] == connectedSpan);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testBumpUpWeakColumns()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 8 });
            parameters.setColumnDimensions(new int[] { 5 });
            initSP();

            mem.setSynPermBelowStimulusInc(0.01);
            mem.setSynPermTrimThreshold(0.05);
            mem.setOverlapDutyCycles(new double[] { 0, 0.009, 0.1, 0.001, 0.002 });
            mem.setMinOverlapDutyCycles(new double[] { .01, .01, .01, .01, .01 });



            int[][] potentialPools = new int[][] {
                new int[] { 1, 1, 1, 1, 0, 0, 0, 0 },
                new int[] { 1, 0, 0, 0, 1, 1, 0, 1 },
                new int[] { 0, 0, 1, 0, 1, 1, 1, 0 },
                new int[] { 1, 1, 1, 0, 0, 0, 1, 0 },
                new int[] { 1, 1, 1, 1, 1, 1, 1, 1 }
            };

            double[][] permanences = new double[][] {
                new double[] { 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
                new double[] { 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
                new double[] { 0.000, 0.000, 0.014, 0.000, 0.032, 0.044, 0.110, 0.000 },
                new double[] { 0.041, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 },
                new double[] { 0.100, 0.738, 0.045, 0.002, 0.050, 0.008, 0.208, 0.034 }
            };

            double[][] truePermanences = new double[][] {
            new double[] { 0.210, 0.130, 0.100, 0.000, 0.000, 0.000, 0.000, 0.000 },
            new double[] { 0.160, 0.000, 0.000, 0.000, 0.190, 0.130, 0.000, 0.460 },
            new double[] { 0.000, 0.000, 0.014, 0.000, 0.032, 0.044, 0.110, 0.000 },
            new double[]  { 0.051, 0.000, 0.000, 0.000, 0.000, 0.000, 0.188, 0.000 },
            new double[] { 0.110, 0.748, 0.055, 0.000, 0.060, 0.000, 0.218, 0.000 }
        };

            //    Condition <?> cond = new Condition.Adapter<Integer>()
            //    {
            //    public boolean eval(int n)
            //    {
            //        return n == 1;
            //    }
            //};

            for (int i = 0; i < mem.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], n => n == 1);

                // int[] indexes = ArrayUtils.where(potentialPools[i], cond);
                mem.getColumn(i).setProximalConnectedSynapsesForTest(mem, indexes);
                mem.getColumn(i).setPermanences(mem.HtmConfig, permanences[i]);
            }

            //Execute method being tested
            sp.BumpUpWeakColumns(mem);

            for (int i = 0; i < mem.NumColumns; i++)
            {
                //double[] perms = mem.getPotentialPools().get(i).getDensePermanences(mem);
                double[] perms = mem.getColumn(i).ProximalDendrite.RFPool.getDensePermanences(mem.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void RandomGenMultithreadTest()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom();
            Parallel.For(0, 100, new ParallelOptions(), (i) =>
            {
                Console.WriteLine(rnd.Next());
            });


            Console.WriteLine("-----------");


            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(rnd.Next());
            };
        }

        /// <summary>
        /// Ensures that two calls to mapPotential() gives different results.
        /// This is because of a Random generator.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void TestMapPotentialUndeterminismus()
        {
            int[][] expectedList = new int[2][];

            expectedList[0] = new int[] { 2047, 0, 1 };
            expectedList[1] = new int[] { 2046, 2047, 0 };

            setupParameters();
            parameters.setInputDimensions(new int[] { 10 });
            parameters.setColumnDimensions(new int[] { 5 });
            parameters.setPotentialRadius(94);
            parameters.setPotentialPct(0.5);
            parameters.setGlobalInhibition(true);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(40);
            parameters.setStimulusThreshold(0);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.001);
            parameters.setMinPctActiveDutyCycles(0.001);
            parameters.setDutyCyclePeriod(1000);
            parameters.setMaxBoost(10);
            parameters.setRandom(new ThreadSafeRandom(42));
            initSP();

            mem.InhibitionRadius = 1;

            for (int i = 0; i < mem.NumColumns; i++)
            {
                foreach (var syn in mem.getColumn(i).ProximalDendrite.Synapses)
                {
                    Console.WriteLine($"{i} - {syn.SynapseIndex} - [{String.Join("", "", syn.InputIndex)}]");
                }

                //parameters.setRandom(new Random(42));

                //int[] potential1 = sp.mapPotential(mem, i, mem.isWrapAround());
                //Console.WriteLine($"{i} - [{String.Join(",", potential1)}]");

                //parameters.setRandom(new Random(42));

                //int[] potential2 = sp.mapPotential(mem, i, mem.isWrapAround());
                //Console.WriteLine($"{i} - [{String.Join(",", potential2)}]");

                //// Can be same or different.
                //Assert.IsTrue(potential1.SequenceEqual(potential2) || !potential1.SequenceEqual(potential2));
            }
        }


        /// <summary>
        /// Ensures that neighborhod calculation is thread-safe.
        ///{5 - [4,5,6]}
        ///{2 - [1,2,3]}
        ///{3 - [2,3,4]}
        ///{6 - [5,6,7]}
        ///{4 - [3,4,5]}
        ///{1 - [0,1,2]}
        ///{0 - [0,1]}
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void TestParallelNeighborhood()
        {
            int[][] expectedList = new int[8][];

            expectedList[0] = new int[] { 0, 1 };
            expectedList[1] = new int[] { 0, 1, 2 };
            expectedList[2] = new int[] { 1, 2, 3 };
            expectedList[3] = new int[] { 2, 3, 4 };
            expectedList[4] = new int[] { 3, 4, 5 };
            expectedList[5] = new int[] { 4, 5, 6 };
            expectedList[6] = new int[] { 5, 6, 7 };
            expectedList[7] = new int[] { 6, 7 };

            setupDefaultParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 8 });
            parameters.Set(KEY.WRAP_AROUND, false);
            initSP();

            mem.InhibitionRadius = 1;
            int inhibitionRadius = 1;

            for (int k = 0; k < 100; k++)
            {
                Parallel.For(0, 8, (i) =>
                {
                    int[] neighborhood = HtmCompute.GetNeighborhood(i, inhibitionRadius, mem.getColumnTopology().HtmTopology);

                    Assert.IsTrue(expectedList[i].SequenceEqual(neighborhood));
                });
            }
        }


        /// <summary>
        /// Ensures that neighborhod calculation is thread-safe.
        ///{5 - [4,5,6]}
        ///{2 - [1,2,3]}
        ///{3 - [2,3,4]}
        ///{6 - [5,6,7]}
        ///{4 - [3,4,5]}
        ///{1 - [0,1,2]}
        ///{0 - [0,1]}
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void TestParallelWrappingNeighborhood()
        {
            int[][] expectedList = new int[2][];

            expectedList[0] = new int[] { 2047, 0, 1 };
            expectedList[1] = new int[] { 2046, 2047, 0 };

            setupParameters();
            parameters.setInputDimensions(new int[] { 1, 188 });
            parameters.setColumnDimensions(new int[] { 2048, 1 });
            parameters.setPotentialRadius(94);
            parameters.setPotentialPct(0.5);
            parameters.setGlobalInhibition(true);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(40);
            parameters.setStimulusThreshold(0);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.001);
            parameters.setMinPctActiveDutyCycles(0.001);
            parameters.setDutyCyclePeriod(1000);
            parameters.setMaxBoost(10);
            initSP();

            mem.InhibitionRadius = 1;
            int inhibitionRadius = 1;

            for (int k = 0; k < 100; k++)
            {
                Parallel.For(0, 2048, (i) =>
                //for (int i = 0; i < 2048; i++)
                {
                    int[] neighborhood = HtmCompute.GetWrappingNeighborhood(i, inhibitionRadius, mem.getColumnTopology().HtmTopology);

                    if (i == 0)
                        Assert.IsTrue(expectedList[0].SequenceEqual(neighborhood));
                    else if (i == 2047)
                        Assert.IsTrue(expectedList[1].SequenceEqual(neighborhood));
                    else
                    {
                        Assert.IsTrue(neighborhood[0] == i - 1);
                        Assert.IsTrue(neighborhood[1] == i);
                        Assert.IsTrue(neighborhood[2] == i + 1);
                    }
                });
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testUpdateMinDutyCycleLocal()
        {
            setupDefaultParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 8 });
            parameters.Set(KEY.WRAP_AROUND, false);
            initSP();

            mem.InhibitionRadius = 1;
            mem.setOverlapDutyCycles(new double[] { 0.7, 0.1, 0.5, 0.01, 0.78, 0.55, 0.1, 0.001 });
            mem.setActiveDutyCycles(new double[] { 0.9, 0.3, 0.5, 0.7, 0.1, 0.01, 0.08, 0.12 });
            mem.setMinPctActiveDutyCycles(0.1);
            mem.setMinPctOverlapDutyCycles(0.2);
            sp.updateMinDutyCyclesLocal(mem);

            double[] resultMinActiveDutyCycles = mem.getMinActiveDutyCycles();
            double[] expected0 = { 0.09, 0.09, 0.07, 0.07, 0.07, 0.01, 0.012, 0.012 };

            for (var i = 0; i < expected0.Length; i++)
            {
                Console.WriteLine($"{i}: {expected0[i]}-{resultMinActiveDutyCycles[i]}\t = {expected0[i] - resultMinActiveDutyCycles[i]} | {expected0[i] - resultMinActiveDutyCycles[i] <= 0.01}"); Assert.IsTrue(Math.Abs(expected0[i] - resultMinActiveDutyCycles[i]) <= 0.01);
            }
            //IntStream.range(0, expected0.length)
            //    .forEach(i->assertEquals(expected0[i], resultMinActiveDutyCycles[i], 0.01));

            double[] resultMinOverlapDutyCycles = mem.getMinOverlapDutyCycles();
            double[] expected1 = new double[] { 0.14, 0.14, 0.1, 0.156, 0.156, 0.156, 0.11, 0.02 };

            for (var i = 0; i < expected1.Length; i++)
            {
                Assert.IsTrue(Math.Abs(expected1[i] - resultMinOverlapDutyCycles[i]) <= 0.01, $"At position: {i} - exp: {expected1[i]} - READ: {resultMinOverlapDutyCycles[i]}");
            }

            //IntStream.range(0, expected1.length)
            //    .forEach(i->assertEquals(expected1[i], resultMinOverlapDutyCycles[i], 0.01));

            // wrapAround = true
            setupDefaultParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 8 });
            parameters.Set(KEY.WRAP_AROUND, true);
            initSP();

            mem.InhibitionRadius = 1;
            mem.setOverlapDutyCycles(new double[] { 0.7, 0.1, 0.5, 0.01, 0.78, 0.55, 0.1, 0.001 });
            mem.setActiveDutyCycles(new double[] { 0.9, 0.3, 0.5, 0.7, 0.1, 0.01, 0.08, 0.12 });
            mem.setMinPctActiveDutyCycles(0.1);
            mem.setMinPctOverlapDutyCycles(0.2);
            sp.updateMinDutyCyclesLocal(mem);

            double[] resultMinActiveDutyCycles2 = mem.getMinActiveDutyCycles();
            double[] expected2 = { 0.09, 0.09, 0.07, 0.07, 0.07, 0.01, 0.012, 0.09 };

            for (var i = 0; i < expected2.Length; i++)
            {
                Console.WriteLine($"{i} - exp: {expected2[i]} - read: {resultMinActiveDutyCycles2[i]}");
                Assert.IsTrue(Math.Abs(expected2[i] - resultMinActiveDutyCycles2[i]) <= 0.01, $"At position: {i} - exp: {expected2[i]} - READ: {resultMinActiveDutyCycles2[i]}");
            }

            //IntStream.range(0, expected2.length)
            //  .forEach(i->assertEquals(expected2[i], resultMinActiveDutyCycles2[i], 0.01));

            double[] resultMinOverlapDutyCycles2 = mem.getMinOverlapDutyCycles();
            double[] expected3 = new double[] { 0.14, 0.14, 0.1, 0.156, 0.156, 0.156, 0.11, 0.14 };

            for (var i = 0; i < expected3.Length; i++)
            {
                Assert.IsTrue(Math.Abs(expected3[i] - resultMinOverlapDutyCycles2[i]) <= 0.01, $"At position: {i} - exp: {expected2[i]} - READ: {resultMinActiveDutyCycles2[i]}");
            }

            //IntStream.range(0, expected3.length)
            //    .forEach(i->assertEquals(expected3[i], resultMinOverlapDutyCycles2[i], 0.01));

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testUpdateMinDutyCycleGlobal()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5 });

            parameters.setColumnDimensions(new int[] { 5 });
            initSP();

            mem.setMinPctOverlapDutyCycles(0.01);
            mem.setMinPctActiveDutyCycles(0.02);
            mem.setOverlapDutyCycles(new double[] { 0.06, 1, 3, 6, 0.5 });
            mem.setActiveDutyCycles(new double[] { 0.6, 0.07, 0.5, 0.4, 0.3 });

            sp.updateMinDutyCyclesGlobal(mem);
            double[] trueMinActiveDutyCycles = new double[mem.NumColumns];
            ArrayUtils.fillArray(trueMinActiveDutyCycles, 0.02 * 0.6);
            double[] trueMinOverlapDutyCycles = new double[mem.NumColumns];
            ArrayUtils.fillArray(trueMinOverlapDutyCycles, 0.01 * 6);
            for (int i = 0; i < mem.NumColumns; i++)
            {
                //          System.out.println(i + ") " + trueMinOverlapDutyCycles[i] + "  -  " +  mem.getMinOverlapDutyCycles()[i]);
                //          System.out.println(i + ") " + trueMinActiveDutyCycles[i] + "  -  " +  mem.getMinActiveDutyCycles()[i]);
                Assert.IsTrue(Math.Abs(trueMinOverlapDutyCycles[i] - mem.getMinOverlapDutyCycles()[i]) <= 0.01);
                Assert.IsTrue(Math.Abs(trueMinActiveDutyCycles[i] - mem.getMinActiveDutyCycles()[i]) <= 0.01);
            }

            mem.setMinPctOverlapDutyCycles(0.015);
            mem.setMinPctActiveDutyCycles(0.03);
            mem.setOverlapDutyCycles(new double[] { 0.86, 2.4, 0.03, 1.6, 1.5 });
            mem.setActiveDutyCycles(new double[] { 0.16, 0.007, 0.15, 0.54, 0.13 });
            sp.updateMinDutyCyclesGlobal(mem);
            ArrayUtils.fillArray(trueMinOverlapDutyCycles, 0.015 * 2.4);
            for (int i = 0; i < mem.NumColumns; i++)
            {
                //          System.out.println(i + ") " + trueMinOverlapDutyCycles[i] + "  -  " +  mem.getMinOverlapDutyCycles()[i]);
                //          System.out.println(i + ") " + trueMinActiveDutyCycles[i] + "  -  " +  mem.getMinActiveDutyCycles()[i]);
                Assert.IsTrue(Math.Abs(trueMinOverlapDutyCycles[i] - mem.getMinOverlapDutyCycles()[i]) <= 0.01);
            }

            mem.setMinPctOverlapDutyCycles(0.015);
            mem.setMinPctActiveDutyCycles(0.03);
            mem.setOverlapDutyCycles(new double[5]);
            mem.setActiveDutyCycles(new double[5]);
            sp.updateMinDutyCyclesGlobal(mem);
            ArrayUtils.fillArray(trueMinOverlapDutyCycles, 0);
            ArrayUtils.fillArray(trueMinActiveDutyCycles, 0);
            for (int i = 0; i < mem.NumColumns; i++)
            {
                //          System.out.println(i + ") " + trueMinOverlapDutyCycles[i] + "  -  " +  mem.getMinOverlapDutyCycles()[i]);
                //          System.out.println(i + ") " + trueMinActiveDutyCycles[i] + "  -  " +  mem.getMinActiveDutyCycles()[i]);
                Assert.IsTrue(Math.Abs(trueMinActiveDutyCycles[i] - mem.getMinActiveDutyCycles()[i]) <= 0.01);
                Assert.IsTrue(Math.Abs(trueMinOverlapDutyCycles[i] - mem.getMinOverlapDutyCycles()[i]) <= 0.01);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testIsUpdateRound()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 5 });
            initSP();

            mem.setUpdatePeriod(50);
            mem.setIterationNum(1);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(39);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(50);
            Assert.IsTrue(sp.isUpdateRound(mem));
            mem.setIterationNum(1009);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(1250);
            Assert.IsTrue(sp.isUpdateRound(mem));

            mem.setUpdatePeriod(125);
            mem.setIterationNum(0);
            Assert.IsTrue(sp.isUpdateRound(mem));
            mem.setIterationNum(200);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(249);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(1330);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(1249);
            Assert.IsFalse(sp.isUpdateRound(mem));
            mem.setIterationNum(1375);
            Assert.IsTrue(sp.isUpdateRound(mem));

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testAdaptSynapses()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 8 });
            parameters.setColumnDimensions(new int[] { 4 });
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            initSP();

            mem.setSynPermTrimThreshold(0.05);

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
        };

            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
        };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
        //     Inc     Dec    Dec    Inc    -      -      -      -
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
        //     Inc     -      -      -      Inc    Dec    -      Dec
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
        //      -      -     Trim    -      -      -      Inc    -
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
        //      -      -      -      -      -      -      -      -     // Only cols 0,1,2 are active 
                                                                       // (see 'activeColumns' below)
        };

            //    Condition <?> cond = new Condition.Adapter<Integer>()
            //    {
            //    public boolean eval(int n)
            //    {
            //        return n == 1;
            //    }
            //};

            for (int i = 0; i < mem.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                //    int[] indexes = ArrayUtils.where(potentialPools[i], cond);
                mem.getColumn(i).setProximalConnectedSynapsesForTest(mem, indexes);
                mem.getColumn(i).setPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.NumColumns; i++)
            {
                //double[] perms = mem.getPotentialPools().get(i).getDensePermanences(mem);
                double[] perms = mem.getColumn(i).ProximalDendrite.RFPool.getDensePermanences(mem.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }

            //////////////////////////////

            potentialPools = new int[][] {
                    new int[]{ 1, 1, 1, 0, 0, 0, 0, 0 },
                    new int[]{ 0, 1, 1, 1, 0, 0, 0, 0 },
                    new int[]{ 0, 0, 1, 1, 1, 0, 0, 0 },
                    new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
                };

            permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.000, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.000, 0.017, 0.232, 0.400, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.051, 0.730, 0.000, 0.000, 0.000 },
            new double[]{ 0.170, 0.000, 0.000, 0.000, 0.000, 0.000, 0.380, 0.000 }
        };

            truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.000, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.000, 0.000, 0.222, 0.500, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.000, 0.000, 0.000, 0.151, 0.830, 0.000, 0.000, 0.000 },
           new double[] { 0.170, 0.000, 0.000, 0.000, 0.000, 0.000, 0.380, 0.000 }
        };

            for (int i = 0; i < mem.NumColumns; i++)
            {
                //int[] indexes = potentialPools[i].Where(n => n == 1).ToArray();
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));

                //int[] indexes = ArrayUtils.where(potentialPools[i], cond);
                mem.getColumn(i).setProximalConnectedSynapsesForTest(mem, indexes);
                mem.getColumn(i).setPermanences(mem.HtmConfig, permanences[i]);
            }

            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.NumColumns; i++)
            {
                double[] perms = mem.getColumn(i).ProximalDendrite.RFPool.getDensePermanences(mem.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testRaisePermanenceThreshold()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 5 });
            parameters.setSynPermConnected(0.1);
            parameters.setStimulusThreshold(3);
            parameters.setSynPermBelowStimulusInc(0.01);
            //The following parameter is not set to "1" in the Python version
            //This is necessary to reproduce the test conditions of having as
            //many pool members as Input Bits, which would never happen under
            //normal circumstances because we want to enforce sparsity
            parameters.setPotentialPct(1);

            initSP();

            //We set the values on the Connections permanences here just for illustration
            SparseObjectMatrix<double[]> objMatrix = new SparseObjectMatrix<double[]>(new int[] { 5, 5 });
            objMatrix.set(0, new double[] { 0.0, 0.11, 0.095, 0.092, 0.01 });
            objMatrix.set(1, new double[] { 0.12, 0.15, 0.02, 0.12, 0.09 });
            objMatrix.set(2, new double[] { 0.51, 0.081, 0.025, 0.089, 0.31 });
            objMatrix.set(3, new double[] { 0.18, 0.0601, 0.11, 0.011, 0.03 });
            objMatrix.set(4, new double[] { 0.011, 0.011, 0.011, 0.011, 0.011 });
            mem.setProximalPermanences(objMatrix);

            //      mem.setConnectedSynapses(new SparseObjectMatrix<int[]>(new int[] { 5, 5 }));
            //      SparseObjectMatrix<int[]> syns = mem.getConnectedSynapses();
            //      syns.set(0, new int[] { 0, 1, 0, 0, 0 });
            //      syns.set(1, new int[] { 1, 1, 0, 1, 0 });
            //      syns.set(2, new int[] { 1, 0, 0, 0, 1 });
            //      syns.set(3, new int[] { 1, 0, 1, 0, 0 });
            //      syns.set(4, new int[] { 0, 0, 0, 0, 0 });

            mem.setConnectedCounts(new int[] { 1, 3, 2, 2, 0 });

            double[][] truePermanences = new double[][] {
            new double[]{0.01, 0.12, 0.105, 0.102, 0.02},       // incremented once
            new double[]{0.12, 0.15, 0.02, 0.12, 0.09},         // no change
            new double[]{0.53, 0.101, 0.045, 0.109, 0.33},      // increment twice
            new double[]{0.22, 0.1001, 0.15, 0.051, 0.07},      // increment four times
            new double[]{0.101, 0.101, 0.101, 0.101, 0.101}};   // increment 9 times

            //FORGOT TO SET PERMANENCES ABOVE - DON'T USE mem.setPermanences() 
            int[] indices = mem.getMemory().getSparseIndices();
            for (int i = 0; i < mem.NumColumns; i++)
            {
                // double[] perm = mem.getPotentialPools().get(i).getSparsePermanences();
                double[] perm = mem.getColumn(i).ProximalDendrite.RFPool.getSparsePermanences();
                sp.RaisePermanenceToThreshold(mem.HtmConfig, perm, indices);

                for (int j = 0; j < perm.Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perm[j]) <= 0.001);
                }
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testUpdatePermanencesForColumn()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 5 });
            parameters.setSynPermTrimThreshold(0.05);
            //The following parameter is not set to "1" in the Python version
            //This is necessary to reproduce the test conditions of having as
            //many pool members as Input Bits, which would never happen under
            //normal circumstances because we want to enforce sparsity
            parameters.setPotentialPct(1);
            initSP();

            double[][] permanences = new double[][] {
            new double[]{-0.10, 0.500, 0.400, 0.010, 0.020},
            new double[]{0.300, 0.010, 0.020, 0.120, 0.090},
            new double[]{0.070, 0.050, 1.030, 0.190, 0.060},
            new double[]{0.180, 0.090, 0.110, 0.010, 0.030},
            new double[]{0.200, 0.101, 0.050, -0.09, 1.100}};

            int[][] trueConnectedSynapses = new int[][] {
            new int[]{0, 1, 1, 0, 0},
            new int[]{1, 0, 0, 1, 0},
            new int[]{0, 0, 1, 1, 0},
            new int[]{1, 0, 1, 0, 0},
            new int[]{1, 1, 0, 0, 1}};

            int[][] connectedDense = new int[][] {
            new int[]{ 1, 2 },
            new int[]{ 0, 3 },
            new int[]{ 2, 3 },
            new int[]{ 0, 2 },
            new int[]{ 0, 1, 4 }
        };

            int[] trueConnectedCounts = new int[] { 2, 2, 2, 2, 3 };

            for (int i = 0; i < mem.NumColumns; i++)
            {
                mem.getColumn(i).setPermanences(mem.HtmConfig, permanences[i]);
                HtmCompute.UpdatePermanencesForColumn(mem.HtmConfig, permanences[i], mem.getColumn(i), connectedDense[i], true);
                int[] dense = mem.getColumn(i).getProximalDendrite().getConnectedSynapsesDense();
                trueConnectedSynapses[i].ArrToString().SequenceEqual(dense.ArrToString());
            }

            //trueConnectedCounts.ArrToString().SequenceEqual(mem.getConnectedCounts().getTrueCounts().ArrToString());
            trueConnectedCounts.ArrToString().SequenceEqual(mem.GetTrueCounts().ArrToString());
        }

        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        [TestMethod]
        public void testCalculateOverlap()
        {
            setupDefaultParameters();
            parameters.setInputDimensions(new int[] { 10 });
            parameters.setColumnDimensions(new int[] { 5 });
            initSP();

            int[] dimensions = new int[] { 5, 10 };
            int[][] connectedSynapses = new int[][] {
            new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new int[] {0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
            new int[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1},
            new int[] {0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
            new int[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1}};

            AbstractSparseBinaryMatrix sm = new SparseBinaryMatrix(dimensions);
            for (int i = 0; i < sm.getDimensions()[0]; i++)
            {
                for (int j = 0; j < sm.getDimensions()[1]; j++)
                {
                    sm.set(connectedSynapses[i][j], i, j);
                }
            }

            mem.setConnectedMatrix(sm);

            for (int i = 0; i < 5; i++)
            {
                var column = mem.getColumn(i);

                for (int j = 0; j < 10; j++)
                {
                    Assert.IsTrue(connectedSynapses[i][j] == column.ConnectedInputCounterMatrix.getIntValue(0, j));
                    //Assert.IsTrue(connectedSynapses[i][j] == sm.getIntValue(i, j));
                }
            }

            int[] inputVector = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] overlaps = sp.CalculateOverlap(mem, inputVector);
            int[] trueOverlaps = new int[5];
            double[] overlapsPct = sp.CalculateOverlapPct(mem, overlaps);
            double[] trueOverlapsPct = new double[5];
            Assert.IsTrue(trueOverlaps.SequenceEqual(overlaps));
            Assert.IsTrue(trueOverlapsPct.SequenceEqual(overlapsPct));

            /////////////

            connectedSynapses = new int[][] {
            new int[]{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new int[]{0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
            new int[]{0, 0, 0, 0, 1, 1, 1, 1, 1, 1},
            new int[]{0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 1, 1}};

            sm = new SparseBinaryMatrix(dimensions);
            for (int i = 0; i < sm.getDimensions()[0]; i++)
            {
                for (int j = 0; j < sm.getDimensions()[1]; j++)
                {
                    sm.set(connectedSynapses[i][j], i, j);
                }
            }

            mem.setConnectedMatrix(sm);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Assert.IsTrue(connectedSynapses[i][j] == sm.getIntValue(i, j));
                }
            }

            inputVector = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            overlaps = sp.CalculateOverlap(mem, inputVector);
            trueOverlaps = new int[] { 10, 8, 6, 4, 2 };
            overlapsPct = sp.CalculateOverlapPct(mem, overlaps);
            trueOverlapsPct = new double[] { 1, 1, 1, 1, 1 };
            Assert.IsTrue(trueOverlaps.SequenceEqual(overlaps));
            Assert.IsTrue(trueOverlapsPct.SequenceEqual(overlapsPct));

            //////////////////

            connectedSynapses = new int[][] {
            new int[]{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new int[]{0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
            new int[]{0, 0, 0, 0, 1, 1, 1, 1, 1, 1},
            new int[]{0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 1, 1}};

            sm = new SparseBinaryMatrix(dimensions);
            for (int i = 0; i < sm.getDimensions()[0]; i++)
            {
                for (int j = 0; j < sm.getDimensions()[1]; j++)
                {
                    sm.set(connectedSynapses[i][j], i, j);
                }
            }

            mem.setConnectedMatrix(sm);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Assert.IsTrue(connectedSynapses[i][j] == sm.getIntValue(i, j));
                }
            }

            inputVector = new int[10];
            inputVector[9] = 1;
            overlaps = sp.CalculateOverlap(mem, inputVector);
            trueOverlaps = new int[] { 1, 1, 1, 1, 1 };
            Assert.IsTrue(trueOverlaps.SequenceEqual(overlaps));

            overlapsPct = sp.CalculateOverlapPct(mem, overlaps);
            trueOverlapsPct = new double[] { 0.1, 0.125, 1.0 / 6, 0.25, 0.5 };
            Assert.IsTrue(trueOverlapsPct.SequenceEqual(overlapsPct));

            ///////////////////

            connectedSynapses = new int[][] {
            new int[]{1, 0, 0, 0, 0, 1, 0, 0, 0, 0},
            new int[]{0, 1, 0, 0, 0, 0, 1, 0, 0, 0},
            new int[]{0, 0, 1, 0, 0, 0, 0, 1, 0, 0},
            new int[]{0, 0, 0, 1, 0, 0, 0, 0, 1, 0},
            new int[]{0, 0, 0, 0, 1, 0, 0, 0, 0, 1}};

            sm = new SparseBinaryMatrix(dimensions);
            for (int i = 0; i < sm.getDimensions()[0]; i++)
            {
                for (int j = 0; j < sm.getDimensions()[1]; j++)
                {
                    sm.set(connectedSynapses[i][j], i, j);
                }
            }

            mem.setConnectedMatrix(sm);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Assert.IsTrue(connectedSynapses[i][j] == sm.getIntValue(i, j));
                }
            }

            inputVector = new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            overlaps = sp.CalculateOverlap(mem, inputVector);
            trueOverlaps = new int[] { 1, 1, 1, 1, 1 };
            overlapsPct = sp.CalculateOverlapPct(mem, overlaps);
            trueOverlapsPct = new double[] { 0.5, 0.5, 0.5, 0.5, 0.5 };
            Assert.IsTrue(trueOverlaps.SequenceEqual(overlaps));
            Assert.IsTrue(trueOverlapsPct.SequenceEqual(overlapsPct));
        }

        /**
         * test initial permanence generation. ensure that
         * a correct amount of synapses are initialized in 
         * a connected state, with permanence values drawn from
         * the correct ranges
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testInitPermanence1()
        {
            setupParameters();

            sp = new SpatialPoolerMock4();

            mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);
            mem.HtmConfig.NumInputs = mem.NumInputs = 10;

            mem.setPotentialRadius(2);
            mem.HtmConfig.InitialSynapseConnsPct = mem.InitialSynapseConnsPct = 1;
            int[] mask = new int[] { 0, 1, 2, 8, 9 };

            //var dendriteSeg = mem.getColumn(0).ProximalDendrite;

            //dendriteSeg.Synapses.Clear();

            //for (int i = 0; i < mask.Length; i++)
            //{
            //    dendriteSeg.Synapses.Add(new Synapse(null, dendriteSeg, i, 0) { InputIndex = mask[i] });
            //}

            double[] perm = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            int numcon = ArrayUtils.valueGreaterCount(mem.getSynPermConnected(), perm);

            // Because of connectedPct=1 all 5 specified synapses have to be connected.
            Assert.AreEqual(5, numcon);

            mem.HtmConfig.InitialSynapseConnsPct = mem.InitialSynapseConnsPct = 0;
            perm = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            numcon = ArrayUtils.valueGreaterCount(mem.getSynPermConnected(), perm);
            Assert.AreEqual(0, numcon);

            mem.HtmConfig.InitialSynapseConnsPct = mem.InitialSynapseConnsPct = 0.5;
            mem.setPotentialRadius(100);
            mem.HtmConfig.NumInputs = mem.NumInputs = 100;
            mask = new int[100];
            for (int i = 0; i < 100; i++) mask[i] = i;
            double[] perma = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            numcon = ArrayUtils.valueGreaterOrEqualCount(mem.getSynPermConnected(), perma);
            Assert.IsTrue(numcon > 0);
            Assert.IsTrue(numcon < mem.NumInputs);

            double minThresh = 0.0;
            double maxThresh = mem.getSynPermMax();

            double[] results = perma.Where(d => d > minThresh && d <= maxThresh).ToArray();

            Assert.IsTrue(results.Length > 0);
        }

        /**
         * Test initial permanence generation. ensure that permanence values
         * are only assigned to bits within a column's potential pool.
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testInitPermanence2()
        {
            setupParameters();

            sp = new SpatialPoolerMock4();

            mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            mem.HtmConfig.NumInputs = mem.NumInputs = 10;
            double connectedPct = 1;
            int[] mask = new int[] { 0, 1 };
            double[] perm = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            int[] trueConnected = new int[] { 0, 1 };

            ArrayUtils.toDoubleArray(trueConnected).SequenceEqual(perm.Where(d => d > 0));

            connectedPct = 1;
            mask = new int[] { 4, 5, 6 };
            perm = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            trueConnected = new int[] { 4, 5, 6 };
            ArrayUtils.toDoubleArray(trueConnected).SequenceEqual(perm.Where(d => d > 0));

            connectedPct = 1;
            mask = new int[] { 8, 9 };
            perm = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            trueConnected = new int[] { 8, 9 };
            ArrayUtils.toDoubleArray(trueConnected).SequenceEqual(perm.Where(d => d > 0));

            connectedPct = 1;
            mask = new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 9 };
            perm = HtmCompute.InitSynapsePermanences(mem.HtmConfig, mask, mem.getRandom());
            trueConnected = new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 9 };
            ArrayUtils.toDoubleArray(trueConnected).SequenceEqual(perm.Where(d => d > 0));
        }

        /**
         * Tests that duty cycles are updated properly according
         * to the mathematical formula. also check the effects of
         * supplying a maxPeriod to the function.
         */
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testUpdateDutyCycleHelper()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 5 });
            initSP();

            double[] dc = new double[5];
            ArrayUtils.fillArray(dc, 1000.0);
            double[] newvals = new double[5];
            int period = 1000;
            double[] newDc = sp.updateDutyCyclesHelper(mem, dc, newvals, period);
            double[] trueNewDc = new double[] { 999, 999, 999, 999, 999 };
            Assert.IsTrue(trueNewDc.SequenceEqual(newDc));

            dc = new double[5];
            ArrayUtils.fillArray(dc, 1000.0);
            newvals = new double[5];
            ArrayUtils.fillArray(newvals, 1000);
            period = 1000;
            newDc = sp.updateDutyCyclesHelper(mem, dc, newvals, period);

            trueNewDc = new double[5];
            Array.Copy(dc, trueNewDc, trueNewDc.Length);

            Assert.IsTrue(trueNewDc.SequenceEqual(newDc));

            dc = new double[5];
            ArrayUtils.fillArray(dc, 1000.0);
            newvals = new double[] { 2000, 4000, 5000, 6000, 7000 };
            period = 1000;
            newDc = sp.updateDutyCyclesHelper(mem, dc, newvals, period);
            trueNewDc = new double[] { 1001, 1003, 1004, 1005, 1006 };
            Assert.IsTrue(trueNewDc.SequenceEqual(newDc));

            dc = new double[] { 1000, 800, 600, 400, 2000 };
            newvals = new double[5];
            period = 2;
            newDc = sp.updateDutyCyclesHelper(mem, dc, newvals, period);
            trueNewDc = new double[] { 500, 400, 300, 200, 1000 };
            Assert.IsTrue(trueNewDc.SequenceEqual(newDc));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testInhibitColumnsGlobal()
        {
            setupParameters();
            parameters.setColumnDimensions(new int[] { 10 });
            initSP();
            //Internally calculated during init, to overwrite we put after init
            parameters.setInhibitionRadius(2);
            double density = 0.3;
            double[] overlaps = new double[] { 1, 2, 1, 4, 8/*i=4*/, 3, 12/*i=6*/, 5/*i=7*/, 4, 1 };
            int[] active = sp.inhibitColumnsGlobal(mem, overlaps, density);

            // See overlaps comments (i=k) above.
            int[] trueActive = new int[] { 4, 6, 7 };
            active = active.OrderBy(i => i).ToArray();
            Assert.IsTrue(trueActive.SequenceEqual(active));

            density = 0.5;
            mem.setNumColumns(10);
            //overlaps = IntStream.range(0, 10).mapToDouble(i->i).toArray();
            for (int i = 0; i < 10; i++)
            {
                overlaps[i] = i * 1.0;
            }

            active = sp.inhibitColumnsGlobal(mem, overlaps, density);

            trueActive = new int[5];
            for (int i = 5; i < 10; i++)
                trueActive[i - 5] = i;

            Assert.IsTrue(trueActive.SequenceEqual(active));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testInhibitColumnsLocal()
        {
            setupParameters();
            parameters.setInputDimensions(new int[] { 5 });
            parameters.setColumnDimensions(new int[] { 10 });
            initSP();

            //Internally calculated during init, to overwrite we put after init
            mem.InhibitionRadius = 2;
            double density = 0.5;
            double[] overlaps = new double[] { 1, 2, 7, 0, 3, 4, 16, 1, 1.5, 1.7 };
            //  L  W  W  L  L  W  W   L   W    W (wrapAround=true)
            //  L  W  W  L  L  W  W   L   L    W (wrapAround=false)

            mem.setWrapAround(true);
            int[] trueActive = new int[] { 1, 2, 5, 6, 8, 9 };
            int[] active = sp.InhibitColumnsLocal(mem, overlaps, density);
            Assert.IsTrue(trueActive.SequenceEqual(active));

            mem.setWrapAround(false);
            trueActive = new int[] { 1, 2, 5, 6, 9 };
            active = sp.InhibitColumnsLocal(mem, overlaps, density);
            Assert.IsTrue(trueActive.SequenceEqual(active));

            density = 0.5;
            mem.InhibitionRadius = 3;
            overlaps = new double[] { 1, 2, 7, 0, 3, 4, 16, 1, 1.5, 1.7 };
            //  L  W  W  L  W  W  W   L   L    W (wrapAround=true)
            //  L  W  W  L  W  W  W   L   L    L (wrapAround=false)

            mem.setWrapAround(true);
            trueActive = new int[] { 1, 2, 4, 5, 6, 9 };
            active = sp.InhibitColumnsLocal(mem, overlaps, density);
            Assert.IsTrue(trueActive.SequenceEqual(active));

            mem.setWrapAround(false);
            trueActive = new int[] { 1, 2, 4, 5, 6, 9 };
            active = sp.InhibitColumnsLocal(mem, overlaps, density);
            Assert.IsTrue(trueActive.SequenceEqual(active));

            // Test add to winners
            density = 0.3333;
            mem.InhibitionRadius = 3;
            overlaps = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            //  W  W  L  L  W  W  L  L  L  L (wrapAround=true)
            //  W  W  L  L  W  W  L  L  W  L (wrapAround=false)

            mem.setWrapAround(true);
            trueActive = new int[] { 0, 1, 4, 5 };
            active = sp.InhibitColumnsLocal(mem, overlaps, density);
            Assert.IsTrue(trueActive.SequenceEqual(active));

            density = 0.20;
            overlaps = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            mem.setWrapAround(false);
            trueActive = new int[] { 0, 4, 8 };
            active = sp.InhibitColumnsLocal(mem, overlaps, density);
            Assert.IsTrue(trueActive.SequenceEqual(active));

            //overlaps = new double[] { 1, 2, 7, 0.1, 3, 4, 16, 1, 1.5, 1.7 };
            mem.setWrapAround(false);
            density = 0.10;
            active = sp.InhibitColumnsLocal(mem, overlaps, density);

            density = 0.20;
            active = sp.InhibitColumnsLocal(mem, overlaps, density);

            density = 0.30;
            active = sp.InhibitColumnsLocal(mem, overlaps, density);

            density = 0.40;
            active = sp.InhibitColumnsLocal(mem, overlaps, density);

            density = 0.50;
            active = sp.InhibitColumnsLocal(mem, overlaps, density);
        }

        //    /**
        //     * As coded in the Python test
        //     */
        //    @Test
        //    public void testGetNeighborsND() {
        //        //This setup isn't relevant to this test
        //        setupParameters();
        //        parameters.setInputDimensions(new int[] { 9, 5 });
        //        parameters.setColumnDimensions(new int[] { 5, 5 });
        //        initSP();
        //        ////////////////////// Test not part of Python port /////////////////////
        //        int[] result = sp.getNeighborsND(mem, 2, mem.getInputMatrix(), 3, true).toArray();
        //        int[] expected = new int[] { 
        //                0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 
        //                13, 14, 15, 16, 17, 18, 19, 30, 31, 32, 33, 
        //                34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44 
        //        };
        //        for(int i = 0;i < result.length;i++) {
        //            assertEquals(expected[i], result[i]);
        //        }
        //        /////////////////////////////////////////////////////////////////////////
        //        setupParameters();
        //        int[] dimensions = new int[] { 5, 7, 2 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //        int radius = 1;
        //        int x = 1;
        //        int y = 3;
        //        int z = 2;
        //        int columnIndex = mem.getInputMatrix().computeIndex(new int[] { z, y, x });
        //        int[] neighbors = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        String expect = "[18, 19, 20, 21, 22, 23, 32, 33, 34, 36, 37, 46, 47, 48, 49, 50, 51]";
        //        assertEquals(expect, ArrayUtils.print1DArray(neighbors));
        //
        //        /////////////////////////////////////////
        //        setupParameters();
        //        dimensions = new int[] { 5, 7, 9 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //        radius = 3;
        //        x = 0;
        //        y = 0;
        //        z = 3;
        //        columnIndex = mem.getInputMatrix().computeIndex(new int[] { z, y, x });
        //        neighbors = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        expect = "[0, 1, 2, 3, 6, 7, 8, 9, 10, 11, 12, 15, 16, 17, 18, 19, 20, 21, 24, 25, 26, "
        //                + "27, 28, 29, 30, 33, 34, 35, 36, 37, 38, 39, 42, 43, 44, 45, 46, 47, 48, 51, "
        //                + "52, 53, 54, 55, 56, 57, 60, 61, 62, 63, 64, 65, 66, 69, 70, 71, 72, 73, 74, "
        //                + "75, 78, 79, 80, 81, 82, 83, 84, 87, 88, 89, 90, 91, 92, 93, 96, 97, 98, 99, "
        //                + "100, 101, 102, 105, 106, 107, 108, 109, 110, 111, 114, 115, 116, 117, 118, 119, "
        //                + "120, 123, 124, 125, 126, 127, 128, 129, 132, 133, 134, 135, 136, 137, 138, 141, "
        //                + "142, 143, 144, 145, 146, 147, 150, 151, 152, 153, 154, 155, 156, 159, 160, 161, "
        //                + "162, 163, 164, 165, 168, 169, 170, 171, 172, 173, 174, 177, 178, 179, 180, 181, "
        //                + "182, 183, 186, 187, 188, 190, 191, 192, 195, 196, 197, 198, 199, 200, 201, 204, "
        //                + "205, 206, 207, 208, 209, 210, 213, 214, 215, 216, 217, 218, 219, 222, 223, 224, "
        //                + "225, 226, 227, 228, 231, 232, 233, 234, 235, 236, 237, 240, 241, 242, 243, 244, "
        //                + "245, 246, 249, 250, 251, 252, 253, 254, 255, 258, 259, 260, 261, 262, 263, 264, "
        //                + "267, 268, 269, 270, 271, 272, 273, 276, 277, 278, 279, 280, 281, 282, 285, 286, "
        //                + "287, 288, 289, 290, 291, 294, 295, 296, 297, 298, 299, 300, 303, 304, 305, 306, "
        //                + "307, 308, 309, 312, 313, 314]";
        //        assertEquals(expect, ArrayUtils.print1DArray(neighbors));
        //
        //        /////////////////////////////////////////
        //        setupParameters();
        //        dimensions = new int[] { 5, 10, 7, 6 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //
        //        radius = 4;
        //        int w = 2;
        //        x = 5;
        //        y = 6;
        //        z = 2;
        //        columnIndex = mem.getInputMatrix().computeIndex(new int[] { z, y, x, w });
        //        neighbors = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        TIntHashSet trueNeighbors = new TIntHashSet();
        //        for(int i = -radius;i <= radius;i++) {
        //            for(int j = -radius;j <= radius;j++) {
        //                for(int k = -radius;k <= radius;k++) {
        //                    for(int m = -radius;m <= radius;m++) {
        //                        int zprime = (int)ArrayUtils.positiveRemainder((z + i), dimensions[0]);
        //                        int yprime = (int)ArrayUtils.positiveRemainder((y + j), dimensions[1]);
        //                        int xprime = (int)ArrayUtils.positiveRemainder((x + k), dimensions[2]);
        //                        int wprime = (int)ArrayUtils.positiveRemainder((w + m), dimensions[3]);
        //                        trueNeighbors.add(mem.getInputMatrix().computeIndex(new int[] { zprime, yprime, xprime, wprime }));
        //                    }
        //                }
        //            }
        //        }
        //        trueNeighbors.remove(columnIndex);
        //        int[] tneighbors = ArrayUtils.unique(trueNeighbors.toArray());
        //        assertEquals(ArrayUtils.print1DArray(tneighbors), ArrayUtils.print1DArray(neighbors));
        //
        //        /////////////////////////////////////////
        //        //Tests from getNeighbors1D from Python unit test
        //        setupParameters();
        //        dimensions = new int[] { 8 };
        //        parameters.setColumnDimensions(dimensions);
        //        parameters.setInputDimensions(dimensions);
        //        initSP();
        //        AbstractSparseBinaryMatrix sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        sbm.set(new int[] { 2, 4 }, new int[] { 1, 1 }, true);
        //        radius = 1;
        //        columnIndex = 3;
        //        int[] mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        TIntArrayList msk = new TIntArrayList(mask);
        //        TIntArrayList neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        //////
        //        setupParameters();
        //        dimensions = new int[] { 8 };
        //        parameters.setInputDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        sbm.set(new int[] { 1, 2, 4, 5 }, new int[] { 1, 1, 1, 1 }, true);
        //        radius = 2;
        //        columnIndex = 3;
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        //Wrap around
        //        setupParameters();
        //        dimensions = new int[] { 8 };
        //        parameters.setInputDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        sbm.set(new int[] { 1, 2, 6, 7 }, new int[] { 1, 1, 1, 1 }, true);
        //        radius = 2;
        //        columnIndex = 0;
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        //Radius too big
        //        setupParameters();
        //        dimensions = new int[] { 8 };
        //        parameters.setInputDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        sbm.set(new int[] { 0, 1, 2, 3, 4, 5, 7 }, new int[] { 1, 1, 1, 1, 1, 1, 1 }, true);
        //        radius = 20;
        //        columnIndex = 6;
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        //These are all the same tests from 2D
        //        setupParameters();
        //        dimensions = new int[] { 6, 5 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        int[][] input = new int[][] { 
        //            {0, 0, 0, 0, 0},
        //            {0, 0, 0, 0, 0},
        //            {0, 1, 1, 1, 0},
        //            {0, 1, 0, 1, 0},
        //            {0, 1, 1, 1, 0},
        //            {0, 0, 0, 0, 0}};
        //            for(int i = 0;i < input.length;i++) {
        //                for(int j = 0;j < input[i].length;j++) {
        //                    if(input[i][j] == 1) 
        //                        sbm.set(sbm.computeIndex(new int[] { i, j }), 1);
        //                }
        //            }
        //        radius = 1;
        //        columnIndex = 3*5 + 2;
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        ////////
        //        setupParameters();
        //        dimensions = new int[] { 6, 5 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        input = new int[][] { 
        //            {0, 0, 0, 0, 0},
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 0, 1, 1},
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 1, 1, 1}};
        //        for(int i = 0;i < input.length;i++) {
        //            for(int j = 0;j < input[i].length;j++) {
        //                if(input[i][j] == 1) 
        //                    sbm.set(sbm.computeIndex(new int[] { i, j }), 1);
        //            }
        //        }
        //        radius = 2;
        //        columnIndex = 3*5 + 2;
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        //Radius too big
        //        setupParameters();
        //        dimensions = new int[] { 6, 5 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        input = new int[][] { 
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 0, 1, 1},
        //            {1, 1, 1, 1, 1},
        //            {1, 1, 1, 1, 1}};
        //            for(int i = 0;i < input.length;i++) {
        //                for(int j = 0;j < input[i].length;j++) {
        //                    if(input[i][j] == 1) 
        //                        sbm.set(sbm.computeIndex(new int[] { i, j }), 1);
        //                }
        //            }
        //        radius = 7;
        //        columnIndex = 3*5 + 2;
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //
        //        //Wrap-around
        //        setupParameters();
        //        dimensions = new int[] { 6, 5 };
        //        parameters.setInputDimensions(dimensions);
        //        parameters.setColumnDimensions(dimensions);
        //        initSP();
        //        sbm = (AbstractSparseBinaryMatrix)mem.getInputMatrix();
        //        input = new int[][] { 
        //            {1, 0, 0, 1, 1},
        //            {0, 0, 0, 0, 0},
        //            {0, 0, 0, 0, 0},
        //            {0, 0, 0, 0, 0},
        //            {1, 0, 0, 1, 1},
        //            {1, 0, 0, 1, 0}};
        //        for(int i = 0;i < input.length;i++) {
        //            for(int j = 0;j < input[i].length;j++) {
        //                if(input[i][j] == 1) 
        //                    sbm.set(sbm.computeIndex(new int[] { i, j }), 1);
        //            }
        //        }
        //        radius = 1;
        //        columnIndex = sbm.getMaxIndex();
        //        mask = sp.getNeighborsND(mem, columnIndex, mem.getInputMatrix(), radius, true).toArray();
        //        msk = new TIntArrayList(mask);
        //        neg = new TIntArrayList(ArrayUtils.range(0, dimensions[0]));
        //        neg.removeAll(msk);
        //        assertTrue(sbm.all(mask));
        //        assertFalse(sbm.any(neg));
        //    }



        [TestMethod]
        [DataRow(PoolerMode.SingleThreaded)]
        [DataRow(PoolerMode.Multicore)]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testInit(PoolerMode poolerMode)
        {
            setupParameters();
            parameters.setNumActiveColumnsPerInhArea(0);
            parameters.setLocalAreaDensity(0);

            Connections c = new Connections();
            parameters.apply(c);

            SpatialPooler sp = UnitTestHelpers.CreatePooler(poolerMode);

            // Local Area Density cannot be 0
            try
            {
                sp.init(c);
                Assert.Fail();
                //fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue("Inhibition parameters are invalid" == e.Message);
                Assert.IsTrue(e is ArgumentException);
            }

            // Local Area Density can't be above 0.5
            parameters.setLocalAreaDensity(0.51);
            c = new Connections();
            parameters.apply(c);
            try
            {
                sp.init(c);
                //fail();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue("Inhibition parameters are invalid" == e.Message);
                Assert.IsTrue(e is ArgumentException);
                //assertEquals("Inhibition parameters are invalid", e.getMessage());
                //assertEquals(InvalidSPParamValueException.class, e.getClass());
            }

            // Local Area Density should be sane here
            parameters.setLocalAreaDensity(0.5);
            c = new Connections();
            parameters.apply(c);
            try
            {
                sp.init(c);
            }
            catch (Exception e)
            {
                //fail();
                Assert.Fail();
            }

            // Num columns cannot be 0
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 0 });
            c = new Connections();
            parameters.apply(c);
            try
            {
                sp.init(c);
                //fail();
                Assert.Fail();
            }
            catch (Exception e)
            {
                //     assertEquals("Invalid number of columns: 0", e.getMessage());
                //assertEquals(InvalidSPParamValueException.class, e.getClass());
                Assert.IsTrue("Invalid number of columns: 0" == e.Message);
                Assert.IsTrue(e is ArgumentException);

            }

            // Reset column dims
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });

            // Num columns cannot be 0
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 0 });
            c = new Connections();
            parameters.apply(c);
            try
            {
                sp.init(c);
                //fail();
                Assert.Fail();
            }
            catch (Exception e)
            {
                //assertEquals("Invalid number of inputs: 0", e.getMessage());
                //assertEquals(InvalidSPParamValueException.class, e.getClass());

                Assert.IsTrue("Invalid number of inputs: 0" == e.Message);
                Assert.IsTrue(e is ArgumentException);
            }
        }

        /// <summary>
        /// Input dimensions do not fit inpout vector.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("SpatialPooler")]
        public void testComputeInputMismatch()
        {
            setupParameters();
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 2, 4 });
            parameters.setColumnDimensions(new int[] { 5, 1 });

            Connections c = new Connections();
            parameters.apply(c);

            int misMatchedDims = 6; // not 8
            SpatialPooler sp = new SpatialPooler();
            sp.init(c);
            try
            {
                sp.compute(new int[misMatchedDims], new int[25], true);
                //fail();
                //Assert.Fail();
            }
            catch (ArgumentException e)
            {
                //assertEquals("Input array must be same size as the defined number"
                //    + " of inputs: From Params: 8, From Input Vector: 6", e.getMessage());
                //assertEquals(InvalidSPParamValueException.class, e.getClass());


                //Assert.Equals("Input array must be same size as the defined number of inputs: From Params: 8, From Input Vector: 6", e.Message);
                //Assert.Equals(typeof(ArgumentException), e.GetType());
            }


            // Now Do the right thing
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 2, 4 });
            parameters.setColumnDimensions(new int[] { 5, 1 });

            c = new Connections();
            parameters.apply(c);

            int matchedDims = 8; // same as input dimension multiplied, above
            sp.init(c);
            try
            {
                sp.compute(new int[matchedDims], new int[25], true);
            }
            catch (ArgumentException e)
            {
                //fail();
                Assert.Fail();
            }
        }


    }


}
