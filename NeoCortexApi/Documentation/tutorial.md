# Tutorial

## Encoder

Encoding real-world data into SDRs is a very important process to understand in HTM. Semantic meaning within the input data must be encoded into a binary representation. ([reference](https://numenta.org/htm-school/))

Encoders need to be first initialized with pre-defined settings using its constructor. Encoder settings are created as a `Dictionary` consists of properties and their values. Following code snippet illustrates how to create encoder settings:

```csharp
public Dictionary<string, object> GetDefaultEncoderSettings()
{
    Dictionary<string, object> encoderSettings = new Dictionary<string, object>();
    encoderSettings.Add("N", 5);
    encoderSettings.Add("W", 10);
    encoderSettings.Add("MinVal", (double)5);
    encoderSettings.Add("MaxVal", (double)10);
    encoderSettings.Add("Radius", (double)5);
    encoderSettings.Add("Resolution", (double)10);
    encoderSettings.Add("Periodic", (bool)false);
    encoderSettings.Add("ClipInput", (bool)true);
    return encoderSettings;
}
```

After loading encoder settings into the encoder, method `encoder.Encode()` is invoked to start the encoding process and return an array of '0's and '1's.

```cs
public override int[] Encode(object inputData)
{
    ...
}
```

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
public void TestCategoryEncoderWithInputArrayOfSizeFourDefaultSettings()
{
    // as the size of string array is 4 and width by default is 3 therefore the encoded bit array should be of
    // 12 bits in size
    int[] encodedBits = { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    Dictionary<string, object> encoderSettings = getDefaultSettings(); // creaing default constructor
    string[] arrayOfStrings = new string[] { "Milk", "Sugar", "Bread", "Egg" }; // array of input strings
    CategoryEncoder categoryEncoder = new CategoryEncoder(arrayOfStrings, encoderSettings); // passing the input array here
    int[] result = categoryEncoder.Encode(arrayOfStrings[0]); // encoding string "Milk"

    // validates the result
    Assert.AreEqual(encodedBits.Length, result.Length);
    CollectionAssert.AreEqual(encodedBits, result);
}

```

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/EncoderTests/CategoryEncoderExperimentalTests.cs

#### Datetime Encoder

https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/EncoderTests/DateTimeEncoderTests.cs

```csharp
public void EncodeDateTimeTest(int w, double r, Object input, int[] expectedOutput)
{
    CortexNetworkContext ctx = new CortexNetworkContext();
    DateTimeOffset now = DateTimeOffset.Now;
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
    DateTimeEncoder encoder = new DateTimeEncoder(encoderSettings, DateTimeEncoder.Precision.Days);
    int[] result = encoder.Encode(DateTimeOffset.Parse(input.ToString()));
    Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(result));
    //Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(expectedOutput));
    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 32, 32);
    int[,] twoDimArray = ArrayUtils.Transpose(twoDimenArray);
    NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"DateTime_out_{input.ToString().Replace("/", "-").Replace(":", "-")}_32x32-N-{encoderSettings["DateTimeEncoder"]["N"]}-W-{encoderSettings["DateTimeEncoder"]["W"]}.png");
   // Assert.IsTrue(result.SequenceEqual(expectedOutput));
}
```

The following table visualizes the result from several `input` of the above unit test using `NeoCortexUtils.DrawBitmap()`:  
`input` | `"05/07/2011 21:58:07"` | `"06/07/2012 21:58:07"` | `"07/07/2013 21:58:07"` | `"08/07/2014 21:58:07"` |
--|--|--|--|--|
Results|![][img05.07.2011] | ![][img06.07.2012] | ![][img07.07.2013] | ![][img08.07.2014] |

[img05.07.2011]: ./images/DateTimeEncoder/DateTime_out_05-07-2011%2021-58-07_32x32-N-1024-W-21.png
[img06.07.2012]: ./images/DateTimeEncoder/DateTime_out_06-07-2012%2021-58-07_32x32-N-1024-W-21.png
[img07.07.2013]: ./images/DateTimeEncoder/DateTime_out_07-07-2013%2021-58-07_32x32-N-1024-W-21.png
[img08.07.2014]: ./images/DateTimeEncoder/DateTime_out_08-07-2014%2021-58-07_32x32-N-1024-W-21.png

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


    int[] result = encoder.Encode(input);// it use for encoding the input according to the given parameters.
                                       // printImage(encoder, nameof(GermanyToItalyLongitude));// ít is use to generate the bit map image
                                       // Debug.WriteLine(input);
                                       //Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(result));
                                       //Debug.WriteLine(NeoCortexApi.Helpers.StringifyVector(expectedResult));
    return result;

}
```

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/EncoderTests/GeoSpatialEncoderExperimentalTests.cs

