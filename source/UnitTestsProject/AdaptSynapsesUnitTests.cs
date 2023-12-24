// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class AdaptSynapsesUnitTests
    {
        int inputBits = 88;
        int numColumns = 1024;
        private Parameters parameters;
        private SpatialPooler sp;
        private Connections mem;

        public void SetupParameters()
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
        private HtmConfig SetupHtmConfigParameters()
        {
            var htmConfig = new HtmConfig(new int[] { 5 }, new int[] { 5 })
            {
                PotentialRadius = 5,
                PotentialPct = 0.5,
                GlobalInhibition = false,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 3.0,
                StimulusThreshold = 0.0,
                SynPermInactiveDec = 0.01,
                SynPermActiveInc = 0.1,
                SynPermConnected = 0.1,
                MinPctOverlapDutyCycles = 0.1,
                MinPctActiveDutyCycles = 0.1,
                DutyCyclePeriod = 10,
                MaxBoost = 10,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42),
            };

            return htmConfig;
        }

        public void SetupDefaultParameters()
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

        private HtmConfig SetupHtmConfigDefaultParameters()
        {
            var htmConfig = new HtmConfig(new int[] { 32, 32 }, new int[] { 64, 64 })
            {
                PotentialRadius = 16,
                PotentialPct = 0.5,
                GlobalInhibition = false,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 10.0,
                StimulusThreshold = 0.0,
                SynPermInactiveDec = 0.008,
                SynPermActiveInc = 0.05,
                SynPermConnected = 0.10,
                MinPctOverlapDutyCycles = 0.001,
                MinPctActiveDutyCycles = 0.001,
                DutyCyclePeriod = 1000,
                MaxBoost = 10.0,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42)
            };

            return htmConfig;
        }

        /// <summary>
        /// Intialization for SpatialPoolerMT
        /// </summary>
        private void InitTestSPInstance()
        {
            sp = new SpatialPoolerMT();
            mem = new Connections();
            parameters.apply(mem);
            sp.Init(mem);
        }

        /// <summary>
        /// Initialization of Spatial Pooler
        /// </summary>
        private void InitTestSPInstanceForSP()
        {
            sp = new SpatialPooler();
            HtmConfig htmConfig = HtMConfigInitialization(); //initialized using HtmConfig
            mem = new Connections(htmConfig);
            parameters.apply(mem);
            sp.Init(mem);
        }

        /// <summary>
        /// HtmConfig Initialization with parameters
        /// </summary>
        /// <returns></returns>
        private HtmConfig HtMConfigInitialization()
        {
            HtmConfig htmConfig = new HtmConfig()
            {
                InputDimensions = new int[] { 32, 32 },
                ColumnDimensions = new int[] { 64, 64 },
                PotentialRadius = 16,
                PotentialPct = 0.5,
                GlobalInhibition = false,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 0.02 * 2048,
                StimulusThreshold = 5.0,
                SynPermInactiveDec = 0.008,
                SynPermActiveInc = 0.05,
                SynPermConnected = 0.1,
                SynPermBelowStimulusInc = 0.01,
                SynPermTrimThreshold = 0.05,
                MinPctOverlapDutyCycles = 0.001,
                MinPctActiveDutyCycles = 0.001,
                DutyCyclePeriod = 1000,
                MaxBoost = 10.0,
                WrapAround = true,
                Random = new ThreadSafeRandom(42),
            };

            return htmConfig;
        }

        /// <summary>
        /// Adapt synapses method with maximum threshold value
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithMaxThreshold()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            mem.HtmConfig.SynPermTrimThreshold = .05;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        /// <summary>
        /// Adapt synapses method with single permanences
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithSinglePermanences()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns-4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns-4; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        /// <summary>
        /// Adapt synapses method with two permanences
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithTwoPermanences()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns-4; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        /// <summary>
        /// Adapt synapses method with mininimum threshold value
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithMinThreshold()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            //mininimum threshold value
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        /// <summary>
        /// Adapt synapses method with three permanences
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithThreePermanences()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            mem.HtmConfig.SynPermTrimThreshold = .05;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns-4; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        /// <summary>
        /// Adapt synapses method with four permanences
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithFourPermanences()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            mem.HtmConfig.SynPermTrimThreshold = .05;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }

        // <summary>
        /// Adapt synapses method with No active columns
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithNoColumns()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            //mininimum threshold value
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);

                Assert.IsNotNull(perms);
            }
        }

        /// <summary>
        /// Adapt synapses method with No active columns and no input vectors
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithNoColumnsNoInputVector()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            //mininimum threshold value
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);

                Assert.IsNotNull(perms);
            }
        }


    }


}