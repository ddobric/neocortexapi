using Microsoft.Extensions.Logging;
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
        public static DistributedMemory GetMemory(HtmConfig htmConfig = null)
        {
            if (htmConfig == null)
                return GetInMemoryDictionary();
            else
                return GetDistributedDictionary(htmConfig);
        }

        public static ILogger GetLogger(string logger="UnitTest")
        {
            LoggerFactory factory = new LoggerFactory();

            factory.AddConsole(LogLevel.Information);
            factory.AddDebug(LogLevel.Information);

            return factory.CreateLogger(logger);

        }

        public static DistributedMemory GetDistributedDictionary(HtmConfig htmConfig)
        {
            var cfg = Helpers.DefaultSbConfig;
           
            return new DistributedMemory()
            {
                ColumnDictionary = new ActorSbDistributedDictionaryBase<Column>(cfg, UnitTestHelpers.GetLogger()),

                //ColumnDictionary = new HtmSparseIntDictionary<Column>(cfg),
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
        /// Creates appropriate instance of SpatialPooler.
        /// </summary>
        /// <param name="poolerImplementation"></param>
        /// <returns></returns>
        internal static SpatialPooler CreatePooler(PoolerMode poolerMode)
        {
            SpatialPooler sp;
            if (poolerMode == PoolerMode.SingleThreaded)
                sp = new SpatialPooler();
            else if (poolerMode == PoolerMode.Multicore)
                sp = new SpatialPoolerMT();
            else
                throw new NotImplementedException();

            return sp;
        }

        internal static void InitPooler(PoolerMode poolerMode, SpatialPooler sp, Connections mem, Parameters parameters = null)
        {
            if (poolerMode == PoolerMode.Multinode)
                sp.init(mem, UnitTestHelpers.GetMemory(mem.HtmConfig));
            else if (poolerMode == PoolerMode.Multicore)
                sp.init(mem, UnitTestHelpers.GetMemory());
            else
                sp.init(mem);
        }

        ///// <summary>
        ///// Creates pooler instance.
        ///// </summary>
        ///// <param name="poolerMode"></param>
        ///// <returns></returns>
        //public static SpatialPooler CreatePooler(PoolerMode poolerMode)
        //{
        //    SpatialPooler sp;

        //    if (poolerMode == PoolerMode.Multinode)
        //        sp = new SpatialPoolerParallel();
        //    else if (poolerMode == PoolerMode.Multicore)
        //        sp = new SpatialPoolerMT();
        //    else
        //        sp = new SpatialPooler();

        //    return sp;
        //}
    }


    public enum PoolerMode
    {
        SingleThreaded,

        Multicore,

        Multinode
    }
}
