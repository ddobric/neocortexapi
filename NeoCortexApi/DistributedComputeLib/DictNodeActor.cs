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
            Receive<AddOrUpdateElementsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                if (msg.Elements == null)
                    throw new DistributedException($"{nameof(DictNodeActor)} failed to add element. List of adding elements cannot be empty.");

                foreach (var element in msg.Elements)
                {
                    if(this.dict.ContainsKey(element.Key))
                        this.dict.Add(element.Key, element.Value);
                    else
                        this.dict[element.Key] = element.Value;
                }

                Sender.Tell(msg.Elements.Count, Self);
            });


            Receive<AddElementsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                if (msg.Elements == null)
                    throw new DistributedException($"{nameof(DictNodeActor)} failed to add element. List of adding elements cannot be empty.");

                foreach (var element in msg.Elements)
                {
                    this.dict.Add(element.Key, element.Value);
                }
                
                Sender.Tell(msg.Elements.Count, Self);
            });

            Receive<UpdateElementsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                if (msg.Elements == null)
                    throw new DistributedException($"{nameof(DictNodeActor)} failed to add element. List of adding elements cannot be empty.");

                foreach (var element in msg.Elements)
                {
                    this.dict[element.Key] = element.Value;
                }

                Sender.Tell(msg.Elements.Count, Self);
            });

            Receive<GetElementMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                object element;

                if(dict.TryGetValue(msg.Key, out element))
                    Sender.Tell(new Result { IsError = false, Value = element }, Self);
                else
                    Sender.Tell(new Result { IsError = true, Value = null }, Self);
            });

            Receive<ContainsMsg>(msg =>
            {
                var res = this.dict.ContainsKey(msg.Key);

                Sender.Tell(res, Self);
            });

            Receive<ContainsKeyMsg>(msg =>
            {
                var res = this.dict.ContainsKey(msg.Key);

                Sender.Tell(res, Self);
            });

            Receive<GetCountMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                Sender.Tell(this.dict.Count, Self);
            });

        
            Receive<CreateDictNodeMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");
                Sender.Tell(-1, Self);
            });

            Receive<Terminated>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");
                Console.WriteLine($"{nameof(DictNodeActor)} termintion - {msg.ActorRef}");
                Console.WriteLine("Was address terminated? {0}", msg.AddressTerminated);
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
