// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Exception;
using System;
using System.Collections.Generic;

namespace UnitTestsProject
{

    // Note since we have implemented the matrix as a array and array starts with zero. So what ever bucket is predicted 
    // add 1 to it. Since prediction is made keeping 0 as the base bucket index.
    [TestClass]
    public class SDRClassifierTest
    {
        private const string EMPTY_BUCKET_EXCEPTION = "Unable to predict, since bucket is empty";

        private const string CLASSIFICATION_CANNOT_BE_NULL = "Classification cannot be null";

        private const string PATTERN_NZ_CANNOT_BE_NULL = "Pattern cannot be null or empty";


        /// <summary>
        /// This test checks if Default Constructor for alpha is created or not.
        /// </summary>
        [TestMethod]
        public void TestDefaultConstructor()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            Assert.IsNotNull(sdr);

        }

        /// <summary>
        /// This test checks if object for alpha is created or not.
        /// </summary>
        [TestMethod]
        public void TestObjectCreated()
        {
            double alpha = 0.001;
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>(alpha);
            Assert.IsNotNull(sdr);

        }


        /// <summary>
        /// This test checks for exception throwing in case of classification object is null. 
        /// </summary>
        [TestMethod]
        public void TestComputeShouldThrowObjectShouldNotBeNUllExceptionForClassification()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            try
            {
                sdr.Compute(0, null, null);

            }
            catch (ObjectShouldNotBeNUllException ex)
            {
                Assert.AreEqual(CLASSIFICATION_CANNOT_BE_NULL, ex.Message);
            }
        }

        /// <summary>
        /// This test checks for exception throwing in case of PatternNZ  object is null.
        /// </summary>
        [TestMethod]
        public void TestComputeShouldThrowObjectShouldNotBeNUllExceptionForPatternNZ()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            List<object> list = new List<object>();
            try
            {
                sdr.Compute(0, list, null);
            }
            catch (ObjectShouldNotBeNUllException ex)
            {
                Assert.AreEqual(PATTERN_NZ_CANNOT_BE_NULL, ex.Message);
            }
        }


        /// <summary>
        /// This test trains the classifier with ten values, which are associated to the some bucket and TM array of active cells.
        /// Then the test checks for the prediction made 
        /// </summary>
        [TestMethod]
        public void TestComputeSingleValueMultipleIterations()
        {
            double alpha = 1.0;
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>(alpha);
            List<Object> classification = new List<Object>(); // contains bucket index and actual values
            classification.Add(0);
            classification.Add(10);
            for (int i = 0; i < 10; i++)
            {
                sdr.Compute(i, classification, new int[] { 1, 5 });
            }
            Assert.AreEqual(1, sdr.Predict(new int[] { 1, 5 })[0], 0.5);

        }



        /// <summary>
        /// Tests the compute and learning algorithm using single iteration and validates the result by comparing the probabiliy of each
        /// bucket.
        /// </summary>
        [TestMethod]
        public void TestComputeSingleValueOneIteration()
        {
            double alpha = 0.1;
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>(alpha);
            List<Object> classification = new List<object>();
            classification.Add(4);
            classification.Add(34.7);

            // After this step bucket values will be updated
            sdr.Compute(0, classification, new int[] { 1, 5 });
            Assert.AreEqual(34.7, sdr.bucketEntries[4][0]);
            double[] predictionResult = sdr.Predict(new int[] { 1, 5 });

            // NOTE: if you take some of all predictions it should round upto ~ 1
            Assert.AreEqual(0.19151941467016101, predictionResult[0]);
            Assert.AreEqual(0.19151941467016101, predictionResult[1]);
            Assert.AreEqual(0.19151941467016101, predictionResult[2]);
            Assert.AreEqual(0.19151941467016101, predictionResult[3]);

            // Implies bucket 4 has highest probability to come
            Assert.AreEqual(0.23392234131935596, predictionResult[4]);

        }

        /// <summary>
        /// Test how the bucket system works when complex inputs are given to it
        /// </summary>
        [TestMethod]
        public void TestComplexLearning()
        {
            double alpha = 0.1;
            int recordNumber = 1;
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>(alpha);
            List<object> classification = new List<object>(2);
            classification.Add(4);
            classification.Add(34.7);
            sdr.Compute(recordNumber, classification, new int[] { 1, 5, 9 });

            recordNumber += 1;
            classification[0] = 5;
            classification[1] = 41.7;
            sdr.Compute(recordNumber, classification, new int[] { 0, 6, 9, 11 });

            recordNumber += 1;
            classification[0] = 5;
            classification[1] = 44.9;
            sdr.Compute(recordNumber, classification, new int[] { 6, 9 });

            recordNumber += 1;
            classification[0] = 4;
            classification[1] = 42.9;
            sdr.Compute(recordNumber, classification, new int[] { 1, 5, 9 });

            classification[0] = 4;
            classification[1] = 34.7;
            double[] predictedValues = sdr.Predict(new int[] { 1, 5, 9 });
            Assert.AreEqual(0.11184534926411586, predictedValues[0]);
            Assert.AreEqual(0.11184534926411586, predictedValues[1]);
            Assert.AreEqual(0.11184534926411586, predictedValues[2]);
            Assert.AreEqual(0.11184534926411586, predictedValues[3]);
            Assert.AreEqual(0.30775551952585922, predictedValues[4]);
            Assert.AreEqual(0.24486308341767732, predictedValues[5]);

        }

        /// <summary>
        /// This test checks for exception throwing in case of bucket is empty
        /// </summary>
        [TestMethod]
        public void TestPredictShouldThrowEmptyBucketException()
        {

            double alpha = 0.1;
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>(alpha);
            List<Object> classification = new List<object>();
            classification.Add(4);
            classification.Add(34.7);

            try
            {
                sdr.Predict(new int[] { 1, 5 });

            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, EMPTY_BUCKET_EXCEPTION);
            }
        }


        /// <summary>
        /// This test checks for growth of matrix according the inputs provided. 
        /// </summary>
        [TestMethod]
        public void TestGrowingMatrixAccordingToInputs()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            List<Object> classification = new List<object>();
            classification.Add(4);
            classification.Add(34.7);
            int recordNumber = 0;
            sdr.Compute(recordNumber, classification, new int[] { 0, 6, 9, 11 });

            // Doing minus one since in the code we are actually creating a column at 11th index
            Assert.AreEqual(11, sdr.weightMatrix.Matrix[0].Count - 1);
            sdr.Compute(recordNumber, classification, new int[] { 0, 6, 9, 5 });

            // No change since the maximum input value remains 11 only
            Assert.AreEqual(11, sdr.weightMatrix.Matrix[0].Count - 1);
        }


        /// <summary>
        /// This test checks for growth of Matrix according to the buckets.
        /// </summary>
        [TestMethod]
        public void TestGrowingMatrixAccordingToBuckets()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            List<Object> classification = new List<object>();
            classification.Add(0);
            classification.Add(34.7);
            int recordNumber = 0;
            sdr.Compute(recordNumber, classification, new int[] { 0, 6, 9, 11 });
            Assert.IsTrue(sdr.weightMatrix.Matrix.Count == 1);

            classification[0] = 4;
            classification[1] = 34.7;
            sdr.Compute(1, classification, new int[] { 0, 6, 9, 11 });
            Assert.IsTrue(sdr.weightMatrix.Matrix.Count == 5);

            classification[0] = 6;
            classification[1] = 34.7;
            sdr.Compute(2, classification, new int[] { 0, 6, 9, 11 });
            Assert.IsTrue(sdr.weightMatrix.Matrix.Count == 7);

            classification[0] = 4;
            classification[1] = 34.7;
            sdr.Compute(recordNumber++, classification, new int[] { 0, 6, 9, 11 });
            Assert.IsTrue(sdr.weightMatrix.Matrix.Count == 7);

        }

        /// <summary>
        /// This test checks for a single value in bucket.
        /// </summary>
        [TestMethod]
        public void TestBucketEntriesSingleInput()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            List<Object> classification = new List<object>();
            classification.Add(4);
            classification.Add(34.7);
            sdr.Compute(0, classification, new int[] { 0, 6, 9, 11 });

            Assert.AreEqual(1, sdr.bucketEntries.Count);
            Assert.AreEqual(34.7, sdr.bucketEntries[4][0]);

        }

        /// <summary>
        /// This test checks for multiple values in bucket.
        /// </summary>
        [TestMethod]
        public void TestBucketEntriesMultipleInputs()
        {
            SdrClassifier<int, int> sdr = new SdrClassifier<int, int>();
            List<Object> classification = new List<object>();
            classification.Add(4);
            classification.Add(34.7);
            sdr.Compute(0, classification, new int[] { 0, 6, 9, 11 });

            classification[0] = 2;
            classification[1] = 35.7;
            sdr.Compute(0, classification, new int[] { 0, 6, 9, 11 });

            classification[0] = 3;
            classification[1] = 36.7;
            sdr.Compute(0, classification, new int[] { 0, 6, 9, 11 });

            classification[0] = 4;
            classification[1] = 37.7;
            sdr.Compute(0, classification, new int[] { 0, 6, 9, 11 });

            classification[0] = 5;
            classification[1] = 38.7;
            sdr.Compute(0, classification, new int[] { 0, 6, 9, 11 });

            Assert.AreEqual(4, sdr.bucketEntries.Count);
            Assert.AreEqual(34.7, sdr.bucketEntries[4][0]);
            Assert.AreEqual(35.7, sdr.bucketEntries[2][0]);
            Assert.AreEqual(36.7, sdr.bucketEntries[3][0]);
            Assert.AreEqual(37.7, sdr.bucketEntries[4][1]);
            Assert.AreEqual(38.7, sdr.bucketEntries[5][0]);
        }
    }
}

