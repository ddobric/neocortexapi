# How to Create SDR Using Spatial Pooler:

In this tutorial you will see how to generate SDR using Spatial Pooler.

First we need to initialize the Spatial Pooler by setting the `htmConfig` parameters. Spatial Pooler settings are created as a Dictionary consists of keys and their values. The following code snippet illustrates what Spatial Pooler settings we used:

```csharp

 HtmConfig htmConfig = new HtmConfig(new int[] { imgSize, imgSize }, new int[] { 64, 64 })
            {
                PotentialRadius = 10,
                PotentialPct = 1,
                GlobalInhibition = true,
                LocalAreaDensity = -1.0,
                NumActiveColumnsPerInhArea = 0.02 * numOfCols,
                StimulusThreshold = 0.0,
                SynPermInactiveDec = 0.008,
                SynPermActiveInc = 0.05,
                SynPermConnected = 0.10,
                MinPctOverlapDutyCycles = 1.0,
                MinPctActiveDutyCycles = 0.001,
                DutyCyclePeriod = 100,
                MaxBoost = 10.0,
                RandomGenSeed = 42,
                Random = new ThreadSafeRandom(42)

            };
```

These parameters are then assign to `Connections connections = new Connections(htmConfig)`. Once connections of the columns have been defined now by calling `.Init(connections)` function in the Spatial Pooler class we will initialize the SP. Below code snippet show how we have done it:

```csharp
            Connections connections = new Connections(htmConfig);
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(connections, trainingImages.Length * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                isInStableState = true;
                Debug.WriteLine($"Entered STABLE state: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
            });

            SpatialPooler sp = new SpatialPoolerMT(hpa);

            sp.Init(connections);
```

`HomeostaticPlasticityController ` is to trace the state of the stable SDR. Once the stable SDR on any input is achieved it will stop the learning process of the Spatial Pooler.
To start the process of learning method `sp.compute(inputVector, activeArray, true)` is invoked. The argument `inputVector` contains the binaries form of the input (Image/Scalar value etc.). `activeArray` will hold the SDR that is learned by the Spatail Pooler.

### Example No 1:

Now lets take an `line.png` as input and generate its SDR using Spaital Pooler.

```csharp
var trainingImage = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.png");
string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImage}", imgSize, testName);
string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImage}", imgSize, testName);
int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

```
First we need to encode the image into a binary array for that we used the method `NeoCortexUtils.BinarizeImage()` the result will be saved in the `inputVector` in the form of '0's and '1's. Afterward we need to pass `inputVector` to the `sp.compute()` to start the process of Spatial Pooler learning. the Image `line.png` and the `SDR` learned using spatial pooler is shown below:

|`Input Image`|`Binary Array`|`SDR`|
|--|--|--|
|![][img1]|![][img2]|![][img3]|

[img1]: https://user-images.githubusercontent.com/74202550/114102421-91fb2000-98c7-11eb-86a5-8f56f8dd6601.png
[img2]: https://user-images.githubusercontent.com/74202550/114101798-89eeb080-98c6-11eb-85c6-9670aa7ef1da.PNG
[img3]: https://user-images.githubusercontent.com/74202550/114101864-a5f25200-98c6-11eb-885b-284181be99d1.PNG

### Example of Intersection, Overlap and Union between two SDRs:

In this example we will compare `Line.png` and `Image_7.png` and see there comparision between the SDRs of `line.png` and `Image_7.png` using methods Union, Overlap and Intersection. The binary array and the SDR are shown below:

|`Binary Array Line.png`|`SDR Line.png`|`Binary Array Image_7.png`|`SDR Image_7.png`|
|--|--|--|--|
|![][img4]|![][img5]|![][img6]|![][img7]|

[img4]: https://user-images.githubusercontent.com/74202550/114105745-a17d6780-98cd-11eb-9edc-397e74e656e3.PNG
[img5]: https://user-images.githubusercontent.com/74202550/114105650-6bd87e80-98cd-11eb-87a6-fcf57aa828e3.PNG
[img6]: https://user-images.githubusercontent.com/74202550/114105503-29af3d00-98cd-11eb-9322-f05014c9a8e9.PNG
[img7]: https://user-images.githubusercontent.com/74202550/114105531-36339580-98cd-11eb-809e-cbe659c83536.PNG

