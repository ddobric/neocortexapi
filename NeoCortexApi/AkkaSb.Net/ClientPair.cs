using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaSb.Net
{
    public class ClientPair
    {
        public QueueClient ReceiverClient { get; set; }

        public QueueClient SenderClient { get; set; }
    }
}
