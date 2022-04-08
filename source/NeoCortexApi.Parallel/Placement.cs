// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.DistributedComputeLib
{
    public class Placement<TKey>
    {
        public int NodeIndx { get; set; }

        public string NodePath { get; set; }

        public int PartitionIndx { get; set; }

        public TKey MinKey { get; set; }

        public TKey MaxKey { get; set; }

        public object ActorRef { get; set; }

        public override string ToString()
        {
            return $"Partition: {PartitionIndx}, Node: {NodePath}, MinKey:{MinKey}, MaxKey: {MaxKey} TotalKeys: {(int)(object)MaxKey - (int)(object)MinKey + 1}";
        }
    }
}
