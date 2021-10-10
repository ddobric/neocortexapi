// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Experiments
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

        /// <summary>
        /// Gest the logger instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ILogger GetLogger(string logger = "UnitTest")
        {
            ILoggerFactory factory = LoggerFactory.Create(logBuilder =>
            {
                logBuilder.AddDebug();
                logBuilder.AddConsole();
            });

            return factory.CreateLogger(logger);
        }

        /// <summary>
        /// Gets default sparse dictionary configuration.
        /// </summary>
        public static ActorSbConfig DefaultSbConfig
        {
            get
            {
                ActorSbConfig cfg = new ActorSbConfig
                {
                    SbConnStr = Environment.GetEnvironmentVariable("SbConnStr"),
                    ReplyMsgQueue = "actorsystem/rcvlocal",
                    RequestMsgTopic = "actorsystem/actortopic",
                    NumOfElementsPerPartition = -1, // This means, number of partitions equals number of nodes.
                    NumOfPartitions = 35,// Should be uniformly distributed across nodes.
                    BatchSize = 1000,
                    ConnectionTimeout = TimeSpan.FromMinutes(5),

                    //Nodes = new List<string>() { "node1", "node2", "node3" }
                    Nodes = new List<string>() { "node1" }
                };

                return cfg;
            }
        }

        public static DistributedMemory GetDistributedDictionary(HtmConfig htmConfig)
        {
            var cfg = UnitTestHelpers.DefaultSbConfig;

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
                sp.Init(mem, UnitTestHelpers.GetMemory(mem.HtmConfig));
            else if (poolerMode == PoolerMode.Multicore)
                sp.Init(mem, UnitTestHelpers.GetMemory());
            else
                sp.Init(mem);
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
