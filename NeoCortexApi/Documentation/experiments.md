# Experiments

## Investigation of sequence learning of SP/TM layers

### Authors

Ghulam Mustafa,
Muhammad Mubashir Ali Khan,
Abdul Samad,
Treesa Maria Thomas,
Diab elmehdi

### Project reference

ML19.20 - 5.4  
Issue 85

### Abstract

#### **Cells Per Column Experiment**

The ability to recognize and predict temporal sequences of sensory inputs is vital for survival in natural environments. Based on the number of known properties of cortical neurons, hierarchical temporal memory (HTM) has been recently proposed as a theoretical framework for sequence learning in the neo cortex. In this paper, we analyze the sequence learning behavior of spatial pooler and temporal memory layer in dependence on learning parameter-Cells per Column. We have demonstrated how changing the number of cells per column improvised the learning process for different input sequences. The results show the proposed model is able to learn a sequence of data by keeping the number of cells beyond a certain value depending upon the complexity of input sequence.

[Download student paper here](./Experiments/ML-19-20_20-5.4_CellsPerColumnExperiment_Paper.pdf)

#### **HTM Sparsity**

It is necessary for survival in natural environment to be able to identify and predict temporal sequence of sensory input. Based on numerous common properties of the cortical neurons, the theoretical framework for sequence learning in the neo cortex recently proposed hierarchical temporal memory (HTM). In this paper, we analyze the sequence learning behavior of spatial pooler and temporal memory layer in dependence on HTM Sparsity. We found the ideal value of HTM Sparsity that will have optimal learning for the given input sequence. We also showed the effect of changing Width and Input Bits on learning such that the value of HTM Sparsity remains constant. We devised a relation between HTM Sparsity and max for optimal learning of the given sequence.

[Download student paper here](./Experiments/ML-19-20_20-5.4_HtmSparsityExperiments_Paper.pdf)

#### **Parameter Change Experiment**

Hierarchical Temporal Memory (HTM) is based on the supposition that the world has a structure and is therefore predictable. The development of HTM for Artificial Neural Networks has led to an advancement in the field of artificial intelligence and leading the computing intelligence to a new age. In this paper, we studied various learning parameters like Width(W), Input Bits(N), Max,Min values and the number of columns, that majorly contribute to optimize the sequence learning behavior of spatial pooler and temporal memory layer. We also performed experiment to obtain stability of Spatial Pooler output by tuning the boost and duty cycles. We evaluated each of these parameters based on the theoretical and practical framework and summarized the results in graphical diagrams.

[Download student paper here](./Experiments/ML-19-20_20-5.4_ParameterChangeExperiment_Paper.pdf)

## Performance Spatial Pooler between Global and Local Inhibition

### Authors

Tran Quang Trung,
Nguyen Thanh Quang

### Project reference

ML19/20 - 5.7  
Issue 89

### Abstract

Each region in the cortex receives input through millions of axons from sensory organs and from other cortical regions. It remains a mystery how cortical neurons learn to form specific connections from this large number of unlabeled inputs in order to support further computations. Hierarchical temporal memory (HTM) provides a theoretical framework for understanding the computational principles in the neo-cortex. HTM spatial pooler was created to model how neurons learn feed forward connections. The spatial pooler method is converting the arbitrary binary input patterns into sparse distributed representations (SDRs) using competitive Hebbian learning’s rules and homeostasis excitability control mechanisms. In this paper, one of the Spatial Pooler’s key parameters, which is the “inhibition”, will be described. The main part is to show the differences between the “local” and “global” inhibition and how and what kind of affects they contribute to the process to the Spatial Pooler learning algorithm.

[Download student paper here](./Experiments/ML-19-20_20-5.7_PerformanceSpatialPooler-between-Global-and-Local-Inhibition.pdf)

## Investigation of Hierarchical Temporal Memory Spatial Pooler's Noise Robustness against Gaussian noise

### Author

