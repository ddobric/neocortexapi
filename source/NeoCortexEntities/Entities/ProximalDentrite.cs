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
    /// Defines th eproximal dentritte segment. Note the segment is used during SP compute operation.
    /// TM does not use this segment.
    /// </summary>
    public class ProximalDendrite : Segment
    {
        /// <summary>
        /// The pool of synapses in the receptive field.
        /// </summary>
        public Pool RFPool { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colIndx">The global index of the segment.</param>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="numInputs">Number of input neorn cells.</param>
        public ProximalDendrite(int colIndx, double synapsePermConnected, int numInputs) : base(colIndx, synapsePermConnected, numInputs)
        {

        }

        public ProximalDendrite()
        {

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
        public Synapse CreateSynapse(int index, int inputIndex)
        {
            Synapse synapse = new Synapse(this.SegmentIndex, index, inputIndex);
            this.Synapses.Add(synapse);
            return synapse;
        }

        

        /// <summary>
        /// Array of indicies of connected inputs. Defines RF. Sometimes also called 'Potential Pool'.
        /// </summary>
        public int[] ConnectedInputs
        {
            get
            {
                int[] lst = new int[this.Synapses.Count];
                for (int i = 0; i < lst.Length; i++)
                {
                    lst[i] = this.Synapses[i].InputIndex;
                }

                return lst;
            }
        }


        /// <summary>
        /// Clears all synapses on the segment.
        /// </summary>
        /// <param name="c"></param>
        public void ClearSynapses(Connections c)
        {
            this.Synapses.Clear();
        }



        /// <summary>
        /// Sets the permanences for each {@link Synapse} specified by the indexes passed in which identify the input vector indexes associated with the
        /// <see cref="Synapse"/>. The permanences passed in are understood to be in "sparse" format and therefore require the int array identify their 
        /// corresponding indexes.
        /// </summary>
        /// <param name="connectedCounts"></param>
        /// <param name="htmConfig"></param>
        /// <param name="perms">the floating point degree of connectedness</param>
        /// <param name="inputIndexes"></param>
        /// <remarks>
        /// Note: This is the "sparse" version of this method.
        /// </remarks>
        public void SetPermanences(AbstractSparseBinaryMatrix connectedCounts, HtmConfig htmConfig, double[] perms, int[] inputIndexes)
        {
            var permConnThreshold = htmConfig.SynPermConnected;

            RFPool.ResetConnections();
            // c.getConnectedCounts().clearStatistics(ParentColumnIndex);
            connectedCounts.ClearStatistics(0 /*this.ParentColumnIndex*/);
            for (int i = 0; i < inputIndexes.Length; i++)
            {
                var synapse = RFPool.GetSynapseForInput(inputIndexes[i]);
                synapse.Permanence = perms[i];

                if (perms[i] >= permConnThreshold)
                {
                    connectedCounts.set(1, 0 /*ParentColumnIndex*/, i);
                }
            }
        }

        /// <summary>
        /// Returns an array of synapse indexes as a dense binary array.
        /// </summary>
        /// <returns></returns>
        public int[] GetConnectedSynapsesDense()
        {
            return this.RFPool.GetDenseConnected();
        }

        /// <summary>
        /// Returns an array of indexes of input neurons connected to this pool. 
        /// </summary>
        /// <returns>Indexes of connected input neurons.</returns>
        public int[] GetConnectedSynapsesSparse()
        {
            return this.RFPool.GetSparsePotential();
        }

        public bool Equals(ProximalDendrite obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            ProximalDendrite other = (ProximalDendrite)obj;

            if (RFPool == null)
            {
                if (other.RFPool != null)
                    return false;
            }
            else if (!RFPool.Equals(other.RFPool))
                return false;

            if (SegmentIndex != other.SegmentIndex)
                return false;

            if (Synapses == null)
            {
                if (other.Synapses != null)
                    return false;
            }
            else if (!Synapses.SequenceEqual(other.Synapses))
                    return false;

            //if (boxedIndex == null)
            //{
            //    if (other.boxedIndex != null)
            //        return false;
            //}
            //else if (!boxedIndex.Equals(other.boxedIndex))
            //    return false;

            if (SynapsePermConnected != other.SynapsePermConnected)
                return false;
            if (NumInputs != other.NumInputs)
                return false;
            
            return true;
        }
        #region Serialization
        public override void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(ProximalDendrite), writer);

            ser.SerializeValue(this.SegmentIndex, writer);
            ser.SerializeValue(this.SynapsePermConnected, writer);
            ser.SerializeValue(this.NumInputs, writer);

            if (this.RFPool != null)
            {
                this.RFPool.Serialize(writer);
            }

            //if (this.boxedIndex != null)
            //{
            //    this.boxedIndex.Serialize(writer);
            //}
            ser.SerializeValue(this.Synapses, writer);
            
            ser.SerializeEnd(nameof(ProximalDendrite), writer);
        }

        public static ProximalDendrite Deserialize(StreamReader sr)
        {
            ProximalDendrite proximal = new ProximalDendrite();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(ProximalDendrite)) || (data.ToCharArray()[0] == HtmSerializer2.ElementsDelimiter && data.ToCharArray()[1] == HtmSerializer2.ParameterDelimiter) || data.ToCharArray()[1] == HtmSerializer2.ParameterDelimiter)
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(Pool)))
                {
                    proximal.RFPool = Pool.Deserialize(sr);
                }
                //else if (data == ser.ReadBegin(nameof(Integer)))
                //{
                //    proximal.boxedIndex = Integer.Deserialize(sr);
                //}
                else if (data == ser.ReadBegin(nameof(Synapse)))
                {
                    proximal.Synapses.Add(Synapse.Deserialize(sr));
                }
                else if (data == ser.ReadEnd(nameof(ProximalDendrite)))
                {
                    break;
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
                                    proximal.SegmentIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    proximal.SynapsePermConnected = ser.ReadDoubleValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    proximal.NumInputs = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return proximal;
        }

        #endregion
    }
}
