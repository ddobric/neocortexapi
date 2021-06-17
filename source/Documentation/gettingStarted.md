# Getting started

### HTM Encoders

An encoder is the component that converts some value to a sparse distributed representation (SDR). Depending on the encoder, the value can be anything (i.E.: scaler value, time, day in a week, geo-location etc.). The SDR is typically an array in '1' and '0'. Currentlly , the SDR is defined in C# as `int[]`.

Following example demonstrates how to encode scalar values from 0-367.

```csharp
[TestMethod]
[DataRow(1, new int[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 })]
[DataRow(2, new int[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 })]
[DataRow(35, new int[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
[DataRow(61, new int[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
[DataRow(62, new int[] { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 })]
public void SeasonEncoderTest(int input, int[] expectedResult)
{
    CortexNetworkContext ctx = new CortexNetworkContext();
    ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
        {
            { "W", 3},
            { "N", 12},
            { "Radius", -1.0},
            { "MinVal", 1.0},
            { "MaxVal", 366.0},
            { "Periodic", true},
            { "Name", "season"},
            { "ClipInput", true},
        });

    for (int i = 1; i < 367; i++)
    {
        var result = encoder.Encode(input);
        Debug.WriteLine(input);
        Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(result));
        Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(expectedResult));
    }

    Assert.IsTrue(expectedResult.SequenceEqual(result));
}
```

### Working with the Spatial Pooler

The Spatial Pooler is an algorithm inside of HTM that is capable of learning of spatial patterns. The Spatial Pooler typically gets array of bits as an input and converts it into the SDR. Input of the Spatial Pooler is usually the array converted by some encoder or bits of an image.

See also: https://numenta.com/neuroscience-research/research-publications/papers/htm-spatial-pooler-neocortical-algorithm-for-online-sparse-distributed-coding/

```csharp
/// <summary>
/// Corresponds to git\nupic\examples\sp\sp_tutorial.py
/// </summary>
[TestMethod]
public void SPTutorialTest()
{
    var parameters = GetDefaultParams();

    parameters.setInputDimensions(new int[] { 1000 });
    parameters.setColumnDimensions(new int[] { 2048 });
    parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
    parameters.setGlobalInhibition(false);

    var sp = new SpatialPooler();

    var mem = new Connections();
    parameters.apply(mem);
    sp.Init(mem);

    int[] activeArray = new int[2048];

    int[] inputVector = Helpers.GetRandomVector(1000, parameters.Get<Random>(KEY.RANDOM));

    sp.compute(inputVector, activeArray, true);

    var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

    var str = Helpers.StringifyVector(activeCols);

    Debug.WriteLine(str);

}

```

### Example for initialize Temporal Memory

```csharp
// missing code
```
