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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using System.Data;
using System.IO;
using System;



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
                InputDimensions = new int[] { 8 },
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

        
        private SpatialPooler sp;
        private Connections mem;
        private void InitTestSPInstanceEmpty(HtmConfig htmConfig)
        {
            sp = new SpatialPoolerMT();
            mem = new Connections(htmConfig);
            sp.Init(mem);
        }
        /// <summary>
        /// The objective of this test case is to verify how the AdaptSynapses method of an HTM (Hierarchical Temporal Memory) system handles a scenario where the input vector is null. 
        /// The purpose is to ensure that the system robustly responds to this error condition, 
        /// ideally by throwing a NullReferenceException with a specific error message indicating that the input vector cannot be null.
        /// </summary>


        private const string InputVector_Cannot_be_Null = "Object reference not set to an instance of an object.";
        [TestMethod]
        [TestCategory("Prod")]

        public void TestAdaptSynapsesWithNullInputVector()
        {
            // Set up the Htmcongif parameters
            var htmConfig = SetupHtmConfigDefaultParameters();
            mem = new Connections(htmConfig);
            mem.HtmConfig.InputDimensions = new int[] { 8 };
            mem.HtmConfig.ColumnDimensions = new int[] { 4 };
            mem.HtmConfig.SynPermActiveInc = 0.1;
            mem.HtmConfig.SynPermInactiveDec = 0.01;
            InitTestSPInstanceEmpty(htmConfig);

            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            int[] NullInput = null;
            int[] activeColumns = new int[] { 0, 1, 2 };



            // Assert.ThrowsException<NullReferenceException>(() => sp.AdaptSynapses(mem, NullInput, activeColumns));

            try
            {
                sp.AdaptSynapses(mem, NullInput, activeColumns);
            }
            catch (NullReferenceException ex)
            {
                Assert.AreEqual(InputVector_Cannot_be_Null, ex.Message);
            }

        }

        private const string InputVector_Invalid = "Object reference not set to an instance of an object.";


        /// <summary>
        /// The objective of this test is to evaluate how the AdaptSynapses method in the SpatialPooler handles an input vector under typical conditions.
        /// The test checks for the correct handling of input values, ensuring the method doesn't throw unexpected exceptions when processing standard integer values within the input vector. 
        /// </summary>

        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithInvalidInputVector1()
        {
            // Set up the Htmcongif parameters
            var htmConfig = SetupHtmConfigDefaultParameters();
            mem = new Connections(htmConfig);
            mem.HtmConfig.InputDimensions = new int[] { 8 };
            mem.HtmConfig.ColumnDimensions = new int[] { 4 };
            mem.HtmConfig.SynPermActiveInc = 0.1;
            mem.HtmConfig.SynPermInactiveDec = 0.01;
            InitTestSPInstanceEmpty(htmConfig);

            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            int[] Input = { 0, 1, 2, 3, 2, 1, 2, 3 };
            Input[2] = (int)1.5;  // Set a non-integer value
            int[] activeColumns = new int[] { 0, 1, 2 };


            try
            {
                sp.AdaptSynapses(mem, Input, activeColumns);
            }
            catch (NullReferenceException ex)
            {
                Assert.AreEqual(InputVector_Invalid, ex.Message);
            }
        }

        /// </summary>
        /// Test case for validating synapse adaptation with small-dimensional inputs
        /// The objective of this test case is to evaluate how the AdaptSynapses method in an HTM (Hierarchical Temporal Memory) system handles a scenario with unusually small input and column dimensions. 
        /// Specifically, the test is designed to verify that the method can correctly update synaptic permanences when dealing with a minimal setup where the input vector and the number of columns are smaller than what might be typically encountered in practical applications.
        /// </summary>

        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithSmallDimension()
        {
            // Set up the parameters
            var htmConfig = SetupHtmConfigDefaultParameters();
            mem = new Connections(htmConfig);
            mem.HtmConfig.InputDimensions = new int[] { 2 };
            mem.HtmConfig.ColumnDimensions = new int[] { 3 };
            mem.HtmConfig.SynPermActiveInc = 0;
            mem.HtmConfig.SynPermInactiveDec = 1;
            InitTestSPInstanceEmpty(htmConfig);

            // Define the input vector and active columns
            int[] inputVector = new int[] { 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            // Run the AdaptSynapses method
            sp.AdaptSynapses(mem, inputVector, activeColumns);


            // Check that the permanences were updated correctly
            var column = mem.GetColumn(0);
            var permanences = column.ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);

            // the expected permanences should be SynPermInactiveDec for input 1 and SynPermActiveInc for input 2

            double expectedPermanence1 = mem.HtmConfig.SynPermInactiveDec;
            double expectedPermanence2 = mem.HtmConfig.SynPermActiveInc;

            Assert.AreEqual(expectedPermanence2, permanences[1]);
        }

        /// <summary>
        /// The objective of this test case is designed to evaluate the AdaptSynapses method within a Hierarchical Temporal Memory (HTM) system, 
        /// specifically under the condition of zero overlap between the input vector and the active columns. 
        /// The core purpose is to confirm that the method correctly decrements the synapse permanence values when there are no active bits in the input vector, 
        /// meaning there's no overlap between the inputs and the columns deemed active.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(AssertFailedException))]///the tests are expected to throw an AssertFailedException to pass
        public void TestAdaptSynapsesWithZeroOverlap()
        {
            // Instantiate a new HTM config for each test to ensure no shared state
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.SynPermInactiveDec = 0.1;
            htmConfig.SynPermConnected = 0.2;
            htmConfig.SynPermMin = 0.0;

            // Instantiate a new Connections object for each test
            mem = new Connections(htmConfig);

            // No need to call InitTestSPInstanceEmpty because we are instantiating new objects above

            // Define the input vector with zero overlap - all bits inactive
            int[] inputVector = new int[htmConfig.InputDimensions[0]]; // All bits are set to '0'
            int[] activeColumns = Enumerable.Range(0, htmConfig.ColumnDimensions[0]).ToArray();

            // Instantiate a new SpatialPooler instance for each test
            SpatialPooler sp = new SpatialPooler();

            // Initialize the spatial pooler - any state that might cause a duplicate key error should be cleared here
            sp.Init(mem);

            // Run the AdaptSynapses method
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            // Verify that the permanences have been decremented for zero overlap
            foreach (int columnIndex in activeColumns)
            {
                Column column = mem.GetColumn(columnIndex);
                // Directly access the synapses and check their permanence
                foreach (Synapse synapse in column.ProximalDendrite.Synapses)
                {
                    // We expect the permanence to have decreased due to zero overlap
                    Assert.IsTrue(synapse.Permanence < htmConfig.SynPermConnected,
                        $"Permanence did not decrement as expected for synapse connected to input index {synapse.InputIndex} in column {columnIndex}.");
                }
            }
        }

        /// <summary>
        /// Validates that the AdaptSynapses method throws an exception for out-of-range synaptic permanence increment and decrement values.
        /// This test verifies the method's robustness against invalid input by providing values outside the acceptable range (0, 1) for
        /// SynPermActiveInc and SynPermInactiveDec. It expects an ArgumentException to be thrown, indicating the method's capability to
        /// handle such erroneous conditions, thus preventing unintended behavior in the spatial pooling process.
        /// </summary>

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [TestCategory("Prod")]
        public void ValidateSynapticPermanenceAdjustmentWithOutOfRangeValues()
        {

            // Instantiate a new HTM config for each test to ensure no shared state
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermActiveInc = -0.01;
            htmConfig.SynPermInactiveDec = 1.1;
            htmConfig.SynPermConnected = 0.2;
            htmConfig.SynPermMin = 0.0;

            // Instantiate a new Connections object for each test
            mem = new Connections(htmConfig);

            InitTestSPInstanceEmpty(htmConfig);
            mem.HtmConfig.SynPermTrimThreshold = 0.05;

            int[] inputVector = { 0, 1, 2, 3, 2, 1, 2, 3 };
            int[] activeColumns = { 0, 1, 2 };

            // Instantiate a new SpatialPooler instance for each test
            SpatialPooler sp = new SpatialPooler();

            // Initialize the spatial pooler - any state that might cause a duplicate key error should be cleared here
            sp.Init(mem);


            // Expecting an ArgumentException due to invalid SynPermActiveInc and SynPermInactiveDec values
            Assert.ThrowsException<System.ArgumentException>(() => {
                sp.AdaptSynapses(mem, inputVector, activeColumns);
            }, "Expected an ArgumentException for out-of-range SynPermActiveInc or SynPermInactiveDec values.");
        }

        /// <summary>
        /// Verifies the synaptic adaptation functionality within an HTM system by simulating the response to a specific input and set of active columns.
        /// Test case scenario: Providing a valid 8x4 input matrix to evaluate the synaptic adaptation mechanisms. The test initializes the potential pools for each column and sets the initial permanences.
        /// It then applies an input vector along with specified active column indices to adapt the synapses' permanence values.
        /// The adaptation is verified by comparing the post-adaptation permanences against expected values, ensuring the system's ability to adjust synaptic strengths according to the input patterns and active column selections, 
        /// thereby validating the core functionality of synaptic plasticity within the spatial pooling process.
        /// </summary>

        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesPotentialPool()
        {
            // Instantiate a new HTM config for each test to ensure no shared state
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermActiveInc = 0.01;
            htmConfig.SynPermInactiveDec = 0.1;
            htmConfig.SynPermConnected = 0.2;
            htmConfig.SynPermMin = 0.0;

            // Instantiate a new Connections object for each test
            mem = new Connections();

            InitTestSPInstanceEmpty(htmConfig);
            mem.HtmConfig.SynPermTrimThreshold = 0.05;


            int[][] potentialPools = new int[][] {
                    new int[]{ 1, 1, 1, 0, 0, 0, 0, 0 },
                    new int[]{ 0, 1, 1, 1, 0, 0, 0, 0 },
                    new int[]{ 0, 0, 1, 1, 1, 0, 0, 0 },
                    new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }

                };

            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.000, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.000, 0.017, 0.232, 0.400, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.051, 0.730, 0.000, 0.000, 0.000 },
            new double[]{ 0.170, 0.000, 0.000, 0.000, 0.000, 0.000, 0.380, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.000, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.000, 0.000, 0.222, 0.500, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.000, 0.000, 0.000, 0.151, 0.830, 0.000, 0.000, 0.000 },
            new double[] { 0.170, 0.000, 0.000, 0.000, 0.000, 0.000, 0.380, 0.000 }
            };


            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                List<int> indexList = new List<int>(); // Create a list to hold the indices

                // Loop through the potentialPools array for the current column
                for (int j = 0; j < potentialPools[i].Length; j++)
                {
                    if (potentialPools[i][j] == 1) // Check if the condition is met
                    {
                        indexList.Add(j); // Add the index to the list
                    }
                }

                int[] indexes = indexList.ToArray(); // Convert the list to an array

                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }


            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.9);
                }
            }

        }


        

        public static IEnumerable<object[]> AdditionData  //Member data-driven
        {
            get
            {
                return new[]
                {
                new object[] { 2, 38, 0.6 },
                new object[] { 2, 42, 0.55 },
                new object[] { 3, 73, 0.2 },
                new object[] { 7, 10, 0.9 },

                };
            }

        }
        /// <summary>
        /// This unit test method is designed to verify the behavior of adapting segment permanence based on the activity of presynaptic cells.
        /// It utilizes member data-driven testing with a predefined set of test cases provided through the `AdditionData` property.
        /// For each test case, it sets up a temporal memory environment with default parameters and creates a distal dendrite and an active cell 
        /// based on the provided cell numbers. It then creates a synapse between the distal dendrite and the active cell with the specified initial 
        /// permanence. Next, it simulates the activation of presynaptic cells represented by a list of random active cell numbers. Depending on whether 
        /// the active cell is included in the list of active cells, it checks if the permanence of the synapse increases by the permanence increment 
        /// or decreases by the permanence decrement, and asserts the expected outcome accordingly.
        /// </summary>
        /// <param name="getCellnumber"></param>
        /// <param name="activeCellnumber"></param>
        /// <param name="initialPermanence"></param>
        [TestMethod]
        [DynamicData(nameof(AdditionData))]
        public void AdaptSegments_UnitTest_DecrementPermanenceIfInactivePresynapticCells(int getCellnumber, int activeCellnumber, double initialPermanence)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            //DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(2));
            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(getCellnumber));
            //Cell activeCell = conn.GetCell(38);
            Cell activeCell = conn.GetCell(activeCellnumber);


            Synapse synapse = conn.CreateSynapse(dd, activeCell, initialPermanence);
            List<int> randomActiveCellNumbers = new List<int> { 55 };
            List<Cell> ActiveCells = randomActiveCellNumbers.Select(cellNumber => conn.GetCell(cellNumber)).ToList();

            TemporalMemory.AdaptSegment(conn, dd, ActiveCells, conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            //synapse = dd.Synapses.First();

            if (dd.Synapses.Any())
            {
                synapse = dd.Synapses.First();
            }
            if (ActiveCells.Any(cell => cell.Index == activeCellnumber))
            {
                // Assert for initialPermanence + permanenceIncrement
                Assert.AreEqual(initialPermanence + conn.HtmConfig.PermanenceIncrement, synapse.Permanence);
            }
            else
            {
                // Assert for initialPermanence - permanenceDecrement
                Assert.AreEqual(initialPermanence - conn.HtmConfig.PermanenceDecrement, synapse.Permanence);
            }

        }
        /// <summary>
        /// This unit test method verifies the bounds of synapse permanence after adaptation based on the activity of presynaptic cells. 
        /// It utilizes inline data-driven testing with various combinations of active cell numbers and initial permanence values provided 
        /// as data rows using the DataRow attribute. For each test case, it sets up a temporal memory environment with default parameters 
        /// and creates a distal dendrite segment and an active cell based on the specified active cell number. Additionally, it creates 
        /// an inactive cell for comparison purposes. The method then creates a synapse between the segment and the active cell with the 
        /// given initial permanence value. Next, it simulates the adaptation process by activating the presynaptic cells represented by 
        /// a list containing only the active cell. After adaptation, the method asserts that the permanence of the synapse falls within 
        /// the expected range, which is defined with an allowed deviation of 0.1. This range ensures that the permanence remains bounded 
        /// within acceptable limits after adaptation.
        /// </summary>
        /// <param name="activeCellnum"></param>
        /// <param name="initialPermanence"></param>
        [TestMethod]
        //Inline Data driven test
        [DataRow(42, 5)]    // Test case with active cell number 42 and initial permanence 5
        [DataRow(88, 2.5)]  // Test case with active cell number 88 and initial permanence 2.5
        [DataRow(35, 100)]  // Test case with active cell number 35 and initial permanence 100
        public void AdaptSegments_UnitTest_VerifyPermanenceBoundsAfterAdaptation(double activeCellnum, double initialPermanence)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite segment = conn.CreateDistalSegment(conn.GetCell(4));
            Cell activeCell = conn.GetCell((int)activeCellnum);
            Cell inactiveCell = conn.GetCell(2);

            //double initialPermanence = 5; // Initial permanence value

            Synapse synapse = conn.CreateSynapse(segment, activeCell, initialPermanence);

            TemporalMemory.AdaptSegment(conn, segment, new List<Cell> { activeCell }, conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            // Assert
            Assert.AreEqual(1.0, synapse.Permanence, 0.1, "Permanence should be in the range");
        }

        /// <summary>
        /// This unit test method verifies the change in synapse permanence for the previous cycle based on the activity of presynaptic cells.
        /// It sets up a temporal memory environment with default parameters and creates a distal dendrite segment associated with a specific cell index.
        /// Three synapses are then created on the distal segment, each linked to different presynaptic cells with initial permanence values provided.
        /// Next, the method simulates the adaptation process by invoking the AdaptSegment method with only the presynaptic cells 
        /// from the previous cycle, assuming their activity status has changed. After adaptation, the expected changes in synapse permanence 
        /// are calculated based on the configured permanence increment and decrement values. Finally, the method asserts that the actual 
        /// permanence values of the synapses match the expected values within a tolerance range of 0.1, ensuring the correctness of the adaptation process.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_VerifyPermanenceChangeForPreviousCycle()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(7));/// Created a Distal dendrite segment of a cell0
            Synapse s1 = conn.CreateSynapse(dd, conn.GetCell(118), 0.7);/// Created a synapse on a distal segment of a cell index 23
            Synapse s2 = conn.CreateSynapse(dd, conn.GetCell(150), 0.2);/// Created a synapse on a distal segment of a cell index 37
            Synapse s3 = conn.CreateSynapse(dd, conn.GetCell(366), 0.3);/// 

            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 150, 366 }), conn.HtmConfig.PermanenceIncrement,
                conn.HtmConfig.PermanenceDecrement);/// Invoking AdaptSegments with only the cells with index 23 and 37
                                                    /// whose presynaptic cell is considered to be Active in the
                                                    /// previous cycle and presynaptic cell is Inactive for the cell 477

            double expectedS1Permanence = 0.7 - conn.HtmConfig.PermanenceDecrement; // Active
            double expectedS2Permanence = 0.2 + conn.HtmConfig.PermanenceIncrement; // Active
            double expectedS3Permanence = 0.3 + conn.HtmConfig.PermanenceIncrement; // Inactive

            // Assert
            Assert.AreEqual(expectedS1Permanence, s1.Permanence, 0.1);
            Assert.AreEqual(expectedS2Permanence, s2.Permanence, 0.1);
            Assert.AreEqual(expectedS3Permanence, s3.Permanence, 0.1);

        }
        /// <summary>
        /// This unit test method verifies the state of a distal dendrite segment after reaching the maximum number of synapses per segment.
        /// It sets up a temporal memory environment with default parameters and creates a distal dendrite segment associated with a specific cell index.
        /// Then, the method creates the maximum number of synapses allowed per segment (225 synapses) on the segment.
        /// After that, it simulates the adaptation process by invoking the AdaptSegment method with a set of presynaptic cells.
        /// The method then retrieves internal fields from the Connections object to examine the segment and synapse counts.
        /// Finally, it asserts that the segment count is as expected (1 segment) and the synapse count matches the maximum allowed synapses per segment.
        /// </summary>

        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_VerifySegmentStateAfterMaxSynapsesPerSegment()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd1 = conn.CreateDistalSegment(conn.GetCell(100));

            // Create maximum synapses per segment (225 synapses)
            int MaxSynapsesPerSegment = conn.HtmConfig.MaxSynapsesPerSegment;

            for (int i = 0; i < conn.HtmConfig.MaxSegmentsPerCell; i++)
            {
                conn.CreateSynapse(dd1, conn.GetCell(77), 0.2);
            }
            TemporalMemory.AdaptSegment(conn, dd1, conn.GetCells(new int[] { 7 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            var field1 = conn.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field2 = conn.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var segmentCount = Convert.ToInt32(field1.GetValue(conn));
            var synapseCount = Convert.ToInt32(field2.GetValue(conn));

            //Assert
            Assert.AreEqual(1, segmentCount, "Unexpected segment count");
            Assert.AreEqual(MaxSynapsesPerSegment, synapseCount, "Unexpected synapse count");
        }
        /// <summary>
        /// It verifies the state of distal dendrite segments and active segments after adaptation.
        /// It sets up a temporal memory environment with default parameters and creates multiple distal dendrite segments, each associated with a specific cell index.
        /// Then, it creates synapses on these segments with varying initial permanence values.
        /// Next, it simulates the adaptation process for each segment by invoking the AdaptSegment method with a set of active cells.
        /// The method then retrieves internal fields from the Connections object to examine segment and synapse counts, as well as active segment and matching segment counts.
        /// Finally, it asserts that the segment count matches the expected number, the synapse count is as expected, and there are no active segments or matching segments.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_VerifySegmentAndActiveSegmentStateAfterAdaptation()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite[] segments = new DistalDendrite[4];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = conn.CreateDistalSegment(conn.GetCell(76 + i));
            }
            Synapse[] synapses = new Synapse[5];
            int[] synapseCellIndices = { 36, 46, 56, 66, 76 };
            for (int i = 0; i < synapses.Length; i++)
            {
                synapses[i] = conn.CreateSynapse(segments[i % 4], conn.GetCell(synapseCellIndices[i]), i * 0.5 - 1.5);
            }

            for (int i = 0; i < segments.Length; i++)
            {
                int[] activeCells = { synapseCellIndices[i], synapseCellIndices[(i + 1) % synapseCellIndices.Length] };
                TemporalMemory.AdaptSegment(conn, segments[i], conn.GetCells(activeCells), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            }
            var field1 = conn.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field3 = conn.GetType().GetField("m_SegmentForFlatIdx", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field4 = conn.GetType().GetField("m_ActiveSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field5 = conn.GetType().GetField("m_MatchingSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var dictionary = (ConcurrentDictionary<int, DistalDendrite>)field3.GetValue(conn);
            var field2 = conn.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var GetSegmentCount = Convert.ToInt32(field1.GetValue(conn));
            var GetActiveSegments = ((List<DistalDendrite>)field4.GetValue(conn)).Count;
            var GetMatchingSegments = ((List<DistalDendrite>)field5.GetValue(conn)).Count;
            var GetSynapseCount = Convert.ToInt32(field2.GetValue(conn));

            //Assert
            Assert.AreEqual(4, GetSegmentCount, "Unexpected segment count");
            Assert.AreEqual(2, GetSynapseCount, "Unexpected synapse count");
            Assert.AreEqual(0, GetActiveSegments, "Unexpected active segment count");
            Assert.AreEqual(0, GetMatchingSegments, "Unexpected matching segment count");
        }
        /// <summary>
        /// Verifies the behavior of the AdaptSegment method when the maximum number of synapses per segment is reached and exceeded.
        /// It sets up a temporal memory environment with default parameters and creates a distal dendrite segment.
        /// Then, it iteratively creates synapses on this segment until the maximum number of synapses per segment is reached.
        /// If the maximum synapse count is reached, it invokes the AdaptSegment method to adapt the segment with a randomly selected cell.
        /// The test is expected to throw an ArgumentOutOfRangeException when the maximum synapse count is exceeded, indicating that the AdaptSegment method correctly handles the situation.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]///This attribute is used to specify the expected 
                                                                ///exception. Therefore, the test will pass if the expected exception 
                                                                ///of type ArgumentOutOfRangeException is thrown, and it will fail if 
                                                                ///any other exception or no exception is thrown.
        public void AdaptSegments_UnitTest_VerifyAdaptationWhenMaxSynapsesPerSegmentIsReachedAndExceeded()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd1 = conn.CreateDistalSegment(conn.GetCell(1));
            int numSynapses = 0;/// Create maximum synapses per segment (225 synapses)

            Random random = new Random();
            int totalCells = conn.Cells.Length;// Get total number of cells in cn1

            // Generate a random integer between 1 and totalCells
            while (numSynapses < conn.HtmConfig.MaxSynapsesPerSegment)
            {
                int randomCellNumber = random.Next(1, totalCells + 1);
                Synapse s = conn.CreateSynapse(dd1, conn.GetCell(randomCellNumber), 0.5);
                numSynapses++;

                if (numSynapses == conn.HtmConfig.MaxSynapsesPerSegment)
                {
                    TemporalMemory.AdaptSegment(conn, dd1, conn.GetCells(new int[] { randomCellNumber }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
                }
            }

            if (numSynapses >= conn.HtmConfig.MaxSynapsesPerSegment)
            {
                throw new ArgumentOutOfRangeException("The Maximum Synapse per segment was exceeded.");
            }
        }

        /// <summary>
        /// Verifies the behavior of segment destruction when no synapse is present on the segment.
        /// It initializes a temporal memory environment with default parameters and creates five distal dendrite segments, each with a corresponding synapse.
        /// Segments are adapted using the AdaptSegment method, and then the DestroyDistalDendrite method is explicitly called to destroy three of the segments.
        /// The test asserts the segment and synapse status before and after the destruction to ensure proper segment destruction without affecting other segments.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_VerifySegmentDestructionWhenNoSynapseIsPresent()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite[] segments = new DistalDendrite[5];
            Synapse[] synapses = new Synapse[5];

            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = conn.CreateDistalSegment(conn.GetCell(0));
                synapses[i] = conn.CreateSynapse(segments[i], conn.GetCell(23 + i), -1.5 + i * 0.4); // Adjusting the permanence values
            }

            // Adapt segments
            for (int i = 0; i < 3; i++)
            {
                TemporalMemory.AdaptSegment(conn, segments[i], conn.GetCells(new int[] { 21 + i, 22 + i, 23 + i }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            }

            var field1 = conn.GetType().GetField("m_ActiveSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field2 = conn.GetType().GetField("m_MatchingSegments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field4 = conn.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field5 = conn.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field9 = conn.GetType().GetField("nextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var field6 = conn.GetType().GetField("m_SegmentForFlatIdx", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo destroyDistalDendriteMethod = typeof(Connections).GetMethod("DestroyDistalDendrite", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var m_ASegment = field1.GetValue(conn);
            var m_MSegment = field2.GetValue(conn);

            ;
            ///Assert the segment and synapse status before the DestroyDistalDendrite method is explicitly called.
            Assert.AreEqual(5, Convert.ToInt32(field4.GetValue(conn)));
            Assert.AreEqual(2, Convert.ToInt32(field5.GetValue(conn)));

            // Destroy distal dendrite segments

            for (int i = 0; i < 3; i++)
            {
                destroyDistalDendriteMethod.Invoke(conn, new object[] { segments[i] });
            }

            ///Now checking the segment and synapse status after the DestroyDistalDendrite method is explicitly called.
            Assert.AreEqual(5, Convert.ToInt32(field4.GetValue(conn)));
            Assert.AreEqual(2, Convert.ToInt32(field5.GetValue(conn)));

        }
        /// <summary>
        /// Verifies that synapses with small negative permanence values are preserved 
        /// when the AdaptSegment method is invoked. It initializes a temporal memory environment with default parameters 
        /// and creates a distal dendrite segment. Three synapses are created on the segment with small negative permanence values. 
        /// The AdaptSegment method is then called with cells 102, 401, and 300. The test asserts that the synapses are not destroyed 
        /// after the adaptation process, ensuring that the preservation of synapses with small negative permanence values is maintained.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_PreservesSynapses_ForSmallNegativePermanenceValues()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small negative permanence values
            Synapse[] synapses = new Synapse[3];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(102), -0.0000003);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(401), -0.0000002);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(300), -0.0000004);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401, 300 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert that synapses are not destroyed
            foreach (Synapse synapse in synapses)
            {
                Assert.IsTrue(dd.Synapses.Contains(synapse));
            }
            Assert.AreEqual(3, dd.Synapses.Count);
        }
        [TestMethod]
        public void AdaptSegments_UnitTest_VerifySynapseDestructionWithNegativePermanenceValuesAfterAdaptation()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            // Create a distal dendrite segment
            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(55));

            // Create synapses with negative permanence values
            Synapse[] synapses = new Synapse[4];
            double[] permanenceValues = { -0.286, -0.355, -0.788, -0.817 }; // New permanence values
            for (int i = 0; i < synapses.Length; i++)
            {
                synapses[i] = conn.CreateSynapse(dd, conn.GetCell(52 + i), permanenceValues[i]);
            }

            // Invoke AdaptSegment with cells 23 and 24
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 52, 53, 54, 55 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert that synapses are destroyed and count is 0
            foreach (Synapse synapse in synapses)
            {
                Assert.IsFalse(dd.Synapses.Contains(synapse));
            }
            Assert.AreEqual(0, dd.Synapses.Count);
        }

        /// <summary>
        /// Verifies that synapses with negative permanence values are destroyed 
        /// after the AdaptSegment method is invoked. It initializes a temporal memory environment with default parameters 
        /// and creates a distal dendrite segment. Four synapses are created on the segment with negative permanence values. 
        /// The AdaptSegment method is then called with cells 52, 53, 54, and 55. The test asserts that the synapses are destroyed 
        /// after the adaptation process, ensuring that synapses with negative permanence values are removed as expected.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_EnsureAdaptSegmentThrowsExceptionWhenDistalDendriteIsNull()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(54));
            Synapse s1 = conn.CreateSynapse(dd, conn.GetCell(17), 0.66);

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                TemporalMemory.AdaptSegment(conn, null, conn.GetCells(new int[] { 17 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            }, "Expected NullReferenceException was not thrown"); // Because DD cannot be Null

        }
        /// <summary>
        /// This unit test method checks the state of synapses on a distal dendrite segment after invoking the AdaptSegment method. 
        /// It initializes a temporal memory environment with default parameters and creates a distal dendrite segment. 
        /// Several synapses are created on the segment with various permanence values. The AdaptSegment method is then called 
        /// with cells corresponding to the synapses. After adaptation, the test verifies the state of each synapse, 
        /// ensuring that synapses with positive permanence values remain intact, while synapses with negative permanence values 
        /// are removed from the segment.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_CheckSynapseStateAfterAdaptatione()
        {
            // Arrange
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);


            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(0));
            Synapse[] synapses = new Synapse[]
            {
            conn.CreateSynapse(dd, conn.GetCell(82), 0.3),       // Updated value for s1
            conn.CreateSynapse(dd, conn.GetCell(85), 0.015),     // Updated value for s2
            conn.CreateSynapse(dd, conn.GetCell(89), 0.77),      // Updated value for s3
            conn.CreateSynapse(dd, conn.GetCell(92), 0.06),      // Updated value for s4
            conn.CreateSynapse(dd, conn.GetCell(93), 0.002),     // Updated value for s5
            conn.CreateSynapse(dd, conn.GetCell(95), 0.003),     // Updated value for s6
            conn.CreateSynapse(dd, conn.GetCell(97), -0.23),     // Updated value for s7
            conn.CreateSynapse(dd, conn.GetCell(99), -0.13)      // Updated value for s8
            };

            // Adapt segment with new values
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 82, 85, 89, 92, 93, 95, 97, 99 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert
            foreach (Synapse synapse in synapses)
            {
                if (synapse.Permanence >= 0)
                    Assert.IsTrue(dd.Synapses.Contains(synapse));
                else
                    Assert.IsFalse(dd.Synapses.Contains(synapse));
            }
            Assert.AreEqual(6, dd.Synapses.Count);
        }
        /// <summary>
        /// This unit test method verifies the boundary constraint for permanence increment by attempting to increase 
        /// the permanence value beyond the maximum allowed bound. It initializes a temporal memory environment with 
        /// default parameters and creates a distal dendrite segment with a synapse having a permanence value that 
        /// exceeds the maximum bound. The method then invokes the AdaptSegment method twice with the same active cell 
        /// to simulate two consecutive iterations. After the adaptations, the test verifies that the synapse permanence 
        /// remains capped at the maximum bound defined by the system configuration.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_TestPermanenceIncrement_BoundaryConstraint()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite segment = conn.CreateDistalSegment(conn.GetCell(44));
            Synapse synapse = conn.CreateSynapse(segment, conn.GetCell(66), 1.1);

            // Act
            TemporalMemory.AdaptSegment(conn, segment, conn.GetCells(new int[] { 66 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            TemporalMemory.AdaptSegment(conn, segment, conn.GetCells(new int[] { 66 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert
            Assert.AreEqual(1.0, synapse.Permanence, 0.1, "Permanence should be capped at the maximum bound");
        }
        /// <summary>
        /// This unit test method verifies the behavior of the GetCells method when provided with an empty array 
        /// of cell indexes. It initializes a temporal memory environment with default parameters and attempts 
        /// to retrieve cells using an empty array of cell indexes. The method then asserts that the returned 
        /// array of cells is also empty, as no cells are expected to be retrieved in this scenario.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_TestGetCells_ReturnsEmptyArrayForEmptyInput()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            int[] cellIndexes = Array.Empty<int>();
            Cell[] expectedCells = Array.Empty<Cell>();

            // Act
            Cell[] result = conn.GetCells(cellIndexes);

            // Assert
            CollectionAssert.AreEqual(expectedCells, result);
        }
        /// <summary>
        /// This unit test method verifies the behavior of the GetCells method when provided with a valid input array 
        /// of cell indexes. It initializes a connections object with a specified array of cells and attempts to 
        /// retrieve cells using a given array of cell indexes. The method then asserts that the returned array of 
        /// cells matches the expected array of cells based on the provided cell indexes.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_TestGetCells_ValidInput_ReturnsExpectedCellArray()
        {
            Connections conn = new Connections();
            int[] cellIndexes = { 13, 12, 24 };
            conn.Cells = new Cell[50];
            Cell[] expectedCells = { conn.Cells[13], conn.Cells[12], conn.Cells[24] };

            // Act
            Cell[] result = conn.GetCells(cellIndexes);

            // Assert
            CollectionAssert.AreEqual(expectedCells, result);
        }
        /// <summary>
        /// This unit test method verifies the behavior of the AdaptSegment method when provided with a complex 
        /// double permanence input that reaches the maximum permanence value. It initializes a temporal memory 
        /// and connections object with default parameters, creates a distal dendrite segment, and creates a synapse 
        /// with a specific initial permanence value. The method then adapts the segment with a single active cell, 
        /// incrementing the permanence using the HTM configuration parameters. After the first adaptation, it asserts 
        /// that the permanence has been incremented accordingly. Next, it attempts to adapt the segment again with 
        /// the same active cell, and asserts that the permanence is capped at the maximum bound defined by the HTM 
        /// configuration.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_ComplexDoublePermanenceInput_MaxPermanenceReached()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(8));
            Synapse s1 = conn.CreateSynapse(dd, conn.GetCell(29), 0.865467362567887);

            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 29 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(0.965467362567887, s1.Permanence);
            // Now permanence should be at max
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 29 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(1.0, s1.Permanence, 0.1);
        }
        /// <summary>
        /// This unit test method verifies the behavior of the AdaptSegment method when adapting a segment with 
        /// a synapse whose permanence falls below the minimum threshold. It initializes a temporal memory and 
        /// connections object with default parameters, creates a distal dendrite segment, and creates a synapse 
        /// with a specific initial permanence value. The method then adapts the segment without any active cells, 
        /// decrementing the permanence using the HTM configuration parameters. After the adaptation, it asserts 
        /// that the synapse has been removed from the segment since its permanence falls below the minimum threshold 
        /// defined by the HTM configuration.
        /// </summary>
        [TestMethod]
        public void AdaptSegments_UnitTest_VerifySynapseRemovalOnMinimumPermanenceAdaptation()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(4));
            Synapse synapse = conn.CreateSynapse(dd, conn.GetCell(32), 0.1);/// create a synapse on a dital segment of a cell with index 23

            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);/// Invoking AdaptSegments with the cell 15 whose presynaptic cell is 
                                                                                                                                                        /// considered to be InActive in the previous cycle.
            //Assert.IsFalse(cn.GetSynapses(dd).Contains(s1));
            Assert.IsFalse(dd.Synapses.Contains(synapse));/// permanence is decremented for presynaptie cell 477 from 
                                                          /// 0.1 to 0 as presynaptic cell was InActive in the previous cycle
                                                          /// There the synapse is destroyed as permanence < HtmConfig.Epsilon
        }
        /// <summary>
        /// This unit test method verifies the behavior of the AdaptSegment method when adapting a segment with 
        /// a synapse whose permanence falls below a certain threshold. It initializes a temporal memory and 
        /// connections object with default parameters, creates a distal dendrite segment, and creates a synapse 
        /// with a specific initial negative permanence value. The method then adapts the segment with an active 
        /// cell, which decrements the synapse's permanence using the HTM configuration parameters. After the 
        /// adaptation, it asserts that the synapse has been removed from the segment since its permanence falls 
        /// below the threshold defined by the HTM configuration.
        /// </summary>
        [TestMethod]
        public void AdaptSegments_UnitTest_VerifySynapseDestructionOnLowPermanenceAdaptation()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite distalDendrite = conn.CreateDistalSegment(conn.GetCell(99));
            Synapse synapse = conn.CreateSynapse(distalDendrite, conn.GetCell(49), -2.35);


            TemporalMemory.AdaptSegment(conn, distalDendrite, conn.GetCells(new int[] { 49 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            Assert.IsFalse(distalDendrite.Synapses.Contains(synapse),
            "The synapse was not destroyed as expected.");
        }
        /// <summary>
        /// This unit test method verifies that a synapse remains in the segment after adaptation if its permanence 
        /// value remains within the valid range. It initializes a temporal memory and connections object with 
        /// default parameters, creates a distal dendrite segment, and creates two synapses with different initial 
        /// permanence values. The method then adapts the segment without any active cells, which might result in 
        /// adjustments to the synapses' permanence values. After the adaptation, it asserts that the synapse 
        /// with a permanence value within the valid range remains in the segment, while the synapse with an 
        /// out-of-range permanence value is removed.
        /// </summary>
        [TestMethod]
        public void AdaptSegments_UnitTest_VerifyStayOfSynapseAfterSegmentAdaptation()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite distalDendrite = conn.CreateDistalSegment(conn.GetCell(0));
            Synapse synapse1 = conn.CreateSynapse(distalDendrite, conn.GetCell(65), -0.1);
            Synapse synapse2 = conn.CreateSynapse(distalDendrite, conn.GetCell(69), 0.9);

            //Act
            TemporalMemory.AdaptSegment(conn, distalDendrite, conn.GetCells(new int[] { }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            //Assert
            Assert.IsFalse(distalDendrite.Synapses.Contains(synapse1), "The synapse created earlier is no longer present in the segment.");
            Assert.IsTrue(distalDendrite.Synapses.Contains(synapse2), "The synapse created earlier is still present in the segment.");

        }
        /// <summary>
        /// This unit test method verifies that attempting to retrieve cells from an invalid array of cell indexes
        /// throws an IndexOutOfRangeException as expected. It initializes a connections object with an array of 
        /// cells of size 3. The method then tries to retrieve cells using an array of cell indexes containing 
        /// invalid indices. It expects an IndexOutOfRangeException to be thrown because the provided cell 
        /// indexes are out of the valid range for the array of cells.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(IndexOutOfRangeException))]///This attribute is used to specify the expected 
                                                             ///exception. Therefore, the test will pass if the expected exception 
                                                             ///of type IndexOutOfRangeException is thrown, and it will fail if 
                                                             ///any other exception or no exception is thrown.
        public void AdaptSegments_UnitTest_TestInvalidArrayCells_WithInvalidArray_ThrowsIndexOutOfRangeException()
        {
            Connections cn = new Connections();
            cn.Cells = new Cell[3];
            int[] cellIndexes = new int[] { 2, 4, 7 };
            Cell[] expectedCells = new Cell[] { cn.Cells[2], cn.Cells[4], cn.Cells[7] };

            // Act & Assert
            Cell[] result = cn.GetCells(cellIndexes);

        }
        /// <summary>
        /// This unit test method verifies that attempting to retrieve cells with a null array of cell indexes
        /// throws a NullReferenceException as expected. It initializes a connections object with a null array 
        /// of cells and a null array of cell indexes. The method then tries to retrieve cells using the null 
        /// array of cell indexes. It expects a NullReferenceException to be thrown because the cells array is null.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        [ExpectedException(typeof(NullReferenceException))]///This attribute is used to specify the expected 
                                                           ///exception. Therefore, the test will pass if the expected exception 
                                                           ///of type ArgumentNullException is thrown, and it will fail if 
                                                           ///any other exception or no exception is thrown.
        public void AdaptSegments_UnitTest_TestNullArrayCells_ThrowsException()
        {
            // Arrange
            Connections cn = new Connections();
            cn.Cells = null;
            int[] indices_of_cell = null;

            // Act & Assert
            Cell[] output = cn.GetCells(indices_of_cell);

        }
        /// <summary>
        /// This unit test verifies that synapses are preserved when the permanence values are very small.
        /// It initializes a temporal memory object, a connections object, and applies default parameters.
        /// Then, it creates a distal dendrite segment and three synapses with very small permanence values.
        /// After that, it invokes the AdaptSegment method with the three synapses' presynaptic cells.
        /// Finally, it asserts that all synapses are preserved after adaptation.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_PreservesSynapses_ForVerySmallPermanenceValues()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[3];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(102), 0.0000003);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(401), 0.0000002);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(300), 0.0000004);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401, 300 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert that synapses are not destroyed
            foreach (Synapse synapse in synapses)
            {
                Assert.IsTrue(dd.Synapses.Contains(synapse));
            }
            Assert.AreEqual(3, dd.Synapses.Count);
        }
        /// <summary>
        /// This unit test verifies that synapses are preserved when the permanence values are very large.
        /// It initializes a temporal memory object, a connections object, and applies default parameters.
        /// Then, it creates a distal dendrite segment and three synapses with very large permanence values.
        /// After that, it invokes the AdaptSegment method with the three synapses' presynaptic cells.
        /// Finally, it asserts that all synapses are preserved after adaptation.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_PreservesSynapses_ForVeryLargePermanenceValues()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[3];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(102), 16799999);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(401), 762638282);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(300), 817637383);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401, 300 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert that synapses are not destroyed
            foreach (Synapse synapse in synapses)
            {
                Assert.IsTrue(dd.Synapses.Contains(synapse));
            }
            Assert.AreEqual(3, dd.Synapses.Count);

        }
        /// <summary>
        /// This unit test verifies that the AdaptSegment method adjusts synapse permanence based on the previous active cells.
        /// It initializes a temporal memory object, a connections object, and applies default parameters.
        /// Then, it creates a distal dendrite segment and three synapses with initial permanence values.
        /// After that, it invokes the AdaptSegment method with the cells representing previous active states.
        /// Finally, it asserts that the synapse permanence values are adjusted as expected.
        /// </summary>
        [TestMethod]
        public void AdjustsSynapsePermanenceBasedOnPreviousActiveCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite distalDendrite = conn.CreateDistalSegment(conn.GetCell(0));
            Synapse s1 = conn.CreateSynapse(distalDendrite, conn.GetCell(42), 0.5);
            Synapse s2 = conn.CreateSynapse(distalDendrite, conn.GetCell(45), 0.3);
            Synapse s3 = conn.CreateSynapse(distalDendrite, conn.GetCell(44), 0.7);

            TemporalMemory.AdaptSegment(conn, distalDendrite, conn.GetCells(new int[] { 42, 45 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            Assert.AreEqual(0.6, s1.Permanence, 0.1);
            Assert.AreEqual(0.4, s2.Permanence, 0.1);
            Assert.AreEqual(0.6, s3.Permanence, 0.1);

        }
        /// <summary>
        /// It initializes a temporal memory instance, sets up connections, and creates a distal dendrite segment (`dd`).
        /// Three synapses are created with very large negative permanence values.
        /// The `AdaptSegment` method is invoked with two active cells.
        /// It asserts that none of the synapses are preserved in the distal dendrite segment, and the count of synapses is zero.
        /// </summary>
        [TestMethod]
        public void PreservesSynapses_ForVeryLargeNegativePermanenceValues()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[3];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(102), -16799999);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(401), -762638282);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(300), -817637383);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            // Assert that synapses are not destroyed
            foreach (Synapse synapse in synapses)
            {
                Assert.IsFalse(dd.Synapses.Contains(synapse));
            }
            Assert.AreEqual(0, dd.Synapses.Count);

        }
        /// <summary>
        /// Unit test to verify that synapses with zero permanence values are preserved after adaptation.
        /// </summary>
        [TestMethod]
        public void PreservesSynapses_ForZeroPermanenceValues()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[3];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(102), 0);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(401), 0);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);

            Assert.AreEqual(2, dd.Synapses.Count);

        }
        /// <summary>
        /// Unit test to verify that an empty segment has no synapses after adaptation.
        /// </summary>
        [TestMethod]
        public void Verify_Emptysegement()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[3];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(102), -167737383);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(401), -762638282);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(300), -817637383);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(0, dd.Synapses.Count);

            var field1 = conn.GetType().GetField("m_NumSynapses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo destroyDistalDendriteMethod = typeof(Connections).GetMethod("DestroyDistalDendrite", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            // Assert that synapses are not destroyed
            destroyDistalDendriteMethod.Invoke(conn, new object[] { dd });

            Assert.AreEqual(0, Convert.ToInt32(field1.GetValue(conn)));

        }
        /// <summary>
        /// Unit test to ensure that a segment is destroyed even if only one synapse is left after adaptation.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_KillSegmentEvenIfOnlyoneSynapse_is_left()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(55));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[5];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(15), -167737383);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(16), -762638282);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(17), -817637383);
            synapses[3] = conn.CreateSynapse(dd, conn.GetCell(18), -817637383);
            synapses[4] = conn.CreateSynapse(dd, conn.GetCell(19), 0.5);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 102, 401 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(1, dd.Synapses.Count);

            var field1 = conn.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var segmentCount = Convert.ToInt32(field1.GetValue(conn));
            MethodInfo destroyDistalDendriteMethod = typeof(Connections).GetMethod("DestroyDistalDendrite", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            // Assert that synapses are not destroyed
            destroyDistalDendriteMethod.Invoke(conn, new object[] { dd });

            Assert.AreEqual(1, segmentCount);

        }
        /// <summary>
        /// Unit test to check if a segment survives after adaptation.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void AdaptSegments_UnitTest_CheckIfSegmentSurvives()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections conn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(conn);
            tm.Init(conn);

            DistalDendrite dd = conn.CreateDistalSegment(conn.GetCell(5));

            // Create synapses with small permanence values
            Synapse[] synapses = new Synapse[5];
            synapses[0] = conn.CreateSynapse(dd, conn.GetCell(15), 16);
            synapses[1] = conn.CreateSynapse(dd, conn.GetCell(16), 76);
            synapses[2] = conn.CreateSynapse(dd, conn.GetCell(17), -8);
            synapses[3] = conn.CreateSynapse(dd, conn.GetCell(18), 8);
            synapses[4] = conn.CreateSynapse(dd, conn.GetCell(19), 0.5);

            // Invoke AdaptSegment with cells 23, 24, and 25
            TemporalMemory.AdaptSegment(conn, dd, conn.GetCells(new int[] { 15, 16, 17, 18, 19 }), conn.HtmConfig.PermanenceIncrement, conn.HtmConfig.PermanenceDecrement);
            Assert.AreEqual(4, dd.Synapses.Count);

            var field1 = conn.GetType().GetField("m_NextSegmentOrdinal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var segmentCount = Convert.ToInt32(field1.GetValue(conn));
            MethodInfo destroyDistalDendriteMethod = typeof(Connections).GetMethod("DestroyDistalDendrite", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.AreEqual(1, segmentCount);

        }
    }
}