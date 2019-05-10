using NeoCortexApi;
using NeoCortexApi.DistributedCompute;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnitTestsProject
{
    public class UnitTestHelpers
    {
        public static DistributedMemory GetMemory(int numOfColumns)
        {
            return GetInMemoryDictionary();
            //return GetDistributedDictionary(numOfColumns);
        }

        public static DistributedMemory GetDistributedDictionary(int numOfColumns)
        {
            var cfg = Helpers.DefaultHtmSparseIntDictionaryConfig;
            cfg.NumColumns = numOfColumns;

            return new DistributedMemory()
            {                
                ColumnDictionary = new HtmSparseIntDictionary<Column>(cfg),
                PoolDictionary = new HtmSparseIntDictionary<Pool>(cfg),
            };
        }

        public static DistributedMemory GetInMemoryDictionary()
        {
            return new DistributedMemory()
            {
                ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1),
                PoolDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Pool>(1),
            };
        }
    }
}
