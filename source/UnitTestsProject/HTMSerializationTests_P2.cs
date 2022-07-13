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
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Test()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Cell[] cells = new Cell[2];
            cells[0] = new Cell(12, 14, 16, 18, new CellActivity());

            var distSeg1 = new DistalDendrite(cells[0], 1, 2, 2, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg1);

            var distSeg2 = new DistalDendrite(cells[0], 44, 24, 34, 1.0, 100);
            cells[0].DistalDendrites.Add(distSeg2);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test)}_123.txt"))
            {
                HtmSerializer2.Serialize(cells, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test)}_123.txt"))
            {
                var c = HtmSerializer2.Deserialize<Cell[]>(sr);
            }
        }

        [TestMethod]
        public void Test1()
        {
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test1)}_123.txt"))
            {
                HtmSerializer2.Serialize(new List<string> { "bla" }, null, sw);
            }

        }
        [TestMethod]
        public void Test2()
        {
            var dict = new Dictionary<string, Cell>();

            var cell1 = new Cell(12, 14, 16, 18, new CellActivity());
            var distSeg1 = new DistalDendrite(cell1, 1, 2, 2, 1.0, 100);
            var distSeg2 = new DistalDendrite(cell1, 2, 2, 12, 1.0, 100);
            cell1.DistalDendrites.Add(distSeg1);
            cell1.DistalDendrites.Add(distSeg2);
            dict.Add("1", cell1);

            var cell2 = new Cell(12, 14, 16, 18, new CellActivity());
            var distSeg3 = new DistalDendrite(cell2, 44, 24, 34, 1.0, 102);
            cell2.DistalDendrites.Add(distSeg3);
            dict.Add("2", cell2);


            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test2)}_123.txt"))
            {
                HtmSerializer2.Serialize(dict, null, sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test2)}_123.txt"))
            {
                var content = sr.ReadToEnd();

            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(Test2)}_123.txt"))
            {
                var dict1 = HtmSerializer2.Deserialize<Dictionary<string, Cell>>(sr);

                foreach (var key in dict.Keys)
                {
                    dict1.TryGetValue(key, out Cell cell);
                    Assert.IsTrue(dict[key].Equals(cell));
                }
            }

        }

        [TestMethod]
        public void Test3()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            DistalDendrite[] dd = new DistalDendrite[2];
            dd[0] = new DistalDendrite(null, 1, 2, 2, 1.0, 100);

            dd[1] = new DistalDendrite(null, 44, 24, 34, 1.0, 100);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test)}_123.txt"))
            {
                HtmSerializer2.Serialize(dd, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test)}_123.txt"))
            {
                var d = HtmSerializer2.Deserialize<DistalDendrite[]>(sr);
            }
        }

        [TestMethod]
        public void Test4()
        {
            HtmSerializer2 serializer = new HtmSerializer2();

            Dictionary<string, Bla> dict = new Dictionary<string, Bla>
            {
                {"1", new Bla{ Id = 1, Name = "real", In = new List<Internal>() } },
                {"2", new Bla{ Id = 21, Name = "real1", In = new List<Internal>{ new Internal{ Risk = 0.1f } } } },
            };

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(Test4)}_123.txt"))
            {
                HtmSerializer2.Serialize(dict, null, sw);
            }

            using (StreamReader sr = new StreamReader($"ser_{nameof(Test4)}_123.txt"))
            {
                var d = HtmSerializer2.Deserialize<Dictionary<string, Bla>>(sr);
            }
        }

        private class Bla
        {
            public string Name { get; set; }
            public int Id { get; set; }

            public List<Internal> In { get; set; }
        }

        private class Internal
        {
            public float Risk { get; set; }
        }

        [TestMethod]
        public void DeserializationArrayTest()
        {
            var array = new int[] { 45, 35 };
            
            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(array, null, sw);
            }
            var reader = new StreamReader($"{TestContext.TestName}.txt");

            var res = HtmSerializer2.Deserialize<int[]>(reader);

            Assert.IsTrue(array.SequenceEqual(res));
        }

        [TestMethod]
        public void DeserializeIEnumerableTest()
        {
            var array = new List<int> { 45, 34 };

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(array, null, sw);
            }
            var reader = new StreamReader($"{TestContext.TestName}.txt");

            var res = HtmSerializer2.Deserialize<List<int>>(reader);

            Assert.IsTrue(array.SequenceEqual(res));
        }

        [TestMethod]
        public void DeserializeIEnumerable1Test()
        {
            var array = new List<int> { 45, 34 };

            using (var sw = new StreamWriter($"{TestContext.TestName}.txt"))
            {
                HtmSerializer2.Serialize(array, null, sw);
            }
            var reader = new StreamReader($"{TestContext.TestName}.txt");

            var res = HtmSerializer2.Deserialize<int[]>(reader);

            Assert.IsTrue(array.SequenceEqual(res));
        }
    }
}
