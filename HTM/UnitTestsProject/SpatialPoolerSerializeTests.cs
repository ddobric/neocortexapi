using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using NeoCortexApi.Utility;
using NeoCortex;
using Newtonsoft.Json;

using Newtonsoft.Json.Serialization;

namespace UnitTestsProject
{
    [TestClass]
    /// <summary>
    /// This file contains multiple Unit Test that implements and demonstrates newly integrated Serialization Functionality of Spatial Pooler
    /// </summary>
    public class SpatialPoolerSerializeTests
    {
        //Below Inputs can be used Globally for all the test cases
        //  int[] activeArray = new int[32 * 32];
        /*   int[] inputVector =  {
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                          1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                          1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 }; */




        //Setting up Default Parameters for the Spatial Pooler Test cases
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
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);


            return parameters;
        }


        /// <summary>
        /// This test runs Spatial Pooler without trained data and with Certain Input parameters.It Serializes the instance of Spatial Pooler in a JSON file.
        /// Further scopes about Deserialization test and Serialized and Deserialized Data comparison function is written towards the end part of this test but commented out for now since Deserialization method is not complete.
        /// Serialized and Deserialized value comparison can be done once the Deserialization function is fully implemented.
        /// </summary>

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationTest1()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 500 });
            parameters.setColumnDimensions(new int[] { 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
            parameters.setGlobalInhibition(true);

            var sp1 = new SpatialPooler();

            var mem1 = new Connections();
            parameters.apply(mem1);

            sp1.init(mem1);

            sp1.Serializer("spTestingnewone.json");
            string ser = File.ReadAllText("spTestingnewone.json");
            var sp2 = SpatialPooler.Deserializer("spTestingnewone.json");

         //    sp2.Serializer("spTestingDeserialized");

       //     string des = File.ReadAllText("spTestingDeserialized");

       //     Assert.IsTrue(ser.SequenceEqual(des));

        /*    JsonSerializerSettings settings = new JsonSerializerSettings
            {

                DefaultValueHandling = DefaultValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                TypeNameHandling = TypeNameHandling.Auto

            };

            var jsonData = JsonConvert.SerializeObject(sp1, settings);
            File.WriteAllText("spSerializedFile.json", jsonData);
    

            var sp2 = JsonConvert.DeserializeObject<SpatialPooler>(File.ReadAllText("spSerializedFile.json"), settings);
            var jsonData2 = JsonConvert.SerializeObject(sp2, settings);



            File.WriteAllText("spSerializedFile2.json", jsonData2);

            var sp3 = JsonConvert.DeserializeObject<SpatialPooler>(File.ReadAllText("spSerializedFile2.json"), settings);
            var jsonData3 = JsonConvert.SerializeObject(sp3, settings);

            File.WriteAllText("spSerializedFile3.json", jsonData3);
            
            Assert.IsTrue(jsonData2.SequenceEqual(jsonData3));
            Assert.IsTrue(jsonData.SequenceEqual(jsonData2));
            */
        }


        /// <summary>
        /// This test runs SpatialPooler 64x64 with input of 32x32 . It learns the sequence to stable SDR representation.
        /// in very few steps (2 steps). Test runs 5 iterations and keeps stable SDR encoded sequence.
        /// Further scope of the test(once Deserialization is implemented).
        /// After 5 steps, current instance of learned SpatialPooler (SP1) is serialized to JSON and then
        /// deserialized to second instance SP2.
        /// Second instance SP2 continues learning of the same input. Expectation is that SP2 continues in stable state with same 
        /// set of active columns as SP1.
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationTestWithTrainedData()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 16 * 16 });
            parameters.setColumnDimensions(new int[] { 32 * 32 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 32 * 32);
            parameters.setMinPctOverlapDutyCycles(0.01);

            var mem = new Connections();
            parameters.apply(mem);

            var sp1 = new SpatialPooler();
            sp1.init(mem);


            int[] activeArray = new int[32 * 32];

            int[] inputVector = Helpers.GetRandomVector(16 * 16, parameters.Get<Random>(KEY.RANDOM));
            /*  int [] inputVector =  {
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                             1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,
                                             1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                             1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 };

          */
            string str1 = String.Empty;

            for (int i = 0; i < 5; i++)
            {
                sp1.compute(inputVector, activeArray, true);

                var activeCols1 = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                str1 = Helpers.StringifyVector(activeCols1);

                Debug.WriteLine(str1);
            }

            /*  JsonSerializerSettings settings = new JsonSerializerSettings
              {

                  DefaultValueHandling = DefaultValueHandling.Include,
                  ObjectCreationHandling = ObjectCreationHandling.Auto,
                  ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                  ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                  TypeNameHandling = TypeNameHandling.Auto

              };
              */
            //  var jsConverted = JsonConvert.SerializeObject(sp1, Formatting.Indented,  settings);

            //   string file2 = "spSerializeTrain-newtonsoft.json";
            //  File.WriteAllText(file2, jsConverted);

            sp1.Serializer("NewTestSR1.txt");
            string ser1 = File.ReadAllText("NewTestSR1.txt");

            var sp2 = SpatialPooler.Deserializer("NewTestSR1.txt");

            sp2.Serializer("NewTestSR2.txt");

            string des1 = File.ReadAllText("NewTestSR2.txt");

            Assert.IsTrue(ser1.SequenceEqual(des1));

        //    SpatialPooler sp2 = JsonConvert.DeserializeObject<SpatialPooler>(File.ReadAllText(file2), settings);

           for (int i = 5; i < 10; i++)
            {
                sp2.compute(inputVector, activeArray, false);

                var activeCols2 = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str2 = Helpers.StringifyVector(activeCols2);

                Debug.WriteLine(str2);
                Assert.IsTrue(str1.SequenceEqual(str2));

            }
            sp2.Serializer("spTrain22222.txt");
            string ser2 = File.ReadAllText("spTrain22222.txt");

        //  Assert.IsTrue(ser2.SequenceEqual(des1));

            /*    string serializedSecondPooler = JsonConvert.SerializeObject(sp2, Formatting.Indented, settings);
                string fileSecondPooler = "spSerializeTrain-secondpooler-newtonsoft.json";
                File.WriteAllText(fileSecondPooler, serializedSecondPooler);

                SpatialPooler sp3 = JsonConvert.DeserializeObject<SpatialPooler>(File.ReadAllText(fileSecondPooler), settings);
                string serializedThirdPooler = JsonConvert.SerializeObject(sp3, Formatting.Indented, settings);


                Assert.IsTrue(serializedThirdPooler.SequenceEqual(serializedSecondPooler), "Third and second poolers are not equal");
                Assert.IsTrue(jsConverted.SequenceEqual(serializedSecondPooler), "First and second poolers are not equal");
                */
        }

       
    }

}

