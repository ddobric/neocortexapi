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

CancellationToken cancelToken = new CancellationToken();
while(true){
    if (cancelToken.IsCancellationRequested)
    {
        return;
    }
    // This can be later changed to the validation test
    var result = experiment.Predict(invariantSet.PickRandom());
    Debug.WriteLine(result);
    Task.Delay(1000);
}