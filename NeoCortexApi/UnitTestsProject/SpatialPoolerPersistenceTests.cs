using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;


namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerPersistenceTests
    {
        #region Private Methods
        private static Parameters GetDefaultParams()
        {
            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 5);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);
            //int r = parameters.Get<int>(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA);

            return parameters;
        }
        #endregion


        [TestMethod]
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

            var json = JsonConvert.SerializeObject(mem1, settings);

            var mem2 = JsonConvert.DeserializeObject<Connections>(json, settings);
        
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

            sp.init(mem1);
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
