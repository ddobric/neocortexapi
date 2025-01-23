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
        // display csv data is readable format
        public void DisplayCSVData(List<List<string>> csvData)
        {
            for (int i = 0; i < csvData.Count; i++)
            {
                Console.WriteLine($"Row {i + 1}: {string.Join(" | ", csvData[i])}");
            }
        }


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
                for (int j = 1; j < row.Count; j++)
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

