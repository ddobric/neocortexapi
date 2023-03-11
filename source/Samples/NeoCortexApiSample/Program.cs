using GemBox.Spreadsheet.Drawing;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
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

            //RunMultiSimpleSequenceLearningExperiment();
            //RunMultiSequenceLearningExperiment();

            // new RunPredictionMultiSequenceExperiment() 
            RunPredictionMultiSequenceExperiment(); /* This method is developed by Team_MSL to read arbitrary data from single txt file and improve CPU utilization*/
        }



        private static void RunPredictionMultiSequenceExperiment()
        {
            Dictionary<string, List<double>> sequences = new();

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));

            //sequences = GetInputFromTextFile();   //uncomment this to read values from text file
            sequences = GetInputFromCsvFile(@"D:\SE_Project\Project\neocortexapi_Team_MSL\source\MultiSequenceLearning_Team_MSL\Input_Files\input1.csv");


            foreach (KeyValuePair<string, List<double>> kvp in sequences)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));

            //
            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new();

            // to get list of double values needed in later code changes
            //foreach (List<Double> entry in sequences.Values)
            //{
            //List<Double> InputSeq = new();
            //   InputSeq = entry;
            //    Console.WriteLine(InputSeq);
            //}

            var predictor = experiment.Run(sequences);

            //
            // These list are used to see how the prediction works.
            // Predictor is traversing the list element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.
            var list1 = new double[] { 0.0, 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0, 20.0, 23.0 };

            predictor.Reset();
            PredictNextElement(predictor, list1);
        }



        //method to add input sequences through external text files

        private static Dictionary<string, List<double>> GetInputFromTextFile()
        {
            Dictionary<string, List<double>> sequences = new();
            using (StreamReader reader = new StreamReader(@"D:\FUAS\Software_Engineering\MSL\neocortexapi_Team_MSL\source\MultiSequenceLearning_Team_MSL\Input_Files\input1.txt"))
            {
                int temp = 0;
                List<double> inputList = new();
                // for 10 input sequences
                string[] list1 = new string[10];

                while (!reader.EndOfStream)
                {

                    var all_rows = reader.ReadToEnd();

                    for (int i = 0; i <= all_rows.Length; i++)
                    {
                        if (all_rows.Contains("\r\n"))
                        {
                            list1 = Regex.Split(all_rows, "(?<=\r\n)");
                        }
                    }
                    var numbers = all_rows.Split(',');
                    //Console.WriteLine(numbers[temp]);
                    //string[] splittedFile = Regex.Split(all_rows, "(?<=\r\n)");

                    foreach (var sequence in list1)
                    {
                        inputList.Add(Convert.ToDouble(sequence));
                        temp++;
                        sequences.Add("Sequence" + temp, inputList);
                    }


                }
            }
            return sequences;
        }

        /* Method to read the data from CSV file for comparing which file type is having less CPU utilization */

        private static Dictionary<string, List<double>> GetInputFromCsvFile(string filePath)
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                int temp = 0;
                List<double> inputList = new List<double>();
                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine();
                    var numbers = row.Split(',');
                    Console.WriteLine(row);

                    foreach (var digit in numbers)
                    {
                        if (double.TryParse(digit, out double number))
                        {
                            inputList.Add(number);
                        }
                        else
                        {
                            temp++;
                            sequences.Add("Sequence: " + temp, inputList);
                            inputList = new List<double>();
                            break;
                        }
                    }
                }
                if (inputList.Count > 0)
                {
                    temp++;
                    sequences.Add("Sequence: " + temp, inputList);
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


        /*<summary>
        This example demonstrates how to learn two sequences and how to use the prediction mechanism.
        First, two sequences are learned.
        Second, three short sequences with three elements each are created und used for prediction.The predictor used by experiment privides to the HTM every element of every predicting sequence.
        The predictor tries to predict the next element.
        </summary>*/
        private static void RunMultiSequenceLearningExperiment()
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));

            sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));
            sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));

            //
            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);

            //
            // These list are used to see how the prediction works.
            // Predictor is traversing the list element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.
            var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
            var list2 = new double[] { 2.0, 3.0, 4.0 };
            var list3 = new double[] { 8.0, 1.0, 2.0 };

            predictor.Reset();
            PredictNextElement(predictor, list1);

            predictor.Reset();
            PredictNextElement(predictor, list2);

            predictor.Reset();
            PredictNextElement(predictor, list3);
        }


        // Method to read subsequences input from text files
        public List<List<double>> GetSubSequencesInputFromTextFiles()
        {
            var SubSequences = new List<List<double>>();
            var TestSubSequences = new List<double>();

            using (StreamReader reader = new StreamReader(@"D:\Software_Engineering\Neocortex_MSL\neocortexapi_Team_MSL\source\MultiSequenceLearning_Team_MSL\Input_Files\Subsequence_input1"))
            {


                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine();
                    var numbers = row.Split(',');

                    foreach (var digit in numbers)
                    {
                        TestSubSequences.Add(Convert.ToDouble(digit));
                    }
                    SubSequences.Add(TestSubSequences);
                }
            }
            return SubSequences;
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
                    Debug.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                }
                else
                    Debug.WriteLine("Nothing predicted :(");
            }

            Debug.WriteLine("------------------------------");
        }
    }
}