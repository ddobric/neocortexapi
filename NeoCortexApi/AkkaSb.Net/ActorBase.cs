
using System;
using System.Collections.Generic;
using System.Text;
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

    }

    public class ActorBase
    {
        private Dictionary<object, object> dict = new Dictionary<object, object>();
        
        public ActorId Id { get; set; }

        public ActorBase(ActorId id)
        {
            this.Id = id;
        }


        void Receive<T>(Action<T> handler)
        {
            dict[typeof(T).ToString()] = handler;
        }
     
 
    }
}
