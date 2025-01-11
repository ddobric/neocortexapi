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



