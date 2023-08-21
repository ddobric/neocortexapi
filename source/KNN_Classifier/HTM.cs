using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************************************************************

################################################# CLASS SUMMARY ######################################################################

Class HTM:

- This C# code contains a class called "HTM" that performs text classification by converting input text into vectors. 
The class contains public method, "generateInputVector".

- Method "generateInputVector": A static public method that takes a List of strings containing Bag of Words and a string array of 
words from the input text as input. The method creates an input vector of integers by checking if each word in the Bag of Words list 
exists in the input array and assigns the value of 1 or 0 to the corresponding index in the input vector. The method returns the 
input vector.

*************************************************************************************************************************************/


namespace textClassification
{
    /// <summary>
    /// This class represents a Hierarchical Temporal Memory (HTM) model for text classification. It takes in input text and converts it into vectors for further processing.
    /// </summary>
    
    public class HTM
    {
        //private Dictionary<string, int> _wordCount;
        public static int[] inputVector;
<<<<<<< HEAD:MySEProject/KNN_Project/KNN_Classifier/HTMClassifier.cs
=======
        /// <summary>
        /// Constructor for HTM class.
        /// </summary>
        public HTM()
        {
            _wordCount = new Dictionary<string, int>();
        }

        /// <summary>
        /// Trains the HTM model by processing input text files.
        /// </summary>
        /// <param name="filePaths">Array of file paths to the input text files.</param>
        public void Train(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                string[] words = File.ReadAllText(filePath).ToLower().Split(' ', '\n', '\r', '\t');
                foreach (string word in words)
                {
                    if (string.IsNullOrWhiteSpace(word))
                    {
                        continue;
                    }

                    if (!_wordCount.ContainsKey(word))
                    {
                        _wordCount[word] = 0;
                    }

                    _wordCount[word]++;
                }
            }
        }

        /// <summary>
        /// Converts input text into a vector for further processing.
        /// </summary>
        /// <param name="words">Array of words in the input text.</param>
        /// <returns>Array of integers representing the vector of the input text.</returns>
        public int[] ConvertToVector(string[] words)
        {
            List<int> vector = new List<int>();
            foreach (string word in _wordCount.Keys)
            {
                if (Array.IndexOf(words, word) >= 0)
                {
                    vector.Add(1);
                }
                else
                {
                    vector.Add(0);
                }
            }
            return vector.ToArray();
        }
>>>>>>> 7305e039302f6b4e10550ca91874c0d57699aba4:MySEProject/KNN_Project/KNN_Classifier/HTM.cs

        /// <summary>
        /// Generates an input vector from the Bag of Words and the input text.
        /// </summary>
        /// <param name="BagOfWords">List of words to consider for the input vector.</param>
        /// <param name="wordsInInput">Array of words in the input text.</param>
        /// <returns>Array of integers representing the input vector.</returns>
        public static int[] generateInputVector(List<String> BagOfWords, String[] wordsInInput)
        {


            inputVector = new int[BagOfWords.Count];

            for (int i = 0; i < BagOfWords.Count; i++)
            {
                if (wordsInInput.Contains(BagOfWords[i]))
                {
                    inputVector[i] = 1;
                }
                else
                {
                    inputVector[i] = 0;
                }
            }
            return inputVector;

        }
    }
}