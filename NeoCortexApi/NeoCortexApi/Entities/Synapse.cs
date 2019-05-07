using System;
using System.Collections.Generic;
using System.Text;


namespace NeoCortexApi.Entities
{
    /**
  * Represents a connection with varying strength which when above 
  * a configured threshold represents a valid connection. 
  * 
  * IMPORTANT: 	For DistalDendrites, there is only one synapse per pool, so the
  * 				synapse's index doesn't really matter (in terms of tracking its
  * 				order within the pool). In that case, the index is a global counter
  * 				of all distal dendrite synapses.
  * 
  * 				For ProximalDendrites, there are many synapses within a pool, and in
  * 				that case, the index specifies the synapse's sequence order within
  * 				the pool object, and may be referenced by that index.
  *    
  * 
  * @author Chetan Surpur
  * @author David Ray
  * 
  * @see DistalDendrite
  * @see Connections
  */
 
   // [System.SerializableAttribute] 
    public class Synapse : IEquatable<Synapse>, IComparable<Synapse>
    {
        /// <summary>
        /// Cell which activates this synapse. On proximal dendrite is this set on NULL. That means proximal dentrites have no presynaptic cell.
        /// </summary>
        private Cell sourceCell;

        private Segment segment;
        private Pool pool;
        private int synapseIndex;
        private Integer boxedIndex;

        /// <summary>
        /// Index of pre-synaptic cell.
        /// </summary>
        private int inputIndex;
        private double permanence;
        private bool m_Isdestroyed;

        /**
         * Constructor used when setting parameters later.
         */
        public Synapse() { }

        /**
         * Constructs a new {@code Synapse} for a {@link DistalDendrite}
         * @param sourceCell    the {@link Cell} which will activate this {@code Synapse};
         * @param segment       the owning dendritic segment
         * @param pool          this {@link Pool} of which this synapse is a member
         * @param index         this {@code Synapse}'s index
         * @param permanence    
         */
        public Synapse(Cell presynapticCell, Segment segment, int index, double permanence)
        {
            this.sourceCell = presynapticCell;
            this.segment = segment;
            this.synapseIndex = index;
            this.boxedIndex = new Integer(index);
            this.inputIndex = presynapticCell.Index;
            this.permanence = permanence;
        }

        /**
         * Constructs a new {@code Synapse}
         * 
         * @param c             the connections state of the temporal memory
         * @param sourceCell    the {@link Cell} which will activate this {@code Synapse};
         *                      Null if this Synapse is proximal
         * @param segment       the owning dendritic segment
         * @param pool		    this {@link Pool} of which this synapse is a member
         * @param index         this {@code Synapse}'s index
         * @param inputIndex	the index of this {@link Synapse}'s input; be it a Cell or InputVector bit.
         */
        public Synapse(Cell sourceCell, Segment segment, Pool pool, int index, int inputIndex)
        {
            this.sourceCell = sourceCell;
            this.segment = segment;
            this.pool = pool;
            this.synapseIndex = index;
            this.boxedIndex = new Integer(index);
            this.inputIndex = inputIndex;
        }

        /**
         * Returns this {@code Synapse}'s index.
         * @return
         */
        public int getIndex()
        {
            return synapseIndex;
        }

        /**
         * Returns the index of this {@code Synapse}'s input item
         * whether it is a "sourceCell" or inputVector bit.
         * @return
         */
        public int getInputIndex()
        {
            return inputIndex;
        }

        /**
         * Returns this {@code Synapse}'s degree of connectedness.
         * @return
         */
        public double getPermanence()
        {
            return permanence;
        }

        /**
         * Sets this {@code Synapse}'s degree of connectedness.
         * @param perm
         */
        public void setPermanence(double synPermConnected, double perm)
        {
            this.permanence = perm;

            // On proximal dendrite which has no presynaptic cell
            if (sourceCell == null)
            {
                pool.updatePool(synPermConnected, this, perm);
            }
        }

        /**
         * Returns the owning dendritic segment
         * @return
         */
        public Segment getSegment()
        {
            return segment;
        }

        /**
         * Called by {@link Connections#destroySynapse(Synapse)} to assign
         * a reused Synapse to another presynaptic Cell
         * @param cell  the new presynaptic cell
         */
        public void setPresynapticCell(Cell cell)
        {
            this.sourceCell = cell;
        }

        /**
         * Returns the containing {@link Cell} 
         * @return
         */
        public Cell getPresynapticCell()
        {
            return sourceCell;
        }

        /**
         * Returns the flag indicating whether this segment has been destroyed.
         * @return  the flag indicating whether this segment has been destroyed.
         */
        public bool destroyed()
        {
            return m_Isdestroyed;
        }

        /**
         * Sets the flag indicating whether this segment has been destroyed.
         * @param b the flag indicating whether this segment has been destroyed.
         */
        public void setDestroyed(bool b)
        {
            this.m_Isdestroyed = b;
        }

        /**
         * {@inheritDoc}
         */
        public override String ToString()
        {
            string srcCell = String.Empty;
            if (sourceCell != null)
            {
                srcCell = $"[SrcCell: {sourceCell.ToString()}]";
            }

            return $"Syn: synIndx:{synapseIndex}, inpIndx:{inputIndex}, perm:{this.permanence}[{segment}], {srcCell}";
        }

    

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + inputIndex;
            result = prime * result + ((segment == null) ? 0 : segment.GetHashCode());
            result = prime * result + ((sourceCell == null) ? 0 : sourceCell.GetHashCode());
            result = prime * result + synapseIndex;
            return result;
        }


        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */
       // @Override
    public bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (typeof(Synapse) != obj.GetType())
                return false;
            Synapse other = (Synapse)obj;
            if (inputIndex != other.inputIndex)
                return false;
            if (segment == null)
            {
                if (other.segment != null)
                    return false;
            }
            else if (!segment.Equals(other.segment))
                return false;
            if (sourceCell == null)
            {
                if (other.sourceCell != null)
                    return false;
            }
            else if (!sourceCell.Equals(other.sourceCell))
                return false;
            if (synapseIndex != other.synapseIndex)
                return false;
            if (permanence != other.permanence)
                return false;
            return true;
        }

        public bool Equals(Synapse obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (inputIndex != obj.inputIndex)
                return false;

            //
            // Synapses are equal if they belong to the same segment.
            if (segment == null && obj.segment != null)
            {
                return false;
            }
            else if (!segment.Equals(obj.segment))
                return false;

            if (sourceCell == null)
            {
                if (obj.sourceCell != null)
                    return false;
            }
            else if (!sourceCell.Equals(obj.sourceCell))
                return false;
            if (synapseIndex != obj.synapseIndex)
                return false;
            if (permanence != obj.permanence)
                return false;
            return true;
        }

        public int CompareTo(Synapse other)
        {
            if (this.getIndex() < other.getIndex())
                return -1;
            if (this.getIndex() > other.getIndex())
                return 1;
            else
                return 0;
        }
    }
}
