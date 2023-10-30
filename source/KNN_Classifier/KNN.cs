using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*************************************************************************************************************************************

################################################# CLASS SUMMARY ######################################################################

Class KNN:

- This C# code defines a class called KNN that implements the K-Nearest Neighbors algorithm for text classification. The class is 
marked with the [Serializable] attribute, which means that it can be serialized and deserialized. 

- The class has three member variables: trainingData, labels, and k. trainingData is a list of feature vectors used to train the model, 
labels is a list of string labels corresponding to each feature vector, and k is an integer that represents the number of neighbors 
to consider when classifying a new input vector. 

- The class has a constructor that takes an integer k as input and sets the k member 
variable to the input value. The class also has two public methods: Train and Classify.

*************************************************************************************************************************************/



namespace textClassification
{
    [Serializable]

    /// <summary>
    /// This class implements the K-Nearest Neighbors (KNN) algorithm for text classification.
    /// </summary>
    public class KNN
    {
        public List<Vector> trainingData = new List<Vector>();
        public List<string> labels = new List<string>();
        public int k;


        /// <summary>
        /// Constructor for the KNN class.
        /// </summary>
        /// <param name="k">The number of nearest neighbors to consider for classification.</param>
        public KNN(int k)
        {
            this.k = k;
        }


        /// <summary>
        /// Train the KNN algorithm with the given feature vectors and labels.
        /// </summary>
        /// <param name="featureVectors">A list of feature vectors, where each vector represents a text document.</param>
        /// <param name="labels">A list of labels, where each label corresponds to a text document in the featureVectors list.</param>
        public void Train(List<Vector> featureVectors, List<string> labels)
        {
            this.trainingData = featureVectors;
            this.labels = labels;
        }


        /// <summary>
        /// Classify the input text document as either "politic" or "sport" based on its feature vector.
        /// </summary>
        /// <param name="inputVector">The feature vector of the input text document.</param>
        /// <returns>The predicted label of the input text document.</returns>
        public string Classify(int[] inputVector)
        {

            // Calculate the distances between the input vector and the training data.
            Dictionary<double, string> distances = new Dictionary<double, string>();
            Random random = new Random();
            for (int i = 0; i < trainingData.Count; i++)
            {
                double distance = CalculateDistance(trainingData[i].vector, inputVector);

                // If two distances are the same, add a small random value to one of them to ensure unique keys in the dictionary.
                while (distances.ContainsKey(distance))
                {
                    distance += random.NextDouble() * 1e-6; // add small random value to distance
                }
                distances.Add(distance, trainingData[i].label);
            }

            // Get the k-nearest neighbors and their corresponding labels.
            List<string> kNearestLabels = new List<string>();

            foreach (var distance in distances.OrderBy(x => x.Key).Take(k))
            {
                kNearestLabels.Add(distance.Value);
            }

            // Predict the label of the input text document based on the most common label among its k-nearest neighbors.
            return kNearestLabels.GroupBy(x => x)
                                          .OrderByDescending(x => x.Count())
                                          .First()
                                          .Key;
        }


        /// <summary>
        /// Calculates the Euclidean distance between two vectors for KNN.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The Euclidean distance between the two vectors.</returns>
        private double CalculateDistance(int[] vector1, int[] vector2)
        {
            return Math.Sqrt(vector1.Zip(vector2, (x, y) => Math.Pow(x - y, 2)).Sum());
        }

    }
}
