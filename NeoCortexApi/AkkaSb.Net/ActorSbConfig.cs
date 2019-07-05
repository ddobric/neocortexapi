using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaSb.Net
{
    public class ActorSbConfig 
    {
        public string ActorSystemName{ get; set; }

        public string TblStoragePersistenConnStr { get; set; }

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

        /// <summary>
        /// Connection timeout of Ask() method.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Number partitions, which will be concurrentlly dispatched for calculation.
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// 
        /// </summary>
        public int NumOfElementsPerPartition { get; set; }
    }

    //public class NodeConfig
    //{
    //    public string RequestMsgQueue { get; set; }

    //    public string ReplyMsgReceiveQueue { get; set; }
    //}
}
