// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;

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


        internal static HtmConfig GetHtmConfig(int inpBits, int columns)
        {
            var htmConfig = new HtmConfig(new int[] { inpBits }, new int[] { columns })
            {
                PotentialRadius = 5,
                PotentialPct = 0.5,
                GlobalInhibition = false,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 3.0,
                StimulusThreshold = 0.0,
                SynPermInactiveDec = 0.01,
                SynPermActiveInc = 0.1,
                SynPermConnected = 0.5,
                ConnectedPermanence = 0.5,
                MinPctOverlapDutyCycles = 0.1,
                MinPctActiveDutyCycles = 0.1,
                DutyCyclePeriod = 10,
                MaxBoost = 10,
                ActivationThreshold = 10,
                MinThreshold = 6,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42),
            };

            return htmConfig;
        }
        public static DistributedMemory GetDistributedDictionary(HtmConfig htmConfig)
        {
            var cfg = UnitTestHelpers.DefaultSbConfig;

            return new DistributedMemory()
            {
                ColumnDictionary = new ActorSbDistributedDictionaryBase<Column>(cfg, UnitTestHelpers.GetLogger()),
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

        public static long[] CreateRandomSdr(long numCells, double sparsity)
        {
            Random rnd = new Random();

            var cells = new List<long>();

            int numActCells = (int)(numCells * sparsity);

            int actual = 0;

            while (actual < numActCells)
            {
                long index = rnd.NextInt64(0, numCells-1);

                if (cells.Contains(index) == false)
                {
                    cells.Add(index);
                    actual++;
                }
            }

            return cells.ToArray();
        }
    }


    public enum PoolerMode
    {
        SingleThreaded,

        Multicore,

        Multinode
    }
}
