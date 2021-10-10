// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using System;
using System.Collections.Generic;

namespace UnitTestsProject.CortexNetworkTests
{
    [TestClass]
    public class ClaClassifierTest
    {
        /// <summary>
        /// general method for checking expected probability and value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retVal">Result of compute method</param>
        /// <param name="index">value of particular bucket index</param>
        /// <param name="value">expected value</param>
        /// <param name="probability">expected probability</param>
        public void checkValue<T>(ClassificationExperiment<T> retVal, int index, Object value, double probability)
        {
            Assert.AreEqual(retVal.getActualValue(index), value);
            Assert.AreEqual(probability, retVal.getStat(1, index), 0.01);
        }

        /// <summary>
        /// general method for calling computing method of claclassifier class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classifier">instance of claclassifier class</param>
        /// <param name="recordNum">iteration number</param>
        /// <param name="pattern">list of the active indices</param>
        /// <param name="bucket">bucket index</param>
        /// <param name="value">value of predicted field</param>
        /// <returns>classification object</returns>
        public ClassificationExperiment<T> compute<T>(CLAClassifier<T> classifier, int recordNum, int[] pattern, int bucket, Object value) where T : struct
        {
            Dictionary<string, object> classification = new Dictionary<string, object>();
            classification.Add("bucketIdx", bucket);
            classification.Add("actValue", value);
            return classifier.Compute(recordNum, classification, pattern, true, true);
        }

        /// <summary>
        /// This test trains the classifier for ten times with same bucket index and TM array of active cells.
        /// Then the test checks for the probability
        /// </summary>
        [TestMethod]
        public void TestSingleValueWithMultipleIteration()
        {
            var classifier = new CLAClassifier<double>();

            ClassificationExperiment<Double> retVal = null;
            for (int recordNum = 0; recordNum < 10; recordNum++)
            {
                retVal = compute(classifier, recordNum, new int[] { 1, 5 }, 0, 10);
            }

            checkValue(retVal, 0, 10.0, 1);
        }

        /// <summary>
        /// This test trains the classifier with twenty times with two different bucket index and TM array of active cells.
        /// Then the test checks for the probability
        /// </summary>
        [TestMethod]
        public void TestWithDiffertBucketIndexWithMultipleIteration()
        {
            var classifier = new CLAClassifier<double>();

            ClassificationExperiment<Double> retVal = null;
            for (int recordNum = 0; recordNum < 20; recordNum++)
            {
                int bucketIndex = recordNum % 2;
                retVal = compute(classifier, recordNum, new int[] { 1, 5 }, bucketIndex, 10);
            }

            checkValue(retVal, 0, 10.0, 0.5);
        }

        /// <summary>
        /// This test trains the classifier for ten times with same bucket index and TM array of active cells.
        /// Then compare the actual value and probability of predictive field 
        /// </summary>
        [TestMethod]
        public void TestSingleValue()
        {
            var classifier = new CLAClassifier<double>(new List<int> { 0 }, 0.001, 0.3);

            ClassificationExperiment<double> retVal = null;
            for (int recordNum = 0; recordNum < 10; recordNum++)
            {
                retVal = compute(classifier, recordNum, new int[] { 1, 5 }, 0, 10);
            }

            Assert.AreEqual(10.0, retVal.getActualValue(0), .00001);
            Assert.AreEqual(1.0, retVal.getStat(0, 0), .00001);
        }

        /// <summary>
        /// This test trains the classifier for single times with same bucket index and TM array of active cells.
        /// Then check number of actual value
        /// </summary>
        [TestMethod]
        public void TestCompute()
        {
            var classifier = new CLAClassifier<double>(new List<int> { 1 }, 0.1, 0.1);
            Dictionary<string, object> classification = new Dictionary<string, object>();
            classification.Add("bucketIdx", 4);
            classification.Add("actValue", 34.7);
            ClassificationExperiment<double> result = classifier.Compute(0, classification, new int[] { 1, 5, 9 }, true, true);

            //it is first time and so it wont learn and that's why value count will be one
            Assert.AreEqual(1, result.getActualValueCount());
            Assert.AreEqual(34.7, result.getActualValue(0), 0.01);
        }