Sang Nguyen,
Duy Nguyen

### Project reference

ML19/20 - 5.12  
Issue 126

### Abstract

The Thousand Brains Theory of Intelligence is a new and rising approach to understand human intelligence. The theory attempts to explain the fundamental principles behind human intelligence through many discovered biological evidences and logical reasoning. This theory lays the foundation for Hierarchical Temporal Memory (HTM) - an AI framework, which has many applications in practice. In this paper’s HTM model, building block of a basic HTM structure comprises of an Encoder, a Spatial Pooler and a Temporal Memory. This fundamental component has two prominent features: noise robustness and prediction. The Spatial Pooler is mostly responsible for noise handling function of the completestructure. This paper provides some experimental data and comments about the reliability of the Spatial Pooler’s noise handling function. Specifically, the level of noise robustness is measured by the similarity between outputs of the Spatial Pooler when it takes the original data set and then the additiveGaussian noisy data sets as inputs, provided that it is only trained with the original data set.

[Download student paper here](./Experiments/ML-19-20_20-5.12_SpatialPooler_NoiseRobustness.pdf)

## Validate Memorizing capabilities of SpatialPooler

### Authors

Dipanjan Saha dipanjan.saha@stud.fra-uas.de,  
Pradosh Kumar Panda ppanda@stud.fra-uas.de,  
Rina Yadav rina.yadav@stud.fra-uas.de

### Project reference

ML19/20 - 5.10  
Issue 145

### Abstract

The main objective of the project is to describe memorizing capabilites as the ability to recall last impression of input sequence. It utilizes Hierarchical temporal memory (HTM) to generate SDR’s. We have described how input vectors are generated for different learning sequences and shown output for the learning sequences using encoder. We have discussed different test cases to verify the memorizing capabilities of the Spatial Pooler using different learning sequences. To check the similarities of this sequences we have used hamming distance and overlap plotting.

[Download student paper here](./Experiments/ML-19-20_20-5.10_ValdatingMemorizingCapabilitesOfSpatialPooler.pdf)

## ML19/20-5.2. Improving of implementation of the Scalar encoder in HTM

### Authors

### Project reference

ML19/20 - 5.2
Issue 156

### Abstract

Scalar Encoder is one of the encoding techniques and is a part of Hierarchical Temporal Memory (HTM). HTM is a machine intelligence technology which is trying to imitate the process and architecture of neocortex. The main purpose for scalar encoder is to encode numeric or floating-point value into an array of bits, where the output has 0’s with an adjacent block of 1’s. The location of the block of 1’s varies continuously depending on the input value.

## Sequence Learning - Music Notes Experiment

### Author

Damir Dobric
2019/2020

### Abstract

