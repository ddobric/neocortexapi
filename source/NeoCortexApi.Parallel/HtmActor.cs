// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApi.DistributedComputeLib
{
    public class HtmActor : ActorBase
    {
        public Dictionary<object, object> Dict = new Dictionary<object, object>();

        public HtmConfig HtmConfig;

        public HtmActor(ActorId id) : base(id)
        {
            Receive<PingNodeMsg>((msg) =>
            {
                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                return $"Ping back - {msg.Msg}";
            });

            Receive<CreateDictNodeMsg>((msg) =>
            {
                this.HtmConfig = msg.HtmAkkaConfig;

                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                return -1;
            });

            Receive<InitColumnsMsg>((msg) =>
            {
                this.Logger?.LogDebug($"Received message: '{msg.GetType().Name}', Id: {this.Id}");

                var res = InitializeColumns(msg);

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}'. min:{msg.MinKey}, max:{msg.MaxKey} ,Column range: {res}, Hashcode: {this.GetHashCode()}, Elements: {this.Dict.Count}, Id: {this.Id}");

                return res;
            });

            Receive<ConnectAndConfigureColumnsMsg>((msg) =>
            {
                this.Logger?.LogDebug($"{Id} - Received message: '{msg.GetType().Name}',  dict: {Dict.Count}, Id: {this.Id}");

                var res = CreateAndConnectColumns(msg);

                if (Dict.Count == 0)
                {

                }

                this.Logger?.LogInformation($"{Id} - Completed message: '{msg.GetType().Name}'. Avg. col. span: {res}, Hashcode: {this.GetHashCode()}, dict: {Dict.Count}, Id: {this.Id}");

                return res;
            });

            Receive<CalculateOverlapMsg>((msg) =>
            {
                this.Logger?.LogDebug($"Received message: '{msg.GetType().Name}'");

                var res = CalculateOverlap(msg);

                if (res.Count == 0)
                {

                }

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}', Hashcode: {this.GetHashCode()}, Result: {res.Count}, Id={this.Id} ");

                return res;
            });

            Receive<AdaptSynapsesMsg>((msg) =>
            {
                this.Logger?.LogDebug($"Started message: '{msg.GetType().Name}'");

                var res = AdaptSynapses(msg);

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}'. Result: {res}");

                return res;

            });


            Receive<BumUpWeakColumnsMsg>((msg) =>
            {
                this.Logger?.LogDebug($"Started message: '{msg.GetType().Name}'");

                var res = BumpUpWeakColumns(msg);

                Console.WriteLine($"Completed message: '{msg.GetType().Name}'");

                return res;
            });
        }

        public override void Activated()
        {
            Console.WriteLine($"Actor '{this.GetType().Name}' activated. Id: {this.Id}");

        }

        public override void DeActivated()
        {
            Console.WriteLine($"Actor '{this.GetType().Name}' deactivated.");
        }

        #region Private Methods
        /// <summary>
        /// Creates columns on the node.
        /// </summary>
        /// <param name="msg"></param>
        private int InitializeColumns(InitColumnsMsg msg)
        {
            Dict = new Dictionary<object, object>();

            for (int i = msg.MinKey; i <= msg.MaxKey; i++)
            {
                this.Dict[i] = new Column(this.HtmConfig.CellsPerColumn, i, this.HtmConfig.SynPermConnected, this.HtmConfig.NumInputs);
            }

            return msg.MaxKey - msg.MinKey + 1;
        }


        /// <summary>
        /// Initialize all columns inside of partition and connect them to sensory input.
        /// It returns the average connected span of the partition.
        /// </summary>
        /// <param name="msg"></param>
        //TODO remove unnecessary argument
        private double CreateAndConnectColumns(ConnectAndConfigureColumnsMsg msg)
        {
            Debug.Write(".");
            List<double> avgConnections = new List<double>();

            Random rnd;

            if (this.HtmConfig.RandomGenSeed > 0)
                rnd = new Random(this.HtmConfig.RandomGenSeed);
            else
                rnd = new Random();

            foreach (var element in this.Dict)
            {
                if (this.HtmConfig == null)
                    throw new ArgumentException($"HtmConfig must be set in the message.");

                int colIndx = -1;

                Column column;

                if (element.Key is string)
                {
                    if (!int.TryParse(element.Key as string, out colIndx))
                        throw new ArgumentException($"The key must be of type 'int' or string convertable to 'int");

                    column = (Column)this.Dict[element.Key];
                }
                else
                {
                    colIndx = (int)element.Key;
                    column = (Column)this.Dict[colIndx];
                }

                // Gets RF
                var potential = HtmCompute.MapPotential(this.HtmConfig, colIndx, rnd);


                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                //connectColumnToInputRF(c.HtmConfig, data.Potential, data.Column);
                column.CreatePotentialPool(this.HtmConfig, potential, -1);

                var perms = HtmCompute.InitSynapsePermanences(this.HtmConfig, potential, rnd);

                avgConnections.Add(HtmCompute.CalcAvgSpanOfConnectedSynapses(column, this.HtmConfig));

                HtmCompute.UpdatePermanencesForColumn(this.HtmConfig, perms, column, potential, true);
            }

            Debug.Write(".");

            double avgConnectedSpan = ArrayUtils.Average(avgConnections.ToArray());

            Debug.Write(".");
            return avgConnectedSpan;
        }

        private List<KeyPair> CalculateOverlap(CalculateOverlapMsg msg)
        {
            Console.WriteLine($"Received message: '{msg.GetType().Name}'");

            ConcurrentDictionary<int, int> overlaps = new ConcurrentDictionary<int, int>();

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(this.Dict, opts, (keyPair) =>
            {
                Column col = keyPair.Value as Column;

                var overlap = col.CalcMiniColumnOverlap(msg.InputVector, this.HtmConfig.StimulusThreshold);

                overlaps.TryAdd(keyPair.Key is string ? int.Parse(keyPair.Key as string) : (int)keyPair.Key, overlap);
            });

            List<KeyPair> result = new List<KeyPair>();
            foreach (var item in overlaps)
            {
                result.Add(new KeyPair { Key = item.Key, Value = item.Value });
            }

            var sortedRes = result.OrderBy(k => k.Key).ToList();

            //Console.Write($"o = {sortedRes.Count(p => (int)p.Value > 0)}");
            if (sortedRes.Count < this.Dict.Count)
            {

            }

            return sortedRes;
        }

        private object AdaptSynapses(AdaptSynapsesMsg msg)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = msg.ColumnKeys.Count
            };

            Parallel.ForEach(msg.ColumnKeys, opts, (colPair) =>
            {
                Column activeColumn = GetColumn(colPair.Key);
                //Pool pool = c.getPotentialPools().get(activeColumns[i]);
                Pool pool = activeColumn.ProximalDendrite.RFPool;
                double[] perm = pool.GetDensePermanences(this.HtmConfig.NumInputs);
                int[] indexes = pool.GetSparsePotential();
                ArrayUtils.RaiseValuesBy(msg.PermanenceChanges, perm);

                HtmCompute.UpdatePermanencesForColumn(this.HtmConfig, perm, activeColumn, indexes, true);
            });

            // We send this to ensure reliable messaging. No other result is required here.
            return 0;
        }

        private object BumpUpWeakColumns(BumUpWeakColumnsMsg msg)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = msg.ColumnKeys.Count
            };

            Parallel.ForEach(msg.ColumnKeys, opts, (colPair) =>
            {
                Column weakColumn = GetColumn(colPair.Key);

                Pool pool = weakColumn.ProximalDendrite.RFPool;
                double[] perm = pool.GetSparsePermanences();
                ArrayUtils.RaiseValuesBy(this.HtmConfig.SynPermBelowStimulusInc, perm);
                int[] indexes = pool.GetSparsePotential();

                weakColumn.UpdatePermanencesForColumnSparse(this.HtmConfig, perm, indexes, true);
            });

            return 0;
        }

        private Column GetColumn(object key)
        {
            if (this.Dict.ContainsKey(key.ToString()))
                return (Column)Dict[key.ToString()];
            else if (this.Dict.ContainsKey((int)(long)key))
                return (Column)Dict[(int)(long)key];
            else
                return (Column)Dict[key];
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
