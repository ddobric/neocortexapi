
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    public class DistributedMemory
    {
        public IDistributedDictionary<int, Column> ColumnDictionary { get; set; }

        //public IDistributedDictionary<int, Pool> PoolDictionary { get; set; }
    }
}
