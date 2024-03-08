// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests for HomeostaticPlasticityActivator.
    /// </summary>
    [TestClass]
    [TestCategory("Prod")]
    public class HomeostaticPlasticityActivatorTests
    {
        [TestMethod]
        public void TestHashDictionary()
        {
            double minOctOverlapCycles = 1.0;
            int inputBits = 100;
            int numColumns = 2048;
            double maxBoost = 5.0;

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.MAX_BOOST, maxBoost);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minOctOverlapCycles);

            var mem = new Connections();

            List<int[]> inputs = new List<int[]>();
            List<int[]> outputs = new List<int[]>();

            // Create random vectors with 2% sparsity
            for (int i = 0; i < 10; i++)
            {
                var inp = NeoCortexUtils.CreateRandomVector(inputBits, 20);
                var outp = NeoCortexUtils.CreateRandomVector(numColumns, 40);

                inputs.Add(inp);
                outputs.Add(outp);
            }

            bool isBoostOff = true;

            int learningCycles = 100;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputs.Count * learningCycles,
                (isStable, numPatterns, actColAvg, seenInputs) => { });

            for (int cycle = 0; cycle < 1000; cycle++)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    isBoostOff = hpa.Compute(inputs[i], outputs[i]);

                    if (isBoostOff)
                        Assert.IsTrue(cycle >= learningCycles);
                }
            }

            Assert.IsTrue(isBoostOff);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestCorrelation1()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));


            Assert.IsTrue(res == 1.0);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestCorrelation2()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 0, 0, 1 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));

            Assert.IsTrue(res == 0.8);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestCorrelation3()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 0, 0 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));

            Assert.IsTrue(res == 0.6);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestCorrelation4()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));

            Assert.IsTrue(res == 0.8);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Check CalcArraySimilarity method in different scenarios")]
        [DataRow(-1.0, new int[] { }, new int[] { })]
        [DataRow(0.6, new int[] { 1, 2, 3, 4, 5 }, new int[] { 3, 4, 5, 6, 7 })]
        [DataRow(0.0, new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 })]
        [DataRow(1.0, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
        public void CalcArraySimilarityTest(double expectedResult, int[] arrayOne, int[] arrayTwo)
        {
            double result = HomeostaticPlasticityController.CalcArraySimilarity(arrayOne, arrayTwo);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Check GetHash function")]
        [DataRow("��V�쬐�h���F@B���Wh�/ߌ?)K��I", new int[] { 1, 2, 3, 4, 5, 6 })]
        [DataRow("Oj��e�o��Kf���*��n�?�>�e(�H��", new int[] { 1, 2, 3, 4, 5 })]
        public void GetHashTest(string hashValue, int[] obj)
        {
            string result = HomeostaticPlasticityController.GetHash(obj);
            Assert.AreEqual(hashValue, result);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Check TraceState function when stable state is never achieved")]
        [DataRow("[0 - stable cycles: 0,len = 0] 	 ", "MinKey=7G��q�՗��u�l�(om<��1z;%c*�(�7�, min stable states=0", 10)]
        public void TraceStateTest(string expectedTraceState1, string expectedTraceState2, int elements)
        {
            string currentBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = System.IO.Path.Combine(currentBaseDirectory, @"TestFiles/traceStateTest.txt");
            string filePath = Path.GetFullPath(fileName);
            int[] inputArray = new int[4];
            int[] outputArray = new int[4];
            HtmConfig prms = new HtmConfig(inputArray, outputArray);
            Connections htmMemory = new Connections(prms);
            double requiredSimilarityThreshold = 0.6;
            Action<bool, int, double, int> OnStabilityStatusUpdate = (a, b, c, d) => Console.WriteLine("Write {0}, {1}, {2}, {3}", a, b, c, d);
            HomeostaticPlasticityController homeostaticPlasticityController =
                new HomeostaticPlasticityController(htmMemory, 5, OnStabilityStatusUpdate, 12, requiredSimilarityThreshold);
            bool res = false;

            for (int i = 0; i < 10; i++)
            {
                res = homeostaticPlasticityController.Compute(inputArray, outputArray);
            }
            homeostaticPlasticityController.TraceState(filePath);
            string traceStateOutput1 = File.ReadLines(filePath).First();
            string traceStateOutput2 = File.ReadLines(filePath).Last();
            Assert.AreEqual(expectedTraceState1, traceStateOutput1);
            Assert.AreEqual(expectedTraceState2, traceStateOutput2);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Check TraceState function with some stable cycles")]
        [DataRow("[0 - stable cycles: 29,len = 0] 	 ", "MinKey=7G��q�՗��u�l�(om<��1z;%c*�(�7�, min stable states=29", 10)]
        public void TraceStateTest2(string expectedTraceState1, string expectedTraceState2, int elements)
        {
            string currentBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = System.IO.Path.Combine(currentBaseDirectory, @"TestFiles/traceStateTest.txt");
            string filePath = Path.GetFullPath(fileName);
            int[] inputArray = new int[4];
            int[] outputArray = new int[4];
            HtmConfig prms = new HtmConfig(inputArray, outputArray);
            Connections htmMemory = new Connections(prms);
            double requiredSimilarityThreshold = -1;
            Action<bool, int, double, int> OnStabilityStatusUpdate = (a, b, c, d) => Console.WriteLine("Write {0}, {1}, {2}, {3}", a, b, c, d);
            HomeostaticPlasticityController homeostaticPlasticityController =
                new HomeostaticPlasticityController(htmMemory, 5, OnStabilityStatusUpdate, 10, requiredSimilarityThreshold);
            bool res = false;

            for (int i = 0; i < 30; i++)
            {
                res = homeostaticPlasticityController.Compute(inputArray, outputArray);
            }
            homeostaticPlasticityController.TraceState(filePath);
            string traceStateOutput1 = File.ReadLines(filePath).First();
            string traceStateOutput2 = File.ReadLines(filePath).Last();
            Assert.AreEqual(expectedTraceState1, traceStateOutput1);
            Assert.AreEqual(expectedTraceState2, traceStateOutput2);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Check CalcArraySimilarityOld2 function")]
        [DataRow(0.6, new int[] { 1, 2, 3, 4, 5 }, new int[] { 6, 7, 3, 4, 5 })]
        [DataRow(0.0, new int[] { 1, 2, 3, 4, 5 }, new int[] { 6, 7, 8, 9, 10 })]
        public void CalcArraySimilarityOld2Test(double expectedResult, int[] arrayOne, int[] arrayTwo)
        {
            double result = HomeostaticPlasticityController.CalcArraySimilarityOld2(arrayOne, arrayTwo);
            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        /// Nut used, because Deserialize is not completed.
        /// </summary>
       // [TestMethod]
       // [Description("Validate Serialization and Deserialization operation for a HomeostaticPlasticityController object")]
        //public void DeserializeTest()
        //{
        //    HtmConfig prms = new HtmConfig(new int[4], new int[4]);
        //    Connections htmMemory = new Connections(prms);
        //    double requiredSimilarityThreshold = 1.0;
        //    HomeostaticPlasticityController homeostaticPlasticityController =
        //        new HomeostaticPlasticityController(htmMemory, 5, null, 50, requiredSimilarityThreshold);
        //    string currentBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //    string fileName = System.IO.Path.Combine(currentBaseDirectory, @"TestFiles/testSerialize.txt");
        //    string filePath = Path.GetFullPath(fileName);
        //    HomeostaticPlasticityController deserializedObject = null;

        //    using (StreamWriter streamWriter = new StreamWriter(filePath))
        //    {
        //        homeostaticPlasticityController.Serialize(streamWriter);
        //    }

        //    using (StreamReader streamReader = new StreamReader(filePath))
        //    {
        //        deserializedObject = HomeostaticPlasticityController.Deserialize(streamReader, htmMemory);
        //    }

        //    Assert.IsTrue(homeostaticPlasticityController.Equals(deserializedObject));
        //}

        [TestMethod]
        [TestCategory("Prod")]
        [Description("HomeostaticPlasticityController with unrealistic parameters, compute method output expected is False as m_IsStable is False")]
        public void ComputeTest()
        {
            int[] inputArray = new int[4];
            int[] outputArray = new int[4];
            HtmConfig prms = new HtmConfig(inputArray, outputArray);
            Connections htmMemory = new Connections(prms);
            double requiredSimilarityThreshold = -1;
            HomeostaticPlasticityController homeostaticPlasticityController =
                new HomeostaticPlasticityController(htmMemory, 5, null, 15, requiredSimilarityThreshold);

            bool res = homeostaticPlasticityController.Compute(new int[4], new int[4]);
            // In the second run of HomeostaticPlasticityController.Compute, condition m_InOutMap.ContainsKey(inpHash) is True
            res = homeostaticPlasticityController.Compute(new int[4], new int[4]);

            Assert.IsFalse(res);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Using HomeostaticPlasticityController in simulated training conditions where stable state is achieved")]
        public void ComputeTest2()
        {
            int[] inputArray = new int[4];
            int[] outputArray = new int[4];
            HtmConfig prms = new HtmConfig(inputArray, outputArray);
            Connections htmMemory = new Connections(prms);
            double requiredSimilarityThreshold = -1;
            Action<bool, int, double, int> OnStabilityStatusUpdate = (a, b, c, d) => Console.WriteLine("Write {0}, {1}, {2}, {3}", a, b, c, d);
            HomeostaticPlasticityController homeostaticPlasticityController =
                new HomeostaticPlasticityController(htmMemory, 5, OnStabilityStatusUpdate, 10, requiredSimilarityThreshold);
            bool res = false;

            for (int i = 0; i < 30; i++)
            {
                res = homeostaticPlasticityController.Compute(inputArray, outputArray);
            }
            Assert.IsTrue(res);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Using HomeostaticPlasticityController in simulated training conditions to check the state of private variables")]
        public void ComputeTest3()
        {
            int[] inputArray = new int[4];
            int[] outputArray = new int[4];
            HtmConfig prms = new HtmConfig(inputArray, outputArray);
            Connections htmMemory = new Connections(prms);
            double requiredSimilarityThreshold = -1;
            Dictionary<string, int> stableCyclesForInput = null;
            int requiredNumOfStableCycles = 0;
            bool isStable = false;
            Action<bool, int, double, int> OnStabilityStatusUpdate = (a, b, c, d) => Console.WriteLine("Write {0}, {1}, {2}, {3}", a, b, c, d);
            HomeostaticPlasticityController homeostaticPlasticityController =
                new HomeostaticPlasticityController(htmMemory, 5, OnStabilityStatusUpdate, 15, requiredSimilarityThreshold);

            for (int i = 0; i < 20; i++)
            {
                homeostaticPlasticityController.Compute(inputArray, outputArray);
            }

            stableCyclesForInput = GetPrivateFieldValue<Dictionary<string, int>>(homeostaticPlasticityController, "m_NumOfStableCyclesForInput");

            requiredNumOfStableCycles = GetPrivateFieldValue<int>(homeostaticPlasticityController, "m_RequiredNumOfStableCycles");

            isStable = GetPrivateFieldValue<bool>(homeostaticPlasticityController, "m_IsStable");

            Assert.IsTrue(stableCyclesForInput[HomeostaticPlasticityController.GetHash(inputArray)] > requiredNumOfStableCycles);
            Assert.IsTrue(isStable);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [Description("Check Equals method under different scenarios")]
        public void EqualsTest()
        {
            // Comparing a HomeostaticPlasticityController to itself
            HomeostaticPlasticityController obj = new HomeostaticPlasticityController();
            bool result = obj.Equals(obj);
            Assert.IsTrue(result);

            // Comparing a HomeostaticPlasticityController to null
            result = obj.Equals(null);
            Assert.IsFalse(result);

            // Comparing HomeostaticPlasticityController objects with different htmMemory
            HtmConfig prms1 = new HtmConfig(new int[4], new int[4]);
            Connections htmMemory1 = new Connections(prms1);
            HomeostaticPlasticityController obj1 = new HomeostaticPlasticityController(htmMemory1, 5, null);
            HtmConfig prms2 = new HtmConfig(new int[5], new int[5]);
            Connections htmMemory2 = new Connections(prms2);
            HomeostaticPlasticityController obj2 = new HomeostaticPlasticityController(htmMemory2, 5, null);
            result = obj1.Equals(obj2);
            Assert.IsFalse(result);

            // Comparing HomeostaticPlasticityController objects with different requiredSimilarityThreshold
            prms1 = new HtmConfig(new int[4], new int[4]);
            htmMemory1 = new Connections(prms1);
            double requiredSimilarityThreshold1 = 1.0;
            obj1 = new HomeostaticPlasticityController(htmMemory1, 5, null, 50, requiredSimilarityThreshold1);
            double requiredSimilarityThreshold2 = 0.97;
            obj2 = new HomeostaticPlasticityController(htmMemory1, 5, null, 50, requiredSimilarityThreshold2);
            result = obj1.Equals(obj2);
            Assert.IsFalse(result);

            // Comparing HomeostaticPlasticityController objects with different minCycles
            prms1 = new HtmConfig(new int[4], new int[4]);
            htmMemory1 = new Connections(prms1);
            requiredSimilarityThreshold1 = 1.0;
            obj1 = new HomeostaticPlasticityController(htmMemory1, 4, null, 50, requiredSimilarityThreshold1);
            obj2 = new HomeostaticPlasticityController(htmMemory1, 5, null, 50, requiredSimilarityThreshold1);
            result = obj1.Equals(obj2);
            Assert.IsFalse(result);

            // Comparing HomeostaticPlasticityController objects with different numOfCyclesToWaitOnChange
            prms1 = new HtmConfig(new int[4], new int[4]);
            htmMemory1 = new Connections(prms1);
            requiredSimilarityThreshold1 = 1.0;
            obj1 = new HomeostaticPlasticityController(htmMemory1, 5, null, 40, requiredSimilarityThreshold1);
            obj2 = new HomeostaticPlasticityController(htmMemory1, 5, null, 50, requiredSimilarityThreshold1);
            result = obj1.Equals(obj2);
            Assert.IsFalse(result);
        }

        private const BindingFlags BindToEveryThing = BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        /// <summary>
        /// Returns private field value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="prms"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static T GetPrivateFieldValue<T>(object obj, string name, object[] prms = null)
        {
            FieldInfo field = obj.GetType().GetField(name, BindToEveryThing);
            if (field != null)
                return (T)field.GetValue(obj);
            else
                throw new ArgumentException();

        }
    }
}
