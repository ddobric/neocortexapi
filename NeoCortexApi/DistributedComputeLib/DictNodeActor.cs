using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    

    public class DictNodeActor : ReceiveActor
    {
        private Dictionary<object, object> dict = new Dictionary<object, object>();

        public DictNodeActor()
        {
            Receive<AddElementMsg>(msg =>
            {
                Console.WriteLine($"{nameof(DictNodeActor)} -  Receive<string> - {msg} - Calculation started.");

                if (msg.Elements == null)
                    throw new DistributedException($"{nameof(DictNodeActor)} failed to add element. List of adding elements cannot be empty.");

                foreach (var element in msg.Elements)
                {
                    this.dict.Add(element.Key, element.Value);
                }
                
                Sender.Tell(msg.Elements.Count, Self);
            });

            Receive<GetElementMsg>(msg =>
            {
                Console.WriteLine($"{nameof(DictNodeActor)} -  Receive<string> - {msg} - Calculation started.");

                object element;

                if(dict.TryGetValue(msg.Key, out element))
                    Sender.Tell(new Result { IsError = false, Value = element }, Self);
                else
                    Sender.Tell(new Result { IsError = true, Value = null }, Self);
            });

            Receive<CreateDictNodeMsg>(msg =>
            {
                Console.WriteLine($"{nameof(DictNodeActor)} -  Receive<CreateDictNodeMsg> - {msg} - Calculation started.");
            
                Sender.Tell(-1, Self);
            });

            Receive<Terminated>(terminated =>
            {
                Console.WriteLine($"{nameof(DictNodeActor)} termintion - {terminated.ActorRef}");
                Console.WriteLine("Was address terminated? {0}", terminated.AddressTerminated);
            });
        }

        protected override void PreStart()
        {
            Console.WriteLine($"{nameof(DictNodeActor)} started.");
            //m_HelloTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
            //    TimeSpan.FromSeconds(1), Context.Self, new Actor1Message(), ActorRefs.NoSender);
        }

        protected override void PostStop()
        {
            Console.WriteLine($"{nameof(DictNodeActor)} stoped.");
        }
    }
}
