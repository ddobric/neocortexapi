# ML22/23-15   Approve Prediction of Multisequence Learning 

## Introduction

In this project, we have tried to implement new methods along `MultisequenceLearning` alogrithm. The new methods are automatically reading the dataset from the given path in `HelperMethods.ReadDataset(datasetPath)`, we also have test data in other file which needs to be read for later testing the subsequences in similar form as `HelperMethods.ReadDataset(testsetPath)`. `RunMultiSequenceLearningExperiment(sequences, sequencesTest)` takes the multiple sequences in `sequences` and test subsequences in `sequencesTest` and is passed to `RunMultiSequenceLearningExperiment(sequences, sequencesTest)`. After learning is completed, calculation of accuracy of predicted element.

## Implementation

![image](./images/overview.png)

Fig: Architecture of Approve Prediction of Multisequence Learning

Above the flow of implementation of our project.

`Sequence` is the model of how we process and store the dataset. And can be seen below:

```csharp
public class Sequence
{
    public String name { get; set; }
    public int[] data { get; set; }
}
```

eg:
- Dataset

```json
[
  {
    "name": "S1",
    "data": [ 0, 2, 5, 6, 7, 8, 10, 11, 13 ]
  },
  {
    "name": "S2",
    "data": [ 1, 2, 3, 4, 6, 11, 12, 13, 14 ]
  },
  {
    "name": "S3",
    "data": [ 1, 2, 3, 4, 7, 8, 10, 12, 14 ]
  }
]
```

- Test Dataset

```json
[
  {
    "name": "T1",
    "data": [ 1, 2, 4 ]
  },
  {
    "name": "T2",
    "data": [ 2, 3, 4 ]
  },
  {
    "name": "T3",
    "data": [ 4, 5, 7 ]
  },
  {
    "name": "T4",
    "data": [ 5, 8, 9 ]
  }
]

```

Our implemented methods are in `HelperMethod.cs` and can be found [here](../HelperMethods.cs):

1. FetchHTMConfig()

Here we save the HTMConfig which is used for Hierarchical Temporal Memory to `Connections`

```csharp
/// <summary>
/// HTM Config for creating Connections
/// </summary>
/// <param name="inputBits">input bits</param>
/// <param name="numColumns">number of columns</param>
/// <returns>Object of HTMConfig</returns>
public static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
{
    HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
    {
        Random = new ThreadSafeRandom(42),

        CellsPerColumn = 25,
        GlobalInhibition = true,
        LocalAreaDensity = -1,
        NumActiveColumnsPerInhArea = 0.02 * numColumns,
        PotentialRadius = (int)(0.15 * inputBits),
        MaxBoost = 10.0,
        DutyCyclePeriod = 25,
        MinPctOverlapDutyCycles = 0.75,
        MaxSynapsesPerSegment = (int)(0.02 * numColumns),
        ActivationThreshold = 15,
        ConnectedPermanence = 0.5,e.
        PermanenceDecrement = 0.25,
        PermanenceIncrement = 0.15,
        PredictedSegmentDecrement = 0.1,
    };

    return cfg;
}
```

All the fields are self explanotary as per HTM theory.

2. getEncoder()

We have used `ScalarEncoder` since we are encoding all numeric value only.

Remeber that `inputBits` is same as `HTMConfig`.

```csharp
/// <summary>
/// Get the encoder with settings
/// </summary>
/// <param name="inputBits">input bits</param>
/// <returns>Object of EncoderBase</returns>
public static EncoderBase GetEncoder(int inputBits)
{
        double max = 20;

        Dictionary<string, object> settings = new Dictionary<string, object>()
        {
        { "W", 15},
        { "N", inputBits},
        { "Radius", -1.0},
        { "MinVal", 0.0},
        { "Periodic", false},
        { "Name", "scalar"},
        { "ClipInput", false},
        { "MaxVal", max}
        };

        EncoderBase encoder = new ScalarEncoder(settings);

        return encoder;
}
```

Note that `MaxValue` for encoder is set to `20` which can be change but then this value should be matched while creating synthetic dataset.

3. ReadDataset()

Reads the JSON file wehen passed as full path and retuns the object of list of `Sequence`

```csharp
/// <summary>
/// Reads dataset from the file
/// </summary>
/// <param name="path">full path of the file</param>
/// <returns>Object of list of Sequence</returns>
public static List<Sequence> ReadDataset(string path)
{
        Console.WriteLine("Reading Sequence...");
        String lines = File.ReadAllText(path);
        //var sequence = JsonConvert.DeserializeObject(lines);
        List<Sequence> sequence = System.Text.Json.JsonSerializer.Deserialize<List<Sequence>>(lines);

        return sequence;
}
```

