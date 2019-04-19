using NeoCortexApi.DistributedComputeLib;
using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi.Entities;

namespace NeoCortexApi.DistributedCompute
{
    /// <summary>
    /// Acts as distributed dictionary of SparseObjectMatrix.
    /// </summary>
    public class HtmSparseIntDictionary : AkkaDistributedDictionaryBase<int, Column>
    {
        public HtmSparseIntDictionary(HtmSparseIntDictionaryConfig config) : base(config)
        {
        }


        /// <summary>
        /// Nodes = 2, Cols = 7 => Node 0: {0,1,2,3}, Node 1: {4,5,6}
        /// Nodes = 3, Cols = 7 => Node 0: {0,1,2}, Node 1: {3,4}, Node 1: {5,6}
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override int GetNodeIndexFromKey(int key)
        {
            int cols = (this.Config as HtmSparseIntDictionaryConfig).NumColumns;

            return GetNode(this.Config.Nodes.Count, cols, key);
        }

        public static int GetNode(int nodes, int elements, int placingElement)
        {
            int roundedElements = elements;

            while (true)
            {
                if (roundedElements % nodes == 0)
                {
                    break;
                }
                else
                    roundedElements++;
            }

            int numOfElemementsPerNode = roundedElements / nodes;

            int targetNode = placingElement / numOfElemementsPerNode;

            return targetNode;
        }
    }

   

    public class HtmSparseIntDictionaryConfig : AkkaDistributedDictConfig
    {
        public int NumColumns { get; set; }
    }
}
