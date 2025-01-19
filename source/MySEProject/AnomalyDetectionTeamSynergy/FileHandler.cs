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
            string trainingFile;
            string inferringFile;
            string trainingFolder;
            string inferringFolder;

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
        }

        }
}
