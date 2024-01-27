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
        private const string DatasetFolder = "dataset";
        private const string ReportFolder = "report";
        private const string DatasetFileName = "dataset_03.json";
        private const string TestsetFileName = "test_01.json";

        static void Main(string[] args)
        {
            //to read Input Dataset
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            List<Sequence> sequences = ReadDataset(Path.Combine(basePath, DatasetFolder, DatasetFileName));
            //to read test dataset
            List<Sequence> sequencesTest = ReadDataset(Path.Combine(basePath, DatasetFolder, TestsetFileName));

            List<Report> reports = RunMultiSequenceLearningExperiment(sequences, sequencesTest);
            WriteReport(reports, basePath);
        }

        private static List<Sequence> ReadDataset(string datasetPath)
        {
            try
            {
                Console.WriteLine($"Reading Dataset: {datasetPath}");
                return JsonConvert.DeserializeObject<List<Sequence>>(File.ReadAllText(datasetPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading dataset: {ex.Message}");
                return new List<Sequence>();
            }
        }

        private static void WriteReport(List<Report> reports, string basePath)
        {
            string reportFolder = EnsureDirectory(Path.Combine(basePath, ReportFolder));
            string reportPath = Path.Combine(reportFolder, $"report_{DateTime.Now.Ticks}.txt");

            using (StreamWriter sw = File.CreateText(reportPath))
            {
                foreach (Report report in reports)
                {
                    WriteReportContent(sw, report);
                }
            }
        }

        private static string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private static void WriteReportContent(StreamWriter sw, Report report)
        {
            sw.WriteLine("------------------------------");
            sw.WriteLine($"Using test sequence: {report.SequenceName} -> {string.Join("-", report.SequenceData)}");
            foreach (string log in report.PredictionLog)
            {
                sw.WriteLine($"\t{log}");
            }
            sw.WriteLine($"\tAccuracy: {report.Accuracy}%");
            sw.WriteLine("------------------------------");
        }

        private static List<Report> RunMultiSequenceLearningExperiment(List<Sequence> sequences, List<Sequence> sequencesTest)
        {
            var reports = new List<Report>();
            var experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);

            foreach (Sequence item in sequencesTest)
            {
                var report = new Report
                {
                    SequenceName = item.name,
                    SequenceData = item.data
                };

                double accuracy = PredictNextElement(predictor, item.data, report);
                report.Accuracy = accuracy;
                reports.Add(report);

                Console.WriteLine($"Accuracy for {item.name} sequence: {accuracy}%");
            }

            return reports;
        }

        private static double PredictNextElement(Predictor predictor, int[] list, Report report)
        {
            int matchCount = 0, predictions = 0;
            List<string> logs = new List<string>();

            predictor.Reset();

            for (int i = 0; i < list.Length - 1; i++)
            {
                int current = list[i];
                int next = list[i + 1];

                logs.Add(PredictElement(predictor, current, next, ref matchCount));
                predictions++;
            }

            report.PredictionLog = logs;
            return CalculateAccuracy(matchCount, predictions);
        }

        private static string PredictElement(Predictor predictor, int current, int next, ref int matchCount)
        {
            Console.WriteLine($"Input: {current}");
            var predictions = predictor.Predict(current);
            if (predictions.Any())
            {
                var highestPrediction = predictions.OrderByDescending(p => p.Similarity).First();
                string predictedSequence = highestPrediction.PredictedInput.Split('-').First();
                int predictedNext = int.Parse(highestPrediction.PredictedInput.Split('-').Last());

                Console.WriteLine($"Predicted Sequence: {predictedSequence} - Predicted next element: {predictedNext}");
                if (predictedNext == next)
                    matchCount++;

                return $"Input: {current}, Predicted Sequence: {predictedSequence}, Predicted next element: {predictedNext}";
            }
            else
            {
                Console.WriteLine("Nothing predicted");
                return $"Input: {current}, Nothing predicted";
            }
        }

        private static double CalculateAccuracy(int matchCount, int predictions)
        {
            return (double)matchCount / predictions * 100;
        }
    }

    
}