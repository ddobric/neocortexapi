# SDR Representation Using Encoders

An encoder is the component that converts some value to a sparse distributed representation (SDR). Depending on the encoder, the value can be anything (i.E.: scaler value, time, day in a week, geo-location etc.). The SDR is typically an array in '1' and '0'.

### Scalar Encoder Example

In this example we will generate SDR's using Scaler Encoder and represent them in form of text and as a bitmap.

Our inputs are integers from 1 to 5. Encoders need to be first initialized with pre-defined settings using its constructor. Encoder settings are created as a Dictionary consists of properties and their values. Following code snippet illustrates what encoder settings we used:

```csharp
ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
{
    { "W", 25},
    { "N", (int)0},
    { "Radius", (double)2.5},
    { "MinVal", (double)1},
    { "MaxVal", (double)50},
    { "Periodic", false},
    { "Name", "Scalar Encoder"},
    { "ClipInput", false},
});
```

After loading encoder settings into the encoder, method `encoder.Encode()` is invoked to start the encoding process and `NeoCortexApi.Helpers.StringifyVector()`return an array of SDR's that are generated in form of '0's and '1's.
```csharp
var result = encoder.Encode(input);
Debug.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
```


Following are the SDR's generated for input "2"
```csharp
Input = 2
SDRs Generated = 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
```
#### 2- SDR as Text
We can represent SDR as a text using `NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))`. Text representation is basically representation of index numbers where SDR's is '1'. Following is input '2' SDR's represented as text.
```csharp
SDR As Text = 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34,
```
#### 3- SDR as Bitmaps
SDR generated can also be represented in form of bitmaps using function `NeoCortexUtils.DrawBitmap()`. For that 2D array is created using `ArrayUtils.Make2DArray()` and then bitmap can be generated of 2D Array with or without Tranpose function. In this expirement we have taken transpose using `ArrayUtils.Transpose()` function.

```csharp
int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input}.png", Color.Yellow, Color.Black, text: input.ToString());
```
The following table visualizes the result of numbers of the above program using `NeoCortexUtils.DrawBitmap()`:
|`number`|`1`|`2`|`3`|`4`|`5`|
|--|--|--|--|--|--|
|Results|![][img1]|![][img2]|![][img3]|![][img4]|![][img5]|

[img1]: https://user-images.githubusercontent.com/59200478/113517493-ba1e1280-9599-11eb-8dfd-2e11e0ffe729.png
[img2]: https://user-images.githubusercontent.com/59200478/113517510-d5891d80-9599-11eb-8976-be6d4d8c805b.png
[img3]: https://user-images.githubusercontent.com/59200478/113517513-dc179500-9599-11eb-8baf-ee34092cbd16.png
[img4]: https://user-images.githubusercontent.com/59200478/113517520-e3d73980-9599-11eb-956d-c299a5b452f6.png
[img5]: https://user-images.githubusercontent.com/59200478/113517529-efc2fb80-9599-11eb-9eb7-90bbc97d42d5.png

### Scalar Encoder Example 2
In this example we will go through step-by-step procedure to create bitmaps and SDR as text of any scalar value. This time we will encode the scalar value from 1 to 6 sequence using different scalar encoder parameters given below:

```csharp
 ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 100},
                { "MinVal", (double)0},
                { "MaxVal", (double)99},
                { "Periodic", true},
                { "Name", "Scalar Sequence"},
                { "ClipInput", true},
            });
```
The weight "W" is 3 and number of bits "N" is 100, so every input will be encoded with 3 nos of bits set to "1" in the total of 100 bits. Now lets use the method `encoder.Encode()` to start the encoding process. As a result you will get binary arrays in "0" and "1" forms.

For example after encoding the sequence from 1 to 6. The binary array as a result are:
```csharp
Input = 1
Result = 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

Input = 2
Result = 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

Input = 3
Result = 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

Input = 4
Result = 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

Input = 5
Result = 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

Input = 6
Result = 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

```
Now we have the encoded arrays of value 1 to 6. We have two ways to represent these array. Both ways are shown below:

#### 2- SDR as Text

To show it as Text we need this:
```csharp
var activeCols = ArrayUtils.IndexWhere(result, (el) => el == 1);
```
It takes the indices of the array where the value is "1" and store those indices in another array `activeCols`. By using function `Helpers.StringifyVector(activeCols)` we can print "SDR as Text" of any encoded array. In our case "SDR as text" from 1 to 6 are:

