using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    public partial class HTMSerializationTests
    {
        [TestMethod]
        public void Test()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Cell[] cells = new Cell[2];
            cells[0] = new Cell(12, 14, 16, new CellActivity());

            var distSeg1 = new DistalDendrite(cells[0], 1, 2, 2, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cells[0], 44, 24, 34, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg2);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test)}_123.txt"))
            {
                HtmSerializer2.Serialize(cells, sw);
            }
        }

        [TestMethod]
        public void Test1()
        {
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test1)}_123.txt"))
            {
                HtmSerializer2.Serialize(new List<string> { "bla" }, sw);
            }

        }
        [TestMethod]
        public void Test2()
        {
            var dict = new Dictionary<string, Cell>();

            var cell1 = new Cell(12, 14, 16, new CellActivity());
            var distSeg1 = new DistalDendrite(cell1, 1, 2, 2, 1.0, 100);
            var distSeg2 = new DistalDendrite(cell1, 2, 2, 12, 1.0, 100);
            cell1.DistalDendrites.Add(distSeg1);
            cell1.DistalDendrites.Add(distSeg2);
            dict.Add("1", cell1);

            var cell2 = new Cell(12, 14, 16, new CellActivity());
            var distSeg3 = new DistalDendrite(cell2, 44, 24, 34, 1.0, 100);
            cell2.DistalDendrites.Add(distSeg3);
            dict.Add("2", new Cell(12, 14, 16, new CellActivity()));


            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test2)}_123.txt"))
            {
                HtmSerializer2.Serialize(dict, sw);
            }

        }

        [TestMethod]
        public void DeserializationArrayTest()
        {
            var content = "[45, 34]";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var reader = new StreamReader(ms);

            var res = HtmSerializer2.Deserialize<int[]>(reader);
        }
    
        [TestMethod]
        public void DeserializationListTest()
        {
            var content = "[45, 34]";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var reader = new StreamReader(ms);

            var res = HtmSerializer2.Deserialize<List<int>>(reader);
        }
    }
}
