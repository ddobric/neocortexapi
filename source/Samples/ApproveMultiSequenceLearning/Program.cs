using NeoCortexApi;
using NeoCortexApi.Encoders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MultiSequenceLearning.MultiSequenceLearning;

namespace MultiSequenceLearning
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

            //to create synthetic dataset
            /*string path = HelperMethods.SaveDataset(HelperMethods.CreateDataset());
            Console.WriteLine($"Dataset saved: {path}");*/

            //to read dataset
            string BasePath = AppDomain.CurrentDomain.BaseDirectory;
            string datasetPath = Path.Combine(BasePath, "dataset", "dataset_03.json");
            Console.WriteLine($"Reading Dataset: {datasetPath}");
            List<Sequence> sequences = HelperMethods.ReadDataset(datasetPath);

            //to read test dataset
            string testsetPath = Path.Combine(BasePath, "dataset", "test_01.json");
            Console.WriteLine($"Reading Testset: {testsetPath}");
            List<Sequence> sequencesTest = HelperMethods.ReadDataset(testsetPath);

            //run learing only
            //RunSimpleMultiSequenceLearningExperiment(sequences);

            //run learning + prediction and generates report for results
            List<Report> reports = RunMultiSequenceLearningExperiment(sequences, sequencesTest);

            WriteReport(sequences, reports);

            Console.WriteLine("Done...");

        }

        /// <summary>
        /// write and formats data in report object to a file
        /// </summary>
        /// <param name="sequences">input sequence</param>
        /// <param name="reports">object of report</param>
        private static void WriteReport(List<Sequence> sequences, List<Report> reports)
        {
            string BasePath = AppDomain.CurrentDomain.BaseDirectory;
            string reportFolder = Path.Combine(BasePath, "report");
            if (!Directory.Exists(reportFolder))
                Directory.CreateDirectory(reportFolder);
            string reportPath = Path.Combine(reportFolder, $"report_{DateTime.Now.Ticks}.txt");

            if (!File.Exists(reportPath))
            {
                using (StreamWriter sw = File.CreateText(reportPath))
                {
                    sw.WriteLine("------------------------------");
                    foreach (Sequence sequence in sequences)
                    {
                        sw.WriteLine($"Sequence: {sequence.name} -> {string.Join("-",sequence.data)}");
                    }
                    sw.WriteLine("------------------------------");
                    foreach (Report report in reports)
                    {
                        sw.WriteLine($"Using test sequence: {report.SequenceName} -> {string.Join("-",report.SequenceData)}");
                        foreach (string log in report.PredictionLog)
                        {
                            sw.WriteLine($"\t{log}");
                        }
                        sw.WriteLine($"\tAccuracy: {report.Accuracy}%");
                        sw.WriteLine("------------------------------");
                    }
                }
            }

        }

        /// <summary>
        /// takes input data set and runs the alogrithm
        /// </summary>
        /// <param name="sequences">input test dataset</param>
        private static void RunSimpleMultiSequenceLearningExperiment(List<Sequence> sequences)
        {
            //
            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);
        }


        /// <summary>
        /// This example demonstrates how to learn two sequences and how to use the prediction mechanism.
        /// First, two sequences are learned.
        /// Second, three short sequences with three elements each are created und used for prediction. The predictor used by experiment privides to the HTM every element of every predicting sequence.
        /// The predictor tries to predict the next element.
        /// </summary>
        /// <param name="sequences">input dataset</param>
        /// <param name="sequencesTest">input test dataset</param>
        /// <returns>list of Report per sequence</returns>
        private static List<Report> RunMultiSequenceLearningExperiment(List<Sequence> sequences, List<Sequence> sequencesTest)
        {
            List<Report> reports = new List<Report>();
            Report report = new Report();

            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);

            // These list are used to see how the prediction works.
            // Predictor is traversing the list element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.

            foreach (Sequence item in sequencesTest)
            {
                report.SequenceName = item.name;
                Debug.WriteLine($"Using test sequence: {item.name}");
                Console.WriteLine("------------------------------");
                Console.WriteLine($"Using test sequence: {item.name}");
                predictor.Reset();
                report.SequenceData = item.data;
                var accuracy = PredictNextElement(predictor, item.data, report);
                reports.Add(report);
                Console.WriteLine($"Accuracy for {item.name} sequence: {accuracy}%");
            }

            return reports;

        }

        /// <summary>
        /// Takes predicted model, subsequence and generates report stating accuracy
        /// </summary>
        /// <param name="predictor">Object of Predictor</param>
        /// <param name="list">sub-sequence to be tested</param>
        /// <returns>accuracy of predicting elements in %</returns>
        private static double PredictNextElement(Predictor predictor, int[] list, Report report)
        {
            int matchCount = 0;
            int predictions = 0;
            double accuracy = 0.0;
            List<string> logs = new List<string>();
            Console.WriteLine("------------------------------");

            int prev = -1;
            bool first = true;

            /*
             * Pseudo code for calculating accuracy:
             * 
             * 1.      loop for each element in the sub-sequence
             * 1.1     if the element is first element do nothing and save the element as 'previous' for further comparision
             * 1.2     take previous element and predict the next element
             * 1.2.1   get the predicted element with highest similarity and compare with 'next' element
             * 1.2.1.1 if predicted element matches the next element increment the counter of matched elements
             * 1.2.2   increment the count for number of calls made to predict an element
             * 1.2     update the 'previous' element with 'next' element
             * 2.      calculate the accuracy as (number of matched elements)/(total number of calls for prediction) * 100
             * 3.      end the loop
             */

            foreach (var next in list)
            {
                if(first)
                {
                    first = false;
                }
                else
                {
                    Console.WriteLine($"Input: {prev}");
                    var res = predictor.Predict(prev);
                    string log = "";
                    if (res.Count > 0)
                    {
                        foreach (var pred in res)
                        {
                            Debug.WriteLine($"Predicted Input: {pred.PredictedInput} - Similarity: {pred.Similarity}%");
                        }

                        var sequence = res.First().PredictedInput.Split('_');
                        var prediction = res.First().PredictedInput.Split('-');
                        Console.WriteLine($"Predicted Sequence: {sequence.First()} - Predicted next element: {prediction.Last()}");
                        log = $"Input: {prev}, Predicted Sequence: {sequence.First()}, Predicted next element: {prediction.Last()}";
                        //compare current element with prediction of previous element
                        if(next == Int32.Parse(prediction.Last()))
                        {
                            matchCount++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nothing predicted :(");
                        log = $"Input: {prev}, Nothing predicted";
                    }

                    logs.Add(log);
                    predictions++;
                }

                //save previous element to compare with upcoming element
                prev = next;
            }

            report.PredictionLog = logs;

            /*
             * Accuracy is calculated as number of matching predictions made 
             * divided by total number of prediction made for an element in subsequence
             * 
             * accuracy = number of matching predictions/total number of prediction * 100
             */
            accuracy = (double)matchCount / predictions * 100;
            report.Accuracy = accuracy;
            Console.WriteLine("------------------------------");

            return accuracy;
        }
    }
}
