using Akka.Actor;
using AkkaSb.Net;
using System;
using System.Collections.Generic;
using System.Text;

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
            return $"Partition: {PartitionIndx}, Node: {NodePath}, MinKey:{MinKey}, MaxKey: {MaxKey} TotalKeys: {(int)(object)MaxKey-(int)(object)MinKey + 1}" ;
        }
    }
}
