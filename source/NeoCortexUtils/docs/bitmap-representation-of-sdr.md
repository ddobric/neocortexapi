# Bitmap Representation of SDRs

**Sparse Distributed Representations** (SDRs) are a fundamental concept in various fields, including machine learning, neuroscience, and data encoding. At their core, SDRs are binary vectors where only a small fraction of the elements are **active (set to 1)**, while the majority remain **inactive (set to 0)**. This sparse nature enables efficient representation and processing of complex data.

This document explains how to generate SDR using ScalarEncoder, DateTimeEncoder, GeoSpatialEncoder, Spatial Pooler and represent them into bitmaps.

The Bitmap Representation of SDRs is to provide a visual representation of the generated SDRs. This visualization helps in analyzing the encoded information, debugging the models, and communicating the results effectively.



## Encoder
Encoding real-world data into SDRs is a very important process to understand in HTM. Semantic meaning within the input data must be encoded into a binary representation. 

```C#
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
Encoders need to be first initialized with pre-defined settings using its constructor. Encoder settings are created as a Dictionary consists of properties and their values. Following code snippet illustrates how to create encoder settings:

### Parameter Definition
| Parameter	   | Data type | Definition |
| -------------| ------------- | -------- |
| ```N```|  Integer  | The number of bits in the output. Must be greater than or equal to ``W``|
| ```W```|  Integer  | The number of bits that are set to encode a single value - the “width” of the output signal restriction: w must be odd to avoid centering problems. |
| ```MinVal```| double  | The minimum value of the input signal. |
| ```MaxVal```| double  | The upper bound of the input signal. (input is strictly less if ``Periodic == True``) |
| ```Radius```| double  | 	Two inputs separated by more than the radius have non-overlapping representations. Two inputs separated by less than the radius will in general overlap in at least some of their bits. You can think of this as the radius of the input. |
| ```Resolution```|  double  | Two inputs separated by greater than, or equal to the resolution are guaranteed to have different representations. |
| ```Periodic```|  boolean  | If true, then the input value “wraps around” such that ``Minval`` = ``Maxval``. For a periodic value, the input must be strictly less than ``Maxval``, otherwise ``Maxval`` is a true upper bound. |
| ```ClipInput```|  boolean  | if true, non-periodic inputs smaller than minval or greater than maxval will be clipped to minval/maxval |


The following section demonstrate how to use an encoder.

To initialize an encoder, use the code snippet with method ```GetDefaultEncoderSettings()```
```C#
ScalarEncoder encoder = new ScalarEncoder(GetDefautEncoderSettings());
```
is equivalent to writing the following
```C#
ScalarEncoder encoder = new ScalarEncoder();
encoder.Initialize(GetDefautEncoderSettings());
```
After loading encoder settings into the encoder, method ```encoder.Encode()``` is invoked to start the encoding process and return an array of '0's and '1's.
```C#
double input = 99.50;
int[] result = encoder.Encode(input);
```
#### The default encoder settings is summarized as follow:

| Parameter	   | Data type | Definition |
| -------------| ------------- | -------- |
| ```N```|  Integer  | 0 |
| ```W```|  Integer  | 11 |
| ```MinVal```| double  | 1 |
| ```MaxVal```| double  | 100 |
| ```Radius```| double  | 0 |
| ```Resolution```|  double  | 0.15 |
| ```Periodic```|  boolean  | true |
| ```ClipInput```|  boolean  | true |

The ``ScalarEncoder`` encodes number 99.6 and produces the following output with the previous encoder settings.




## DrawBitmap Method

The method involves generating bitmap images from arrays of active columns using a common method ```DrawBitmap()```  for all the encoders and spatial pooler. This method takes several parameters such as the array of active columns, output width and height, file path for saving the bitmap, colors for inactive and active cells, and optional text to be written with the bitmap.

```C#
void DrawBitmap(int[,] twoDimArray, int width, int height, String filePath, Color inactiveCellColor, Color activeCellColor, string text = null);
```
The key steps in the ```DrawBitmap()``` method are as follows:

- The size of the bitmap is determined based on the specified width and height parameters.
- The active and inactive colors for the bitmap are set.
- For each index in the two-dimensional array, if the corresponding value is 1, the active cell color is set; otherwise, the inactive cell color is set.
- The bitmap is generated and saved in the specified file path.


### Parameters:

- **twoDimArray:** Array of active columns.
- **width:** Output width of the bitmap.
- **height:** Output height of the bitmap.
- **filePath:** The file path to save the bitmap and bitmap filename.
- **inactiveCellColor:** Color for the inactive cells.
- **activeCellColor:** Color for the active cells.
- **text:** Text to be written with the bitmap.


### Description
The parameters above collectively define the properties and appearance of the bitmap image generated from the provided SDRs. They allow for customization of the image's dimensions, colours for active and inactive cells, as well as the ability to add text for additional context or labeling.

The ```twoDimArray``` array likely contains the binary representation of the SDR where each element corresponds to a column and indicates whether it's active or inactive. For the chosen indexes X, Y, If ```twoDimArray[X, Y]==1```, then the activeCellColor is set using the ```SetPixel``` function. Otherwise, the inactiveCellColor is set for that pixel.

```width``` determines the number of pixels **horizontally** in the resulting bitmap image. Like width, ```height``` determines the number of pixels **vertically** in the resulting image. The size of the bitmap can be altered by changing the values of ```width``` and ```height```.

Then using a parameter **scale** which is calculated using ```scale=width/w``` (width: requested width and w: width inside of array), the width and height are given to the bitmap (**wscale, hscale)**.

The different coloured bitmaps can be generated by setting ```activeCellColor``` and ```inactiveCellColor``` to different colours are desired. **Inactive cells** typically correspond to **0**s in the SDR representation. **Active cells** typically correspond to **1**s in the SDR representation.

Once the bitmap is created, it will be saved at the ```filePath``` location with the specified filename.

The below method is used to generate a bitmap image.
```C#
NeoCortexUtils.DrawBitmap(twoDimArray, 1024,1024, $"{outFolder}\\{input}.png", Color.Yellow,Color.Black, text: input.ToString());
```



## 1-D array and 2-D array

1-D array is one-dimensional array, also known as a **vector or list**, is a linear collection of elements stored in contiguous memory locations.
Each element in a one-dimensional array is accessed using a **single index**.

For example, consider an array representing the temperatures of a week:
```c#
int[] temperatures = { 70, 72, 68, 74, 75, 71, 73 };
```
Here, temperatures is a one-dimensional array (int[]) containing temperature values for each day of the week. Accessing a specific temperature, such as the temperature on Tuesday (72), is done using an index (temperatures[1]).

2-D array is two-dimensional array, also known as a **matrix**, is a collection of elements organized in rows and columns.
Each element in a two-dimensional array is accessed using **two indices**: one for the row and one for the column.

For example, consider a two-dimensional array representing a grid of pixels in an image:
```c#
int[,] pixels = {
    { 255, 255, 255 },
    { 0, 0, 0 },
    { 255, 255, 255 }
};
```
Here, pixels is a two-dimensional array (int[,]) containing pixel values. Each element represents the color intensity at a specific row and column in the image. Accessing a specific pixel, such as the pixel at row 1, column 1 (0), is done using two indices (pixels[1, 1]).

In this document, ```Encode()``` method processes the input and a one-dimensional array (**1-D array**) is generated, and stored in ```result2``` which are SDRs. ```twoDimenArray2``` is then generated from **result2** by converting it into a two-dimensional array(**2-D array**), possibly for visualization or further processing.
```C#
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(result2, (int)Math.Sqrt(result2.Length), (int)Math.Sqrt(result2.Length));
```








## SDR Generation Using ScalarEncoder
Scalar encoding is a technique used to convert continuous scalar values into Sparse Distributed Representations (SDRs), commonly employed in neural network models.
By partitioning the input range into smaller bins and activating specific bits within each bin, scalar encoding generates binary vectors that represent numerical values. 

These binary vectors, or SDRs, are sparse, meaning only a small fraction of bits are active at any given time, preserving semantic information in a high-dimensional space.Scalar encoding facilitates the representation of numerical data in a format suitable for processing by neural networks, enabling tasks such as classification, regression, and anomaly detection.

### DrawBitmap sample for Scalar Encoder

First, scalar encoders are initialized with predefined settings using their constructors. These settings are stored in a dictionary, specifying parameters such as **"W"** (width of each encoding), **"N"** (total number of bits in the encoding), **"MinVal"** (minimum value of the input), **"MaxVal"** (maximum value of the input), and others.

Once the encoder is initialized, the ```Encode()``` method is invoked with the input values to start the encoding process. This method returns binary arrays representing the encoded values.

To generate Bit Maps in Scalar Encoder the printBitMap function is called in the Test Method. Here we take **TestMethod9** in our Unit Test into account.
In case of Scalar Encoder numbers are encoded.
In **TestMethod9** we are encoding the Air Quality Index (AQI) to which humans are exposed to. The value of Air Quality Index varies from 0 to 500.
The data that we have used is that the quality of air that humans are exposed to 0-49 is Good; 50-149 is Moderate; 150-249 is Unhealthy for sensitive groups;
250-349 is Unhealthy; 350-449 is Very Unhealthy; 450-500 is Hazardous.

The function **printBitMap** is called in **TestMethod9**. Once executed, **printBitMap** encodes the input and coverts it to a 1-D Array.
This output is equal to the SDR . This 1-D Array is stored in result1.
This 1-D Array is then converted to a 2-D array using

```C#
int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result1, (int)Math.Sqrt(result1.Length), (int)Math.Sqrt(result1.Length));
```

This resulting 2-D array is then transposed using

```C#
var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
```

This transposed array is then passed to the **DrawBitmap** method

```C#
NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, Path.Combine(folderName, filename), Color.Yellow, Color.DarkOrange, text: i.ToString());
```

In this example, the value of N is set 20 while initializing the encoding parameters for the encoder. So, the size of SDR which is saved in result1 is 20.

As this value is then converted to 2-D Array, hence 20 is paased to the **(int) Math.Sqrt(result1.Length)** and **(int) Math.Sqrt(result1.Length)**
This becomes  **(int) Math.Sqrt(20) = 4** and **(int) Math.Sqrt(20) = 4**

These results are then transposed and passed to the DrawBitmap method. In this method  In this method
- Height and Width of the Bit Map is set to 1024.
- The Path to the output folder is also mentioned. The generated Bit Maps will be saved in this folder.
- An active bit in the SDR is represented by Dark Orange colour and an inactive bit is represented by Yellow colour.
- We also see the value of i for which the Bit Map is generated in the top left corner of the Bit Map.

### The generated SDRs are as follows
|``For Range 0-49``|``For Range 50-149``|``For Range 150-249``|``For Range 250-349``|``For Range 350-449``|``For Range 450-500``|
|----------|---------|-------|--------|--------|----------|
| ![24,5](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/d868d946-15e3-4405-b447-b39681c700a1) | ![99,5](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/a3a522ac-5f3c-4b8e-9d65-fc7a1bc74057)   |  ![199,5](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/7b91ab36-c42a-4792-8f9e-3d64a8abfa7e)  |   ![299,5](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/8035f1dc-db02-4b31-a193-309f56f9de14) |   ![399,5](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/74549c24-e178-4e61-91e4-7776a1ef9272) |  ![475](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/a1e8e00e-92ac-46c1-9399-3112923d1a13)  |       


In this example, We have width = 1024 and w =20.

**scale = width / w**. So, scale =  1024 / 20 = 51

For the SDR bit at index **[Xcount, Ycount]** in **twoDimArray**, the colour is set for scale * scale (51 x 51 = 2601) pixels out of 1024 x 1024 pixels.
More example can be found [here](https://github.com/ddobric/neocortexapi/blob/add99c3396c55a14bf397050fd34ec42b34707f7/source/NeoCortexApi/Encoders/ScalarEncoder.cs#L28) 








## SDR Generation Using DateTime Encoder

The DateTime Encoder serves as a crucial component for encoding date and time information into Sparse Distributed Representations (SDRs). Initialized with 
specific settings such as width (W), number of bits (N), minimum and maximum values, periodicity, and padding, this encoder efficiently processes date and time inputs through its Encode() method.
 

### DrawBitmap sample for DateTime Encoder
The DateTime Encoder is a component used to **encode date and time information** into SDRs. 

The DateTime Encoder is initialized with specific settings include parameters such as the **width (W)** and **number of bits (N)** for the encoder, **minimum and maximum values**, **periodicity**, and **padding**. Once initialized, the encoder's ```Encode()``` method is invoked with a specific date and time input. 

This method processes the input and a one-dimensional array (**1-D array**) is generated, which represents a Sparse Distributed Representation (SDR). 

The resulting SDR can be represented both as text and as a bitmap. 

In bitmap representation, the SDR is converted into a **2-D array** (twoDimArray), and then the SDRs can be further visualized using tools like DrawBitmap(), which generates bitmap images from the SDRs.

In our example, ```result2``` contains the 1-D array.
This 1-D array is converted to a 2-D array by:

```C#
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(result2, (int)Math.Sqrt(result2.Length), (int)Math.Sqrt(result2.Length));
```

and then the transpose of the 2-D array (twoDimArray) is passed to the DrawBitmap method.

```C#
var twoDimArray2 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray2, 1024, 1024, $"{prefix}_out_{input.ToString().Replace("/", "-").Replace(":", "-")}_32x32-N-{encoderSettings2["DayOfWeekEncoder"]["N"]}-W-{encoderSettings2["DayOfWeekEncoder"]["W"]}.png", Color.Yellow, Color.Black);
```
In ```twoDimenArray2```, **N=156**, so the size of 1-D array (result2) is 156, and the 2-D array is **(int)Math.Sqrt(156) = 12**, **(int)Math.Sqrt(156)=12**.

In ```DrawBitmap```, Width and height given is: 1024,1024 respectively. Color for inactive cell is set to Black and for active cell is set to Green.

The generated SDR for TestMethod8 with inputs is:

|``input``| ``"05/07/2011 21:58:07"	`` |``"06/07/2012 21:58:07"	``|``"07/07/2013 21:58:07"``|``"08/07/2014 21:58:07"``|
|----------|---------|-------|--------|--------|
|Results|![DateTime_out_05-07-2011 21-58-07_32x32-N-1024-W-21](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/84415515-4a61-4195-ae4f-ba82d1dc7484) |![DateTime_out_06-07-2012 21-58-07_32x32-N-1024-W-21](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/3dee3db4-e272-4eaf-b7c5-e020d91fe687)|![DateTime_out_07-07-2013 21-58-07_32x32-N-1024-W-21](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/5c371a76-6701-44aa-a32c-4450f9719ee4)|![DateTime_out_08-07-2014 21-58-07_32x32-N-1024-W-21](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/d06edb69-1e89-4b42-9da9-be57c51407ea)|


Further unit tests can be found [here](https://github.com/ddobric/neocortexapi/blob/ac26d4acecea21d74b6648cf23c6360a244d1f9d/source/UnitTestsProject/EncoderTests/DateTimeEncoderTests.cs#L21)











## SDR Generation Using Geo-Spatial Encoder

The Geospatial Encoder facilitates the conversion of geospatial data into binary arrays and enables the visualization of this data as bitmap images. In the exploration of geospatial data through Sparse Distributed Representations (SDRs), we utilize the DrawBitmap method to translate encoded geographical coordinates into visually interpretable bitmap images. This approach allows for the visualization of spatial information encoded within SDRs, offering insights into the encoded geographical regions.


### DrawBitmap sample for Geospatial Encoder

The Geospatial Encoder facilitates the conversion of geospatial data into binary arrays and enables the visualization of this data as bitmap images.

 **TestMethod5**
 To generate SDRs and and create bitmap, the values of W, N, MinVal and MaxVal are set as below. These settings define how the geospatial data will be encoded into binary arrays.

                ```encoderSettings.Add("W", 103);
                   encoderSettings.Add("N", 238);
                   encoderSettings.Add("MinVal", (double)48.75);
                   encoderSettings.Add("MaxVal", (double)51.86);```

For bitmaps in the **printImage** method

```C#
for (double j = (long)encoder.MinVal; j < (long)encoder.MaxVal; j +=1)
````

