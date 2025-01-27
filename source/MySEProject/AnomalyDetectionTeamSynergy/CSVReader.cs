using System.Globalization;

namespace AnomalyDetectionTeamSynergy
{
    public class CSVReader
    {
        // Method to parse sequences from a CSV file
        public List<List<double>> ParseSequencesFromCSV(string filePath)
        {
            var sequences = new List<List<double>>();

            // Read all lines from the CSV file
            var lines = File.ReadAllLines(filePath);

            // Skip the first line (header) and iterate through the remaining lines
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                // Split the line by tab or whitespace
                var values = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // Try to convert all values to double
                if (values.All(v => double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out _)))
                {
                    var sequence = values.Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToList();

                    // Skip sequences with only two values
                    if (sequence.Count > 2)
                    {
                        sequences.Add(sequence);
                    }
                }
            }

            return sequences;
        }

        // Method to display sequence data
        public void DisplaySequenceData(List<List<double>> sequences)
        {
            for (int i = 0; i < sequences.Count; i++)
            {
                Console.WriteLine($"Sequence {i + 1}: {string.Join(", ", sequences[i])}");
            }
        }
    }
}