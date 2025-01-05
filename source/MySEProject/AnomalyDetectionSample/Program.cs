using System;

namespace AnomalyDetectionSample
{

    class Program
    {
        static void Main(string[] args)
        {
            // Starts experiment that demonstrates how to perform anomaly detection using multisequencelearning.
            HTMAnomalyExperiment tester = new HTMAnomalyExperiment();
            tester.ExecuteExperiment();

        }

    }
}