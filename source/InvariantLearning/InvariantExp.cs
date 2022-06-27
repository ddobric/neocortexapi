using NeoCortexApi;
using System.Diagnostics;

namespace InvariantLearning
{
    internal class InvariantExperiment
    {
        private DataSet invariantSet;
        private RunConfig runParams;
        Dictionary<string, LearningUnit> poolerDict;

        /// <summary>
        /// Experiment specification for Htm Invariant Object Representation Learning
        /// </summary>
        /// <param name="invariantSet">the image set used for invariant learning</param>
        /// <param name="runParams">experiment parameters used for experiment</param>
        public InvariantExperiment(DataSet invariantSet, RunConfig runParams)
        {
            this.invariantSet = invariantSet;
            this.runParams = runParams;
            poolerDict = new Dictionary<string, LearningUnit>();
        }

        internal void Train()
        {
            Debug.WriteLine($"-------------- Training in Progress with {runParams.Epoch} epochs---------------");
            #region training process
            // BEGIN TRAINING

            // Initiate Spatial Pooler Dictionaries
            foreach(var dim in Frame.GetDivisionIndex(10,invariantSet.images[0].imageHeight,10))
            {
                poolerDict.Add(dim.ToString(),new LearningUnit(dim,1024));
            }

            // iterate through all learning unit
            foreach (var unit in poolerDict) {
                // for loop with epochs
                for (int epoch = 1; epoch <= runParams.Epoch; epoch += 1)
                {
                    // for loop with training:
                    foreach(var sample in invariantSet.images)
                    {
                        unit.Value.Learn(sample);
                    }
                }
            }
            // END TRAINING
            #endregion
        }

        /// <summary>
        /// Validate the training process
        /// </summary>
        /// <param name="times"></param>
        internal void Validate(int times)
        {
            int correctGuess = 0;
            for(int time = 0; time< times; time += 1)
            {
                (string predicted, string realLabel) = Predict(invariantSet.PickRandom());
                correctGuess += (predicted == realLabel)? 1 : 0;
            }
            Debug.WriteLine($"validation of {times} datapoints: {(double)(correctGuess/times)}");
        }

        /// <summary>
        /// Predict the object, returning the correct label from the set and the predicted label
        /// </summary>
        /// <param name="v">input image in InvImage</param>
        /// <returns></returns>
        internal (string, string) Predict(Picture inputImage)
        {
            List<Dictionary<string,double>> cosensus = new List<Dictionary<string,double>>();

            // Prepare Output Folder
            string predictProcessName = Path.GetFileNameWithoutExtension(inputImage.imagePath) + Utility.GetHash();
            Utility.CreateFolderIfNotExist(Path.Combine("Predict", predictProcessName));

            // Collecting Vote
            foreach (var sp in poolerDict)
            {
                sp.Value.OutputPredictFolder = predictProcessName;
                Dictionary<string, double> a = sp.Value.Predict(inputImage);
                cosensus.Add(a);
            }

            // Calculating Vote
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach(var label in invariantSet.classes)
            {
                result.Add(label, 0);
            }

            foreach (var spVote in cosensus)
            {
                foreach (var classResult in spVote)
                {
                    result[classResult.Key]+= classResult.Value;
                }
            }
            // Check highest label score
            var predictedLabel = result.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

            return (predictedLabel, inputImage.label);
        }
    }
}