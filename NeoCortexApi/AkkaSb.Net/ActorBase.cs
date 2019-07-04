
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AkkaSb.Net
{
    public class ActorId
    {
        public long? Id { get; set; }

        public string IdAsString { get;set; }
        
        public ActorId(long id)
        {
            this.Id = id;
            this.IdAsString = id.ToString();
        }

        public ActorId(string id)
        {
            this.IdAsString = id.ToString();
        }

        public override string ToString()
        {
            return $"{this.IdAsString}";
        }

        public static implicit operator string(ActorId id)
        {
            return id.IdAsString;
        }

        public static implicit operator ActorId(long id)
        {
            return new ActorId(id);
        }

    }

    public class ActorBase
    {
        internal Dictionary<Type, Delegate> dict = new Dictionary<Type, Delegate>();

        public ActorId Id { get; set; }

        public ILogger Logger { get;  set; }

        //public ActorBase()
        //{
  
        //}

        public ActorBase(ActorId id)
        {
            this.Id = id;
   
        }

        /// <summary>
        /// Invoked if the actor receives the message, which cannot be dispatched.
        /// </summary>
        public virtual void UnrouteedMessage()
        {

        }

        internal object Invoke(object message)
        {
            var pair = dict.FirstOrDefault(o => o.Key == message.GetType());
            if (pair.Key == null)
                throw new ArgumentException($"Message contains unregistered message type. '{message.GetType()}' was not registerd in Receive() method.");
         
            var res = pair.Value.DynamicInvoke(message);

            return res;
        }

        public virtual void Activated()
        {

        }

        public virtual void DeActivated()
        {

        }

        public void Receive<T>(Func<T, object> handler)
        {
            dict[typeof(T)] = (Delegate)handler;
        }

    }
}
