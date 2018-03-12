using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
   
        /**
 * Base class which handles the creation of {@link Synapse}s on behalf of
 * inheriting class types.
 * 
 * @author David Ray
 * @see DistalDendrite
 * @see ProximalDendrite
 */
        public abstract class Segment :  IEquatable<Segment> {
    /** keep it simple */
    private static readonly long serialVersionUID = 1L;

        protected int index;
        protected Integer boxedIndex;

        public Segment(int index)
        {
            this.index = index;
            this.boxedIndex = new Integer(index);
        }

        /**
         * Returns this {@link ProximalDendrite}'s index.
         * @return
         */
        public int getIndex()
        {
            return index;
        }

        /**
         * <p>
         * Creates and returns a newly created {@link Synapse} with the specified
         * source cell, permanence, and index.
         * </p><p>
         * IMPORTANT: 	<b>This method is only called for Proximal Synapses.</b> For ProximalDendrites, 
         * 				there are many synapses within a pool, and in that case, the index 
         * 				specifies the synapse's sequence order within the pool object, and may 
         * 				be referenced by that index.
         * </p>
         * @param c             the connections state of the temporal memory
         * @param sourceCell    the source cell which will activate the new {@code Synapse}
         * @param pool		    the new {@link Synapse}'s pool for bound variables.
         * @param index         the new {@link Synapse}'s index.
         * @param inputIndex	the index of this {@link Synapse}'s input (source object); be it a Cell or InputVector bit.
         * 
         * @return the newly created {@code Synapse}
         * @see Connections#createSynapse(DistalDendrite, Cell, double)
         */
        public Synapse createSynapse(Connections c, List<Synapse> syns, Cell sourceCell, Pool pool, int index, int inputIndex)
        {
            Synapse s = new Synapse(c, sourceCell, this, pool, index, inputIndex);
            syns.Add(s);
            return s;
        }

        //    /**
        //     * {@inheritDoc}
        //     * 
        //     * <em> Note: All comparisons use the segment's index only </em>
        //     */
        //    @Override
        //public int compareTo(Segment arg0)
        //    {
        //        return boxedIndex.compareTo(arg0.boxedIndex);
        //    }

        //    /* (non-Javadoc)
        //     * @see java.lang.Object#hashCode()
        //     */
        //    @Override
     

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + index;
            return result;
        }

        //    /* (non-Javadoc)
        //     * @see java.lang.Object#equals(java.lang.Object)
        //     */
        //    @Override
        //public boolean equals(Object obj)
        //    {
        //        if (this == obj)
        //            return true;
        //        if (obj == null)
        //            return false;
        //        if (getClass() != obj.getClass())
        //            return false;
        //        Segment other = (Segment)obj;
        //        if (index != other.index)
        //            return false;
        //        return true;
        //    }

        public virtual bool Equals(Segment obj)
            {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            if (index != obj.index)
                return false;
            else
                return true;
        }

     
    }
}

