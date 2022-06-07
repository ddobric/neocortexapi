using Newtonsoft;
using Newtonsoft.Json;
namespace InvariantLearning
{
    public class Utility
    {
        /// <summary>
        /// Read configuration from json file
        /// </summary>
        /// <param name="jsonFilePath">path of the json file</param>
        /// <returns></returns>
        internal static ExperimentConfig ReadConfig(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            return JsonConvert.DeserializeObject<ExperimentConfig>(json);
        }

        /// <summary>
        /// Create Folder
        /// </summary>
        /// <param name="Folder">relative path of the folder from directory of the running program or absolute path</param>
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
}