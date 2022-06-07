using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    public class Predictor
    {
        public Connections Connections { get; set; }

        public CortexLayer<object, object> Layer { get; set; }

        public HtmClassifier<string, ComputeCycle> Classifier { get; set; }


        public Predictor(CortexLayer<object, object> layer, Connections connections, HtmClassifier<string, ComputeCycle> classifier)
        { 
            this.Connections = connections;
            this.Layer = layer;
            this.Classifier = classifier;
        }

        public void Reset()
        {
            var tm = this.Layer.HtmModules.FirstOrDefault(m => m.Value is TemporalMemory);
            ((TemporalMemory)tm.Value).Reset(this.Connections);
        }
        public List<ClassifierResult<string>> Predict(double input)
        {
            var lyrOut = this.Layer.Compute(input, false) as ComputeCycle;

            List<ClassifierResult<string>> predictedInputValues = this.Classifier.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

            return predictedInputValues;
        }

 
    }
}
