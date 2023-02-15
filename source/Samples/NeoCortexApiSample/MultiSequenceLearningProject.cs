using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApiSample;

namespace MultiSequencePrediction
{
    class Project
    {
        public Predictor PredictionExperiment()
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();
            /*Code for reading the learning sequences from .txt file. The file has n rows which have numbers seperated by commas.*/
            //string path = ".//.//" + System.IO.Directory.GetCurrent‌​Directory();
            string seqPath = @"..\..\..\..\..\MySEProject/trainingSequences.txt";
            string sequencePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), seqPath));
            sequences = readSequences(sequencePath);
            MultiSequenceLearning newExperiment = new MultiSequenceLearning();
            var predictor = newExperiment.Run(sequences);

            //Testing sequences
            string tpaths = @"..\..\..\..\..\MySEProject/testingData.txt";
            string testDataPath = Path.GetFullPath(Path.Combine(Directory.GetCurrent‌​Directory(), tpaths));
            var testSequences = new List<List<double>>();
            testSequences = readTestSequences(tpaths);



            return (null);
        }
        public Dictionary<string, List<double>> readSequences(string sequencePath)
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();
            var sequence = new List<double>();
            using (StreamReader reader = new(sequencePath))
            {
                int count = 1;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Console.WriteLine(line);

                    foreach (var value in values)
                    {
                        // sequence.Add(Convert.ToDouble(value));
                        sequence.Add(double.Parse(value));
                    }
                    string seqName = "seq" + count;
                    sequences.Add(seqName, sequence);
                    count++;

                }
                return sequences;
            }
        }
        public List<List<double>> readTestSequences(string path)
        {
            var testSequences = new List<List<double>>();
            var testList = new List<double>();


            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Console.WriteLine(line);

                    foreach (var value in values)
                    {
                        testList.Add(Convert.ToDouble(value));
                    }
                    testSequences.Add(testList);
                }
            }
            return testSequences;
        }
    }
}