Now lets create union and overlap of the SDRs using method `UnionArraFun(ActiveArray, CompareArray)` and `OverlapArraFun(ActiveArray, CompareArray)` repectively. The 1st argument it will take is the `ActiveArray` which will hold the the SDR of any image we passed from the directory. `CompareArray` will hold the SDR we want to compare with all the SDR we learned using SP. Below mentioned is the code snippet how we have created the bitmap images of the Union and Overlap:

```csharp

Array = OverlapArraFun(ActiveArray, CompareArray);
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
int[,] twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{outFolder}\\Overlap_{sdrs.Count}.png", Color.PaleGreen, Color.Red, text: $"Overlap.png");

Array = UnionArraFun(ActiveArray, CompareArray);
int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);
NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{outFolder}\\Union_{sdrs.Count}.png", Color.PaleGreen, Color.Green, text: $"Union.png");


```
Once you have created both overlap and union array now you can generate the intersection image between the two SDRs. We have modified the `NeoCortexUtils.DrawBitmap()` code and created a method `NeoCortexUtils.DrawIntersections()` which will take Union and overlapping array as input and generate intersection bitmap as output of two comparing SDRs:

Results are shown here:



|`Intersection`|`Overlap`|`Union`|
|--|--|--|
|![][img8]|![][img9]|![][img10]|

[img8]: https://user-images.githubusercontent.com/74202550/114108635-c4127f00-98d3-11eb-8e1a-6adba0ae5e57.png
[img9]: https://user-images.githubusercontent.com/74202550/114108378-43538300-98d3-11eb-921e-b8ad7d538593.png
[img10]: https://user-images.githubusercontent.com/74202550/114108393-4e0e1800-98d3-11eb-9485-f329d8a7b1bb.png

Now in the intersection image you will see all the columns in gray color it is because no column is overlapping between the SDRs of the `Line.png` and `Image_7.png`, from this information we can see that there is no similarity between the SDR of the comparing images.



### Another Example of Intersection, Overlap and Union between two SDRs:

In this example we will compare `Image_8.png` and `Image_7.png` and see there comparision between the SDRs of `Image_8.png` and `Image_7.png` using methods Union, Overlap and Intersection. The binary array and the SDR are shown below:

|`Binary Array Image_8.png`|`SDR Image_8.png`|`Binary Array Image_7.png`|`SDR Image_7.png`|
|--|--|--|--|
|![][img11]|![][img12]|![][img13]|![][img14]|

[img11]: https://user-images.githubusercontent.com/74202550/114111926-84e82c00-98db-11eb-93f0-9c9395a7b60f.PNG
[img12]: https://user-images.githubusercontent.com/74202550/114111970-9a5d5600-98db-11eb-87ea-1f126f10bc30.PNG
[img13]: https://user-images.githubusercontent.com/74202550/114112002-acd78f80-98db-11eb-917d-4aa986beb604.PNG
[img14]: https://user-images.githubusercontent.com/74202550/114112039-c678d700-98db-11eb-98d9-2e0d54f748ab.PNG


Now lets create union and overlap of the SDRs using method `UnionArraFun(ActiveArray, CompareArray)` and `OverlapArraFun(ActiveArray, CompareArray)` repectively. The 1st argument it will take is the `ActiveArray` which will hold the the SDR of any image we passed from the directory. `CompareArray` will hold the SDR we want to compare with all the SDR we learned using SP. Below mentioned is the code snippet how we have created the bitmap images of the Union and Overlap:


