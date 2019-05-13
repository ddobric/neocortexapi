using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi.Entities;

namespace NeoCortexApi
{
    public class SpatialPoolerST : SpatialPooler
    {
        /// <summary>
        /// Implements single threaded (originally based on JAVA implementation) initialization of SP.
        /// </summary>
        /// <param name="c"></param>
        protected override void ConnectAndConfigureInputs(Connections c)
        {
            List<KeyPair> colList = new List<KeyPair>();
            ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            int numColumns = c.NumColumns;

            for (int i = 0; i < numColumns; i++)
            {
                // Gets RF
                int[] potential = mapPotential(c, i, c.isWrapAround());

                Column column = c.getColumn(i);

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                connectColumnToInputRF(c, potential, column);

                //c.getPotentialPools().set(i, potPool);

                colList.Add(new KeyPair() { Key = i, Value = column });

                double[] perm = initPermanence(c, potential, column);

                updatePermanencesForColumn(c, perm, column, potential, true);
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            updateInhibitionRadius(c);
        }

    }
}
