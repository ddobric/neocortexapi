using Akka.Actor;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{


    public class DictNodeActor : ReceiveActor
    {
        public Topology ColumnTopology { get; set; }

        public Topology InputTopology { get; set; }

        private Dictionary<object, object> dict = new Dictionary<object, object>();

        private HtmConfig config;

        protected override void Unhandled(object msg)
        {
            Console.WriteLine($"Unhandled message: '{msg.GetType().Name}'");
            //base.Unhandled(msg);
        }

        public DictNodeActor()
        {
            Receive<CreateDictNodeMsg>(msg =>
            {
                this.config = msg.HtmAkkaConfig;

                this.ColumnTopology =new Topology(this.config.ColumnDimensions);
                this.InputTopology = new Topology(this.config.InputDimensions);
                
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");
                Sender.Tell(-1, Self);
            });

            Receive<AddOrUpdateElementsMsg>(msg =>
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
                    //Console.WriteLine(JsonConvert.SerializeObject(element));
                    this.dict[element.Key] = element.Value;
                }

                Sender.Tell(msg.Elements.Count, Self);
            });

            Receive<GetElementsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                object element;

                if (msg.Keys == null)
                    throw new ArgumentException("Key must be specified.");
                
                if (dict.TryGetValue(msg.Keys, out element))
                    Sender.Tell(new Result { IsError = false, Value = element }, Self);
                else
                    Sender.Tell(new Result { IsError = true, Value = null }, Self);
            });

            Receive<GetElementsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                object element;

                if (msg.Keys == null)
                    throw new ArgumentException("At least one key must be specified.");

                List<KeyPair> result = new List<KeyPair>();

                //
                // Returns a single value.
                foreach (var key in msg.Keys)
                {
                    if (dict.TryGetValue(msg.Keys, out element))
                        result.Add(new KeyPair() { Key = key, Value = element });
                    else
                        result.Add(new KeyPair() { Key = key, Value = null });
                }
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


        public int[] GetInputNeighborhood(int centerInput, int potentialRadius)
        {
            return this.config.IsWrapAround ?
                this.InputTopology.GetWrappingNeighborhood(centerInput, potentialRadius) :
                    this.InputTopology.GetNeighborhood(centerInput, potentialRadius);
        }

    }
}
