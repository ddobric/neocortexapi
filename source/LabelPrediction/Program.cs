using System;

namespace LabelPrediction
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /// <summary>
            /// Predict Power consumption using Multisequence Learning
            /// </summary>

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            Console.WriteLine("Starting to learn power comsumption data");

            MultiSequenceLearning mseq = new MultiSequenceLearning();

            mseq.StartLearning();



        }
    }
}
