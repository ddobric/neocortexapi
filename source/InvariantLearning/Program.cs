using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;

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
string PathToFeatureFolder = config.PathToFeatureFolder;

// Generate the training data
var invariantSet = new TrainingData(PathToTrainingFolder);
var featureSet = new TrainingData(PathToFeatureFolder);

// Passing the training data to the training experiment
InvariantExp exp  = new(invariantSet, config.experimentParams);
Debug.WriteLine(invariantSet.Count);

// train the network
exp.Train();

// using the predictor to classify image from dataset
while(true){
    var result = exp.Predict(invariantSet.PickRandom());
    Console.ReadLine();
}