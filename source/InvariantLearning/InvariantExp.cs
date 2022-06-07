using NeoCortexApi;
using System.Diagnostics;

namespace InvariantLearning
{
    internal class InvariantExperiment
    {
        private TrainingData invariantSet;
        private RunConfig experimentParams;

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
            Debug.WriteLine($"-------------- Training in Progress with {experimentParams.epochs} epochs---------------");
            #region training process
            // BEGIN EXPERIMENT
            // Initiate Spatial Pooler
            
            // for loop with epochs
            for (int epoch = 1; epoch <= experimentParams.epochs; epoch += 1)
            {
                
            }
            // END EXPERIMENT
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
        internal (string, string) Predict(InvImage v)
        {
            return ("- Prediction in Progress -", v.label);
        }
    }
}