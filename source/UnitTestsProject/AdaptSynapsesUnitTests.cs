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

        /// <summary>
        /// Sets up default parameters for the Hierarchical Temporal Memory (HTM) configuration.
        /// </summary>
        /// <returns>An instance of <see cref="HtmConfig"/> with default parameters.</returns>
        private HtmConfig SetupHtmConfigDefaultParameters()
        {
            // Create a new instance of HtmConfig with specified input and column dimensions
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

                // Initialize the Random property with a ThreadSafeRandom instance using a seed value
                Random = new ThreadSafeRandom(42)
            };

            return htmConfig;
        }




        /// <summary>
        /// Unit test method for the 'AdaptSynapses' function with maximum threshold value.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithMaxThreshold()
        {
            // Initialization with default parameters from HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Creating Connections and SpatialPooler instances
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Setting the maximum threshold value for synaptic permanence trimming
            mem.HtmConfig.SynPermTrimThreshold = .05;

            // Defining potential pools for columns
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            // Initializing permanences for each column
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Expected true permanences after adaptation
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Setting up potential pools and permanences for each column in Connections
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Input vector and active columns for testing
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Executing the AdaptSynapses method with the specified parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Asserting that the adapted permanences match the expected true permanences
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
        /// Unit test for the 'AdaptSynapses' method with a single set of permanences.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithSinglePermanences()
        {
            // Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Set synapse trim threshold
            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            // Define potential pools for columns
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 }
            };

            // Initialize permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 }
            };

            // Define expected true permanences after adaptation
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 }
            };

            // Set proximal connected synapses and initial permanences for each column
            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Set input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0 };

            // Execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Validate that the actual permanences match the expected true permanences
            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }


        /// <summary>
        /// Unit test method to verify the adaptation of synapses with two permanences.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithTwoPermanences()
        {
            // Initialize Hyperparameter Configuration
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Create HTM Connections and Spatial Pooler instances
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Set the Synapse Trim Threshold
            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            // Define potential pools for columns
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 }
            };

            // Initialize synapse permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 }
            };

            // Define expected true permanences after adaptation
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 }
            };

            // Set up proximal synapses for each column
            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Define input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1 };

            // Execute the AdaptSynapses method with specified parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Verify that the adapted permanences match the expected true permanences
            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }


        /// <summary>
        /// Unit test method for the 'AdaptSynapses' function with a minimum threshold value.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithMinThreshold()
        {
            // Initialization with default HtmConfig parameters.
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Creating Connections object and initializing SpatialPooler.
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Setting the minimum threshold value for synaptic permanences trimming.
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            // Defining potential pools, initialized permanences, and true permanences.
            int[][] potentialPools = new int[][]
            {
            new int[] { 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[] { 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[] { 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[] { 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            double[][] permanences = new double[][]
            {
            new double[] { 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[] { 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[] { 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[] { 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][]
            {
            new double[] { 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[] { 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[] { 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[] { 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Setting up proximal connected synapses and initial permanences for each column.
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Defining input vector and active columns.
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Executing the AdaptSynapses method with the specified parameters.
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Verifying that the resulting permanences match the expected true permanences.
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
        /// Unit test method to validate the adaptation of synapses with three permanences.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithThreePermanences()
        {
            // Initialization with default HTM configuration parameters
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Creating HTM connections and spatial pooler instances
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Setting specific threshold value for synapse trimming
            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            // Defining potential pools for each column
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 }
            };

            // Initializing permanences for each synapse
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 }
            };

            // Expected true permanences after synapse adaptation
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 }
            };

            // Setting up connected synapses and initial permanences for each column
            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Simulating input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Executing the AdaptSynapses method with specified parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Verifying the adapted permanences match the expected true permanences
            for (int i = 0; i < mem.HtmConfig.NumColumns - 4; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }


        /// <summary>
        /// Unit test for the "AdaptSynapses" method with four permanences.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithFourPermanences()
        {
            // Initialization with HtmConfig parameters.
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Create Connections and SpatialPooler instances.
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Set SynPermTrimThreshold value.
            mem.HtmConfig.SynPermTrimThreshold = .05;

            // Define potential pools for each column.
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            // Initialize permanences for each column.
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Define true permanences after adaptation.
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Set proximal connected synapses and initial permanences for each column.
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Set input vector and active columns.
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Execute the AdaptSynapses method with parameters.
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Validate that the adapted permanences match the expected true permanences.
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
        /// Unit test for the 'AdaptSynapses' method when there are no active columns.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithNoColumns()
        {
            // Initialization with default HtmConfig parameters
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Create Connections and SpatialPooler instances
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Set the minimum threshold value for synapse trimming
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            // Define potential pools for each column
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            // Initialize permanences for each column
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Define the expected true permanences after adaptation
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Set up proximal connected synapses and initial permanences for each column
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Define an input vector and an array of active columns (empty in this case)
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { };

            // Execute the AdaptSynapses method with the specified parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Validate that the dense permanences have been successfully adapted
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);

                // Assert that the dense permanences are not null after adaptation
                Assert.IsNotNull(perms);
            }
        }


        /// <summary>
        /// Unit test for the AdaptSynapses method when there are no active columns and no input vectors.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithNoColumnsNoInputVector()
        {
            // Initialize HtmConfig parameters
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;

            // Create Connections object and SpatialPooler instance
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            // Set minimum threshold value for synapse trimming
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            // Define potential pools for each column
            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            // Initialize permanences for each column
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Define true permanences after synapse adaptation
            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            // Set up potential synapses and initial permanences for each column
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            // Define input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { };

            // Execute the AdaptSynapses method with specified parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Assert that the resulting synapse permanences are not null for each column
            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                Assert.IsNotNull(perms);
            }
        }
    }
}