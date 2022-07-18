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
            double match = 0;
            foreach (string currentRes in currentResList)
            {
                var a = currentRes.Split('_');
                if (a[0] == a[1])
                {
                    match += 1;
                }
            }
            double accuracy = match / currentResList.Count;
            return accuracy;
        }

        /// <summary>
        /// Write the predict result of one SpatialPooler into a file
        /// </summary>
        /// <param name="a"></param>
        /// <param name="filePath"></param>
        internal static void WriteResultOfOneSP(Dictionary<string, double> a, string filePath)
        {
            using (StreamWriter file = new StreamWriter($"{filePath}.csv"))
                foreach (var entry in a)
                    file.WriteLine("{0}, {1}", entry.Key, entry.Value.ToString());
        }

        internal static void WriteOutputToFile(string v, (string, string) result)
        {
            using (StreamWriter file = new StreamWriter($"{v}.csv"))
            file.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");
        }

        internal static void WriteResultOfOneSPDetailed(Dictionary<string, string> allResultForEachFrame, string filePath)
        {
            using (StreamWriter file = new StreamWriter($"{filePath}.csv"))
                foreach (var entry in allResultForEachFrame)
                    file.WriteLine("{0}; {1}", entry.Key, entry.Value);
        }

        internal static void WriteListToCsv(string path,  List<Dictionary<string, string>> allResult)
        {

            using(StreamWriter file = new StreamWriter($"{path}.csv"))
            {
                List<string> headers = new List<string>();

                foreach(var entry in allResult)
                {
                    foreach(var key in entry.Keys)
                    {
                        if (!headers.Contains(key))
                        {
                            headers.Add(key);
                        }
                    }
                }
                // Header
                file.WriteLine(string.Join(";", headers));
                // Info
                foreach (var entry in allResult)
                {
                    List<string> arrangedEntry = new List<string>();

                    foreach(var header in headers)
                    {
                        if (entry.ContainsKey(header))
                        {
                            arrangedEntry.Add(entry[header]);
                        }
                        else
                        {
                            arrangedEntry.Add("0");
                        }
                    }

                    file.WriteLine(string.Join(";", arrangedEntry));
                }
            }
        }
    }
}