using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi.Entities;

namespace NeoCortexApi
{
    public class ManualInput : IInference
    {
        public int RecordNum {
            set;
            get;
        }

        public ComputeCycle ComputeCycle { get; }

        public object CustomObject { get; }

        public Dictionary<string, object> ClassifierInput { get; }

        public object Classifiers { get; }

        public object LayerInput { get; set; }

        public int[] Sdr { get; set; }

        public int[] getEncoding { get; }

        public double AnomalyScore { get; }

        public int[] FeedForwardActiveColumns { get; }

        public int[] FeedForwardSparseActives { get; }

        public List<Cell> ActiveCells { get; }

        public List<Cell> PreviousPredictiveCells { get; }

        public List<Cell> PredictiveCells { get; }

        private Dictionary<String, Classification<Object>> classification;

        public Classification<object> GetClassification(string fieldName)
        {
            if (classification == null)
                return null;

            return classification[fieldName];
        }
    }
}
