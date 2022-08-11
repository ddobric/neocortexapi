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

        internal static void WriteListToOutputFile(string path, List<Dictionary<string, string>> allResult)
        {
            using (StreamWriter file = new StreamWriter($"{path}.txt"))
            {

                double accuracy = 0;
                double correctCount = 0;
                // Info
                foreach (var entry in allResult)
                {
                    file.WriteLine($"Predicting image {entry["fileName"]}");
                    List<string> arrangedEntry = new List<string>();
                    foreach(var a in entry)
                    {
                        arrangedEntry.Add($"{a.Key}__{a.Value}");
                    }
                    file.WriteLine(string.Join(";", arrangedEntry));

                    Dictionary<string, string> top3similarities = GetTop3Similarites(entry);
                    if (GetLabelFromResult(top3similarities).Contains(entry["CorrectLabel"]))
                    {
                        correctCount += 1;
                    }
                    List<string> top3 = new List<string>(ToListResult(top3similarities));
                    file.WriteLine(string.Join(";", top3));
                }
                accuracy = correctCount/ (double)allResult.Count;
                file.WriteLine($"accuracy: {accuracy} {correctCount}/{allResult.Count}");
            }
        }

        private static List<string> ToListResult(Dictionary<string, string> top3similarities)
        {
            List<string> strings = new List<string>();
            foreach(var a in top3similarities)
            {
                strings.Add($" {a.Key}: {a.Value}% ");
            }
            return strings;
        }

        private static List<string> GetLabelFromResult(Dictionary<string, string> top3similarities)
        {
            List<string> result = new List<string>();
            foreach(var a in top3similarities)
            {
                string label = a.Key.Split("_")[0];
                result.Add(label);
            }
            return result;
        }

        private static Dictionary<string, string> GetTop3Similarites(Dictionary<string,string> result)
        {
            Dictionary<string, string> top3similarities = new Dictionary<string, string>();
            foreach (var entry in result)
            {
                if((entry.Key == "CorrectLabel") || (entry.Key == "fileName")){
                    continue;
                }
                else
                {
                    top3similarities.Add(entry.Key, entry.Value);
                    if(top3similarities.Count == 4)
                    {
                        top3similarities.Remove(top3similarities.MinBy(kvp => kvp.Value).Key);
                    }
                }
            }
            return top3similarities;
        }

        internal static int[] AddArray(int[] current, int[] a)
        {
            int[] res = new int[current.Length];
            for(int i = 0; i< current.Length; i += 1)
            {
                res[i] = current[i] + a[i];
            }
            return res;
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

        internal static double CalcArrayDistance(int[] entry, int[] current)
        {
            double result = 0;
            for(int i = 0; i < entry.Length; i += 1)
            {
                result += Math.Pow((entry[i] - current[i]), 2);
            }
            return Math.Sqrt(result);
        }
        internal static double CalArrayUnion(int[] entry, int[] current)
        {
            double result = 0;
            for (int i = 0; i < entry.Length; i += 1)
            {
                int a = (entry[i] > 0) ? 1 : 0;
                int b = (current[i] > 0) ? 1 : 0;
                result += Math.Pow((a - b), 2);
            }
            return Math.Sqrt(result);
        }
    }
}