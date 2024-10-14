using ExcelDataReader;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Org.BouncyCastle.Ocsp;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using static NeoCortexApiSample.MultisequenceLearningTeamMSL;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NeoCortexApiSample
{
    class Program
    {
        static double MinVal = 0.0;
        static double MaxVal = 99.0;

        /// <summary>
        /// This sample shows a typical experiment code for SP and TM.
        /// You must start this code in debugger to follow the trace.
        /// and TM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //
            // Starts experiment that demonstrates how to learn spatial patterns.
            //SpatialPatternLearning experiment = new SpatialPatternLearning();
            //experiment.Run();

            //
            // Starts experiment that demonstrates how to learn spatial patterns.
            //SequenceLearning experiment = new SequenceLearning();
            //experiment.Run();

            // This method is developed by Team_MSL to read arbitrary data from single txt file and improve CPU utilization*/
            RunPredictionMultiSequenceExperiment();
        }


        /// <summary>
        /// Runs a multi-sequence prediction experiment using a prototype for building the prediction engine.
        /// </summary>
        private static void RunPredictionMultiSequenceExperiment()
        {
            Dictionary<string, List<double>> sequences = new();

            // Step 1: Retrieve sequences from an Excel file.
            sequences = GetInputFromExcelFile();

            // Step 2: Create an instance of the MultisequenceLearningTeamMSL class for the experiment.
            // Prototype for building the prediction engine.
            MultisequenceLearningTeamMSL experiment = new();

            // Step 3: Train the prediction engine using the provided sequences.
            var predictor = experiment.Run(sequences);

            List<List<double>> testSequences = new();

            // Step 4: Retrieve test sequences from another Excel file.
            testSequences = GetSubSequencesInputFromExcelFile();

            // Step 5: Iterate through each test sequence and make predictions.
            foreach (var numberList in testSequences)
            {
                // Reset the predictor for each new test sequence.
                predictor.Reset();

                // Step 6: Make predictions for the next elements in the test sequence.
                PredictNextElement(predictor, numberList);
            }

        }



        /// <summary>
        /// Reads a set of sequences from an Excel file, filtering values within a specified range.
        /// The Excel file should contain numerical values in columns, and each row represents a subsequence.
        /// Values outside the specified range (MinVal to MaxVal) are excluded from the sequences.
        /// </summary>
        /// <returns>A List of Lists, where each inner list represents a valid sequences.</returns>
        private static Dictionary<string, List<double>> GetInputFromExcelFile()
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "Input.xlsx");
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int temp = 0;

                    while (reader.Read())
                    {
                        List<double> inputList = new List<double>();
                        bool rowHasData = false;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string digit = reader.GetValue(i)?.ToString();

                            if (string.IsNullOrWhiteSpace(digit))
                            {
                                // Skip over empty cells
                                continue;
                            }

                            if (double.TryParse(digit, out double number))
                            {

                                if (number >= MinVal && number <= MaxVal)
                                {
                                    inputList.Add(number);
                                }
                                rowHasData = true;

                            }

                        }

                        if (rowHasData)
                        {

                            Console.Write("Sequence " + temp + " : ");
                            Console.WriteLine(string.Join(" ", inputList));
                            temp++;
                            sequences.Add("Sequence: " + temp, inputList);
                        }
                    }
                }
            }

            return sequences;
        }


        private static void RunMultiSimpleSequenceLearningExperiment()
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            sequences.Add("S1", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 }));
            sequences.Add("S2", new List<double>(new double[] { 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0 }));

            //
            // Prototype for building the prediction engine.
            MultisequenceLearningTeamMSL experiment = new MultisequenceLearningTeamMSL();
            var predictor = experiment.Run(sequences);
        }


        /// <summary>
        /// Reads a set of subsequences from an Excel file, filtering values within a specified range.
        /// The Excel file should contain numerical values in columns, and each row represents a subsequence.
        /// Values outside the specified range (MinVal to MaxVal) are excluded from the subsequences.
        /// </summary>
        /// <returns>A List of Lists, where each inner list represents a valid subsequence.</returns>
        public static List<List<double>> GetSubSequencesInputFromExcelFile()
        {
            var SubSequences = new List<List<double>>();

            using (var stream = File.Open("Subsequence_input.xlsx", FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        var TestSubSequences = new List<double>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.GetDouble(i) >= MinVal && reader.GetDouble(i) <= MaxVal)
                            {
                                TestSubSequences.Add(reader.GetDouble(i));
                            }
                        }

                        SubSequences.Add(TestSubSequences);
                    }
                }
            }

            return SubSequences;
        }


        /// <summary>
        /// Predicts the next elements in a sequence using a given predictor and evaluates prediction accuracy.
        /// </summary>
        /// <param name="predictor">The predictor object used to make predictions.</param>
        /// <param name="list">The input list representing a sequence of elements.</param>
        private static void PredictNextElement(Predictor predictor, List<double> list)
        {
            Debug.WriteLine("------------------------------");
            int countOfMatches = 0;
            int totalPredictions = 0;
            string predictedSequence = "";
            string predictedNextElement = "";
            string predictedNextElementsList = "";

            // Generate file name with current date and time
            string fileName = string.Format("Final Accuracy ({0:dd-MM-yyyy HH-mm-ss}).csv", DateTime.Now);
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            for (int i = 0; i < list.Count - 1; i++)
            {
                var item = list[i];
                var nextItem = list[i + 1];
                var res = predictor.Predict(item);

                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                        Debug.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    var tokens3 = res.Last().PredictedInput.Split('_');
                    predictedSequence = tokens[0];
                    predictedNextElement = tokens2.Last();
                    predictedNextElementsList = string.Join("-", tokens3.Skip(1));
                    Debug.WriteLine($"Predicted Sequence: {predictedSequence}, predicted next element {predictedNextElement}");

                    if (nextItem == double.Parse(predictedNextElement))
                    {
                        countOfMatches++;
                    }
                }
                else
                {
                    Debug.WriteLine("Nothing predicted :(");
                }

                totalPredictions++;

                // Accuracy logic added which is based on count of matches and total predictions.
                double accuracy = AccuracyCalculation(list, countOfMatches, totalPredictions, predictedSequence, predictedNextElement, predictedNextElementsList, filePath);
                Debug.WriteLine($"Final Accuracy for elements found in predictedNextElementsList = {accuracy}%");

            }

            Debug.WriteLine("------------------------------");
        }

        // Accuracy logic added which is based on count of matches and total predictions.
        // Accuracy is calculated in the context of predicting the next element in a sequence.
        // The accuracy is calculated as the percentage of correctly predicted next elements (countOfMatches)
        // out of the total number of predictions (totalPredictions).
        private static double AccuracyCalculation(List<double> list, int countOfMatches, int totalPredictions, string predictedSequence, string predictedNextElement, string predictedNextElementsList, string filePath)
        {
            double accuracy = (double)countOfMatches / totalPredictions * 100;
            Debug.WriteLine(string.Format("The test data list: ({0}).", string.Join(", ", list)));

            // Append to file in each iteration
            if (predictedNextElementsList != "")
            {
                string line = $"Predicted Sequence Number is: {predictedSequence}, Predicted Sequence: {predictedNextElementsList}, Predicted Next Element: {predictedNextElement}, with Accuracy =: {accuracy}%";

                Debug.WriteLine(line);
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
            else
            {
                string line = $"Nothing is predicted, Accuracy is: {accuracy}%";
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
            return accuracy;
        }
    }
}