namespace AnomalyDetectionTeamSynergy
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
        public Dictionary<string, List<double>> BuildHTMInput(List<List<double>> sequences, string keyPrefix = "S")
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences), "Sequences list cannot be null.");

            if (sequences.Any(seq => seq == null))
                throw new ArgumentException("One or more sequences in the list are null.", nameof(sequences));

            var dictionary = new Dictionary<string, List<double>>(sequences.Count);

            for (int i = 0; i < sequences.Count; i++)
            {
                string key = $"{keyPrefix}{i + 1}";
                dictionary[key] = sequences[i];
            }

            return dictionary;
        }
    }
}
