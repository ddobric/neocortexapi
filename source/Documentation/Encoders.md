# Encoder

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

### Parameter definition

| Parameter    | Data type | Definition                                                                                                                                                                                                                                |
| ------------ | --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `N`          | Integer   | The number of bits in the output. Must be greater than or equal to `W`                                                                                                                                                                    |
| `W`          | Integer   | The number of bits that are set to encode a single value - the “width” of the output signal restriction: w must be odd to avoid centering problems.                                                                                       |
| `MinVal`     | double    | The minimum value of the input signal.                                                                                                                                                                                                    |
| `MaxVal`     | double    | The upper bound of the input signal. (input is strictly less if `Periodic == True`)                                                                                                                                                       |
| `Radius`     | double    | Two inputs separated by more than the radius have non-overlapping representations. Two inputs separated by less than the radius will in general overlap in at least some of their bits. You can think of this as the radius of the input. |
| `Resolution` | double    | Two inputs separated by greater than, or equal to the resolution are guaranteed to have different representations.                                                                                                                        |
| `Periodic`   | boolean   | If true, then the input value “wraps around” such that `Minval` = `Maxval`. For a periodic value, the input must be strictly less than `Maxval`, otherwise `Maxval` is a true upper bound.                                                |
| `ClipInput`  | boolean   | if true, non-periodic inputs smaller than minval or greater than maxval will be clipped to minval/maxval                                                                                                                                  |

The following section demonstrate how to use an encoder.

To initialize an encoder, use the code snippet with method `GetDefaultEncoderSettings()`

```cs
ScalarEncoder encoder = new ScalarEncoder(GetDefautEncoderSettings());
```

is equivalent to writing the following

```cs
ScalarEncoder encoder = new ScalarEncoder();
encoder.Initialize(GetDefautEncoderSettings());
```

After loading encoder settings into the encoder, method `encoder.Encode()` is invoked to start the encoding process and return an array of '0's and '1's.

```cs
double input = 99.50;
int[] result = encoder.Encode(input);
```

The default encoder settings is summarized as follow:

| Parameter    | Data type | Definition |
| ------------ | --------- | ---------- |
| `N`          | Integer   | 0          |
| `W`          | Integer   | 11         |
| `MinVal`     | double    | 1          |
| `MaxVal`     | double    | 100        |
| `Radius`     | double    | 0          |
| `Resolution` | double    | 0.15       |
| `Periodic`   | boolean   | true       |
| `ClipInput`  | boolean   | true       |

The `ScalarEncoder` encodes number 99.6 and produces the following output with the previous encoder settings.

```
1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1
```

## Current supported encoders

- Boolean Encoder
- Category Encoder
- Datetime Encoder
- Geo-Spatial Encoder
- Multi-Encoder
- Scalar Encoder

## Examples

### Boolean Encoder

```csharp
// Not Implemented
```

### Category Encoder

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

Further unit tests can be found [here](../UnitTestsProject/EncoderTests/CategoryEncoderExperimentalTests.cs)

### Datetime Encoder

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

Further unit tests can be found [here](../UnitTestsProject/EncoderTests/DateTimeEncoderTests.cs)

### Geo-Spatial Encoder

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

Further unit tests can be found [here](../UnitTestsProject/EncoderTests/GeoSpatialEncoderExperimentalTests.cs)

### Multi-Encoder

Further unit tests can be found [here](../UnitTestsProject/EncoderTests/MultiEncoderTests.cs)

```csharp
// Not Implemented
```

### Scalar Encoder

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

Further unit tests can be found [here](../UnitTestsProject/EncoderTests/ScalarEncoderTests.cs)

## How to create new Encoder?

New encoder should extend `EncoderBase` class as follow:

```cs
{
    public class BloodPressureEncoder : EncoderBase
    {
        public override int Width { get; }
        public override bool IsDelta { get { return false; } }
        public override int[] Encode(object inputData)
        {
            // Implementation of the blood pressure encoder
            // Output is 1D array SDR
        }
        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
```
