using System;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Reads a single CSV file and returns its contents as a list of sequences.
    /// </summary>
    public class CSVFileReader
    {
        private string _filePathToCSV;

        /// <summary>
        /// Creates a new instance of the CSVFileReader class with the provided file path to the constructor.
        /// </summary>
        /// <param name="filePathToCSV">The path to the CSV file to be read.</param>
        public CSVFileReader(string filePathToCSV)
        {
            _filePathToCSV = filePathToCSV;
        }

        /// <summary>
        /// Reads the CSV file at the file path specified in the constructor,
        /// and returns its contents as a list of sequences.
        /// </summary>
        /// <returns>A list of sequences contained in the CSV file.</returns>
        public List<List<double>> ReadFile()
        {
            List<List<double>> sequences = new List<List<double>>();
            string[] csvLines = File.ReadAllLines(_filePathToCSV);
            // Loop through each line in the CSV File
            for (int i = 0; i < csvLines.Length; i++)
            {
                // Current line is split into an array of columns
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
                        throw new ArgumentException($"Non-numeric value found! Please check the file.");
                    }
                }
                sequences.Add(sequence);
            }
            return sequences;
        }

        /// <summary>
        /// This method reads the CSV file at the file path passed on to the constructor,
        /// and outputs its contents to the console.
        /// </summary>
        public void CSVSequencesConsoleOutput()
        {
            List<List<double>> sequences = ReadFile();
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
