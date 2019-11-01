using System;
using System.Collections.Generic;
using NeoCortexApi.Entities;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Defines a single cell (neuron).
    /// </summary>
    public class Cell : IEquatable<Cell>, IComparable<Cell>
    {

        /// <summary>
        /// Index of the cell.
        /// </summary>
        public int Index { get; set; }

        //public List<DistalDendrite> Segments
        //{
        //    get;
        //}

        /// <summary>
        /// The column, which owns this cell.
        /// </summary>
        public int ParentColumnIndex { get; set; }

        /** Cash this because Cells are immutable */
        private readonly int m_Hashcode;

        public Cell()
        {
            //this.Segments = new List<DistalDendrite>(); 
        }
        /**
         * Constructs a new {@code Cell} object
         * @param column    the containing {@link Column}
         * @param colSeq    this index of this {@code Cell} within its column
         */
        public Cell(int parentColumnIndx, int colSeq, int numCellsPerColumn)
        {
            this.ParentColumnIndex = parentColumnIndx;
            //this.Index = parentColumnIndx.getIndex() * parentColumnIndx.getNumCellsPerColumn() + colSeq;
            this.Index = parentColumnIndx * numCellsPerColumn + colSeq;
            //this.Segments = new List<DistalDendrite>();
        }


        /**
         * Returns the column within which this cell resides
         * @return
         */
        public int getParentColumnIndex()
        {
            return ParentColumnIndex;
        }

        /**
         * Returns the Set of {@link Synapse}s which have this cell
         * as their source cells.
         *  
         * @param   c               the connections state of the temporal memory
         *                          return an orphaned empty set.
         * @return  the Set of {@link Synapse}s which have this cell
         *          as their source cells.
         */
        public ISet<Synapse> getReceptorSynapses(Connections c)
        {
            return getReceptorSynapses(c, false);
        }

        /**
         * Returns the Set of {@link Synapse}s which have this cell
         * as their source cells.
         *  
         * @param   c               the connections state of the temporal memory
         * @param doLazyCreate      create a container for future use if true, if false
         *                          return an orphaned empty set.
         * @return  the Set of {@link Synapse}s which have this cell
         *          as their source cells.
         */
        public ISet<Synapse> getReceptorSynapses(Connections c, bool doLazyCreate)
        {
            return c.getReceptorSynapses(this, doLazyCreate);
        }

        /**
         * Returns a {@link List} of this {@code Cell}'s {@link DistalDendrite}s
         * 
         * @param   c               the connections state of the temporal memory
         * @param doLazyCreate      create a container for future use if true, if false
         *                          return an orphaned empty set.
         * @return  a {@link List} of this {@code Cell}'s {@link DistalDendrite}s
         */
        public List<DistalDendrite> getSegments(Connections c)
        {
            return getSegments(c, false);
        }

        ///**
        // * Returns a {@link List} of this {@code Cell}'s {@link DistalDendrite}s
        // * 
        // * @param   c               the connections state of the temporal memory
        // * @param doLazyCreate      create a container for future use if true, if false
        // *                          return an orphaned empty set.
        // * @return  a {@link List} of this {@code Cell}'s {@link DistalDendrite}s
        // */
        public List<DistalDendrite> getSegments(Connections c, bool doLazyCreate)
        {
            return c.getSegments(this, doLazyCreate);
        }



        ///**
        // * {@inheritDoc}
        // * 
        // * <em> Note: All comparisons use the cell's index only </em>
        // */
        //@Override
        //public int compareTo(Cell arg0)
        //{
        //    return boxedIndex.compareTo(arg0.boxedIndex);
        //}

        public override int GetHashCode()
        {
            if (m_Hashcode == 0)
            {
                int prime = 31;
                int result = 1;
                result = prime * result + Index;
                return result;
            }
            return m_Hashcode;
        }



        public bool Equals(Cell obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            if (Index != obj.Index)
                return false;
            else
                return true;
        }

        public override string ToString()
        {
            return $"Cell: Indx={this.Index}, [{this.ParentColumnIndex}]";
        }


        /// <summary>
        /// Compares two cells.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Cell other)
        {
            if (this.Index < other.Index)
                return -1;
            else if (this.Index > other.Index)
                return 1;
            else
                return 0;
        }
    }
}