// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
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
               

        /// <summary>
        /// The column, which owns this cell.
        /// </summary>
        public int ParentColumnIndex { get; set; }

        /// <summary>
        /// Stores the calculated cell's hashcode.
        /// </summary>
        private readonly int m_Hashcode;

        /// <summary>
        /// Used for testing.
        /// </summary>
        public Cell()
        {
            //this.Segments = new List<DistalDendrite>(); 
        }
       
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

    
        /// <summary>
        /// Returns the Set of <see cref="Synapse"/>s which have this cell as their source cell.
        /// </summary>
        /// <param name="c">the connections state of the temporal memory</param>
        /// <param name="doLazyCreate">create a container for future use if true, if false return an orphaned empty set.</param>
        /// <returns>the Set of <see cref="Synapse"/>s which have this cell as their source cells.</returns>
        public ISet<Synapse> GetReceptorSynapses(Connections c, bool doLazyCreate = false)
        {
            return c.GetReceptorSynapses(this, doLazyCreate);
        }

     
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

        /// <summary>
        /// Gets the hashcode of the cell.
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
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