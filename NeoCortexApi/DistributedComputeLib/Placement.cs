using Akka.Actor;
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

        public IActorRef ActorRef { get; set; }
    }
}
