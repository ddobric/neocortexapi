using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi.Entities;
using System.Linq;
using System.Diagnostics;
using NeoCortexApi.Utility;
using NeoCortexApi.DistributedCompute;

namespace NeoCortexApi
{
    public class SpatialPoolerParallel : SpatialPooler
    {
        private DistributedMemory distMemConfig;

        public override void InitMatrices(Connections c, DistributedMemory distMem)
        {
            this.distMemConfig = distMem;

            SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.getMemory();

            c.setMemory(mem == null ? mem = new SparseObjectMatrix<Column>(c.getColumnDimensions(), dict: distMem == null ? null : distMem.ColumnDictionary) : mem);

            c.setInputMatrix(new SparseBinaryMatrix(c.getInputDimensions()));

            // Initiate the topologies
            c.setColumnTopology(new Topology(c.getColumnDimensions()));
            c.setInputTopology(new Topology(c.getInputDimensions()));

            //Calculate numInputs and numColumns
            int numInputs = c.getInputMatrix().getMaxIndex() + 1;
            int numColumns = c.getMemory().getMaxIndex() + 1;
            if (numColumns <= 0)
            {
                throw new ArgumentException("Invalid number of columns: " + numColumns);
            }
            if (numInputs <= 0)
            {
                throw new ArgumentException("Invalid number of inputs: " + numInputs);
            }
            c.NumInputs = numInputs;
            c.setNumColumns(numColumns);

            //
            // Fill the sparse matrix with column objects
            var numCells = c.getCellsPerColumn();

            var partitions = mem.GetPartitions();

            //Parallel.ForEach(pages, opts, (keyValPair) =>
            //{

            //}
                List<KeyPair> colList = new List<KeyPair>();
            for (int i = 0; i < numColumns; i++)
            {
                colList.Add(new KeyPair() { Key = i, Value = new Column(numCells, i, c.getSynPermConnected(), c.NumInputs) });
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            mem.set(colList);

            sw.Stop();
            //c.setPotentialPools(new SparseObjectMatrix<Pool>(c.getMemory().getDimensions(), dict: distMem == null ? null : distMem.PoolDictionary));

            Debug.WriteLine($" Upload time: {sw.ElapsedMilliseconds}");

            c.setConnectedMatrix(new SparseBinaryMatrix(new int[] { numColumns, numInputs }));

            //Initialize state meta-management statistics
            c.setOverlapDutyCycles(new double[numColumns]);
            c.setActiveDutyCycles(new double[numColumns]);
            c.setMinOverlapDutyCycles(new double[numColumns]);
            c.setMinActiveDutyCycles(new double[numColumns]);
            c.BoostFactors = (new double[numColumns]);
            ArrayUtils.fillArray(c.BoostFactors, 1);
        }

        /// <summary>
        /// Implements muticore initialization of pooler.
        /// </summary>
        /// <param name="c"></param>
        protected override void ConnectAndConfigureInputs(Connections c)
        {
            List<KeyPair> colList = new List<KeyPair>();

            ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            int numColumns = c.NumColumns;

            // Parallel implementation of initialization
            ParallelOptions opts = new ParallelOptions();
            //int synapseCounter = 0;

            Parallel.For(0, numColumns, opts, (indx) =>
            {
                int i = (int)indx;
                var data = new ProcessingDataParallel();

                // Gets RF
                data.Potential = MapPotential(c, i, c.isWrapAround());
                data.Column = c.getColumn(i);

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                connectColumnToInputRF(c, data.Potential, data.Column);

                //Interlocked.Add(ref synapseCounter, data.Column.ProximalDendrite.Synapses.Count);

                //colList.Add(new KeyPair() { Key = i, Value = column });

                data.Perm = initSynapsePermanencesForColumn(c, data.Potential, data.Column);

                data.AvgConnected = GetAvgSpanOfConnectedSynapses(c, i);

                updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                if (!colList2.TryAdd(i, new KeyPair() { Key = i, Value = data }))
                {

                }
            });

            //c.setProximalSynapseCount(synapseCounter);

            List<double> avgSynapsesConnected = new List<double>();

            foreach (var item in colList2.Values)
            //for (int i = 0; i < numColumns; i++)
            {
                int i = (int)item.Key;

                ProcessingDataParallel data = (ProcessingDataParallel)item.Value;
                //ProcessingData data = new ProcessingData();

                // Debug.WriteLine(i);
                //data.Potential = mapPotential(c, i, c.isWrapAround());

                //var st = string.Join(",", data.Potential);
                //Debug.WriteLine($"{i} - [{st}]");

                //var counts = c.getConnectedCounts();

                //for (int h = 0; h < counts.getDimensions()[0]; h++)
                //{
                //    // Gets the synapse mapping between column-i with input vector.
                //    int[] slice = (int[])counts.getSlice(h);
                //    Debug.Write($"{slice.Count(y => y == 1)} - ");
                //}
                //Debug.WriteLine(" --- ");
                // Console.WriteLine($"{i} - [{String.Join(",", ((ProcessingData)item.Value).Potential)}]");

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                //var potPool = data.Column.createPotentialPool(c, data.Potential);
                //connectColumnToInputRF(c, data.Potential, data.Column);

                //data.Perm = initPermanence(c.getSynPermConnected(), c.getSynPermMax(),
                //      c.getRandom(), c.getSynPermTrimThreshold(), c, data.Potential, data.Column, c.getInitConnectedPct());

                //updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                avgSynapsesConnected.Add(data.AvgConnected);

                colList.Add(new KeyPair() { Key = i, Value = data.Column });
            }

            SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.getMemory();

            if (mem.IsRemotelyDistributed)
            {
                // Pool is created and attached to the local instance of Column.
                // Here we need to update the pool on remote Column instance.
                mem.set(colList);
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            updateInhibitionRadius(c, avgSynapsesConnected);
        }

        /*
        /// <summary>
        /// Implements single threaded (originally based on JAVA implementation) initialization of SP.
        /// </summary>
        /// <param name="c"></param>
        protected override void ConnectAndConfigureInputs(Connections c)
        {
            List<KeyPair> colList = new List<KeyPair>();
            ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            int numColumns = c.NumColumns;

            // We need dictionary , which implements Akk paralellism.
            HtmSparseIntDictionary<Column> colDict = this.distMemConfig.ColumnDictionary as HtmSparseIntDictionary<Column>;
            if (colDict == null)
                throw new ArgumentException($"ColumnDictionary must be of type {nameof(HtmSparseIntDictionary<Column>)}!");

            // Parallel implementation of initialization
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = colDict.Nodes;

            //int synapseCounter = 0;

            SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.getMemory();
            
            if (mem.IsRemotelyDistributed == false)
                throw new ArgumentException("Column memory matrix 'SparseObjectMatrix<Column>' must be remotely distributed.");

            var partitions = mem.GetPartitions();
            
            Parallel.ForEach(partitions, opts, (keyValPair) =>
            {
                //// We get here keys grouped to actors, which host partitions.
                //var partitions = mem.GetPartitionsForKeyset(keyValuePairs);

                ////int i = keyValPair.Key;

                ////mem.GetObjects(keyValPair.ToArray());

                //var data = new ProcessingData();

               
                //// Gets RF
                //data.Potential = mapPotential(c, i, c.isWrapAround());

                //mem.GetObjects(keyValPair.Value.ToArray());

                //data.Column = c.getColumn(i);

                //Parallel.ForEach(pages, opts, (keyValPair) =>
                //{

                
                //    // This line initializes all synases in the potential pool of synapses.
                //    // It creates the pool on proximal dendrite segment of the column.
                //    // After initialization permancences are set to zero.
                //    connectColumnToInputRF(c, data.Potential, data.Column);

                ////Interlocked.Add(ref synapseCounter, data.Column.ProximalDendrite.Synapses.Count);

                ////colList.Add(new KeyPair() { Key = i, Value = column });

                //data.Perm = initSynapsePermanencesForColumn(c, data.Potential, data.Column);

                //updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                //if (!colList2.TryAdd(i, new KeyPair() { Key = i, Value = data }))
                //{

                //}
                //});
            });

            //c.setProximalSynapseCount(synapseCounter);

            foreach (var item in colList2.Values)
            //for (int i = 0; i < numColumns; i++)
            {
                int i = (int)item.Key;

                ProcessingData data = (ProcessingData)item.Value;
                //ProcessingData data = new ProcessingData();

                // Debug.WriteLine(i);
                //data.Potential = mapPotential(c, i, c.isWrapAround());

                //var st = string.Join(",", data.Potential);
                //Debug.WriteLine($"{i} - [{st}]");

                //var counts = c.getConnectedCounts();

                //for (int h = 0; h < counts.getDimensions()[0]; h++)
                //{
                //    // Gets the synapse mapping between column-i with input vector.
                //    int[] slice = (int[])counts.getSlice(h);
                //    Debug.Write($"{slice.Count(y => y == 1)} - ");
                //}
                //Debug.WriteLine(" --- ");
                // Console.WriteLine($"{i} - [{String.Join(",", ((ProcessingData)item.Value).Potential)}]");

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                //var potPool = data.Column.createPotentialPool(c, data.Potential);
                //connectColumnToInputRF(c, data.Potential, data.Column);

                //data.Perm = initPermanence(c.getSynPermConnected(), c.getSynPermMax(),
                //      c.getRandom(), c.getSynPermTrimThreshold(), c, data.Potential, data.Column, c.getInitConnectedPct());

                //updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                colList.Add(new KeyPair() { Key = i, Value = data.Column });
            }


            if (mem.IsRemotelyDistributed)
            {
                // Pool is created and attached to the local instance of Column.
                // Here we need to update the pool on remote Column instance.
                mem.set(colList);
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            updateInhibitionRadius(c);
        }
        */


        /// <summary>
        /// Does paging inside of partition. Every page will contain items (kays) from same partition.
        /// WE DO NOT SUPPORT PAGING INSIDE OF PARTITION!
        /// NUMBER OF PARTITIONS PER NODE MUST BE DESIGNED TO AVOID PAGING!
        /// </summary>
        /// <param name="partitions"></param>
        /// <returns></returns>
        //public static List<Dictionary<int, List<int>>> SplitPartitionsToPages(int pageSize, IDictionary<int, List<int>> partitions)
        //{
        //    List<Dictionary<int, List<int>>> pages = new List<Dictionary<int, List<int>>>();

        //    foreach (var keyPair in partitions)
        //    {
        //        int alreadyProcessed = 0;

        //        while (true)
        //        {
        //            var lst = new List<int>();

        //            foreach (var key in keyPair.Value.Skip(alreadyProcessed).Take(pageSize))
        //            {
        //                lst.Add(key);
        //                alreadyProcessed++;
        //            }

        //            if (lst.Count > 0)
        //            {
        //                var d = new Dictionary<int, List<int>>();
        //                d.Add(keyPair.Key, lst);
        //                pages.Add(d);
        //            }
        //            else
        //                break;
        //        }              
        //    }

        //    return pages;
        //}

        class ProcessingDataParallel
        {
            public int[] Potential { get; set; }

            public Column Column { get; set; }

            public double[] Perm { get; internal set; }

            public double AvgConnected { get; set; }
        }
    }
}

