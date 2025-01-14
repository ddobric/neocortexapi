using System;
using System.Collections.Generic;
using System.IO;
using CSVFileHandling;


namespace AnomalyDetectionTeamSynergy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("Enter the full path to the folder containing CSV files:");
            string folderPath = Console.ReadLine();

            try
            {
                // Check if folder exists
                if (!Directory.Exists(folderPath))
                {
                    throw new DirectoryNotFoundException("The specified folder does not exist.");
                }

                // Get all CSV files in the folder
                string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");

                if (csvFiles.Length == 0)
                {
                    throw new FileNotFoundException("No CSV files found in the specified folder.");
                }

                // Create an instance of the CSVFileReader class
                var csvReader = new CSVFileReader();

                foreach (var filePath in csvFiles)
                {
                    Console.WriteLine($"\n--- Reading File: {Path.GetFileName(filePath)} ---");

                    // Read and display the CSV data
                    var csvData = csvReader.ReadCSVFile(filePath);
                    csvReader.DisplayCSVData(csvData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
