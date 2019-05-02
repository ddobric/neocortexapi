using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedCompute
{
    public class SetValueMessage<TKey, TValue>
    {
        public TKey Key { get; set; }

        public TValue Value { get; set; }
    }
}
