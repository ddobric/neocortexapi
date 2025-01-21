namespace AnomalyDetectionTeamSynergy
{
    public class AnomalyDetection
    {
        private double _threshold;

        public AnomalyDetection(double threshold = 0.8)
        {
            _threshold = threshold;
        }

        // Method to calculate anomaly based on similarity threshold
        public bool IsAnomaly(double predictedValue, double actualValue)
        {
            return Math.Abs(predictedValue - actualValue) > _threshold;
        }

        // This method can be called after making predictions to identify anomalies
        public void DetectAnomalies(double predictedValue, double actualValue)
        {
            if (IsAnomaly(predictedValue, actualValue))
            {
                Console.WriteLine($"Anomaly detected: Predicted ({predictedValue}) vs. Actual ({actualValue})");
            }
            else
            {
                Console.WriteLine($"No anomaly detected: Predicted ({predictedValue}) vs. Actual ({actualValue})");
            }
        }
    }

    public class Prediction
    {
        public string PredictedInput { get; set; }
        public double Similarity { get; set; }

        public Prediction(string predictedInput, double similarity)
        {
            PredictedInput = predictedInput;
            Similarity = similarity;
        }
    }
}
