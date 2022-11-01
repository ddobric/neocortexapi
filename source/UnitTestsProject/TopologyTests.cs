using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using NeoCortexEntities.NeuroVisualizer;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests for Topology equal method.
    /// </summary>
    [TestClass]
    public class TopologyTests
    {
        [TestMethod]
        public void CompareTopologies()
        {
            int[] shape1 = { 1, 2, 4 };
            int[] shape2 = { 98, 72, 67, 54 };
            bool useColumnMajorOrdering1 = true;
            bool useColumnMajorOrdering2 = false;

            Topology topology1 = new Topology(shape1, useColumnMajorOrdering1);
            Topology topology2 = new Topology(shape1, useColumnMajorOrdering1);
            Topology topology3 = new Topology(shape2, useColumnMajorOrdering1);
            Topology topology4 = new Topology(shape1, useColumnMajorOrdering2);

            //Not same by reference
            Assert.IsFalse(topology1 == topology2);

            //topology1 and topology2 are same by value
            Assert.IsTrue(topology1.Equals(topology2));

            //topology1 and topology3 are NOT same by value (different shape)
            Assert.IsFalse(topology1.Equals(topology3));

            //topology1 and topology4 are NOT same by value (different column major ordering)
            Assert.IsFalse(topology1.Equals(topology3));
        }
    }
}
