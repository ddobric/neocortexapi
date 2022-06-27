using Newtonsoft;
using Newtonsoft.Json;
using shortid;

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

        internal static string GetHash()
        {
            return ShortId.Generate();
        }

        internal static double AccuracyCal(List<string> currentResList)
        {
            double accuracy = 0;
            double match = 0;
            foreach (string currentRes in currentResList)
            {
                var a = currentRes.Split('_');
                if (a[0] == a[1])
                {
                    match += 1;
                }
            }
            return match/currentResList.Count;
        }
    }
}