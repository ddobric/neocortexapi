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
    }
}
