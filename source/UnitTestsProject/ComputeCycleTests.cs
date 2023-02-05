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
    /// Tests related to ComputeCycle.
    /// </summary>
    [TestClass]
    public class ComputeCycleTests
    {
        /// <summary>
        /// Tests for ComputeCycle equal method.
        /// </summary>
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

            #region Test with same/different connection input/activeSegments
            ComputeCycle computeCycle1 = new ComputeCycle(connections1);
            ComputeCycle computeCycle2 = new ComputeCycle(connections1);
            ComputeCycle computeCycle3 = new ComputeCycle(connections2);

            //Not same by reference
            Assert.IsFalse(computeCycle1 == computeCycle2);
            Assert.IsFalse(computeCycle1 == computeCycle3);

            //computeCycle1 and computeCycle2 are same by value
            Assert.IsTrue(computeCycle1.Equals(computeCycle2));

            //computeCycle1 and computeCycle3 are NOT same by value
            Assert.IsFalse(computeCycle1.Equals(computeCycle3));
            #endregion


            #region Test with same/different ActiveCells
            Cell activeCell1 = new Cell(1, 1, 10, new CellActivity());
            Cell activeCell2 = new Cell(1, 2, 20, new CellActivity());
            Cell activeCell3 = new Cell(2, 2, 10, new CellActivity());
            Cell activeCell4 = new Cell(2, 3, 30, new CellActivity());
            Cell activeCell5 = new Cell(2, 4, 40, new CellActivity());            

            connections1.ActiveCells.Add(activeCell1);
            connections1.ActiveCells.Add(activeCell2);
            connections1.ActiveCells.Add(activeCell3);            

            connections2.ActiveCells.Add(activeCell1);
            connections2.ActiveCells.Add(activeCell4);
            connections2.ActiveCells.Add(activeCell5);

            ComputeCycle computeCycle4 = new ComputeCycle(connections1);
            ComputeCycle computeCycle5 = new ComputeCycle(connections1);
            ComputeCycle computeCycle6 = new ComputeCycle(connections2);

            //computeCycle4 and computeCycle5 are same by value
            Assert.IsTrue(computeCycle4.Equals(computeCycle5));
            Assert.IsTrue(computeCycle4.ActiveCells.ElementsEqual(computeCycle5.ActiveCells));

            //computeCycle4 and computeCycle6 are NOT same by value
            Assert.IsFalse(computeCycle4.Equals(computeCycle6));
            Assert.IsFalse(computeCycle4.ActiveCells.ElementsEqual(computeCycle6.ActiveCells));
            #endregion

            #region Test with same/different WinnerCells
            Cell winnerCell1 = new Cell(10, 10, 10, new CellActivity());
            Cell winnerCell2 = new Cell(12, 22, 20, new CellActivity());
            Cell winnerCell3 = new Cell(22, 32, 30, new CellActivity());

            connections1.WinnerCells.Add(winnerCell1);
            connections1.WinnerCells.Add(winnerCell2);

            connections2.WinnerCells.Add(winnerCell2);
            connections2.WinnerCells.Add(winnerCell3);

            ComputeCycle computeCycle7 = new ComputeCycle(connections1);
            ComputeCycle computeCycle8 = new ComputeCycle(connections1);
            ComputeCycle computeCycle9 = new ComputeCycle(connections2);

            //computeCycle7 and computeCycle8 are same by value
            Assert.IsTrue(computeCycle7.Equals(computeCycle8));
            Assert.IsTrue(computeCycle7.WinnerCells.ElementsEqual(computeCycle8.WinnerCells));

            //computeCycle7 and computeCycle9 are NOT same by value
            Assert.IsFalse(computeCycle7.Equals(computeCycle9));
            Assert.IsFalse(computeCycle7.WinnerCells.ElementsEqual(computeCycle9.WinnerCells));
            #endregion

            #region Test with same/different MatchingSegments
            Cell c1 = new Cell(1, 1, 10, new CellActivity());
            Cell c2 = new Cell(1, 1, 10, new CellActivity());
            Cell c3 = new Cell(2, 1, 10, new CellActivity());

            DistalDendrite d1 = new DistalDendrite(c1, 1, 1, 1, 0.5, 10);
            DistalDendrite d2 = new DistalDendrite(c1, 1, 1, 1, 0.5, 10);
            DistalDendrite d3 = new DistalDendrite(c1, 2, 1, 1, 0.5, 10);
            DistalDendrite d4 = new DistalDendrite(c2, 2, 2, 1, 0.5, 10);

            connections1.MatchingSegments.Add(d1);
            connections1.MatchingSegments.Add(d2);

            connections2.MatchingSegments.Add(d3);
            connections2.MatchingSegments.Add(d4);

            ComputeCycle computeCycle10 = new ComputeCycle(connections1);
            ComputeCycle computeCycle11 = new ComputeCycle(connections1);
            ComputeCycle computeCycle12 = new ComputeCycle(connections2);

            //computeCycle10 and computeCycle11 are same by value
            Assert.IsTrue(computeCycle10.Equals(computeCycle11));
            Assert.IsTrue(computeCycle10.MatchingSegments.ElementsEqual(computeCycle11.MatchingSegments));

            //computeCycle10 and computeCycle12 are NOT same by value
            Assert.IsFalse(computeCycle10.Equals(computeCycle12));
            Assert.IsFalse(computeCycle10.MatchingSegments.ElementsEqual(computeCycle12.MatchingSegments));
            #endregion

        }
    }
}
