// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    //[Serializable]

    /// <summary>
    /// Implements a distal dendritic segment.
    /// Segments are owned by <see cref="Cell"/>s and in turn own <see cref="Synapse"/>s which are obversely connected to by a "source cell", 
    /// which is the <see cref="Cell"/> which will activate a given <see cref="Synapse"/> owned by this <see cref="Segment"/>.
    /// </summary>
    /// <remarks>
    /// Authors of the JAVA implementation: Chetan Surpur, David Ray
    /// </remarks>
    public class DistalDendrite : Segment, IComparable<DistalDendrite>
    {
        public Cell ParentCell;

        private long m_LastUsedIteration;

        public int m_Ordinal = -1;

        /// <summary>
        /// Creates the Distal Segment.
        /// </summary>
        /// <param name="parentCell">The cell, which owns the segment.</param>
        /// <param name="flatIdx">The flat index of the cell.</param>
        /// <param name="lastUsedIteration"></param>
        /// <param name="ordinal"></param>
        /// <param name="synapsePermConnected"></param>
        /// <param name="numInputs"></param>
        public DistalDendrite(Cell parentCell, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs) : base(flatIdx, synapsePermConnected, numInputs)
        {
            this.ParentCell = parentCell;
            this.m_Ordinal = ordinal;
            this.m_LastUsedIteration = lastUsedIteration;
        }

        /**
         * Returns the owner {@link Cell}
         * 
         * @return
         */
        //public Cell GetParentCell()
        //{
        //    return ParentCell;
        //}


        /// <summary>
        /// Gets all synapses owned by this distal dentrite segment.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Synapses.</returns>
        public List<Synapse> GetAllSynapses(Connections c)
        {
            return c.GetSynapses(this);
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
        /// the last iteration in which this segment was active.
        /// </summary>
        public long LastUsedIteration { get => m_LastUsedIteration; set => m_LastUsedIteration = value; }
        ///**
        // * Sets the last iteration in which this segment was active.
        // * @param iteration
        // */
        //public void setLastUsedIteration(long iteration)
        //{
        //    this.m_LastUsedIteration = iteration;
        //}

        ///**
        // * Returns the iteration in which this segment was last active.
        // * @return  the iteration in which this segment was last active.
        // */
        //public long getLastUsedIteration()
        //{
        //    return m_LastUsedIteration;
        //}

        public int Ordinal { get => m_Ordinal; set => m_Ordinal = value; }
        ///**
        // * Returns this {@code DistalDendrite} segment's ordinal
        // * @return	this segment's ordinal
        // */
        //public int getOrdinal()
        //{
        //    return m_Ordinal;
        //}

        ///**
        // * Sets the ordinal value (used for age determination) on this segment.
        // * @param ordinal	the age or order of this segment
        // */
        //public void setOrdinal(int ordinal)
        //{
        //    this.m_Ordinal = ordinal;
        //}


        public override string ToString()
        {
            return $"DistalDendrite: Indx:{this.SegmentIndex}";
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */

        public override int GetHashCode()
        {
            int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + ((ParentCell == null) ? 0 : ParentCell.GetHashCode());
            return result;
        }


        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */

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

        ///** Sorting Lambda used for sorting active and matching segments */
        //public IComparer<DistalDendrite> segmentPositionSortKey = (s1, s2) =>
        //        {
        //            double c1 = s1.getParentCell().getIndex() + ((double)(s1.getOrdinal() / (double)nextSegmentOrdinal));
        //            double c2 = s2.getParentCell().getIndex() + ((double)(s2.getOrdinal() / (double)nextSegmentOrdinal));
        //            return c1 == c2 ? 0 : c1 > c2 ? 1 : -1;
        //        };
    }
}

