using System;
using System.Collections.Generic;
using System.IO;
using CSVFileHandling;


namespace AnomalyDetectionTeamSynergy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            string defaultTrainingFolder = Path.Combine(projectbaseDirectory, "TrainingData");
            string defaultInferringFolder = Path.Combine(projectbaseDirectory, "InferringData");

            Console.WriteLine("Training Data Path: " + defaultTrainingFolder);
            Console.WriteLine("Inferring Data Path: " + defaultInferringFolder);

            try
            {
                // Check if folder exists
                if (!Directory.Exists(defaultTrainingFolder))
                {
                    throw new DirectoryNotFoundException("The specified folder does not exist.");
                }

                // Get all CSV files in the folder
                string[] csvFiles = Directory.GetFiles(defaultTrainingFolder, "*.csv");

                if (csvFiles.Length == 0)
                {
                    throw new FileNotFoundException("No CSV files found in the specified folder.");
                }

                // Create an instance of the CSVFilesFolderReader class
                var csvReader = new CSVFileReader();

                foreach (var filePath in csvFiles)
                {
                    Console.WriteLine($"\n--- Reading File: {Path.GetFileName(filePath)} ---");

                    // Read and display the CSV data
                    var csvData = csvReader.ReadCSVFile(filePath);
                    csvReader.DisplayCSVData(csvData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            RunMultiSequenceLearningExperiment.Run();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
