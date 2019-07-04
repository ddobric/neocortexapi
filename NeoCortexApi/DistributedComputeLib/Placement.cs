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

        public string NodeUrl { get; set; }

        public int PartitionIndx { get; set; }

        public TKey MinKey { get; set; }

        public TKey MaxKey { get; set; }

        public object ActorRef { get; set; }

        public override string ToString()
        {
            return $"Partition: {PartitionIndx}, MinKey:{MinKey}, MaxKey: {MaxKey} TotalKeys: {(int)(object)MinKey-(int)(object)MaxKey + 1}" ;
        }
    }
}
