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
        private string sbConnStr;

        private ConcurrentDictionary<string, Message> receivedMsgQueue = new ConcurrentDictionary<string, Message>();

        private TimeSpan MaxProcessingTimeOfMessage { get; set; } = TimeSpan.FromDays(1);

        internal volatile Dictionary<string, QueueClient> sendReplyQueueClients = new Dictionary<string, QueueClient>();

        private QueueClient ReplyMsgReceiverQueueClient;

        private SessionClient sessionRcvClient;

        private QueueClient sendRequestQueueClient;


        private ConcurrentDictionary<string, ActorBase> actorMap = new ConcurrentDictionary<string, ActorBase>();

        public string Name { get; set; }

        public ActorSystem(string name, AkkaSbConfig config)
        {
            this.Name = name;
            this.sbConnStr = config.SbConnStr;
            this.sessionRcvClient = new SessionClient(config.SbConnStr, config.RequestMsgQueue,
            retryPolicy: createRetryPolicy(),
            receiveMode: ReceiveMode.PeekLock);

            this.sendRequestQueueClient = new QueueClient(config.SbConnStr, config.RequestMsgQueue,
            retryPolicy: createRetryPolicy(),
            receiveMode: ReceiveMode.PeekLock);

            //
            // Receiving of reply messages is optional. If the actor system does not send messages
            // then it will also not listen for reply messages.
            if (config.ReplyMsgQueue != null)
            {
                ReplyMsgReceiverQueueClient = new QueueClient(config.SbConnStr, config.ReplyMsgQueue,
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
                ReplyMsgReceiverQueueClient.RegisterMessageHandler(OnMessageReceivedAsync, messageHandlerOptions);
            }
        }

        public ActorReference CreateActor<TActor>(ActorId id) where TActor : ActorBase
        {
            ActorReference actorRef = new ActorReference(typeof(TActor), id, this.sendRequestQueueClient, this.ReplyMsgReceiverQueueClient.Path,  receivedMsgQueue, this.rcvEvent, this.MaxProcessingTimeOfMessage, this.Name);
            return actorRef;
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
                        Debug.WriteLine($"{this.Name} - Accepted new session: {session.SessionId}");
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

                    Debug.WriteLine($"{this.Name} - Received message: {tp.Name}/{id}");

                    var invokingMsg = ActorReference.DeserializeMsg<object>(msg.Body);
                   
                    await InvokeOperationOnActorAsync(actor, invokingMsg, (bool)msg.UserProperties[ActorReference.cExpectResponse], 
                        msg.MessageId, msg.ReplyTo);

                    await session.CompleteAsync(msg.SystemProperties.LockToken);
                }
                else
                {
                    Debug.WriteLine($"{this.Name} - No more messages received for sesson {session.SessionId}");

                    //break;
                }
            }
        }

        private ManualResetEvent rcvEvent = new ManualResetEvent(false);

        private async Task OnMessageReceivedAsync(Message message, CancellationToken token)
        {
            Debug.WriteLine($"ActorSystem: {Name} Response received. receivedMsgQueue instance: {receivedMsgQueue.GetHashCode()}");

            receivedMsgQueue.TryAdd(message.CorrelationId, message);

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
            return Task.FromException(exceptionReceivedEventArgs.Exception);
        }

        private async Task InvokeOperationOnActorAsync(ActorBase actor, object msg, bool expectResponse, string replyMsgId, string replyTo)
        {
            var res = actor.Invoke(msg);
            if (expectResponse)
            {
                if (this.sendReplyQueueClients.ContainsKey(replyTo) == false)
                {
                    this.sendReplyQueueClients.Add(replyTo, new QueueClient(this.sbConnStr, replyTo,
                    retryPolicy: createRetryPolicy(),
                    receiveMode: ReceiveMode.PeekLock));
                }

                var sbMsg = ActorReference.CreateResponseMessage(res, replyMsgId, actor.GetType(), actor.Id);
                await this.sendReplyQueueClients[replyTo].SendAsync(sbMsg);
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
