using NeoCortexApi;
using System.Collections.Generic;

class SDRClassifierExample
{
    static void Main()
    {
        // Prepare the training data
        var inputData = new List<double[]>()
        {
            new double[] { 0, 0, 1, 1, 1, 0, 0, 1 },
            new double[] { 1, 0, 1, 1, 0, 1, 0, 1 },
            new double[] { 1, 1, 0, 1, 1, 0, 1, 0 },
            // Add more data as needed
        };
        var outputData = new List<double[]>()
        {
            new double[] { 0, 1, 0, 0 },
            new double[] { 1, 0, 0, 0 },
            new double[] { 0, 0, 1, 0 },
            // Add more data as needed
        };

        // Create an SDR classifier with the desired parameters
        var sdr = new SDR(inputCount: 8, outputCount: 4);
        sdr.Sparsity = 0.05;
        sdr.LearningRate = 0.1;
        sdr.Iterations = 1000;
        sdr.BatchSize = 100;

        // Train the classifier using the training data
        sdr.Train(inputData, outputData);

        // Use the trained classifier to classify new data
        var input = new double[] { 0, 1, 0, 1, 1, 0, 1, 1 };
        var output = sdr.Compute(input);
    }
}