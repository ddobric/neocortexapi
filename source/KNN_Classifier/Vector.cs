using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*************************************************************************************************************************************

################################################# CLASS SUMMARY ######################################################################

Class Vector:

- This C# code includes a namespace called "textClassification" which has a class called "Vector". The purpose of this class is to 
hold a label and a vector of integers. The class also contains static methods to retrieve words from input, create a vector from a 
list of words, and create labels based on filenames.

- the Vector class is used to create and manipulate vectors of integers with labels, and provides methods for cleaning input text, 
creating vectors from lists of words, and creating labels for files based on their filenames. It is used as a helper class in the 
KNN class for text classification.

*************************************************************************************************************************************/



namespace textClassification
{
    [Serializable]
    /// <summary>
    /// This class represents a vector that contains a label and an int array that forms the vector.
    /// </summary>
    public class Vector
    {
        /// <summary>
        /// The label of the vector.
        /// </summary>
        public String label { get; set; }

        /// <summary>
        /// The int array that represents the vector.
        /// </summary>
        public int[] vector { get; set; }

        /// <summary>
        /// A dynamic variable that holds the cleaned files.
        /// </summary>
        public static dynamic cleanedFiles;

        /// <summary>
        /// The input vector that will be used to compare with other vectors.
        /// </summary>
        public static int[] inputVector;


        /// <summary>
        /// Initializes a new instance of the Vector class.
        /// </summary>
        /// <param name="label">The label of the vector.</param>
        /// <param name="vector">The int array that represents the vector.</param>
        public Vector(string label, int[] vector)
        {
            this.label = label;
            this.vector = vector;
        }

        /// <summary>
        /// This method takes input in the class.
        /// </summary>
        /// <param name="wordsInInput">The words in the input.</param>
        public static void GetWordsFromInput(String[] wordsInInput)
        {

            List<String> listOfInputWords = new List<String>();

            foreach (var word in wordsInInput)
            {

                var builder = new StringBuilder();

                foreach (char letter in word)
                {

                    if (letter <= 90 && letter >= 69 || letter >= 97 && letter <= 122) //changed letter number by 1 not worked so restoring back to normal
                    {
                        builder.Append(letter);
                    }

                    listOfInputWords.Add(builder.ToString().ToLower());
                }
            }
        }



        /// <summary>
        /// This method creates vectors for each document in the dataset.
        /// </summary>
        /// <param name="BagOfWords">The list of unique words in the dataset.</param>
        /// <returns>A list of vectors representing the dataset.</returns>
        public static List<Vector> CreateVector(List<String> BagOfWords)
        {
            List<Vector> listOfVectors = new List<Vector>();

            // Get the list of cleaned text files in the Articles\CleanedText directory.
            cleanedFiles = Directory.EnumerateFiles(@"..\..\..\Articles\CleanedText\", "*.txt");

            // For each cleaned text file, create a vector by counting the number of occurrences of each word in the BagOfWords.
            foreach (var cleanedFile in cleanedFiles)
            {

                // Load all words in that file into a string array.
                String[] Words = System.Text.RegularExpressions.Regex.Split(File.ReadAllText(cleanedFile), @"[\s,;:.!?-]+");
                String label = "";
                int[] temporaryVectorList = new int[BagOfWords.Count];

                // For every word in BagOfWords in the cleaned Textfile. check if the WORD is present in the file, return value 1, else 0. 
                for (int i = 0; i < BagOfWords.Count; i++)
                {
                    if (Words.Contains(BagOfWords[i]))
                    {
                        temporaryVectorList[i] = 1;
                    }
                    else
                    {
                        temporaryVectorList[i] = 0;
                    }
                }

                // Based on the file name of the cleaned textfile, identify which theme it belongs to and save that as the attached label.
                if (cleanedFile.Contains("politic"))
                {
                    label = "politic";
                }
                else if (cleanedFile.Contains("sport"))
                {
                    label = "sport";
                }

                // Add a new entry into my listOfVector with the Object Vector, which consists of a label to indentify it's belonging, along with an int vector of 0,0,0,0,1,1,1 etc.
                listOfVectors.Add(new Vector(label, temporaryVectorList));
            }
            return listOfVectors;
        }


        /// <summary>
        /// This method creates labels for each file based on its name.
        /// </summary>
        /// <param name="files">The list of files to create labels for.</param>
        /// <returns>A list of labels corresponding to the files.</returns>
        public static List<string> CreateLabels(string[] files)
        {
            List<string> labels = new List<string>();

            // For each file in the list of files, determine its label based on its file name.
            foreach (string file in files)
            {
                if (file.Contains("politic"))
                {
                    labels.Add("politic");
                }
                else if (file.Contains("sport"))
                {
                    labels.Add("sport");
                }
            }
            return labels;
        }

    }
}

