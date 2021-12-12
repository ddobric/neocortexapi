// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoCortexApi.Entities
{
    public interface IDistributedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>>
    {
        HtmConfig htmConfig { get; set; }

        void AddOrUpdate(ICollection<KeyPair> keyValuePairs);

        /// <summary>
        /// Gets the list of objects assotiated with keys.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        ICollection<KeyPair> GetObjects(TKey[] keys);

        public void Serialize(StreamWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// If imlementation of an object includes Actors, it should be merked with this interface.
    /// </summary>
    public interface IHtmDistCalculus
    {
        /// <summary>
        /// All required HTM configuration in serializable form.
        /// </summary>
        HtmConfig HtmConfig { get; set; }

        /// <summary>
        /// Gets number of nodes in distributed cluster.
        /// </summary>
        int Nodes { get; }

        /// <summary>
        /// Gets partitions (nodes) with assotiated indexes.
        /// </summary>
        /// <returns>
        /// </returns>
        //List<(int partId, int minKey, int maxKey)> GetPartitions();

        void InitializeColumnPartitionsDist(ICollection<KeyPair> keyValuePairs);

        List<double> ConnectAndConfigureInputsDist(HtmConfig htmConfig);

        int[] CalculateOverlapDist(int[] inputVector);

        void AdaptSynapsesDist(int[] inputVector, double[] permChanges, int[] activeColumns);

        void BumpUpWeakColumnsDist(int[] weakColumns);

        
    }
    
}
