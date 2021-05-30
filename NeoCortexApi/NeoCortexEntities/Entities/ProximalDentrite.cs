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
        /// The pool of synapses.
        /// </summary>
        public Pool RFPool {get;set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colIndx">The global index of the segment.</param>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="numInputs">Number of input neorn cells.</param>
        public ProximalDendrite(int colIndx, double synapsePermConnected, int numInputs) : base(colIndx, synapsePermConnected, numInputs)
        {

        }

        /// <summary>
        /// Default constructor used by deserializer.
        /// </summary>
        public ProximalDendrite()
        {

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
                    lst[i]=  this.Synapses[i].InputIndex;
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
                //RFPool.setPermanence(c, RFPool.getSynapseWithInput(inputIndexes[i]), perms[i]);
                if (perms[i] >= permConnThreshold)
                {
                    //c.getConnectedCounts().set(1, ParentColumnIndex, i);
                    connectedCounts.set(1, 0 /*ParentColumnIndex*/, i);
                }
            }
        }

        //public double SynPermConnected { get; set; }


        /**
         * Sets the input vector synapse indexes which are connected (&gt;= synPermConnected)
         * @param c
         * @param connectedIndexes
         */
        //public void setConnectedSynapsesForTest(Connections c, int[] connectedIndexes)
        //{
        //    //Pool pool = createPool(c, connectedIndexes);
        //    var pool = new Pool(connectedIndexes.Length, c.NumInputs);
        //    //c.getPotentialPools().set(index, pool);
        //    c.getPotentialPoolsOld().set(index, pool);
        //}

        /// <summary>
        /// Returns an array of synapse indexes as a dense binary array.
        /// </summary>
        /// <returns></returns>
        public int[] GetConnectedSynapsesDense()
        {
            return this.RFPool.GetDenseConnected();
            //return c.getPotentialPools().get(index).getDenseConnected(c);
        }

        /// <summary>
        /// Returns an sparse array of synapse indexes representing the connected bits.
        /// </summary>
        /// <returns></returns>
        public int[] GetConnectedSynapsesSparse()
        {
            return this.RFPool.GetSparsePotential();
            //return c.getPotentialPools().get(index).getSparsePotential();
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

            if (boxedIndex == null)
            {
                if (other.boxedIndex != null)
                    return false;
            }
            else if (!boxedIndex.Equals(other.boxedIndex))
                return false;

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

            if (this.RFPool != null)
            {
                ser.SerializeValue(this.SegmentIndex, writer);
                ser.SerializeValue(this.SynapsePermConnected, writer);
                ser.SerializeValue(this.NumInputs, writer);

                this.RFPool.Serialize(writer);
                
                this.boxedIndex.Serialize(writer);

                ser.SerializeValue(this.Synapses, writer);

                ser.SerializeValue(this.Synapses, writer);
            }

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
                else if (data == ser.ReadBegin(nameof(Integer)))
                {
                    proximal.boxedIndex = Integer.Deserialize(sr);
                }
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
