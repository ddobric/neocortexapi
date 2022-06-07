using System.Diagnostics;

namespace InvariantLearning
{
    internal class InvariantExperiment
    {
        private object invariantSet;
        private RunConfig experimentParams;
        private int count = 0;

        /// <summary>
        /// Experiment specification for Htm Invariant Object Representation Learning
        /// </summary>
        /// <param name="invariantSet">the image set used for invariant learning</param>
        /// <param name="experimentParams">experiment parameters used for experiment</param>
        public InvariantExperiment(TrainingData invariantSet, RunConfig experimentParams)
        {
            this.invariantSet = invariantSet;
            this.experimentParams = experimentParams;
        }

        internal void Train()
        {
            Debug.WriteLine("-------------- Training in Progress---------------");
        }

        internal string Predict(object v)
        {
            return "- Prediction in Progress -";
        }
    }
}