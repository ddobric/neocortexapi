using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using System.IO;

namespace UnitTestsProject
{   
    [TestClass]
    public class NetworkTests
    {

        /// <summary>
        /// Initializes encoder and invokes Encode method.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        public void InitTests()
        {
            bool learn = true;
            Parameters p = Parameters.getAllDefaultParameters();
            //string[] categories = ReadCsvFileTest("C:\\Users\\n.luu\\Desktop\\testValues.txt",5); ;
            string[] categories = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K"};
            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");
            regions.Add(region0);
            
            SpatialPooler sp1 = new SpatialPooler();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.init(mem);
            tm1.init(mem);
            Dictionary<string, object> settings = new Dictionary<string, object>();
            settings.Add("W", 10);
            settings.Add("N", 100);
            settings.Add("Radius", 1);

            EncoderBase encoder = new DateTimeEncoder();
            //encoder.Encode()
            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add(encoder);
            layer1.HtmModules.Add(sp1);
            layer1.HtmModules.Add(tm1);
            //layer1.Compute();

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            HtmClassifier_Test<string, string> cls1 = new HtmClassifier_Test<string, string>();
            string[] inputs = new string[] { "A", "B", "C","C", "D" };
          
            //string[] inputs = new string[] { "A", "B", "C", "C", "D", "E", "F", "G", "G", "G", "G", "L", "M", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
            for (int i = 0; i < 50; i++)
            {
                foreach (var input in inputs)
                {
                    var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;
                    cls1.Learn(input,lyrOut.activeCells.ToArray(),learn);
                    //Debug.WriteLine($"Current Input: {input}");
                    //cls.Learn(input, lyrOut.activeCells.ToArray(), lyrOut.predictiveCells.ToArray());
                    Debug.WriteLine($"Current Input: {input}");
                    if (learn == false)
                    {
                        //Debug.WriteLine($"Predict Input When Not Learn: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                        Debug.WriteLine($"Predict Input When Not Learn: {cls1.Inference(lyrOut.predictiveCells.ToArray())}");
                    }
                    else
                    {
                        //Debug.WriteLine($"Predict Input: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                        Debug.WriteLine($"Predict Input: {cls1.Inference(lyrOut.predictiveCells.ToArray())}");
                    }
                    
                    //Debug.WriteLine("-----------------------------------------------------------\n----------------------------------------------------------");
                }
                tm1.reset(mem);
                if (i == 20)
                {
                    Debug.WriteLine("Stop Learning From Here-----------------------------------------------------------------------------------------------------\n"
                        +"-----------------------------------------------------------------------------------------------" +
                        "-------------------------------------------------------------------------------------------------");
                    learn = false;
                    //tm1.reset(mem);
                    
                }
            }
            
            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
            /*
            learn = false;
            for (int i = 0; i < 19; i++)
            {
                foreach (var input in inputs)
                {
                    layer1.Compute((object)input, learn);
                }
            }
            */
            
        }
        [TestMethod]
        public void NewNetworkTest()
        {
            
        }
        public Dictionary<string,double> ReadCsvFileTest(string sourcePath,int numberOfRow)
        {
            string fileContent = File.ReadAllText(sourcePath);
            string[] stringArr = fileContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string,double> dict =new Dictionary<string, double>();
            for (int i = 0; i < numberOfRow; i++)
            {
                string[] row = stringArr[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                dict.Add(row[0], double.Parse(row[1]));
            }
            return dict;
        }
    }
}
