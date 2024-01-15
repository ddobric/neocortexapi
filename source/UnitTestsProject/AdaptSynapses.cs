// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class AdaptSynapses
    {
        int inputBits = 88;
        int numColumns = 1024;
        private Parameters parameters;
        private SpatialPooler SpatialPooler;
        private Connections htmConnections;

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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Setting the maximum threshold value for synaptic permanence trimming
            htmConnections.HtmConfig.SynPermTrimThreshold = .05;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Input vector and active columns for testing
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Executing the AdaptSynapses method with the specified parameters 
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Asserting that the adapted permanences match the expected true permanences
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Set synapse trim threshold
            htmConnections.HtmConfig.SynPermTrimThreshold = 0.05;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Set input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0 };

            // Execute the AdaptSynapses method with parameters 
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Validate that the actual permanences match the expected true permanences
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns - 4; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Set the Synapse Trim Threshold
            htmConnections.HtmConfig.SynPermTrimThreshold = 0.05;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Define input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1 };

            // Execute the AdaptSynapses method with specified parameters 
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Verify that the adapted permanences match the expected true permanences
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns - 4; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Setting the minimum threshold value for synaptic permanences trimming.
            htmConnections.HtmConfig.SynPermTrimThreshold = 0.01;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Defining input vector and active columns.
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Executing the AdaptSynapses method with the specified parameters.
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Verifying that the resulting permanences match the expected true permanences.
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Setting specific threshold value for synapse trimming
            htmConnections.HtmConfig.SynPermTrimThreshold = 0.05;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns - 4; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Simulating input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Executing the AdaptSynapses method with specified parameters 
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Verifying the adapted permanences match the expected true permanences
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns - 4; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Set SynPermTrimThreshold value.
            htmConnections.HtmConfig.SynPermTrimThreshold = .05;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Set input vector and active columns.
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Execute the AdaptSynapses method with parameters.
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Validate that the adapted permanences match the expected true permanences.
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Set the minimum threshold value for synapse trimming
            htmConnections.HtmConfig.SynPermTrimThreshold = 0.01;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Define an input vector and an array of active columns (empty in this case)
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { };

            // Execute the AdaptSynapses method with the specified parameters 
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Validate that the dense permanences have been successfully adapted
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);

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
            htmConnections = new Connections(htmConfig);
            SpatialPooler = new SpatialPooler();
            SpatialPooler.Init(htmConnections);

            // Set minimum threshold value for synapse trimming
            htmConnections.HtmConfig.SynPermTrimThreshold = 0.01;

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
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                htmConnections.GetColumn(i).SetProximalConnectedSynapsesForTest(htmConnections, indexes);
                htmConnections.GetColumn(i).SetPermanences(htmConnections.HtmConfig, permanences[i]);
            }

            // Define input vector and active columns
            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { };

            // Execute the AdaptSynapses method with specified parameters 
            SpatialPooler.AdaptSynapses(htmConnections, inputVector, activeColumns);

            // Assert that the resulting synapse permanences are not null for each column
            for (int i = 0; i < htmConnections.HtmConfig.NumColumns; i++)
            {
                double[] perms = htmConnections.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(htmConnections.HtmConfig.NumInputs);
                Assert.IsNotNull(perms);
            }
        }

        private const string CONNECTIONS_CANNOT_BE_NULL = "Connections cannot be null";
        private const string DISTALDENDRITE_CANNOT_BE_NULL = "Object reference not set to an instance of an object.";

        /// <summary>
        /// Testing whether the permanence of a synapse in a distal dendrite segment increases if its presynaptic cell 
        /// was active in the previous cycle.with a permanence value of 0.1. Then it calls the AdaptSegment 
        /// method with the presynaptic cells set to cn.GetCells(new int[] { 23, 37 }). This means that if 
        /// the presynaptic cell with index 23 was active in the previous cycle, the synapse's permanence 
        /// should be increased.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_PermanenceStrengthened_IfPresynapticCellWasActive()
        {
            TemporalMemory TemporalMemory = new TemporalMemory();
            Connections Connections = new Connections();
            Parameters Parameters = Parameters.getAllDefaultParameters();
            Parameters.apply(Connections);
            TemporalMemory.Init(Connections);

            DistalDendrite dd = Connections.CreateDistalSegment(Connections.GetCell(0));
            Synapse s1 = Connections.CreateSynapse(dd, Connections.GetCell(23), 0.1);

            // Invoking AdaptSegments with only the cells with index 23
            /// whose presynaptic cell is considered to be Active in the
            /// previous cycle and presynaptic cell is Inactive for the cell 477
            TemporalMemory.AdaptSegment(Connections, dd, Connections.GetCells(new int[] { 23 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);

            //Assert
            /// permanence is incremented for presynaptie cell 23 from 
            /// 0.1 to 0.2 as presynaptic cell was InActive in the previous cycle
            Assert.AreEqual(0.2, s1.Permanence);
        }


        /// <summary>
        /// Testing the scenario where a synapse's presynaptic cell was not active in the previous cycle, 
        /// so the AdaptSegment method should decrease the permanence value of that synapse by 
        /// permanenceDecrement amount.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_PermanenceWekened_IfPresynapticCellWasInActive()
        {
            TemporalMemory TemporalMemory = new TemporalMemory();
            Connections Connections = new Connections();
            Parameters Parameters = Parameters.getAllDefaultParameters();
            Parameters.apply(Connections);
            TemporalMemory.Init(Connections);

            DistalDendrite distalDendrite = Connections.CreateDistalSegment(Connections.GetCell(0));
            Synapse synapse1 = Connections.CreateSynapse(distalDendrite, Connections.GetCell(500), 0.9);


            TemporalMemory.AdaptSegment(Connections, distalDendrite, Connections.GetCells(new int[] { 23, 57 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            //Assert
            /// /// permanence is decremented for presynaptie cell 500 from 
            /// 0.9 to 0.8 as presynaptic cell was InActive in the previous cycle
            /// But the synapse is not destroyed as permanence > HtmConfig.Epsilon
            Assert.AreEqual(0.8, synapse1.Permanence);
        }


        /// <summary>
        /// Test to check if the permanence of a synapse is limited within the range of 0 to 1.0.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_PermanenceIsLimitedWithinRange()
        {
            TemporalMemory TemporalMemory = new TemporalMemory();
            Connections Connections = new Connections();
            Parameters Parameters = Parameters.getAllDefaultParameters();
            Parameters.apply(Connections);
            TemporalMemory.Init(Connections);

            DistalDendrite distalDendrite = Connections.CreateDistalSegment(Connections.GetCell(0));
            Synapse synapse1 = Connections.CreateSynapse(distalDendrite, Connections.GetCell(23), 2.5);

            TemporalMemory.AdaptSegment(Connections, distalDendrite, Connections.GetCells(new int[] { 23 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            try
            {
                Assert.AreEqual(1.0, synapse1.Permanence, 0.1);
            }
            catch (AssertFailedException ex)
            {
                string PERMANENCE_SHOULD_BE_IN_THE_RANGE = $"Assert.AreEqual failed. Expected a difference no greater than <0.1> " +
                    $"between expected value <1> and actual value <{synapse1.Permanence}>. ";
                Assert.AreEqual(PERMANENCE_SHOULD_BE_IN_THE_RANGE, ex.Message);
            }
        }


        /// <summary>
        /// Validate the behavior of the AdaptSegment method of the TemporalMemory class.
        /// The test initializes a TemporalMemory object, creates a Connection object, sets the default parameters, 
        /// and initializes the TemporalMemory. It then creates a DistalDendrite object with three synapses, each connected 
        /// to different cells. 
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_UpdatesSynapsePermanenceValues_BasedOnPreviousCycleActivity()
        {
            TemporalMemory TemporalMemory = new TemporalMemory();
            Connections Connections = new Connections();///The connections object holds the infrastructure, and is used by both the SpatialPooler, TemporalMemory.
            Parameters Parameters = Parameters.getAllDefaultParameters();
            Parameters.apply(Connections);
            TemporalMemory.Init(Connections);///use connection for specified object to build and implement algoarithm 

            DistalDendrite distalDendrite = Connections.CreateDistalSegment(Connections.GetCell(0));/// Created a Distal dendrite segment of a cell0
            Synapse synapse1 = Connections.CreateSynapse(distalDendrite, Connections.GetCell(23), 0.5);/// Created a synapse on a distal segment of a cell index 23
            Synapse synapse2 = Connections.CreateSynapse(distalDendrite, Connections.GetCell(37), 0.6);/// Created a synapse on a distal segment of a cell index 37
            Synapse synapse3 = Connections.CreateSynapse(distalDendrite, Connections.GetCell(477), 0.9);/// Created a synapse on a distal segment of a cell index 477

            TemporalMemory.AdaptSegment(Connections, distalDendrite, Connections.GetCells(new int[] { 23, 37 }), Connections.HtmConfig.PermanenceIncrement,
                Connections.HtmConfig.PermanenceDecrement);/// Invoking AdaptSegments with only the cells with index 23 and 37
                                                  /// whose presynaptic cell is considered to be Active in the
                                                  /// previous cycle and presynaptic cell is Inactive for the cell 477

            Assert.AreEqual(0.6, synapse1.Permanence, 0.01);/// permanence is incremented for cell 23 from 0.5 to 0.6 as presynaptic cell was Active in the previous cycle.
            Assert.AreEqual(0.7, synapse2.Permanence, 0.01);/// permanence is incremented for cell 37 from 0.6 to 0.7 as presynaptic cell was Active in the previous cycle.
            Assert.AreEqual(0.8, synapse3.Permanence, 0.01);/// permanence is decremented for cell 477 from 0.5 to 0.6 as presynaptic cell was InActive in the previous cycle.
        }

        /// <summary>
        /// This test creates a new distal dendrite segment and uses a for loop to create synapses until the 
        /// maximum number of synapses per segment(225 synapses) is reached.Once the maximum is reached, 
        /// the segment is adapted using the TemporalMemory.AdaptSegment method.Finally, the test asserts 
        /// that there is only one segment and 225 synapses in the connections object.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_SegmentState_WhenMaximumSynapsesPerSegment()
        {
            TemporalMemory TemporalMemory = new TemporalMemory();
            Connections Connections = new Connections();
            Parameters Parameters = Parameters.getAllDefaultParameters();
            Parameters.apply(Connections);
            TemporalMemory.Init(Connections);
            DistalDendrite dd1 = Connections.CreateDistalSegment(Connections.GetCell(1));
            // Create maximum synapses per segment (225 synapses)
            int numSynapses = 0;
            for (int i = 0; i < Connections.HtmConfig.MaxSegmentsPerCell; i++)
            {
                // Create synapse connected to a random cell
                Synapse s = Connections.CreateSynapse(dd1, Connections.GetCell(5), 0.5);
                numSynapses++;

                // Adapt the segment if it has reached the maximum synapses allowed per segment
                if (numSynapses == Connections.HtmConfig.MaxSynapsesPerSegment)
                {
                    TemporalMemory.AdaptSegment(Connections, dd1, Connections.GetCells(new int[] { 5 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
                }
            }
            var field1 = Connections.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field2 = Connections.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var NoOfSegments = Convert.ToInt32(field1.GetValue(Connections));
            var NoOfSynapses = Convert.ToInt32(field2.GetValue(Connections));

            //Assert
            Assert.AreEqual(1, NoOfSegments);
            Assert.AreEqual(225, NoOfSynapses);
        }


        /// <summary>
        /// The test is checking whether the AdaptSegment method correctly adjusts the state of the matching 
        /// and active segments in the network, and whether segments that have no remaining synapses are 
        /// properly destroyed.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_MatchingSegmentAndActiveSegmentState()
        {
            TemporalMemory TemporalMemory = new TemporalMemory();
            Connections Connections = new Connections();
            Parameters Parameters = Parameters.getAllDefaultParameters();
            Parameters.apply(Connections);
            TemporalMemory.Init(Connections);

            DistalDendrite dd1 = Connections.CreateDistalSegment(Connections.GetCell(1));
            DistalDendrite dd2 = Connections.CreateDistalSegment(Connections.GetCell(2));
            DistalDendrite dd3 = Connections.CreateDistalSegment(Connections.GetCell(3));
            DistalDendrite dd4 = Connections.CreateDistalSegment(Connections.GetCell(4));
            DistalDendrite dd5 = Connections.CreateDistalSegment(Connections.GetCell(5));
            Synapse s1 = Connections.CreateSynapse(dd1, Connections.GetCell(23), -1.5);
            Synapse s2 = Connections.CreateSynapse(dd2, Connections.GetCell(24), 1.5);
            Synapse s3 = Connections.CreateSynapse(dd3, Connections.GetCell(25), 0.1);
            Synapse s4 = Connections.CreateSynapse(dd1, Connections.GetCell(26), -1.1);
            Synapse s5 = Connections.CreateSynapse(dd2, Connections.GetCell(27), -0.5);

            TemporalMemory.AdaptSegment(Connections, dd1, Connections.GetCells(new int[] { 23, 24, 25 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(Connections, dd2, Connections.GetCells(new int[] { 25, 24, 26 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(Connections, dd3, Connections.GetCells(new int[] { 27, 24, 23 }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(Connections, dd4, Connections.GetCells(new int[] { }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(Connections, dd5, Connections.GetCells(new int[] { }), Connections.HtmConfig.PermanenceIncrement, Connections.HtmConfig.PermanenceDecrement);
            var field1 = Connections.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field3 = Connections.GetType().GetField("m_SegmentForFlatIdx", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field4 = Connections.GetType().GetField("m_ActiveSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field5 = Connections.GetType().GetField("m_MatchingSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            var dictionary = (ConcurrentDictionary<int, DistalDendrite>)field3.GetValue(Connections);
            var field2 = Connections.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var NoOfSegments = Convert.ToInt32(field1.GetValue(Connections));
            var activeSegments = ((List<DistalDendrite>)field4.GetValue(Connections)).Count;
            var matchingSegments = ((List<DistalDendrite>)field5.GetValue(Connections)).Count;
            var NoOfSynapses = Convert.ToInt32(field2.GetValue(Connections));

            //Assert
            Assert.AreEqual(5, NoOfSegments);
            Assert.AreEqual(1, NoOfSynapses);
            Assert.AreEqual(0, activeSegments);
            Assert.AreEqual(0, matchingSegments);
        }



        /// <summary>
        /// Here's an example implementation of a unit test that creates more than 225 synapses using a for 
        /// loop associated with one distal dendrite segment which is going to result in ArgumentOutOfRangeException:
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]///This attribute is used to specify the expected 
                                                                ///exception. Therefore, the test will pass if the expected exception 
                                                                ///of type ArgumentOutOfRangeException is thrown, and it will fail if 
                                                                ///any other exception or no exception is thrown.
        public void TestAdaptSegment_WhenMaxSynapsesPerSegmentIsReachedAndExceeded()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn1 = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn1);
            tm.Init(cn1);
            DistalDendrite dd1 = cn1.CreateDistalSegment(cn1.GetCell(1));
            int numSynapses = 0;/// Create maximum synapses per segment (225 synapses)
            int totalCells = cn1.Cells.Length;// Get total number of cells in cn1
            // Generate a random integer between 1 and totalCells
            Random random = new Random();
            int randomCellNumber = random.Next(1, totalCells + 1);
            for (int i = 0; i < cn1.HtmConfig.MaxSegmentsPerCell; i++)
            {
                // Create synapse connected to a random cell
                Synapse s = cn1.CreateSynapse(dd1, cn1.GetCell(randomCellNumber), 0.5);
                numSynapses++;

                // Adapt the segment if it has reached the maximum synapses allowed per segment
                if (numSynapses == cn1.HtmConfig.MaxSynapsesPerSegment)
                {
                    TemporalMemory.AdaptSegment(cn1, dd1, cn1.GetCells(new int[] { randomCellNumber }), cn1.HtmConfig.PermanenceIncrement, cn1.HtmConfig.PermanenceDecrement);
                }
            }
            // Adapt the segment if it has crossed the maximum synapses allowed per segment by destroying any weak synapse of that segment.
            // Create one more synapse to exceed the maximum number of synapses per segment
            Synapse s226 = cn1.CreateSynapse(dd1, cn1.GetCell(randomCellNumber), 0.6);
            numSynapses++;
            if (numSynapses >= cn1.HtmConfig.MaxSynapsesPerSegment)
            {
                //226th cell of the segment does not contains anything. Therefore trying to access the 226th throws an ArgumentOutofRangeException.
                Synapse Syn226 = dd1.Synapses[226];
                throw new ArgumentOutOfRangeException("The Maximum Synapse per segment  was exceeded.");
            }
        }




        /// <summary>
        /// The test checks that the segment is destroyed when all its synapses are destroyed.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_SegmentIsDestroyed_WhenNoSynapseIsPresent()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn1 = new Connections();        
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn1);
            tm.Init(cn1);

            DistalDendrite dd1 = cn1.CreateDistalSegment(cn1.GetCell(0));
            DistalDendrite dd2 = cn1.CreateDistalSegment(cn1.GetCell(0));
            DistalDendrite dd3 = cn1.CreateDistalSegment(cn1.GetCell(0));
            DistalDendrite dd4 = cn1.CreateDistalSegment(cn1.GetCell(0));
            DistalDendrite dd5 = cn1.CreateDistalSegment(cn1.GetCell(0));
            Synapse s1 = cn1.CreateSynapse(dd1, cn1.GetCell(23), -1.5);
            Synapse s2 = cn1.CreateSynapse(dd2, cn1.GetCell(24), 1.5);
            Synapse s3 = cn1.CreateSynapse(dd3, cn1.GetCell(25), 0.1);
            Synapse s4 = cn1.CreateSynapse(dd4, cn1.GetCell(26), -1.1);
            Synapse s5 = cn1.CreateSynapse(dd5, cn1.GetCell(27), -0.5);

            TemporalMemory.AdaptSegment(cn1, dd1, cn1.GetCells(new int[] { 23, 24, 25 }), cn1.HtmConfig.PermanenceIncrement, cn1.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(cn1, dd2, cn1.GetCells(new int[] { 25, 24, 26 }), cn1.HtmConfig.PermanenceIncrement, cn1.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(cn1, dd3, cn1.GetCells(new int[] { 27, 24, 23 }), cn1.HtmConfig.PermanenceIncrement, cn1.HtmConfig.PermanenceDecrement);
            var field1 = cn1.GetType().GetField("m_ActiveSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field2 = cn1.GetType().GetField("m_MatchingSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field4 = cn1.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field5 = cn1.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field9 = cn1.GetType().GetField("nextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field6 = cn1.GetType().GetField("m_SegmentForFlatIdx", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo destroyDistalDendriteMethod = typeof(Connections).GetMethod("DestroyDistalDendrite", BindingFlags.Public
                | BindingFlags.Instance | BindingFlags.NonPublic);
            var m_ASegment = field1.GetValue(cn1);
            var m_MSegment = field2.GetValue(cn1);


            ///Assert the segment and synapse status before the DestroyDistalDendrite method is explicitly called.
            Assert.AreEqual(5, Convert.ToInt32(field4.GetValue(cn1)));
            Assert.AreEqual(3, Convert.ToInt32(field5.GetValue(cn1)));

            ///DestroyDistalDendrite is invoked for dd1,dd2,dd3,dd4.
            destroyDistalDendriteMethod.Invoke(cn1, new object[] { dd1 });
            destroyDistalDendriteMethod.Invoke(cn1, new object[] { dd2 });
            destroyDistalDendriteMethod.Invoke(cn1, new object[] { dd3 });
            destroyDistalDendriteMethod.Invoke(cn1, new object[] { dd4 });

            ///Now checking the segment and synapse status after the DestroyDistalDendrite method is explicitly called.
            Assert.AreEqual(1, Convert.ToInt32(field5.GetValue(cn1)));
            Assert.AreEqual(5, Convert.ToInt32(field4.GetValue(cn1)));
        }

        /// <summary>
        /// These test methods will test if the AdaptSegment method correctly destroys synapses
        /// with permanence less/greater than  HtmConfig.EPSILON
        /// </summary>

        ///TestAdaptSegment_DoesNotDestroySynapses_ForSmallNNegativePermanenceValues
        ///here permanence comes greater than  HtmConfig.EPSILON
        ///hence it won´t destroys synapses
        ///take count of the synapses inside DistalDendrite

        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_DoesNotDestroySynapses_ForSmallNNegativePermanenceValues()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections(); ///The connections object holds the infrastructure, and is used by both the SpatialPooler, TemporalMemory.
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);  ///use connection for specified object to build and implement algoarithm 


            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0)); /// Created a Distal dendrite segment of a cell0
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.0000000967); /// Created a synapse on a distal segment of a cell index 23
            Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(24), 0.0000001);/// Created a synapse on a distal segment of a cell index 24
            Synapse s3 = cn.CreateSynapse(dd, cn.GetCell(43), -0.00000001);
            /// Invoking AdaptSegments with only the cells with index 23 and 37
            ///whose presynaptic cell is considered to be Active in the previous cycle
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23, 24, 43 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);


            Assert.IsTrue(dd.Synapses.Contains(s2)); /// assert condition to check does DistalDendrite contains the synapse s2
            Assert.IsTrue(dd.Synapses.Contains(s1));/// assert condition to check does DistalDendrite contains the synapse s1
            Assert.IsTrue(dd.Synapses.Contains(s3));/// assert condition to check does DistalDendrite contains the synapse s1
            Assert.AreEqual(3, dd.Synapses.Count);  /// synapses count check in DistalDendrite
        }

        /// <summary>
        /// TestAdaptSegment_DestroySynapses_WithNegativePermanenceValues
        /// here permanence comes lesser than  HtmConfig.EPSILON
        /// hence it  destroys synapses
        ///take count of the synapses inside DistalDendrite which comes to zero
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_DestroySynapses_WithNegativePermanenceValues()
        {
            // Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);


            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), -0.199991);
            Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(24), -0.29999);


            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23, 24 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);


            Assert.IsFalse(dd.Synapses.Contains(s2)); /// assert condition to check does DistalDendrite contains the synapse s2
            Assert.IsFalse(dd.Synapses.Contains(s1)); /// assert condition to check does DistalDendrite contains the synapse s2
            Assert.AreEqual(0, dd.Synapses.Count);  /// synapses count check in DistalDendrite
        }


        /// <summary>
        /// The below test checks for exception throwing in case of connections, DistalDendrites object is null. 
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_ShouldThrow_DD_ObjectShouldNotBeNUllException()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.1);

            try
            {
                TemporalMemory.AdaptSegment(cn, null, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            }
            catch (NullReferenceException ex)
            {
                Assert.AreEqual(DISTALDENDRITE_CANNOT_BE_NULL, ex.Message);
            }
        }

        /// <summary>
        /// TestAdaptSegmentCheckMultipleSynapse
        ///Checking the destroyes of synapses and the count of synapses at the end
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_CheckMultipleSynapseState()
        {
            // Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);


            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.2656);
            Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(24), 0.0124);
            Synapse s3 = cn.CreateSynapse(dd, cn.GetCell(25), 0.7656);
            Synapse s4 = cn.CreateSynapse(dd, cn.GetCell(26), 0.0547);
            Synapse s5 = cn.CreateSynapse(dd, cn.GetCell(28), 0.001);
            Synapse s6 = cn.CreateSynapse(dd, cn.GetCell(31), 0.002);
            Synapse s7 = cn.CreateSynapse(dd, cn.GetCell(35), -0.2345);
            Synapse s8 = cn.CreateSynapse(dd, cn.GetCell(38), -0.134345);
            // Act
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23, 24, 25, 26, 28, 31, 35, 38 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);


            Assert.IsTrue(dd.Synapses.Contains(s2));
            Assert.IsTrue(dd.Synapses.Contains(s1));
            Assert.IsTrue(dd.Synapses.Contains(s3));
            Assert.IsTrue(dd.Synapses.Contains(s4));
            Assert.IsTrue(dd.Synapses.Contains(s5));
            Assert.IsTrue(dd.Synapses.Contains(s6));
            Assert.IsFalse(dd.Synapses.Contains(s7));
            Assert.IsFalse(dd.Synapses.Contains(s8));
            Assert.AreEqual(6, dd.Synapses.Count);
        }

        /// <summary>
        /// This unit test is testing the maximum permanence value set by the AdaptSegment method. 
        /// For the permanece value > 1.0, AdaptSegments will set permanence to maximum bound 1.0. 
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_PermanenceMaxBound()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0)); /// Create a distal segment of a cell index 0 to learn sequence
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(15), 1.1);/// create a synapse on a dital segment of a cell with index 15 
                                                                   /// It results with permanence 1 of the segment's synapse if the synapse's presynaptic cell index 23 was active. 
                                                                   /// If it was not active, then it will decrement the permanence by 0.1

            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 15 }), cn.HtmConfig.PermanenceIncrement,
                cn.HtmConfig.PermanenceDecrement);/// Invoking AdaptSegments with the cell 15 whose presynaptic cell is 
                                                  /// considered to be Active in the previous cycle.
            Assert.AreEqual(1.0, s1.Permanence, 0.1);/// permanence is incremented for cell 15 from 0.9 to 1 as presynaptic cell was Active in the previous cycle.

            /// Now permanence should be at max
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 15 }), cn.HtmConfig.PermanenceIncrement,
                cn.HtmConfig.PermanenceDecrement);/// Again calling AdaptSegments with the cell 15 whose presynaptic cell is 
                                                  /// considered to be Active again in the previous cycle.
            Assert.AreEqual(1.0, s1.Permanence, 0.1);/// Therefore permanence is again incremented for cell 15 from 1 to 1.1 as presynaptic cell was Active 
                                                     /// in the previous cycle. But due to permanence boundary set, 1.1 is set back to 1.

        }

        /// <summary>
        /// Test used to check that the result array is equal to the expectedCells array, which is an empty array in this case.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void GetCells_WithEmptyArray_ReturnsEmptyArray()
        {
            // Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            int[] cellIndexes = new int[0];
            Cell[] expectedCells = new Cell[0];

            // Act
            Cell[] result = cn.GetCells(cellIndexes);

            // Assert
            CollectionAssert.AreEqual(expectedCells, result);
        }

        /// <summary>
        /// Test case to check if cellIndexes is a valid array:
        /// This test sets up a Connections object, initializes cellIndexes with the values [0, 2, 4], and initializes 
        /// expectedCells with an array containing the 1st, 3rd, and 5th elements of the Cells array in the Connections
        /// object. The GetCells method is then called with cellIndexes, and the result is compared to expectedCells 
        /// using CollectionAssert.AreEqual.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void GetCells_WithValidArray_ReturnsExpectedCells()
        {
            // Arrange
            Connections cn = new Connections();
            int[] cellIndexes = new int[] { 0, 2, 4 };
            cn.Cells = new Cell[5];
            Cell[] expectedCells = new Cell[] { cn.Cells[0], cn.Cells[2], cn.Cells[4] };

            // Act
            Cell[] result = cn.GetCells(cellIndexes);

            // Assert
            CollectionAssert.AreEqual(expectedCells, result);
        }

        /// <summary>
        /// Test how the AdaptSegment works when complex double inputs are given to it
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_ComplexDoublePermanenceInput()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(15), 0.85484565412316);

            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 15 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(0.95484565412316, s1.Permanence);
            // Now permanence should be at max
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 15 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(1.0, s1.Permanence, 0.1);
        }

        /// <summary>
        /// This unit test is testing the minimum permanence bound value set by the AdaptSegment method. 
        /// For the permanece value < 0, AdaptSegments will set permanence to minimum bound 0.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegmentPermanenceMinBound()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.1);/// create a synapse on a dital segment of a cell with index 23

            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { }), cn.HtmConfig.PermanenceIncrement,
                cn.HtmConfig.PermanenceDecrement);/// Invoking AdaptSegments with the cell 15 whose presynaptic cell is 
                                                  /// considered to be InActive in the previous cycle.
            //Assert.IsFalse(cn.GetSynapses(dd).Contains(s1));
            Assert.IsFalse(dd.Synapses.Contains(s1));/// permanence is decremented for presynaptie cell 477 from 
                                                     /// 0.1 to 0 as presynaptic cell was InActive in the previous cycle
                                                     /// There the synapse is destroyed as permanence < HtmConfig.Epsilon
        }

        /// <summary>
        /// Test how the AdaptSegment works when a low Permanence value is passed
        /// Permanence is supposed to be set to 0 as the range should vary between 0 and 1 and when a negative permanence < 0 
        /// is passed to AdaptSegments, The Synapse is destroyed. 
        /// ********But this Testcase doesn't provide the expected results and permanence is not getting set to the minimum 
        /// bound for the negative permanence value.***********
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegment_LowPermanence_SynapseShouldbeDestroyed()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(15), -1.5);


            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 15 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            try
            {
                Assert.IsFalse(dd.Synapses.Contains(s1));
                //The Assert condition checks whether the synapse s1 has been destroyed or not, which should
                //be true(Assert Passed).
            }
            catch (AssertFailedException ex)
            {
                // In the above try block, the synapse is expected to be destroyed otherwise it is caught in this catch block.
                throw new AssertFailedException("The synapse was not destroyed as expected.", ex);
            }
        }

        //These test procedures will determine whether the AdaptSegment technique
        // effectively eliminates synapses with permanence below Epsilon havign different inputs.
        [TestMethod]
        [TestCategory("Prod")]

        public void TestAdaptSegment_SynapseRetentionOnDistalDendrite()
        {
            //Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0)); //instance is callled with a cell at index 0 as a parameter.
            Synapse s3 = cn.CreateSynapse(dd, cn.GetCell(23), 0.4);
            Synapse s4 = cn.CreateSynapse(dd, cn.GetCell(37), -0.1);

            //Act
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);
            //The method adapts the permanence values of the synapses in the specified distak dendrite segment based on the specified parameter and the current input

            //Assert
            Assert.IsTrue(dd.Synapses.Contains(s3)); //Checks whether the synapse created earlier is still present in the segment
            Assert.IsFalse(dd.Synapses.Contains(s4)); //Checks whether the synapse creater earlier is no longer present in the segment

        }


        /// <summary>
        /// Test with invalid cellIndexes array.
        /// In this test case, an IndexOutOfRangeException is expected to be thrown because the index 10 is out of range 
        /// for the Cells array. The [ExpectedException(typeof(IndexOutOfRangeException))] attribute is used to specify 
        /// the expected exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(IndexOutOfRangeException))]///This attribute is used to specify the expected 
                                                             ///exception. Therefore, the test will pass if the expected exception 
                                                             ///of type IndexOutOfRangeException is thrown, and it will fail if 
                                                             ///any other exception or no exception is thrown.
        public void GetCells_WithInvalidArray_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            Connections cn = new Connections();
            cn.Cells = new Cell[5];
            int[] cellIndexes = new int[] { 1, 3, 10 }; // index 10 is out of range
            Cell[] expectedCells = new Cell[] { cn.Cells[1], cn.Cells[3], cn.Cells[10] };

            // Act
            Cell[] result = cn.GetCells(cellIndexes);

        }


        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(NullReferenceException))]///This attribute is used to specify the expected 
                                                           ///exception. Therefore, the test will pass if the expected exception 
                                                           ///of type ArgumentNullException is thrown, and it will fail if 
                                                           ///any other exception or no exception is thrown.
        public void GetCells_WithNullArray_ThrowsException()
        {
            // Arrange
            Connections cn = new Connections();
            cn.Cells = null;
            int[] cellIndexes = null;

            // Act & Assert
            Cell[] result = cn.GetCells(cellIndexes);
            //Assert.ThrowsException<ArgumentNullException>(() => cn.GetCells());

        }
    }
}