using NeoCortexApi;
using System.Diagnostics;

namespace AnomalyDetectionTeamSynergy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunMultiSequenceLearningExperiment();
        }

        /// <summary>
        /// This example demonstrates how to learn two sequences and how to use the prediction mechanism.
        /// First, two sequences are learned.
        /// Second, three short sequences with three elements each are created und used for prediction. The predictor used by experiment privides to the HTM every element of every predicting sequence.
        /// The predictor tries to predict the next element.
        /// </summary>
        private static void RunMultiSequenceLearningExperiment()
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            sequences.Add("S1", new List<double>(new double[] { 1, 3, 5, 7, 9, 10}));
            sequences.Add("S2", new List<double>(new double[] { 1, 3, 6, 7, 8, 10}));
            sequences.Add("S3", new List<double>(new double[] { 3, 4, 5, 7, 9, 11 }));

            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);

            var list1 = new double[] { 1, 3, 5, 7, 9 };

        }
    }
}
