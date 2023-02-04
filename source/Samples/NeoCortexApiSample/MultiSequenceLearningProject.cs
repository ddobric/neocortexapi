using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;


namespace MultiSequencePrediction
{
    class Program
    {
        static void Main(string[] args)
        {
            List<List<double>> sequences = new List<List<double>>()
            {
                new List<double>() {1, 2, 3, 4},
                new List<double>() {5, 6, 7, 8},
                new List<double>() {9, 10, 11, 12}
            };

            int windowSize = 2;
            List<double> predictions = Predict(sequences, windowSize);

            Console.WriteLine("Predictions: ");
            predictions.ForEach(p => Console.WriteLine(p));
        }

        static List<double> Predict(List<List<double>> sequences, int windowSize)
        {
            List<double> predictions = new List<double>();

            foreach (var sequence in sequences)
            {
                for (int i = windowSize; i < sequence.Count; i++)
                {
                    List<double> window = sequence.Skip(i - windowSize).Take(windowSize).ToList();
                    double prediction = window.Average();
                    predictions.Add(prediction);
                }
            }

            return predictions;
        }
    }
}

