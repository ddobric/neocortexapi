// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
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
    }
}
