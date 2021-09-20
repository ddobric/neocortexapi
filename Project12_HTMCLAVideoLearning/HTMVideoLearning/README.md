# Video Learning With NeoCortexApi:
Module: Project 12  
Instructor: Damir Dobric, Proffessor Andreas Pech.  
Student:  
Toan Thanh Truong, Mtr. 1185050 Major: IT Gbr. 23.02.1997  
_this readme serves as the submitted projectreport for the registered project Video Learning with HTM_
## 1. Motivation:
This work "Video Learning with HTM CLA" introduces videos data into the Cortical Learning Algorithm in [NeoCortex Api](https://github.com/ddobric/neocortexapi).  
Experiment in current work involves using Temporal Memory to learn binary representation of videos (sequence of bitarrays - each bitarray represents 1 frame).  
Afterwards the result of the learning is tested by giving the trained model an abitrary image, the model then tries to recreate a video with proceeding frame after the input frame.

## 2. Overview:
In this experiment, Videos are learned as sequences of Frames. The link to the project code can be found in [VideoLearning.cs](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/HTMVideoLearning/VideoLearning.cs). An overall view of the experiment can be found in the [Projet Folder](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning).  

This project references Sequence Learning sample, see [SequenceLearning.cs](https://github.com/ddobric/neocortexapi/tree/master/source/Samples/NeoCortexApiSample).  

Input Videos are currently generated from python scripts, using OpenCV2. See [DataGeneration](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/DataGeneration) for manual on usage and modification.  

The Reading of Videos are enabled by [VideoLibrary](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary), written for this project using OpenCV2. This library requires nuget package [Emgu.CV](https://www.nuget.org/packages/Emgu.CV/), [Emgu.CV.Bitmap](https://www.nuget.org/packages/Emgu.CV.Bitmap/), [Emgu.CV.runtimes.windows](https://www.nuget.org/packages/Emgu.CV.runtime.windows/) version > 4.5.3.  

Learning process include: 
1. reading videos.
2. convert videos to Lists of bitarrays.
3. Spatial Pooler Learning with Homeostatic Plasticity Controller until reaching stable state.
4. Learning with Spatial pooler and Temporal memory, conditional exit.
5. Interactive testing section, output video from frame input.
## 3. Data Generation:
The current encoding mechanism of the frame employs the convert of each pixels into an part in the input bit array. This input bit array is used by the model for training.  
There are currently 3 training set:
- SmallTrainingSet: has 3 video, 1 foreach label in {circle rectangle triangle}.    
- TrainingVideos: has more video, intended for training in `PURE` colorMode
- oneVideoTrainingSet
The current most used set for training and debugging is SmallTrainingSet.  

## 4. Videos Reading:
For more examples on how to use the Library, see [VideoLibraryTest](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibraryTest).  
Video Library is seperated into 3 sub classes:
- [**VideoSet**](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/VideoSet.cs):  
Group of multiple **NVideo** put under the same label.  
Read multiple video under a folder, use the folder name as label.  
Get the stored **NFrame** from a given framekey.  
Create converted videos as output from read training set.  
There is a TestClass

- [**NVideo**](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/NVideo.cs):  
Represent a video, has multiple **NFrame**.  
Read a video in different frame rate. (only equal/lower framerates are possible)

- [**NFrame**](https://github.com/ddobric/neocortexapi/blob/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/VideoLibrary/NFrame.cs):  
represent a frame, has converted bitarray from the frame pixel reading.
Can convert a bit array to Bitmap.  
Also includes Framkey parameters, which is used to index the frame and learning with [HTMClassifier](https://github.com/ddobric/neocortexapi/blob/master/source/NeoCortexApi/Classifiers/HtmClassifier.cs).  
**Framkey = (label)\_(VideoName)\_(index)**  e.g. circle_vd1_03  
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
**NOTE:**  
The current implementation of VideoLibrary saves all training data into a List of VideoSet, which contains all video information and their contents. For further scaling of the training set. It would be better to only store the index, where to access the video from the training data. This way the data would only be access when it is indexed and save memory for other processes.
## 4. Learning Process:
Current HTM Configuration:
```csharp
private static HtmConfig GetHTM(int[] inputBits, int[] numColumns)
{
    HtmConfig htm = new(inputBits, numColumns)
    {
        Random = new ThreadSafeRandom(42),

        CellsPerColumn = 30,
        GlobalInhibition = true,
        //LocalAreaDensity = -1,
        NumActiveColumnsPerInhArea = 0.02 * numColumns[0],
        PotentialRadius = (int)(0.15 * inputBits[0]),
        //InhibitionRadius = 15,

        MaxBoost = 10.0,
        //DutyCyclePeriod = 25,
        //MinPctOverlapDutyCycles = 0.75,
        MaxSynapsesPerSegment = (int)(0.02 * numColumns[0]),

        //ActivationThreshold = 15,
        //ConnectedPermanence = 0.5,

        // Learning is slower than forgetting in this case.
        //PermanenceDecrement = 0.15,
        //PermanenceIncrement = 0.15,

        // Used by punishing of segments.
    };
    return htm;
}
```
Running the experiment Run1 and Run2 first prompt the user to input a training folder path. There are currently 3 sample training data sets, drag the folder to the command window to insert its path to the prompt. Hit `Enter` and the Video reading begins.

After reading the Videos into VideoSets, the learning begins.
### 1. SP Learning with HomeoStatic Plasticity Controller (HPA):
This first section of learning use Homeostatic Plasticity Controller:
```csharp
HomeostaticPlasticityController hpa = new(mem, 30 * 150*3, (isStable, numPatterns, actColAvg, seenInputs) =>
{
    if (isStable)
        // Event should be fired when entering the stable state.
        Console.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
    else
        // Ideal SP should never enter unstable state after stable state.
        Console.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
        // We are not learning in instable state.
        learn = isInStableState = isStable;
        // Clear all learned patterns in the classifier.
        cls.ClearState();
}, numOfCyclesToWaitOnChange: 50);
```
The average number of cycles required for the "smallTrainingSet" is 270 cycles, "training with Spatial Pooler only" in this experiment used a while loop until the model reach true in parameter **IsInStableState**.
### 2. SP+TM Learning of frame sequences in the video set:
HPA will be triggered with the Compute method of the current layer. One problem during the learning is that even after successfull enter to stable state in Learning only with SP, the model can get unstable again after learning the first video or the second video in SP+TM stage. Thus:
```csharp
//Iteration:
foreach (VideoSet vd in videoData)
    {
    foreach (NVideo nv in vd.nVideoList)
        {
            // LOOP1
            // After finished learning in this cycle and move to the next video
            // The model somtimes becomes unstable and trigger cls.ClearState in HPA, making the HTMClassifier forget all what it has learned.  
            // Specificaly It clears the m_ActiveMap2 
            // To cope with this problem and faster debug the learning process, some time the experiment comment out the cls.ClearState() in HPA
        for (int i = 0; i < maxCycles; i++)
        }
    }
``` 
may be changed to:  
```csharp
//Iteration:
for (int i = 0; i < maxCycles; i++)
    {
        foreach (VideoSet vd in videoData)
        {
            foreach (NVideo nv in vd.nVideoList)
            {
                // LOOP2
                // This ensure the spreading learning of all the frames in different videos  
                // this keep cls.ClearState() in hpa and successully run to the end means that Learning process doesn't end in unstable state.
            }
        }
    }
``` 
For the current 2 tests:  
**_Run1: "SP only" runs LOOP2 || SP+TM runs LOOP1_**  
Key to learn with HTMClassifier: **FrameKey**, e.g.  rectangle_vd5_0, triangle_vd4_18, circle_vd1_9.  
Condition to get out from loop:
- Accuracy is calulated from prediction of all videos
- After run on each video, a loop is used to calculate the Predicted nextFrame of each frame in the video, the last frame doesn't have next predicted cells by usage of `tm.Reset(mem)` as indentification for end of video.  
```csharp
// correctlyPredictedFrame increase by 1 when the next FrameKey is in the Pool of n possibilities calculated from HTMClassifier cls.  
var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;
var nextFramePossibilities = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 1);
// e.g. if current frame is rectangle_vd5_0 and rectangle_vd5_1 is in nextFramePossibilities, then correctlyPredictedFrame for this Video increase by 1.  

double accuracy = correctlyPredictedFrame / ((double)trainingVideo.Count-1);
// The accuracy of each video add up to the final average accuracy of the VideoSet
videoAccuracy.Add(accuracy);
...
double currentSetAccuracy = videoAccuracy.Average();
// The accuracy of each VideoSet add up to the total cycleAccurary
setAccuracy.Add(currentSetAccuracy);
...
cycleAccuracy = setAccuracy.Average();
// The Learning is consider success when cycleAccuracy exceed 90% and stay the same more than 40 times
if(stableAccuracyCount >= 40 && cycleAccuracy> 0.9)
// The result is saved in Run1ExperimentOutput/TEST/saturatedAccuracyLog_Run1.txt.  
// In case the Experiment reach maxCycle instead of end condition, the log will be saved under Run1ExperimentOutput/TEST/MaxCycleReached.txt
``` 
After finishing the user will be prompted to input a picture path.  
The trained layer will use this image to try recreate the video it has learned from the training data.  
- The image can be drag into the command window and press enter to confirm input. The model use the input frame to predict the next frame, then continue the process with the output frame if there are still predicted cells from calculation. For the first prediction HTMClassifier takes at most 5 possibilities of the next frame from the input.  
- In case there are at least 1 frame, the codecs will appears and the green lines indicate the next predicted frame from the memory by HTMClassifier. 
- The output video can be found under Run1ExperimentOutput/TEST/ with the folder name (Predicted From "Image name").  
- Usually in this Test, The input image are chosen from the Directory Run1Experiment/converted/(label)/(videoName)/(FrameKey) for easier check if the trained model predict the correct next frame.  


**RESULT**  
- Due to the conversion of the input picture to fit the current model the input is also processed by VideoLibrary to the dimension of the training model. The scaled input image can also be found in the Run1ExperimentOutput/TEST/Predicted from (image name)/.  
- Ideally, a sequence of half the length of the video would regards this experiment as a success. Unfortunately, runs result in sequence of 1-5 frames after the input frame. 
- It is observed that the triangle set - the last training set in small training set has the best sequence generation with sometime up to 15 frames. 
- In some case frame that overlap each other e.g. the triangle at the same place of the circle may result in shape change but correct translation.  
- There are also cases where next frame calculated from input frame resulted in a loop sequence with 1,2,3 frame, these are bad connection and can continue to infinity.  
A max number of predicted frames was set to 42 to avoid running to infinity. The max number of frame in the current training set is 3*12 = 36 frames.
- This experiment has a very long run time because it covers all the frame in each cycle due to usage of LOOP1.

**_Run2: "SP only" runs LOOP2 || SP+TM runs LOOP2_**  
_This Run is inspired by the experiment [SequenceLearning_Duy](https://github.com/perfectaccountname/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/SequenceLearning_DUY.cs), here the video would be the double sequence._  
Key to learn with HTMClassifier: **FrameKey**-**FrameKey**-...-**FrameKey**, e.g.  rectangle_vd5_0-rectangle_vd5_1-...rectangle_vd5_29.  

Run2 running used the following parameters:
1. previousInputs: the key used for learning with HTMClassifier(only at full length)
2. lastPredictedValue: the predicted value from the last cycle
3. maxPrevInputs = video length (in frame) -1

Input for the model is bit array created from encoding the current active frame.  
When starting the training for one video, the early cycle will begin to build up a list of all Framekey in time order.
```csharp
previousInputs.Add(currentFrame.FrameKey);
if (previousInputs.Count > (maxPrevInputs + 1))
    previousInputs.RemoveAt(0);

// In the pretrained SP with HPC, the TM will quickly learn cells for patterns
// In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
// Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
// HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
// memorized, it will match as the first one.
if (previousInputs.Count < maxPrevInputs)
    continue;
```
when previousInputs reach the same length of the video, the learning with HTMClassifier begins:  
```csharp
string key = GetKey(previousInputs);
List<Cell> actCells;
if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
{
    actCells = lyrOut.ActiveCells;
}
else
{
    actCells = lyrOut.WinnerCells;
}
cls.Learn(key, actCells.ToArray());
```
the key used for learning is generated from the FrameKey List previousInputs of the current Video.  
From this way of generating the key, the current frame bitArray Collums output will be learn with a key in which FrameKey sequence run from the next FrameKey from the current input to the end of the video and back to the current FrameKey.  
e.g. current frame: rectangle_vd5_4  
Video has 8 frames, then the generated key to be learned along with the active columns of current frame would be:  
_rectangle_vd5_5-rectangle_vd5_6-rectangle_vd5_7-rectangle_vd5_0-rectangle_vd5_1-rectangle_vd5_2-rectangle_vd5_3-rectangle_vd5_4_  

By Learning this way one frame info is associated with the information of the whole frame sequence (Video). This compared to Run1 reduce the error compared with predicting the frame one by one, the output video can also be recalled in full length. 

Condition to get out of the loop:  
- By using LOOP2, Run2 learn each video respectively. The training of one video ends after the model reach acuracy > 80% and stay the same for 50 cycles or reaching maxCycles. The models then trains with the next video until there is no video left.  
```csharp
if(accuracy == lastCycleAccuracy)
{
    // The learning may result in saturated accuracy
    // Unable to learn to higher accuracy, Exit
    saturatedAccuracyCount += 1;
    if (saturatedAccuracyCount >= 50 && lastCycleAccuracy>80)
        {
            ...
        }
}
```
In case the end condition is reached, a saturatedAccuracyLog will appear in Run2ExperimentOutput/TEST/.
After finishing the user will be prompted to input a picture path.  
The trained layer will use this image to try recreate the video it has learned from the training data.  
- The image can be drag into the command window and press enter to confirm input. The model use the input frame to predict the key.  
- if there are predicted cells from compute, HTMClassifier takes at most 5 possibilities (can be changed in code) of the predicted key.  
- In case there are at least 1 key, the codecs will appears and the green lines indicate the next predicted key from the memory by HTMClassifier. 
- The output video can be found under Run2ExperimentOutput/TEST/ with the folder name (Predicted From "Image name").  
- Usually in this Test, The input image are chosen from the Directory Run1Experiment/converted/(label)/(videoName)/(FrameKey) for easier check if the trained model predict the correct next frame.  

**RESULT**
- Due to the conversion of the input picture to fit the current model the input is also processed by VideoLibrary to the dimension of the training model. The scaled input image can also be found in the Run1ExperimentOutput/TEST/Predicted from (image name)/. 
- The log files after learning each video are also recorded as saturatedAccuracyLog_(label)_(video name) in the TEST/ directory.
- The output Video has full length of the video.
- The prediction sometimes forget the first video and enter unstable state again. This was mentioned in HPA above. The current way to cope with the phenomenom is comment out `cls.ClearState()` in declaration of HPA.
- Prediction ends with rather high accuracy 89-93% recorded after learning of a video.  
- the output video will run from the predicted next frame from input frame to the end of the video then back to the input frame.  

For an review on output folder TEST of both the Run after the learning one can refer to [SampleExperimentOutputTEST](https://github.com/ddobric/neocortexapi/tree/SequenceLearning_ToanTruong/Project12_HTMCLAVideoLearning/HTMVideoLearning/SampleExperimentOutputTEST).


