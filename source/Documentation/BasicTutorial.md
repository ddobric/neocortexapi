# Samples

## Sequence Learning

1. Setup HtmConfig with the following code:
```csharp
int inputBits = 100;
int numColumns = 1024;

HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
{
    Random = new ThreadSafeRandom(42),

    CellsPerColumn = 25,
    GlobalInhibition = true,
    LocalAreaDensity = -1,
    NumActiveColumnsPerInhArea = 0.02 * numColumns,
    PotentialRadius = 50,
    InhibitionRadius = 15,

    MaxBoost = 10.0,
    DutyCyclePeriod = 25,
    MinPctOverlapDutyCycles = 0.75,
    MaxSynapsesPerSegment = (int)(0.02 * numColumns),

    ActivationThreshold = 15,
    ConnectedPermanence = 0.5,

    // Learning is slower than forgetting in this case.
    PermanenceDecrement = 0.25,
    PermanenceIncrement = 0.15,

    // Used by punishing of segments.
    PredictedSegmentDecrement = 0.1
};
```

2. Setup encoder settings with the following code:
```csharp
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
```

3. Providing sequence input:
```c#
List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 });
```



Check out full implementation [here](../Samples/NeoCortexApiSample/SequenceLearning.cs).
