using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace NeoCortexApi.Entities
{


    /**
     * Convenience container for "bound" {@link Synapse} values
     * which can be dereferenced from both a Synapse and the 
     * {@link Connections} object. All Synapses will have a reference
     * to a {@code Pool} to retrieve relevant values. In addition, that
     * same pool can be referenced from the Connections object externally
     * which will update the Synapse's internal reference.
     * 
     * @author David Ray
     * @see Synapse
     * @see Connections
     */
    public class Pool
    {
        /** keep it simple */
        private static readonly long serialVersionUID = 1L;

        int size;

        /** Allows fast removal of connected synapse indexes. List of connected synapses. These
         are synapses with permanence value greather than permanence connected threshold. See synPermConnected*/

        ///
        private HashSet<int> synapseConnections = new HashSet<int>();
        /** 
         * Indexed according to the source Input Vector Bit (for ProximalDendrites),
         * and source cell (for DistalDendrites).
         */
        private Dictionary<int, Synapse> synapsesBySourceIndex = new Dictionary<int, Synapse>();

        public Pool(int size)
        {
            this.size = size;
        }

        /**
         * Returns the permanence value for the {@link Synapse} specified.
         * 
         * @param s	the Synapse
         * @return	the permanence
         */
        public double getPermanence(Synapse s)
        {
            return synapsesBySourceIndex[s.getInputIndex()].getPermanence();
        }

        /**
         * Sets the specified  permanence value for the specified {@link Synapse}
         * @param s
         * @param permanence
         */
        public void setPermanence(Connections c, Synapse s, double permanence)
        {
            s.setPermanence(c, permanence);
        }

        /**
         * Updates this {@code Pool}'s store of permanences for the specified {@link Synapse}
         * @param c				the connections memory
         * @param s				the synapse who's permanence is recorded
         * @param permanence	the permanence value to record
         */
        public void updatePool(Connections c, Synapse s, double permanence)
        {
            int inputIndex = s.getInputIndex();
            if (synapsesBySourceIndex.ContainsKey(inputIndex) == false)
            {
                synapsesBySourceIndex.Add(inputIndex, s);
            }
            if (permanence >= c.getSynPermConnected())
            {
                synapseConnections.Add(inputIndex);
            }
            else
            {
                synapseConnections.Remove(inputIndex);
            }
        }

        /**
         * Resets the current connections in preparation for new permanence
         * adjustments.
         */
        public void resetConnections()
        {
            synapseConnections.Clear();
        }

        /**
         * Returns the {@link Synapse} connected to the specified input bit
         * index.
         * 
         * @param inputIndex	the input vector connection's index.
         * @return
         */
        public Synapse getSynapseWithInput(int inputIndex)
        {
            return synapsesBySourceIndex[inputIndex];
        }

        /**
         * Returns an array of permanence values
         * @return
         */
        public double[] getSparsePermanences()
        {
            double[] retVal = new double[size];
            int[] keys = getSynapseKeys();// synapsesBySourceIndex.Keys;
            for (int x = 0, j = size - 1; x < size; x++, j--)
            {
                retVal[j] = synapsesBySourceIndex[keys[size-x-1]].getPermanence();
            }

            //for (int x = size-1, j = 0; x >= 0; x--, j++)
            //{
            //    retVal[j] = synapsesBySourceIndex[x].getPermanence();
            //}

            return retVal;
        }

        private int[] getSynapseKeys()
        {
            List<int> keys = new List<int>();

            foreach (var keyVal in synapsesBySourceIndex)
            {
                keys.Add(keyVal.Key);
            }

            return keys.ToArray();
        }


        /**
         * Returns a dense array representing the potential pool permanences
         * 
         * Note: Only called from tests for now...
         * @param c
         * @return
         */
        public double[] getDensePermanences(Connections c)
        {
            double[] retVal = new double[c.getNumInputs()];
            // int[] keys = synapsesBySourceIndex.keys();
            foreach (int inputIndex in synapsesBySourceIndex.Keys)
            {
                retVal[inputIndex] = synapsesBySourceIndex[inputIndex].getPermanence();
            }
            return retVal;
        }

        /**
         * Returns an array of input bit indexes indicating the index of the source. 
         * (input vector bit or lateral cell)
         * @return the sparse array
         */
        public int[] getSparsePotential()
        {
            // Original requires reverse, because JAVA keys() methode returns keys reversed.
            //return ArrayUtils.reverse(synapsesBySourceIndex.Keys.Select(i => i).ToArray());
            return synapsesBySourceIndex.Keys.Select(i => i).ToArray();
        }

        /**
         * Returns a dense binary array containing 1's where the input bits are part
         * of this pool.
         * @param c     the {@link Connections}
         * @return  dense binary array of member inputs
         */
        public int[] getDensePotential(Connections c)
        {
            List<int> newArr = new List<int>();

            for (int i = 0; i < c.getNumInputs(); i++)
            {
                newArr.Add(synapsesBySourceIndex.ContainsKey(i) ? 1 : 0);
                //    }
                //return IntStream.range(0, c.getNumInputs())
                //    .map(i->synapsesBySourceIndex.containsKey(i) ? 1 : 0)
                //    .toArray();
            }
            return newArr.ToArray();
        }

        /**
         * Returns an binary array whose length is equal to the number of inputs;
         * and where 1's are set in the indexes of this pool's assigned bits.
         * 
         * @param   c   {@link Connections}
         * @return the sparse array
         */
        public int[] getDenseConnected(Connections c)
        {
            List<int> newArr = new List<int>();

            for (int i = 0; i < c.getNumInputs(); i++)
            {
                newArr.Add(synapseConnections.Contains(i) ? 1 : 0);
              
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
        public void destroySynapse(Synapse synapse)
        {
            synapseConnections.Remove(synapse.getInputIndex());
            synapsesBySourceIndex.Remove(synapse.getInputIndex());
            if (synapse.getSegment() is DistalDendrite)
            {
                destroy();
            }
        }

        /**
         * Clears the state of this {@code Pool}
         */
        public void destroy()
        {
            synapseConnections.Clear();
            synapsesBySourceIndex.Clear();
            synapseConnections = null;
            synapsesBySourceIndex = null;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */
        //@Override
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + size;
            result = prime * result + ((synapseConnections == null) ? 0 : synapseConnections.ToString().GetHashCode());
            result = prime * result + ((synapsesBySourceIndex == null) ? 0 : synapsesBySourceIndex.ToString().GetHashCode());
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */
        //@Override
        public override bool Equals(Object obj)
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
            if (synapseConnections == null)
            {
                if (other.synapseConnections != null)
                    return false;
            }

            // bool hasAll = matrix.getSparseIndices().All(itm2 => sparseSet.Contains(itm2));
            // return hasAll;
            else if(!other.synapseConnections.All(itm2 => synapseConnections.Contains(itm2)) ||
                !synapseConnections.All(itm2 => other.synapseConnections.Contains(itm2)))
            //else if ((!synapseConnections.containsAll(other.synapseConnections) ||
            //  !other.synapseConnections.containsAll(synapseConnections)))
                return false;
            if (synapsesBySourceIndex == null)
            {
                if (other.synapsesBySourceIndex != null)
                    return false;
            }
            else if (!synapsesBySourceIndex.ToString().Equals(other.synapsesBySourceIndex.ToString()))
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"Conns={this.synapseConnections.Count} - ConnsBySrc= {this.synapsesBySourceIndex.Count}";
        }
    }
}
