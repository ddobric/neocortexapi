using NeoCortexApi;
using System.Diagnostics;
using System.Linq;
using Invariant.Entities;

namespace InvariantLearning
{
    internal class InvariantExperimentImageClassification
    {
        private DataSet trainingDataSet;
        private RunConfig runParams;
        Dictionary<string, LearningUnit> poolerDict;

        /// <summary>
        /// Experiment specification for Htm Invariant Object Representation Learning
        /// </summary>
        /// <param name="trainigDataSet">the image set used for invariant learning</param>
        /// <param name="runParams">experiment parameters used for experiment</param>
        public InvariantExperimentImageClassification(DataSet trainigDataSet, RunConfig runParams)
        {
            this.trainingDataSet = trainigDataSet;
            this.runParams = runParams;
            poolerDict = new Dictionary<string, LearningUnit>();
        }


        /// <summary>
        /// 
        /// </summary>
        internal void Train(bool visualizeTrainingImages = true)
        {
            Debug.WriteLine($"-------------- Training in Progress with {runParams.Epoch} epochs---------------");

            CreateSpatialPoolers();

            if(visualizeTrainingImages)
                VisualizeTrainingImages($"TrainingSet_{Utility.GetHash()}");
            
            Debug.WriteLine("Stage 1: Training in Newborn cycle");

            Parallel.ForEach(poolerDict, new ParallelOptions(), (unit) =>
            {
                unit.Value.TrainingNewbornCycle(trainingDataSet);
            });
            
            
            Debug.WriteLine("Stage 2: Training of Images");

            Parallel.ForEach(poolerDict, new ParallelOptions(), (unit)=>
            {
                unit.Value.TrainingNormal(trainingDataSet,runParams.Epoch);
            });

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

                foreach (var label in trainingDataSet.ImageClasses)
                {
                    string pathForImageInOneLabelFolder = Path.Combine(trainDataFolder, $"SP dim {dim.ToString()}", label);

                    Utility.CreateFolderIfNotExist(pathForImageInOneLabelFolder);

                    var imagesFilteredByLabel = trainingDataSet.Images.Where(a => (a.label == label));

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
                poolerDict.Add(dim.ToString(), new LearningUnit(dim, 2048));
            }
        }

        /// <summary>
        /// Predict the object, returning the correct label from the set and the predicted label
        /// </summary>
        /// <param name="v">input image in InvImage</param>
        /// <returns></returns>
        internal Dictionary<string, Dictionary<string, string>> Predict(Picture inputImage, string kFold = "")
        {
            Dictionary<string, Dictionary<string, string>> allSPPredictResult = new Dictionary<string, Dictionary<string, string>>();

            // Prepare Output Folder
            string predictProcessName = Path.GetFileNameWithoutExtension(inputImage.imagePath) + Utility.GetHash();
            Utility.CreateFolderIfNotExist(Path.Combine($"Predict_{kFold}", predictProcessName));

            // Collecting Vote
            foreach (var sp in poolerDict)
            {
                sp.Value.OutputPredictFolder = predictProcessName;
                Dictionary<string, string> predictResultOfCurrentSP = sp.Value.PredictScaledImage(inputImage, $"Predict_{kFold}");
                predictResultOfCurrentSP.Add("CorrectLabel",inputImage.label);
                allSPPredictResult.Add(sp.Key,predictResultOfCurrentSP);
            }
            return allSPPredictResult;
        }
    }
}