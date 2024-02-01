using System;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Converts a list of sequences to a dictionary of sequences for facilitating HTM Engine training.
    /// </summary>
    public class CSVToHTMInput
    {
        /// <summary>
        /// Builds a dictionary of sequences from a list of sequences.
        /// An unique key is added, which is later used as an input for HtmClassifier.
        /// </summary>
        /// <param name="sequences">A list of sequences read from CSV file/files in a folder.</param>
        /// <returns>A dictionary of sequences required for HTM Engine training.</returns>
        public Dictionary<string, List<double>> BuildHTMInput(List<List<double>> sequences)
        {
            Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
            for (int i = 0; i < sequences.Count; i++)
            {
                // Unique key created and added to dictionary for HTM Input                
                string key = "S" + (i + 1);
                List<double> value = sequences[i];
                dictionary.Add(key, value);
            }
            return dictionary;
        }
    }
}
