using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    public class AkkaDistributedDictConfig
    {
        public TimeSpan ConnectionTimout { get; set; } = TimeSpan.FromMinutes(1);

        public List<string> Nodes { get; set; }
    }
}
