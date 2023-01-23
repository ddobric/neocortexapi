using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApiPersistenceSample;
using System.Diagnostics;

Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(SpatialPatternLearning)}");

bool overwrite = true;

        //
        // Prepare input values for the HTM system by randomly choose from the sequence of integer.
        List<double> values = new List<double>();
        int max = 20;
        for (int i = 0; i < max; i++)
        {
            values.Add(i);
        }

        int seed = 42;
        var random = new Random(seed);

        List<double> inputValues = new List<double>();
        for (int i = 0; i < max * 0.5; i++)
        {
            var index = random.Next(0, max);
            while (inputValues.Contains(values[index]))
            {
                index = random.Next(0, max);
            }

            inputValues.Add(values[index]);
        }

        //
        // Split the input values into two sequences and train separately.
        List<double> testValues = values.Except(inputValues).ToList();

//
// Train the first set.
var model1Name = "Model1.txt";
var model1Trace = "Model1trace.txt";
CortexLayer<object, object> model1;
if (HtmSerializer.TryLoad<CortexLayer<object, object>>(model1Name, out model1) == false || overwrite)
{
    Console.WriteLine($"Training the first set of values: {string.Join(',', inputValues)}");
    var experiment = new SpatialPatternLearning();
    model1 = experiment.Initialize(max, inputValues);
    model1.Train(inputValues, 1000, "sp");
    Console.WriteLine("Saving the model...");
    // persist the state of the model.
    HtmSerializer.Save(model1Name, model1);
}
else
{
    Console.WriteLine("The model already exists. Loading the model...");
}
var sp1 = (SpatialPooler)model1.HtmModules["sp"];

// Trace the permanence value of every column.
sp1.TraceColumnPermenances(model1Trace);

//
// Recreate the model from the persisted state and train it with the second set.
var model2Name = "Model2.txt";
var model2Trace = "Model2trace.txt";
CortexLayer<object, object> model2;
if (HtmSerializer.TryLoad<CortexLayer<object, object>>(model2Name, out model2) == false || overwrite)
{
    Console.WriteLine($"Training the second set of values: {string.Join(',', testValues)}");
    model2 = HtmSerializer.Load<CortexLayer<object, object>>(model1Name);
    model2.Train(testValues, 1000, "sp");
    Console.WriteLine("Saving the model...");

    HtmSerializer.Save(model2Name, model2);
}
else
{
    Console.WriteLine("The model already exists. Loading the model...");
}
var sp2 = (SpatialPooler)model2.HtmModules["sp"];

// Trace the permanence value of every column.
sp2.TraceColumnPermenances(model2Trace);

//foreach (var input in testValues)
//{
//    var encoder = (ScalarEncoder)model1.HtmModules["encoder"];

//    var sdr = encoder.Encode(input);

//    var inputHash = HomeostaticPlasticityController.GetHash(sdr);
//    var res1 = model1.Compute(input, false);
//    var res2 = model2.Compute(input, false);

//}
