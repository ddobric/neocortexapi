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

        // override object.Equals
        public override bool Equals(object obj)
        {
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var htms = obj as HtmStatistics;

            return base.Equals(htms);
        }
               
        public bool Equals(HtmStatistics obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if(this.Synapses != obj.Synapses)            
                return false;            

            if (this.ConnectedSynapses != obj.ConnectedSynapses)
                return false;

            if (this.SynapticActivity != obj.SynapticActivity)
                return false;

            if (this.MinPermanence != obj.MinPermanence)
                return false;

            if (this.MaxPermanence != obj.MaxPermanence)
                return false;

            if (this.AvgPermanence != obj.AvgPermanence)
                return false;

            return true;
        }
    }
}
