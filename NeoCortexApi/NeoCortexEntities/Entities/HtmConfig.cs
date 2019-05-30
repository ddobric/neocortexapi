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
        public int[] ColumnDimensions { get; set; }

        public bool IsColumnMajor { get; set; } = false;

        public int[] InputDimensions { get; set; }

        public HtmModuleTopology ColumnTopology { get; set; }

        public HtmModuleTopology InputTopology { get; set; }


        public bool IsWrapAround { get; set; }

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }

        public double PotentialPct { get; set; }

        public int PotentialRadius { get; set; }
    }
}
