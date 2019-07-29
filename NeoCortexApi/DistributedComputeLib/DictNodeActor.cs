using Akka.Actor;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace NeoCortexApi.DistributedComputeLib
{


    public class DictNodeActor : ReceiveActor
    {
        private int partitionKey;

        private Dictionary<object, object> dict = new Dictionary<object, object>();

        private HtmConfig config;

        protected override void Unhandled(object msg)
        {
            Console.WriteLine($"Unhandled message: '{msg.GetType().Name}'");
            //base.Unhandled(msg);
        }

        public DictNodeActor()
        {
            Debug.WriteLine($"ctor: {this.Self.Path}");

            Receive((Action<PingNodeMsg>)(msg =>
            {
                Sender.Tell($"Ping back - {msg.Msg}", Self);
            }));

            Receive<CreateDictNodeMsg>(msg =>
            {
                this.config = msg.HtmAkkaConfig;

                //this.ColumnTopology =new Topology(this.config.ColumnDimensions);
                //this.InputTopology = new Topology(this.config.InputDimensions);

                Console.WriteLine($"Received message: '{msg.GetType().Name}'");
                Sender.Tell(-1, Self);
            });

            Receive((Action<InitColumnsMsg>)(msg =>
            { 
                initializeColumns(msg);

                if (this.partitionKey == 1)
                {
                    Debug.WriteLine($"INIT: this: {this.GetHashCode()} - dict: {this.dict.GetHashCode()}");
                }
            }));

            Receive((Action<ConnectAndConfigureColumnsMsg>)(msg =>
            {
                if (this.partitionKey == 1)
                {
                    Debug.WriteLine($"CONNECT: this: {this.GetHashCode()} - dict: {this.dict.GetHashCode()}");
                }

                createAndConnectColumns(msg);
            }));

            Receive((Action<CalculateOverlapMsg>)(msg =>
            {
                calculateOverlap(msg);
            }));

            Receive((Action<AdaptSynapsesMsg>)(msg =>
            {
                adaptSynapses(msg);
            }));


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

                if (msg.Key == null)
                    throw new ArgumentException("Key must be specified.");

                if (dict.TryGetValue(msg.Key, out element))
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

            Receive<BumUpWeakColumnsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                bumpUpWeakColumns(msg);
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
            Console.WriteLine($"{nameof(DictNodeActor)} | '{Self.Path}' started.");
            //m_HelloTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
            //    TimeSpan.FromSeconds(1), Context.Self, new Actor1Message(), ActorRefs.NoSender);
        }

        protected override void PostStop()
        {
            Console.WriteLine($"{nameof(DictNodeActor)} | '{Self.Path}' stoped.");
        }


        //public int[] GetInputNeighborhood(int centerInput, int potentialRadius)
        //{
        //    return this.config.IsWrapAround ?
        //        this.InputTopology.GetWrappingNeighborhood(centerInput, potentialRadius) :
        //            this.InputTopology.GetNeighborhood(centerInput, potentialRadius);
        //}


        #region Private Methods
        /// <summary>
        /// Creates columns on the node.
        /// </summary>
        /// <param name="msg"></param>
        private void initializeColumns(InitColumnsMsg msg)
        {
            this.partitionKey = msg.PartitionKey;

            dict = new Dictionary<object, object>();

            Console.WriteLine($"{Self.Path} -  Received message: '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}, partitionKey: {this.partitionKey}");

            for (int i = msg.MinKey; i <= msg.MaxKey; i++)
            {
                this.dict[i] = new Column(this.config.CellsPerColumn, i, this.config.SynPermConnected, this.config.NumInputs);
            }

            /*
            if (msg.Elements == null || msg.Elements.Count == 0)
                throw new DistributedException($"{nameof(DictNodeActor)} failed to create columns. List of elements cannot be empty.");

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Environment.ProcessorCount;

            //
            // Here is upload performed in context of every actor (partition).
            // Because keys are grouped by partitions (actors) parallel upload can be done here.
            foreach (var element in msg.Elements)
            {
                HtmConfig cfg = element.Value as HtmConfig;
                if (cfg == null)
                    throw new ArgumentException($"Value hast to be of type {nameof(HtmConfig)}");

                this.dict[element.Key] = new Column(this.config.CellsPerColumn, (int)element.Key, this.config.SynPermConnected, cfg.NumInputs);
            }
            */

            Debug.WriteLine($"Init completed. {msg.PartitionKey} - {this.dict.Count} - min={msg.MinKey}, max={msg.MaxKey}");

            Console.WriteLine($"{Self.Path} - Init completed. '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}");

            Sender.Tell(msg.MaxKey - msg.MinKey, Self);

            Console.WriteLine($"{Self.Path} -  Response on init message sent '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}");

        }


        /// <summary>
        /// Initialize all columns inside of partition and connect them to sensory input.
        /// It returns the average connected span of the partition.
        /// </summary>
        /// <param name="msg"></param>
        private void createAndConnectColumns(ConnectAndConfigureColumnsMsg msg)
        {
            DateTime startTime = DateTime.Now;

            Log(msg, Self, "Entered.");

            List<double> avgConnections = new List<double>();

            Random rnd;

            if (this.config.RandomGenSeed > 0)
                rnd = new Random(this.config.RandomGenSeed);
            else
                rnd = new Random();

            if (this.dict.Count == 0)
            {

            }

            foreach (var element in this.dict)
            {
                if (this.config == null)
                    throw new ArgumentException($"HtmConfig must be set in the message.");

                int colIndx = (int)element.Key;

                // Gets RF
                var potential = HtmCompute.MapPotential(this.config, colIndx, rnd);
                var column = (Column)this.dict[colIndx];

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                //connectColumnToInputRF(c.HtmConfig, data.Potential, data.Column);
                column.CreatePotentialPool(this.config, potential, -1);

                var perms = HtmCompute.InitSynapsePermanences(this.config, potential, rnd);

                avgConnections.Add(HtmCompute.CalcAvgSpanOfConnectedSynapses(column, this.config));

                //Log(msg, Self, $".:). {dict.Count}/{dict.Keys.Min()}/{dict.Keys.Min()} - duration: {(DateTime.Now - startTime).TotalSeconds}");

                HtmCompute.UpdatePermanencesForColumn(this.config, perms, column, potential, true);
            }

            double avgConnectedSpan = ArrayUtils.average(avgConnections.ToArray());

            Log(msg, Self, "Completed.");
            Sender.Tell(avgConnectedSpan, Self);
            Log(msg, Self, $"Response sent. {(DateTime.Now - startTime).TotalSeconds}");

        }

        private void Log(object msg, IActorRef aRef, string txt)
        {
            Console.WriteLine($"{DateTime.UtcNow} - {aRef.Path.Elements.Last()} - '{msg.GetType().Name}'. {txt}");

        }

        private void calculateOverlap(CalculateOverlapMsg msg)
        {
            Console.WriteLine($"{Self.Path} - Received message: '{msg.GetType().Name}'");

            ConcurrentDictionary<int, int> overlaps = new ConcurrentDictionary<int, int>();

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Environment.ProcessorCount;

            Parallel.ForEach(this.dict, opts, (keyPair) =>
            {
                Column col = keyPair.Value as Column;

                var overlap = col.GetColumnOverlapp(msg.InputVector, this.config.StimulusThreshold);

                overlaps.TryAdd((int)keyPair.Key, overlap);
            });

            List<KeyPair> result = new List<KeyPair>();
            foreach (var item in overlaps)
            {
                result.Add(new KeyPair { Key = item.Key, Value = item.Value });
            }

            var sortedRes = result.OrderBy(k => k.Key).ToList();

            //Console.Write($"o = {sortedRes.Count(p => (int)p.Value > 0)}");

            Sender.Tell(sortedRes, Self);
        }

        void adaptSynapses(AdaptSynapsesMsg msg)
        {
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = msg.ColumnKeys.Count;

            Parallel.ForEach(msg.ColumnKeys, opts, (colPair) =>
            {
                Column activeColumn = (Column)this.dict[colPair.Key];
                //Pool pool = c.getPotentialPools().get(activeColumns[i]);
                Pool pool = activeColumn.ProximalDendrite.RFPool;
                double[] perm = pool.getDensePermanences(this.config.NumInputs);
                int[] indexes = pool.getSparsePotential();
                ArrayUtils.raiseValuesBy(msg.PermanenceChanges, perm);

                HtmCompute.UpdatePermanencesForColumn(this.config, perm, activeColumn, indexes, true);
            });

            // We send this to ensure reliable messaging. No other result is required here.
            Sender.Tell(0, Self);
        }

        public void bumpUpWeakColumns(BumUpWeakColumnsMsg msg)
        {
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = msg.ColumnKeys.Count;

            Parallel.ForEach(msg.ColumnKeys, opts, (colPair) =>
            {
                Column weakColumn = (Column)dict[colPair.Key];

                Pool pool = weakColumn.ProximalDendrite.RFPool;
                double[] perm = pool.getSparsePermanences();
                ArrayUtils.raiseValuesBy(this.config.SynPermBelowStimulusInc, perm);
                int[] indexes = pool.getSparsePotential();

                weakColumn.UpdatePermanencesForColumnSparse(this.config, perm, indexes, true);
            });

            Sender.Tell(0, Self);
        }

        public static string StringifyVector(double[] vector)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                sb.Append(", ");
            }

            return sb.ToString();
        }
        #endregion
    }
}
