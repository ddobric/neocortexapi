using NeoCortexApi;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace AnomalyDetectionSample
{

    public class HTMTrainingManager
    {
        /// <summary>
        /// Executes the HTM model training experiment using CSV files from specified folders and returns the trained predictor.
        /// </summary>
        /// <param name="trainingFolderPath">The path to the folder containing the CSV files used for training.</param>
        /// <param name="predictionFolderPath">The path to the folder containing the CSV files used for prediction.</param>
        /// <param name="trainedPredictor">The trained model that will be used for prediction.</param>
        public void ExecuteHTMModelTraining(string trainingFolderPath, string predictionFolderPath, out Predictor trainedPredictor)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            CsvSequenceFolder trainingReader = new CsvSequenceFolder(trainingFolderPath);
            var trainingSequences = trainingReader.ExtractSequencesFromFolder();

            // Read numerical sequences from CSV files in the specified prediction folder
            CsvSequenceFolder predictionReader = new CsvSequenceFolder(predictionFolderPath);
            var predictionSequences = predictionReader.ExtractSequencesFromFolder();

            // Combine sequences from both training and prediction folders
            List<List<double>> combinedSequences = new List<List<double>>(trainingSequences);
            combinedSequences.AddRange(predictionSequences);

            // Convert sequences to HTM input format
            CSVToHTMInputConverter sequenceConverter = new CSVToHTMInputConverter();
            var htmInput = sequenceConverter.ConvertToHTMInput(combinedSequences);

            // Start multi-sequence learning experiment to generate predictor model
            MultiSequenceLearning learningAlgorithm = new MultiSequenceLearning();
            trainedPredictor = learningAlgorithm.Run(htmInput);

            // HTM model training completed

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("HTM training completed! Total training time: " + stopwatch.Elapsed.TotalSeconds + " seconds.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
        }
    }
}
