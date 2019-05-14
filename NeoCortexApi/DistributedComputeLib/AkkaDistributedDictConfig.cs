using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    /// <summary>
    /// Defines the confguration of Akka Cluster.
    /// </summary>
    public class AkkaDistributedDictConfig
    {
        /// <summary>
        /// Time to wait to connect to Akk Cluster.
        /// </summary>
        public TimeSpan ConnectionTimout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Address of all nodes in cluster.
        /// </summary>
        public List<string> Nodes { get; set; }

        /// <summary>
        /// Configuration used to initialize HTM actor.
        /// </summary>
        public ActorConfig ActorConfig { get; set; }
    }
}
