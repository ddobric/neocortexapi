
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AkkaSb.Net;
using Microsoft.Extensions.Logging;

namespace NeoCortexApi.DistributedComputeLib
{


    public class HtmActor : ActorBase
    {
        private Dictionary<object, object> dict = new Dictionary<object, object>();

        private HtmConfig config;


        public HtmActor()
        {
            Receive<PingNodeMsg>((msg) =>
            {
                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                return $"Ping back - {msg.Msg}";
            });

            Receive<CreateDictNodeMsg>((msg) =>
            {
                this.config = msg.HtmAkkaConfig;

                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                return -1;
            });

            Receive<InitColumnsMsg>((msg) =>
            {
                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                var res = initializeColumns(msg);

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}'. Result: {res}");

                return res;
            });

            Receive<ConnectAndConfigureColumnsMsg>((msg) =>
            {
                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                var res =createAndConnectColumns(msg);

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}'. Result: {res}");

                return res;
            });

            Receive<CalculateOverlapMsg>((msg) =>
            {
                this.Logger?.LogInformation($"Received message: '{msg.GetType().Name}'");

                var res = calculateOverlap(msg);

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}'. Result: {res}");

                return res;
            });

            Receive<AdaptSynapsesMsg>((msg) =>
            {
                this.Logger?.LogInformation($"Started message: '{msg.GetType().Name}'");

                var res = adaptSynapses(msg);

                this.Logger?.LogInformation($"Completed message: '{msg.GetType().Name}'. Result: {res}");

                return res;

            });


            Receive<BumUpWeakColumnsMsg>((msg) =>
            {
                Console.WriteLine($"Started message: '{msg.GetType().Name}'");

                var res = bumpUpWeakColumns(msg);

                Console.WriteLine($"Completed message: '{msg.GetType().Name}'");

                return res;
            });
        }

        public override void Activated()
        {
            Console.WriteLine($"Actor '{this.GetType().Name}' activated.");
      
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
        private object initializeColumns(InitColumnsMsg msg)
        {
            dict = new Dictionary<object, object>();

            for (int i = msg.MinKey; i <= msg.MaxKey; i++)
            {
                this.dict[i] = new Column(this.config.CellsPerColumn, i, this.config.SynPermConnected, this.config.NumInputs);
            }

            return msg.MaxKey - msg.MinKey;
        }


        /// <summary>
        /// Initialize all columns inside of partition and connect them to sensory input.
        /// It returns the average connected span of the partition.
        /// </summary>
        /// <param name="msg"></param>
        private object createAndConnectColumns(ConnectAndConfigureColumnsMsg msg)
        {
            List<double> avgConnections = new List<double>();

            Random rnd;

            if (this.config.RandomGenSeed > 0)
                rnd = new Random(this.config.RandomGenSeed);
            else
                rnd = new Random();

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

                HtmCompute.UpdatePermanencesForColumn(this.config, perms, column, potential, true);
            }

            double avgConnectedSpan = ArrayUtils.average(avgConnections.ToArray());

            return avgConnectedSpan;
        }

        private object calculateOverlap(CalculateOverlapMsg msg)
        {
            Console.WriteLine($"Received message: '{msg.GetType().Name}'");

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

            return sortedRes;
        }

        private object adaptSynapses(AdaptSynapsesMsg msg)
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
            return 0;
        }

        private object bumpUpWeakColumns(BumUpWeakColumnsMsg msg)
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

            return 0;
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
