using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// HTM required configuration sent from Akka-client to Akka Actor.
    /// </summary>
    public class HtmConfig
    {
        //public int[] ColumnDimensions { get; set; }

        //public bool IsColumnMajor { get; set; } = false;

        /// <summary>
        /// Use -1 if real random generator has to be used with timestamp seed.
        /// </summary>
        public int RandomGenSeed { get; set; } = 42;

        public HtmModuleTopology ColumnTopology { get; set; }

        public HtmModuleTopology InputTopology { get; set; }
        
        public bool IsWrapAround { get; set; }

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }

        public double PotentialPct { get; set; }

        public int PotentialRadius { get; set; }

        public double SynPermConnected { get; set; }

        public double StimulusThreshold { get; set; }        

        public int NumInputs { get;  set; }

        public int NumColumns { get; set; }

        public double SynPermMax { get; set; }

        public double SynPermMin { get; set; }

        public double InitialSynapseConnsPct { get; set; }

        public double SynPermTrimThreshold { get; set; }

        public double SynPermBelowStimulusInc { get; set; }

        public int CellsPerColumn { get; set; }

        public double SynPermInactiveDec { get; set; }

        public double PermanenceIncrement { get;  set; }

        public double PermanenceDecrement { get;  set; }
        public int MaxNewSynapseCount { get; internal set; }
    }
}
