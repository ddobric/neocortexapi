// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// HTM required configuration sent from Akka-client to Akka Actor.
    /// </summary>
    public class HtmConfig
    {
        public HtmConfig()
        {

        }
        public class TemporalMemoryConfig
        {

        }

        public class SpatialPoolerConfig
        {

        }

        public TemporalMemoryConfig TemporalMemory { get; set; } = new TemporalMemoryConfig();

        public SpatialPoolerConfig SpatialPooler { get; set; } = new SpatialPoolerConfig();

        #region Spatial Pooler Variables
        public int NumInputs { get; set; }
        public int PotentialRadius { get; set; }
        public double PotentialPct { get; set; }
        public double StimulusThreshold { get; set; }
        public double SynPermBelowStimulusInc { get; set; }
        public double SynPermInactiveDec { get; set; }
        public double SynPermActiveInc { get; set; }
        public double SynPermConnected { get; set; }
        public bool WrapAround { get; set; } = true;
        public bool GlobalInhibition { get; set; } = false;
        public double LocalAreaDensity { get; set; } = -1.0;

        public double SynPermTrimThreshold { get; set; }
        public double SynPermMax { get; set; } = 1.0;
        public double SynPermMin { get; set; }

        public double InitialSynapseConnsPct { get; set; } = 0.5;
        public ISparseMatrix<int> InputMatrix { get; set; }
        public double NumActiveColumnsPerInhArea { get; set; }
        public double MinPctOverlapDutyCycles { get; set; } = 0.001;
        public double MinPctActiveDutyCycles { get; set; } = 0.001;
        public double PredictedSegmentDecrement { get; set; } = 0.0;
        public int DutyCyclePeriod { get; set; } = 1000;
        public double MaxBoost { get; set; } = 10.0;
        public bool IsBumpUpWeakColumnsDisabled { get; set; } = false;
        public int UpdatePeriod { get; set; } = 50;
        #endregion

        #region Temporal Memory Variables
        public int[] ColumnDimensions { get; set; } = new int[] { 2048 };
        public int CellsPerColumn { get; set; } = 32;
        public int[] InputDimensions { get; set; } = new int[] { 100 };
        public int MaxNewSynapseCount { get; internal set; }
        public int MaxSegmentsPerCell { get; set; }

        public int MaxSynapsesPerSegment { get; set; }
        public double PermanenceIncrement { get; set; }

        public double PermanenceDecrement { get; set; }
        public HtmModuleTopology ColumnTopology { get; set; }

        public HtmModuleTopology InputTopology { get; set; }
        public AbstractSparseMatrix<Column> Memory { get; set; }

        public int ActivationThreshold { get; set; } = 13;
        public int LearningRadius { get; set; } = 2048;
        public int MinThreshold { get; set; } = 10;
        public double InitialPermanence { get; set; } = 0.21;
        public double ConnectedPermanence { get; set; } = 0.5;
        #endregion

        //public bool IsColumnMajor { get; set; } = false;

        /// <summary>
        /// Use -1 if real random generator has to be used with timestamp seed.
        /// </summary>
        public int RandomGenSeed { get; set; } = 42;

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }

        public int NumColumns { get; set; }
    }

    public class test
    {
        public test()
        {
            HtmConfig htm = new HtmConfig();

            Connections c = new Connections(htm);

            HtmConfig.TemporalMemoryConfig x = new HtmConfig.TemporalMemoryConfig();

        }
    }
}