initialises j with minimum value 48 and iterates the loop for bitmap until j less than the maximum value 51, creating bitmap for 48, 49 and 50.


```C#
var result2 = encoder.Encode(j);
```

**result2** generates 1-D array of size 238.

```C#
int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result2,  (int)Math.Sqrt(result2.Length), (int)Math.Sqrt(result2.Length));
```

**twoDimenArray** generates 2-D array with the size 15*15 since Square root of 238 =15.

```C#
var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
```

**twoDimArray** transpose of twoDimenArray (rows becomes the columns)

This **twoDimArray** is then passed to ```Drawbitmap``` method.
Bitmap of Height and the width 1024
Inactive cell are represted by red color and active cells black color.


```C#
public static void DrawBitmap(int[,] twoDimArray, int width, int height, String filePath, Color inactiveCellColor, Color activeCellColor, string text = null)
```

```C#
NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{folderName}\\{j}.png", Color.Red, Color.Black, text: j.ToString());
```

For the 2D array, **w=15** and **h=15**.

The w(15) and h(15) of array must be always lesser than the width(1024) and height(1024) of bitmap.

If twoDimArray[Xcount, Ycount] == 1, then Pixel is set to **active cell color Black** else it is set to **inactive cell color Red**.
The bitmaps generated are as below.

