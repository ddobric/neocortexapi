using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi.Entities;

namespace NeoCortexApi
{
    public class SpatialPoolerMT : SpatialPooler
    {
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
                var data = new ProcessingData();

                // Gets RF
                data.Potential = mapPotential(c, i, c.isWrapAround());
                data.Column = c.getColumn(i);

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                connectColumnToInputRF(c, data.Potential, data.Column);

                //Interlocked.Add(ref synapseCounter, data.Column.ProximalDendrite.Synapses.Count);

                //colList.Add(new KeyPair() { Key = i, Value = column });

                data.Perm = initPermanence(c, data.Potential, data.Column);

                updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                if (!colList2.TryAdd(i, new KeyPair() { Key = i, Value = data }))
                {

                }
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
            updateInhibitionRadius(c);
        }

        class ProcessingData
        {
            public int[] Potential { get; set; }

            public Column Column { get; set; }
            public double[] Perm { get; internal set; }
        }
    }
}
