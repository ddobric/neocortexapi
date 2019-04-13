using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnitTestsProject
{
    [TestClass]
    public class InMemoryDistributedDictionaryTests
    {
        [TestMethod]
        public void TestInMemoryDictionary()
        {
          
            //InMemoryDistributedDictionary<int, int> dict = new InMemoryDistributedDictionary<int, int>(3);

            //for (int i = 0; i < 90; i++)
            //{
            //    dict.Add(i, i);
            //}

            //int n = 0;
            //foreach (var item in dict)
            //{
            //    Assert.AreEqual<int>(item.Key, n);
            //    Assert.AreEqual<int>(item.Value, n);

            //    n++;

            //    Debug.WriteLine(n);
            //}
        }


        [TestMethod]
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
            var rnd = new Random();

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
            sp.init(mem);
        }
    }
}
