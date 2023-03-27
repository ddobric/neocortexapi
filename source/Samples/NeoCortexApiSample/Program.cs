using ExcelDataReader;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
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
using static NeoCortexApiSample.MultiSequenceLearning;
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
            RunPredictionMultiSequenceExperiment(); /* This method is developed by Team_MSL to read arbitrary data from single txt file and improve CPU utilization*/
        }



        private static void RunPredictionMultiSequenceExperiment()
        {
            Dictionary<string, List<double>> sequences = new();

            sequences = GetInputFromExcelFile();
            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new();

            var predictor = experiment.Run(sequences);
            List<List<double>> testSequences = new();
            testSequences = GetSubSequencesInputFromExcelFile();
            predictor.Reset();
            foreach (var numberList in testSequences)
            {
                PredictNextElement(predictor, numberList);
            }

        }



        /* This code detects empty cell at the end of the row and it takes input from excel*/

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
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);
        }



        /* This method takes the input from Excel file */

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



        private static void PredictNextElement(Predictor predictor, List<double> list)
        {
            Debug.WriteLine("------------------------------");
            int countOfMatches = 0;
            int totalPredictions = 0;
            string predictedSequence = "";
            string predictedNextElement = "";
            string[] tokens = null;
            string[] tokens2 = null;

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

                    tokens = res.First().PredictedInput.Split('_');
                    tokens2 = res.First().PredictedInput.Split('-');
                    predictedSequence = tokens[0];
                    predictedNextElement = tokens2.Last();
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
            }

            double accuracy = (double)countOfMatches / totalPredictions * 100;
            Debug.WriteLine($"Final Accuracy: {accuracy}%");
            Debug.WriteLine(string.Format("The test data list: ({0}).", string.Join(", ", list)));

            // Generate file name with current date and time
            string fileName = string.Format("Final Accuracy ({0:dd-MM-yyyy HH-mm-ss}).csv", DateTime.Now);
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                //writer.WriteLine($"Predicted Sequence,{predictedSequence}");
                //writer.WriteLine($"Predicted Next Element,{predictedNextElement}");
                //writer.WriteLine($"Final Accuracy,{accuracy}%");
                writer.WriteLine("Predicted Sequence: " + string.Join(",", tokens) + ", Predicted Next Element: " + tokens2.Last() + ", Final Accuracy: " + accuracy + "%");

            }

            Debug.WriteLine("------------------------------");
        }


    }
}