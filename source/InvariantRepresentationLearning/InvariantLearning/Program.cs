using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using InvariantLearning;
using Invariant.Entities;
using MnistDataGen;

namespace InvariantLearning
{
    public class InvariantLearning
    {
        public static void Main()
        {
            //ExperimentPredictingWithFrameGrid();
            ExperimentNormalImageClassification();
        }

        private static void ExperimentNormalImageClassification()
        {
            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            string pathToTrainDataFolder = config.PathToTrainDataFolder;

            //Mnist.DataGenAll("MnistDataset", "TrainingFolder");
            Mnist.DataGen("MnistDataset", "TrainingFolder",20);

            List<DataSet> testingData = new List<DataSet>();
            List<DataSet> trainingData = new List<DataSet>();

            DataSet originalTrainingDataSet = new DataSet(pathToTrainDataFolder);

            int k = 5;

            (trainingData, testingData) = originalTrainingDataSet.KFoldDataSetSplit(k);

            Dictionary<string, double> foldValidationResult = new Dictionary<string, double>();

            for (int i = 0; i < k; i += 1)
            {
                // passing the training data to the training experiment
                InvariantExperimentImageClassification experiment = new(trainingData[i], config.runParams);

                // train the network
                experiment.Train(false);

                double currentAccuracy = 0;

                foreach (var testImage in testingData[i].Images)
                {
                    Utility.CreateFolderIfNotExist($"Predict_{i}");
                    List<string> currentResList = new List<string>();

                    var result = experiment.Predict(testImage, i.ToString());

                    Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");

                    currentResList.Add($"{result.Item1}_{result.Item2}");

                    currentAccuracy = Utility.AccuracyCal(currentResList);

                    Utility.WriteOutputToFile(Path.Combine($"Predict_{i}", $"{Utility.GetHash()}_____PredictionOutput of testImage label {testImage.label}"), result);
                }

                foldValidationResult.Add($"Fold_{i}_accuracy", currentAccuracy);
            }
            Utility.WriteResultOfOneSP(foldValidationResult, $"KFold_{k}_Validation_Result");
        }

        private static void ExperimentPredictingWithFrameGrid()
        {
            // populate the training and testing dataset with Mnist DataGen
            Mnist.DataGen("MnistDataset", "TrainingFolder", 5);
            Mnist.TestDataGen("MnistDataset", "TestingFolder", 5);

            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            string pathToTrainDataFolder = config.PathToTrainDataFolder;
            string pathToTestDataFolder = config.PathToTestDataFolder;

            // generate the training data
            DataSet trainingSet = new DataSet(pathToTrainDataFolder);

            // generate the testing data
            DataSet testingSet = new DataSet(pathToTestDataFolder);

            // passing the training data to the training experiment
            InvariantExperimentImageClassification experiment = new(trainingSet, config.runParams);

            // train the network
            experiment.Train(true);


            // using predict to classify image from dataset
            Utility.CreateFolderIfNotExist("Predict");
            List<string> currentResList = new List<string>();
            /*
            CancellationToken cancelToken = new CancellationToken();
            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }
                // This can be later changed to the validation test
                var result = experiment.Predict(testingSet.PickRandom());
                Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");


                double accuracy = Utility.AccuracyCal(currentResList);
                currentResList.Add($"{result.Item1}_{result.Item2}");
                Utility.WriteOutputToFile(Path.Combine("Predict", "PredictionOutput"),result);
            }
            */


            foreach (var testImage in testingSet.Images)
            {
                var result = experiment.Predict(testImage);

                Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");

                double accuracy = Utility.AccuracyCal(currentResList);

                currentResList.Add($"{result.Item1}_{result.Item2}");

                Utility.WriteOutputToFile(Path.Combine("Predict", $"{Utility.GetHash()}_____PredictionOutput of testImage label {testImage.label}"), result);
            }
        }
    }
}