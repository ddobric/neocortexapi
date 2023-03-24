

<h3 align="center">Project Name</h3>
<h3 align="center">ML22/23- 2 Investigate and Implement KNN Classifier</h3>

<!--The Project Problem Statement  -->
## The Project Problem Statement 
1. Please first read about classifiers in general. Try to understand a difference and explain it. Also, explain why and how it can be used with HTM.
2. Then investigate the KNN Classifier at [http://nupic.docs.numenta.org/1.0.0/api/algorithms/classifiers.html#knn-classifier](http://nupic.docs.numenta.org/1.0.0/api/algorithms/classifiers.html#knn-classifier) in detail and figure out how to implement it in C#
3. Please take also a look at university implementation and see if that one can be used  [https://github.com/UniversityOfAppliedSciencesFrankfurt/LearningApi/blob/f713a28984e8f3115952c54cd9d60d53faa76ffe/LearningApi/src/MLAlgorithms/AnomDetect.KMeans/KMeansAlgorithm.cs](https://github.com/UniversityOfAppliedSciencesFrankfurt/LearningApi/blob/f713a28984e8f3115952c54cd9d60d53faa76ffe/LearningApi/src/MLAlgorithms/AnomDetect.KMeans/KMeansAlgorithm.cs)

<!--Getting Started  -->
## Getting Started






<!-- Professor Review and Feedbcak  -->
## Professor Review and Feedbcak 



<!-- Sources -->
### Sources
1.  Research Paper 
   - A Brief Review of Nearest Neighbor Algorithm for Learning and Classification https://www.researchgate.net/publication/340693569_A_Brief_Review_of_Nearest_Neighbor_Algorithm_for_Learning_and_Classification
   - Using KNN Algorithm for Classification of Textual
Documents https://ieeexplore.ieee.org/document/8079924
   - Investigation of Hierarchical Temporal Memory Spatial Pooler’s Noise Robustness against Gaussian noise https://www.researchgate.net/publication/261021625_A_robust_implementation_of_the_spatial_pooler_within_the_theory_of_Hierarchical_Temporal_Memory_HTM
   - KNN Model-Based Approach in Classification https://www.researchgate.net/publication/2948052_KNN_Model-Based_Approach_in_Classification
   - Research and Implementation of Machine Learning Classifier Based on KNN https://iopscience.iop.org/article/10.1088/1757-899X/677/5/052038/pdf
   

2. Documents
   - Github https://docs.github.com/en/get-started
   - A Machine Lerning Guide to HTM https://www.numenta.com/blog/2019/10/24/machine-learning-guide-to-htm/
   - IBM's K-Nearest Neighbors Algorithm https://www.ibm.com/topics/knn
https://www.javatpoint.com/k-nearest-neighbor-algorithm-for-machine-learning
   - K-Nearest Neighbor(KNN) Algorithm for Machine Learning
https://www.tutorialspoint.com/machine_learning_with_python/machine_learning_with_python_knn_algorithm_finding_nearest_neighbors.htm
   - KNeighborsClassifierhttps://scikit-learn.org/stable/modules/generated/sklearn.neighbors.KNeighborsClassifier.html
   -  KNN Algorithm for Machine Learning https://serokell.io/blog/knn-algorithm-in-ml
https://www.datacamp.com/tutorial/k-nearest-neighbor-classification-scikit-learn
https://www.tutorialspoint.com/scikit_learn/scikit_learn_kneighbors_classifier.htm
   - Weighted-k-nn-classification  https://visualstudiomagazine.com/articles/2022/05/19/weighted-k-nn-classification.aspx

3. Videos
   - All basics for Temporial Memory, HTM https://www.youtube.com/@NumentaTheory
   - StatQuest: K-nearest neighbors, Clearly Explained https://www.youtube.com/watch?v=HVXime0nQeI
   - Working of KNN Algorithm https://www.youtube.com/watch?v=UqYde-LULfs
   - How to Choose the K Value into consideration https://www.youtube.com/watch?v=4HKqjENq9OU&t=1470s

<!--About Team_KNN  -->
## About Team_KNN 
	1. Ankush Patil 
	 - Started with reading the documnetaion and Understood the Working of KNN
	 - Build KNN Prototype for 2D array which can predict the outcome in the form of lables however the distnacee calclations are not appropriate
	 - Explored the diffrent methods used for Distance calculations such as Euclidian distance,Manhattan distance, Hamming distance
	 - Build the KNN based model for sequntial input and fine tuning the distnace and voting mechanism in KNN Model and tried to link it with Neocortix API

	2. Ayan Borthakur
	 - Started with the creating Matrix based KNN Implementation for hard coded data inputs and labels
	 - Worked on the pipeline to integrated the diffrenrent funtionality of matrix and sequnce data designed by Ankush and Build for Sequance based KNN predicaton 
	 - Integrated the Model with the Neo cortix API and observed the output and for accuracy calculation 

	3. Nasir Ishaq
	 - Initially Worked of decideing the K value and its diffrent outcomes 
	 - Started with the writing the K value funcation and worked figiring out the k value and model accuract
	 - Explored the voating principle and HTM and Temporial Memory based inout dataset