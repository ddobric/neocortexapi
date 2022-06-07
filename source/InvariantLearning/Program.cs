using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using InvariantLearning;
#region Testing Code
List<Connections> conns = new();
List<HtmConfig> htmConfigs = new List<HtmConfig>();
List<SpatialPoolerMT> sps = new();

for(int i = 0; i < 2; i += 1)
{
    htmConfigs.Add(new HtmConfig());
    conns.Add(new Connections(htmConfigs.Last()));
}


HomeostaticPlasticityController hpa_sp_L4 = new HomeostaticPlasticityController(conns[0], 100 * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
    {
        if (isStable)
            Debug.WriteLine($"SP L4 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
        else
            Debug.WriteLine($"SP L4 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

    }, numOfCyclesToWaitOnChange: 50);
#endregion

// Reading Config from json
var config = Utility.ReadConfig("experimentParams.json");
string PathToTrainingFolder = config.PathToTrainingFolder;


// Generate the training data
TrainingData invariantSet = new TrainingData(PathToTrainingFolder);


// Passing the training data to the training experiment
InvariantExperiment experiment  = new(invariantSet, config.runParams);

// train the network
experiment.Train();

// using the predictor to classify image from dataset
CancellationToken cancelToken = new CancellationToken();
while(true){
    if (cancelToken.IsCancellationRequested)
    {
        return;
    }
    var result = experiment.Predict(invariantSet.PickRandom());
    Debug.WriteLine(result);
    Task.Delay(1000);
}