#### Multi-Encoder

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/EncoderTests/MultiEncoderTests.cs

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
        int[] result = encoder.Encode(i);
        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
        int[,] twoDimArray = ArrayUtils.Transpose(twoDimenArray);
        NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{i}.png", Color.Yellow, Color.Black, text:i.ToString());
    }
}
```

The following table visualizes the result from several numbers of the above unit test using `NeoCortexUtils.DrawBitmap()`:
|`number`|`0.1`|`0.3`|`17.6`|`18.0`|
|--|--|--|--|--|
|Results|![][img0.1]|![][img0.3]|![][img17.6]|![][img18.0]|

[img0.1]: ./images/ScalarEncoder/0.1.png
[img0.3]: ./images/ScalarEncoder/0.3.png
[img17.6]: ./images/ScalarEncoder/17.6.png
[img18.0]: ./images/ScalarEncoder/18.0.png

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/EncoderTests/ScalarEncoderTests.cs

## Spatial Pooler

The HTM spatial pooler represents a neurally inspired algorithm for learning sparse representations from noisy data streams in an online fashion. ([reference](https://www.frontiersin.org/articles/10.3389/fncom.2017.00111/full))

Right now, three versions of SP are implemented and considered:

- Spatial Pooler single threaded original version without algorithm specific changes.
- SP-MT multithreaded version, which supports multiple cores on a single machine and
- SP-Parallel, which supports multicore and multimode calculus of spatial pooler.

Spatial Pooler algorithm requires 2 steps.

1. Parameters configuration

   There are 2 ways to configure Spatial Pooler's parameters.

   1.1. Using `HtmConfig` (**Preferred** way to intialize `SpatialPooler` )

   ```csharp
   public void SpatialPoolerInit()
   {
       HtmConfig htmConfig = new HtmConfig()
       {
           InputDimensions = new int[] { 32, 32 },
           ColumnDimensions = new int[] { 64, 64 },
           PotentialRadius = 16,
           PotentialPct = 0.5,
           GlobalInhibition = false,
           LocalAreaDensity = -1.0,
           NumActiveColumnsPerInhArea = 10.0,
           StimulusThreshold = 0.0,
           SynPermInactiveDec = 0.008,
           SynPermActiveInc = 0.05,
           SynPermConnected = 0.10,
           MinPctOverlapDutyCycles = 0.001,
           MinPctActiveDutyCycles = 0.001,
           DutyCyclePeriod = 1000,
           MaxBoost = 10.0,
           RandomGenSeed = 42,
           Random = new ThreadSafeRandom(42)
       };

       Connections connections = new Connections(htmConfig);

       SpatialPooler spatialPooler = new SpatialPoolerMT();
       spatialPooler.Init(connections);
   }
   ```

   1.2. Using `Parameters` (**Obsoleted** - this is for supporting the compatibility with the Java implementation)

   ```csharp
   public void setupParameters()
   {
       Parameters parameters = Parameters.getAllDefaultParameters();
       parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 5 });
       parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
       parameters.Set(KEY.POTENTIAL_RADIUS, 5);
       parameters.Set(KEY.POTENTIAL_PCT, 0.5);
       parameters.Set(KEY.GLOBAL_INHIBITION, false);
       parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
       parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
       parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
       parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
       parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
       parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
       parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
       parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
       parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
       parameters.Set(KEY.MAX_BOOST, 10.0);
       parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));

       Connnections mem = new Connections();
       parameters.apply(mem);

       SpatialPooler sp = new SpatialPooler();
       sp.Init(mem);
   }

   ```

2. Invocation of `Compute()`

   ```csharp
   public void TestSpatialPoolerCompute()
   {
       // parameters configuration
       ...

       // Invoke Compute()
       int[] outputArray = sp.Compute(inputArray, learn: true);
   }
   ```

3. How SP learn
 Following is an example illustrates how to use `SpatialPooler` algorithm.

```csharp
public void testCompute1_1()
{
    HtmConfig htmConfig = SetupHtmConfigParameters();
    htmConfig.InputDimensions = new int[] { 9 };
    htmConfig.ColumnDimensions = new int[] { 5 };
    htmConfig.PotentialRadius = 5;

    // This is 0.3 in Python version due to use of dense
    // permanence instead of sparse (as it should be)
    htmConfig.PotentialPct = 0.5;

    htmConfig.GlobalInhibition = false;
    htmConfig.LocalAreaDensity = -1.0;
    htmConfig.NumActiveColumnsPerInhArea = 3;
    htmConfig.StimulusThreshold = 1;
    htmConfig.SynPermInactiveDec = 0.01;
    htmConfig.SynPermActiveInc = 0.1;
    htmConfig.MinPctOverlapDutyCycles = 0.1;
    htmConfig.MinPctActiveDutyCycles = 0.1;
    htmConfig.DutyCyclePeriod = 10;
    htmConfig.MaxBoost = 10;
    htmConfig.SynPermTrimThreshold = 0;

    // This is 0.5 in Python version due to use of dense
    // permanence instead of sparse (as it should be)
    htmConfig.PotentialPct = 1;

    htmConfig.SynPermConnected = 0.1;

    mem = new Connections(htmConfig);

    SpatialPoolerMock mock = new SpatialPoolerMock(new int[] { 0, 1, 2, 3, 4 });
    mock.Init(mem);

    int[] inputVector = new int[] { 1, 0, 1, 0, 1, 0, 0, 1, 1 }; // output of encoder
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

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/SpatialPoolerTests.cs

## Temporal memory

Temporal memory is an algorithm which learns sequences of Sparse Distributed Representations (SDRs) formed by the Spatial Pooling algorithm, and makes predictions of what the next input SDR will be. ([reference](https://numenta.com/resources/biological-and-machine-intelligence/temporal-memory-algorithm/))

Temporal memory alogirithm's input is the active columns output of Spatial Pooler algorithm.

```cs
public void TemporalMemoryInit()
{
    HtmConfig htmConfig = Connections.GetHtmConfigDefaultParameters();
    Connections connections = new Connections(htmConfig);

    TemporalMemory temporalMemory = new TemporalMemory();

    temporalMemory.Init(connections);
}
```

To apply the algorithm, method `Compute()` is invoked. The result will then be stored as `ComputeCycle` object.

```cs
public ComputeCycle Compute(int[] activeColumns, bool learn)
{
    ...
}
```

Following is an example illustrates how to use `TemporalMemory` algorithm.

```cs
public void TestBurstUnpredictedColumns1()
{
    HtmConfig htmConfig = GetDefaultTMParameters();
    Connections cn = new Connections(htmConfig);

    TemporalMemory tm = new TemporalMemory();

    tm.Init(cn);

    int[] activeColumns = { 0 };
    ISet<Cell> burstingCells = cn.GetCellSet(new int[] { 0, 1, 2, 3 });

    ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

    Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
}
```

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/TemporalMemoryTests.cs
