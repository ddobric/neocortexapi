// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerPersistenceTests
    {
        #region Private Methods
        private static Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 80.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);
            //int r = parameters.Get<int>(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA);

            return parameters;
        }
        #endregion


        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 1000 });
            parameters.setColumnDimensions(new int[] { 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
            parameters.setGlobalInhibition(true);

            var sp = new SpatialPooler();

            var mem1 = new Connections();
            parameters.apply(mem1);

            var settings = new JsonSerializerSettings { ContractResolver = new ContractResolver(), Formatting = Formatting.Indented };

            sp.Init(mem1);

            var jsonMem = JsonConvert.SerializeObject(mem1, settings);

            var mem2 = JsonConvert.DeserializeObject<Connections>(jsonMem, settings);

            var jsonSp = JsonConvert.SerializeObject(sp, settings);

            var sp2 = JsonConvert.DeserializeObject<SpatialPooler>(jsonSp, settings);

            #region Binary Serialization DOES NOT WORK

            /*
              MemoryStream ms = new MemoryStream();

              // Construct a BinaryFormatter and use it to serialize the data to the stream.
              BinaryFormatter formatter = new BinaryFormatter();
              try
              {
                  Random x = new Random(1);
                  formatter.Serialize(ms, x);
              }
              catch (SerializationException e)
              {
                  Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                  throw;
              }

              ms.Seek(0, SeekOrigin.Begin);

              Connections mem2 = (Connections)formatter.Deserialize(ms);
              */
            #endregion

            sp.Init(mem1);
        }


        /// <summary>
        /// This test runs SpatialPooler 64x64 with input of 32x32. It learns the sequence to stable SDR representation
        /// in very few steps (2 steps). Test runs 10 iterations and keeps stable SDR encoded sequence.
        /// After 10 steps, current instance of learned SpatialPooler (SP1) is serialized to JSON and then
        /// deserialized to second instance SP2.
        /// Second instance SP2 continues learning of the same input. Expectation is that SP2 continues in stable state with same 
        /// set of active columns as SP1.
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void StableOutputWithPersistence()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);

            var mem = new Connections();
            parameters.apply(mem);

            var sp = new SpatialPooler();
            sp.Init(mem);


            int[] activeArray = new int[64 * 64];

            int[] inputVector = Helpers.GetRandomVector(32 * 32, parameters.Get<Random>(KEY.RANDOM));

            string str1 = String.Empty;

            for (int i = 0; i < 2; i++)
            {
                sp.compute(inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                str1 = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str1);
            }

            var settings = new JsonSerializerSettings { ContractResolver = new ContractResolver(), Formatting = Formatting.Indented };

            var jsonSp = JsonConvert.SerializeObject(sp, settings);

            var sp2 = JsonConvert.DeserializeObject<SpatialPooler>(jsonSp, settings);

            activeArray = new int[activeArray.Length];

            for (int i = 10; i < 20; i++)
            {
                sp2.compute(inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str2 = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str2);

                Assert.IsTrue(str1.SequenceEqual(str2));
            }

        }



        class ContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(p => base.CreateProperty(p, memberSerialization))
                    .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(f => base.CreateProperty(f, memberSerialization)))
                    .ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
            }
        }
    }
}
