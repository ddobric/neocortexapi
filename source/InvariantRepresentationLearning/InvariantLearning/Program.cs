using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using InvariantLearning;
using Invariant.Entities;
using MnistDataGen;
using System.Collections.Concurrent;

namespace InvariantLearning
{
    public class InvariantLearning
    {
        public static void Main()
        {
            //ExperimentPredictingWithFrameGrid();
            //ExperimentNormalImageClassification();
            ExperimentEvaluatateImageClassification();
        }

        private static void ExperimentEvaluatateImageClassification()
        {
            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            Utility.CreateFolderIfNotExist(config.ExperimentFolder);
            string pathToTrainDataFolder = config.PathToTrainDataFolder;
            string pathToTestDataFolder = config.PathToTestDataFolder;
            
            Mnist.DataGen("MnistDataset", Path.Combine(config.ExperimentFolder, pathToTrainDataFolder), 100);

            Utility.CreateFolderIfNotExist(Path.Combine(config.ExperimentFolder, pathToTrainDataFolder));
            DataSet trainingData = new DataSet(Path.Combine(config.ExperimentFolder,pathToTrainDataFolder));

            Utility.CreateFolderIfNotExist(Path.Combine(config.ExperimentFolder, pathToTestDataFolder));
            DataSet testingData = trainingData.GetTestData(10);
            testingData.VisualizeSet(Path.Combine(config.ExperimentFolder, pathToTestDataFolder));

            LearningUnit sp = new(trainingData.PickRandom().imageWidth, 1024);

            sp.TrainingNewbornCycle(trainingData);

            sp.TrainingNormal(trainingData, config.runParams.Epoch);

            var allResult = new List<Dictionary<string, string>>();

            foreach(var testingImage in testingData.Images)
            {
                Utility.CreateFolderIfNotExist("TestResult");
                var res = sp.PredictScaledImage(testingImage, Path.Combine(config.ExperimentFolder, "TestResult"));
                res.Add("fileName", $"{testingImage.label}_{Path.GetFileName(testingImage.imagePath)}");
                res.Add("CorrectLabel", testingImage.label);
                allResult.Add(res);
            }
            Utility.WriteListToCsv(Path.Combine(config.ExperimentFolder, "TestResult", "testOutput"), allResult);
            Utility.WriteListToOutputFile(Path.Combine(config.ExperimentFolder, "TestResult", "testOutput"), allResult);


        }

        private static void ExperimentNormalImageClassification()
        {
            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            string pathToTrainDataFolder = config.PathToTrainDataFolder;

            //Mnist.DataGenAll("MnistDataset", "TrainingFolder");
            Mnist.DataGen("MnistDataset", "TrainingFolder",10);

            List<DataSet> testingData = new List<DataSet>();
            List<DataSet> trainingData = new List<DataSet>();

            DataSet originalTrainingDataSet = new DataSet(pathToTrainDataFolder);

            int k = 5;

            (trainingData, testingData) = originalTrainingDataSet.KFoldDataSetSplitEvenly(k);

            ConcurrentDictionary<string, double> foldValidationResult = new ConcurrentDictionary<string, double>();

            Parallel.For(0, k, (i) =>
            //for (int i = 0; i < k; i += 1)
            {
                // Visualizing data in k-fold scenarios
                string setPath = $"DatasetFold{i}";
                string trainSetPath = Path.Combine(setPath, "TrainingData");
                Utility.CreateFolderIfNotExist(trainSetPath);
                trainingData[i].VisualizeSet(trainSetPath);

                string testSetPath = Path.Combine(setPath, "TestingData");
                Utility.CreateFolderIfNotExist(trainSetPath);
                testingData[i].VisualizeSet(testSetPath);

                // passing the training data to the training experiment
                InvariantExperimentImageClassification experiment = new(trainingData[i], config.runParams);

                // train the network
                experiment.Train(false);

                // Prediction phase
                Utility.CreateFolderIfNotExist($"Predict_{i}");

                List<string> currentResList = new List<string>();

                Dictionary<string, List<Dictionary<string, string>>> allResult = new Dictionary<string, List<Dictionary<string, string>>>();

                foreach (var testImage in testingData[i].Images)
                {
                   var result = experiment.Predict(testImage, i.ToString());

                    string testImageID = $"{testImage.label}_{Path.GetFileNameWithoutExtension(testImage.imagePath)}";
                    UpdateResult(ref allResult, testImageID, result);
                }
                double foldValidationAccuracy = CalculateAccuracy(allResult);

                foreach(var sp in allResult)
                {
                    string path = Path.Combine($"Predict_{i}", sp.Key);
                    Utility.WriteListToCsv(path, allResult[sp.Key]);
                }

                foldValidationResult.TryAdd($"Fold_{i}_accuracy", foldValidationAccuracy);
            });
            Utility.WriteResultOfOneSP(new Dictionary<string, double>(foldValidationResult), $"KFold_{k}_Validation_Result");
        }

        /// <summary>
        /// Calculate by averaging similarity prediction of the correct label
        /// </summary>
        /// <param name="allResult"></param>
        /// <returns></returns>
        private static double CalculateAccuracy(Dictionary<string, List<Dictionary<string, string>>> allResult)
        {
            List< double> spAccuracy = new List<double>();

            foreach (var spResult in allResult.Values)
            {  
                List<double> similarityList = new List<double>();
                foreach(var imagePredictResult in spResult)
                {
                    if (imagePredictResult.ContainsKey(imagePredictResult["CorrectLabel"]))
                    {
                        similarityList.Add(Double.Parse(imagePredictResult[imagePredictResult["CorrectLabel"]]));
                    }
                    else
                    {
                        similarityList.Add(0.0);
                    }
                }
                spAccuracy.Add(similarityList.Average());
            }
            return spAccuracy.Average();
        }

        private static void UpdateResult(ref Dictionary<string, List<Dictionary<string, string>>> allResult, string testImageID, Dictionary<string, Dictionary<string, string>> result)
        {
            foreach(var spKey in result.Keys)
            {
                if (!allResult.ContainsKey(spKey))
                {
                    allResult.Add(spKey, new List<Dictionary<string, string>>());
                }
            }

            foreach(var spKey in allResult.Keys)
            {
                Dictionary<string, string> resultEntryOfOneSP = new Dictionary<string, string>();
                resultEntryOfOneSP.Add("fileName", testImageID);
                foreach(var labelPred in result[spKey])
                {
                    resultEntryOfOneSP.Add(labelPred.Key, labelPred.Value);
                }
                allResult[spKey].Add(resultEntryOfOneSP);
            }
        }

        /*
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



   foreach (var testImage in testingSet.Images)
   {
       var result = experiment.Predict(testImage);

       Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");

       double accuracy = Utility.AccuracyCal(currentResList);

       currentResList.Add($"{result.Item1}_{result.Item2}");

       Utility.WriteOutputToFile(Path.Combine("Predict", $"{Utility.GetHash()}_____PredictionOutput of testImage label {testImage.label}"), result);
   }

}
*/
    }
}