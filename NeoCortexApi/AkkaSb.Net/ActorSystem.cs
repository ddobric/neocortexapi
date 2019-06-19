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
        internal Dictionary<string, ClientPair> RemoteQueueClients = new Dictionary<string, ClientPair>();

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
            var pair = this.RemoteQueueClients.FirstOrDefault(i=>i.Key == remoteNodeName);
            if (pair.Key == remoteNodeName)
            {
                actorInst.ClientPair = pair.Value;
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

        #endregion
    }
}