To demonstrate learning of sequences, I have originally developed an experiment to learn the song. In this experiment the input is defined a set of music notes.
Every music note is represented as a scalar value, which appear in the sequence of notes. For example, notes C, D, E, F, G and H can be associated with the scalar values: C-0, D-1, E-2, F-3, G-4, H-5. By following that rule notes of some has been taken. In the very first experiment the song _twinkle, twinkle little star_ was used in the experiment: [here] (https://www.bethsnotesplus.com/2013/08/twinkle-twinkle-little-star.html).
Over time, the experiment has grown, but we have kept the original name '_Music Notes Experiment_'. In this experiment various outputs are generated, which trace the state of active columns and active cells during the learning process. Today, we use this experiment to learn how HTM learns sequences.

## On the Relationship Between Input Sparsity and Noise Robustness in SP (Paper)

### Author

Damir Dobric, Andreas Pech, Bogdan Ghita, Thomas Wennekers
Published at 2020 at [SPRA 2020 - Rome, Italy](http://spra.org/)

### Abstract

Hierarchical Temporal Memory - Spatial Pooler is a cortical learning algorithm inspired by the biological functioning of the neocortex. It is responsible for the sparse encoding of spatial patterns used as an input for further processing inside of a Hierarchical Temporal Memory (HTM). During the learning process, the Spatial Pooler groups spatially similar inputs into the same sparse distributed representation (SDR) memorized as a set of active mini-columns. The role of SDR generated by the learning
process of the Spatial Pooler is to provide an input for learning of sequences inside of the HTM. One of the features of the Spatial Pooler is also the robustness to noise in the input. This paper summarizes the work in progress, which analyses the relationship between the encoding of the input pattern and the robustness of the
memorized pattern against noise. In this work many synthetic input patterns with different sparsity were used to set the hypothesis, which claims that SP robustness against the noise in the input depends on the sparsity of the input. To validate the hypothesis, many random input vectors with a large portion of noise were generated. Then the change of SDR output was compared with the change of input by the given portion of noise. It was shown that the SDR output change is very small in comparison to change of the
input by adding of a large portion of the noise in the input. By adding of a significant portion of the noise to the input, the learned output remains almost unchanged. This indicates a great robustness to noise. Experiments show that the robustness against noise of the Spatial Pooler directly depends on the sparsity of the input pattern.
Preliminary tests suggest implementation of a boosting mechanism of the input to improve the robustness against noise.

[Download Paper](./Experiments/S020%20SPRA%202020%20Noise%20Reduction.pdf).

[Video](https://dobricfamily-my.sharepoint.com/:v:/g/personal/damir_dobric_de/EUSE6s3qPcRBjxtlOkKaPakBWowUNrAY8bvjfS9DNLHBKA?e=2WlzqZ).

[Presentation](https://dobricfamily-my.sharepoint.com/:p:/g/personal/damir_dobric_de/Ed0Ja1BE3_5Hix5sz8ANN1IBX2IKAUN29VMw84kc2-xjpg?e=Ydpvsi).

## Scaling of the Spatial Pooler (Paper)

### Author

Damir Dobric, Andreas Pech, Bogdan Ghita, Thomas Wennekers
Published 2020 in the [Internatioanl Journal for Artifficial Intelligence](https://aircconline.com/abstract/ijaia/v11n4/11420ijaia07.html)
Also published at [AIS 2020](https://10times.com/ais-helsinki)

### Abstract

The Hierarchical Temporal Memory Cortical Learning Algorithm (HTM CLA) is a theory and machine learning technology that aims to capture cortical algorithm of the neocortex. Inspired by the biological functioning of the neocortex, it provides a theoretical framework, which helps to better understand how the cortical algorithm inside of the brain might work. It organizes populations of neurons in column-like units, crossing several layers such that the units are connected into structures called regions (areas). Areas and columns are hierarchically organized and can further be connected into more complex networks, which implement higher cognitive capabilities like invariant representations. Columns inside of layers are specialized on learning of spatial patterns and sequences. This work targets specifically spatial pattern learning algorithm called Spatial Pooler. A complex topology and high number of neurons used in this algorithm, require more computing power than even a single machine with multiple cores or a GPUs could provide. This work aims to improve the HTM CLA Spatial Pooler by enabling it to run in the distributed environment on multiple physical machines by using the Actor Programming Model. The proposed model is based on a mathematical theory and computation model, which targets massive concurrency. Using this model drives different reasoning about concurrent execution and enables flexible distribution of parallel cortical computation logic across multiple physical nodes. This work is the first one about the parallel HTM Spatial Pooler on multiple physical nodes with named computational model. With the increasing popularity of cloud computing and server less architectures, it is the first step towards proposing interconnected independent HTM CLA units in an elastic cognitive network. Thereby it can provide an alternative to deep neuronal networks, with theoretically unlimited scale in a distributed cloud environment.

[Download from IJAIA](https://aircconline.com/abstract/ijaia/v11n4/11420ijaia07.html)  
[Download from AIS 2020 publishing site](https://aircconline.com/csit/abstract/v10n6/csit100606.html)
