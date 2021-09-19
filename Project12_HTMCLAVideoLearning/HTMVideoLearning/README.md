# Video Learning With NeoCortexApi:

## 1. Motivation:
This work "Video Learning with HTM CLA" introduces videos data into the Cortical Learning Algorithm in [NeoCortex Api](https://github.com/ddobric/neocortexapi).

## 2. Overview:
In this Experience, Videos are learned as sequences of Frames.  
For example of Sequence Learning, see [SequenceLearning.cs](https://github.com/ddobric/neocortexapi/tree/master/source/Samples/NeoCortexApiSample).  

Input Videos are currently generated from python scripts, using OpenCV2. See [DataGeneration](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/DataGeneration) for manual on usage and modification.  

The Reading of Videos are enabled by [VideoLibrary](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary), written for this project using OpenCV2. This library requires nuget package [Emgu.CV](https://www.nuget.org/packages/Emgu.CV/), [Emgu.CV.Bitmap](https://www.nuget.org/packages/Emgu.CV.Bitmap/), [Emgu.CV.runtimes.windows](https://www.nuget.org/packages/Emgu.CV.runtime.windows/) version > 4.5.3.  

Learning process include: 
1. reading videos.
2. convert videos to Lists of bitarrays.
3. Spatial Pooler Learning with Homeostatic Plasticity Controller until reaching stable state.
4. Learning with Spatial pooler and Temporal memory, conditional exit.
5. Interactive testing section, output video from frame input.
## 3. Data Generation:
The current encoding mechanism of the frame employs the convert of each pixels into an 
## 4. Videos Reading:
Video Library is seperated into 3 sub library:
- [**VideoSet**](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/VideoSet.cs):  
Group of multiple **NVideo** put under the same label.  
Read multiple video under a folder, use the folder name as label.  
Get the stored **NFrame** from a given framekey.  
Create converted videos as output from read training set.

- [**NVideo**](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/NVideo.cs):  
Represent a video, has multiple **NFrame**.  
Read a video in different frame rate. (only equal/lower framerates are possible)

- [**NFrame**](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/NFrame.cs):  
represent a frame, has converted bitarray from the frame pixel reading.
Can convert a bit array to Bitmap.  

The current color encoding of each frame when reading videos [includes 3 mode](https://github.com/ddobric/neocortexapi/blob/027ead7a860f1ae115c56583035fc8fe21b97c83/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/NFrame.cs#L12):  
1. ``BLACKWHITE``: binarized frame by reading luminance value:  
```csharp
double luminance = (3 * pixel.R + pixel.B + 4 * pixel.G)>>3; 
```  
2. ``BINARIZEDRGB``: ref [ImageBinarizer](https://github.com/UniversityOfAppliedSciencesFrankfurt/imagebinarizer) binarized each color channel in Red, Green and Blue:
```csharp
imageBinary.AddRange(new List<int>() { (pixel.R > 255 / 2) ? 1 : 0, (pixel.G > 255 / 2) ? 1 : 0, (pixel.B > 255 / 2) ? 1 : 0 });
```
3. ``PURE``: encode all 8 bits for each channel in RGB adding 3 channel x 8bits for each pixel:

```csharp
imageBinary.AddRange(ColorChannelToBinList(pixel.R));
imageBinary.AddRange(ColorChannelToBinList(pixel.G));
imageBinary.AddRange(ColorChannelToBinList(pixel.B));
```
These can be further added to the [ImageBinarizer](https://github.com/UniversityOfAppliedSciencesFrankfurt/imagebinarizer) as custom encoder.  
The current experiment focus on the ``BLACKWHITE`` colorMode, due to its low consumption in memory, which result in faster runtime of experiment.
The lowest dimension of the video is 18*18, test has revealed lower dimension result in codec error of Emgu.CV with auto config mode -1 in code:  
```csharp
VideoWriter videoWriter = new($"{videoOutputPath}.mp4", -1, (int)frameRate, dimension, isColor)
// Whereas dimension is dimension = new Size(18,18);
// -1 means choosing an codec automatically, only on windows
```

## 4. Learning Process:
After reading the Videos into VideoSets, the learning begins.


