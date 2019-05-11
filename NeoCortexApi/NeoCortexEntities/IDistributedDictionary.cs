using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    public interface IDistributedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>>
    {
        void AddOrUpdate(ICollection<KeyPair> keyValuePairs);
    }

    /// <summary>
    /// If imlementation of an object includes Actors, it should be merked with this interface.
    /// </summary>
    public interface IRemotelyDistributed
    {
        /// <summary>
        /// Gets number of nodes in distributed cluster.
        /// </summary>
        int Nodes { get; }
    }
}
