// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace NeoCortexApi
{
    /// <summary>
    /// The parallel version of the SP on to of the actor model.
    /// </summary>
    public class SpatialPoolerParallel : SpatialPooler
    {
        private DistributedMemory distMemConfig;

        /// <summary>
        /// Performs the remote initialization ot the SP on actor nodes.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="distMem"></param>
        public override void InitMatrices(Connections c, DistributedMemory distMem)
        {
            IHtmDistCalculus remoteHtm = distMem?.ColumnDictionary as IHtmDistCalculus;
            if (remoteHtm == null)
                throw new ArgumentException("Must implement IHtmDistCalculus!");

            this.distMemConfig = distMem;

            SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.Memory;

            c.Memory = mem == null ? mem = new SparseObjectMatrix<Column>(c.HtmConfig.ColumnDimensions, dict: distMem.ColumnDictionary) : mem;

            c.HtmConfig.InputMatrix = new SparseBinaryMatrix(c.HtmConfig.InputDimensions);

            // Initiate the topologies
            c.HtmConfig.ColumnTopology = new Topology(c.HtmConfig.ColumnDimensions);
            c.HtmConfig.InputTopology = new Topology(c.HtmConfig.InputDimensions);

            //Calculate numInputs and numColumns
            int numInputs = c.HtmConfig.InputMatrix.GetMaxIndex() + 1;
            int numColumns = c.Memory.GetMaxIndex() + 1;
            if (numColumns <= 0)
            {
                throw new ArgumentException("Invalid number of columns: " + numColumns);
            }
            if (numInputs <= 0)
            {
                throw new ArgumentException("Invalid number of inputs: " + numInputs);
            }
            c.HtmConfig.NumInputs = numInputs;
            c.HtmConfig.NumColumns = numColumns;

            if (distMem != null)
            {
                //var distHtmCla = distMem.ColumnDictionary as HtmSparseIntDictionary<Column>;
                var distHtmCla = distMem.ColumnDictionary;// as ActorSbDistributedDictionaryBase<Column>;

                distHtmCla.htmConfig = c.HtmConfig;
            }

            //
            // Fill the sparse matrix with column objects
            //var numCells = c.getCellsPerColumn();

            // var partitions = mem.GetPartitions();


            List<KeyPair> colList = new List<KeyPair>();
            for (int i = 0; i < numColumns; i++)
            {
                //colList.Add(new KeyPair() { Key = i, Value = new Column(numCells, i, c.getSynPermConnected(), c.NumInputs) });
                colList.Add(new KeyPair() { Key = i, Value = c.HtmConfig });
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            remoteHtm.InitializeColumnPartitionsDist(colList);

            //mem.set(colList);

            sw.Stop();
            //c.setPotentialPools(new SparseObjectMatrix<Pool>(c.getMemory().getDimensions(), dict: distMem == null ? null : distMem.PoolDictionary));

            Debug.WriteLine($" Upload time: {sw.ElapsedMilliseconds}");

            // Already initialized by creating of columns.
            //c.setConnectedMatrix(new SparseBinaryMatrix(new int[] { numColumns, numInputs }));

            //Initialize state meta-management statistics
            c.HtmConfig.OverlapDutyCycles = new double[numColumns];
            c.HtmConfig.ActiveDutyCycles = new double[numColumns];
            c.HtmConfig.MinOverlapDutyCycles = new double[numColumns];
            c.HtmConfig.MinActiveDutyCycles = new double[numColumns];
            c.BoostFactors = (new double[numColumns]);
            ArrayUtils.FillArray(c.BoostFactors, 1);
        }

        /// <summary>
        /// Implements multinode initialization of pooler.
        /// It creates the pool of potentially connected synapses on ProximalDendrite segment.
        /// </summary>
        /// <param name="c"></param>
        protected override void ConnectAndConfigureInputs(Connections c)
        {
            IHtmDistCalculus remoteHtm = this.distMemConfig.ColumnDictionary as IHtmDistCalculus;
            if (remoteHtm == null)
                throw new ArgumentException("");

            List<double> avgSynapsesConnected = remoteHtm.ConnectAndConfigureInputsDist(c.HtmConfig);


            //List<KeyPair> colList = new List<KeyPair>();

            //ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            //int numColumns = c.NumColumns;

            //// Parallel implementation of initialization
            //ParallelOptions opts = new ParallelOptions();
            ////int synapseCounter = 0;



            //Parallel.For(0, numColumns, opts, (indx) =>
            //{

            //    //Random rnd = new Random(42);

            //    //int i = (int)indx;
            //    //var data = new ProcessingDataParallel();

            //    //// Gets RF
            //    //data.Potential = HtmCompute.MapPotential(c.HtmConfig, i, rnd);
            //    //data.Column = c.getColumn(i);

            //    //// This line initializes all synases in the potential pool of synapses.
            //    //// It creates the pool on proximal dendrite segment of the column.
            //    //// After initialization permancences are set to zero.
            //    ////connectColumnToInputRF(c.HtmConfig, data.Potential, data.Column);
            //    //data.Column.CreatePotentialPool(c.HtmConfig, data.Potential, -1);

            //    ////Interlocked.Add(ref synapseCounter, data.Column.ProximalDendrite.Synapses.Count);

            //    ////colList.Add(new KeyPair() { Key = i, Value = column });

            //    //data.Perm = HtmCompute.InitSynapsePermanences(c.HtmConfig, data.Potential, rnd);

            //    //data.AvgConnected = GetAvgSpanOfConnectedSynapses(c, i);

            //    //HtmCompute.UpdatePermanencesForColumn(c.HtmConfig, data.Perm, data.Column, data.Potential, true);

            //    if (!colList2.TryAdd(i, new KeyPair() { Key = i, Value = data }))
            //    {

            //    }
            //});

            ////c.setProximalSynapseCount(synapseCounter);

            //List<double> avgSynapsesConnected = new List<double>();

            //foreach (var item in colList2.Values)
            ////for (int i = 0; i < numColumns; i++)
            //{
            //    int i = (int)item.Key;

            //    ProcessingDataParallel data = (ProcessingDataParallel)item.Value;
            //    //ProcessingData data = new ProcessingData();

            //    // Debug.WriteLine(i);
            //    //data.Potential = mapPotential(c, i, c.isWrapAround());

            //    //var st = string.Join(",", data.Potential);
            //    //Debug.WriteLine($"{i} - [{st}]");

            //    //var counts = c.getConnectedCounts();

            //    //for (int h = 0; h < counts.getDimensions()[0]; h++)
            //    //{
            //    //    // Gets the synapse mapping between column-i with input vector.
            //    //    int[] slice = (int[])counts.getSlice(h);
            //    //    Debug.Write($"{slice.Count(y => y == 1)} - ");
            //    //}
            //    //Debug.WriteLine(" --- ");
            //    // Console.WriteLine($"{i} - [{String.Join(",", ((ProcessingData)item.Value).Potential)}]");

            //    // This line initializes all synases in the potential pool of synapses.
            //    // It creates the pool on proximal dendrite segment of the column.
            //    // After initialization permancences are set to zero.
            //    //var potPool = data.Column.createPotentialPool(c, data.Potential);
            //    //connectColumnToInputRF(c, data.Potential, data.Column);

            //    //data.Perm = initPermanence(c.getSynPermConnected(), c.getSynPermMax(),
            //    //      c.getRandom(), c.getSynPermTrimThreshold(), c, data.Potential, data.Column, c.getInitConnectedPct());

            //    //updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

            //    avgSynapsesConnected.Add(data.AvgConnected);

            //    colList.Add(new KeyPair() { Key = i, Value = data.Column });
            //}

            SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.Memory;

            //if (mem.IsRemotelyDistributed)
            //{
            //    // Pool is created and attached to the local instance of Column.
            //    // Here we need to update the pool on remote Column instance.
            //    mem.set(colList);
            //}

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            UpdateInhibitionRadius(c, avgSynapsesConnected);
        }


        /// <summary>
        /// Starts distributed calculation of overlaps.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="inputVector">Overlap of every column.</param>
        /// <returns></returns>
        public override int[] CalculateOverlap(Connections c, int[] inputVector)
        {
            IHtmDistCalculus remoteHtm = this.distMemConfig.ColumnDictionary as IHtmDistCalculus;
            if (remoteHtm == null)
                throw new ArgumentException("disMemConfig is not of type IRemotelyDistributed!");

            //c.getColumn(0).GetColumnOverlapp(inputVector, c.StimulusThreshold);

            int[] overlaps = remoteHtm.CalculateOverlapDist(inputVector);

            var overlapsStr = Helpers.StringifyVector(overlaps);
            // Debug.WriteLine("overlap: " + overlapsStr);

            return overlaps;
        }

        public override void AdaptSynapses(Connections c, int[] inputVector, int[] activeColumns)
        {
            IHtmDistCalculus remoteHtm = this.distMemConfig.ColumnDictionary as IHtmDistCalculus;
            if (remoteHtm == null)
                throw new ArgumentException("disMemConfig is not of type IRemotelyDistributed!");

            // Get all indicies of input vector, which are set on '1'.
            var inputIndices = ArrayUtils.IndexWhere(inputVector, inpBit => inpBit > 0);

            double[] permChanges = new double[c.HtmConfig.NumInputs];

            // First we initialize all permChanges to minimum decrement values,
            // which are used in a case of none-connections to input.
            ArrayUtils.InitArray(permChanges, -1 * c.HtmConfig.SynPermInactiveDec);

            // Then we update all connected permChanges to increment values for connected values.
            // Permanences are set in conencted input bits to default incremental value.
            ArrayUtils.SetIndexesTo(permChanges, inputIndices.ToArray(), c.HtmConfig.SynPermActiveInc);

            remoteHtm.AdaptSynapsesDist(inputVector, permChanges, activeColumns);
        }

        public override void BoostColsWithLowOverlap(Connections c)
        {
            IHtmDistCalculus remoteHtm = this.distMemConfig.ColumnDictionary as IHtmDistCalculus;
            if (remoteHtm == null)
                throw new ArgumentException("disMemConfig is not of type IRemotelyDistributed!");

            var weakColumns = c.Memory.Get1DIndexes().Where(i => c.HtmConfig.OverlapDutyCycles[i] < c.HtmConfig.MinOverlapDutyCycles[i]).ToArray();
            if (weakColumns.Length > 0)
                remoteHtm.BumpUpWeakColumnsDist(weakColumns);
        }

        class ProcessingDataParallel
        {
            public int[] Potential { get; set; }

            public Column Column { get; set; }

            public double[] Perm { get; internal set; }

            public double AvgConnected { get; set; }
        }
    }
}

