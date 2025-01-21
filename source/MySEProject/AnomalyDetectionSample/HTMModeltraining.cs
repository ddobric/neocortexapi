using NeoCortexApi;
using System.Diagnostics;


namespace AnomalyDetectionSample
{
    public class HTMModeltraining
    {
        public void RunHTMModelLearning(string trainingfolderPath, string predictingfolderPath, out Predictor predictor)
        {

            // Using stopwatch to calculate the total training time
            Stopwatch swh = Stopwatch.StartNew();

            CSVFolderReader reader = new CSVFolderReader(trainingfolderPath);
            var sequences1 = reader.ReadFolder();

            CSVFolderReader reader1 = new CSVFolderReader(predictingfolderPath);
            var sequences2 = reader1.ReadFolder();

            List<List<double>> combinedSequences = new List<List<double>>(sequences1);
            combinedSequences.AddRange(sequences2);

            CSVToHTMInput converter = new CSVToHTMInput();
            var htmInput = converter.BuildHTMInput(combinedSequences);

            MultiSequenceLearning learning = new MultiSequenceLearning();
            predictor = learning.Run(htmInput);

            swh.Stop();

            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("HTM Model trained!! Training time is: " + swh.Elapsed.TotalSeconds + " seconds.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
        }

    }
}