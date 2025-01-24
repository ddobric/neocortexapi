# Project Title: ML 24/25-01 Investigate Image Reconstruction by using Classifiers

## Table of Contents

- [Problem Statement](#problem-statement)
- [Introduction](#introduction)
- [Image Encoder](#image-encoder)
- [Sparse Distributed Representations (SDR)](#sparse-distributed-representations-sdr)

### Problem Statement:
This project aims to explore the role of classifiers in Hierarchical Temporal Memory (HTM) systems,
focusing on their ability to associate input patterns with meaningful predictions and reconstruct
original inputs from Sparse Distributed Representations (SDRs). By investigating and comparing two
existing classifiers, HtmClassifier and KNN, the project seeks to evaluate their functionality,
performance, and differences. Inspired by the SpatialLearning experiment, a new experiment will be
implemented to regenerate input images from SDRs produced by the Spatial Pooler (SP), leveraging
the IClassifier interface for learning and prediction. The experiment will use the ImageEncoder to
process images, reconstruct inputs via classifiers, and compare them with the originals using
similarity measures. Results will be illustrated with diagrams, analysed quantitatively, and discussed,
providing insights into the reconstruction capabilities of classifiers in HTM systems and their
practical implications.

### Introduction:
This project explores the integration and application of classifiers within the Hierarchical Temporal Memory (HTM) framework to regenerate input data from Sparse Distributed Representations (SDRs). The core goal is to understand the role of classifiers in reverse encoding, where the learned SDR representations are used to reconstruct the original input. Through this process, we aim to analyse the behaviour and performance of two existing classifiers—HtmClassifier and KNN—and implement a new experiment that leverages their capabilities. The two classifiers under study in this project—HtmClassifier and KNN—serve as foundational implementations. The HtmClassifier leverages the principles of temporal memory within HTM, while the KNN classifier employs a distance-based approach to classify SDRs based on nearest neighbours. Through this investigation, the project bridges the gap between abstract HTM theories and practical applications, contributing to advancements in intelligent systems and neural computation.

### Image Encoder:
The ImageEncoder plays a crucial role in preparing image data for processing within Hierarchical
Temporal Memory (HTM) systems by converting raw image inputs into binary representations
compatible with HTM's Sparse Distributed Representations (SDRs). Based on the ImageBinarizer
NuGet package, the ImageEncoder encodes pixel intensity or feature information into a format that
preserves essential patterns while reducing redundancy. This encoding ensures that similar images
produce similar SDRs, a key characteristic that enables effective learning and pattern recognition in
HTM systems. By preprocessing images into this sparse binary format, the ImageEncoder bridges the
gap between raw image data and the HTM's Spatial Pooler, making it a foundational component for
image-based experiments, such as learning spatial patterns or regenerating inputs from SDRs.

### Sparse Distributed Representations (SDR):
Sparse Distributed Representations (SDRs) are analogous to how the human brain encodes
information. Just as neurons in the brain fire in sparse patterns, with only a small fraction of neurons
active at any time, SDRs use binary vectors where a small percentage of bits are active (1s) while the
rest are inactive (0s). In the brain, these sparse activations ensure energy efficiency and robustness,
as the overlap in neural firing patterns helps identify similar stimuli. Similarly, SDRs are sparse to
reduce computational complexity and distributed to make the system resilient to noise or
corruption. Technically, SDRs preserve key properties of input data, such as similarity and
distinctiveness, through overlapping active bits for similar inputs and distinct patterns for dissimilar
inputs. This allows HTM systems to efficiently encode, recognize, and generalize patterns, just as the
brain does when processing sensory input. SDRs form the foundation for all processing stages in
HTM, including spatial pooling and temporal memory, providing a biologically plausible and
computationally robust framework for learning and prediction.

### Spatial Pooler (SP):
The Spatial Pooler is a fundamental component of Hierarchical Temporal Memory (HTM) systems, transforming raw input data into Sparse Distributed Representations (SDRs). Its primary function is to encode the input while ensuring key properties such as sparsity and similarity preservation. Sparsity ensures that only a small percentage of bits in the SDR are active, which improves computational efficiency and reduces noise sensitivity. Similarity preservation means that inputs with similar patterns produce SDRs with overlapping active bits, enabling the system to recognize related patterns effectively. The Spatial Pooler achieves this through competition among columns of cells, where each column competes to represent specific input features, guided by synaptic connections that adapt over time. This adaptation allows the Spatial Pooler to learn the statistical structure of the input space, making it robust to noise and capable of generalizing from limited data. As a result, the Spatial Pooler provides the foundation for further processing, such as temporal learning and classification, in HTM systems.

### K-Nearest Neighbors (KNN):
The K-Nearest Neighbours (KNN) classifier is a simple, non-parametric algorithm used for classification and regression tasks. It stores all the labeled training data and classifies new data points based on their similarity to the closest training samples. When a new input is provided, the algorithm computes its distance (commonly using Euclidean distance) from all training points, identifies the "k" nearest neighbors, and assigns the most common label among those neighbours to the input. In the context of this project, KNN could serve as a baseline classifier or a comparative model for evaluating SDR representations. When provided with an SDR or a derived feature vector, KNN computes distances (e.g., Euclidean) to its "k" closest neighbors and predicts the most frequent label among them. This method can be useful in this project to classify SDRs or reconstructed patterns, allowing comparisons between HTM's ability to generalize patterns and KNN's reliance on proximity and similarity. While KNN is straightforward and effective for small-scale problems, it lacks the adaptive learning and biological inspiration of HTM, making it less dynamic for processing evolving data streams.
