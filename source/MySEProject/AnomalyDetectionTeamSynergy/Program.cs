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

                foreach (var filePath in fileHandler.TrainingDataFiles)
                {
                    Console.WriteLine($"\n--- Reading File: {Path.GetFileName(filePath)} ---");
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
