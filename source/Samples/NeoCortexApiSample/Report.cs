using System.Collections.Generic;

namespace NeoCortexApiSample
{
    public class Report
    {
        public string SequenceName { get; set; }
        public int[] SequenceData { get; set; }
        public List<string> PredictionLog { get; set; }
        public double Accuracy { get; set; }
    }
}