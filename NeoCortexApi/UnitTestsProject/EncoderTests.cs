using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UnitTestsProject
{
    [TestClass]
    public class EncoderTests
    {
        [TestMethod]
        public void StableOutputOnSameInputTest()
        {
            CortexNetworkContext ctx = new CortexNetworkContext();

            ctx.CreateEncoder(typeof(TestEncoder).FullName, null);

            ctx.CreateEncoder(typeof(MultiEncoder).FullName, null);


        }




    }

    /// <summary>
    /// TODO
    /// </summary>
    public class TestEncoder : EncoderBase
    {
        #region Private Fields

        #endregion

        #region Properties

        #endregion



        #region Private Methods

        #endregion



        #region Public Methods

        public TestEncoder()
        {

        }

        public TestEncoder(Dictionary<string, object> encoderSettings)
        {
            this.Initialize(encoderSettings);
        }

        public override void Initialize(Dictionary<string, object> encoderSettings)
        {

        }

       
        public override int[] Encode(object inputData)
        {
            return new[] { 1,0,0,1,0,1,0};
        }

        public override List<B> getBucketValues<B>()
        {
            throw new NotImplementedException();
        }

        public override int Width
        {
            get
            {
                return this.Width;
            }
        }


        public override bool IsDelta
        {
            get { return false; }
        }
        #endregion
    }
}
