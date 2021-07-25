# Video Learning using HTMCLA
## Project Description:
This project examine the ability of HTMCLA in processing Videos.

## Current State of the Project:
Reading of multiple videos in multiple folders and use the folders' names as Labels. After building the project, the TrainingVideos\ folder has to be copied to the bin\ directory of VideoLearningUnitTest to execute the VideoLearningExperiment test.

 Project12_HTMCLAVideoLearning\HTMVideoLearning\VideoLearningUnitTest\bin\Debug\net5.0\TrainingVideos\

 ![Example directory layout of TrainingVideos](TrainingVideos_DirectoryStructure.png?raw=true)

The current active test for the project is VideoLearningExperiment,
![Test Explorer's view after successful build](pictures/TestExplorer.png?raw=true)  

The class VideoSet provides methods to read the videos into InputBit array  
> InputBit array: binary array.  
> A VideoSet consists of many Video and a Label,  
> Each Video is a List of InputBitArray,  
> Each InputBit Array represents a frame.  

To reduce the data generated in 

