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
        public static DistributedMemory GetMemory(Parameters htmParams = null)
        {
            if (htmParams == null)
                return GetInMemoryDictionary();
            else
                return GetDistributedDictionary(htmParams);
        }

        public static DistributedMemory GetDistributedDictionary(Parameters htmParams)
        {
            var cfg = Helpers.DefaultHtmSparseIntDictionaryConfig;
            cfg.HtmActorConfig = new HtmConfig()
            {
                ColumnDimensions = (int[])htmParams[KEY.COLUMN_DIMENSIONS],
                InputDimensions = (int[])htmParams[KEY.INPUT_DIMENSIONS],
                IsWrapAround = (bool)htmParams[KEY.WRAP_AROUND],
                PotentialPct = (double)htmParams[KEY.POTENTIAL_PCT]
            };
        
            return new DistributedMemory()
            {
                ColumnDictionary = new HtmSparseIntDictionary<Column>(cfg),
                //PoolDictionary = new HtmSparseIntDictionary<Pool>(cfg),
            };
        }

        public static DistributedMemory GetInMemoryDictionary()
        {
            return new DistributedMemory()
            {
                ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1),
                //PoolDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Pool>(1),
            };
        }


        /// <summary>
        /// Creates apprpriate instance of SpatialPooler.
        /// </summary>
        /// <param name="poolerImplementation"></param>
        /// <returns></returns>
        internal static SpatialPooler CreatePooler(int poolerImplementation)
        {
            SpatialPooler sp;
            if (poolerImplementation == 0)
                sp = new SpatialPooler();
            else if (poolerImplementation == 1)
                sp = new SpatialPoolerMT();
            else
                throw new NotImplementedException();

            return sp;
        }

        internal static void InitPooler(PoolerMode poolerMode, SpatialPooler sp, Connections mem, Parameters parameters = null)
        {
            if (poolerMode == PoolerMode.Multinode)
                sp.init(mem, UnitTestHelpers.GetMemory(parameters));
            else if (poolerMode == PoolerMode.Multicore)
                sp.init(mem, UnitTestHelpers.GetMemory());
            else
                sp.init(mem);
        }

        /// <summary>
        /// Creates pooler instance.
        /// </summary>
        /// <param name="poolerMode"></param>
        /// <returns></returns>
        public static SpatialPooler CreatePooler(PoolerMode poolerMode)
        {
            SpatialPooler sp;

            if (poolerMode == PoolerMode.Multinode)
                sp = new SpatialPoolerParallel();
            else if (poolerMode == PoolerMode.Multicore)
                sp = new SpatialPoolerMT();
            else
                sp = new SpatialPooler();

            return sp;
        }
    }


    public enum PoolerMode
    {
        SingleThreaded,

        Multicore,

        Multinode
    }
}
