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
    public class ActorReference
    {
        public const string cActorType = "ActorType";
        public const string cActorId = "ActorId";
        public const string cMsgType = "MsgType";
        public TimeSpan MaxProcessingTime { get; set; }


        internal ClientPair ClientPair { get; set; }

        private ConcurrentDictionary<string, Message> receivedMessages ;

        private ManualResetEvent rcvEvent;

        private Type actorType;

        private ActorId actorId;

        private string remoteNodeName;

        internal ActorReference(Type actorType, ActorId id, ClientPair remoteNode,  ConcurrentDictionary<string, Message> receivedMsgQueue, ManualResetEvent rcvEvent, TimeSpan maxProcessingTime)
        {
            this.actorType = actorType;
            this.actorId = id;
            this.ClientPair = remoteNode;
            this.MaxProcessingTime = maxProcessingTime;
            this.receivedMessages = receivedMsgQueue;
            this.rcvEvent = rcvEvent;
        }


        public async Task<TResponse> Ask<TResponse>(object msg, TimeSpan? timeout = null)
        {
            var sbMsg = CreateMessage(msg);

            await this.ClientPair.SenderClient.SendAsync(sbMsg);

            TResponse res = await WaitOnResponse<TResponse>(sbMsg, timeout);

            return res;
        }

        public async Task Tell(object msg, TimeSpan? timeout = null)
        {
            var sbMsg = CreateMessage(msg);

            await this.ClientPair.SenderClient.SendAsync(sbMsg);
        }

   

        #region Private Methods

   

        private async Task<TResponse> WaitOnResponse<TResponse>(Message sbMsg, TimeSpan? timeout = null)
        {
            DateTime entered = DateTime.Now;

            TResponse msg = default(TResponse);

            while (DateTime.Now < entered.AddMinutes(timeout.HasValue ? timeout.Value.TotalMinutes : this.MaxProcessingTime.TotalMinutes))
            {
                rcvEvent.WaitOne();

                Message sbRcvMsg;

                if (receivedMessages.TryGetValue(sbMsg.MessageId, out sbRcvMsg))
                {
                    msg = DeserializeMsg<TResponse>(sbMsg.Body);
                    break;
                }

                rcvEvent.Reset();
            }

            return msg;
        }

      

        private Message CreateMessage(object msg)
        {
            Message sbMsg = new Message(SerializeMsg(msg));
            sbMsg.UserProperties.Add(cActorType, this.actorType.AssemblyQualifiedName);
            sbMsg.UserProperties.Add(cMsgType, msg.GetType().AssemblyQualifiedName);
            sbMsg.UserProperties.Add(cActorId, (string)this.actorId);            

            sbMsg.SessionId = $"{this.GetType().Name}/{this.actorId}";

            return sbMsg;
        }

        internal static byte[] SerializeMsg(object msg)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.TypeNameHandling = TypeNameHandling.All;
            var strObj = JsonConvert.SerializeObject(msg, sett);

            return UTF8Encoding.UTF8.GetBytes(strObj);
        }

        internal static T DeserializeMsg<T>(byte[] msg)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.TypeNameHandling = TypeNameHandling.All;
            var strObj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(msg), sett);

            return strObj;
        }


        ///// <summary>
        ///// Register two handlers: Message Receive- and Error-handler.
        ///// </summary>
        //private void RegisterReceiveHandler<TResponse>()
        //{
        //    // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
        //    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        //    {

        //        // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
        //        // Set it according to how many messages the application wants to process in parallel.
        //        MaxConcurrentCalls = 1,

        //        // Indicates whether the message pump should automatically complete the messages after returning from user callback.
        //        // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
        //        AutoComplete = true
        //    };


        //    // Register the function that processes messages with reliable messaging.
        //    this.ClientPair.ReceiverClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        //}

       

        #endregion
    }
}
