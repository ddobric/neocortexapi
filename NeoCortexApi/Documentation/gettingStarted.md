# Getting started

### Example for intialize Encoder

```csharp
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
    //for (int i = 1; i < 367; i++)
    //{
    var result = encoder.Encode(input);
    Debug.WriteLine(input);
    Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(result));
    Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(expectedResult));
    //}
    Assert.IsTrue(expectedResult.SequenceEqual(result));
}
```

### Example of initialize Spatial Pooler

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
