using NeoCortexApi;
using System.Diagnostics;


namespace AnomalyDetectionSample
{
    public class HTMModeltraining
    {
        /// <summary>
        /// Runs the HTM model learning experiment on folders containing CSV files, and returns the trained model.
        /// </summary>
        /// <param name="trainingfolderPath">The path to the folder containing the CSV files to be used for training(learning).</param>
        /// <param name="predictingfolderPath">The path to the folder containing the CSV files which contains sequences for prediction.</param>
        /// <param name="predictor">The trained model that will be used for prediction.</param>
        public void RunHTMModelLearning(string trainingfolderPath, string predictingfolderPath, out Predictor predictor)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Starting our anomaly detection experiment!!");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("HTM Model training initiated...................");
            // Using stopwatch to calculate the total training time
            Stopwatch swh = Stopwatch.StartNew();

            // Read numerical sequences from CSV files in the specified folder containing files having training(learning) sequences
            // CSVFileReader class can also be used for single files
            CSVFolderReader reader = new CSVFolderReader(trainingfolderPath);
            var sequences1 = reader.ReadFolder();

            // Read numerical sequences from CSV files in the specified folder containing files having prediction sequences
            // CSVFileReader class can also be used for single files
            CSVFolderReader reader1 = new CSVFolderReader(predictingfolderPath);
            var sequences2 = reader1.ReadFolder();

            // Combine these sequences for using both training(learning) and predicting sequences
            // We will use both of them to feed into HTM Model for training
            List<List<double>> combinedSequences = new List<List<double>>(sequences1);
            combinedSequences.AddRange(sequences2);

            // Convert sequences to HTM input format
            CSVToHTMInput converter = new CSVToHTMInput();
            var htmInput = converter.BuildHTMInput(combinedSequences);

            // Starting multi-sequence learning experiment to generate predictor model
            // by passing htmInput 
            MultiSequenceLearning learning = new MultiSequenceLearning();
            predictor = learning.Run(htmInput);

            // Our HTM model training concludes here

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
