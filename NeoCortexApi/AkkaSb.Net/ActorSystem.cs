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
using System.Reflection;

namespace AkkaSb.Net
{
    public class ActorSystem
    {
        private ConcurrentDictionary<string, Message> receivedMsgQueue = new ConcurrentDictionary<string, Message>();

        private TimeSpan MaxProcessingTimeOfMessage { get; set; } = TimeSpan.FromDays(1);

        internal Dictionary<string, ClientPair> RemoteQueueClients = new Dictionary<string, ClientPair>();

        protected SessionClient sessionRcvClient;

        protected QueueClient sendReplyQueueClient;

        private ConcurrentDictionary<string, ActorBase> actorMap = new ConcurrentDictionary<string, ActorBase>();

        public ActorSystem(AkkaSbConfig config)
        {
            this.sessionRcvClient = new SessionClient(config.SbConnStr, config.LocalNode.ReceiveQueue,
            retryPolicy: createRetryPolicy(),
            receiveMode: ReceiveMode.PeekLock);

            this.sendReplyQueueClient = new QueueClient(config.SbConnStr, config.LocalNode.ReplyQueue,
            retryPolicy: createRetryPolicy(),
            receiveMode: ReceiveMode.PeekLock);

            foreach (var node in config.RemoteNodes)
            {
                ClientPair pair = new ClientPair();

                pair.SenderClient = new QueueClient(config.SbConnStr, node.Value.ReplyQueue,
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

                // Register the function that receives reply messages.
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

        /// <summary>
        /// Should implemented partitioning.
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActorReference CreateActor<TActor>(ActorId id) where TActor : ActorBase
        {
            throw new NotImplementedException();
        }

        public void Start(CancellationToken cancelToken)
        {
            Task[] tasks = new Task[2];

            tasks[0] = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var session = await this.sessionRcvClient.AcceptMessageSessionAsync();
                        Debug.WriteLine($"Accepted new session: {session.SessionId}");
                        _ = RunDispatcherForActor(session, cancelToken).ContinueWith(async (t) => { await session.CloseAsync(); });
                    }
                    catch (ServiceBusTimeoutException ex)
                    {

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
            }, cancelToken);

            tasks[1] = Task.Run(async () =>
            {
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

                    await InvokeOperationOnActorAsync(actor, invokingMsg, (bool)msg.UserProperties[ActorReference.cExpectResponse], msg.MessageId);

                    await session.CompleteAsync(msg.SystemProperties.LockToken);
                }
                else
                {
                    Debug.WriteLine($"No more messages received for sesson {session.SessionId}");
                   
                    //break;
                }
            }
        }

        private ManualResetEvent rcvEvent = new ManualResetEvent(false);

        private async Task OnMessageReceivedAsync(Message message, CancellationToken token)
        {
            receivedMsgQueue.TryAdd(message.ReplyTo, message);

            rcvEvent.Set();

            await Task.CompletedTask;
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

        private async Task InvokeOperationOnActorAsync(ActorBase actor, object msg, bool expectResponse, string replyMsgId)
        {
            var res = actor.Invoke(msg);
            if (expectResponse)
            {
                var sbMsg = ActorReference.CreateResponseMessage(res, replyMsgId, actor.GetType(), actor.Id);
                await this.sendReplyQueueClient.SendAsync(sbMsg);
            }
            else
            {
                if (res != null)
                    throw new InvalidOperationException($"The actor {actor} should return NULL.");
            }
        }
        #endregion
    }
}
