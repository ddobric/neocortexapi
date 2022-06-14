using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to distal segments.
    /// Distal segments are used by TM to learn sequences.
    /// </summary>
    [TestClass]
    public class DistalDendriteTests
    {
        [TestMethod]
        [TestCategory("Prod")]
        public void CompareDentrites()
        {
            Cell c1 = new Cell(1, 1, 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);
            Cell c2 = new Cell(1, 1, 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);
            Cell c3 = new Cell(2, 1, 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);

            DistalDendrite d1 = new DistalDendrite(c1, 1, 1, 1, 0.5, 10);
            DistalDendrite d2 = new DistalDendrite(c1, 1, 1, 1, 0.5, 10);
            DistalDendrite d3 = new DistalDendrite(c1, 2, 1, 1, 0.5, 10);

            Assert.IsTrue(d1.Equals(d2));

            Assert.IsFalse(d1.Equals(d3));
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void CompareDentriteList()
        {
            Cell c1 = new Cell(1, 1, 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);
            Cell c2 = new Cell(1, 1, 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);
            Cell c3 = new Cell(2, 1, 10, NeoCortexEntities.NeuroVisualizer.CellActivity.ActiveCell);

            DistalDendrite d1 = new DistalDendrite(c1, 1, 1, 1, 0.5, 10);
            DistalDendrite d2 = new DistalDendrite(c1, 1, 1, 1, 0.5, 10);
            DistalDendrite d3 = new DistalDendrite(c1, 2, 1, 1, 0.5, 10);
            DistalDendrite d4 = new DistalDendrite(c2, 2, 2, 1, 0.5, 10);

            List<DistalDendrite> list1 = new List<DistalDendrite>() { d1, d2, d2 };
            List<DistalDendrite> list2 = new List<DistalDendrite>() { d1, d2, d3 };

            // Not same by reference
            Assert.IsFalse(d1 == d2);

            // d1 and d2 are same by value.
            Assert.IsTrue(d1.Equals(d2));

            // d1 and d3 are NOT same by value.
            Assert.IsFalse(d1.Equals(d3));

            // d3 and d4 are NOT same by value.
            Assert.IsFalse(d3.Equals(d4));

            // Lists are same by value.
            list1.SequenceEqual(list2);

            // All same by references.
            list2 = new List<DistalDendrite>() { d1, d1, d1 };

            // Lists are strill same by value.
            list1.SequenceEqual(list2);

            list2 = new List<DistalDendrite>() { d1, d2, d3 };

            Assert.IsTrue(d1.Equals(d2));

            Assert.IsFalse(d1.Equals(d3));
        }
    }
}
