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
    public class DistalDendrite : Segment //implements Persistable
    {
        /** keep it simple */
        private static readonly long serialVersionUID = 1L;

        private Cell cell;

        private long m_LastUsedIteration;

        public int ordinal = -1;

        /**
         * Constructs a new {@code Segment} object with the specified owner
         * {@link Cell} and the specified index.
         * 
         * @param cell      the owner
         * @param flatIdx     this {@code Segment}'s index.
         */
        public DistalDendrite(Cell cell, int flatIdx, long lastUsedIteration, int ordinal) : base(flatIdx)
        {
            this.cell = cell;
            this.ordinal = ordinal;
            this.m_LastUsedIteration = lastUsedIteration;
        }

        /**
         * Returns the owner {@link Cell}
         * 
         * @return
         */
        public Cell getParentCell()
        {
            return cell;
        }

        /**
         * Returns all {@link Synapse}s
         * 
         * @param c     the connections state of the temporal memory
         * @return
         */
        public List<Synapse> getAllSynapses(Connections c)
        {
            return c.getSynapses(this);
        }

        /**
         * Returns the synapses on a segment that are active due to lateral input
         * from active cells.
         * 
         * @param c                 the layer connectivity
         * @param activeCells       the active cells
         * @return  Set of {@link Synapse}s connected to active presynaptic cells.
         */
        public ISet<Synapse> getActiveSynapses(Connections c, ISet<Cell> activeCells)
        {
            ISet<Synapse> synapses = new LinkedHashSet<Synapse>();
            
            foreach (var synapse in c.getSynapses(this))
            {
                if (activeCells.Contains(synapse.getPresynapticCell()))
                {
                    synapses.Add(synapse);
                }
            }

            return synapses;
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
            return index.ToString();
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */

        public override int GetHashCode()
        {
            int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + ((cell == null) ? 0 : cell.GetHashCode());
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
            if (cell == null)
            {
                if (other.cell != null)
                    return false;
            }
            else if (!cell.Equals(other.cell))
                return false;

            return true;
        }
    }
}

