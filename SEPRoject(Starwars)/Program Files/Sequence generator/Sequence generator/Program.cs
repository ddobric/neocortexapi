using System;
using System.IO;
using System.Text;

class Program
{
    static Random random = new Random();

    static string GenerateSequence(int length)
    {
        StringBuilder sequenceBuilder = new StringBuilder();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < length; i++)
        {
            sequenceBuilder.Append(chars[random.Next(chars.Length)]);
        }
        return sequenceBuilder.ToString();
    }

    static string[] GenerateMultiSequenceDataset(int numSequences, int minLength, int maxLength)
    {
        string[] dataset = new string[numSequences];
        for (int i = 0; i < numSequences; i++)
        {
            int length = random.Next(minLength, maxLength + 1);
            dataset[i] = GenerateSequence(length);
        }
        return dataset;
    }

    static void SaveDatasetToFile(string[] dataset, string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            for (int i = 0; i < dataset.Length; i++)
            {
                writer.WriteLine($"S{i + 1}: {dataset[i]}");
            }
        }
    }

    static void Main(string[] args)
    {
        int numSequences = 100;
        int minSequenceLength = 20;
        int maxSequenceLength = 50;

        string[] dataset = GenerateMultiSequenceDataset(numSequences, minSequenceLength, maxSequenceLength);

        SaveDatasetToFile(dataset, "multisequence_dataset_2.txt");
    }
}
