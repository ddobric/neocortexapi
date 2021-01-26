// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace UnitTests.Parallel
{
    [TestClass]
    public class InMemoryDistributedDictionaryTests
    {


        [TestCategory("Prod")]
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
            var parameters = UnitTestHelpers.GetDefaultParams();
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
                catch (Exception ignored)
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
                        throw ex;
                }
            }

            return lastKnownGood;
        }

        [TestMethod]
        public void TTEST()
        {
            Helpers.F1();
        }

    }


}
