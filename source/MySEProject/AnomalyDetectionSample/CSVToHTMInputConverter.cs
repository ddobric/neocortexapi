using System;
using System.Collections.Generic;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Converts a list of sequences into a dictionary suitable for HTM Engine training.
    /// </summary>
    public class CSVToHTMInputConverter
    {
        /// <summary>
        /// Transforms a list of sequences into a dictionary of sequences.
        /// A unique identifier is assigned as the key, which is utilized as input for HtmClassifier.
        /// </summary>
        /// <param name="inputSequences">List of sequences obtained from CSV file/files in a folder.</param>
        /// <returns>A dictionary of sequences formatted for HTM Engine training.</returns>
        public Dictionary<string, List<double>> ConvertToHTMInput(List<List<double>> inputSequences)
        {
            Dictionary<string, List<double>> sequenceDictionary = new Dictionary<string, List<double>>();

            for (int index = 0; index < inputSequences.Count; index++)
            {
                // Generate a unique identifier for the sequence
                string sequenceKey = "Sequence" + (index + 1);

                // Retrieve the sequence data
                List<double> sequenceData = inputSequences[index];

                // Add the sequence to the dictionary
                sequenceDictionary.Add(sequenceKey, sequenceData);
            }

            return sequenceDictionary;
        }
    }
}
