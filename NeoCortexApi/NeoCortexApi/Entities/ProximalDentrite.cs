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

        /**
         * 
         * @param index     this {@code ProximalDendrite}'s index.
         */
        public ProximalDendrite(int index) : base(index)
        {

        }
               

        /// <summary>
        /// Creates object, which represents the pool of input neurons, which are connected.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="inputIndexes">Indexes of connected input bits.</param>
        /// <returns></returns>
        public Pool createPool(Connections c, int[] inputIndexes)
        {
            this.RFPool = new Pool(inputIndexes.Length);
            for (int i = 0; i < inputIndexes.Length; i++)
            {
                int synCount = c.getProximalSynapseCount();
                var synapse = createSynapse(c, c.getSynapses(this), null, this.RFPool, synCount, inputIndexes[i]);
                synapse.setPermanence(c.getSynPermConnected(), 0);
                c.setProximalSynapseCount(synCount + 1);
            }
            return RFPool;
        }

        public void clearSynapses(Connections c)
        {
            c.getSynapses(this).Clear();
        }


        public void setPermanences2(Connections c, AbstractSparseBinaryMatrix coonCounts, double[] perms)
        {
            RFPool.resetConnections();
            coonCounts.clearStatistics(index);
            List<Synapse> synapses = c.getSynapses(this);

            foreach (Synapse s in synapses)
            {
                int indx = s.getInputIndex();

                s.setPermanence(c.getSynPermConnected(), perms[indx]);

                if (perms[indx] >= c.getSynPermConnected())
                {
                   // connCounts.set(1, index, s.getInputIndex());
                }
            }
        }

        /**
         * Sets the permanences for each {@link Synapse}. The number of synapses
         * is set by the potentialPct variable which determines the number of input
         * bits a given column will be "attached" to which is the same number as the
         * number of {@link Synapse}s
         * 
         * @param c			the {@link Connections} memory
         * @param perms		the floating point degree of connectedness
         */
        public void setPermanences(Connections c, double[] perms)
        {
            var connCounts = c.getConnectedCounts();

            this.RFPool.resetConnections();

            connCounts.clearStatistics(index);

            List<Synapse> synapses = c.getSynapses(this);

            foreach (Synapse s in synapses)
            {
                int indx = s.getInputIndex();

                s.setPermanence(c.getSynPermConnected(), perms[indx]);

                if (perms[indx] >= c.getSynPermConnected())
                {
                    connCounts.set(1, index, s.getInputIndex());
                }
            }
        }

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
        public void setPermanences(Connections c, double[] perms, int[] inputIndexes)
        {
            RFPool.resetConnections();
            c.getConnectedCounts().clearStatistics(index);
            for (int i = 0; i < inputIndexes.Length; i++)
            {
                var synapse = RFPool.getSynapseWithInput(inputIndexes[i]);
                synapse.setPermanence(c.getSynPermConnected(), perms[i]);
                //RFPool.setPermanence(c, RFPool.getSynapseWithInput(inputIndexes[i]), perms[i]);
                if (perms[i] >= c.getSynPermConnected())
                {
                    c.getConnectedCounts().set(1, index, i);
                }
            }
        }

        /**
         * Sets the input vector synapse indexes which are connected (&gt;= synPermConnected)
         * @param c
         * @param connectedIndexes
         */
        public void setConnectedSynapsesForTest(Connections c, int[] connectedIndexes)
        {
            Pool pool = createPool(c, connectedIndexes);
            c.getPotentialPools().set(index, pool);
        }

        /**
         * Returns an array of synapse indexes as a dense binary array.
         * @param c
         * @return
         */
        public int[] getConnectedSynapsesDense(Connections c)
        {
            return c.getPotentialPools().get(index).getDenseConnected(c);
        }

        /**
         * Returns an sparse array of synapse indexes representing the connected bits.
         * @param c
         * @return
         */
        public int[] getConnectedSynapsesSparse(Connections c)
        {
            return c.getPotentialPools().get(index).getSparsePotential();
        }
    }
}
