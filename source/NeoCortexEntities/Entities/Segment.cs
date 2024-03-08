// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        private long m_LastUsedIteration;

        /// <summary>
        /// the last iteration in which this segment was active.
        /// </summary>
        public long LastUsedIteration { get => m_LastUsedIteration; set => m_LastUsedIteration = value; }


        /// <summary>
        /// The cell that owns (parent) the segment.
        /// </summary>        
        public Cell ParentCell;

        /// <summary>
        /// The index of the segment.
        /// </summary>
        public int SegmentIndex { get; set; }

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
        /// Gets the number of connected (active) synapses.These are synapses with premanence value greather than <see cref="HtmConfig.SynapsePermConnected"/>.
        /// </summary>
        public int NumConnectedSynapses
        {
            get
            {
                return this.Synapses.Count(s => s.Permanence >= this.SynapsePermConnected);
            }
        }

        /// <summary>
        /// Default constructor used by serialization.
        /// </summary>
        protected Segment()
        {
            this.Synapses = new List<Synapse>();
        }


        /// <summary>
        /// Creates the proximal dentrite segment with specified index.
        /// </summary>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="index">Index of segment.</param>
        /// <param name="numInputs">Number of input cells.</param>
        public Segment(int index, long lastUsedIteration, double synapsePermConnected, int numInputs)
        {
            this.NumInputs = numInputs;
            this.SynapsePermConnected = synapsePermConnected;
            this.Synapses = new List<Synapse>();
            this.SegmentIndex = index;
            this.m_LastUsedIteration = lastUsedIteration;
        }

        public void KillSynapse(Synapse synapse)
        {
            synapse.SourceCell.ReceptorSynapses.Remove(synapse);

            this.Synapses.Remove(synapse);
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
        /// Used internally to find the synapse with the smallest permanence
        /// on the given segment.
        /// </summary>
        /// <param name="seg">Segment object to search for synapses on</param>
        /// <returns>Synapse object on the segment with the minimal permanence</returns>
        public Synapse GetMinPermanenceSynapse()
        {
            List<Synapse> synapses = this.Synapses;

            Synapse min = null;

            double minPermanence = Double.MaxValue;

            foreach (Synapse synapse in synapses)
            {
                if (!synapse.IsDestroyed && synapse.Permanence < minPermanence - 0.00001)
                {
                    min = synapse;
                    minPermanence = synapse.Permanence;
                }
            }

            return min;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sbPerms = new StringBuilder();

            foreach (var syn in Synapses)
            {
                sbPerms.Append($" {syn.Permanence}");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"\tcell:{this.ParentCell.Index}/seg {this.SegmentIndex}, Synapses: {this.Synapses.Count}, Active Synapses: {this.Synapses.Where(s => s.Permanence > SynapsePermConnected).Count()}, [Permanences: {sbPerms}]");
                     
            return sb.ToString();


        }

    }
}

