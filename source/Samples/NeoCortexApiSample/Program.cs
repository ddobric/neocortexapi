using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using NeoCortexApi;
using System.Linq;

namespace NeoCortexApiSample
{
    class Program
    {
       

        /// <summary>
        /// This sample shows a typical experiment code for SP and TM
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // SE Project: Approve Prediction of Multisequence Learning 

            // Starts experiment that demonstrates how to learn spatial patterns.

            //to creating synthetic dataset
            //string path = HelpMethod.SaveDataset(HelpMethod.CreateDataset());
            //Console.WriteLine($"Dataset saved: {path}");

            //to read dataset
            string BasePath = AppDomain.CurrentDomain.BaseDirectory;
            string datasetPath = Path.Combine(BasePath, "dataset", "dataset_03.json");
            Console.WriteLine($"Reading Dataset: {datasetPath}");
            List<Sequence> sequences = HelpMethod.ReadDataset(datasetPath);

            //to read test dataset
            string testsetPath = Path.Combine(BasePath, "dataset", "test_01.json");
            Console.WriteLine($"Reading Testset: {testsetPath}");
            List<Sequence> sequencesTest = HelpMethod.ReadDataset(testsetPath);


            //run learing  part only
            //RunSimpleMultiSequenceLearningExperiment(sequences);

            //run learning + prediction and generates report for results
            List<Report> reports = RunMultiSequenceLearningExperiment(sequences, sequencesTest);

            WriteReport(sequences, reports);

            Console.WriteLine("Done...");

        }

        /*private static List<Report> RunMultiSequenceLearningExperiment(List<Sequence> sequences, List<Sequence> sequencesTest)
        {
            throw new NotImplementedException();
        }*/

        // <summary>
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
        }
        private static void RunSimpleMultiSequenceLearningExperiment(List<Sequence> sequences)
        {
            // Prototype for building the prediction engine.
            //List<Report> reports = new List<Report>();
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);
        }


        /// <summary>
        /// This example demonstrates how to learn two sequences and how to use the prediction mechanism.
        /// First, two sequences are learned.
        /// Second, three short sequences with three elements each are created und used for prediction. The predictor used by experiment privides to the HTM every element of every predicting sequence.
        /// The predictor tries to predict the next element.
        /// </summary>



        private static void RunMultiSequenceLearningExperiment()
        {

            //
            // Prototype for building the prediction engine.
            List<Report> reports = new List<Report>();
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            var predictor = experiment.Run(sequences);

            //
            // These list are used to see how the prediction works.
            // Predictor is traversing the list element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.
            /*var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
            var list2 = new double[] { 2.0, 3.0, 4.0 };
            var list3 = new double[] { 8.0, 1.0, 2.0 };

            predictor.Reset();
            PredictNextElement(predictor, list1);

            predictor.Reset();
            PredictNextElement(predictor, list2);

            predictor.Reset();
            PredictNextElement(predictor, list3);*/
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