```csharp

Array = OverlapArraFun(ActiveArray, CompareArray);
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
int[,] twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{outFolder}\\Overlap_{sdrs.Count}.png", Color.PaleGreen, Color.Red, text: $"Overlap.png");

Array = UnionArraFun(ActiveArray, CompareArray);
int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);
NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{outFolder}\\Union_{sdrs.Count}.png", Color.PaleGreen, Color.Green, text: $"Union.png");


```
Once you have created both overlap and union array now you can generate the intersection image between the two SDRs. We modified the `NeoCortexUtils.DrawBitmap()` code and created a method `NeoCortexUtils.DrawIntersections()` which will take Union and overlapping array as input and generate intersection bitmap as output of two comparing SDRs:

Results are shown here:



|`Intersection`|`Overlap`|`Union`|
|--|--|--|
|![][img15]|![][img16]|![][img17]|

[img15]: https://user-images.githubusercontent.com/74202550/114112067-da243d80-98db-11eb-8806-a596bd1836aa.png
[img16]: https://user-images.githubusercontent.com/74202550/114112119-fd4eed00-98db-11eb-855e-1ed6222532e1.png
[img17]: https://user-images.githubusercontent.com/74202550/114112151-1192ea00-98dc-11eb-9489-5e12769b0a3b.png


Now in the intersection image you see the columns in gray color it is because no column is overlapping between the SDRs of the `Image_8.png` and `Image_7.png`. Similarity between the two SDRs are shown by red columns.



### Example of Intersection, Overlap and Union between two SDRs using Datetime Encoder:

In this example we will compare two datetime inputs `06/04/2020 01:28:07` and `08/01/2017 11:27:07` and see there comparison between the SDRs of `06/04/2020 01:28:07` and `08/01/2017 11:27:07` using methods Union, Overlap and Intersection. The SDR array of both the inputs are shown below:

```csharp
Input = 06/04/2020 01:28:07
SDR1 = 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0
Input = 08/01/2017 11:27:07
SDR2 = 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0

```

Now lets create union and overlap of the SDRs using method `UnionArraFun1(SDR1, SDR2)` and `OverlapArraFun1(SDR1, SDR2)` repectively. The 1st argument it will take is the `SDR1` which will hold the the SDR of any Datetime input we passed from the directory and similar is the case with `SDR2`. Below mentioned is the code snippet how we have created the bitmap images of the Union and Overlap:


```csharp

Overlaparray = OverlapArraFun1(sdr1, sdr2);
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, 32, 32);
var twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_Union\\Overlap_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

Unionarray = UnionArraFun1(sdr1, sdr2);
int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Unionarray, 32, 32);
int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Overlap_Union\\Union{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Green, text: $"Overlap_{h}_{w}.png");

```
Once you have created both overlap and union array now you can generate the intersection image between the two SDRs. We modified the `NeoCortexUtils.DrawBitmap()` code and created a method `NeoCortexUtils.DrawIntersections()` which will take Union and overlapping array as input and generate intersection bitmap as output of two comparing SDRs:

Results are shown here:

|`Intersection`|`Overlap`|`Union`|
|--|--|--|
|![][img18]|![][img19]|![][img20]|

[img18]: https://user-images.githubusercontent.com/76734938/114115822-9ab61b00-98f4-11eb-80fe-19bf2e7777e5.jpg
[img19]: https://user-images.githubusercontent.com/76734938/114114916-e4056b00-98f2-11eb-9851-c73be60bb25b.png
[img20]: https://user-images.githubusercontent.com/76734938/114114968-fbdcef00-98f2-11eb-8a18-edcd0dd39b88.png


Now in the intersection image you see the columns in gray color it is because no column is overlapping between the SDRs of the `06/04/2020 01:28:07` and `08/01/2017 11:27:07`. Similarity between the two SDRs are shown by red columns.


### Example of Intersection, Overlap and Union between two SDRs using Category Encoder:

In this example we will compare two string inputs `Doctor` and `Engineer` and see there comparison between the created SDRs using methods Union, Overlap and Intersection. The SDR array of both the inputs are shown below:

```csharp
Input = Doctor
SDR1 = 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0
Input = Engineer
SDR2 = 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0

```

