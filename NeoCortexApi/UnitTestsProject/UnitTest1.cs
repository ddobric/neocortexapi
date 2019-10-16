using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        [DataRow(new int[] { 2048, 6 })]
        [DataRow(new int[] { 100, 20 })]
        public void TestMethod1(int[] data)
        {
            byte[] d = new byte[10000];

            var res = initDimensionMultiples(data, 3);

            res = initDimensionMultiples(data, 2);
        }


        [TestMethod]       
        public void KeyTest()
        {
            Dictionary<int[], string> dict = new Dictionary<int[], string>();
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 0, 1, 0, 1});
            list.Add(new int[] { 0, 0, 0, 1 });
            list.Add(new int[] { 0, 1, 0, 1 });

            dict.Add(list[0], "A");
            dict.Add(list[1], "B");
            dict.Add(list[2], "C");

            dict[new int[] { 0, 0, 0, 0 }] = "0";
            dict[new int[] { 0, 1, 0, 1 }] = "0";

            // Assert.IsTrue(dict.ContainsKey(list[0]]));

        }

        protected int[] initDimensionMultiples(int[] dimensions, int numDimensions)
        {
            int holder = 1;
            int len = dimensions.Length;
            int[] dimensionMultiples = new int[numDimensions];
            for (int i = 0; i < len; i++)
            {
                holder *= (i == 0 ? 1 : dimensions[len - i]);
                dimensionMultiples[len - 1 - i] = holder;
            }
            return dimensionMultiples;
        }


        [TestMethod]
        [DataRow(42)]
        public void RandomSeed(int seed)
        {
            ThreadSafeRandom x = new ThreadSafeRandom(seed);
            var res = x.NextDouble();
            Assert.AreEqual(res, 0.668, 0.01);
        }



        [TestMethod]
        public void TestMethod2()
        {
            Topology t = new Topology(new int[] { 2048, 40 });
            int[] coords = new int[] { 200, 10 };
            var indx = HtmCompute.GetFlatIndexFromCoordinates(coords, t.HtmTopology);
        }


        [TestMethod]
        public void TestMethod3()
        {
            Topology t = new Topology(new int[] { 2048, 40 });
            int[] coords = new int[] { 200, 10 };
            var indx = HtmCompute.GetFlatIndexFromCoordinates(coords, t.HtmTopology);
            var coords2 = HtmCompute.GetCoordinatesFromIndex(indx, t.HtmTopology);

            Assert.AreEqual(coords[0], coords2[0]);
            Assert.AreEqual(coords[1], coords2[1]);
        }

        [TestMethod]
        public void ModuloTest()
        {
            int r = ArrayUtils.modulo(2, 7);
            r = ArrayUtils.modulo(3, 7);
            r = ArrayUtils.modulo(6, 7);
            r = ArrayUtils.modulo(7, 7);

            r = ArrayUtils.modulo(14, 7);
            r = ArrayUtils.modulo(-14, 7);
            r = ArrayUtils.modulo(15, 7);
            Assert.IsTrue(r == 1);

            r = ArrayUtils.modulo(-15, 7);
            Assert.IsTrue(r == 6);

            r = ArrayUtils.modulo(20, 7);
            Assert.IsTrue(r == 6);

            r = ArrayUtils.modulo(-20, 7);
            Assert.IsTrue(r == 1);
        }


        [TestMethod]
        public void ColumnCompareTest()
        {
            Column c1 = new Column(10, 0, 0, 0);
            Column c2 = new Column(10, 1, 0, 0);

            List<Column> l = new List<Column>(new Column[] { c1, c2});

            Assert.IsTrue(l.Min(i=>i) == c1);

        }


        [TestMethod]
        public void CastTest()
        {
            int[] a = new int[] { 1, 2, 3, 4, 5 };
            object[] b = Array.ConvertAll(a, item => (object)item);
        }


        [TestMethod]
        public void CompareDentrites()
        {
            var empty1 = (NeoCortexApi.Utility.GroupBy2<object>.Slot < Pair<object, List<object>> > )NeoCortexApi.Utility.GroupBy2<object>.Slot<Pair<object, List<object>>>.empty();
            var empty2 = (NeoCortexApi.Utility.GroupBy2<object>.Slot < Pair<object, List<object>> > )NeoCortexApi.Utility.GroupBy2<object>.Slot<Pair<object, List<object>>>.empty();

            var slot2 = NeoCortexApi.Utility.GroupBy2<object>.Slot<object>.of(7);
            Assert.AreNotEqual(empty1, slot2);

            Assert.AreEqual(empty1, empty2);
        }

        [TestMethod]
        public void CreateDutyCycleGraphTest()
        {
            for (int i = 0; i < 1; i++)
            {

            }
        }

        [TestMethod]
        [DataRow(20)]
        [DataRow(30)]
        [DataRow(40)]
        [DataRow(50)]
        public void TestHeatmapCreation(int threshold)
        {
            List<double[,]> bostArrays = new List<double[,]>();
            bostArrays.Add(new double[64, 64]);
            bostArrays.Add(new double[64, 64]);

            double v = 0;
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    bostArrays[0][i, j] = v;
                    bostArrays[1][j, i] = v;
                }

                v += 1;
            }

            NeoCortexUtils.DrawHeatmaps(bostArrays, $"tessheat_{threshold}.png", 1024, 1024, 60, threshold, 10);

        }

        /// <summary>
        /// Extracts performance data from debug output of test SparseSingleMnistImageTest.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="token"></param>
        [TestMethod]
        [DataRow(@"c:\temp\results 3 nodes.txt", "Compute time: ")]
        public void CutoutTest(string fileName, string token)
        {        
            List<string> data = new List<string>();

            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {                    
                    if (line.ToLower().Contains(token.ToLower()))
                    {
                        var tokens = line.Split(token);
                        data.Add(tokens[1].Trim());
                    }
                }
            }

            foreach (var item in data)
            {
                Debug.WriteLine(item);
            }
        }

        //[TestMethod]
        //public void CompareDentrites()
        //{
        //    DistalDendrite d1 = new DistalDendrite();
        //    DistalDendrite bestSegment = matchingSegments.Max();
        //}


    }
}
