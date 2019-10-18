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

namespace UnitTestsProject
{
    [TestClass]
    public class NetworkTests
    {

        /// <summary>
        /// It learns the sequence of elements.
        /// We provide a sequence A,B,C,D and repeat it. 
        /// By calling .Reset() we indicate end of sequence.
        /// We first start learning of region with SP algorithm only. This is called NewBorn-stage. In this stage
        /// SP simply learn patterns to enter a stable stage. To eneter stable stage SP needs 2-3 iterations.
        /// After NewBorn-stage we add TM as second algorithm and start learning sequences.
        /// We do learn sequences 10 iterations with learn=true. When starting the 11th iteration (experimentaly detected) learn is set to false
        /// and inference mode is entered to predict newxt step.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        public void InitTests()
        {
            bool learn = true;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { 100 });
            p.Set(KEY.CELLS_PER_COLUMN, 30);
            string[] categories = new string[] { "A", "B", "C", "D" };
            //string[] categories = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K", "L" , "M", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Ö" };
            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");
            regions.Add(region0);

            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.init(mem, UnitTestHelpers.GetMemory());
            tm1.init(mem);
            Dictionary<string, object> settings = new Dictionary<string, object>();
            //settings.Add("W", 25);
            settings.Add("N", 100);
            //settings.Add("Radius", 1);

            EncoderBase encoder = new CategoryEncoder(categories, settings);
            //encoder.Encode()
            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add(encoder);
            layer1.HtmModules.Add(sp1);
            //layer1.HtmModules.Add(tm1);
            //layer1.Compute();

            //IClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            HtmUnionClassifier<string, ComputeCycle> cls1 = new HtmUnionClassifier<string, ComputeCycle>();
            //string[] inputs = new string[] { "A", "B", "C", "D" };
            string[] inputs = new string[] { "A", "B", "C", "D" };

            //
            // This trains SP.
            foreach (var input in inputs)
            {
                Debug.WriteLine($" ** {input} **");
                for (int i = 0; i < 3; i++)
                {
                    var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;
                }
            }

            // Here we add TM module to the layer.
            layer1.HtmModules.Add(tm1);

            //
            // Now, training with SP+TM. SP is pretrained on pattern.
            for (int i = 0; i < 200; i++)
            {
                foreach (var input in inputs)
                {
                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;
                    //cls1.Learn(input, lyrOut.activeCells.ToArray(), learn);
                    //Debug.WriteLine($"Current Input: {input}");
                    cls.Learn(input, lyrOut.activeCells.ToArray(), lyrOut.predictiveCells.ToArray());
                    Debug.WriteLine($"Current Input: {input}");
                    if (learn == false)
                    {
                        Debug.WriteLine($"Predict Input When Not Learn: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                    }
                    else
                    {
                        Debug.WriteLine($"Predict Input: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                    }

                    //Debug.WriteLine("-----------------------------------------------------------\n----------------------------------------------------------");
                }

                tm1.reset(mem);
                if (i == 10)
                {
                    Debug.WriteLine("Stop Learning From Here----------------------------");
                    learn = false;
                    //tm1.reset(mem);

                }

                tm1.reset(mem);
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

        /// <summary>
        /// It learns the sequence of elements of type double.
        /// </summary>
        [TestMethod]
        [TestCategory("NetworkTests")]
        public void SequenceExperiment()
        {
            bool learn = true;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { 1024 });
            p.Set(KEY.CELLS_PER_COLUMN, 30);

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");
            regions.Add(region0);

            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.init(mem, UnitTestHelpers.GetMemory());
            tm1.init(mem);

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 21},
                { "N", 1024},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "MaxVal", 100.0 },
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            };


            EncoderBase encoder = new ScalarEncoder(settings);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            //
            // NewBorn learning stage.
            region0.AddLayer(layer1);
            layer1.HtmModules.Add(encoder);
            layer1.HtmModules.Add(sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();
            double[] inputs = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };

            //
            // This trains SP.
            foreach (var input in inputs)
            {
                Debug.WriteLine($" ** {input} **");
                for (int i = 0; i < 3; i++)
                {
                    var lyrOut = layer1.Compute((object)input, learn) as ComputeCycle;
                }
            }

            // Here we add TM module to the layer.
            layer1.HtmModules.Add(tm1);

            //
            // Now, training with SP+TM. SP is pretrained on pattern.
            for (int i = 0; i < 200; i++)
            {
                foreach (var input in inputs)
                {
                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    cls.Learn(input, lyrOut.activeCells.ToArray(), lyrOut.predictiveCells.ToArray());

                    Debug.WriteLine($"-------------- {input} ---------------");
                   
                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.winnerCells.Select(c => c.Index).ToArray())}");
                    Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.predictiveCells.Select(c => c.Index).ToArray())}");

                    Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray())}");
                }

                tm1.reset(mem);

                if (i == 50)
                {
                    Debug.WriteLine("Stop Learning From Here. Entering inference mode.");
                    learn = false;
                }

                tm1.reset(mem);
            }

            Debug.WriteLine("------------------------------------------------------------------------\n----------------------------------------------------------------------------");
        }

    }
}