The following table visualizes the result from several ``input`` of the above unit test using ``NeoCortexUtils.DrawBitmap()``:
|``input``| ``48`` |``49``|``50``|
|----------|---------|-------|--------|
|Results|![48](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/1240a76e-7963-494e-a921-12d4f0c8150e)|![49](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/c3b31a7a-c7e4-42c7-b33f-6dd07b90e187)|![50](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/99d8d4da-f25f-4027-ab96-4fc7037fe22f)|


Further unit tests can be found [here](https://github.com/ddobric/neocortexapi/blob/ac26d4acecea21d74b6648cf23c6360a244d1f9d/source/UnitTestsProject/EncoderTests/GeoSpatialEncoderExperimentalTests.cs#L34) 

## Changes In The Size And Color of Bitmap

Modifing the parameters of the encoder and bitmap, leades to change in the resulting SDRs and their bitmap representations.

- By modifing the following parameters as:

  ```W= 21, N=40  MinVal=48.75  MaxVal=51.86```

  The resulting 1-D array size is 40 and is converted to a 2-D array with dimensions 6×6 (w and h of 2D array is 6.).

- Now, The **height and the width** of the bitmap are both set to **1024 pixels**.

   **Scale = width / w** = 1024/6 = 170
   
  Therefore, each cell in the 2-D array corresponds to 170 pixels in the bitmap.

- The color of the bitmap is changed to **Red for inactive cells** and **green for active cells**.

```C#
NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{folderName}\\{j}.png", Color.Red, Color.Green, text: j.ToString());
```

The SDR’s generated for input 51.85 is
```C#
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
```

The bitmaps generated in this case are:
|``Input Image``|``Binary Image``| ``SDR``|
|--------|-------|-------|
|![48](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/830ff6b8-0a84-4517-aea4-900f7eb1cf65)|![49](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/822bd41c-5987-45a1-a782-43bdcfb04ad7)|![50](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/941cbdcd-e10b-429c-af22-af13c747e6e7)|


## Bitmap representation of Image using Spatial Pooler

HTM (Hierarchical Temporal Memory) principles are used to train a Spatial Pooler (SP) using a set of input images and generate Sparse Distributed Representations (SDRs) for each input image. The SP is trained to recognize patterns in the input images and generate corresponding SDRs that represent those patterns. These SDRs are then visualized as bitmaps to observe the activation patterns of the SP columns.

- The SP is initialized with parameters such as potential radius, potential percentage, global inhibition, local area density, etc., defined in the `HtmConfig` object.
- Training images are loaded from the specified directory (`trainingFolder`).
  ```C#
  string trainingFolder = @"..\..\..\TestFiles\Sdr";
  var trainingImages = Directory.GetFiles(trainingFolder, "*.jpeg");
  ```
- For each image, the input vector is computed using `NeoCortexUtils.BinarizeImage()` and then read into an array using `NeoCortexUtils.ReadCsvIntegers`.
  
  ```C#
  string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImage}", imgSize, testName);
  int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();
  ```
- The input vector is fed into the SP using the `sp.compute()` method, which calculates the active columns based on the input.
  ```C#
  sp.compute(inputVector, activeArray, true);
  ```
- The active columns are determined by finding the indices where the value is equal to 1 in the output array of the SP.
  ```C#
  var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
  ```
- Once the system is in a stable state (indicating convergence), the results are calculated and stored.  
- The active columns obtained from the SP represent the SDR for the input image.
- The SDR is stored in a dictionary (`sdrs`) with the image file name as the key.

The activeArray computed by the spatial pooler is converted to a 2-dimentional array with the help of `ArrayUtils.Make2DArray`.
```C#
int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, colDims[0], colDims[1]);
```
In this case the dimensions are set to be 64x64. The 2-D array can be further transposed and used as an input parameter for Bitmap function.
SDRs generated can be represented in graphical presentation with the help of bitmaps.

```C#
System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(bmpWidth, bmpHeight);
```

 Bitmap function can be accessible from the library “System.Drawing” which needs 2 parameters – the bitmap width and bitmap height.

```C#
NeoCortexUtils.DrawBitmaps(arrays, outputImage, Color.Yellow, Color.Gray, OutImgSize, OutImgSize);
```

The DrawBitmaps function of the NeoCortexUtils helps to build the SDR represention. It takes 6 parameters to process the final bitmap.
|``Input Image``|``Binary Image``| ``SDR``|
|--------|-------|-------|
|![input](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/1dfe1a2d-1869-45ff-affc-894dad9afd03)|![binary](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/3f422ab8-b697-4669-bfd3-66eff03384ec)|![SDR](https://github.com/Yatish0/neocortexapi_Team_PY/assets/117783043/80c02d24-8dd6-4782-ae58-db0337b531de)|


Further unit tests can be found [here](https://github.com/ddobric/neocortexapi/blob/ac26d4acecea21d74b6648cf23c6360a244d1f9d/source/UnitTestsProject/SdrRepresentation/ScalarEncoderTestOverBitmap.cs#L16)



### Example representing an Alphabet L in Bitmap after computing in spatial pooler
2-D array with dimension 64x64 containing the SDRs, generated from spatial pooler has to be represented in Bitmap. The dimensions of the bitmap are chosen to be 1024x1024.
Since the 2-D array is of a smaller dimension to that of Bitmap, a scale is considered here so that the bitmap with dimension 1024x1024 can be fully utilized to represent
the 64x64 2-D array.

```C#
var scale = ((bmpWidth) / twoDimArrays.Count) / (w+1) ;
```

In this use case, the scale is calculated to be 7 with the below values set to the variables

- var scale = (1024 / 2) / (64+1);

The count for the 2D arrays is 2 here because it represents 2 Bitmaps- one for a 64x64 SDR representation and one is 28x28 for the binarized alphabet L representation.

<img src="https://user-images.githubusercontent.com/74201563/113511808-0baaab00-9562-11eb-81ea-3ccc35eaa34d.png" width="450"><br />

With scale set to 7, for each position of 64x64 2-D array is set to 49 pixel positions in the 1024x1024 bitmap.

For example, if the scale varies to 15 with the following changes in the variable values to *var scale = (1024 / 1) / (64+1)* then each position in 64x64 2D array is represented
by 225 pixel positions of 1024x1024 bitmap as below.

<img src="https://user-images.githubusercontent.com/74201563/113511852-3563d200-9562-11eb-8eb7-55ce5127983d.png" width="450"><br />

With the help of the below code snippet
```C#
if (arr[Xcount, Ycount] == 1)
    {
        myBitmap.SetPixel(n * (bmpWidth / twoDimArrays.Count) + Xcount * scale + padX, Ycount * scale + padY, activeCellColor);
        k++;
    }
else
    {
        myBitmap.SetPixel(n * (bmpWidth / twoDimArrays.Count) + Xcount * scale + padX, Ycount * scale + padY, inactiveCellColor);
        k++;
    }
```
For each active bit in 64x64 2D array and with the scale of 7, 49 pixel positions in 1024x1024 are set to color **Grey** and the rest inactive bits to **yellow**.

The dimensions of the bitmap to be represented can be changed by providing the values for width and height in ```DrawBitMap()``` function.

### Example representing Overlap(Intersection),Difference and Union for Alphabet L and V in Bitmap after computing in spatial pooler
General Bitmap representation of L and V:

<img src="https://user-images.githubusercontent.com/74201563/114621457-014c8780-9cad-11eb-88aa-211f8af78214.png" width="450"><br />

Below are the represenatation for Overlap, Difference and Union:

<img src="https://user-images.githubusercontent.com/74201563/114621659-42dd3280-9cad-11eb-8ff3-be63f8b7f4d2.png" width="450"><br />

Two SDRs are compared with the help of **UnionSDRFun(), DiffSDRFun()** and **OverlapSDRFun()** functions.
The overlap.png shows very few intersections as the SDRs are very different from each other. Basically, the combination of Overlap and Difference gives us Union.

### Example representing Overlap(Intersection),Difference and Union for Alphabet L and small l in Bitmap after computing in spatial pooler
General Bitmap representation of L and l:

<img src="https://user-images.githubusercontent.com/74201563/114622067-ca2aa600-9cad-11eb-9433-739543ee776f.png" width="450"><br />

Below are the represenatation for Overlap, Difference and Union:

<img src="https://user-images.githubusercontent.com/74201563/114622123-da428580-9cad-11eb-829b-d5af3aebd614.png" width="450"><br />

On comaprison between two SDRs, the overlap.png shows more  overlaps/intersections in comparison to the above example. SDRs are simlar to each other.
