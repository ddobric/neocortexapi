# Problem Statement:
The project aims to implement the concept of classifiers and their application within the Hierarchical Temporal Memory (HTM) framework. Building on the Spatial Learning experiment, which demonstrates learning, this project will introduce a new experiment focused on reconstructing input from the generated Sparse Distributed Representations (SDRs) using HTM/KNN classifiers and comparing the output with the original input.

![SprintPlan](https://github.com/user-attachments/assets/cfecfff9-f1c3-4248-b684-3c4e5db0c5a8)

# Project Workflow:

The experiment aims to extend the existing **SpatialLearning** experiment by adding functionality to reconstruct the input images from the SDR using classifiers. The project follows this methodology:

### A. Input Preparation:
1. Loading multiple images from a specified folder as input data.
2. Encoding the images into SDRs using the ImageBinarizer, which converts pixel data into binarized representations suitable for HTM processing.

### B. HTM Processing:
1. Passing the binarized image through the Spatial Pooler (SP) to generate stable SDRs that represent the essential spatial patterns in the input, which represents the learning phase.

### C.Reconstruction via Classifiers:
1. Implement the functionality of HTM classifier/KNN classifier , where we pass the stable SDRs to generate the reconstructed input.

### D. Evaluation:
1. Comparing the reconstructed inputs with the original images using similarity metrics , generate similarity scores for both classifiers.
2. Visualizing the differences between input and reconstructed images to assess the effectiveness of the classifiers.
3. Create similarity graphs to compare original input with outputs from HTM and KNN classifiers using visualization tools like Matplotlib or OxyPlot.
   
![Project Workflow](https://github.com/user-attachments/assets/ac9ab830-e9d1-4a6f-9ef1-912b37018e68)

## Expected Outcomes:

 1.Generate similarity scores between original images and reconstructed inputs for HTM and KNN classifiers.  
 2.Visualize differences between original images and reconstructed inputs to evaluate classifier accuracy.  
 3.Compare the performance of HTM and KNN classifiers in processing stable SDRs.  
 4.Evaluate the effectiveness of HTM and KNN classifiers in handling spatial pattern recognition.