|`number`|`1`|`2`|`3`|`4`|`5`|`6`|
|--|--|--|--|--|--|--|
|activeCols|0, 1, 2,|1, 2, 3,|2, 3, 4,|3, 4, 5,|4, 5, 6,|5, 6, 7,|


#### 3- SDR as Bitmap

To show it as a bitmap image we need to first convert the one dimensional binary array into two-dimensional binary array using predefined function `ArrayUtils.Make2DArray`

```csharp
int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
```
1st argument `result` is one dimensional binary array, 2nd argument `(int)Math.Sqrt(result.Length)` will give the rows value, 3rd argument `(int)Math.Sqrt(result.Length)` will give the columns value.

Then we can to take the transpose using `ArrayUtils.Transpose()` function. It is optional just to set the orientation of the bitmap image. Further we will pass the `twoDimArray` to create its bitmap image using `NeoCortexUtils.DrawBitmap` function. Syntax is given below:
```csharp
NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input}.png",Color.PaleGreen, Color.Blue, text: input.ToString());
```

In this function, 1st argument `twoDimArray` is two-dimensional array, 2nd argument `1024`(in pixels) is the width of the bitmap we want to set, 3rd argument `1024`(in pixels) is the height we want to set of the bitmap, 4th argument `$"{outFolder}\\{input}.png"` is the file path where you want to store it, 5th argument `Color.PaleGreen` is the color we set for `"0"s` in the two-dimensional array, 6th argument `Color.Blue` is the color we set for `"1"s` in the two-dimensional array.

|`number`|`1`|`2`|`3`|`4`|`5`|`6`|
|--|--|--|--|--|--|--|
|Results|![][img6]|![][img7]|![][img8]|![][img9]|![][img10]|![][img11]|

[img6]: https://user-images.githubusercontent.com/74202550/113589152-53e4ce80-9631-11eb-8f86-188daf8e6caa.png
[img7]: https://user-images.githubusercontent.com/74202550/113589178-5c3d0980-9631-11eb-8290-52e40a344508.png
[img8]: https://user-images.githubusercontent.com/74202550/113589206-63641780-9631-11eb-9602-420337310166.png
[img9]: https://user-images.githubusercontent.com/74202550/113589222-68c16200-9631-11eb-8861-6c719040ed2e.png
[img10]: https://user-images.githubusercontent.com/74202550/113589243-6d861600-9631-11eb-80de-906f262d64cd.png
[img11]: https://user-images.githubusercontent.com/74202550/113589309-7d9df580-9631-11eb-9818-d6c819e1b572.png


### DateTime Encoder

In this example we will generate SDR's using DateTime Encoder and represent them in form of text and as a bitmap.

Our inputs are different date and times. As in every encoder, encoders need to be first initialized with pre-defined settings using its constructor. Encoder settings are created as a Dictionary consists of properties and their values. Following code snippet illustrates what encoder settings we used:

```csharp
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
```

After loading encoder settings into the encoder, method `encoder.Encode()` is invoked to start the encoding process and `NeoCortexApi.Helpers.StringifyVector()`return an array of SDR's that are generated in form of '0's and '1's.
```csharp
var result = encoder.Encode(DateTimeOffset.Parse(input.ToString()));
Debug.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(result)}");
```


Following are the SDR's generated for Datetime input "05/02/2020 22:58:07"
```csharp
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
```

#### 2- SDR as Text
We can represent SDR as a text using `NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))`. Text representation is basically representation of index numbers where SDR's is '1'. Following is input '05/02/2020 22:58:07' SDR's represented as text.
```csharp
SDR As Text = 910, 911, 912, 913, 914, 915, 916, 917, 918, 919, 920, 921, 922, 923, 924, 925, 926, 927, 928, 929, 930,
```
#### 3- SDR as Bitmaps
SDR generated can also be represented in form of bitmaps using function `NeoCortexUtils.DrawBitmap()`. For that 2D array is created using `ArrayUtils.Make2DArray()` and then bitmap can be generated of 2D Array with or without Tranpose function. In this expirement we have taken transpose using `ArrayUtils.Transpose()` function.

