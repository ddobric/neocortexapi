using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedCompute
{
    public class GetValueMessage<TKey>
    {
        public TKey Key{ get; set; }
    }
}
