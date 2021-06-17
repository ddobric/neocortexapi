// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
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
        private int m_PartitionKey;

        private Dictionary<object, object> m_Dict = new Dictionary<object, object>();

        private HtmConfig m_Config;

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
                this.m_Config = msg.HtmAkkaConfig;

                //this.ColumnTopology =new Topology(this.config.ColumnDimensions);
                //this.InputTopology = new Topology(this.config.InputDimensions);

                Console.WriteLine($"Received message: '{msg.GetType().Name}'");
                Sender.Tell(-1, Self);
            });

            Receive((Action<InitColumnsMsg>)(msg =>
            { 
                InitializeColumns(msg);

                if (this.m_PartitionKey == 1)
                {
                    Debug.WriteLine($"INIT: this: {this.GetHashCode()} - dict: {this.m_Dict.GetHashCode()}");
                }
            }));

            Receive((Action<ConnectAndConfigureColumnsMsg>)(msg =>
            {
                if (this.m_PartitionKey == 1)
                {
                    Debug.WriteLine($"CONNECT: this: {this.GetHashCode()} - dict: {this.m_Dict.GetHashCode()}");
                }

                CreateAndConnectColumns(msg);
            }));

            Receive((Action<CalculateOverlapMsg>)(msg =>
            {
                CalculateOverlap(msg);
            }));

            Receive((Action<AdaptSynapsesMsg>)(msg =>
            {
                AdaptSynapses(msg);
            }));


            Receive<AddOrUpdateElementsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                if (msg.Elements == null)
                    throw new DistributedException($"{nameof(DictNodeActor)} failed to add element. List of adding elements cannot be empty.");

                foreach (var element in msg.Elements)
                {
                    this.m_Dict[element.Key] = element.Value;
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
                    this.m_Dict.Add(element.Key, element.Value);
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
                    this.m_Dict[element.Key] = element.Value;
                }

                Sender.Tell(msg.Elements.Count, Self);
            });

            Receive<GetElementMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                object element;

                if (msg.Key == null)
                    throw new ArgumentException("Key must be specified.");

                if (m_Dict.TryGetValue(msg.Key, out element))
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
                    if (m_Dict.TryGetValue(msg.Keys, out element))
                        result.Add(new KeyPair() { Key = key, Value = element });
                    else
                        result.Add(new KeyPair() { Key = key, Value = null });
                }
            });

         
            Receive<ContainsKeyMsg>(msg =>
            {
                var res = this.m_Dict.ContainsKey(msg.Key);

                Sender.Tell(res, Self);
            });

            Receive<GetCountMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                Sender.Tell(this.m_Dict.Count, Self);
            });

            Receive<BumUpWeakColumnsMsg>(msg =>
            {
                Console.WriteLine($"Received message: '{msg.GetType().Name}'");

                BumpUpWeakColumns(msg);
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
        private void InitializeColumns(InitColumnsMsg msg)
        {
            this.m_PartitionKey = msg.PartitionKey;

            m_Dict = new Dictionary<object, object>();

            Console.WriteLine($"{Self.Path} -  Received message: '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}, partitionKey: {this.m_PartitionKey}");

            for (int i = msg.MinKey; i <= msg.MaxKey; i++)
            {
                this.m_Dict[i] = new Column(this.m_Config.CellsPerColumn, i, this.m_Config.SynPermConnected, this.m_Config.NumInputs);
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

            Debug.WriteLine($"Init completed. {msg.PartitionKey} - {this.m_Dict.Count} - min={msg.MinKey}, max={msg.MaxKey}");

            Console.WriteLine($"{Self.Path} - Init completed. '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}");

            Sender.Tell(msg.MaxKey - msg.MinKey, Self);

            Console.WriteLine($"{Self.Path} -  Response on init message sent '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}");

        }


        /// <summary>
        /// Initialize all columns inside of partition and connect them to sensory input.
        /// It returns the average connected span of the partition.
        /// </summary>
        /// <param name="msg"></param>
        private void CreateAndConnectColumns(ConnectAndConfigureColumnsMsg msg)
        {
            DateTime startTime = DateTime.Now;

            Log(msg, Self, "Entered.");

            List<double> avgConnections = new List<double>();

            Random rnd;

            if (this.m_Config.RandomGenSeed > 0)
                rnd = new Random(this.m_Config.RandomGenSeed);
            else
                rnd = new Random();

            if (this.m_Dict.Count == 0)
            {

            }

            foreach (var element in this.m_Dict)
            {
                if (this.m_Config == null)
                    throw new ArgumentException($"HtmConfig must be set in the message.");

                int colIndx = (int)element.Key;

                // Gets RF
                var potential = HtmCompute.MapPotential(this.m_Config, colIndx, rnd);
                var column = (Column)this.m_Dict[colIndx];

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                //connectColumnToInputRF(c.HtmConfig, data.Potential, data.Column);
                column.CreatePotentialPool(this.m_Config, potential, -1);

                var perms = HtmCompute.InitSynapsePermanences(this.m_Config, potential, rnd);

                avgConnections.Add(HtmCompute.CalcAvgSpanOfConnectedSynapses(column, this.m_Config));

                //Log(msg, Self, $".:). {dict.Count}/{dict.Keys.Min()}/{dict.Keys.Min()} - duration: {(DateTime.Now - startTime).TotalSeconds}");

                HtmCompute.UpdatePermanencesForColumn(this.m_Config, perms, column, potential, true);
            }

            double avgConnectedSpan = ArrayUtils.Average(avgConnections.ToArray());

            Log(msg, Self, "Completed.");
            Sender.Tell(avgConnectedSpan, Self);
            Log(msg, Self, $"Response sent. {(DateTime.Now - startTime).TotalSeconds}");

        }

        private void Log(object msg, IActorRef aRef, string txt)
        {
            Console.WriteLine($"{DateTime.UtcNow} - {aRef.Path.Elements.Last()} - '{msg.GetType().Name}'. {txt}");

        }

        private void CalculateOverlap(CalculateOverlapMsg msg)
        {
            Console.WriteLine($"{Self.Path} - Received message: '{msg.GetType().Name}'");

            ConcurrentDictionary<int, int> overlaps = new ConcurrentDictionary<int, int>();

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(this.m_Dict, opts, (keyPair) =>
            {
                Column col = keyPair.Value as Column;

                var overlap = col.GetColumnOverlapp(msg.InputVector, this.m_Config.StimulusThreshold);

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

        void AdaptSynapses(AdaptSynapsesMsg msg)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = msg.ColumnKeys.Count
            };

            Parallel.ForEach(msg.ColumnKeys, opts, (colPair) =>
            {
                Column activeColumn = (Column)this.m_Dict[colPair.Key];
                //Pool pool = c.getPotentialPools().get(activeColumns[i]);
                Pool pool = activeColumn.ProximalDendrite.RFPool;
                double[] perm = pool.GetDensePermanences(this.m_Config.NumInputs);
                int[] indexes = pool.GetSparsePotential();
                ArrayUtils.RaiseValuesBy(msg.PermanenceChanges, perm);

                HtmCompute.UpdatePermanencesForColumn(this.m_Config, perm, activeColumn, indexes, true);
            });

            // We send this to ensure reliable messaging. No other result is required here.
            Sender.Tell(0, Self);
        }

        public void BumpUpWeakColumns(BumUpWeakColumnsMsg msg)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = msg.ColumnKeys.Count
            };

            Parallel.ForEach(msg.ColumnKeys, opts, (colPair) =>
            {
                Column weakColumn = (Column)m_Dict[colPair.Key];

                Pool pool = weakColumn.ProximalDendrite.RFPool;
                double[] perm = pool.GetSparsePermanences();
                ArrayUtils.RaiseValuesBy(this.m_Config.SynPermBelowStimulusInc, perm);
                int[] indexes = pool.GetSparsePotential();

                weakColumn.UpdatePermanencesForColumnSparse(this.m_Config, perm, indexes, true);
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
