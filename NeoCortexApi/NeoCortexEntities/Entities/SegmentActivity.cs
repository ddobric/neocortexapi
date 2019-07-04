using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Used as tupple in temporal cycle calculus.
    /// </summary>
    //[Serializable]
    public class SegmentActivity
    {
        /// <summary>
        /// Contains the index of segments with number of synapses with permanence higher than threshold,
        /// which makes synapse active.
        /// [segment index, number of active synapses].
        /// </summary>
        public Dictionary<int, int> Active = new Dictionary<int, int>();

        /// <summary>
        /// Contains the index of segments with number of synapses with permanence higher than minimum threshold,
        /// which makes synapse potential one.
        /// Dictionary [segment index, number of potential synapses].
        /// </summary>
        public Dictionary<int, int> Potential = new Dictionary<int, int>();

        public SegmentActivity()
        {

        }
    }
}
