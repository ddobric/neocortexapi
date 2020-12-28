// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Implements a distal dendritic segment that is used for learning sequences.
    /// Segments are owned by <see cref="Cell"/>s and in turn own <see cref="Synapse"/>s which are obversely connected to by a "source cell", 
    /// which is the <see cref="Cell"/> which will activate a given <see cref="Synapse"/> owned by this <see cref="Segment"/>.
    /// </summary>
    /// <remarks>
    /// Authors of the JAVA implementation: Chetan Surpur, David Ray
    /// </remarks>
    public class DistalDendrite : Segment, IComparable<DistalDendrite>
    {
        /// <summary>
        /// The cell that owns (parent) the segment.
        /// </summary>
        public Cell ParentCell;

        private long m_LastUsedIteration;

        private int m_Ordinal = -1;

        /// <summary>
        /// the last iteration in which this segment was active.
        /// </summary>
        public long LastUsedIteration { get => m_LastUsedIteration; set => m_LastUsedIteration = value; }

        /// <summary>
        /// The seqence number of the segment. Specifies the order of the segment of the <see cref="Connections"/> instance.
        /// </summary>
        public int Ordinal { get => m_Ordinal; set => m_Ordinal = value; }

        /// <summary>
        /// Creates the Distal Segment.
        /// </summary>
        /// <param name="parentCell">The cell, which owns the segment.</param>
        /// <param name="flatIdx">The flat index of the segment. If some segments are destroyed (synapses lost permanence)
        /// then the new segment will reuse the flat index. In contrast, 
        /// the ordinal number will increas when new segments are created.</param>
        /// <param name="lastUsedIteration"></param>
        /// <param name="ordinal">The ordindal number of the segment. This number is incremented on each new segment.
        /// If some segments are destroyed, this number is still incrementd.</param>
        /// <param name="synapsePermConnected"></param>
        /// <param name="numInputs"></param>
        public DistalDendrite(Cell parentCell, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs) : base(flatIdx, synapsePermConnected, numInputs)
        {
            this.ParentCell = parentCell;
            this.m_Ordinal = ordinal;
            this.m_LastUsedIteration = lastUsedIteration;

            
        }


        /// <summary>
        /// Gets all synapses owned by this distal dentrite segment.
        /// </summary>
        /// <param name="mem"></param>
        /// <returns>Synapses.</returns>
        public List<Synapse> GetAllSynapses(Connections mem)
        {
            return mem.GetSynapses(this);
        }

        /// <summary>
        /// Gets all active synapses of this segment, which have presynaptic cell as active one.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="activeCells"></param>
        /// <returns></returns>
        public ISet<Synapse> GetActiveSynapses(Connections c, ISet<Cell> activeCells)
        {
            ISet<Synapse> activeSynapses = new LinkedHashSet<Synapse>();

            foreach (var synapse in c.GetSynapses(this))
            {
                if (activeCells.Contains(synapse.getPresynapticCell()))
                {
                    activeSynapses.Add(synapse);
                }
            }

            return activeSynapses;
        }

           
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"DistalDendrite: Indx:{this.SegmentIndex}";
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + ((ParentCell == null) ? 0 : ParentCell.GetHashCode());
            return result;
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool Equals(Segment obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            DistalDendrite other = (DistalDendrite)obj;
            if (ParentCell == null)
            {
                if (other.ParentCell != null)
                    return false;
            }
            else if (!ParentCell.Equals(other.ParentCell))
                return false;

            return true;
        }


        /// <summary>
        /// Compares by index.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(DistalDendrite other)
        {
            if (this.SegmentIndex > other.SegmentIndex)
                return 1;
            else if (this.SegmentIndex < other.SegmentIndex)
                return -1;
            else
                return 0;
        }
        #region Serialization
        public void serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(HtmConfig), writer);

            ser.SerializeValue(this.SegmentIndex, writer);

            ser.SerializeEnd(nameof(HtmConfig), writer);
        }
        #endregion
    }
}

