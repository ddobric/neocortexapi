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


        //public int[] ColumnDimensions { get; set; }

        //public bool IsColumnMajor { get; set; } = false;

        /// <summary>
        /// Use -1 if real random generator has to be used with timestamp seed.
        /// </summary>
        public int RandomGenSeed { get; set; } = 42;

        public HtmModuleTopology ColumnTopology { get; set; }

        public HtmModuleTopology InputTopology { get; set; }

        public bool WrapAround { get; set; } = true;

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }

        public double PotentialPct { get; set; }

        public int PotentialRadius { get; set; }

        public double SynPermConnected { get; set; }

        public double StimulusThreshold { get; set; }

        public int NumInputs { get; set; }

        public int NumColumns { get; set; }

        public double SynPermMax { get; set; } = 1.0;

        public double SynPermMin { get; set; }

        public double InitialSynapseConnsPct { get; set; } = 0.5;

        public double SynPermTrimThreshold { get; set; }

        public double SynPermBelowStimulusInc { get; set; }

        public int CellsPerColumn { get; set; } = 32;

        public double SynPermInactiveDec { get; set; }
        public double SynPermActiveInc { get; set; }

        public double PermanenceIncrement { get; set; }

        public double PermanenceDecrement { get; set; }
        public int MaxNewSynapseCount { get; internal set; }

        public int MaxSegmentsPerCell { get; set; }

        public int MaxSynapsesPerSegment { get; set; }

        public int[] InputDimensions { get; set; } = new int[] { 100 };
        public int[] ColumnDimensions { get; set; } = new int[] { 2048 };

        public AbstractSparseMatrix<Column> Memory { get; set; }
        public ISparseMatrix<int> InputMatrix { get; set; }
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
