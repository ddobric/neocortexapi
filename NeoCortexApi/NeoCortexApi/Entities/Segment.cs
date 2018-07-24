using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{


    /// <summary>
    /// Base class which handles the creation of <seealso cref="Synapse"/> on behalf of inheriting class types.
    /// </summary>
    public abstract class Segment : IEquatable<Segment>
    {
        private static readonly long serialVersionUID = 1L;

        protected int index;

        protected Integer boxedIndex;


        /// <summary>
        /// Creates the proximal dentrite segment with specified index.
        /// </summary>
        /// <param name="index">Index of segment.</param>
        public Segment(int index)
        {
            this.index = index;
            this.boxedIndex = new Integer(index);
        }
    

        /// <summary>
        /// Returns the index of proximal dentrite.
        /// </summary>
        /// <seealso cref="ProximalDendrite"/>
        /// <returns>Index</returns>
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



        /// <summary>
        /// Creates and returns a newly created synapse with the specified
        /// source cell, permanence, and index.
        /// </summary>
        /// <param name="c">Memory instance.</param>
        /// <param name="syns">List of synapses, where th ene one has to be added.</param>
        /// <param name="sourceCell"></param>
        /// <param name="pool"></param>
        /// <param name="index"></param>
        /// <param name="inputIndex"></param>
        /// <remarks>
        /// This method is only called for Proximal Synapses. For ProximalDendrites, 
        /// there are many synapses within a pool, and in that case, the index 
        /// specifies the synapse's sequence order within the pool object, and may be referenced by that index</remarks>
        /// <returns>Instance of the new synapse.</returns>
        /// <seealso cref="Synapse"/>
        public Synapse createSynapse(Connections c, List<Synapse> syns, Cell sourceCell, Pool pool, int index, int inputIndex)
        {
            Synapse s = new Synapse(c, sourceCell, this, pool, index, inputIndex);
            syns.Add(s);
            return s;
        }

       
        /// <summary>
        /// Hashcode calculation.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + index;
            return result;
        }

        /// <summary>
        /// Compares two segments
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

