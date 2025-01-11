#ML 24/25-01 Investigate Image Reconstruction by using Classifiers

Project Overview
The project titled "Investigate Image Reconstruction by Using Classifiers" focuses on exploring the role of machine learning classifiers within the framework of Hierarchical Temporal Memory (HTM) for reconstructing images from Sparse Distributed Representations (SDRs). The implementation is carried out using the C# programming language. The objective is to understand how classifiers can be effectively used to reverse the HTM encoding process and regenerate input images with minimal loss of information. The project involves working with two existing classifiers, HtmClassifier and K-Nearest Neighbors (KNN), both of which were previously implemented in the HTM system by students.

#Background and Motivation
Hierarchical Temporal Memory (HTM) is a computational framework inspired by the structure and function of the human neocortex. HTM systems encode input data into Sparse Distributed Representations (SDRs), which are highly efficient and resilient to noise. While SDRs are excellent for learning and making predictions, the ability to reverse-engineer these SDRs back into their original input form is equally valuable. This project investigates how classifiers can assist in reconstructing input images from SDRs, effectively acting as a reverse encoder.
The need for accurate image reconstruction arises in various domains, such as:
Computer Vision: Reconstructing images from compressed representations.
Data Compression: Developing efficient encoding and decoding techniques.
Pattern Recognition: Enhancing the interpretability of machine learning models by visualizing learned representations.

#Project Objectives
The primary objectives of this project are:
Understand the Role of Classifiers in HTM: Explore how classifiers work within the HTM framework to decode SDRs.
Investigate Existing Classifiers: Analyze the performance of two existing classifiers, HtmClassifier and KNN, in the context of image reconstruction.
Implement an Image Reconstruction Experiment: Create a new experiment inspired by the SpatialLearning experiment to reconstruct input images from SDRs using classifiers.
Evaluate the Accuracy of Reconstruction: Measure the similarity between the original images and the reconstructed images using various similarity metrics.

#Classifiers in HTM
In the HTM system, classifiers play a critical role in decoding the predictions made by the Spatial Pooler (SP) and Temporal Memory (TM). Classifiers map SDRs back to their corresponding input values, thereby enabling the reconstruction of original inputs. This process can be summarized as follows:
INPUT -> ENCODER -> SP -> SDR -> CLASSIFIER -> INPUT
The encoder converts raw input data (such as images) into a binary SDR format. The Spatial Pooler and Temporal Memory process the SDRs to identify patterns and make predictions. The classifier then takes the predicted SDRs and attempts to reconstruct the original input data.
In this project, we will utilize the IClassifier<TIN, TOUT> interface, which defines methods for learning and predicting input values from SDRs. Key methods include:
Learn(key, actCells.ToArray()): Learns the association between input values and active cells.HTM's capacity to forecast future patterns based on previously trained data patterns. After a few cycles, HTM receives a unique pattern that compares the prior patterns to the current pattern. Input patterns should not repeat, and the uniqueness should be maintained.
GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3): Retrieves the predicted input values based on the SDR.
HTM's capacity to forecast future patterns based on previously trained data patterns.HTM's capacity to forecast future patterns based on previously trained data patterns. After a few 
cycles, HTM receives a unique pattern that compares the prior patterns to the current pattern.

#Steps to be Implemented
            •	The project will follow these steps to achieve the desired image reconstruction:
            •	Load Images from a Folder: Use a dataset of images to serve as input for the experiment.
            •	Encode Images Using ImageEncoder: Convert the input images into SDRs using the ImageEncoder, which is based on the ImageBinarizer NuGet package.
            •	Run the SpatialLearning Experiment: Utilize the existing SpatialLearning experiment to process the encoded SDRs.
            •	Invoke Classifiers to Reconstruct Input: Implement both the HtmClassifier and KNN to reconstruct the input images from the SDRs.
            •	Compare Reconstructed Images with Original Images: Use similarity functions from the neocortexapi to evaluate the accuracy of the reconstruction.

#Expected Outcomes:
By the end of this project, we expect to:
            •	Gain a deeper understanding of how classifiers can be used within HTM to reverse the encoding process.
            •	Evaluate and compare the performance of HtmClassifier and KNN in reconstructing images.
            •	Provide insights into the strengths and weaknesses of each classifier for image reconstruction tasks.
In conclusion, we have explored the utilization of the Neocortex API for image and scalar data processing, employing techniques inspired by the biological principles of the neocortex.The project will contribute to the broader understanding of how HTM can be applied in areas such as image processing, pattern recognition, and data reconstruction, offering practical insights for future applications in machine learning and artificial intelligence.
