// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnitTestsProject.EncoderTests
{
    [TestClass]
    public class EncoderTests
    {

        /// <summary>
        /// Initializes encoder and invokes Encode method.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        [DataRow(2.81)]
        [DataRow(281)]
        [DataRow(42)]
        [DataRow(123242)]
        [DataRow(-1)]
        public void EncodeTest1(double input)
        {
            Dictionary<string, object> encoderSettings = getDefaultSettings();

            TestEncoder encoder = new TestEncoder(encoderSettings);

            var result = encoder.Encode(input);

            Assert.IsTrue(result.Length == 2);

            Assert.IsTrue(result[0] == Convert.ToInt32(input) + 1);

            Assert.IsTrue(result[1] == 1);
        }

        /// <summary>
        /// Demonstratses how to create an encoder by explicite invoke of initialization.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void EncoderInitTest1()
        {
            Dictionary<string, object> encoderSettings = getDefaultSettings();

            // We change here value of Name property.
            encoderSettings["Name"] = "hello";

            // We add here new property.
            encoderSettings.Add("TestProp1", "hello");

            var encoder = new TestEncoder();

            // Settings can also be passed by invoking Initialize(sett)
            encoder.Initialize(encoderSettings);

            // Property can also be set this way.
            encoder["abc"] = "1";

            Assert.IsTrue((string)encoder["TestProp1"] == "hello");

            Assert.IsTrue((string)encoder["Name"] == "hello");

            Assert.IsTrue((string)encoder["abc"] == "1");
        }


        /// <summary>
        /// Initializes encoder and sets mandatory properties.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void EncoderInitTest2()
        {
            CortexNetworkContext ctx = new CortexNetworkContext();

            Dictionary<string, object> encoderSettings = getDefaultSettings();

            var encoder = ctx.CreateEncoder(typeof(TestEncoder).FullName, encoderSettings);

            foreach (var item in encoderSettings)
            {
                Assert.IsTrue(item.Value == encoder[item.Key]);
            }
        }

        /// <summary>
        /// Demonstratses how to create an encoder and how to set encoder properties by using of context.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void EncoderInitTest3()
        {
            CortexNetworkContext ctx = new CortexNetworkContext();

            // Gets set of default properties, which more or less every encoder requires.
            Dictionary<string, object> encoderSettings = getDefaultSettings();

            // We change here value of Name property.
            encoderSettings["Name"] = "hello";

            // We add here new property.
            encoderSettings.Add("TestProp1", "hello");

            var encoder = ctx.CreateEncoder(typeof(TestEncoder).FullName, encoderSettings);

            // Property can also be set this way.
            encoder["abc"] = "1";

            Assert.IsTrue((string)encoder["TestProp1"] == "hello");

            Assert.IsTrue((string)encoder["Name"] == "hello");

            Assert.IsTrue((string)encoder["abc"] == "1");
        }


        /// <summary>
        /// Demonstratses how to create an encoder by explicite invoke of initialization.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void EncoderInitTest4()
        {
            Dictionary<string, object> encoderSettings = getDefaultSettings();

            // We change here value of Name property.
            encoderSettings["Name"] = "hello";

            // We add here new property.
            encoderSettings.Add("TestProp1", "hello");

            var encoder = new TestEncoder();

            // Settings can also be passed by invoking Initialize(sett)
            encoder.Initialize(encoderSettings);

            // Property can also be set this way.
            encoder["abc"] = "1";

            Assert.IsTrue((string)encoder["TestProp1"] == "hello");

            Assert.IsTrue((string)encoder["Name"] == "hello");

            Assert.IsTrue((string)encoder["abc"] == "1");
        }

        /// <summary>
        /// Demonstratses how to create an encoder and how to set encoder properties by using of context.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void InitMultiencoder()
        {
            CortexNetworkContext ctx = new CortexNetworkContext();

            // Gets set of default properties, which more or less every encoder requires.
            Dictionary<string, object> encoderSettings = getDefaultSettings();
            encoderSettings[EncoderProperties.EncoderQualifiedName] = typeof(TestEncoder).AssemblyQualifiedName;

            // We change here value of Name property.
            encoderSettings["Name"] = "hello";

            // We add here new property.
            encoderSettings.Add("TestProp1", "hello");

            var encoder = ctx.CreateEncoder(typeof(TestEncoder).FullName, encoderSettings);

            // Property can also be set this way.
            encoder["abc"] = "1";

            Assert.IsTrue((string)encoder["TestProp1"] == "hello");

            Assert.IsTrue((string)encoder["Name"] == "hello");

            Assert.IsTrue((string)encoder["abc"] == "1");
        }

        /// <summary>
        /// Initializes all encoders.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void InitializeAllEncodersTest()
        {
            CortexNetworkContext ctx = new CortexNetworkContext();

            Assert.IsTrue(ctx.Encoders != null && ctx.Encoders.Count > 0);

            Dictionary<string, object> encoderSettings = getDefaultSettings();

            foreach (var item in ctx.Encoders)
            {
                var encoder = ctx.CreateEncoder(typeof(TestEncoder).FullName, encoderSettings);

                foreach (var sett in encoderSettings)
                {
                    Assert.IsTrue(sett.Value == encoder[sett.Key]);
                }
            }
        }



        private static Dictionary<string, object> getDefaultSettings()
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("N", 5);
            encoderSettings.Add("W", 10);
            encoderSettings.Add("MinVal", (double)5);
            encoderSettings.Add("MaxVal", (double)10);
            encoderSettings.Add("Radius", (double)5);
            encoderSettings.Add("Resolution", (double)10);
            encoderSettings.Add("Periodic", (bool)false);
            encoderSettings.Add("ClipInput", (bool)true);
            return encoderSettings;
        }
    }


    public class BinaryEncoder : EncoderBase
    {
        #region Private Fields

        #endregion

        #region Properties

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        public BinaryEncoder()
        {

        }

        public BinaryEncoder(Dictionary<string, object> encoderSettings)
        {
            this.Initialize(encoderSettings);
        }

        public override void AfterInitialize()
        {

        }

        /// <summary>
        /// Sample encoder. It encodes specified value to the binary code sequence.
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public override int[] Encode(object inputData)
        {
            if (inputData == null)
                throw new ArgumentException("inputData cannot be empty!");

            double val;

            if (!double.TryParse(inputData as string, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                throw new ArgumentException($"Value {inputData} cannot be casted to integer.");

            string binary = Convert.ToString((int)val, 2);

            binary = binary.PadLeft(this.N, '0');

            List<int> result = new List<int>();
            foreach (var chr in binary)
            {
                result.Add(chr == '1' ? 1 : 0);
            }

            return result.ToArray();
        }

        public override List<B> GetBucketValues<B>()
        {
            throw new NotImplementedException();
        }

        public override int Width
        {
            get
            {
                return this.N;
            }
        }


        public override bool IsDelta
        {
            get { return false; }
        }
        #endregion
    }


    /// <summary>
    /// Implementation of simple test encoder. It demonstrates how to implement encoders.
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

        public override void AfterInitialize()
        {

        }

        /// <summary>
        /// Encodes specified value by adding +1.
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public override int[] Encode(object inputData)
        {
            if (inputData.GetType() == typeof(int) || inputData.GetType() == typeof(double))
            {
                return new int[] { Convert.ToInt32(inputData) + 1, 1 };
            }
            else
                throw new NotSupportedException();
        }

        public override List<B> GetBucketValues<B>()
        {
            throw new NotImplementedException();
        }

        public override int Width
        {
            get
            {
                return 1;
            }
        }


        public override bool IsDelta
        {
            get { return false; }
        }
        #endregion
    }
}
