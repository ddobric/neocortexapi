using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /**
     * Represents a proximal or distal dendritic segment. Segments are owned by
     * {@link Cell}s and in turn own {@link Synapse}s which are obversely connected
     * to by a "source cell", which is the {@link Cell} which will activate a given
     * {@link Synapse} owned by this {@code Segment}.
     * 
     * @author Chetan Surpur
     * @author David Ray
     */
    //[Serializable]
    public class DistalDendrite : Segment, IComparable<DistalDendrite>
    {
        /** keep it simple */
        private static readonly long serialVersionUID = 1L;

        public Cell ParentCell;

        private long m_LastUsedIteration;

        public int ordinal = -1;

        /**
         * Constructs a new {@code Segment} object with the specified owner
         * {@link Cell} and the specified index.
         * 
         * @param cell      the owner
         * @param flatIdx     this {@code Segment}'s index.
         */
        public DistalDendrite(Cell parentCell, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs) : base(flatIdx, synapsePermConnected, numInputs)
        {
            this.ParentCell = parentCell;
            this.ordinal = ordinal;
            this.m_LastUsedIteration = lastUsedIteration;
        }

        /**
         * Returns the owner {@link Cell}
         * 
         * @return
         */
        public Cell GetParentCell()
        {
            return ParentCell;
        }


        /// <summary>
        /// Gets all synapses owned by this distal dentrite segment.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Synapses.</returns>
        public List<Synapse> getAllSynapses(Connections c)
        {
            return c.getSynapses(this);
        }

        /// <summary>
        /// Gets all active synapses of this segment, which have presynaptic cell as active one.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="activeCells"></param>
        /// <returns></returns>
        public ISet<Synapse> getActiveSynapses(Connections c, ISet<Cell> activeCells)
        {
            ISet<Synapse> activeSynapses = new LinkedHashSet<Synapse>();

            foreach (var synapse in c.getSynapses(this))
            {
                if (activeCells.Contains(synapse.getPresynapticCell()))
                {
                    activeSynapses.Add(synapse);
                }
            }

            return activeSynapses;
        }

        /**
         * Sets the last iteration in which this segment was active.
         * @param iteration
         */
        public void setLastUsedIteration(long iteration)
        {
            this.m_LastUsedIteration = iteration;
        }

        /**
         * Returns the iteration in which this segment was last active.
         * @return  the iteration in which this segment was last active.
         */
        public long getLastUsedIteration()
        {
            return m_LastUsedIteration;
        }

        /**
         * Returns this {@code DistalDendrite} segment's ordinal
         * @return	this segment's ordinal
         */
        public int getOrdinal()
        {
            return ordinal;
        }

        /**
         * Sets the ordinal value (used for age determination) on this segment.
         * @param ordinal	the age or order of this segment
         */
        public void setOrdinal(int ordinal)
        {
            this.ordinal = ordinal;
        }


        public override String ToString()
        {
            return $"DistalDendrite: Indx:{this.getIndex()}";
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
            if (this.getIndex() > other.getIndex())
                return 1;
            else if (this.getIndex() < other.getIndex())
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

