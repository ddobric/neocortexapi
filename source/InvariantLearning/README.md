# **Invariant Representation Learning using Hierarchical Temporal Memory**

## **Invariant Representations**

### **Abstract**

There is an everlasting urges for human in studying about the brain.
This question triggered ones curiosity to the extreme, as it doesn't only show significant application potential, it also questioning what existence and intelligence means to ourselves.
Researches in brain ended up with developing blueprints to build a neural model in computer and use a scoped Turing's test to verified it.
However, most models nowadays focus on their applicable fields, and are highly specialised in these, with some exceeded humans in execution.
Numenta's HTM-CLA is a neural model which was inspired by our biological brain structure and is showing promising potential in research about Intelligence, or general AI. 
This work incooperated a method to use HTM-CLA model for prediction/ learning of invariant object representation.

### **Introduction**

### **Definition**
Invariant Representations in this research are variant portrayal of an object, acquired through means of [geometric transformation](https://en.wikipedia.org/wiki/Geometric_transformation).  
In the scope of this reearch, Invariant Representation in means of [Similarity(geometry)](https://en.wikipedia.org/wiki/Similarity_(geometry)) through means of translation are used.


### **Dataset**
In this Experiment, MNIST Dataset was used.
The dimension of the picture used for predicting is regarded as the visual receptive field.
To prepare the invariant representations for the training of the data, the number inside will be first cropped to its edge. Then the cropped frame can be translated around the visual receptive field.  

## **Experiment**
The experiment on this work will only learn the original image of the numbers, not their Invariant Representations (this may changed) in case of rotation. This work suggests that for a thought experiment, humans focus on any object to learn them, putting object in peripheral vision to learn is not preffered.

The training phase consist of a network of 3 layer, the first layer takes input directly from the images pixel values, the second layer deals with a bigger region than of the first, and the third layer use the whole image as it input.
Each of the layer's SPs take the image region and creating SDRs. These SDRs will then be observed to find the best voting function for getting the output label.  
![Convolution Scheme](assets/ConvolutionScheme.png)  
In the first layer, the visual receptive field is divided gridwise into NoX x NoY frames. These frames then will be learned by first layer's spatial pooler.   
If the whole image is of 16x16 dimension, NoX = 4, NoY = 4, input bits of the first layer' spatial pooler can be 4x4 = 16bits.  
There is also a possibility in which these region can overlap each other. This can be changed from the setting of frame resolution (NoX,NoY and the dimension of the frame). As the regions will always be evenly spaced, if frame dimension > image dimension divided by number of frames(NoX x NoY) there will be overlap.   



