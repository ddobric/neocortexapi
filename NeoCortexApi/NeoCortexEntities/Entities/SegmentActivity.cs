// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Stores the calculus of a temporal cycle.
    /// </summary>
    //[Serializable]
    public class SegmentActivity
    {
        /// <summary>
        /// Contains the index of segments with number of synapses with permanence higher than threshold ( <see cref="connectedPermanence"/>connectedPermanence),
        /// which makes synapse active.
        /// Dictionary[segment index, number of active synapses].
        /// </summary>
        public Dictionary<int, int> ActiveSynapses = new Dictionary<int, int>();

        /// <summary>
        /// Dictionary, which holds the number of potential synapses of every segment.
        /// Potential synspses are all established synapses between receptor cell and the segment's cell. Receprot cell was active cell in the previous cycle.
        /// Dictionary [segment index, number of potential synapses].
        /// </summary>
        public Dictionary<int, int> PotentialSynapses = new Dictionary<int, int>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SegmentActivity()
        {

        }
    }
}
