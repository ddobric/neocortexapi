using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    public interface IDistributedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>>
    {
    }

    /// <summary>
    /// If imlementation of an object includes Actors, it should be merked with this interface.
    /// </summary>
    public interface IRemotelyDistributed
    {

    }
}
