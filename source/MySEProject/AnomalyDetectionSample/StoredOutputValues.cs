namespace AnomalyDetectionSample
{
    /// <summary>
    /// Output result class
    /// </summary>
    public static class StoredOutputValues
    {
        public static double TrainingTimeInSeconds { get; set; }
        public static string OutputPath { get; set; }
        public static double totalAvgAccuracy { get; set; }
    }
}