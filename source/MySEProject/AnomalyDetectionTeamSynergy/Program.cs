namespace AnomalyDetectionTeamSynergy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            string defaultTrainingFolder = Path.Combine(projectbaseDirectory, "TrainingData");
            string defaultInferringFolder = Path.Combine(projectbaseDirectory, "InferringData");

            Console.WriteLine("Training Data Path: " + defaultTrainingFolder);
            Console.WriteLine("Inferring Data Path: " + defaultInferringFolder);

            try
            {
                // Check if folder exists
                if (!Directory.Exists(defaultTrainingFolder))
                {
                    throw new DirectoryNotFoundException("The specified folder does not exist.");
                }

                // Get all CSV files in the folder
                string[] csvFiles = Directory.GetFiles(defaultTrainingFolder, "*.csv");

                if (csvFiles.Length == 0)
                {
                    throw new FileNotFoundException("No CSV files found in the specified folder.");
                }

                // Create an instance of the CSVFileReader class and CSVToHTMInput
                var csvReader = new CSVFileReader();
                var htmInputConverter = new CSVToHTMInput();
                var sequences = new Dictionary<string, List<double>>();

                // Create an instance of the CSVFilesFolderReader class
                //var csvReader = new CSVFileReader();


                foreach (var filePath in csvFiles)
                {
                    Console.WriteLine($"\n--- Reading File: {Path.GetFileName(filePath)} ---");

                    // Read and display the CSV data
                    var csvData = csvReader.ReadCSVFile(filePath);
                    csvReader.DisplayCSVData(csvData);

                    // Parse sequences from CSV
                    var parsedSequences = csvReader.ParseSequencesFromCSV(csvData);
                    foreach (var seq in parsedSequences)
                    {
                        if (!sequences.ContainsKey(seq.Key))
                            sequences.Add(seq.Key, seq.Value);
                    }
                }


                // Convert the parsed sequences into HTM input
                Console.WriteLine("\n--- Converting Sequences to HTM Input Format ---");
                var htmInput = htmInputConverter.BuildHTMInput(new List<List<double>>(sequences.Values));

                // Display the HTM input dictionary
                foreach (var entry in htmInput)
                {
                    Console.WriteLine($"Key: {entry.Key}, Values: {string.Join(", ", entry.Value)}");
                }
                // Here you can pass the HTM input to your HTM engine or learning algorithm
                // Example:
                // var experiment = new MultiSequenceLearning();
                // experiment.Run(htmInput);

                // Now run the learning experiment with anomaly detection
                var experiment = new MultiSequenceLearning();
             //  experiment.Run(sequences);
               experiment.Run(htmInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
          //  RunMultiSequenceLearningExperiment.Run();
        }
       
    }
}
