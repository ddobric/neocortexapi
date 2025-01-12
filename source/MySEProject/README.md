# Project Title: ML 24/25-01 Investigate Image Reconstruction by using Classifiers

## Problem Statement:
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

## Introduction:
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

## Image Encoder:
The ImageEncoder plays a crucial role in preparing image data for processing within Hierarchical
Temporal Memory (HTM) systems by converting raw image inputs into binary representations
compatible with HTM&#39;s Sparse Distributed Representations (SDRs). Based on the ImageBinarizer
NuGet package, the ImageEncoder encodes pixel intensity or feature information into a format that
preserves essential patterns while reducing redundancy. This encoding ensures that similar images
produce similar SDRs, a key characteristic that enables effective learning and pattern recognition in
HTM systems. By preprocessing images into this sparse binary format, the ImageEncoder bridges the
gap between raw image data and the HTM&#39;s Spatial Pooler, making it a foundational component for
image-based experiments, such as learning spatial patterns or regenerating inputs from SDRs.

## Sparse Distributed Representations (SDR):
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
