using NeoCortexApi.Entities;
using Newtonsoft;
using Newtonsoft.Json;

public class Utility
{
    internal static ExpCfg ReadConfig(string jsonFilePath)
    {
        string json = File.ReadAllText(jsonFilePath);
        return JsonConvert.DeserializeObject<ExpCfg>(json);
    }

    public static void CreateFolderIfNotExist(string Folder)
    {
        if (Directory.Exists(Folder))
        {
            return;
        }
        else
        {
            Directory.CreateDirectory(Folder);
        }
    }
}

/// <summary>
/// Experiment Folder Configuration
/// </summary>
public class ExpCfg
{
    public string PathToTrainingFolder;
    public string PathToFeatureFolder;

    public RunConfig experimentParams;
}

/// <summary>
/// Experiment running configuration
/// </summary>
public class RunConfig
{
    /// <summary>
    /// Hierarchical Temporal Memory Configuration
    /// </summary>
    public HtmConfig htmConfig;

    /// <summary>
    /// Iteration through whole dataset
    /// </summary>
    public int epochs;
}