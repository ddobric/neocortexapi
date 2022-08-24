using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;


namespace UnitTestsProject
{
    public partial class HTMSerializationTests
    {

        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_Cell()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);
            cell.DistalDendrites.Add(distDend);

            
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_Cell)}_cell.txt"))
            {
                HtmSerializer2.Serialize(cell, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_Cell)}_cell.txt"))
            {
                var cellD = HtmSerializer2.Deserialize<Cell>(sr);
                Assert.IsTrue(cell.Equals(cellD));
            }
        }

        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_Column()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Column column = new Column(2,12,12.2,2);
            
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_Column)}_column.txt"))
            {
                HtmSerializer2.Serialize(column, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_Column)}_column.txt"))
            {
                var columnD = HtmSerializer2.Deserialize<Column>(sr);
                Assert.IsTrue(column.Equals(columnD));
            }
        }


        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_SparseBinaryMatrix()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            int[] dimension = { 100, 100 };

            SparseBinaryMatrix matrix = new SparseBinaryMatrix(dimension,false);            

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_SparseBinaryMatrix)}_sbmatrix.txt"))
            {
                HtmSerializer2.Serialize(matrix, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_SparseBinaryMatrix)}_sbmatrix.txt"))
            {
                var matrixD = HtmSerializer2.Deserialize<SparseBinaryMatrix>(sr);
                Assert.IsTrue(matrix.Equals(matrixD));
            }
        }


        [TestMethod]
        [TestCategory("Serialization")]
        public void Test_ComputeCycle()
        {
            int[] inputDims = { 100, 100 };
            int[] columnDims = { 10, 10 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            connections.ActiveSegments.Add(distDend);

            ComputeCycle computeCycle = new ComputeCycle(connections);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_ComputeCycle)}_compute.txt"))
            {
                HtmSerializer2.Serialize(computeCycle, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_ComputeCycle)}_compute.txt"))
            {
                ComputeCycle computeCycleD = HtmSerializer2.Deserialize<ComputeCycle>(sr);
                Assert.IsTrue(computeCycle.Equals(computeCycleD));
            }
        }


        [TestMethod]
        [TestCategory("serialize_test")]
        public void Test_Connections()
        {
            int[] inputDims = { 100,100 };
            int[] columnDims = { 10,10 };
            HtmConfig config = new HtmConfig(inputDims, columnDims);

            Connections connections = new Connections(config);

            Cell cell = new Cell(12, 14, 16, new CellActivity());

            var distDend = new DistalDendrite(cell, 1, 2, 2, 1.0, 100);

            connections.ActiveSegments.Add(distDend);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_Connections)}_connections.txt"))
            {
                HtmSerializer2.Serialize(connections, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_Connections)}_connections.txt"))
            {
                Connections connectionsD = HtmSerializer2.Deserialize<Connections>(sr);
                Assert.IsTrue(connections.Equals(connectionsD));
            }
        }

        [TestMethod]
        [TestCategory("Serialization")]
        public void Test_HtmConfig()
        {
            int[] inputDims = { 10, 12,14 };
            int[] columnDims = { 1, 2, 3 };

            HtmConfig config = new HtmConfig(inputDims, columnDims);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_HtmConfig)}_config.txt"))
            {
                HtmSerializer2.Serialize(config, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_HtmConfig)}_config.txt"))
            {
                HtmConfig configD = HtmSerializer2.Deserialize<HtmConfig>(sr);
                Assert.IsTrue(config.Equals(configD));
            }
        }


        [TestMethod]
        [TestCategory("Serialization")]
        public void Test_BurstingResult()
        {
            Cell[] cells = new Cell[2];

            cells[0] = new Cell(12, 14, 16, new CellActivity());
            cells[1] = new Cell(22, 33, 44, new CellActivity());             

            BurstingResult burstingResult = new BurstingResult(cells, cells[0]);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test_HtmConfig)}_config.txt"))
            {
                HtmSerializer2.Serialize(burstingResult, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test_HtmConfig)}_config.txt"))
            {
                BurstingResult burstingResultD = HtmSerializer2.Deserialize<BurstingResult>(sr);
                Assert.IsTrue(burstingResult.Equals(burstingResultD));
            }
        }

    }
}
