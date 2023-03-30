### SDR_Classifier


## Overview
Sparse distribution representation classifier is a type of machine learning classifier that uses a sparse feature representation to classify data. Unlike traditional classifiers that use dense feature representation, where all the features are used to represent the data, sparse distribution representation classifier uses a subset of features to represent the data. The selected features are used to create a binary vector where only a small number of bits are turned on and the remaining bits are turned off. This sparse representation reduces the dimensionality of the data and avoids overfitting, reduces computation cost, and may capture important features.

![image](https://user-images.githubusercontent.com/116573939/228845153-4beb3696-b1c1-40fd-aec0-0025b219ea1e.png)

Fig : Flow of data in HTM-SDR hierarchy


## Working
To classify the input data, the classifier uses the sparse representation as input. The classifier can be any machine learning algorithm such as logistic regression, support vector machine, or neural network. The classifier learns to associate the sparse representation with the output class labels during the training phase.

During the testing phase, the input data is converted into a sparse representation using the same feature selection algorithm used during training. The classifier then uses this sparse representation to predict the output class label.


Sparse distributed representation classifier is different from traditional classifiers that use dense feature representation. In dense representation, all the features are used to represent the input data, which can lead to overfitting, high computation cost, and may not capture important features. Sparse representation, on the other hand, reduces the dimensionality of the data and avoids overfitting, reduces computation cost, and may capture important features.

## Summary
In summary, the sparse distribution representation classifier is a machine learning classifier that uses a sparse feature representation to classify data. It works by selecting a subset of features from the input data using a feature selection algorithm and creating a binary vector where only a small number of bits are turned on and the remaining bits are turned off. This sparse representation reduces the dimensionality of the data and avoids overfitting, reduces computation cost, and may capture important features. The classifier uses the sparse representation as input to predict the output class label. It has many applications, including image classification, natural language processing, and financial fraud detection. Overall, the sparse distribution representation classifier is a useful technique for improving the efficiency of computation and capturing important features in machine learning tasks.

## Project outline
1. Understanding current implementation
2. Finding input dataset and preprocessing and converting it to the format accepted by SDR classifier as input.
3. Implementing buckets from scalar encoders
4. Adding tests to check the noise tolerance

## Project References

[1] Hawkins, J. and Ahmad, S. (2016). Why neurons have thousands of synapses, a theory of sequence memory in neocortex. Frontiers in neural circuits, 10, p.23. 

[2]  Numenta. (2021). HTM Overview. [online] Available at: https://numenta.com/resources/htm-overview/ [Accessed 23 Mar. 2023]. 

[3] Roberts, E., James, K. and Ahmad, S. (2021). HTM anomaly detection in a manufacturing plant. [online] Available at: https://numenta.com/resources/papers/htm-anomaly-detection-in-a-manufacturing-plant/ [Accessed 23 Mar. 2023]. 

[4] Dillon A, “SDR Classifier”, Sept 2016, https://hopding.com/sdrclassifier#title 

[5] Purdy S, “BaMI-Encoders”, Technical Report Version 0.4, Numenta Inc 

[6] "Softmax function" by Wikipedia contributors. Wikipedia, The Free Encyclopedia. Accessed on 24 March 2023. Available online: https://en.wikipedia.org/wiki/Softmax_function 

[7] "Hierarchical Temporal Memory" by Wikipedia contributors. Wikipedia, The Free Encyclopedia. Accessed on 24 March 2023.  

[8] Ameer, P. M., & Parthasarathy, P. (2019). An efficient and effective algorithm for novelty detection and classification using hierarchical temporal memory. Neural Computing and Applications, 31(2), 501-513. https://doi.org/10.1007/s00521-017-3025-5
