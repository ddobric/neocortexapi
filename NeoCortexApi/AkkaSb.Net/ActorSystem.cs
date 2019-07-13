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
using Microsoft.Extensions.Logging;

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

        private TopicClient sendRequestClient;

        private ILogger logger;

        private int CriticalMemInGb = 10;

        private ConcurrentDictionary<string, ActorBase> actorMap = new ConcurrentDictionary<string, ActorBase>();

        /// <summary>
        /// The name of subscription where to listen messages.
        /// </summary>
        private string subscriptionName;

        public string Name { get; set; }

        public int MaxAccetedSessionsAtOnce = 10;

        private IPersistenceProvider persistenceProvider;

        /// <summary>
        /// 
        /// </summary>
        public IPersistenceProvider PersistenceProvider
        {
            get
            {
                return this.persistenceProvider;
            }
        }

        public ActorSystem(string name, ActorSbConfig config, ILogger logger = null, IPersistenceProvider persistenceProvider = null)
        {
            this.logger = logger;
            this.persistenceProvider = persistenceProvider;
            this.Name = name;
            this.sbConnStr = config.SbConnStr;
            this.subscriptionName = config.RequestSubscriptionName;
            this.sessionRcvClient = new SessionClient(config.SbConnStr, $"{config.RequestMsgTopic}/Subscriptions/{config.RequestSubscriptionName}",
            retryPolicy: createRetryPolicy(),
            receiveMode: ReceiveMode.PeekLock);

            this.sendRequestClient = new TopicClient(config.SbConnStr, config.RequestMsgTopic,
            retryPolicy: createRetryPolicy());

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
            ActorReference actorRef = new ActorReference(typeof(TActor), id, this.sendRequestClient, this.ReplyMsgReceiverQueueClient.Path, receivedMsgQueue, this.rcvEvent, this.MaxProcessingTimeOfMessage, this.Name, this.logger);
            return actorRef;
        }

        long runningTasks = 0;

        public void Start(CancellationToken cancelToken)
        {
            CancellationTokenSource src = new CancellationTokenSource();

            Task[] tasks = new Task[2];

            tasks[0] = Task.Run(async () =>
            {
                while (!src.Token.IsCancellationRequested)
                {
                    var proc = Process.GetCurrentProcess();

                    Debug.WriteLine($"WS={Environment.WorkingSet / 1024 / 1024 } MB, PWS64={proc.WorkingSet64}, PVM={proc.VirtualMemorySize64}");

                    var val = Interlocked.Read(ref runningTasks);

                    if (val >= 10)
                    {
                        logger?.LogWarning($"Accepted maximal nuber of sessions: {runningTasks}.");

                        await Task.Delay(1000);

                        GC.Collect();
                    }
                    else
                    {
                        try
                        {
                            var session = await this.sessionRcvClient.AcceptMessageSessionAsync();
                            logger?.LogInformation($"{this.Name} - Accepted new session: {session.SessionId}");
                            Interlocked.Increment(ref runningTasks);

                            _ = RunDispatcherForSession(session, cancelToken).ContinueWith(
                                async (t) =>
                                {
                                    Interlocked.Decrement(ref runningTasks);
                                    if (t.Exception != null)
                                    {
                                        logger.LogError(t.Exception, $"Session error: {session.SessionId}");
                                    }

                                    await session.CloseAsync();

                                    logger?.LogTrace("Session closed.");
                                });
                        }
                        catch (ServiceBusTimeoutException ex)
                        {
                            logger?.LogWarning($"ServiceBusTimeoutException");
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError("Listener has failed.", ex);
                            //throw;
                        }
                    }
                }
            }, src.Token);

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


        private async Task RunDispatcherForSession(IMessageSession session, CancellationToken cancelToken)
        {
            while (cancelToken.IsCancellationRequested == false)
            {
                var msg = await session.ReceiveAsync(TimeSpan.FromSeconds(1));

                if (msg != null)
                {
                    bool isPersistedAfterCalculus = false;
                    try
                    {
                        ActorBase actor = null;

                        Type tp = Type.GetType((string)msg.UserProperties[ActorReference.cActorType]);
                        if (tp == null)
                            throw new ArgumentException($"Cannot find type '{session.SessionId}'");

                        var id = new ActorId((string)msg.UserProperties[ActorReference.cActorId]);
                        if (!actorMap.ContainsKey(session.SessionId))
                        {
                            if (this.persistenceProvider != null)
                            {
                                actor = await this.persistenceProvider.LoadActor(id);
                                if(actor != null)
                                    logger?.LogInformation($"{this.Name} - Loaded from pesisted store: {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");
                            }

                            if (actor == null)
                            {
                                actor = Activator.CreateInstance(tp, id) as ActorBase;
                                logger?.LogInformation($"{this.Name} - New instance created: {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");
                            }

                            actor.PersistenceProvider = this.PersistenceProvider;

                            actor.Logger = logger;

                            actorMap[session.SessionId] = actor;

                            actor.Activated();
                        }

                        actor = actorMap[session.SessionId];

                        logger?.LogInformation($"{this.Name} - Received message: {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");

                        var invokingMsg = ActorReference.DeserializeMsg<object>(msg.Body);

                        var replyMsg = await InvokeOperationOnActorAsync(actor, invokingMsg, (bool)msg.UserProperties[ActorReference.cExpectResponse],
                            msg.MessageId, msg.ReplyTo);

                        logger?.LogInformation($"{this.Name} - Invoked : {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");

                        await persistAndCleanupIfRequired(session);

                        logger?.LogInformation($"{this.Name} - Persisted : {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");

                        isPersistedAfterCalculus = true;

                        // If actor operation was invoked with Ask<>(), then reply is expected.
                        if (replyMsg != null)
                        {
                            await this.sendReplyQueueClients[msg.ReplyTo].SendAsync(replyMsg);

                            logger?.LogInformation($"{this.Name} - Replied : {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");
                        }

                        await session.CompleteAsync(msg.SystemProperties.LockToken);

                        logger?.LogInformation($"{this.Name} - Completed : {tp.Name}/{id}, actorMap: {actorMap.Keys.Count}");

                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Messsage processing error");

                        if (isPersistedAfterCalculus == false)
                            await persistAndCleanupIfRequired(session);

                        if (!(ex is SessionLockLostException))
                            await session.AbandonAsync(msg.SystemProperties.LockToken);
                    }
                }
                else
                {
                    logger?.LogTrace($"{this.Name} - No more messages received for sesson {session.SessionId}");
                    await persistAndCleanupIfRequired(session);
                    //return;
                }

                if (IsMemoryCritical())
                {
                    logger?.LogWarning($"Memory reached critical value: {this.CriticalMemInGb}, {Environment.WorkingSet / 1024 / 1024 / 1024}");
                }

                break;
            }
        }

        private Task persistAndCleanupIfRequired(IMessageSession session)
        {
            return Task.CompletedTask;
            //if (this.persistenceProvider != null)
            //{
            //    ActorBase removed;
            //    //if (IsMemoryCritical())
            //    if (actorMap.TryRemove(session.SessionId, out removed))
            //        await this.persistenceProvider.PersistActor(removed);
            //    else
            //        logger?.LogError($"Cannot remove actor from map. {session.SessionId}");

            //    logger?.LogTrace($"{this.Name} -  Actor for '{session.SessionId}' persisted.");
            //}
        }


        /// <summary>
        /// SessionId = "Actor Type Name/ActorId"
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        private ActorId getActorIdFromSession(string sessionId)
        {
            var strId = sessionId.Split('/')[1];

            return new ActorId(strId);
        }

        private ManualResetEvent rcvEvent = new ManualResetEvent(false);

        private async Task OnMessageReceivedAsync(Message message, CancellationToken token)
        {
            logger?.LogInformation($"ActorSystem: {Name} Response received. receivedMsgQueue instance: {receivedMsgQueue.GetHashCode()}");

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

        private async Task<Message> InvokeOperationOnActorAsync(ActorBase actor, object msg, bool expectResponse, string replyMsgId, string replyTo)
        {
            return await Task<Message>.Run(() =>
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

                    return sbMsg;
                }
                else
                {
                    if (res != null)
                        throw new InvalidOperationException($"The actor {actor} should return NULL.");
                    else
                        return null;
                }
            });
        }

        private bool IsMemoryCritical()
        {
            if (Environment.WorkingSet / 1024 / 1024 / 1024 >= this.CriticalMemInGb)
                return true;
            else
                return false;
        }
        #endregion
    }
}
