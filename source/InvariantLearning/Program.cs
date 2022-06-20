using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using InvariantLearning;

// reading Config from json
var config = Utility.ReadConfig("experimentParams.json");
string PathToTrainingFolder = config.PathToTrainingFolder;


// generate the training data
TrainingData invariantSet = new TrainingData(PathToTrainingFolder);


// passing the training data to the training experiment
InvariantExperiment experiment  = new(invariantSet, config.runParams);


// train the network
experiment.Train();


// using predict to classify image from dataset
Utility.CreateFolderIfNotExist("Predict");
List<string> currentResList = new List<string>();
CancellationToken cancelToken = new CancellationToken();
while(true){
    if (cancelToken.IsCancellationRequested)
    {
        return;
    }
    // This can be later changed to the validation test
    var result = experiment.Predict(invariantSet.PickRandom());
    Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");

    
    double accuracy = Utility.AccuracyCal(currentResList);
    currentResList.Add($"{result.Item1}_{result.Item2}");
    //Utility.WriteToFile(Path.Combine("Predict", "PredictionOutput.csv"),result);
}