using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaSb.Net
{
    public class AkkaSbConfig
    {
        public string SbConnStr { get; set; }

        /// <summary>
        /// Queue used to receive reply messages send as responses from actor operations.
        /// Sender of th emessage appends this queue name, so the receiver knows where to send reply the message.
        /// </summary>
        public string ReplyMsgQueue { get; set; }

        /// <summary>
        /// Messages sent to invoke operations on actors. ActorSystem registers a session receiver
        /// on this queue.
        /// </summary>
        public string RequestMsgQueue { get; set; }

        //public NodeConfig LocalNode { get; set; }

        // public Dictionary<string, NodeConfig> RemoteNodes { get; set; } = new Dictionary<string, NodeConfig>();
    }

    //public class NodeConfig
    //{
    //    public string RequestMsgQueue { get; set; }

    //    public string ReplyMsgReceiveQueue { get; set; }
    //}
}
