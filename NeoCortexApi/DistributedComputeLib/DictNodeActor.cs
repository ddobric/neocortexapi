using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedComputeLib
{
    public class DictNodeActor : ReceiveActor
    {
        public class CreateDictNodeMsg
        {

        }

        public DictNodeActor()
        {            
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
