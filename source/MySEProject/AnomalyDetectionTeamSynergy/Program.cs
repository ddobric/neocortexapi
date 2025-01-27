using NeoCortexApi;
using System.Linq.Expressions;

namespace AnomalyDetectionTeamSynergy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            string defaultTrainingFolder = Path.Combine(projectbaseDirectory, "TrainingData");
            string defaultInferringFolder = Path.Combine(projectbaseDirectory, "InferringData");

            var fileHandler = new FileHandler(defaultTrainingFolder, defaultInferringFolder);

            try
            {
                fileHandler.ProcessArguments(args);
                var training_files = fileHandler.TrainingDataFiles;
                var inferring_files = fileHandler.InferringDataFiles;

                var csv_reader = new CSVReader();
                var csv_htm_input = new CSVToHTMInput();

                foreach (var filePath in fileHandler.TrainingDataFiles)
                {
                    Console.WriteLine($"\n--- Reading File: {Path.GetFileName(filePath)} ---");

                    var training_sequences = csv_reader.ParseSequencesFromCSV(filePath);
                    csv_reader.DisplaySequenceData(training_sequences);
                    var htm_training_sequence = csv_htm_input.BuildHTMInput(training_sequences);

                    MultiSequenceLearning learning = new MultiSequenceLearning();
                    var predictor = learning.Run(htm_training_sequence);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
          //  RunMultiSequenceLearningExperiment.Run();
        }
       
    }
}
