using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Implements the class that helps inferring (prediction). It is typically used by the Sequence Learning algorithm.
    /// After the learning process, the algorithm returns the instance of this class. This class provides a method <see cref="nameof(Predictor.Predict)"/> with a list of input elements.
    /// For every presented input element the predictor tries to predict the next element. The more element provided in a sequence the predictor returns with the higher score.
    /// For example, assume there ar two sequences: ABCD and ABGHI. By presenting the element B, the predictor is not sure if the next element is C or D. 
    /// When presenting after B the element C, the predictor knows that the next element must be C.
    /// </summary>
    public class Predictor : ISerializable
    {
        private Connections connections { get; set; }

        private CortexLayer<object, object> layer { get; set; }

        private HtmClassifier<string, ComputeCycle> classifier { get; set; }

        /// <summary>
        /// Initializes the predictor functionality.
        /// </summary>
        /// <param name="layer">The HTM Layer.</param>
        /// <param name="connections">The HTM memory in the learned state.</param>
        /// <param name="classifier">The classifier that contains the state of learned sequences.</param>

        public Predictor(CortexLayer<object, object> layer, Connections connections, HtmClassifier<string, ComputeCycle> classifier)
        {
            this.connections = connections;
            this.layer = layer;
            this.classifier = classifier;
        }

        /// <summary>
        /// Starts predicting of the next subsequences.
        /// </summary>
        public void Reset()
        {
            var tm = this.layer.HtmModules.FirstOrDefault(m => m.Value is TemporalMemory);
            ((TemporalMemory)tm.Value).Reset(this.connections);
        }


        /// <summary>
        /// Predicts the list of next expected elements.
        /// </summary>
        /// <param name="input">The element that will cause the next expected element.</param>
        /// <returns>The list of expected (predicting) elements.</returns>
        public List<ClassifierResult<string>> Predict(double input)
        {
            var lyrOut = this.layer.Compute(input, false) as ComputeCycle;

            List<ClassifierResult<string>> predictedInputValues = this.classifier.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

            return predictedInputValues;
        }

        /// <summary>
        /// Serialize the Predictor instance
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="sw"></param>
        public void Serialize(object obj, string name, StreamWriter sw)
        {
            if (obj is Predictor predictor)
            {
                // Serialize the Connections in Predictor instance
                var connections = predictor.connections;
                connections.Serialize(connections, null, sw);

                // Serialize the CortexLayer in Predictor instance               
                var layer = predictor.layer;
                layer.Serialize(layer, null, sw);

                // Serialize the HtmClassifier object in Predictor instance             
                var classifier = predictor.classifier;
                classifier.Serialize(classifier, null, sw);
            }
        }


        /// <summary>
        /// Method used to de-serialize the Predictor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sr"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object Deserialize<T>(StreamReader sr, string name)
        {
            HtmSerializer ser = new HtmSerializer();
            // Initialize the Predictor
            Predictor predictor = new Predictor(null, null, null);
            // Initialize the CortexLayer
            CortexLayer<object, object> layer = new CortexLayer<object, object>("L1");

            // Add SP and TM objects to CortexLayer, initialize the values (null) 
            layer.HtmModules.Add("encoder", (ScalarEncoder)null);
            layer.HtmModules.Add("sp", (SpatialPoolerMT)null);
            layer.HtmModules.Add("tm", (TemporalMemory)null);

            while (sr.Peek() >= 0)
            {

                var data = sr.ReadLine();

                // Deserialize Connections object 
                if (data == ser.ReadBegin(nameof(Connections)) && (predictor.connections == null))
                {
                    var con = Connections.Deserialize<Connections>(sr, null);
                    if (con is Connections connections)
                    {
                        predictor.connections = connections;
                    }

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);

                }

                // Deserialize the ScalarEncoder             
                if (data == ser.ReadBegin(nameof(ScalarEncoder)) && (layer.HtmModules["encoder"] == null))
                {
                    var en = EncoderBase.Deserialize<ScalarEncoder>(sr, null);
                    if (en is ScalarEncoder encoder)
                    {
                        layer.HtmModules["encoder"] = encoder;
                    }

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize Spatial Pooler object
                if (data == ser.ReadBegin(nameof(SpatialPoolerMT)) && (layer.HtmModules["sp"] == null))
                {
                    var sp = SpatialPooler.Deserialize<SpatialPoolerMT>(sr, null);
                    layer.HtmModules["sp"] = (SpatialPoolerMT)sp;

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize Temporal Memory object
                if (data == ser.ReadBegin(nameof(TemporalMemory)) && (layer.HtmModules["tm"] == null))
                {
                    var tm = TemporalMemory.Deserialize<TemporalMemory>(sr, null);
                    layer.HtmModules["tm"] = (TemporalMemory)tm;

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize the HtmClassifier object
                if (data == ser.ReadBegin(nameof(HtmClassifier<string, ComputeCycle>)) && (predictor.classifier == null))
                {
                    var cls = HtmClassifier<string, ComputeCycle>.Deserialize<HtmClassifier<string, ComputeCycle>>(sr, null);
                    predictor.classifier = (HtmClassifier<string, ComputeCycle>)cls;

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }
            }

            predictor.layer = layer;
            return predictor;

        }

        /// <summary>
        /// Save Predictor (obj) model to fileName
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sw"></param>
        public static void Save(object obj, string fileName)
        {
            if (obj is Predictor predictor)
            {
                HtmSerializer.Reset();
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    predictor.Serialize(obj, null, sw);
                    //predictor.Serialize(sw);
                }
            }
        }

        /// <summary>
        /// Load Predictor model from fileName
        /// </summary>
        /// <param name="sr"></param>
        public static T Load<T>(string fileName)
        {
            HtmSerializer.Reset();
            using StreamReader sr = new StreamReader(fileName);
            return (T)Deserialize<T>(sr, null);
        }
    }
}