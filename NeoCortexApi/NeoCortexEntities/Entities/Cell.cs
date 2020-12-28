// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

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
        public int CellId { get; }

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
         * 
         * @param column    the containing {@link Column}
         * @param colSeq    
         */
        /// <summary>
        /// Constructs a new <see cref="Cell"/> object
        /// </summary>
        /// <param name="parentColumnIndx"></param>
        /// <param name="colSeq">the index of this <see cref="Cell"/> within its column</param>
        /// <param name="numCellsPerColumn"></param>
        /// <param name="cellId"></param>
        /// <param name="cellActivity"></param>
        public Cell(int parentColumnIndx, int colSeq, int numCellsPerColumn, int cellId, CellActivity cellActivity)
        {
            this.ParentColumnIndex = parentColumnIndx;
            //this.Index = parentColumnIndx.getIndex() * parentColumnIndx.getNumCellsPerColumn() + colSeq;
            this.Index = parentColumnIndx * numCellsPerColumn + colSeq;
            //this.Segments = new List<DistalDendrite>();
            this.CellId = cellId;
        }


        /**
         * Returns the column within which this cell resides
         * @return
         */
        //public int getParentColumnIndex()
        //{
        //    return ParentColumnIndex;
        //}

        /**
         * Returns the Set of {@link Synapse}s which have this cell
         * as their source cells.
         *  
         * @param   c               the connections state of the temporal memory
         *                          return an orphaned empty set.
         * @return  the Set of {@link Synapse}s which have this cell
         *          as their source cells.
         */
        //public ISet<Synapse> getReceptorSynapses(Connections c)
        //{
        //    return getReceptorSynapses(c, false);
        //}

        /// <summary>
        /// Returns the Set of <see cref="Synapse"/>s which have this cell as their source cells.
        /// </summary>
        /// <param name="c">the connections state of the temporal memory</param>
        /// <param name="doLazyCreate">create a container for future use if true, if false return an orphaned empty set.</param>
        /// <returns>the Set of <see cref="Synapse"/>s which have this cell as their source cells.</returns>
        public ISet<Synapse> GetReceptorSynapses(Connections c, bool doLazyCreate = false)
        {
            return c.GetReceptorSynapses(this, doLazyCreate);
        }

        /**
         * Returns a {@link List} of this {@code Cell}'s {@link DistalDendrite}s
         * 
         * @param   c               the connections state of the temporal memory
         * @param doLazyCreate      create a container for future use if true, if false
         *                          return an orphaned empty set.
         * @return  a {@link List} of this {@code Cell}'s {@link DistalDendrite}s
         */
        //public List<DistalDendrite> getSegments(Connections c)
        //{
        //    return getSegments(c, false);
        //}

        /// <summary>
        /// Returns a <see cref="List{T}"/> of this <see cref="Cell"/>'s <see cref="DistalDendrite"/>s
        /// </summary>
        /// <param name="c">the connections state of the temporal memory</param>
        /// <param name="doLazyCreate">create a container for future use if true, if false return an orphaned empty set.</param>
        /// <returns>a <see cref="List{T}"/> of this <see cref="Cell"/>'s <see cref="DistalDendrite"/>s</returns>
        public List<DistalDendrite> GetSegments(Connections c, bool doLazyCreate = false)
        {
            return c.GetSegments(this, doLazyCreate);
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
        #region Serialization
        public void serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Cell), writer);
            ser.SerializeValue(this.CellId, writer);

            ser.SerializeEnd(nameof(Cell), writer);
        }
        #endregion
    }
}