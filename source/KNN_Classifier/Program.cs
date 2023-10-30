using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

/************************************************************************************************************************************

################################################# CLASS SUMMARY #####################################################################

Class Program:

- This class contains the main method that controls the execution flow of the application. 
It prompts the user for input data and training data paths, creates a bag of words and feature vectors based on the training data, 
trains a KNN model on the feature vectors, saves the model and bag of words to a binary file, loads the saved model and bag of words, 
classifies the input data using the KNN model, and evaluates the KNN model on the test set using various metrics.

Class ModelSave: 

- This class defines a static method to save the KNN model and bag of words to a binary file.

Class ModelLoad: 

- This class defines a static method to load the KNN model and bag of words from a binary file.

Class Evaluate: 

- This class defines a static method to evaluate the KNN model on a test set using various metrics.

*************************************************************************************************************************************/


namespace textClassification
{
    /// <summary>
    /// Main class for the text classification program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the Training Data Path: ");
            string trainingDataPath = Console.ReadLine();

            // Load the training data files and create feature vectors
            string[] path = Directory.EnumerateFiles(@trainingDataPath, "*.txt").ToArray();
            KNN knn = new KNN(k: 3);
            List<string> BagOfWords = BOW.CreateBoW(path);
            List<Vector> featureVectors = Vector.CreateVector(BagOfWords);
            List<string> labels = Vector.CreateLabels(path);

            // Train the KNN model using the feature vectors and labels
            knn.Train(featureVectors, labels);

            // Save the trained KNN model and bag of words to a binary file
            Console.WriteLine("Enter the model file path to save: ");
            string modelFilePath = Console.ReadLine();
            ModelSave.Save(knn, BagOfWords, modelFilePath);
            Console.WriteLine($"Model saved to {modelFilePath}");

            // Load the trained KNN model and bag of words from a binary file
            Console.WriteLine("Enter the model file path to load: ");
            string loadModelFilePath = Console.ReadLine();
            (KNN loadedKnn, List<string> loadedBagOfWords) = ModelLoad.Load(loadModelFilePath);
            Console.WriteLine($"Model loaded from {loadModelFilePath}");

            Console.WriteLine("Enter the Input Data Path: ");
            string inputPath = Console.ReadLine();

            // Load the input text file for HTM
            String[] wordsInInput = System.Text.RegularExpressions.Regex.Split(File.ReadAllText(inputPath), @"[\s,;:.!?-]+");
            Vector.GetWordsFromInput(wordsInInput);
            
            // Convert the input text into an output vector using HTM
            int[] inputVector = HTM.generateInputVector(loadedBagOfWords, wordsInInput);

            // Use the output vector as input to the KNN model for classification
            string classification = loadedKnn.Classify(inputVector);
            Console.WriteLine(classification);

            // Evaluate the KNN model on the test set
            Evaluate.ModelEvaluate(knn, featureVectors, labels, "sport","politic");
            Console.ReadKey();
        }
    }


    /// <summary>
    /// Provides functionality to save a trained KNN model and Bag of Words to a binary file
    /// </summary>
    /// <param name="knn">The KNN model to save.</param>
    /// <param name="bagOfWords">The bag of words used to train the KNN model.</param>
    /// <param name="filePath">The file path where the model should be saved.</param>
    public class ModelSave
    {
        public static void Save(KNN knn, List<string> bagOfWords, string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, knn);
                formatter.Serialize(stream, bagOfWords);
            }
        }
    }

    
    public class ModelLoad
    {
        /// <summary>
        /// Provides functionality to load a trained KNN model and Bag of Words from a binary file
        /// </summary>
        /// <param name="filePath">The path to the file containing the serialized model.</param>
        /// <returns>A tuple containing the loaded KNN model and its associated bag of words.</returns>
        
        public static (KNN, List<string>) Load(string filePath)
        {
            KNN loadedKnn;
            List<string> bagOfWords;

            // Open the file for reading
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Deserialize the KNN model and bag of words from the file
                loadedKnn = (KNN)formatter.Deserialize(stream);
                bagOfWords = (List<string>)formatter.Deserialize(stream);
            }

            // Return the loaded KNN model and its associated bag of words as a tuple
            return (loadedKnn, bagOfWords);
        }
    }

    public class Evaluate
    {
        
        /// <summary>
        /// Evaluate the KNN model on a test set
        /// </summary>
        /// <param name="knn">The KNN model to evaluate</param>
        /// <param name="testFeatureVectors">The feature vectors of the test set</param>
        /// <param name="testLabels">The labels of the test set</param>
        /// <param name="class1">The positive class label</param>
        /// <param name="class2">The negative class label</param>
        public static void ModelEvaluate(KNN knn, List<Vector> testFeatureVectors, List<string> testLabels, string class1, string class2)
        {
            
            // Initialize variables for counting true/false positives/negatives
            int truePositives = 0;
            int falsePositives = 0;
            int falseNegatives = 0;
            int trueNegatives = 0;

            // Iterate through the test set
            for (int i = 0; i < testFeatureVectors.Count; i++)
            {
                // Classify the feature vector
                string predictedLabel = knn.Classify(testFeatureVectors[i].vector);

                // Update the counts based on the predicted and actual labels
                if (predictedLabel == testLabels[i])
                {
                    if (predictedLabel == class1)
                    {
                        truePositives++;
                    }
                    else
                    {
                        trueNegatives++;
                    }
                }
                else
                {
                    if (predictedLabel == class1)
                    {
                        falsePositives++;
                    }
                    else
                    {
                        falseNegatives++;
                    }
                }
            }

            // Calculate the accuracy, precision, recall, and F1 score
            double accuracy = (double)(truePositives + trueNegatives) / (truePositives + trueNegatives + falsePositives + falseNegatives);
            double precision = (double)truePositives / (truePositives + falsePositives);
            double recall = (double)truePositives / (truePositives + falseNegatives);
            double f1Score = 2 * precision * recall / (precision + recall);

            // Print the evaluation metrics
            Console.WriteLine($"Accuracy: {accuracy}");
            Console.WriteLine($"Precision: {precision}");
            Console.WriteLine($"Recall: {recall}");
            Console.WriteLine($"F1 Score: {f1Score}");
        }
    }

}