Now lets create union and overlap of the SDRs using method `UnionArraFun1(SDR1, SDR2)` and `OverlapArraFun1(SDR1, SDR2)` repectively. The 1st argument it will take is the `SDR1` which will hold the the SDR of any string input we passed from the directory and similar is the case with `SDR2`. Below mentioned is the code snippet how we have created the bitmap images of the Union and Overlap:


```csharp

Overlaparray = OverlapArraFun2(sdr1, sdr2);
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, 32, 32);
var twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_Union\\Overlap_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

Unionarray = UnionArraFun2(sdr1, sdr2);
int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Unionarray, 32, 32);
int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Overlap_Union\\Union{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Green, text: $"Overlap_{h}_{w}.png");

NeoCortexUtils.DrawIntersections(twoDimArray3, twoDimArray1, 100, $"{folder}\\Overlap_Union\\Intersection_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.Black, Color.Gray, text: $"Intersection_{h}_{w}.png");

```
Once you have created both overlap and union array now you can generate the intersection image between the two SDRs. We modified the `NeoCortexUtils.DrawBitmap()` code and created a method `NeoCortexUtils.DrawIntersections()` which will take Union and overlapping array as input and generate intersection bitmap as output of two comparing SDRs:


Results are shown here:

|`Intersection`|`Overlap`|`Union`|
|--|--|--|
|![][img21]|![][img22]|![][img23]|

[img21]: https://user-images.githubusercontent.com/53593496/114378910-9d896800-9ba1-11eb-8808-02bc4df007f7.png
[img22]: https://user-images.githubusercontent.com/53593496/114374860-8779a880-9b9d-11eb-9cd6-48b3698800a7.png
[img23]: https://user-images.githubusercontent.com/53593496/114379445-433cd700-9ba2-11eb-8128-5e05c6ea06e7.jpeg




Now in the intersection image you see the columns in gray color it is because no column is overlapping between the SDRs of the `Doctor` and `Engineer`.


### Another Example of Intersection, Overlap and Union between two SDRs using Category Encoder:
In this model we will look at two city names as string inputs 'Frankfurt' and 'Berlin' and see there correlation between the made SDRs utilizing strategies Union, Overlap and Intersection. The SDR exhibit of both the sources of info are appeared beneath


```csharp
Input = Frankfurt
SDR1 = 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0
Input = Berlin
SDR2 = 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0

```

Now lets create union and overlap of the SDRs using method `UnionArraFun1(SDR1, SDR2)` and `OverlapArraFun1(SDR1, SDR2)` repectively. The 1st argument it will take is the `SDR1` which will hold the the SDR of any string input we passed from the directory and similar is the case with `SDR2`. Below mentioned is the code snippet how we have created the bitmap images of the Union and Overlap:


```csharp

Overlaparray = OverlapArraFun2(sdr1, sdr2);
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Overlaparray, 32, 32);
var twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{folder}\\Overlap_Union\\Overlap_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Red, text: $"Overlap_{h}_{w}.png");

Unionarray = UnionArraFun2(sdr1, sdr2);
int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Unionarray, 32, 32);
int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);

NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{folder}\\Overlap_Union\\Union{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.PaleGreen, Color.Green, text: $"Overlap_{h}_{w}.png");

NeoCortexUtils.DrawIntersections(twoDimArray3, twoDimArray1, 100, $"{folder}\\Overlap_Union\\Intersection_{h.ToString().Replace("/", "-").Replace(":", "-")}_{w.ToString().Replace("/", "-").Replace(":", "-")}.png", Color.Black, Color.Gray, text: $"Intersection_{h}_{w}.png");

```
Once you have created both overlap and union array now you can generate the intersection image between the two SDRs. We modified the `NeoCortexUtils.DrawBitmap()` code and created a method `NeoCortexUtils.DrawIntersections()` which will take Union and overlapping array as input and generate intersection bitmap as output of two comparing SDRs:


Results are shown here:

|`Intersection`|`Overlap`|`Union`|
|--|--|--|
|![][img28]|![][img26]|![][img27]|

