using Akka.Actor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    

    public class DictNodeActor : ReceiveActor
    {
        private Dictionary<object, object> dict = new Dictionary<object, object>();

        protected override void Unhandled(object msg)
        {
            Console.WriteLine($"Unhandled message: '{msg.GetType().Name}'");
            //base.Unhandled(msg);
        }

        public DictNodeActor()
        {
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

        /// <summary>
        /// It traverses all connected synapses of the column and calculates the span, which synapses
        /// spans between all input bits. Then it calculates average of spans accross all dimensions. 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        //private virtual double getAvgSpanOfConnectedSynapsesForColumn(Connections c, int columnIndex)
        //{
        //    int[] dimensions = c.getInputDimensions();

        //    // Gets synapses connected to input bits.(from pool of the column)
        //    int[] connected = c.getColumn(columnIndex).ProximalDendrite.getConnectedSynapsesSparse();

        //    if (connected == null || connected.Length == 0) return 0;

        //    int[] maxCoord = new int[c.getInputDimensions().Length];
        //    int[] minCoord = new int[c.getInputDimensions().Length];
        //    ArrayUtils.fillArray(maxCoord, -1);
        //    ArrayUtils.fillArray(minCoord, ArrayUtils.max(dimensions));
        //    ISparseMatrix<int> inputMatrix = c.getInputMatrix();

        //    //
        //    // It takes all connected synapses
        //    // 
        //    for (int i = 0; i < connected.Length; i++)
        //    {
        //        maxCoord = ArrayUtils.maxBetween(maxCoord, inputMatrix.computeCoordinates(connected[i]));
        //        minCoord = ArrayUtils.minBetween(minCoord, inputMatrix.computeCoordinates(connected[i]));
        //    }
        //    return ArrayUtils.average(ArrayUtils.add(ArrayUtils.subtract(maxCoord, minCoord), 1));
        //}
    }
}
