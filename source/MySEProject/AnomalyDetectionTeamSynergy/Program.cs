
using CSVFileHandling;


namespace AnomalyDetectionTeamSynergy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("Enter the full path to the CSV file:");
            string filePath = Console.ReadLine();

            try
            {
                // Create an instance of the CSVFileReader class
                var csvReader = new CSVFileReader();

                // Read the CSV file and get its content
                var csvData = csvReader.ReadCSVFile(filePath);

                // Display the content of the CSV file
                Console.WriteLine("\n--- CSV File Data ---");
                csvReader.DisplayCSVData(csvData);
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
