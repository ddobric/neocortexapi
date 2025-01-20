using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDetectionTeamSynergy
{
    internal class FileHandler
    {
        private readonly string defaultTrainingFolder;
        private readonly string defaultInferringFolder;

        public List<string> TrainingDataFiles { get; private set; } = new List<string>();
        public List<string> InferringDataFiles { get; private set; } = new List<string>();

        public FileHandler(string defaultTrainingFolder, string defaultInferringFolder)
        {
            this.defaultTrainingFolder = defaultTrainingFolder;
            this.defaultInferringFolder = defaultInferringFolder;
        }

        public void ProcessArguments(string[] args)
        {
            string? trainingFile = null;
            string? inferringFile = null;
            string? trainingFolder = null;
            string? inferringFolder = null;

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
            if (!string.IsNullOrEmpty(trainingFile) && File.Exists(trainingFile) && IsCsv(trainingFile))
            {
                TrainingDataFiles.Add(trainingFile);
            }

            if (!string.IsNullOrEmpty(trainingFolder) && Directory.Exists(trainingFolder))
            {
                TrainingDataFiles.AddRange(Directory.GetFiles(trainingFolder).Where(file => IsCsv(file)));
            }

            // Process inferring files and folders
            if (!string.IsNullOrEmpty(inferringFile) && File.Exists(inferringFile) && IsCsv(inferringFile))
            {
                InferringDataFiles.Add(inferringFile);
            }

            if (!string.IsNullOrEmpty(inferringFolder) && Directory.Exists(inferringFolder))
            {
                InferringDataFiles.AddRange(Directory.GetFiles(inferringFolder).Where(file => IsCsv(file)));
            }

            // Use default folders if no arguments are provided
            if (TrainingDataFiles.Count == 0 && string.IsNullOrEmpty(trainingFile) && string.IsNullOrEmpty(trainingFolder))
            {
                if (Directory.Exists(defaultTrainingFolder))
                {
                    TrainingDataFiles.AddRange(Directory.GetFiles(defaultTrainingFolder).Where(file => IsCsv(file)));
                }
            }

            if (InferringDataFiles.Count == 0 && string.IsNullOrEmpty(inferringFile) && string.IsNullOrEmpty(inferringFolder))
            {
                if (Directory.Exists(defaultInferringFolder))
                {
                    InferringDataFiles.AddRange(Directory.GetFiles(defaultInferringFolder).Where(file => IsCsv(file)));
                }
            }

            // Validate results
            if (TrainingDataFiles.Count == 0 && InferringDataFiles.Count == 0)
            {
                throw new Exception("No CSV files found in specified or default locations.");
            }
        }

        private bool IsCsv(string filePath)
        {
            return Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
