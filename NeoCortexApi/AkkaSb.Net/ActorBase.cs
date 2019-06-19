
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkkaSb.Net
{
    public class ActorId
    {
        public long Id { get; set; }

        public ActorId(long id)
        {
            this.Id = id;
        }

        public override string ToString()
        {
            return $"{this.Id}";
        }

        public static implicit operator string(ActorId id)
        {
            return id.Id.ToString();
        }

    }

    public class ActorBase
    {
        public const string cActorType = "ActorType";
        public const string cActorId = "ActorId";
        public const string cMsgType = "MsgType";
        private const int cMaxProcessingTimeOfMessage = 10;
        private Dictionary<object, object> dict = new Dictionary<object, object>();

        public ActorId Id { get; set; }

        internal ClientPair ClientPair { get; set; }

        private ConcurrentDictionary<string, Message> receivedMessages = new ConcurrentDictionary<string, Message>();


        public ActorBase(ActorId id)
        {
            this.Id = id;

            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {

                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                MaxAutoRenewDuration = TimeSpan.FromMinutes(cMaxProcessingTimeOfMessage),

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = true
            };

            // Register the function that processes messages with reliable messaging.
            this.ClientPair.ReceiverClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

        }



        public async Task<TResponse> Ask<TResponse>(object msg, TimeSpan? timeout = null)
        {
            var sbMsg = CreateMessage(msg);
            TResponse res = await SendMessageAndWaitOnResponse<TResponse>(sbMsg, timeout);

            return res;
        }

        void Receive<T>(Action<T> handler)
        {
            dict[typeof(T).ToString()] = handler;
        }

        #region Private Methods

        private ManualResetEvent rcvEvent = new ManualResetEvent(false);
            
        private async Task<TResponse> SendMessageAndWaitOnResponse<TResponse>(Message sbMsg, TimeSpan? timeout = null)
        {
            await this.ClientPair.SenderClient.SendAsync(sbMsg);

            DateTime entered = DateTime.Now;

            TResponse msg = default(TResponse);

            while (DateTime.Now < entered.AddMinutes(timeout.HasValue ? timeout.Value.TotalMinutes : cMaxProcessingTimeOfMessage))
            {
                rcvEvent.WaitOne();

                Message sbRcvMsg;

                if (receivedMessages.TryGetValue(sbMsg.MessageId, out sbRcvMsg))
                {
                    msg= DeserializeMsg<TResponse>(sbMsg.Body);
                    break;
                }

                rcvEvent.Reset();
            }

            return msg;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            rcvEvent.WaitOne();

            receivedMessages.TryAdd(message.ReplyTo, message);

            rcvEvent.Set();

            await Task.FromResult<bool>(true);
        }

        private Message CreateMessage(object msg)
        {
            Message sbMsg = new Message(SerializeMsg(msg));
            sbMsg.UserProperties.Add(cActorType, this.GetType().AssemblyQualifiedName);
            sbMsg.UserProperties.Add(cMsgType, msg.GetType().AssemblyQualifiedName);

            sbMsg.SessionId = this.Id;
            return sbMsg;
        }

        private byte[] SerializeMsg(object msg)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.TypeNameHandling = TypeNameHandling.All;
            var strObj = JsonConvert.SerializeObject(msg, sett);

            return UTF8Encoding.UTF8.GetBytes(strObj);
        }

        private T DeserializeMsg<T>(byte[] msg)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.TypeNameHandling = TypeNameHandling.All;
            var strObj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(msg), sett);

            return strObj;
        }


        /// <summary>
        /// Register two handlers: Message Receive- and Error-handler.
        /// </summary>
        private void RegisterReceiveHandler<TResponse>()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {

                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = true
            };


            // Register the function that processes messages with reliable messaging.
            this.ClientPair.ReceiverClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
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

        #endregion
    }
}
