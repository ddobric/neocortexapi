// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Convenience container for "bound" <see cref="Synapse"/> values which can be dereferenced from both a Synapse and the <see cref="Connections"/> object.
    /// All Synapses will have a reference to a <see cref="Pool"/> to retrieve relevant values. In addition, same pool can be referenced from the Connections 
    /// object externally which will update the Synapse's internal reference.
    /// </summary>
    /// <remarks>
    /// Authors of the JAVA implementation: David Ray
    /// </remarks>

    //[Serializable]
    public class Pool
    {
        int Size;

        /// <summary>
        /// Number of inut neuron cells.
        /// </summary>
        public int NumInputs { get; set; }

        /// <summary>
        /// Allows fast removal of connected synapse indexes. List of connected synapses. 
        /// These are synapses with permanence value greather than permanence connected threshold.
        /// See synPermConnected.
        /// </summary>      
        private List<int> m_SynapseConnections { get; set; }  = new List<int>();

        /// <summary>
        /// Indexed according to the source Input Vector Bit (for ProximalDendrites), and source cell (for DistalDendrites).
        /// </summary>
        public Dictionary<int, Synapse> m_SynapsesBySourceIndex { get; set; }  = new Dictionary<int, Synapse>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Number of connected input neurons. These neurons define RF fo the segment, which owns this pool.</param>
        /// <param name="numInputs">Total number of input neurons.</param>
        public Pool(int size, int numInputs)
        {
            this.NumInputs = numInputs;
            this.Size = size;
        }

        /**
         * Returns the permanence value for the {@link Synapse} specified.
         * 
         * @param s	the Synapse
         * @return	the permanence
         */
        //public double getPermanence(Synapse s)
        //{
        //    return synapsesBySourceIndex[s.getInputIndex()].getPermanence();
        //}

        /**
         * Sets the specified  permanence value for the specified {@link Synapse}
         * @param s
         * @param permanence
         */
        //public void setPermanence(Connections c, Synapse s, double permanence)
        //{
        //    s.setPermanence(c, permanence);
        //}

       
        /// <summary>
        /// Updates this <see cref="Pool"/>'s store of permanences for the specified <see cref="Synapse"/>
        /// </summary>
        /// <param name="synPermConnected"></param>
        /// <param name="synapse">the synapse who's permanence is recorded</param>
        /// <param name="permanence">the permanence value to record</param>
        public void UpdatePool(double synPermConnected, Synapse synapse, double permanence)
        {
            int inputIndex = synapse.InputIndex;
            if (m_SynapsesBySourceIndex.ContainsKey(inputIndex) == false)
            {
                m_SynapsesBySourceIndex.Add(inputIndex, synapse);
            }
            if (permanence >= synPermConnected)
            {
                m_SynapseConnections.Add(inputIndex);
            }
            else
            {
                m_SynapseConnections.Remove(inputIndex);
            }
        }

        /// <summary>
        ///  Resets the current connections in preparation for new permanence adjustments.
        ///  </summary>
        public void ResetConnections()
        {
            m_SynapseConnections.Clear();
        }

        /// <summary>
        /// Returns the {@link Synapse} connected to the specified input bit index.
        /// </summary>
        /// <param name="inputIndex">the input vector connection's index.</param>
        /// <returns></returns>
        public Synapse GetSynapseForInput(int inputIndex)
        {
            return m_SynapsesBySourceIndex[inputIndex];
        }

        /// <summary>
        /// Returns an array of permanence values
        /// </summary>
        /// <returns></returns>
        public double[] GetSparsePermanences()
        {
            double[] retVal = new double[Size];
            int[] keys = GetSynapseKeys();// synapsesBySourceIndex.Keys;
            for (int x = 0, j = Size - 1; x < Size; x++, j--)
            {
                retVal[j] = this.m_SynapsesBySourceIndex[keys[Size-x-1]].Permanence;
            }

            return retVal;
        }

        private int[] GetSynapseKeys()
        {
            List<int> keys = new List<int>();

            foreach (var keyVal in m_SynapsesBySourceIndex)
            {
                keys.Add(keyVal.Key);
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Returns a dense array representing the potential pool permanences
        /// </summary>
        /// <param name="numInputs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note: Only called from tests for now...
        /// </remarks>
        public double[] GetDensePermanences(int numInputs)
        {
            double[] retVal = new double[numInputs];
           
            foreach (int inputIndex in m_SynapsesBySourceIndex.Keys)
            {
                retVal[inputIndex] = m_SynapsesBySourceIndex[inputIndex].Permanence;
            }
            return retVal;
        }

        /// <summary>
        /// Returns an array of input bit indexes indicating the index of the source. (input vector bit or lateral cell)
        /// </summary>
        /// <returns>the sparse array</returns>
        public int[] GetSparsePotential()
        {
            // Original requires reverse, because JAVA keys() methode returns keys reversed.
            //return ArrayUtils.reverse(synapsesBySourceIndex.Keys.Select(i => i).ToArray());
            return m_SynapsesBySourceIndex.Keys.Select(i => i).ToArray();
        }

        /// <summary>
        /// Returns a dense binary array containing 1's where the input bits are part of this pool.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/></param>
        /// <returns>dense binary array of member inputs</returns>
        public int[] GetDensePotential(Connections c)
        {
            List<int> newArr = new List<int>();

            for (int i = 0; i < c.HtmConfig.NumInputs; i++)
            {
                newArr.Add(m_SynapsesBySourceIndex.ContainsKey(i) ? 1 : 0);
                //    }
                //return IntStream.range(0, c.getNumInputs())
                //    .map(i->synapsesBySourceIndex.containsKey(i) ? 1 : 0)
                //    .toArray();
            }
            return newArr.ToArray();
        }

        /// <summary>
        /// Returns an binary array whose length is equal to the number of inputs; and where 1's are set in the indexes of this pool's assigned bits.
        /// </summary>
        /// <returns>the sparse array</returns>
        public int[] GetDenseConnected()
        {
            List<int> newArr = new List<int>();

            for (int i = 0; i < this.NumInputs; i++)
            {
                newArr.Add(m_SynapseConnections.Contains(i) ? 1 : 0);
              
            }
            return newArr.ToArray();

            //return IntStream.range(0, c.getNumInputs())
            //    .map(i->synapseConnections.contains(i) ? 1 : 0)
            //    .toArray();
        }

        /**
         * Destroys any references this {@code Pool} maintains on behalf
         * of the specified {@link Synapse}
         * 
         * @param synapse
         */
        //public void destroySynapse(Synapse synapse)
        //{
        //    synapseConnections.Remove(synapse.getInputIndex());
        //    synapsesBySourceIndex.Remove(synapse.getInputIndex());
        //    if (synapse.getSegment() is DistalDendrite)
        //    {
        //        destroy();
        //    }
        //}

        /// <summary>
        /// Clears the state of this <see cref="Pool"/>.
        /// </summary>
        public void Destroy()
        {
            m_SynapseConnections.Clear();
            m_SynapsesBySourceIndex.Clear();
            m_SynapseConnections = null;
            m_SynapsesBySourceIndex = null;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */
        //@Override
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + Size;
            result = prime * result + ((m_SynapseConnections == null) ? 0 : m_SynapseConnections.ToString().GetHashCode());
            result = prime * result + ((m_SynapsesBySourceIndex == null) ? 0 : m_SynapsesBySourceIndex.ToString().GetHashCode());
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (typeof(Pool) != obj.GetType())
                return false;
            Pool other = (Pool)obj;
            if (Size != other.Size)
                return false;
            if (m_SynapseConnections == null)
            {
                if (other.m_SynapseConnections != null)
                    return false;
            }

            // bool hasAll = matrix.getSparseIndices().All(itm2 => sparseSet.Contains(itm2));
            // return hasAll;
            else if(!other.m_SynapseConnections.All(itm2 => m_SynapseConnections.Contains(itm2)) ||
                !m_SynapseConnections.All(itm2 => other.m_SynapseConnections.Contains(itm2)))
            //else if ((!synapseConnections.containsAll(other.synapseConnections) ||
            //  !other.synapseConnections.containsAll(synapseConnections)))
                return false;
            if (m_SynapsesBySourceIndex == null)
            {
                if (other.m_SynapsesBySourceIndex != null)
                    return false;
            }
            else if (!m_SynapsesBySourceIndex.ToString().Equals(other.m_SynapsesBySourceIndex.ToString()))
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"Conns={this.m_SynapseConnections.Count} - ConnsBySrc= {this.m_SynapsesBySourceIndex.Count}";
        }
    }
}
