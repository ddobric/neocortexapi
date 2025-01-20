using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionTeamSynergy
{
    /// <summary>
    /// Handles file operations for training and inferring data in an anomaly detection system.
    /// </summary>
    internal class FileHandler
    {
        // Default folders for training and inferring data.
        private readonly string defaultTrainingFolder;
        private readonly string defaultInferringFolder;

        /// <summary>
        /// List of processed training data files.
        /// </summary>
        public List<string> TrainingDataFiles { get; private set; } = new List<string>();

        /// <summary>
        /// List of processed inferring data files.
        /// </summary>
        public List<string> InferringDataFiles { get; private set; } = new List<string>();

        /// <summary>
        /// Initializes the FileHandler with default folders.
        /// </summary>
        /// <param name="defaultTrainingFolder">Path to the default training folder.</param>
        /// <param name="defaultInferringFolder">Path to the default inferring folder.</param>
        public FileHandler(string defaultTrainingFolder, string defaultInferringFolder)
        {
            this.defaultTrainingFolder = defaultTrainingFolder;
            this.defaultInferringFolder = defaultInferringFolder;
        }

        /// <summary>
        /// Processes command-line arguments to determine the files and folders for training and inferring.
        /// </summary>
        /// <param name="args">Array of command-line arguments.</param>
        /// <exception cref="Exception">Thrown when no valid CSV files are found.</exception>
        public void ProcessArguments(string[] args)
        {
            string? trainingFile = null;
            string? inferringFile = null;
            string? trainingFolder = null;
            string? inferringFolder = null;

            Console.WriteLine("Processing command-line arguments...");

            // Parse console arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--training-file":
                        trainingFile = i + 1 < args.Length ? args[i + 1] : null;
                        break;
                    case "--inferring-file":
                        inferringFile = i + 1 < args.Length ? args[i + 1] : null;
                        break;
                    case "--training-folder":
                        trainingFolder = i + 1 < args.Length ? args[i + 1] : null;
                        break;
                    case "--inferring-folder":
                        inferringFolder = i + 1 < args.Length ? args[i + 1] : null;
                        break;
                }
            }

            // Process training files and folders
            Console.WriteLine("Processing training data...");
            if (!string.IsNullOrEmpty(trainingFile) && File.Exists(trainingFile) && IsCsv(trainingFile))
            {
                Console.WriteLine($"Adding training file: {trainingFile}");
                TrainingDataFiles.Add(trainingFile);
            }

            if (!string.IsNullOrEmpty(trainingFolder) && Directory.Exists(trainingFolder))
            {
                Console.WriteLine($"Adding training files from folder: {trainingFolder}");
                TrainingDataFiles.AddRange(Directory.GetFiles(trainingFolder).Where(file => IsCsv(file)));
            }

            // Process inferring files and folders
            Console.WriteLine("Processing inferring data...");
            if (!string.IsNullOrEmpty(inferringFile) && File.Exists(inferringFile) && IsCsv(inferringFile))
            {
                Console.WriteLine($"Adding inferring file: {inferringFile}");
                InferringDataFiles.Add(inferringFile);
            }

            if (!string.IsNullOrEmpty(inferringFolder) && Directory.Exists(inferringFolder))
            {
                Console.WriteLine($"Adding inferring files from folder: {inferringFolder}");
                InferringDataFiles.AddRange(Directory.GetFiles(inferringFolder).Where(file => IsCsv(file)));
            }

            // Use default folders if no arguments are provided
            if (TrainingDataFiles.Count == 0 && string.IsNullOrEmpty(trainingFile) && string.IsNullOrEmpty(trainingFolder))
            {
                Console.WriteLine("Using default training folder...");
                if (Directory.Exists(defaultTrainingFolder))
                {
                    TrainingDataFiles.AddRange(Directory.GetFiles(defaultTrainingFolder).Where(file => IsCsv(file)));
                }
            }

            if (InferringDataFiles.Count == 0 && string.IsNullOrEmpty(inferringFile) && string.IsNullOrEmpty(inferringFolder))
            {
                Console.WriteLine("Using default inferring folder...");
                if (Directory.Exists(defaultInferringFolder))
                {
                    InferringDataFiles.AddRange(Directory.GetFiles(defaultInferringFolder).Where(file => IsCsv(file)));
                }
            }

            // Validate results
            if (TrainingDataFiles.Count == 0 && InferringDataFiles.Count == 0)
            {
                Console.WriteLine("Error: No CSV files found.");
                throw new Exception("No CSV files found in specified or default locations.");
            }

            Console.WriteLine("Training data files:");
            TrainingDataFiles.ForEach(file => Console.WriteLine($"  {file}"));

            Console.WriteLine("Inferring data files:");
            InferringDataFiles.ForEach(file => Console.WriteLine($"  {file}"));
        }

        /// <summary>
        /// Checks if a given file has a .csv extension.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>True if the file is a CSV file; otherwise, false.</returns>
        private bool IsCsv(string filePath)
        {
            return Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
