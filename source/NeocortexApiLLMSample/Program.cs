using NeocortexApiLLMSample.Interfaces;

namespace NeocortexApiLLMSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, LLM!");

            ICorpusLoader loader = new TextFileCorpusLoader("corpus.txt");

            await loader.Load("bookmark");
        }
    }
}
