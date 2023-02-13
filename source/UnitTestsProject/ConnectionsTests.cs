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
            Connections connections5 = new Connections(config);

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

            var distDend3 = new DistalDendrite(cellsGroup2[1], 1, 2, 2, 1.0, 100);
            var distDend4 = new DistalDendrite(cellsGroup2[5], 10, 20, 20, 10.0, 256);

            connections1.ActiveSegments.Add(distDend1);
            connections1.ActiveSegments.Add(distDend2);

            connections2.ActiveSegments.Add(distDend1);
            connections2.ActiveSegments.Add(distDend2);

            connections5.Cells = cellsGroup1;
            connections5.ActiveSegments.Add(distDend3);
            connections5.ActiveSegments.Add(distDend4);

            //connections1 and connections2 are same by value
            Assert.IsTrue(connections1.Equals(connections2));

            //connections1 and connections5 are NOT same by value
            Assert.IsFalse(connections1.Equals(connections5)); 
            #endregion

        }
    }
}
