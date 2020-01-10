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
        /// Messages sent to invoke operations on actors. ActorSystem registers a session receivers
        /// on this topic.
        /// </summary>
        public string RequestMsgTopic { get; set; }

        /// <summary>
        /// The name of subscription, which will receive messages on topic.
        /// </summary>
        public string RequestSubscriptionName { get; set; } = "default";

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
        public int NumOfElementsPerPartition { get; set; } = -1;

        /// <summary>
        /// Should be uniformly distributed across nodes. It cannot be less than number of nodes.
        /// </summary>
        public int NumOfPartitions { get; set; } = -1;

        /// <summary>
        /// List of nodes in Actor SB cluster. ENtries in the list defines names of SB subscriptions.
        /// Every node in this list connects to its subscription. RequestSubscriptionName is one of entries in this list.
        /// If the list is empty system assumes that a single 'default' subscription exists.
        /// In this case all nodes in cluster connects to same subscription.
        /// </summary>
        public List<string> Nodes { get; set; }
    }
}
