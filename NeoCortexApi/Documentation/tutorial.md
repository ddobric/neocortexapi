# Tutorial

## Encoder

Encoding real-world data into SDRs is a very important process to understand in HTM. Semantic meaning within the input data must be encoded into a binary representation. ([reference](https://numenta.org/htm-school/))

### List of encoders

- Boolean Encoder
- Category Encoder
- Datetime Encoder
- Geo-Spatial Encoder
- Multi-Encoder
- Scalar Encoder

### Examples

#### Boolean Encoder

```csharp
// Not Implemented
```

#### Category Encoder

```csharp
// Not Implemented
```

#### Datetime Encoder

```csharp
public void EncodeDateTimeTest(int w, double r, Object input, int[] expectedOutput)
{
    CortexNetworkContext ctx = new CortexNetworkContext();
    var now = DateTimeOffset.Now;
    Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();
    encoderSettings.Add("DateTimeEncoder", new Dictionary<string, object>()
        {
            { "W", 21},
            { "N", 1024},
            { "MinVal", now.AddYears(-10)},
            { "MaxVal", now},
            { "Periodic", false},
            { "Name", "DateTimeEncoder"},
            { "ClipInput", false},
            { "Padding", 5},
        });
    var encoder = new DateTimeEncoder(encoderSettings, DateTimeEncoder.Precision.Days);
    var result = encoder.Encode(DateTimeOffset.Parse(input.ToString()));
    Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(result));
    //Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(expectedOutput));
    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 32, 32);
    var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
    NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"DateTime_out_{input.ToString().Replace("/", "-").Replace(":", "-")}_32x32-N-{encoderSettings["DateTimeEncoder"]["N"]}-W-{encoderSettings["DateTimeEncoder"]["W"]}.png");
   // Assert.IsTrue(result.SequenceEqual(expectedOutput));
}
```

#### Geo-Spatial Encoder

```csharp
public int[] GermanyToItalyLongitude(double input)
{

    // CortexNetworkContext ctx = new CortexNetworkContext();

    GeoSpatialEncoderExperimental encoder = new GeoSpatialEncoderExperimental(new Dictionary<string, object>()
    {
        { "W", 89},
        { "N", 153},
        { "Radius", 1.5},
        { "Resolution", 0.016},
        { "MinVal", (double)10.0},// longitude value of Italy
        { "MaxVal", (double)78},// longitude value of germany
        { "Periodic", true},
        { "Name", "longitude"},
        { "ClipInput", true}, // it is use as if the value is less then Min and more then max , it will chlip the input as per the value and if is FAlse then it will give error
    });


    var result = encoder.Encode(input);// it use for encoding the input according to the given parameters.
                                       // printImage(encoder, nameof(GermanyToItalyLongitude));// ít is use to generate the bit map image
                                       // Debug.WriteLine(input);
                                       //Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(result));
                                       //Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(expectedResult));
    return result;

}
```

#### Multi-Encoder

```csharp
// Not Implemented
```

#### Scalar Encoder

```csharp
public void ScalarEncodingExperiment()
{
    string outFolder = nameof(ScalarEncodingExperiment);
    Directory.CreateDirectory(outFolder);
    DateTime now = DateTime.Now;
    ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
    {
        { "W", 21},
        { "N", 1024},
        { "Radius", -1.0},
        { "MinVal", 0.0},
        { "MaxVal", 100.0 },
        { "Periodic", false},
        { "Name", "scalar"},
        { "ClipInput", false},
    });
    for (double i = 0.0; i < (long)encoder.MaxVal; i += 0.1)
    {
       var result = encoder.Encode(i);
        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
        var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
        NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{i}.png", Color.Yellow, Color.Black, text:i.ToString());
    }
}
```

## Spatial Pooler

The HTM spatial pooler represents a neurally inspired algorithm for learning sparse representations from noisy data streams in an online fashion. ([reference](https://www.frontiersin.org/articles/10.3389/fncom.2017.00111/full))

```csharp
public void testCompute1()
{
    setupParameters();
    parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 9 });
    parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
    parameters.setPotentialRadius(5);

    //This is 0.3 in Python version due to use of dense
    // permanence instead of sparse (as it should be)
    parameters.setPotentialPct(0.5);

    parameters.setGlobalInhibition(false);
    parameters.setLocalAreaDensity(-1.0);
    parameters.setNumActiveColumnsPerInhArea(3);
    parameters.setStimulusThreshold(1);
    parameters.setSynPermInactiveDec(0.01);
    parameters.setSynPermActiveInc(0.1);
    parameters.setMinPctOverlapDutyCycles(0.1);
    parameters.setMinPctActiveDutyCycles(0.1);
    parameters.setDutyCyclePeriod(10);
    parameters.setMaxBoost(10);
    parameters.setSynPermTrimThreshold(0);

    //This is 0.5 in Python version due to use of dense
    // permanence instead of sparse (as it should be)
    parameters.setPotentialPct(1);

    parameters.setSynPermConnected(0.1);

    sp = new SpatialPooler();
    mem = new Connections();
    parameters.apply(mem);

    SpatialPoolerMock mock = new SpatialPoolerMock(new int[] { 0, 1, 2, 3, 4 });
    mock.Init(mem);

    int[] inputVector = new int[] { 1, 0, 1, 0, 1, 0, 0, 1, 1 };
    int[] activeArray = new int[] { 0, 0, 0, 0, 0 };
    for (int i = 0; i < 20; i++)
    {
        mock.compute(inputVector, activeArray, true);
    }

    for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
    {
        int[] permanences = ArrayUtils.ToIntArray(mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs));

        Assert.IsTrue(inputVector.SequenceEqual(permanences));
    }
}
```

## Temporal memory

Temporal memory is an algorithm which learns sequences of Sparse Distributed Representations (SDRs) formed by the Spatial Pooling algorithm, and makes predictions of what the next input SDR will be. ([reference](https://numenta.com/resources/biological-and-machine-intelligence/temporal-memory-algorithm/))

```csharp
public void TestBurstUnpredictedColumns()
{
    TemporalMemory tm = new TemporalMemory();
    Connections cn = new Connections();
    Parameters p = getDefaultParameters();
    p.apply(cn);
    tm.Init(cn);

    int[] activeColumns = { 0 };
    ISet<Cell> burstingCells = cn.GetCellSet(new int[] { 0, 1, 2, 3 });

    ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

    Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
}
```
