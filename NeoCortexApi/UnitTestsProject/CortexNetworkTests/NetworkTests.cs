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
        public void InitTests(int input)
        {
            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            net = new CortexNetwork("my cortex", regions );

            SpatialPooler sp1 = new SpatialPooler();
            TemporalMemory tm1 = new TemporalMemory();
            EncoderBase encoder = new BooleanEncoder();
            //encoder.Encode()
            CortexLayer layer1 = new CortexLayer("L1");
            layer1.HtmModules.Add(encoder);
            layer1.HtmModules.Add(sp1);
            layer1.HtmModules.Add(tm1);

            //layer1.Compute();
        }

    }
}
