// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;

namespace NeoCortexApi.Entities
{


    /// <summary>
    /// Base class for different types of segments. It which handles the creation of synapses (<seealso cref="Synapse"/>) on behalf of inheriting class types.
    /// The HTM defines following segment types: Proximal, Distal and Apical.
    /// Proximal segment connects mini-columns to sensory cells.<br/>
    /// Distal (or basal) segment connects cells between mini-columns.<br/>
    /// Apical segment connects cells between different regions.
    /// </summary>
    public abstract class Segment : IEquatable<Segment>
    {
        /// <summary>
        /// The index of the segment.
        /// </summary>
        public int SegmentIndex { get; set; }

        //protected Integer boxedIndex { get; set; }

        /// <summary>
        /// Synapses connected to the segment. Also called potential synapses.
        /// </summary>
        public List<Synapse> Synapses { get; set; }

        /// <summary>
        /// Permanence threshold value to declare synapse as connected.
        /// </summary>
        protected double SynapsePermConnected { get; set; }

        /// <summary>
        /// Number of input cells. Used by proximal dendrite segment by Spatial Pooler.
        /// </summary>
        public int NumInputs { get; set; }

        /// <summary>
        /// Default constructor used by serialization.
        /// </summary>
        protected Segment()
        {
            this.Synapses = new List<Synapse>();
            // this.boxedIndex = new Integer();

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
            //this.boxedIndex = new Integer(index);
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
        }




        #endregion
    }
}

