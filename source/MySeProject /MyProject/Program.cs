using NeoCortexApi;
using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static NeoCortexApiSample.MultiSequenceLearning;

namespace NeoCortexApiSample
{
    class Program
    {
        /// <summary>
        /// This sample shows a typical experiment code for SP and TM.
        /// You must start this code in debugger to follow the trace.
        /// and TM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            RunMultiSequenceLearningExperiment();
        }

        /// <summary>
        /// This example demonstrates how to learn sequences and how to use the prediction mechanism.
        /// First,string is converted into an array of characters, and asciii value of each character is stored in a list.
        /// Second,sequences are learned from the text file.
        /// Third,three short sequences with three elements each are created und used for prediction. The predictor used by experiment privides to the HTM every element of every predicting sequence.
        /// The predictor tries to predict the next element.
        /// </summary>
        private static void RunMultiSequenceLearningExperiment()
        {
            List<double> asciiSequence = new List<double>();

            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //path to the input text file.
            string filePath = @"filename.txt";

            // Call the function to read the file and convert to char array.
            List<char> charList = ReadFileAndConvertToCharList(filePath);

            foreach (char character in charList)
            {
                double asciiValue = (double)character;

                asciiSequence.Add(asciiValue);

            }
            Console.WriteLine("ASCII Sequence:");

            foreach (int asciiCode in asciiSequence)
            {
                Console.Write(asciiCode + " ");
            }
            sequences.Add("S1", asciiSequence);

            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();

            var predictor = experiment.Run(sequences);


            // These list are used to see how the prediction works.
            // Predictor is traversing the list element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.
            var list1 = new double[] { 'F', 'i', 'r', 's', 't' };
            var list2 = new double[] { 'F', 'I', 'R', 'S', 'T' };
            var list3 = new double[] { 'c', 'i' };


            predictor.Reset();
            PredictNextElement(predictor, list1);

            predictor.Reset();
            PredictNextElement(predictor, list2);

            predictor.Reset();
            PredictNextElement(predictor, list3);
        }

        private static void PredictNextElement(Predictor predictor, double[] list)
        {
            Debug.WriteLine("------------------------------");

            foreach (var item in list)
            {
                var res = predictor.Predict(item);

                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                        Debug.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }

                    var tokens = res.First().PredictedInput.Split('_');

                    var tokens2 = res.First().PredictedInput.Split('-');

                    Console.WriteLine(tokens2.Last());

                    var tokens3 = tokens2.Last();

                    Debug.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens3.Last()}");
                }
                else
                    Debug.WriteLine("Nothing predicted :( ");
            }

            Debug.WriteLine("------------------------------");
        }

        //function to read the file and return  char array.
        static List<char> ReadFileAndConvertToCharList(string filePath)
        {
            List<char> charList = new List<char>();

            try
            {
                // Read all text from the file
                string fileContent = File.ReadAllText(filePath);

                // Convert the string to a char array
                char[] charArray = fileContent.ToCharArray();

                // Convert the char array to a list
                charList.AddRange(charArray);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file not found, access denied, etc.)
                Console.WriteLine("Error reading the file: " + ex.Message);
            }

            return charList;
        }
    }

}



