[img28]: https://user-images.githubusercontent.com/76734938/114938840-a6ec2c00-9e50-11eb-9a61-c9e69abf4b53.png
[img26]: https://user-images.githubusercontent.com/76734938/114628795-7ed1c100-9cc8-11eb-8cc4-91634e252751.png
[img27]: https://user-images.githubusercontent.com/76734938/114628839-990b9f00-9cc8-11eb-92a3-11f6c6d5d7ef.png


Now in the intersection image you see the columns in gray color it is because no column is overlapping between the SDRs of the `Frankfurt` and `Berlin`.




### Further Example of Intersection, Overlap and Union between two SDRs:

In this case we are comparing `Image_2.png` and `Image_4.png` and we will see the comparision of SDRs of `Image_2.png` and `Image_4.png` using Union, Overlap and Intersection methods. For this the binary array and the SDR are shown as follows:

|`Binary Array Image_2.png`|`SDR Image_2.png`|`Binary Array Image_4.png`|`SDR Image_4.png`|
|--|--|--|--|
|![][img29]|![][img30]|![][img31]|![][img32]|

[img29]: https://user-images.githubusercontent.com/76734938/114944575-e585e480-9e58-11eb-8885-23c638c28779.jpeg
[img30]: https://user-images.githubusercontent.com/76734938/114944598-f6cef100-9e58-11eb-9561-aefa6939503f.jpeg
[img31]: https://user-images.githubusercontent.com/76734938/114944627-01898600-9e59-11eb-9d6f-4936e5ba89e4.jpeg
[img32]: https://user-images.githubusercontent.com/76734938/114944637-08b09400-9e59-11eb-8f00-3e103dc1bf59.jpeg


We have created union and overlap of the SDRs using method `UnionArraFun(ActiveArray, CompareArray)` and `OverlapArraFun(ActiveArray, CompareArray)` repectively. The 1st argument it will take is the `ActiveArray` which is used to hold the SDR of any image we take from the directory. `CompareArray` it will hold the SDR that we want to compare with all the other SDRs we learned using SP. Below mentioned is the code snippet how we have created the bitmap images of the Union and Overlap:


```csharp

Array = OverlapArraFun(ActiveArray, CompareArray);
int[,] twoDimenArray2 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
int[,] twoDimArray1 = ArrayUtils.Transpose(twoDimenArray2);
NeoCortexUtils.DrawBitmap(twoDimArray1, 1024, 1024, $"{outFolder}\\Overlap_{sdrs.Count}.png", Color.PaleGreen, Color.Red, text: $"Overlap.png");

Array = UnionArraFun(ActiveArray, CompareArray);
int[,] twoDimenArray4 = ArrayUtils.Make2DArray<int>(Array, (int)Math.Sqrt(Array.Length), (int)Math.Sqrt(Array.Length));
int[,] twoDimArray3 = ArrayUtils.Transpose(twoDimenArray4);
NeoCortexUtils.DrawBitmap(twoDimArray3, 1024, 1024, $"{outFolder}\\Union_{sdrs.Count}.png", Color.PaleGreen, Color.Green, text: $"Union.png");


```
When you generated the overlap and union then you can make intersection image between two SDRs. We improved the `NeoCortexUtils.DrawBitmap()` code and created a method `NeoCortexUtils.DrawIntersections()` which takes Union and overlapping array as input and create intersection bitmap as output of two contrasting SDRs:

Results are shown below:



|`Intersection`|`Overlap`|`Union`|
|--|--|--|
|![][img33]|![][img34]|![][img35]|

[img33]: https://user-images.githubusercontent.com/76734938/114945263-0d297c80-9e5a-11eb-97e0-cd2ceebf26fa.png
[img34]: https://user-images.githubusercontent.com/76734938/114944681-1e25be00-9e59-11eb-92cb-334b8be44ab0.png
[img35]: https://user-images.githubusercontent.com/76734938/114944695-254ccc00-9e59-11eb-856d-5493221ce1c3.png

Here, in the intersection image the columns which do not have any overlap between SDRs of the `Image_2.png` and `Image_4.png` are represented using gray color. Whereas the similarity between the columns of two SDRs are shown by red color.
