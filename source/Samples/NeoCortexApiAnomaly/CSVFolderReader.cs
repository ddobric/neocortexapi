using System;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Reads the CSV files inside a folder and returns its contents as a list of sequences.
    /// </summary>
    public class CSVFolderReader
    {
        private string _folderPathToCSV;

        /// <summary>
        /// Creates a new instance of the CSVFolderReader class with the provided file path to the constructor.
        /// </summary>
        /// <param name="folderPathToCSV">The path to the folder containing the CSV files.</param>
        public CSVFolderReader(string folderPathToCSV)
        {
            _folderPathToCSV = folderPathToCSV;
        }

        /// <summary>
        /// Reads all CSV files in the folder, path to the folder is specified in the constructor,
        /// and returns its contents as a list of sequences.
        /// </summary>
        /// <returns>A list of sequences contained in the CSV files present in the folder.</returns>
        public List<List<double>> ReadFolder()
        {
            List<List<double>> folderSequences = new List<List<double>>();

            // All the CSV files present inside the folder are taken
            string[] fileEntries = Directory.GetFiles(_folderPathToCSV, "*.csv");

            // Iterating through each CSV file inside the folder
            foreach (string fileName in fileEntries)
            {
                string[] csvLines = File.ReadAllLines(fileName);
                List<List<double>> sequencesInFile = new List<List<double>>();

                // Looping through each line in the current CSV file
                for (int i = 0; i < csvLines.Length; i++)
                {
                    string[] columns = csvLines[i].Split(new char[] { ',' });
                    List<double> sequence = new List<double>();

                    // Loop through each column in the current line
                    for (int j = 0; j < columns.Length; j++)
                    {
                        // Value of column is parsed as double and added to sequence
                        // if it fails then exception is thrown
                        if (double.TryParse(columns[j], out double value))
                        {
                            sequence.Add(value);
                        }
                        else
                        {
                            throw new ArgumentException($"Non-numeric value found! Please check file: {fileName}.");
                        }
                    }
                    sequencesInFile.Add(sequence);
                }
                folderSequences.AddRange(sequencesInFile);
            }
            return folderSequences;
        }

        /// <summary>
        /// This method reads all CSV files in the folder path passed on to the constructor,
        /// and outputs its contents to the console.
        /// </summary>
        public void CSVSequencesConsoleOutput()
        {
            List<List<double>> sequences = ReadFolder();
            // Looping through each sequence and displaying it in the console
            for (int i = 0; i < sequences.Count; i++)
            {
                Console.Write("Sequence " + (i + 1) + ": ");
                foreach (double number in sequences[i])
                {
                    Console.Write(number + " ");
                }
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Trims a random number of elements (between 1 and 4) from the beginning of each sequence in a list of sequences.
        /// </summary>
        /// <param name="sequences">The list of sequences to trim.</param>
        /// <returns>A new list of trimmed sequences.</returns>
        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            Random rnd = new Random();
            List<List<double>> trimmedSequences = new List<List<double>>();

            foreach (List<double> sequence in sequences)
            {
                // Generate a random number between 1 and 4
                int numElementsToRemove = rnd.Next(1, 5);
                List<double> trimmedSequence = sequence.Skip(numElementsToRemove).ToList();
                trimmedSequences.Add(trimmedSequence);
            }

            return trimmedSequences;
        }

    }
}
