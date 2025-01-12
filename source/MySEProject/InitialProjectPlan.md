# Problem Statement:
This project aims to implement the concept of classifier and its application in Hierarchical Temporal Memory (HTM). Building on the SpatialLearning experiment, which demonstrates learning, the project will implement a new experiment focused on regenerating input from the generated Sparse Distributed Representation (SDR).

![ProjectPipeline](https://github.com/user-attachments/assets/ddb45df5-5408-47b4-a976-169ac8ebe429)

# Project Workflow:

The experiment aims to extend the existing **SpatialLearning** experiment by adding functionality to reconstruct the input images from the SDR using classifiers. The project follows this methodology:

### A. Input Preparation:
1. Loading multiple images from a specified folder as input data.
2. Encoding the images into SDRs using the ImageEncoder, which converts pixel data into binarized representations suitable for HTM processing.

### B. HTM Processing:
1. Passing the encoded SDRs through the Spatial Pooler (SP) to generate stable SDRs that represent the essential spatial patterns in the input, which represents the learning phase.

### C. Classification:
1. Utilizing the HtmClassifier and KNN implementations to learn the mappings from SDRs to input data during the learning phase.
2. During the prediction phase, using these classifiers to reconstruct the input images from the SDRs.

### D. Evaluation:
1. Comparing the reconstructed inputs with the original images using similarity metrics provided in the acceptance.
2. Visualizing the differences between input and reconstructed images to assess the effectiveness of the classifiers.
![Project Workflow](https://github.com/user-attachments/assets/ac9ab830-e9d1-4a6f-9ef1-912b37018e68)

# Expected Outcomes:


This project will provide insights into the effectiveness of classifiers in the HTM framework, particularly in reverse encoding scenarios. By reconstructing input images from SDRs and comparing reconstructed images with original images, we aim to:

- Understand how well HTM can generalize and reconstruct input data.
- Assess the strengths and weaknesses of classifier implementations.
