using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Utility class for reading and processing CSV files from a specified folder.
    /// </summary>
    public class CsvSequenceFolder
    {
        private readonly string _folderPath;

        /// <summary>
        /// Initializes a new instance of the CsvSequenceProcessor class with the provided folder path.
        /// </summary>
        /// <param name="folderPath">The path to the folder containing the CSV files.</param>
        public CsvSequenceFolder(string folderPath)
        {
            _folderPath = folderPath;
        }

        /// <summary>
        /// Reads all CSV files in the specified folder and returns their contents as a list of sequences.
        /// </summary>
        /// <returns>A list of sequences contained in the CSV files present in the folder.</returns>
        public List<List<double>> ExtractSequencesFromFolder()
        {
            List<List<double>> folderSequences = new List<List<double>>();
            string[] fileEntries = Directory.GetFiles(_folderPath, "*.csv");

            foreach (string fileName in fileEntries)
            {
                string[] csvLines = File.ReadAllLines(fileName);
                List<List<double>> sequencesInFile = new List<List<double>>();

                foreach (string line in csvLines)
                {
                    string[] columns = line.Split(',');
                    List<double> sequence = new List<double>();

                    foreach (string column in columns)
                    {
                        if (double.TryParse(column, out double value))
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
        /// Outputs the sequences extracted from the CSV files in the specified folder to the console.
        /// </summary>
        public void DisplayCsvSequences()
        {
            List<List<double>> sequences = ExtractSequencesFromFolder();

            for (int i = 0; i < sequences.Count; i++)
            {
                Console.Write($"Sequence {i + 1}: ");
                foreach (double number in sequences[i])
                {
                    Console.Write($"{number} ");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Trims a random number of elements (between 1 and 4) from the beginning of each sequence in a list of sequences.
        /// </summary>
        /// <param name="sequences">The list of sequences to trim.</param>
        /// <returns>A new list of trimmed sequences.</returns>
        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            Random random = new Random();
            List<List<double>> trimmedSequences = new List<List<double>>();

            foreach (List<double> sequence in sequences)
            {
                int numElementsToRemove = random.Next(1, 5);
                List<double> trimmedSequence = sequence.Skip(numElementsToRemove).ToList();
                trimmedSequences.Add(trimmedSequence);
            }

            return trimmedSequences;
        }
    }
}
