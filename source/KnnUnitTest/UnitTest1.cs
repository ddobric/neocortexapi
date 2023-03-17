using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
//using NUnit.Framework;


namespace KNNClassifier.Tests
{
    [TestClass]
    public class KNNClassifierTests
    {
        [TestMethod]
        public void TestPredict()
        {
            // Loading the base data for traning along with labels
            double[][] trainingData = LoadTrainingData();
            int[] labels = { 0, 1, 0, 1, 1, 0 };
            KNNClassifier knn = new KNNClassifier(k: 4);
            knn.Train(trainingData, labels);
            double[] testData = { 1.2, 3.4, 5.6, 5.2, 2.2, 1.6 };

            // Predicting the inputs on test data 
            int predictedLabel = knn.Predict(testData);

            // Preddiction labeling 
            Assert.AreEqual(1, predictedLabel);
        }
    }
}
