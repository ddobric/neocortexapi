using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AkkaSb.Net
{
    public class ActorSystem
    {
        private ConcurrentDictionary<string, Message> receivedMsgQueue = new ConcurrentDictionary<string, Message>();

        private TimeSpan MaxProcessingTimeOfMessage { get; set; }

        internal Dictionary<string, ClientPair> RemoteQueueClients = new Dictionary<string, ClientPair>();

        protected SessionClient SessionClient;

        private ConcurrentDictionary<string, ActorBase> actorMap = new ConcurrentDictionary<string, ActorBase>();

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

                pair.ReceiverClient = new QueueClient(config.SbConnStr, node.Value.ReceiveQueue,
                   retryPolicy: createRetryPolicy(),
                    receiveMode: ReceiveMode.PeekLock);


                // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {

                    // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                    // Set it according to how many messages the application wants to process in parallel.
                    MaxConcurrentCalls = 1,

                    MaxAutoRenewDuration = this.MaxProcessingTimeOfMessage,

                    // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                    // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                    AutoComplete = true
                };

                // Register the function that processes messages with reliable messaging.
                pair.ReceiverClient.RegisterMessageHandler(OnMessageReceivedAsync, messageHandlerOptions);

                RemoteQueueClients.Add(node.Key, pair);
            }
        }

        public ActorReference CreateActor<TActor>(ActorId id, string remoteNodeName) where TActor : ActorBase
        {
            var pair = this.RemoteQueueClients.FirstOrDefault(i => i.Key == remoteNodeName);
            if (pair.Key == remoteNodeName)
            {
                ActorReference actorRef = new ActorReference(typeof(TActor), id, pair.Value, receivedMsgQueue, this.rcvEvent, this.MaxProcessingTimeOfMessage);
                return actorRef;
            }
            else
            {
                throw new ArgumentException("Specified remote node is not defined!");
            }
        }

        public async Task Start(CancellationToken cancelToken)
        {
            Task[] tasks = new Task[2];

            tasks[0] = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var session = await this.SessionClient.AcceptMessageSessionAsync();
                        Debug.WriteLine($"Session: {session.SessionId}");
                        _ = RunDispatcherForActor(session, cancelToken);
                    }
                    catch (ServiceBusTimeoutException ex)
                    {
                      
                    }
                }
            }, cancelToken);

            tasks[1] = Task.Run(async ()=> {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(500);
                }
            });

            Task.WaitAny(tasks);

        }


        #region Private Methods

        private RetryPolicy createRetryPolicy()
        {
            return new RetryExponential(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 5);
        }


        private async Task RunDispatcherForActor(IMessageSession session, CancellationToken cancelToken)
        {
            while (cancelToken.IsCancellationRequested == false)
            {
                var msg = await session.ReceiveAsync();
                if (msg != null)
                {
                    ActorBase actor;

                    Type tp = Type.GetType((string)msg.UserProperties[ActorReference.cActorType]);
                    if (tp == null)
                        throw new ArgumentException($"Cannot find type '{session.SessionId}'");

                    var id = new ActorId((string)msg.UserProperties[ActorReference.cActorId]);
                    if (!actorMap.ContainsKey(session.SessionId))
                    {
                        actor = Activator.CreateInstance(tp, id) as ActorBase;

                        actorMap[session.SessionId] = actor;

                        actor.Activated();
                    }

                    actor = actorMap[session.SessionId];

                    Debug.WriteLine($"Received message: {tp.Name}/{id}");

                    var invokingMsg = ActorReference.DeserializeMsg<object>(msg.Body);

                    InvokeOperationOnActor(actor, invokingMsg);

                    await session.CompleteAsync(msg.SystemProperties.LockToken);
                }
                else
                {
                    await session.CloseAsync();
                    break;
                }
            }
        }

        private ManualResetEvent rcvEvent = new ManualResetEvent(false);

        private async Task OnMessageReceivedAsync(Message message, CancellationToken token)
        {
            rcvEvent.WaitOne();

            receivedMsgQueue.TryAdd(message.ReplyTo, message);

            rcvEvent.Set();

            await Task.FromResult<bool>(true);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private void InvokeOperationOnActor(ActorBase actor, object msg)
        {
            actor.Invoke(msg);
        }
        #endregion
    }
}
