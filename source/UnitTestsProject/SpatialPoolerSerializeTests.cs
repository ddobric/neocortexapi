using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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


        ///// <summary>
        ///// This test runs Spatial Pooler without trained data and with Certain Input parameters.It Serializes the instance of Spatial Pooler in a JSON file.
        ///// Subsequently it can read the serialized file and deserialize the content to Spatial Pooler Object
        ///// Correctness of Serialized and Dserialized values can be compared through the Assert Function
        ///// </summary>

        //[TestMethod]
        //[TestCategory("LongRunning")]
        //public void SerializationTest1()
        //{
        //    var parameters = GetDefaultParams();

        //    parameters.setInputDimensions(new int[] { 500 });
        //    parameters.setColumnDimensions(new int[] { 2048 });
        //    parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
        //    parameters.setGlobalInhibition(true);

        //    var sp1 = new SpatialPooler();

        //    var mem1 = new Connections();
        //    parameters.apply(mem1);

        //    sp1.init(mem1);

        //    sp1.Serializer("spTesting.json");
        //    string ser = File.ReadAllText("spTesting.json");
        //    var sp2 = SpatialPooler.Deserializer("spTesting.json");

        //  sp2.Serializer("spTestingDeserialized.json");

        //    string des = File.ReadAllText("spTestingDeserialized.json");

        // Assert.IsTrue(ser.SequenceEqual(des));


        //}


        /// <summary>
        /// This test runs SpatialPooler 32x32 with input of 16x16 . It learns the sequence to stable SDR representation.
        /// in very few steps (2 steps). Test runs 5 iterations and keeps stable SDR encoded sequence.
        /// After 5 steps, current instance of learned SpatialPooler (SP1) is serialized to JSON and then
        /// deserialized to second instance SP2.
        /// Second instance SP2 continues learning of the same input. Expectation is that SP2 continues in stable state with same 
        /// set of active columns as SP1.
        /// </summary>
        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationTestWithTrainedData()
        {
            // TODO: see SpatialPooler_Stability_Experiment_3()
            // use it here.

            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 16 * 16 });
            parameters.setColumnDimensions(new int[] { 32 * 32 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 32 * 32);
            parameters.setMinPctOverlapDutyCycles(0.01);

            var mem = new Connections();
            parameters.apply(mem);

            var sp1 = new SpatialPooler();
            sp1.Init(mem);


            int[] output = new int[32 * 32];

            int[] inputVector = Helpers.GetRandomVector(16 * 16, rnd: parameters.Get<Random>(KEY.RANDOM));
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
                sp1.compute(inputVector, output, true);

                var activeCols1 = ArrayUtils.IndexWhere(output, (el) => el == 1);

                // Remember SDR for every input.
                // if(i >= 4)
                // sdrs.Add(output)

                str1 = Helpers.StringifyVector(activeCols1);

                Debug.WriteLine(str1);
            }


            // if stable 
            //  HtmSerializer ser = new HtmSerializer();
            // ser.Serialize(sp, "sp.json");

            //  var sp2 = HtmSerializer.Load("sp.json");
            // Implement a code to compare sdrs of sp2 that have been learned with sp1.


            string ser1 = File.ReadAllText("spTrain1.json");

            //var sp2 = SpatialPooler.Deserializer("spTrain1.json");

            //sp2.Serializer("spTrainDes1.json");

            //string des1 = File.ReadAllText("spTrainDes1.json");

            //Assert.IsTrue(ser1.SequenceEqual(des1));

            //for (int i = 5; i < 10; i++)
            //{
            //    sp1.compute(inputVector, sdrArray, false);

            //    var sdr2 = ArrayUtils.IndexWhere(sdrArray, (el) => el == 1);

            //    // Compare sdr with sdr1 of the same input
            //    var str2 = Helpers.StringifyVector(sdr2);

            //    Debug.WriteLine(str2);
            //    Assert.IsTrue(str1.SequenceEqual(str2));

            //}

        }


        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationDistalSegmentTest()
        {
            Dictionary<Cell, List<DistalDendrite>> distalSegments = new Dictionary<Cell, List<DistalDendrite>>();
            distalSegments.Add(new Cell(), new List<DistalDendrite>() { new DistalDendrite(new Cell(), 1, 1, 1, 1.1, 100) { } });

            var x = new { DistalSegments = distalSegments };

            HtmSerializerOld ser = new HtmSerializerOld();
            ser.Serialize(x, "distalsegment.json");
        }

    }

}

