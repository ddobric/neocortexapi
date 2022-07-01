using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using InvariantLearning;
using Invariant.Entities;

namespace InvariantLearning
{
    public class InvariantLearning
    {
        public static void Main()
        {
            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            string pathToTrainDataFolder = config.PathToTrainDataFolder;
            string pathToTestDataFolder = config.PathToTestDataFolder;

            // generate the training data
            DataSet trainingSet = new DataSet(pathToTrainDataFolder);

            // generate the testing data
            DataSet testingSet = new DataSet(pathToTestDataFolder);

            // passing the training data to the training experiment
            InvariantExperiment experiment = new(trainingSet, config.runParams);

            // train the network
            experiment.Train();


            // using predict to classify image from dataset
            Utility.CreateFolderIfNotExist("Predict");
            List<string> currentResList = new List<string>();
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
                //Utility.WriteToFile(Path.Combine("Predict", "PredictionOutput.csv"),result);
            }
        }
    }
}