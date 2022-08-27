// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Convenience container for "bound" <see cref="Synapse"/> values which can be dereferenced from both a Synapse and the <see cref="Connections"/> object.
    /// All Synapses will have a reference to a <see cref="Pool"/> to retrieve relevant values. In addition, same pool can be referenced from the Connections 
    /// object externally which will update the Synapse's internal reference.
    /// </summary>
    public class Pool : ISerializable
    {
        private int size;

        /// <summary>
        /// Number of inut neuron cells.
        /// </summary>
        private int NumInputs { get; set; }

        /// <summary>
        /// Allows fast removal of connected synapse indexes. List of connected synapses. 
        /// These are synapses with permanence value greather than permanence connected threshold.
        /// See synPermConnected.
        /// </summary>      
        private List<int> m_SynapseConnections { get; set; } = new List<int>();

        /// <summary>
        /// List of potential synapses.Indexed according to the source Input Vector Bit (for ProximalDendrites), and source cell (for DistalDendrites).
        /// </summary>
        internal Dictionary<int, Synapse> m_SynapsesBySourceIndex { get; set; } = new Dictionary<int, Synapse>();

        /// <summary>
        /// Default constructor used by deserializer.
        /// </summary>
        public Pool()
        {

        }

        /// <summary>
        /// Creates the instance of the pool.
        /// </summary>
        /// <param name="size">Number of connected input neurons. These neurons define RF fo the segment, which owns this pool.</param>
        /// <param name="numInputs">Total number of input neurons.</param>
        public Pool(int size, int numInputs)
        {
            this.NumInputs = numInputs;
            this.size = size;
        }

        /// <summary>
        /// Updates this <see cref="Pool"/>'s store of permanences for the specified <see cref="Synapse"/>.
        /// </summary>
        /// <param name="synPermConnected">The synapse is added to the list of connected synapses if the permanence value is greather than this value.</param>
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
        /// Returns an array of permanence values.
        /// </summary>
        /// <returns></returns>
        public double[] GetSparsePermanences()
        {
            double[] retVal = new double[size];
            int[] keys = GetSynapseKeys();// synapsesBySourceIndex.Keys;
            for (int x = 0, j = size - 1; x < size; x++, j--)
            {
                retVal[j] = this.m_SynapsesBySourceIndex[keys[size - x - 1]].Permanence;
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
        /// <param name="numInputs">NUmber of input neurons used for spatial training.</param>
        /// <returns>Permanences of all synapses connected to inut neurons.</returns>
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
        /// Returns an array of indexes of input neurons connected to this pool. 
        /// </summary>
        /// <returns>the sparse array</returns>
        public int[] GetSparsePotential()
        {
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
        }


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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + size;
            result = prime * result + ((m_SynapseConnections == null) ? 0 : m_SynapseConnections.GetHashCode());
            result = prime * result + ((m_SynapsesBySourceIndex == null) ? 0 : m_SynapsesBySourceIndex.GetHashCode());
            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (typeof(Pool) != obj.GetType())
                return false;
            Pool other = (Pool)obj;
            if (size != other.size)
                return false;
            if (NumInputs != other.NumInputs)
                return false;
            if (m_SynapseConnections == null)
            {
                if (other.m_SynapseConnections != null)
                    return false;
            }

            // bool hasAll = matrix.getSparseIndices().All(itm2 => sparseSet.Contains(itm2));
            // return hasAll;
            else if (!other.m_SynapseConnections.All(itm2 => m_SynapseConnections.Contains(itm2)) ||
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



        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Conns={this.m_SynapseConnections.Count} - ConnsBySrc= {this.m_SynapsesBySourceIndex.Count}";
        }

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Pool), writer);

            ser.SerializeValue(this.size, writer);
            ser.SerializeValue(this.NumInputs, writer);
            ser.SerializeValue(this.m_SynapseConnections, writer);
            ser.SerializeValue(this.m_SynapsesBySourceIndex, writer);

            ser.SerializeEnd(nameof(Pool), writer);

        }

        public static Pool Deserialize(StreamReader sr)
        {
            Pool pool = new Pool();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(Pool)) || (data.ToCharArray()[0] == HtmSerializer2.ElementsDelimiter && data.ToCharArray()[1] == HtmSerializer2.ParameterDelimiter))
                {
                    continue;
                }
                else if (data == ser.ReadEnd(nameof(Pool)))
                {
                    break;
                }

                else if (data.Contains(HtmSerializer2.KeyValueDelimiter))
                {
                    int val = ser.ReadKeyISValue(data);
                    data = sr.ReadLine();
                    pool.m_SynapsesBySourceIndex.Add(val, Synapse.Deserialize(sr));

                }
                else
                {
                    string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    pool.size = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    pool.NumInputs = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    pool.m_SynapseConnections = ser.ReadListInt(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return pool;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var ignoreMembers = new List<string>
            {
                nameof(Pool.size),
                nameof(Pool.m_SynapsesBySourceIndex)
            };
            if (obj is Pool pool)
            {

                HtmSerializer2.SerializeObject(obj, name, sw, ignoreMembers);

                var synapses = pool.m_SynapsesBySourceIndex.Values.ToList();
                HtmSerializer2.Serialize(synapses, "synapses", sw, null, ignoreMembers: new List<string> { nameof(Synapse.SegmentIndex) });
            }
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            return HtmSerializer2.DeserializeObject<Pool>(sr, name, new List<string> { "synapses" }, (pool, propName) =>
            {
                if (propName == "synapses")
                {
                    var synapses = HtmSerializer2.Deserialize<List<Synapse>>(sr, propName);
                    pool.m_SynapsesBySourceIndex = synapses.ToDictionary(s => s.InputIndex);
                    pool.size = synapses.Count;
                }
            });
        }
        #endregion
    }
}
