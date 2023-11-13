using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*************************************************************************************************************************************

################################################# CLASS SUMMARY ######################################################################

Class BOW:

- This C# code defines a class called BOW, which is used to create a bag of words from training data text files. The class is marked 
with the [Serializable] attribute, which means that it can be serialized and deserialized.

- The class has a static member called BagOfWords, which is a list of strings representing the bag of words. The class also has a static
method called CreateBoW, which takes an array of file paths as input and returns a list of strings representing the bag of words.

- The CreateBoW method works by iterating over each file path in the input array. For each file, it reads the contents of the file using
File.ReadAllText, splits the text into words using a regular expression, and then iterates over each word. For each word, it creates 
a StringBuilder and adds all alphanumeric characters (i.e. basic Latin letters without special characters and punctuations) to the 
StringBuilder. 

- If the resulting string is longer than two characters, it is written to a file (depending on the file name) and added 
to the BagOfWords list (if it is completely unique). Overall, this class provides a useful utility for text processing tasks that 
require a bag of words representation of text data.

*************************************************************************************************************************************/

namespace textClassification
{
    /// <summary>
    /// This class is used to create bag of words from the training data text files which is a string data type.
    /// </summary> 
    [Serializable]
    public class BOW 
    {
        /// <summary>
        /// A list of all unique words in the bag of words.
        /// </summary>
        public static List<String> BagOfWords = new List<String>();


        /// <summary>
        /// Creates a bag of words from the training data text files.
        /// </summary>
        /// <param name="path">An array of file paths for the training data text files.</param>
        /// <returns>A list of unique words in the bag of words.</returns>
        public static List<String> CreateBoW(string[] path)
        {

            int article = 1;

            foreach (string file in path)
            {

                var everyWords = System.Text.RegularExpressions.Regex.Split(File.ReadAllText(file), @"[\s,;:.!?-]+");

                //Creating new files with the cleaned words with SteamWriter
                TextWriter tw;
                if (file.Contains("politic"))
                {
                    tw = new StreamWriter(@"..\..\..\Articles\CleanedText\politic" + article.ToString() + ".txt");
                }
                else if (file.Contains("sport"))
                {
                    tw = new StreamWriter(@"..\..\..\Articles\CleanedText\sport" + article.ToString() + ".txt");
                }
                else
                {
                    tw = new StreamWriter(@"..\..\..\Articles\CleanedText\unkown" + article.ToString() + ".txt");
                }

                // Get all words from txt files
                foreach (var word in everyWords)
                {
                    var builder = new StringBuilder();
                    foreach (char letter in word)
                    {
                        // If the char matches basic latin alphabet (without special chars and punctuations) 
                        //than the program will add them to the StringBuilder
                        if (letter <= 90 && letter >= 65 || letter >= 97 && letter <= 122)
                        {
                            builder.Append(letter.ToString().ToLower());
                        }
                    }

                    // All words which are longer than 2 chars will be added to the list
                    if (builder.Length > 2)
                    {
                        tw.WriteLine(builder);

                        if (!BagOfWords.Contains(builder.ToString()))
                        {
                            BagOfWords.Add(builder.ToString());
                        }
                    }
                }
                tw.Close();
                article++;
                
            }
            return BagOfWords;

        }
    }
}