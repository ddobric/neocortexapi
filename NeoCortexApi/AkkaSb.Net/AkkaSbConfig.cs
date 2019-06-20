using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaSb.Net
{
    public class AkkaSbConfig
    {
        public string SbConnStr { get; set; }

        public NodeConfig LocalNode { get; set; }

        public Dictionary<string, NodeConfig> RemoteNodes { get; set; } = new Dictionary<string, NodeConfig>();
    }

    public class NodeConfig
    {
        public string SendQueue { get; set; }

        public string ReceiveQueue { get; set; }
    }
}
