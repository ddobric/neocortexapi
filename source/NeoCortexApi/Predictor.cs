using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Implements the class that helps inferring (prediction). It is typically used by the Sequence Learning algorithm.
    /// After the learning process, the algorithm returns the instance of this class. This class provides a method <see cref="nameof(Predictor.Predict)"/> with a list of input elements.
    /// For every presented input element the predictor tries to predict the next element. The more element provided in a sequence the predictor returns wit the higher score.
    /// For example, assume there ar two sequences: ABCD and ABGHI. By presenting the element B, the predictor is not sure if the next element is C or D. 
    /// When presenting after B the element C, the predictor knows that the next element must be C.
    /// </summary>
    public class Predictor : ISerializable
    {
        public Connections connections { get; set; }

        public CortexLayer<object, object> layer { get; set; }

        public HtmClassifier<string, ComputeCycle> classifier { get; set; }

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
        /// Method used to serialize the Predictor object 
        /// </summary>
        /// <param name="obj">The Predictor instance</param>
        /// <param name="name"></param>
        /// <param name="sw">StreamWritter</param>
        public void Serialize(object obj, string name, StreamWriter sw)
        {
            if (obj is Predictor predictor)
            {   
                var model_con = "Model_con.txt";
                StreamWriter sw_con = new StreamWriter(model_con);
                predictor.connections.Serialize(predictor.connections, null, sw_con);
                sw_con.Close();

                var model_layer = "Model_layer.txt";
                StreamWriter sw_layer = new StreamWriter(model_layer);
                predictor.layer.Serialize(predictor.layer, null, sw_layer);
                sw_layer.Close();

                var model_cls = "Model_cls.txt";
                StreamWriter sw_cls = new StreamWriter(model_cls);
                var cls = predictor.classifier;
                cls.Serialize(cls, null, sw_cls);
                
                /*
                HtmSerializer ser = new HtmSerializer();

                ser.SerializeBegin(nameof(Connections), sw);
                predictor.connections.Serialize(predictor.connections, null, sw);
                ser.SerializeEnd(nameof(Connections), sw);

                ser.SerializeBegin(nameof(CortexLayer<Object, Object>), sw);
                predictor.layer.Serialize(predictor.layer, null, sw);
                ser.SerializeEnd(nameof(Connections), sw);

                ser.SerializeBegin(nameof(HtmClassifier<string, ComputeCycle>), sw);
                predictor.classifier.Serialize(predictor.classifier, null, sw);
                ser.SerializeEnd(nameof(Connections), sw);
                */
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
            
            var model_con = "Model_con.txt";
            StreamReader sr_con = new StreamReader(model_con);
            var con = Connections.Deserialize<Connections>(sr_con, null);
            sr_con.Close();

            var model_cls = "Model_cls.txt";
            StreamReader sr_cls = new StreamReader(model_cls);
            var cls = HtmClassifier<string, ComputeCycle>.Deserialize<HtmClassifier<string, ComputeCycle>>(sr_cls, null);
            sr_cls.Close();

            var layer = CortexLayer<object, object>.Deserialize<T>(null);


            /*
            HtmSerializer ser = new HtmSerializer();
            Predictor predictor = new Predictor(null, null, null);

            while (sr.Peek() >= 0)
            {
                
                var data = sr.ReadLine();
                
                
                if (data == ser.ReadBegin(nameof(Connections)))
                {
                    
                    var con = Connections.Deserialize<Connections>(sr, null);
                    predictor.connections = (Connections)con;
                    
                    
                }
                
                
                if (data == ser.ReadBegin(nameof(CortexLayer<Object, Object>)))
                {               
                    var layer = CortexLayer<object, object>.Deserialize<CortexLayer<object, object>>(sr);
                    predictor.layer = (CortexLayer<object, object>)layer;
                    
                }               
                
            
                if (data == ser.ReadBegin(nameof(HtmClassifier<string, ComputeCycle>)))
                {
                    var cls = HtmClassifier<string, ComputeCycle>.Deserialize<HtmClassifier<string, ComputeCycle>>(sr, null);
                    predictor.classifier = (HtmClassifier<string, ComputeCycle>)cls;                    
                }
                        
            }
            sr.Close();
            */
            var predictor = new Predictor((CortexLayer<object, object>)layer, (Connections)con, (HtmClassifier<string, ComputeCycle>)cls);
            return predictor;
        }

        /// <summary>
        /// Save Predictor model
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sw"></param>
        public static void Save(string fileName, object obj)
        {           
            if (obj is Predictor predictor)
            {
                HtmSerializer.Reset();
                StreamWriter sw = new StreamWriter(fileName);
                predictor.Serialize(obj, null, sw);
            }
        }

        /// <summary>
        /// Load Predictor model
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
