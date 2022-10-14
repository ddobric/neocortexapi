using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to HtmModuleTopology.
    /// </summary>
    [TestClass]
    public class HtmModuleTopologyTests
    {
        /// <summary>
        /// Test equal method of HtmModuleTopology
        /// </summary>
        [TestMethod]
        public void CompareHtmModuleTopology()
        {
            int[] dimensions1 = { 1, 2, 3 };
            int[] dimensions2 = { 1, 2, 3 };
            int[] dimensions3 = { 1, 2, 3 };
            int[] dimensions4 = { 1, 2, 3 };

            bool isMajorOrdering1 = true;
            bool isMajorOrdering2 = false;

            HtmModuleTopology top1 = new HtmModuleTopology(dimensions1, isMajorOrdering1);
            HtmModuleTopology top2 = new HtmModuleTopology(dimensions1, isMajorOrdering1);
            HtmModuleTopology top3 = new HtmModuleTopology(dimensions2, isMajorOrdering1);
            HtmModuleTopology top4 = new HtmModuleTopology(dimensions1, isMajorOrdering2);

            //Not same by reference
            Assert.IsFalse(top1 == top2);
            Assert.IsFalse(top1 == top3);
            Assert.IsFalse(top1 == top4);
            
            //top1 and top2 are same by value
            Assert.IsTrue(top1.Equals(top2));

            //top1 and top3 are NOT same by value
            Assert.IsFalse(top1.Equals(top3));

            //top1 and top4 are NOT same by value
            Assert.IsFalse(top1.Equals(top4));
        }
    }
}
