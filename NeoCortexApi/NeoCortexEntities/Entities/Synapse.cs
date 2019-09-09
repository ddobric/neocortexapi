using Newtonsoft.Json;
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
        public Cell SourceCell { get; set; }

        [JsonIgnore]
        public Segment Segment { get; set; }

        //public Pool Pool {get;set;}

        public int SynapseIndex { get; set; }

        public Integer BoxedIndex { get; set; }

        /// <summary>
        /// Index of pre-synaptic cell.
        /// </summary>
        public int InputIndex { get; set; }

        public double Permanence { get; set; }

        public bool m_Isdestroyed { get; set; }

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
            this.SourceCell = presynapticCell;
            this.Segment = segment;
            this.SynapseIndex = index;
            this.BoxedIndex = new Integer(index);
            this.InputIndex = presynapticCell.Index;
            this.Permanence = permanence;
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
        public Synapse(Cell sourceCell, Segment segment, int synapseIndex, int inputIndex)
        {
            this.SourceCell = sourceCell;
            this.Segment = segment;
            //this.Pool = pool;
            this.SynapseIndex = synapseIndex;
            this.BoxedIndex = new Integer(synapseIndex);
            this.InputIndex = inputIndex;
        }

        /**
         * Returns this {@code Synapse}'s index.
         * @return
         */
        public int getIndex()
        {
            return SynapseIndex;
        }

        /**
         * Returns the index of this {@code Synapse}'s input item
         * whether it is a "sourceCell" or inputVector bit.
         * @return
         */
        public int getInputIndex()
        {
            return InputIndex;
        }

        /**
         * Returns this {@code Synapse}'s degree of connectedness.
         * @return
         */
        public double getPermanence()
        {
            return Permanence;
        }

        /**
         * Sets this {@code Synapse}'s degree of connectedness.
         * @param perm
         * TODO: Remove synPermConnected. Not used here
         */
        public void setPermanence(double synPermConnected, double perm)
        {
            this.Permanence = perm;

            //// On proximal dendrite which has no presynaptic cell
            //if (SourceCell == null)
            //{
            //    Pool.updatePool(synPermConnected, this, perm);
            //}
        }

        /**
         * Returns the owning dendritic segment
         * @return
         */
        public Segment getSegment()
        {
            return Segment;
        }

        /**
         * Called by {@link Connections#destroySynapse(Synapse)} to assign
         * a reused Synapse to another presynaptic Cell
         * @param cell  the new presynaptic cell
         */
        public void setPresynapticCell(Cell cell)
        {
            this.SourceCell = cell;
        }

        /**
         * Returns the containing {@link Cell} 
         * @return
         */
        public Cell getPresynapticCell()
        {
            return SourceCell;
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
            if (SourceCell != null)
            {
                srcCell = $"[SrcCell: {SourceCell.ToString()}]";
            }

            return $"Syn: synIndx:{SynapseIndex}, inpIndx:{InputIndex}, perm:{this.Permanence}[{Segment}], {srcCell}";
        }

    

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + InputIndex;
            result = prime * result + ((Segment == null) ? 0 : Segment.GetHashCode());
            result = prime * result + ((SourceCell == null) ? 0 : SourceCell.GetHashCode());
            result = prime * result + SynapseIndex;
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
            if (InputIndex != other.InputIndex)
                return false;
            if (Segment == null)
            {
                if (other.Segment != null)
                    return false;
            }
            else if (!Segment.Equals(other.Segment))
                return false;
            if (SourceCell == null)
            {
                if (other.SourceCell != null)
                    return false;
            }
            else if (!SourceCell.Equals(other.SourceCell))
                return false;
            if (SynapseIndex != other.SynapseIndex)
                return false;
            if (Permanence != other.Permanence)
                return false;
            return true;
        }

        public bool Equals(Synapse obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (InputIndex != obj.InputIndex)
                return false;

            //
            // Synapses are equal if they belong to the same segment.
            if (Segment == null && obj.Segment != null)
            {
                return false;
            }
            else if (!Segment.Equals(obj.Segment))
                return false;

            if (SourceCell == null)
            {
                if (obj.SourceCell != null)
                    return false;
            }
            else if (!SourceCell.Equals(obj.SourceCell))
                return false;
            if (SynapseIndex != obj.SynapseIndex)
                return false;
            if (Permanence != obj.Permanence)
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