```csharp
int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 32, 32);
var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{input.ToString().Replace("/", "-").Replace(":", "-")}_32x32-N-{encoderSettings["DateTimeEncoder"]["N"]}-W-{encoderSettings["DateTimeEncoder"]["W"]}.png");
```
The following table visualizes the result of numbers of the above program using `NeoCortexUtils.DrawBitmap()`:
|`Datetime`|`05/02/2020 22:58:07`|`06/04/2020 01:28:07`|`07/09/2019 21:15:07`|`08/01/2018 11:27:07`|
|--|--|--|--|--|
|Results|![][img12]|![][img13]|![][img14]|![][img15]|

[img12]: https://user-images.githubusercontent.com/59200478/113608335-e00fe980-9663-11eb-9b8f-f9c30ce4ae28.png
[img13]: https://user-images.githubusercontent.com/59200478/113608377-f1f18c80-9663-11eb-987e-f11f472b1ecb.png
[img14]: https://user-images.githubusercontent.com/59200478/113608400-fb7af480-9663-11eb-8c87-80f3d9d49cf0.png
[img15]: https://user-images.githubusercontent.com/59200478/113608430-05045c80-9664-11eb-8389-506784e1f1d6.png



### Category Encoder
In this example we will generate SDR's using Category Encoder and represent them in form of text and as a bitmap.

Our inputs are different strings which are given as an array. As in every encoder, encoders need to be first initialized with pre-defined settings using its constructor. Encoder settings are created as a Dictionary consists of properties and their values. Following code snippet illustrates what encoder settings we used:

```csharp
 Dictionary<string, object> encoderSetting = getDefaultSettings(); // creaing default constructor
            static Dictionary<string, object> getDefaultSettings()
            {
                Dictionary<String, Object> encoderSetting = new Dictionary<string, object>();
                encoderSetting.Add("W", 3);
                encoderSetting.Add("Radius", (double)1);
                return encoderSetting;
            }
```

After loading encoder settings into the encoder, method `encoder.Encode()` is invoked to start the encoding process and `NeoCortexApi.Helpers.StringifyVector()`return an array of SDR's that are generated in form of '0's and '1's.
```csharp
 foreach (string input in arrayOfStrings)
            {
                var results = categoryEncoder.Encode(input); // encoding string "Doctor"

                Debug.WriteLine($"Input = {input}");
                Debug.WriteLine($"SDRs Generated = {NeoCortexApi.Helpers.StringifyVector(results)}");
                Debug.WriteLine($"SDR As Text = {NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(results, k => k == 1))}");


                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(results, (int)Math.Sqrt(results.Length), (int)Math.Sqrt(results.Length));
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 512, 512, $"{outFolder}\\{input}.png", Color.Pink, Color.White, text: input.ToString());
            }
```


Following are the SDR's generated for input string "Doctor"
```csharp
1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
```

#### 2- SDR as Text
We can represent SDR as a text using `NeoCortexApi.Helpers.StringifyVector(ArrayUtils.IndexWhere(result, k => k == 1))`. Text representation is basically representation of index numbers where SDR's is '1'. Following is input 'Doctor' SDR's represented as text.
```csharp
SDR As Text = 0, 1, 2,
```
#### 3- SDR as Bitmaps
SDR generated can also be represented in form of bitmaps using function `NeoCortexUtils.DrawBitmap()`. For that 2D array is created using `ArrayUtils.Make2DArray()` and then bitmap can be generated of 2D Array with or without Tranpose function. In this expirement we have taken transpose using `ArrayUtils.Transpose()` function.

```csharp
 int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(results, (int)Math.Sqrt(results.Length), (int)Math.Sqrt(results.Length));
var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
NeoCortexUtils.DrawBitmap(twoDimArray, 512, 512, $"{outFolder}\\{input}.png", Color.Pink, Color.White, text: input.ToString());
```
The following table visualizes the result of numbers of the above program using `NeoCortexUtils.DrawBitmap()`:
|`Category`|`Doctor`|`Engineer`|`Lawyer`|`Scientist`|
|--|--|--|--|--|
|Results|![][img16]|![][img17]|![][img18]|![][img19]|

[img16]: https://user-images.githubusercontent.com/53593496/113619949-14d76d00-9673-11eb-8c85-4cacd1df2950.png
[img17]: https://user-images.githubusercontent.com/53593496/113620001-29b40080-9673-11eb-9b43-5b034cbf4a8d.png
[img18]: https://user-images.githubusercontent.com/53593496/113620036-36385900-9673-11eb-880f-f1f6e8739af7.png
[img19]: https://user-images.githubusercontent.com/53593496/113620100-4bad8300-9673-11eb-9a55-745038b574bd.png





