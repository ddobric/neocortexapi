using NeoCortexApi.DistributedComputeLib;
using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi.Entities;
using Akka.Actor;

namespace NeoCortexApi.DistributedCompute
{
    /// <summary>
    /// Acts as distributed dictionary of SparseObjectMatrix.
    /// </summary>
    public class HtmSparseIntDictionary<T> : AkkaDistributedDictionaryBase<int, T>
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
        protected override int GetPartitionNodeIndexFromKey(int key)
        {
            return GetPlacementSlotForElement(this.Config.Nodes.Count, NumColumns, key);
        }

        /// <summary>
        ///  Creates maps of partitions.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="numElements"></param>
        /// <returns></returns>
        public override  List<(int nodeIndx, string nodeUrl, int partitionIndx, int minKey, int maxKey, IActorRef actorRef)> GetPartitionMap()
        {
            return GetPartitionMap(this.Config.Nodes, NumColumns, this.Config.PartitionsPerNode);
        }

        /// <summary>
        /// Creates map of partitions.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="numElements"></param>
        /// <param name="numPartitionsPerNode"></param>
        /// <returns></returns>
        public static List<(int nodeIndx, string nodeUrl, int partitionIndx, int minKey, int maxKey, IActorRef actorRef)> GetPartitionMap(List<string> nodes, int numElements, int numPartitionsPerNode)
        {
            List<(int nodeIndx, string nodeUrl, int partitionIndx, int minKey, int maxKey, IActorRef actorRef)> map = new List<(int nodeIndx, string nodeUrl, int partitionIndx, int minKey, int maxKey, IActorRef actorRef)>();

            int numOfElementsPerNode = (numElements % nodes.Count) == 0 ? numElements / nodes.Count : (int)((float)numElements / (float)nodes.Count + 1.0);

            int numOfElementsPerPartition = numOfElementsPerNode % numPartitionsPerNode == 0 ? numOfElementsPerNode / numPartitionsPerNode :  (int)(1.0 + (float)numOfElementsPerNode / (float)numPartitionsPerNode);

            int capacity = nodes.Count * numOfElementsPerPartition * numPartitionsPerNode;

            int globalPartIndx = 0;

            for (int nodIndx = 0; nodIndx < nodes.Count; nodIndx++)
            {
                for (int partIndx = 0; partIndx < numPartitionsPerNode; partIndx++, globalPartIndx++)
                {
                    var min = numOfElementsPerPartition * globalPartIndx;
                    var max = numOfElementsPerPartition * (globalPartIndx + 1) - 1;
                    map.Add((nodIndx, nodes[nodIndx], globalPartIndx, min, max, null));
                }
            }           

            return map;
        }

        //private static string getPartitionId(int nodeIndex, int key)
        //{
        //    return $"{nodeIndex}/key";
        //}

        public static (int nodeIndx, string nodeUrl, int partitionIndx, int minKey, int maxKey, IActorRef actorRef)
            GetPlacementSlotForElement(List<(int nodeIndx, string nodeUrl, int partitionIndx, int minKey, int maxKey, IActorRef actorRef)> map, int key)
        {
            foreach (var placement in map)
            {
                if (placement.minKey <= key && key <= placement.maxKey)
                    return placement;
            }

            throw new KeyNotFoundException($"The specified key {key} cannot be located in the partition map.");
        }

        public static int GetPlacementSlotForElement(int slots, int numElements, int key)
        {
            int numOfElemementsPerNode = numElements % slots == 0 ? numElements / slots : (int)((float)numElements / (float)slots + 1.0);

            int targetNode = key / numOfElemementsPerNode;

            return targetNode;
        }


        /// <summary>
        /// Gets the list of partitions (nodes) with assotiated keys.
        /// Key is assotiated to partition if it is hosted at the partition node.
        /// </summary>
        /// <returns></returns>
        public override IDictionary<int, List<int>> GetPartitions()
        {
            Dictionary<int, List<int>> partitions = new Dictionary<int, List<int>>();

            for (int key = 0; key < this.NumColumns; key++)
            {
                int node = GetPlacementSlotForElement(this.Nodes, this.NumColumns, key);
                if (!partitions.ContainsKey(node))
                {
                    partitions.Add(node, new List<int>());
                }

                partitions[node].Add(key);
            }

            return partitions;
        }

        private int? numColumns = null;

        private int NumColumns
        {
            get
            {
                if (numColumns == null)
                {
                    numColumns = 1;

                    foreach (var dimCols in this.Config.HtmActorConfig.ColumnDimensions)
                    {
                        numColumns *= dimCols;
                    }
                }

                return this.numColumns.Value;
            }
        }
    }



    public class HtmSparseIntDictionaryConfig : AkkaDistributedDictConfig
    {


    }
}
