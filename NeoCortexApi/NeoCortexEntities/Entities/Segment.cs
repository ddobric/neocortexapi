using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{


    /// <summary>
    /// Base class which handles the creation of <seealso cref="Synapse"/> on behalf of inheriting class types.
    /// </summary>
   // [Serializable]
    public abstract class Segment : IEquatable<Segment>
    {       
        public int ParentColumnIndex { get; set; }

        public Integer boxedIndex { get; set; }

        public List<Synapse> Synapses { get; set; }

        /// <summary>
        /// Permanence threshold value to declare synapse as connected.
        /// </summary>
        public double SynapsePermConnected { get; set; }

        /// <summary>
        /// Number of input neorn cells.
        /// </summary>
        public int NumInputs { get; set; }

        /// <summary>
        /// Creates the proximal dentrite segment with specified index.
        /// </summary>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="index">Index of segment.</param>
        /// <param name="numInputs">Number of input neorn cells.</param>
        public Segment(int index, double synapsePermConnected, int numInputs)
        {
            this.NumInputs = NumInputs;
            this.SynapsePermConnected = synapsePermConnected;
            this.Synapses = new List<Synapse>();
            this.ParentColumnIndex = index;
            this.boxedIndex = new Integer(index);
        }
    

        /// <summary>
        /// Returns the index of proximal dentrite.
        /// </summary>
        /// <seealso cref="ProximalDendrite"/>
        /// <returns>Index</returns>
        public int getIndex()
        {
            return ParentColumnIndex;
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
        /// <param name="synapses">List of synapses, where one has to be added.</param>
        /// <param name="sourceCell"></param>
        /// <param name="pool"></param>
        /// <param name="index">Sequence within gthe pool.</param>
        /// <param name="inputIndex"></param>
        /// <remarks>
        /// This method is only called for Proximal Synapses. For ProximalDendrites, 
        /// there are many synapses within a pool, and in that case, the index 
        /// specifies the synapse's sequence order within the pool object, and may be referenced by that index</remarks>
        /// <returns>Instance of the new synapse.</returns>
        /// <seealso cref="Synapse"/>
        public Synapse createSynapse(Cell sourceCell, int index, int inputIndex)
        {
            Synapse synapse = new Synapse(sourceCell, this, index, inputIndex);
            this.Synapses.Add(synapse);
            return synapse;
        }

       
        /// <summary>
        /// Hashcode calculation.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ParentColumnIndex;
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

            if (ParentColumnIndex != obj.ParentColumnIndex)
                return false;
            else
                return true;
        }

        public override string ToString()
        {
            return $"Seg: {this.ParentColumnIndex}";
        }
    }
}

