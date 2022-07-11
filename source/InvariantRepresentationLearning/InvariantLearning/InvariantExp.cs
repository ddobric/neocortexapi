using NeoCortexApi;
using System.Diagnostics;
using System.Linq;
using Invariant.Entities;

namespace InvariantLearning
{
    internal class InvariantExperiment
    {
        private DataSet trainingDataSet;
        private RunConfig runParams;
        Dictionary<string, LearningUnit> poolerDict;

        /// <summary>
        /// Experiment specification for Htm Invariant Object Representation Learning
        /// </summary>
        /// <param name="trainigDataSet">the image set used for invariant learning</param>
        /// <param name="runParams">experiment parameters used for experiment</param>
        public InvariantExperiment(DataSet trainigDataSet, RunConfig runParams)
        {
            this.trainingDataSet = trainigDataSet;
            this.runParams = runParams;
            poolerDict = new Dictionary<string, LearningUnit>();
        }


        /// <summary>
        /// 
        /// </summary>
        internal async void Train(bool visualizeTrainingImages = true)
        {
            Debug.WriteLine($"-------------- Training in Progress with {runParams.Epoch} epochs---------------");

            CreateSpatialPoolers();

            if(visualizeTrainingImages)
                VisualizeTrainingImages($"TrainingSet_{Utility.GetHash()}");

            //
            // Performs the training
            // It iterates through all learning units and learns corresponding images.
            foreach (var unit in poolerDict)
            {
                Debug.WriteLine("Stage 1: Training in Newborn cycle");
                unit.Value.TrainingNewbornCycle(trainingDataSet);

                Debug.WriteLine("Stage 2: Training of Images");
                // for loop with epochs
                for (int epoch = 1; epoch <= runParams.Epoch; epoch += 1)
                {
                    // for loop with training:
                    foreach (var sample in trainingDataSet.images)
                    {
                        unit.Value.Learn(sample);
                    }
                }
            }
        }

        /// <summary>
        /// Visualizing the input image after preprocessing befor they get into every spatial poolers
        /// </summary>
        /// <param name="trainDataFolder">the training input folder</param>
        private void VisualizeTrainingImages(string trainDataFolder)
        {
            foreach (var dim in poolerDict.Keys)
            {
                string pathForTrainDataOfOneSpatialPooler = Path.Combine(trainDataFolder, $"SP dim {dim.ToString()}");

                Utility.CreateFolderIfNotExist(pathForTrainDataOfOneSpatialPooler);

                foreach (var label in trainingDataSet.imageClasses)
                {
                    string pathForImageInOneLabelFolder = Path.Combine(trainDataFolder, $"SP dim {dim.ToString()}", label);

                    Utility.CreateFolderIfNotExist(pathForImageInOneLabelFolder);

                    var imagesFilteredByLabel = trainingDataSet.images.Where(a => (a.label == label));

                    int index = 0;

                    foreach (var img in imagesFilteredByLabel)
                    {
                        string imagePath = Path.Combine(pathForImageInOneLabelFolder, $"{index}.png");
                        img.SaveImageWithSquareDimension(imagePath, int.Parse(dim));
                        index++;
                    }
                }
            }
        }

        private void CreateSpatialPoolers()
        {
            // Initiate Spatial Pooler Dictionaries
            var spFrameSizeLst = Frame.GetIndexes(28, 40, 2);

            foreach (var dim in spFrameSizeLst)
            {
                poolerDict.Add(dim.ToString(), new LearningUnit(dim, 1024));
            }
        }

        /// <summary>
        /// Validate the training process
        /// </summary>
        /// <param name="times"></param>
        internal void Validate(int times)
        {
            int correctGuess = 0;
            for (int time = 0; time < times; time += 1)
            {
                (string predicted, string realLabel) = Predict(trainingDataSet.PickRandom());
                correctGuess += (predicted == realLabel) ? 1 : 0;
            }
            Debug.WriteLine($"validation of {times} datapoints: {(double)(correctGuess / times)}");
        }

        /// <summary>
        /// Predict the object, returning the correct label from the set and the predicted label
        /// </summary>
        /// <param name="v">input image in InvImage</param>
        /// <returns></returns>
        internal (string, string) Predict(Picture inputImage)
        {
            List<Dictionary<string, double>> allSPPredictResult = new List<Dictionary<string, double>>();

            // Prepare Output Folder
            string predictProcessName = Path.GetFileNameWithoutExtension(inputImage.imagePath) + Utility.GetHash();
            Utility.CreateFolderIfNotExist(Path.Combine("Predict", predictProcessName));

            // Collecting Vote
            foreach (var sp in poolerDict)
            {
                sp.Value.OutputPredictFolder = predictProcessName;
                Dictionary<string, double> predictResultOfCurrentSP = sp.Value.Predict(inputImage);
                allSPPredictResult.Add(predictResultOfCurrentSP);

                //Write result to file
                Utility.WriteResultOfOneSP(predictResultOfCurrentSP, Path.Combine("Predict", predictProcessName, $"sp_{sp.Key}"));
            }

            // Calculating Vote
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (var label in trainingDataSet.imageClasses)
            {
                result.Add(label, 0);
            }

            foreach (var spVote in allSPPredictResult)
            {
                foreach (var classResult in spVote)
                {
                    result[classResult.Key] += classResult.Value;
                }
            }
            // Check highest label score
            var predictedLabel = result.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

            return (predictedLabel, inputImage.label);
        }
    }
}