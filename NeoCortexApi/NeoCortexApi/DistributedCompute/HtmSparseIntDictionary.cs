// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.DistributedComputeLib;
using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi.Entities;
using System.Linq;
using AkkaSb.Net;

namespace NeoCortexApi.DistributedCompute
{
    /// <summary>
    /// Acts as distributed dictionary of SparseObjectMatrix.
    /// </summary>
    public class HtmSparseIntDictionary<T> : AkkaDistributedDictionaryBase<int, T>
    {
        public HtmSparseIntDictionary(object config) : base(config, null)
        {
        }

        /// <summary>
        /// Returns the actor reference for specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override IActorRef GetPartitionActorFromKey(int key)
        {
            return this.ActorMap.Where(p => p.MinKey <= key && p.MaxKey >= key).First().ActorRef as IActorRef;
        }

        /// <summary>
        ///  Creates maps of partitions.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="numElements"></param>
        /// <returns></returns>
        public override List<Placement<int>> CreatePartitionMap()
        {
            return CreatePartitionMap(this.Config.Nodes, NumColumns, this.Config.PartitionsPerNode);
        }

        /// <summary>
        /// Creates map of partitions.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="numElements"></param>
        /// <param name="numPartitionsPerNode"></param>
        /// <returns></returns>
        public static List<Placement<int>> CreatePartitionMap(List<string> nodes, int numElements, int numPartitionsPerNode)
        {
            List<Placement<int>> map = new List<Placement<int>>();

            int numOfElementsPerNode = (numElements % nodes.Count) == 0 ? numElements / nodes.Count : (int)((float)numElements / (float)nodes.Count + 1.0);

            int numOfElementsPerPartition = numOfElementsPerNode % numPartitionsPerNode == 0 ? numOfElementsPerNode / numPartitionsPerNode : (int)(1.0 + (float)numOfElementsPerNode / (float)numPartitionsPerNode);

            int capacity = nodes.Count * numOfElementsPerPartition * numPartitionsPerNode;

            int globalPartIndx = 0;

            for (int nodIndx = 0; nodIndx < nodes.Count; nodIndx++)
            {
                for (int partIndx = 0; partIndx < numPartitionsPerNode; partIndx++, globalPartIndx++)
                {
                    var min = numOfElementsPerPartition * globalPartIndx;
                    var maxPartEl = numOfElementsPerPartition * (globalPartIndx + 1) - 1;
                    var max = maxPartEl < numElements ? maxPartEl : numElements - 1;
                   
                    if (min >= numElements)
                        break;

                    map.Add(new Placement<int>() { NodeIndx = nodIndx, NodePath = nodes[nodIndx], PartitionIndx = globalPartIndx, MinKey = min, MaxKey = max, ActorRef = null });

                }
            }

            return map;
        }


        public static Placement<int> GetPlacementSlotForElement(List<Placement<int>> map, int key)
        {
            foreach (var placement in map)
            {
                if (placement.MinKey <= key && key <= placement.MaxKey)
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


        ///// <summary>
        ///// Gets the list of partitions (nodes) with assotiated keys.
        ///// Key is assotiated to partition if it is hosted at the partition node.
        ///// </summary>
        ///// <returns></returns>
        //public override List<(int partId, int minKey, int maxKey)> GetPartitions()
        //{
        //    List<(int partId, int minKey, int maxKey)> map = new List<(int partId, int minKey, int maxKey)>();

        //    foreach (var part in this.ActorMap)
        //    {
        //        map.Add((part.PartitionIndx, part.MinKey, part.MaxKey));
        //    }

        //    return map;
        //}

        private int? numColumns = null;

        private int NumColumns
        {
            get
            {
                if (numColumns == null)
                {
                    numColumns = 1;

                    foreach (var dimCols in this.HtmConfig.ColumnModuleTopology.Dimensions)
                    {
                        numColumns *= dimCols;
                    }
                }

                return this.numColumns.Value;
            }
        }


        /// <summary>
        /// Groups keys by partitions (actors).
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public override Dictionary<IActorRef, List<KeyPair>> GetPartitionsForKeyset(ICollection<KeyPair> keyValuePairs)
        {
            Dictionary<IActorRef, List<KeyPair>> res = new Dictionary<IActorRef, List<KeyPair>>();

            foreach (var partition in this.ActorMap)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (partition.MinKey <= (int)pair.Key && partition.MaxKey >= (int)pair.Key)
                    {
                        if (res.ContainsKey(partition.ActorRef as IActorRef) == false)
                            res.Add(partition.ActorRef as IActorRef, new List<KeyPair>());

                        res[partition.ActorRef as IActorRef].Add(pair);
                    }
                }
            }

            return res;
        }

        public override Dictionary<IActorRef, List<KeyPair>> GetPartitionsByNode()
        {
            return GetPartitionsByNode(this.ActorMap);
        }

        public static Dictionary<IActorRef, List<KeyPair>> GetPartitionsByNode(List<Placement<int>> actorMap)
        {
            var groupedByNode = actorMap.GroupBy(i => i.ActorRef);
            foreach (var node in groupedByNode)
            {
                foreach (var item in node)
                {

                }
            }
            return null;
        }
    }


    // TODO put class to another file?
    public class HtmSparseIntDictionaryConfig : AkkaDistributedDictConfig
    {


    }
}
