using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{

    /// <summary>
    /// Defines th eproximal dentritte segment.
    /// </summary>
    //[Serializable]
    public class ProximalDendrite : Segment
    {
        public Pool RFPool {get;set; }

        ///// <summary>
        ///// Permanence threshold value to declare synapse as connected.
        ///// </summary>
        //public double SynapsePermConnected { get; set; }


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
        /// Array of indicies of connected inputs. Defines RF.
        /// Sometimes also called 'Potential Pool'.
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
        /// Creates object, which represents the pool of input neurons, which are connected.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="inputIndexes">Indexes of connected input bits.</param>
        /// <returns></returns>
        //public Pool createPool(int numInputs, int currentProximalSynapseCount, int[] inputIndexes)
        //{
        //    this.RFPool = new Pool(inputIndexes.Length, numInputs);
        //    for (int i = 0; i < inputIndexes.Length; i++)
        //    {
        //        //var synapse = createSynapse(c, c.getSynapses(this), null, this.RFPool, synCount, inputIndexes[i]);
        //        var synapse = createSynapse(null, this.RFPool, currentProximalSynapseCount, inputIndexes[i]);
        //        synapse.setPermanence(this.SynapsePermConnected, 0);
        //        c.setProximalSynapseCount(currentProximalSynapseCount + 1);
        //    }
        //    return RFPool;
        //}

        public void ClearSynapses(Connections c)
        {
            this.Synapses.Clear();
            //c.getSynapses(this).Clear();
        }


        ///**
        // * Sets the permanences for each {@link Synapse}. The number of synapses
        // * is set by the potentialPct variable which determines the number of input
        // * bits a given column will be "attached" to which is the same number as the
        // * number of {@link Synapse}s
        // * 
        // * @param c			the {@link Connections} memory
        // * @param perms		the floating point degree of connectedness
        // */
        //public void setPermanencesOLD(Connections c, double[] perms)
        //{
        //    var connCounts = c.getConnectedCounts();

        //    this.RFPool.resetConnections();

        //    connCounts.clearStatistics(index);

        //    //List<Synapse> synapses = c.getSynapses(this);

        //    foreach (Synapse s in this.Synapses)
        //    {
        //        int indx = s.getInputIndex();

        //        s.setPermanence(c.getSynPermConnected(), perms[indx]);

        //        if (perms[indx] >= c.getSynPermConnected())
        //        {
        //            connCounts.set(1, index, s.getInputIndex());
        //        }
        //    }
        //}

        /**
         * Sets the permanences for each {@link Synapse} specified by the indexes
         * passed in which identify the input vector indexes associated with the
         * {@code Synapse}. The permanences passed in are understood to be in "sparse"
         * format and therefore require the int array identify their corresponding
         * indexes.
         * 
         * Note: This is the "sparse" version of this method.
         * 
         * @param c			the {@link Connections} memory
         * @param perms		the floating point degree of connectedness
         */
        public void setPermanences(AbstractSparseBinaryMatrix connectedCounts, HtmConfig htmConfig, double[] perms, int[] inputIndexes)
        {
            var permConnThreshold = htmConfig.SynPermConnected;

            RFPool.resetConnections();
            // c.getConnectedCounts().clearStatistics(ParentColumnIndex);
            connectedCounts.clearStatistics(0 /*this.ParentColumnIndex*/);
            for (int i = 0; i < inputIndexes.Length; i++)
            {
                var synapse = RFPool.GetSynapseForInput(inputIndexes[i]);
                synapse.setPermanence(htmConfig.SynPermConnected, perms[i]);
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

        /**
         * Returns an array of synapse indexes as a dense binary array.
         * @param c
         * @return
         */
        public int[] getConnectedSynapsesDense()
        {
            return this.RFPool.getDenseConnected();
            //return c.getPotentialPools().get(index).getDenseConnected(c);
        }

        /**
         * Returns an sparse array of synapse indexes representing the connected bits.
         * @param c
         * @return
         */
        public int[] getConnectedSynapsesSparse()
        {
            return this.RFPool.getSparsePotential();
            //return c.getPotentialPools().get(index).getSparsePotential();
        }
    }
}
