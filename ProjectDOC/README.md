# Video Learning using HTMCLA
## Project Description:
This project examine the ability of HTMCLA in processing Videos.

## Current State of the Project:
Reading of multiple videos in multiple folders and use the folders' names as Labels. After building the project, the TrainingVideos\ folder has to be copied to the bin\ directory of VideoLearningUnitTest to execute the VideoLearningExperiment test.

 Project12_HTMCLAVideoLearning\HTMVideoLearning\VideoLearningUnitTest\bin\Debug\net5.0\TrainingVideos\

 ![Example directory layout of TrainingVideos](pictures/TrainingVideos_DirectoryStructure.png?raw=true)

The current active test for the project is VideoLearningExperiment,
![Test Explorer's view after successful build](pictures/TestExplorer.png?raw=true)  

The class VideoSet provides methods to read the videos into InputBit array  
> InputBit array: binary array.  
> A VideoSet consists of many Video and a Label,  
> Each Video is a List of InputBitArray,  
> Each InputBit Array represents a frame.  

## Current Scope:
1. Training Data:  
Short Video (100 - 1000 frames) that is representative of the Label it has  
2. Testing Data:
Long Video (10000 - 50000 frames) which holds multiple video parts from different learned labels.

CURRENT GOAL:
Streaming while predicting a video  
For example: tells wether we are seeing a cat or a dog while streaming a video.

## Current Obstacles:

### 1. Homeostatic plasticity controller (HPC):
Due to the much similarity between 2 continous frame, reading all of the frames in a Video cause the HPC to figures out lesser set of patterns than the number of inputs.  
This causes:  
> Assert.IsTrue(numPatterns == numInputs);
to throw an error.

**Current Approach:** read video in a lower FrameRate to increse the difference between 2 respective frames.

### 2. Encoding of the Image:
when a video is encoded, all pixel is encoded into bit array, which increase the number of bit stored.  
e.g. 30x40 8 bit color frame create a length 3600 bit array
  


**Current Approach:** option to reduce the frame size, and the color resolution of the frame.  
**Question**
Current nuget GleamTech VideoUltimate for reading the video cannot be used in commercial. Can this be an issue ?  
**Question**  
 the widespread nature of the image will create InputBitArray with various number of on-bits  
 e.g. pitch black frame (all bits on) will activate all collums. Can this be a problem for the learning ?

### 3. Learning mechanics of Temporal memory:

**Question**  
How to predict backward an image from an SDR ?  
How to learn each videos respectively with a label ?
