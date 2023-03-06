using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Net.Http.Headers;
using Naa = NeoCortexApi.NeuralAssociationAlgorithm;
using System.Diagnostics;


namespace UnitTestsProject
{
    /// <summary>
    /// UnitTests for the Cell.
    /// </summary>
    [TestClass]
    public class SynapseTests
    {
        [TestMethod]
        [TestCategory("Prod")]
        public void SynapseCompareTest()
        {
            Cell c1 = new Cell(0, 0);
            Cell c2 = new Cell(0, 1);

            Synapse s1 = new Synapse(c1, 0, 0, 0, "a1", 1.0);

            Synapse s2 = new Synapse(c1, 0, 0, 0, "a1", 1.0);

            Synapse s3 = new Synapse(c1, 0, 0, 0, "a2", 1.0);

            Synapse s4 = new Synapse(c1, 0, 1, 0, "a1", 1.0);

            Synapse s5 = new Synapse(c1, 0, 0, 1, "a1", 1.0);

            Synapse s6 = new Synapse(c1, 0, 0, 0, "a1", 0.2);

            Assert.AreEqual(s2 , s2);

            Assert.AreNotEqual(s2, s3);

            Assert.AreNotEqual(s1, s4);

            Assert.AreNotEqual(s1, s5);

            Assert.AreNotEqual(s1, s6);
        }

    }
}
