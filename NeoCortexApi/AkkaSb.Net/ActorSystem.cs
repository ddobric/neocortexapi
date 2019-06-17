using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


namespace AkkaSb.Net
{
    public class ActorSystem
    {
        protected class ClientPair
        {
            public QueueClient ReceiverClient { get; set; }

            public QueueClient SenderClient { get; set; }
        }
        
        protected Dictionary<string, ClientPair> RemoteQueueClients = new Dictionary<string, ClientPair>();

        protected SessionClient SessionClient;

        private Dictionary<string, ActorBase> actorMap = new Dictionary<string, ActorBase>();

        public ActorSystem(AkkaSbConfig config)
        {
            this.SessionClient = new SessionClient(config.SbConnStr, config.LocalNode.ReceiveQueue,
                retryPolicy: createRetryPolicy(),
                 receiveMode: ReceiveMode.PeekLock);

            foreach (var node in config.RemoteNodes)
            {
                ClientPair pair = new ClientPair();

                pair.SenderClient = new QueueClient(config.SbConnStr, node.Value.SendQueue,
                    retryPolicy: createRetryPolicy(),
                     receiveMode: ReceiveMode.PeekLock);

                RemoteQueueClients.Add(node.Key, pair);
            }        
        }

        public async Task CreateActor(ActorBase actorInst, string remoteNodeName)
        {
            var node = this.RemoteQueueClients.FirstOrDefault(i=>i.Key == remoteNodeName).Key;
            if (node == remoteNodeName)
            {

            }
            else
            {
                throw new ArgumentException("Specified remote node is not defined!");
            }
        }



        public async Task Start(CancellationToken cancelToken)
        {
            // 
            while (!cancelToken.IsCancellationRequested)
            {
                var session = await this.SessionClient.AcceptMessageSessionAsync();
                ActorBase actor;
                if (!actorMap.ContainsKey(session.SessionId))
                {
                    Type tp = Type.GetType(session.SessionId);
                    if (tp == null)
                        throw new ArgumentException($"Cannot find type '{session.SessionId}'");

                    actor = Activator.CreateInstance(tp) as ActorBase;
                    actorMap.Add(session.SessionId, actor);
                }

                var msg = await session.ReceiveAsync();
                if (msg != null)
                {

                }
            }
        }

        #region Private Methods

        private RetryPolicy createRetryPolicy()
        {
            return new RetryExponential(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 5);
        }

        private Message createMessage(ActorBase actorInst, string remoteUrl, string actorName)
        {
            Message msg = new Message(serializeMsg(actorInst));

            return msg;
        }

        private byte[] serializeMsg(ActorBase actorInst)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.TypeNameHandling = TypeNameHandling.All;
            var strObj = JsonConvert.SerializeObject(actorInst, sett);

            return UTF8Encoding.UTF8.GetBytes(strObj);
        }
        #endregion
    }
}
