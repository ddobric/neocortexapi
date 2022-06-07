# **Invariant Representation Learning using Hierarchical Temporal Memory**

## **Invariant Representations**

### **Abstract**
One of humans' interesting cognitive ability is to recognize an object wherever it is on the frame of reference. This work incooperated a method to use HTM model for prediction/ learning of invariant object representation.

### **

### **Definition**
Invariant Representations in this research are variant portrayal of an object, acquired through means of [geometric transformation](https://en.wikipedia.org/wiki/Geometric_transformation).  
In the scope of this reearch, Invariant Representation in means of [Similarity(geometry)](https://en.wikipedia.org/wiki/Similarity_(geometry)) through means of translation are used.


### **Dataset**
In this Experiment, MNIST Dataset was used. The dimension of the picture used for predicting is regarded as the visual receptive field. 
To prepare the invariant representations for the training of the data, the number inside will be first cropped to its edge. Then the cropped frame can be translated around the visual receptive field.  

## **Experiment**
The training phase consist of a network of 3 layer, the first layer takes output directly from the images pixel values, the second layer deals with the pooled information from the first layer, and the third layer draws conclusion about which classes it is from output of the second layer.

![Convolution Scheme](assets/ConvolutionScheme.png)  
In the first layer, the visual receptive field is divided gridwise into NoX x NoY frames. These frames then will be learned by first layer's spatial pooler.   
If the whole image is of 16x16 dimension, NoX = 4, NoY = 4, input bits of the first layer' spatial pooler can be 4x4 = 16bits.  
There is also a direction in which these region can overlap each other. This can be changed from the setting of frame resolution (NoX,NoY and the dimension of the frame). As the regions will always be evenly spaced, if frame dimension > image dimension divided by number of frames(NoX x NoY) there will be overlap.   

The output SDR of the first layer will then be processed ?/ learned by another spatial pooler in the second layer. 
In current setup, the second layer's SP will learned from 4 or 9 region of the image, each region in layer 2 consisted of 4 region in layer 1. This make the SP in layer 2 has an input bit resolution of 4 x SDR layer 1's length. The grouping of adjacent region enable the spatial relationship between near region in layer 1.  

Layer 3 SP will learned input bits as all SDRs from layer 2 combined. The SDRs list from layer 3 trained SP will be saved in an HtmClasifier according to label.  
These saved SDR will be used later in inference mode.