using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi;

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
            string[] categories = new string[] {"germany", "england", "island", "china" };
            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");
            regions.Add(region0);
            CortexRegion region1 = new CortexRegion("2nd Region"); 
            net = new CortexNetwork("my cortex", regions );
            net.AddRegion(region1);
            SpatialPooler sp1 = new SpatialPooler();
            TemporalMemory tm1 = new TemporalMemory();

            Dictionary<string, object> settings = new Dictionary<string, object>();
            settings.Add("W", 3);
            settings.Add("Radius", 1);
            
            EncoderBase encoder = new CategoryEncoder(categories, settings);
            //encoder.Encode()
            CortexLayer<string, object> layer1 = new CortexLayer<string,object>("L1");
            layer1.HtmModules.Add(encoder);
            layer1.HtmModules.Add(sp1);
            layer1.HtmModules.Add(tm1);
            //layer1.Compute();

            string[] inputs = new string[] { "germany", "china", "french", "russia"};
            foreach (var input in inputs)
            {
                layer1.Compute(input, true);
            }
            
        }

    }
}
