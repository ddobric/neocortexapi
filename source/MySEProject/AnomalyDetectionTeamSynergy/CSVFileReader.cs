using System;
using System.Collections.Generic;
using System.IO;

namespace CSVFileHandling
{
    public class CSVFileReader
    {
        public List<List<string>> ReadCSVFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("The specified CSV file does not exist.");

            var csvData = new List<List<string>>();

            try
            {
                foreach (var line in File.ReadLines(filePath))
                    csvData.Add(new List<string>(line.Split(',')));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading the file: {ex.Message}");
            }

            return csvData;
        }

        public void DisplayCSVData(List<List<string>> csvData)
        {
            for (int i = 0; i < csvData.Count; i++)
                Console.WriteLine($"Row {i + 1}: {string.Join(" | ", csvData[i])}");
        }

        public Dictionary<string, List<double>> ParseSequencesFromCSV(List<List<string>> csvData)
        {
            var sequences = new Dictionary<string, List<double>>();

            foreach (var row in csvData)
            {
                if (row.Count >= 2 && double.TryParse(row[1], out double value))
                {
                    string sequenceName = row[0];
                    if (!sequences.ContainsKey(sequenceName))
                        sequences[sequenceName] = new List<double>();
                    sequences[sequenceName].Add(value);
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
