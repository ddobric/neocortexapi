using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace UnitTestsProject
{
    [TestClass]
    public class HTM_serialization_connections_Test

    {

        [TestMethod]
        [TestCategory("Serialization")]

        public void SerializeConnectionsTest()
        {
            int[] inputDims = { 3, 4, 5 };
            int[] columnDims = { 35, 43, 52 };
            HtmConfig matrix = new HtmConfig(inputDims, columnDims);
            Connections connections = new Connections(matrix);

            Cell cells = new Cell(12, 14, 16, 18, new CellActivity());

            var distSeg1 = new DistalDendrite(cells, 1, 2, 2, 1.0, 100);

            var distSeg2 = new DistalDendrite(cells, 44, 24, 34, 1.0, 100);

            connections.ActiveSegments.Add(distSeg1);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeConnectionsTest)}.txt"))
            {
                connections.Serialize(sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(SerializeConnectionsTest)}.txt"))
            {
                Connections connections1 = Connections.Deserialize(sr);
                Assert.IsTrue(connections.Equals(connections1));
            }
        }

       
    }
}