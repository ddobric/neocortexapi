using System;
using System.Collections.Generic;
using System.IO;

namespace CSVFileHandling
{
    /// <summary>
    /// A class for reading and processing CSV files.
    /// </summary>
    public class CSVFileReader
    {
        /// <summary>
        /// Reads a CSV file and returns its content as a list of rows, where each row is a list of column values.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <returns>A list of rows, where each row is a list of string values.</returns>
        public List<List<string>> ReadCSVFile(string filePath)
        {
            var csvData = new List<List<string>>();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified CSV file does not exist.");
            }

            try
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    // Split the line into columns based on commas
                    var columns = new List<string>(line.Split(','));
                    csvData.Add(columns);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while reading the file: {ex.Message}");
            }

            return csvData;
        }

        /// <summary>
        /// Displays the content of a CSV file on the console.
        /// </summary>
        /// <param name="csvData">The list of rows containing CSV data.</param>
        public void DisplayCSVData(List<List<string>> csvData)
        {
            for (int i = 0; i < csvData.Count; i++)
            {
                Console.WriteLine($"Row {i + 1}: {string.Join("   |   ", csvData[i])}");
            }
        }
    }
}