        /// <summary>
        /// This test trains the classifier for two times with same bucket index and TM array of active cells.
        /// Then check number of actual value
        /// </summary>
        [TestMethod]
        public void TestComputeWithTwoIteration()
        {
            var classifier = new CLAClassifier<double>(new List<int> { 1 }, 0.1, 0.1);
            Dictionary<string, object> classification = new Dictionary<string, object>();
            classification.Add("bucketIdx", 4);
            classification.Add("actValue", 34.7);
            classifier.Compute(0, classification, new int[] { 1, 5, 9 }, true, true);
            //It is second iteration and so it will learn and that's why value count will be more than one
            ClassificationExperiment<double> result = classifier.Compute(1, classification, new int[] { 1, 5, 9 }, true, true);

            Assert.AreEqual(5, result.getActualValueCount());
            Assert.AreEqual(34.7, result.getActualValue(4), 0.01);
        }


        /// <summary>
        /// Test how the bucket system works when complex inputs are given to it
        /// </summary>
        [TestMethod]
        public void testComputeComplex()
        {
            var classifier = new CLAClassifier<double>(new List<int> { 1 }, 0.1, 0.1);
            int recordNum = 0;
            Dictionary<string, object> classification = new Dictionary<string, object>();
            classification.Add("bucketIdx", 4);
            classification.Add("actValue", 34.7);
            ClassificationExperiment<double> result = classifier.Compute(recordNum, classification, new int[] { 1, 5, 9 }, true, true);
            recordNum += 1;

            classification["bucketIdx"] = 5;
            classification["actValue"] = 41.7;
            result = classifier.Compute(recordNum, classification, new int[] { 0, 6, 9, 11 }, true, true);
            recordNum += 1;


            classification["bucketIdx"] = 5;
            classification["actValue"] = 44.9;
            result = classifier.Compute(recordNum, classification, new int[] { 6, 9 }, true, true);
            recordNum += 1;


            classification["bucketIdx"] = 4;
            classification["actValue"] = 42.9;
            result = classifier.Compute(recordNum, classification, new int[] { 1, 5, 9 }, true, true);
            recordNum += 1;


            classification["bucketIdx"] = 4;
            classification["actValue"] = 34.7;
            result = classifier.Compute(recordNum, classification, new int[] { 1, 5, 9 }, true, true);



            Assert.AreEqual(35.520000457763672, result.getActualValue(4), 0.00001);
            Assert.AreEqual(42.020000457763672, result.getActualValue(5), 0.00001);
            Assert.AreEqual(6, result.getStatCount(1));
            Assert.AreEqual(0.0, result.getStat(1, 0), 0.00001);
            Assert.AreEqual(0.0, result.getStat(1, 1), 0.00001);
            Assert.AreEqual(0.0, result.getStat(1, 2), 0.00001);
            Assert.AreEqual(0.0, result.getStat(1, 3), 0.00001);
            Assert.AreEqual(0.12300123, result.getStat(1, 4), 0.00001);
            Assert.AreEqual(0.87699877, result.getStat(1, 5), 0.00001);
        }

        /// <summary>
        /// Test when bucket index is missing 
        /// </summary>
        [TestMethod]
        public void TestComputeWithMissingValue()
        {
            var classifier = new CLAClassifier<double>(new List<int> { 1 }, 0.1, 0.1);
            Dictionary<string, object> classification = new Dictionary<string, object>();
            classification.Add("bucketIdx", null);
            classification.Add("actValue", null);
            ClassificationExperiment<double> result = classifier.Compute(0, classification, new int[] { 1, 5, 9 }, true, true);

            Assert.AreEqual(1, result.getActualValueCount());
            Assert.AreEqual(-1, result.getActualValue(0));
        }

        /// <summary>
        /// Test when we use same iteration number twice
        /// </summary>
        [TestMethod]
        public void testOverlapPattern()
        {
            var classifier = new CLAClassifier<double>();

            ClassificationExperiment<Double> result = compute(classifier, 0, new int[] { 1, 5 }, 9, 9);
            result = compute(classifier, 1, new int[] { 1, 5 }, 9, 9);
            result = compute(classifier, 1, new int[] { 1, 5 }, 9, 9);
            result = compute(classifier, 2, new int[] { 3, 5 }, 2, 2);

            // Since overlap - should be previous with 100%
            checkValue(result, 9, 9.0, 1.0);

            result = compute(classifier, 3, new int[] { 3, 5 }, 2, 2);

            // Second example: now new value should be more probable than old
            Assert.IsTrue(result.getStat(1, 2) > result.getStat(1, 9));
        }

    }
}