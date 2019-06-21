using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    public class SbNodeActor
    {
        private Dictionary<object, object> dict = new Dictionary<object, object>();

        private HtmConfig config;

        public  MyProperty { get; set; }
        #region Private Methods

        public void PingActor()
        {

        }
        Receive((Action<PingNodeMsg>)(msg =>
            {
            Sender.Tell($"Ping back - {msg.Msg}", Self);
        }));

        /// <summary>
        /// Creates columns on the node.
        /// </summary>
        /// <param name="msg"></param>
        private void initializeColumns(InitColumnsMsg msg)
        {
            dict = new Dictionary<object, object>();

            Console.WriteLine($"{Self.Path} -  Received message: '{msg.GetType().Name}' - min={msg.MinKey}, max={msg.MaxKey}");

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
            Console.WriteLine($"{Self.Path} - Received message: '{msg.GetType().Name}'");

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

            Console.WriteLine($"{Self.Path} - '{msg.GetType().Name}' completed.");
            Sender.Tell(avgConnectedSpan, Self);
            Console.WriteLine($"{Self.Path} - '{msg.GetType().Name}' response sent.");
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
