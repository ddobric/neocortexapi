using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Calculus of a temporal cycle.
    /// </summary>
    //[Serializable]
    public class SegmentActivity
    {
        /// <summary>
        /// Contains the index of segments with number of synapses with permanence higher than threshold,
        /// which makes synapse active.
        /// [segment index, number of active synapses].
        /// </summary>
        public Dictionary<int, int> ActiveSynapses = new Dictionary<int, int>();

        /// <summary>
        /// Dictionary, which holds the number of potential synapses fro every segment.
        /// Potential synspses are synapses with permanence higher than minimum threshold, which makes synapse potential one.
        /// Dictionary [segment index, number of potential synapses].
        /// </summary>
        public Dictionary<int, int> PotentialSynapses = new Dictionary<int, int>();

        public SegmentActivity()
        {

        }
    }
}
