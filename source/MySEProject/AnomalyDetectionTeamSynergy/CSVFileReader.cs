using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionTeamSynergy
{
    public class CSVFileReader
    {
        /// <summary>
        /// Reads a CSV file and returns the data as a list of lists of strings.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <param name="delimiter">The delimiter used in the CSV file. Defaults to ','</param>
        /// <param name="skipHeader">If true, skips the first row (header row) of the file. Defaults to true.</param>
        /// <returns>A list of lists of strings representing the CSV data.</returns>
        public List<List<string>> ReadCSVFile(string filePath, char delimiter = ',', bool skipHeader = true)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("The specified CSV file does not exist.");

            var csvData = new List<List<string>>();

            try
            {
                var lines = File.ReadLines(filePath).ToList();
                for (int i = skipHeader ? 1 : 0; i < lines.Count; i++)
                {
                    csvData.Add(new List<string>(lines[i].Split(delimiter)));
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading the file: {ex.Message}");
            }

            return csvData;
        }

        /// <summary>
        /// Displays the CSV data in a readable format in the console.
        /// </summary>
        /// <param name="csvData">The CSV data as a list of lists of strings.</param>

        public void DisplayCSVData(List<List<string>> csvData)
        {
            for (int i = 0; i < csvData.Count; i++)
            {
                Console.WriteLine($"Row {i + 1}: {string.Join(" | ", csvData[i])}");
            }
        }
        /// <summary>
        /// Parses sequences of numeric data from CSV data.
        /// Each row in the CSV is converted to a key-value pair, where the key is "Row_<row_number>" and the value is a list of numeric values.
        /// </summary>
        /// <param name="csvData">The CSV data as a list of lists of strings.</param>
        /// <returns>A dictionary where each key is a row identifier and each value is a list of numeric values.</returns>


        public Dictionary<string, List<double>> ParseSequencesFromCSV(List<List<string>> csvData)

        {
            var sequences = new Dictionary<string, List<double>>();
            for (int i = 0; i < csvData.Count; i++)

            {
                var row = csvData[i];

                // Skip rows with less than 2 columns
                if (row.Count < 2)
                {
                    Console.WriteLine($"Warning: Skipping malformed row: {string.Join(",", row)}");
                    continue;
                }

                string sequenceName = $"Row_{i + 1}";
                var numericValues = new List<double>();

                // Parse all numeric columns
                for (int j = 1; j < row.Count; j++) // Assuming columns after the first one contain numeric data
                {
                    if (double.TryParse(row[j], out double value))
                    {
                        numericValues.Add(value);
                    }
                }


                if (numericValues.Count > 0)
                {
                    sequences[sequenceName] = numericValues;
                }
                else
                {
                    Console.WriteLine($"Warning: Skipping malformed row: {string.Join(",", row)}");
                }
            }

            return sequences;
        }
    }
}

