using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NUnit.Framework;


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
        private double[][] LoadTrainingData()
        {
            double[][] trainingData = new double[6][];
            trainingData[0] = new double[] { 0.1, 0.2, 0.3, 1.2, 1.3, 1.4 };
            trainingData[1] = new double[] { 1.2, 1.3, 1.4, 1.0, 3.0, 4.0 };
            trainingData[2] = new double[] { 2.3, 2.4, 2.5, 5.2, 3.3, 1.4 };
            trainingData[3] = new double[] { 3.4, 3.5, 3.6, 0.1, 0.2, 0.3 };
            trainingData[4] = new double[] { 2.3, 2.4, 2.5, 5.2, 3.3, 1.4 };
            trainingData[5] = new double[] { 3.4, 3.5, 3.6, 0.1, 0.2, 0.3 };
            return trainingData;
        }

    }
}
