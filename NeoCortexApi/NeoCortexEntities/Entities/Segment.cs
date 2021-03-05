// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoCortexApi.Entities
{


    /// <summary>
    /// Base class for different types of segments. It which handles the creation of synapses (<seealso cref="Synapse"/>) on behalf of inheriting class types.
    /// </summary>

    public abstract class Segment : IEquatable<Segment>
    {       
        /// <summary>
        /// The index of the segment.
        /// </summary>
        public int SegmentIndex { get; set; }

        protected Integer boxedIndex { get; set; }

        /// <summary>
        /// Synapses connected to the segment.
        /// </summary>
        public List<Synapse> Synapses { get; set; }

        /// <summary>
        /// Permanence threshold value to declare synapse as connected.
        /// </summary>
        public double SynapsePermConnected { get; set; }

        /// <summary>
        /// Number of input cells. Used by proximal dendrite segment by Spatial Pooler.
        /// </summary>
        public int NumInputs { get; set; }

        /// <summary>
        /// Default constructor used by deserializer.
        /// </summary>
        protected Segment()
        {
            this.Synapses = new List<Synapse>();
            this.boxedIndex = new Integer();

        }

        /// <summary>
        /// Creates the proximal dentrite segment with specified index.
        /// </summary>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="index">Index of segment.</param>
        /// <param name="numInputs">Number of input neorn cells.</param>
        public Segment(int index, double synapsePermConnected, int numInputs)
        {
            this.NumInputs = numInputs;
            this.SynapsePermConnected = synapsePermConnected;
            this.Synapses = new List<Synapse>();
            this.SegmentIndex = index;
            this.boxedIndex = new Integer(index);
        }


        /// <summary>
        /// Creates and returns a newly created synapse with the specified source cell, permanence, and index.
        /// </summary>       
        /// <param name="sourceCell">This value is typically set to NULL in a case of proximal segment. This is because, proximal segments 
        /// build synaptic connections from column to the sensory input. They do not cobbect a specific cell inside of the column.</param>
        /// <param name="index">Sequence within gthe pool.</param>
        /// <param name="inputIndex">The index of the sensory neuron connected by this synapse.</param>
        /// <remarks>
        /// <b>This method is only called for Proximal Synapses.</b> For ProximalDendrites, there are many synapses within a pool, and in that case, the index
        /// specifies the synapse's sequence order within the pool object, and may be referenced by that index</remarks>
        /// <returns>Instance of the new synapse.</returns>
        /// <seealso cref="Synapse"/>
        public Synapse CreateSynapse(Cell sourceCell, int index, int inputIndex)
        {
            Synapse synapse = new Synapse(sourceCell, this.SegmentIndex, index, inputIndex);
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
            result = prime * result + SegmentIndex;
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

            if (SegmentIndex != obj.SegmentIndex)
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
            return $"Seg: {this.SegmentIndex}";
        }

        #region Serialization
        public virtual void Serialize(StreamWriter writer)
        {
            throw new NotImplementedException();
            //HtmSerializer2 ser = new HtmSerializer2();

            //ser.SerializeBegin(nameof(Segment), writer);

            //ser.SerializeValue(this.SegmentIndex, writer);
            //this.boxedIndex.Serialize(writer);
            //ser.SerializeValue(this.Synapses, writer);
            //ser.SerializeValue(this.SynapsePermConnected, writer);
            //ser.SerializeValue(this.NumInputs, writer);

            //ser.SerializeEnd(nameof(Segment), writer);
        }

     

       
        #endregion
    }
}

