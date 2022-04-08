// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnitTestsProject
{
    [TestClass]
    public class InMemoryDistributedDictionaryTests
    {


        [TestMethod]
        public void TestInMemoryDictionary()
        {
            InMemoryDistributedDictionary<int, int> dict = new InMemoryDistributedDictionary<int, int>(3);

            for (int i = 0; i < 90; i++)
            {
                dict.Add(i, i);
            }

            int n = 0;
            foreach (var item in dict)
            {
                Assert.AreEqual<int>(item.Key, n);
                Assert.AreEqual<int>(item.Value, n);

                n++;

                Debug.WriteLine(n);
            }
        }


        /// <summary>
        /// Excluded from testing because of OutOfMemory
        /// This test is used to run in out of memory by initializing of to big SpatialPooler.
        /// </summary>
        //[TestMethod]
        [ExpectedException(typeof(OutOfMemoryException))]
        public void TestMaxDims()
        {
            var parameters = SpatialPoolerResearchTests.GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, 64 * 64);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
            parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * 64 * 64);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * 64 * 64);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
            parameters.Set(KEY.MAX_BOOST, 5);

            parameters.setInputDimensions(new int[] { 320, 320 });
            parameters.setColumnDimensions(new int[] { 2048, 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPooler();
            var mem = new Connections();
            //List<int> intList = ArrayUtils.ReadCsvFileTest("TestFiles\\digit1_binary_32bit.txt");
            //intList.Clear();

            //List<int> intList = new List<int>();


            int[] inputVector = new int[1024];

            for (int i = 0; i < 31; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (i > 2 && i < 5 && j > 2 && j < 8)
                        inputVector[i * 32 + j] = 1;
                    else
                        inputVector[i * 32 + j] = 0;
                }
            }

            parameters.apply(mem);
            sp.Init(mem, null);
        }

        /// <summary>
        /// This test will faile with System.AccessViolationException: 
        /// 'Attempted to read or write protected memory. 
        /// This is often an indication that other memory is corrupt.'
        /// </summary>
        //[TestMethod]
        //[ExpectedException(typeof(AccessViolationException))]

        //public void HugeIntArrayTest()
        //{
        //    var x = new int[4096, 250000];
        //    for (int i = 0; i < 4096; i++)
        //    {
        //        int[] y = x.GetRow2(i);
        //    }
        //}



        [TestMethod]
        [TestCategory("LongRunning")]
        public void ArraySizeTestOld()
        {
            byte[] bb = new byte[Int32.MaxValue - 56];

            for (int i = int.MaxValue - 2146435071; i < int.MaxValue; i++)
            {
                try
                {
                    //intmax = 2147483647
                    //2146435071
                    // intmax-2146435071
                    int[] ii = new int[i];
                    Console.Out.WriteLine("MaxValue: " + i);

                }
                catch (Exception)
                {
                    Console.WriteLine($"Max int array: {i - 1}");
                    return;
                }
            }
        }

        /// <summary>
        /// Max size of array is: 2146435071
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void CreateHugeArrayTest()
        {
            var max = lookupMax((arrSize) =>
            {
                var arr = Array.CreateInstance(typeof(int), 1, arrSize);
            });

            Debug.WriteLine($"Max possible size of int[,] array: { max }");
        }


        /// <summary>
        /// Maxint array: 2146435071
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void IntArraySizeTest()
        {
            var max = lookupMax((arrSize) =>
            {
                int[] arr = new int[arrSize];
            });

            Debug.WriteLine($"Max possible size of int[] array: { max }");
        }

        /// <summary>
        /// Max Dict Size: 436217612
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void DictionarySizeTest()
        {
            var max = lookupMax((dictSize) => new Dictionary<int, int>(dictSize));

            Debug.WriteLine($"Max possible size of dictionery: { max }");
        }


        private int lookupMax(Action<int> fnc)
        {
            byte[] bb = new byte[Int32.MaxValue - 56];

            int lastKnownGood = 10000;
            int newVal = lastKnownGood;
            int lastIncrement = int.MaxValue / 4;

            while (true)
            {
                try
                {
                    fnc(newVal);

                    lastKnownGood = newVal;

                    Debug.WriteLine($"Last known good: { lastKnownGood }");

                    newVal = lastKnownGood + lastIncrement;

                    if (newVal < 0)
                    {
                        lastIncrement = lastIncrement / 2;
                        newVal = lastKnownGood + lastIncrement;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is OverflowException || ex is OutOfMemoryException)
                    {
                        lastIncrement = lastIncrement / 2;

                        newVal = lastKnownGood + lastIncrement;

                        if (lastIncrement == 0) break;
                    }
                    else
                        throw;
                }
            }

            return lastKnownGood;
        }

        /*   [TestMethod]
           [TestCategory("AkkaHostRequired")]
           public void AkkClusterTest()
           {
               var actSystem = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
                   akka {  
                       loglevel=DEBUG
                       actor{
                           provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""  		               
                       }
                       remote {
                           connection-timeout = 120 s
                           transport-failure-detector {
                               heartbeat-interval = 1000s 
                               acceptable-heartbeat-pause = 6000s 
                           }
                           dot-netty.tcp {
                               maximum-frame-size = 326000000b
                               port = 8080
                               hostname = 0.0.0.0
                               public-hostname = DADO-SR1
                           }
                       }
                   }"));

               string actorName = $"TestActor2";

               IActorRef aRef = null;

               aRef = actSystem.ActorOf(Props.Create(() => new DictNodeActor())
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(Address.Parse(Helpers.DefaultNodeList.First())))),
                    actorName);

               var sel = actSystem.ActorSelection($"/user/{actorName}");
               aRef = sel.ResolveOne(TimeSpan.FromSeconds(5)).Result;

               //try
               //{
               //    var sel = actSystem.ActorSelection($"/user/{actorName}");
               //    aRef = sel.ResolveOne(TimeSpan.FromSeconds(5)).Result;

               //}
               //catch (AggregateException ex)
               //{
               //    if (ex.InnerException is ActorNotFoundException)
               //    {
               //        aRef =
               //          actSystem.ActorOf(Props.Create(() => new DictNodeActor())
               //          .WithDeploy(Deploy.None.WithScope(new RemoteScope(Address.Parse(Helpers.DefaultNodeList.First())))),
               //          actorName);
               //    }
               //}

               var result = aRef.Ask<string>(new PingNodeMsg()
               {
                   Msg = "Echo"
               }, TimeSpan.FromSeconds(5)).Result;
           }
        */
        [TestMethod]
        //[DataRow(PoolerMode.SingleThreaded)]
        [DataRow(PoolerMode.Multicore)]
        [TestCategory("LongRunning")]
        [TestCategory("Parallel")]
        public void SPInitTest(PoolerMode poolerMode)
        {
            //Thread.Sleep(2000);

            int numOfColsInDim = 12;
            int numInputs = 128;

            Parameters parameters = Parameters.getAllDefaultParameters();

            parameters.Set(KEY.POTENTIAL_RADIUS, 5);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { numInputs, numInputs });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { numOfColsInDim, numOfColsInDim });
            parameters.setPotentialRadius(5);

            //This is 0.3 in Python version due to use of dense 
            // permanence instead of sparse (as it should be)
            parameters.setPotentialPct(0.5);

            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(3);
            parameters.setStimulusThreshold(1);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.1);
            parameters.setMinPctActiveDutyCycles(0.1);
            parameters.setDutyCyclePeriod(10);
            parameters.setMaxBoost(10);
            parameters.setSynPermTrimThreshold(0);

            //This is 0.5 in Python version due to use of dense 
            // permanence instead of sparse (as it should be)
            parameters.setPotentialPct(1);

            parameters.setSynPermConnected(0.1);

            //SpatialPooler sp = UnitTestHelpers.CreatePooler(poolerMode) ;            
            var sp = new SpatialPoolerParallel();
            var mem = new Connections();
            parameters.apply(mem);

            sp.Init(mem, UnitTestHelpers.GetMemory(mem.HtmConfig));
            //sp.init(mem);

            //int[] inputVector = new int[] { 1, 0, 1, 0, 1, 0, 0, 1, 1 };
            //int[] activeArray = new int[] { 0, 0, 0, 0, 0 };
            //for (int i = 0; i < 20; i++)
            //{
            //    sp.compute(mem, inputVector, activeArray, true);
            //}           
        }

        [TestMethod]
        public void TTEST()
        {
            Helpers.F1();
        }

    }


}
