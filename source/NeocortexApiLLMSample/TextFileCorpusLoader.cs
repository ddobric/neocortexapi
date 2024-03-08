using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeocortexApiLLMSample
{
    public class TextFileCorpusLoader : Interfaces.ICorpusLoader
    {
        private readonly string _file;

        private Dictionary<string, string> _words = new Dictionary<string, string>();

        public TextFileCorpusLoader(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name must not be null or empty.", nameof(fileName));
            }

            _file = fileName;
        }

        public async Task<string?> Load(string bookmark)
        {
            using (StreamReader sr = new StreamReader(_file))
            {
                string line = await sr.ReadLineAsync()!;

                if (line != null)
                {
                    var words = ExtractWords(line);
                }
            }

            // We do not support bookmaring right now.
            return null;
        }

        private static List<string> ExtractWords(string input)
        {
            // Use a regular expression to match word boundaries
            // \b asserts a word boundary
            // \w+ matches one or more word characters (alphanumeric or underscore)
            MatchCollection matches = Regex.Matches(input, @"\b\w+\b");

            // Convert MatchCollection to a List<string>
            List<string> words = new List<string>();
            foreach (Match match in matches)
            {
                words.Add(match.Value);
            }

            return words;
        }

        public IEnumerator<string> GetEnumerator()
        {
            using (StreamReader sr = new StreamReader(_file))
            {
                string? line = sr.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = sr.ReadLine();
                }
            }
        }
    }

}
