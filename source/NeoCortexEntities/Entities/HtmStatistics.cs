using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Holds statistics for the current state of HTM (<see cref="Connections"/>
    /// </summary>
    public class HtmStatistics
    {
        /// <summary>
        /// Ration connected synapses / synapses
        /// </summary>
        public double SynapticActivity{ get; set; }

        public int Synapses { get; set; }

        public int ConnectedSynapses { get; set; }

        public double MinPermanence { get; set; }

        public double MaxPermanence { get; set; }

        public double AvgPermanence { get; set; }
    }
}
