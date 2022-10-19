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
    /// Tests for ComputeCycle equal method.
    /// </summary>
    [TestClass]
    public class ComputeCycleTests
    {
        [TestMethod]
        public void CompareComputeCycles()
        {
            int[] inputDims = { 100, 100 };
            int[] columnDims1 = { 10, 10 };
            int[] columnDims2 = { 2, 100 };
            HtmConfig config1 = new HtmConfig(inputDims, columnDims1);
            HtmConfig config2 = new HtmConfig(inputDims, columnDims2);

            Connections connections1 = new Connections(config1);
            Connections connections2 = new Connections(config2);

            Cell cell1 = new Cell(2, 4, 6, new CellActivity());
            Cell cell2 = new Cell(12, 14, 16, new CellActivity());

            var distDend1 = new DistalDendrite(cell1, 1, 2, 2, 1.0, 100);
            var distDend2 = new DistalDendrite(cell2, 10, 20, 20, 10.0, 256);

            connections1.ActiveSegments.Add(distDend1);
            connections2.ActiveSegments.Add(distDend2);

            ComputeCycle computeCycle1 = new ComputeCycle(connections1);
            ComputeCycle computeCycle2 = new ComputeCycle(connections1);
            ComputeCycle computeCycle3 = new ComputeCycle(connections2);

            //Not same by reference
            Assert.IsFalse(computeCycle1 == computeCycle2);

            //computeCycle1 and computeCycle2 are same by value
            Assert.IsTrue(computeCycle1.Equals(computeCycle2));

            //computeCycle1 and computeCycle3 are NOT same by value (different connections)
            Assert.IsFalse(computeCycle1.Equals(computeCycle3));
        }
    }
}
