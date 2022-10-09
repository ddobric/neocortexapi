// See https://aka.ms/new-console-template for more information
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApiPersistenceSample;

List<double> values = new List<double>();
int max = 20;
for (int i = 0; i < (int)max; i++)
{
    values.Add((double)i);
}

int seed = 42;
var random = new Random(seed);

List<double> inputValues = new List<double>();
for (int i = 0; i < max * 0.5; i++)
{
    inputValues.Add(values[i]);
}

List<double> testValues = values.Except(inputValues).ToList();

var model1Name = "Model1.txt";
CortexLayer<object, object> model1;
if (HtmSerializer2.TryLoad<CortexLayer<object, object>>(model1Name, out model1) == false)
{
    var experiment = new SpatialPatternLearning();
    model1 = experiment.Train(max, inputValues);
    HtmSerializer2.Save(model1Name, model1);
}
var model2Name = "Model2.txt";
CortexLayer<object, object> model2;
if (HtmSerializer2.TryLoad<CortexLayer<object, object>>(model2Name, out model2) == false)
{

    model2 = HtmSerializer2.Load<CortexLayer<object, object>>(model1Name);
    model2.Train(testValues, 1000, "sp");

    HtmSerializer2.Save(model2Name, model2); 
}

foreach (var input in testValues)
{
    var encoder = (ScalarEncoder)model1.HtmModules["encoder"];

    var sdr = encoder.Encode(input);

    var inputHash = HomeostaticPlasticityController.GetHash(sdr);
    var res1 = model1.Compute(input, false);
    var res2 = model2.Compute(input, false);
    
}
