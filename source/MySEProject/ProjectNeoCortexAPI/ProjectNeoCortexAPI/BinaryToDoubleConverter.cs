using System;
namespace ProjectNeoCortexAPI
{
    // <summary>
    /// Class for converting Binary Sparse Distributed Representation (SDR) to double value.
    /// </summary>
    public class BinaryToDoubleConverter
    {
        // <summary>
        /// Reads binarized files and prints their double values.
        /// </summary>
        public static void PrintDoubleValuesFromBinarizedFiles()
        {
            string outputFolderPath = ".\\OutputFiles";

            // Check if folder exists
            if (!Directory.Exists(outputFolderPath))
            {
                Console.WriteLine($"Folder {outputFolderPath} does not exist. Please provide a valid path.");
                return;
            }

            Console.WriteLine("Processing binarized files to compute double values...");

            // Process each file in the folder
            foreach (var filePath in Directory.GetFiles(outputFolderPath, "*.txt"))
            {
                // Read the binarized values from the file
                int[] binarySDR = ReadBinarizedFile(filePath);

                if (binarySDR.Length == 0)
                {
                    Console.WriteLine("No binary data found in file.");
                    continue;
                }

                // Convert binary SDR to double
                double doubleValue = ConvertToDouble(binarySDR);

                // Output the result
                Console.WriteLine($"Converted Double Value for {Path.GetFileName(filePath)} is {doubleValue}\n");
            }
        }
        /// <summary>
        /// Reads a binarized file and converts its content to an integer array.
        /// </summary>
        /// <param name="filePath">The path to the binarized file.</param>
        /// <returns>An integer array representing the binary SDR.</returns>
        private static int[] ReadBinarizedFile(string filePath)
        {
            try
            {
                // Read all lines from the file
                var lines = File.ReadAllLines(filePath);

                // Parse each line and split into individual binary digits
                var binaryValues = lines
                    .SelectMany(line => line.Trim()
                        .Select(ch =>
                        {
                            // Validate each character as '0' or '1'
                            if (ch == '0' || ch == '1')
                                return ch - '0'; // Convert char to int (ASCII difference)
                            else
                            {
                                Console.WriteLine($"Invalid binary character '{ch}' in file {filePath}. Skipping...");
                                return -1; // Invalid values marked as -1
                            }
                        })
                        .Where(value => value != -1)) // Exclude invalid values
                    .ToArray();

                return binaryValues;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
                return Array.Empty<int>();
            }
        }
        /// <summary>
        /// Converts a Binary SDR to its equivalent double value.
        /// </summary>
        /// <param name="binarySDR">The Binary SDR array.</param>
        /// <returns>The double value representing the Binary SDR.</returns>
        private static double ConvertToDouble(int[] binarySDR)
        {
            double value = 0;
            for (int i = 0; i < binarySDR.Length; i++)
            {
                // Add the binary value (0 or 1) multiplied by 2 raised to the negative power of the index
                value += binarySDR[i] * Math.Pow(2, -(i + 1));
            }
            return value;
        }
    }
}