4. CreateDataset()

We made an enhancement to create dataset automatically so we do not have to manually spend time. Here we create dataset with paremeters such as `numberOfSequence` to be created, `size` of a sequence, `startVal` possibly start range, and `endVal` possibly start range of sequence.

```csharp
/// <summary>
/// Creates list of Sequence as per configuration
/// </summary>
/// <returns>Object of list of Sequence</returns>
public static List<Sequence> CreateDataset()
{
        int numberOfSequence = 3;
        int size = 12;
        int startVal = 0;
        int endVal = 15;
        Console.WriteLine("Creating Sequence...");
        List<Sequence> sequence = HelperMethods.CreateSequences(numberOfSequence, size, startVal, endVal);

        return sequence;
}
```

Note that `endVal` should be less than equal to `MaxVal` of `ScalarEncoder` used above

5. SaveDataset()

Saves the dataset in `dataset` director of the `BasePath` of the application where it is running.

```csharp
/// <summary>
/// Saves the dataset in 'dataset' folder in BasePath of application
/// </summary>
/// <param name="sequences">Object of list of Sequence</param>
/// <returns>Full path of the dataset</returns>
public static string SaveDataset(List<Sequence> sequences)
{
        string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        string reportFolder = Path.Combine(BasePath, "dataset");
        if (!Directory.Exists(reportFolder))
        Directory.CreateDirectory(reportFolder);
        string reportPath = Path.Combine(reportFolder, $"dataset_{DateTime.Now.Ticks}.json");

        Console.WriteLine("Saving dataset...");

        if (!File.Exists(reportPath))
        {
        using (StreamWriter sw = File.CreateText(reportPath))
        {
          sw.WriteLine(JsonConvert.SerializeObject(sequences));
        }
        }

        return reportPath;
}
```

6. Calculating accuracy in PredictNextElement() in `Program.cs`

![image](./images/approve_prediction.png)

Fig: Predictiona and calculating accuracy

```csharp
int matchCount = 0;
int predictions = 0;
double accuracy = 0.0;

foreach (var item in list)
{
    Predict();
    //compare current element with prediction of previous element
    if(item == Int32.Parse(prediction.Last()))
    {
        matchCount++;
    }
    predictions++;
    accuracy = (double)matchCount / predictions * 100;
}
```

Note that prediction code is omitted.

## How to run the project

### To create synthetic dataset

1. Open the [sln](../../../NeoCortexApi.sln) and select `MultiSequenceLearning` as startup project.

2. In `Program.cs` we have the `Main()`. Uncomment below code to create a synthetic dataset.

```csharp
//to create synthetic dataset
string path = HelperMethods.SaveDataset(HelperMethods.CreateDataset());
Console.WriteLine($"Dataset saved: {path}");
```

*and comment rest of the lines*.

3. Run to create dataset and save the path of dataset folder and name.

![dataset](./images/dataset.jpg)

### To run the experiment

1. Open the [NeoCortexApi.sln](../../../NeoCortexApi.sln) and select `MultiSequenceLearning` as startup project.

2. In `Program.cs` we have the `Main()`. Change the name of `dataset` file saved from previous run  as seen below:

```csharp
//to read dataset
string BasePath = AppDomain.CurrentDomain.BaseDirectory;
string datasetPath = Path.Combine(BasePath, "dataset", "dataset_03.json"); //edit name of dataset here
Console.WriteLine($"Reading Dataset: {datasetPath}");
List<Sequence> sequences = HelperMethods.ReadDataset(datasetPath);
```

and also *copy the [test data](../dataset/test_01.json) to the folder* (`{BASEPATH}\neocortexapi\source\MySEProject\MultiSequenceLearning\bin\Debug\net6.0\dataset`).

## Results

We have run the experiment max possible number of times with different dataset. We have tried to keep the size of dataset small and number of sequences also small due to large time in execution.

![results](./images/result.png)

## Reference

- Forked from [ddobric/neocortexapi](https://github.com/ddobric/neocortexapi)

- [Numenta Research Publication](https://www.numenta.com/resources/research-publications/) 

- [Machine Learning Guide to HTM](https://www.numenta.com/blog/2019/10/24/machine-learning-guide-to-htm/)