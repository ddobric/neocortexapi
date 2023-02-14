using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to Connection.
    /// </summary>
    [TestClass]
    public class ConnectionsTests
    {
        /// <summary>
        /// Column Equal method tests.
        /// </summary>
        [TestMethod]
        public void CompareConnections()
        {
            
            HtmConfig config = new HtmConfig(new int[] { 100, 100 }, new int[] { 10, 10 });
            HtmConfig config2 = new HtmConfig(new int[] { 10, 10 }, new int[] { 10, 10 });

            //Create connections from config
            Connections connections1 = new Connections(config);
            Connections connections2 = new Connections(config);
            Connections connections3 = new Connections(config2);
            Connections connections4 = new Connections(config);

            #region Compare empty connections with different input configs
            //Not same by reference
            Assert.IsFalse(connections1 == connections2);
            Assert.IsFalse(connections1 == connections3);

            //connections1 and connections2 are same by value
            Assert.IsTrue(connections1.Equals(connections2));

            //connections1 and connections3 are NOT same by value
            Assert.IsFalse(connections1.Equals(connections3));
            #endregion

            #region Compare connections with different cells
            Cell[] cellsGroup1 = new Cell[32];
            Cell[] cellsGroup2 = new Cell[32];

            for (int i = 0; i < 32; i++)
            {
                cellsGroup1[i] = new Cell(parentColumnIndx: 1, colSeq: i + 1, numCellsPerColumn: 32, new CellActivity());
                cellsGroup2[i] = new Cell(parentColumnIndx: 2, colSeq: i + 1, numCellsPerColumn: 32, new CellActivity());
            }

            connections1.Cells = cellsGroup1;
            connections2.Cells = cellsGroup1;
            connections4.Cells = cellsGroup2;

            //connections1 and connections2 are same by value
            Assert.IsTrue(connections1.Equals(connections2));

            //connections1 and connections4 are NOT same by value
            Assert.IsFalse(connections1.Equals(connections4));
            #endregion

            #region Compare connections with different active segments
            var distDend1 = new DistalDendrite(cellsGroup1[2], 1, 2, 2, 1.0, 100);
            var distDend2 = new DistalDendrite(cellsGroup1[4], 10, 20, 20, 10.0, 256);

            var distDend3 = new DistalDendrite(cellsGroup2[1], 2, 2, 2, 1.0, 100);
            var distDend4 = new DistalDendrite(cellsGroup2[5], 10, 20, 20, 10.0, 256);

            connections1.ActiveSegments.Add(distDend1);
            connections1.ActiveSegments.Add(distDend2);

            connections2.ActiveSegments.Add(distDend1);
            connections2.ActiveSegments.Add(distDend2);


            Connections connections5 = new Connections(config);
            connections5.Cells = cellsGroup1;
            connections5.ActiveSegments.Add(distDend3);
            connections5.ActiveSegments.Add(distDend4);

            //connections1 and connections2 are same by value
            Assert.IsTrue(connections1.Equals(connections2));

            //Check active segments value            
            Assert.AreEqual(connections1.ActiveSegments[0], distDend1);
            Assert.AreEqual(connections1.ActiveSegments[1], distDend2);
            Assert.IsTrue(connections1.ActiveSegments.ElementsEqual(connections2.ActiveSegments));

            //connections1 and connections5 are NOT same by value
            Assert.IsFalse(connections1.Equals(connections5));
            #endregion

            #region Compare connections with different matching segments
            var distDend5 = new DistalDendrite(cellsGroup1[3], 1, 1, 3, 1.0, 100);
            var distDend6 = new DistalDendrite(cellsGroup1[5], 1, 2, 20, 10.0, 256);

            var distDend7 = new DistalDendrite(cellsGroup2[10], 1, 2, 3, 1.0, 100);
            var distDend8 = new DistalDendrite(cellsGroup2[11], 10, 20, 22, 10.0, 256);

            Connections connections6 = new Connections(config);
            Connections connections7 = new Connections(config);
            Connections connections8 = new Connections(config);
            connections6.Cells = cellsGroup1;
            connections7.Cells = cellsGroup1;
            connections8.Cells = cellsGroup1;

            connections6.MatchingSegments.Add(distDend5);
            connections6.MatchingSegments.Add(distDend6);

            connections7.MatchingSegments.Add(distDend5);
            connections7.MatchingSegments.Add(distDend6);

            connections8.MatchingSegments.Add(distDend7);
            connections8.MatchingSegments.Add(distDend8);

            //connections6 and connections7 are same by value
            Assert.IsTrue(connections6.Equals(connections7));

            //Check active segments value            
            Assert.AreEqual(connections6.MatchingSegments[0], distDend5);
            Assert.AreEqual(connections6.MatchingSegments[1], distDend6);
            Assert.IsTrue(connections6.MatchingSegments.ElementsEqual(connections7.MatchingSegments));

            //connections6 and connections8 are NOT same by value
            Assert.IsFalse(connections1.Equals(connections8));
            #endregion

            #region Compare connections with different winning/active cells

            Connections connections9 = new Connections(config);
            Connections connections10 = new Connections(config);
            Connections connections11 = new Connections(config);
            Connections connections12 = new Connections(config);

            connections9.Cells = cellsGroup1;
            connections10.Cells = cellsGroup1;
            connections11.Cells = cellsGroup1;
            connections12.Cells = cellsGroup1;

            for (int i = 0; i < 10; i++)
            {
                connections9.ActiveCells.Add(cellsGroup1[i]);
                connections10.ActiveCells.Add(cellsGroup1[i]);
                connections11.ActiveCells.Add(cellsGroup1[i + 10]);
                connections12.ActiveCells.Add(cellsGroup1[i]);
            }
            
            //connections9 and connections10 are same by value
            Assert.IsTrue(connections9.Equals(connections10));

            //Check ActiveCells value            
            Assert.AreEqual(connections9.ActiveCells.Count, 10);

            for(int i = 0; i < 10; i++)
            {
                Assert.AreEqual(connections9.ActiveCells.ElementAt(i), cellsGroup1[i]);
                Assert.AreEqual(connections10.ActiveCells.ElementAt(i), cellsGroup1[i]);
            }

            //connections9 and connections11 are NOT same by value (different ActiveCells)
            Assert.IsFalse(connections9.Equals(connections11));


            for (int i = 0; i < 5; i++)
            {
                connections9.WinnerCells.Add(cellsGroup1[i]);
                connections10.WinnerCells.Add(cellsGroup1[i]);
                connections12.WinnerCells.Add(cellsGroup1[i + 5]);
            }

            //connections9 and connections10 are same by value
            Assert.IsTrue(connections9.Equals(connections10));

            //Check WinningCell value            
            Assert.AreEqual(connections9.WinnerCells.Count, 5);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(connections9.WinnerCells.ElementAt(i), cellsGroup1[i]);
                Assert.AreEqual(connections10.WinnerCells.ElementAt(i), cellsGroup1[i]);
            }

            //connections9 and connections12 are NOT same by value (different WinningCells)
            Assert.IsFalse(connections9.Equals(connections12));

            #endregion
        }
    }
}
