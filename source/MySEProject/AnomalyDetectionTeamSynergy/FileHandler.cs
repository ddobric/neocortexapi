using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Handles file operations for training and inferring data in a system that processes CSV files.
/// </summary>
public class FileHandler
{
    // Default folders for training and inferring data.
    private readonly string defaultTrainingFolder;
    private readonly string defaultInferringFolder;

    /// <summary>
    /// List of validated training data files.
    /// </summary>
    public List<string> TrainingDataFiles { get; private set; } = new List<string>();

    /// <summary>
    /// List of validated inferring data files.
    /// </summary>
    public List<string> InferringDataFiles { get; private set; } = new List<string>();

    /// <summary>
    /// Initializes the FileHandler with specified default folders.
    /// </summary>
    /// <param name="defaultTrainingFolder">Path to the default training folder.</param>
    /// <param name="defaultInferringFolder">Path to the default inferring folder.</param>
    public FileHandler(string defaultTrainingFolder, string defaultInferringFolder)
    {
        this.defaultTrainingFolder = defaultTrainingFolder;
        this.defaultInferringFolder = defaultInferringFolder;
    }

    /// <summary>
    /// Processes command-line arguments to determine training and inferring data sources.
    /// </summary>
    /// <param name="args">Array of command-line arguments.</param>
    /// <exception cref="Exception">Thrown when no valid training CSV files are found.</exception>
    public void ProcessArguments(string[] args)
    {
        var allTrainingFiles = new List<string>();
        var allInferringFiles = new List<string>();

        string? trainingFile = null;
        string? inferringFile = null;
        string? trainingFolder = null;
        string? inferringFolder = null;

        Console.WriteLine("Parsing command-line arguments...");

        // Parse console arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--training-file":
                    trainingFile = i + 1 < args.Length ? args[i + 1] : null;
                    break;
                case "--inferring-file":
                    inferringFile = i + 1 < args.Length ? args[i + 1] : null;
                    break;
                case "--training-folder":
                    trainingFolder = i + 1 < args.Length ? args[i + 1] : null;
                    break;
                case "--inferring-folder":
                    inferringFolder = i + 1 < args.Length ? args[i + 1] : null;
                    break;
            }
        }

        // Gather all training files
        Console.WriteLine("Gathering training data files...");
        if (!string.IsNullOrEmpty(trainingFile))
        {
            Console.WriteLine($"Adding training file: {trainingFile}");
            allTrainingFiles.Add(trainingFile);
        }

        if (!string.IsNullOrEmpty(trainingFolder) && Directory.Exists(trainingFolder))
        {
            Console.WriteLine($"Adding files from training folder: {trainingFolder}");
            allTrainingFiles.AddRange(Directory.GetFiles(trainingFolder));
        }

        if (string.IsNullOrEmpty(trainingFile) && string.IsNullOrEmpty(trainingFolder))
        {
            Console.WriteLine("Using default training folder...");
            if (Directory.Exists(defaultTrainingFolder))
            {
                allTrainingFiles.AddRange(Directory.GetFiles(defaultTrainingFolder));
            }
        }

        // Gather all inferring files
        Console.WriteLine("Gathering inferring data files...");
        if (!string.IsNullOrEmpty(inferringFile))
        {
            Console.WriteLine($"Adding inferring file: {inferringFile}");
            allInferringFiles.Add(inferringFile);
        }

        if (!string.IsNullOrEmpty(inferringFolder) && Directory.Exists(inferringFolder))
        {
            Console.WriteLine($"Adding files from inferring folder: {inferringFolder}");
            allInferringFiles.AddRange(Directory.GetFiles(inferringFolder));
        }

        if (string.IsNullOrEmpty(inferringFile) && string.IsNullOrEmpty(inferringFolder))
        {
            Console.WriteLine("Using default inferring folder...");
            if (Directory.Exists(defaultInferringFolder))
            {
                allInferringFiles.AddRange(Directory.GetFiles(defaultInferringFolder));
            }
        }

        // Validate files and filter for CSVs
        Console.WriteLine("Validating and filtering training files...");
        TrainingDataFiles = ValidateAndFilterFiles(allTrainingFiles);

        Console.WriteLine("Validating and filtering inferring files...");
        InferringDataFiles = ValidateAndFilterFiles(allInferringFiles);

        if (TrainingDataFiles.Count == 0)
        {
            Console.WriteLine("Error: No valid training CSV files found.");
            throw new Exception("No valid training CSV files found. Program will terminate.");
        }

        Console.WriteLine("Training Data Files:");
        TrainingDataFiles.ForEach(file => Console.WriteLine($"  {file}"));

        Console.WriteLine("\nInferring Data Files:");
        InferringDataFiles.ForEach(file => Console.WriteLine($"  {file}"));
    }

    /// <summary>
    /// Validates a list of file paths and filters only valid CSV files.
    /// </summary>
    /// <param name="files">List of file paths to validate.</param>
    /// <returns>List of valid CSV file paths.</returns>
    private List<string> ValidateAndFilterFiles(List<string> files)
    {
        var validFiles = new List<string>();

        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"Warning: File not found - {file}");
                continue;
            }

            if (!IsCsv(file))
            {
                Console.WriteLine($"Warning: File is not a CSV - {file}");
                continue;
            }

            validFiles.Add(file);
        }

        return validFiles;
    }

    /// <summary>
    /// Checks if a given file path has a .csv extension.
    /// </summary>
    /// <param name="filePath">File path to check.</param>
    /// <returns>True if the file is a CSV; otherwise, false.</returns>
    private bool IsCsv(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }
}
