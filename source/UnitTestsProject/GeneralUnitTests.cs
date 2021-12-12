// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
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
    public class GeneralUnitTests
    {


        [TestMethod]
        public void IntegerTests()
        {
            Integer i1 = new Integer(1);
            Integer i2 = null;

            Assert.IsFalse(i1 == i2);

            Assert.IsFalse(i2 == i1);

            i2 = new Integer(1);

            Assert.IsTrue(i2 == i1);
        }

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
            list.Add(new int[] { 0, 1, 0, 1 });
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
        [TestCategory("Prod")]
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
        [TestCategory("Prod")]
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
        [TestCategory("Prod")]
        public void ModuloTest()
        {
            int r = ArrayUtils.Modulo(2, 7);
            r = ArrayUtils.Modulo(3, 7);
            r = ArrayUtils.Modulo(6, 7);
            r = ArrayUtils.Modulo(7, 7);

            r = ArrayUtils.Modulo(14, 7);
            r = ArrayUtils.Modulo(-14, 7);
            r = ArrayUtils.Modulo(15, 7);
            Assert.IsTrue(r == 1);

            r = ArrayUtils.Modulo(-15, 7);
            Assert.IsTrue(r == 6);

            r = ArrayUtils.Modulo(20, 7);
            Assert.IsTrue(r == 6);

            r = ArrayUtils.Modulo(-20, 7);
            Assert.IsTrue(r == 1);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void ColumnCompareTest()
        {
            Column c1 = new Column(10, 0, 0, 0);
            Column c2 = new Column(10, 1, 0, 0);

            List<Column> l = new List<Column>(new Column[] { c1, c2 });

            Assert.IsTrue(l.Min(i => i) == c1);

        }


        [TestMethod]
        [TestCategory("Prod")]
        public void CastTest()
        {
            int[] a = new int[] { 1, 2, 3, 4, 5 };
            object[] b = Array.ConvertAll(a, item => (object)item);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void CompareGroupedObjects()
        {
            var empty1 = (NeoCortexApi.Utility.GroupBy2<object>.Slot<Pair<object, List<object>>>)NeoCortexApi.Utility.GroupBy2<object>.Slot<Pair<object, List<object>>>.Empty();
            var empty2 = (NeoCortexApi.Utility.GroupBy2<object>.Slot<Pair<object, List<object>>>)NeoCortexApi.Utility.GroupBy2<object>.Slot<Pair<object, List<object>>>.Empty();

            var slot2 = NeoCortexApi.Utility.GroupBy2<object>.Slot<object>.Of(7);
            Assert.AreNotEqual(empty1, slot2);

            Assert.AreEqual(empty1, empty2);
        }



        [TestMethod]
        [DataRow(20)]
        [DataRow(30)]
        [DataRow(40)]
        [DataRow(50)]
        // [TestCategory("Prod")]
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


        [TestMethod]
        [TestCategory("Prod")]
        public void PushToIntervalTest()
        {
            int[] lst = new int[] { 1, 2, 3, 4, 5 };

            lst = ArrayUtils.PushToInterval(lst, 5, 4);

            lst = ArrayUtils.PushToInterval(lst, 5, 5);

            lst = ArrayUtils.PushToInterval(lst, 5, 2);

            Assert.IsTrue(lst[4] == 2);

            Assert.IsTrue(lst[3] == 1);

            Assert.IsTrue(lst[2] == 4);

            Assert.IsTrue(lst[1] == 5);

            Assert.IsTrue(lst[0] == 2);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void RememberArrayTest()
        {
            int numOfElems = 3;
            List<int[]> rememberedSdrs = new List<int[]>();

            List<int[]> allSdrs = new List<int[]>();
            for (int i = 0; i < 100; i++)
            {
                allSdrs.Add(new int[] { i, i, i, i, i });
            }

            for (int i = 0; i < 100; i++)
            {
                rememberedSdrs = ArrayUtils.RememberArray<int>(rememberedSdrs, numOfElems, allSdrs[i]);

                if (i == numOfElems)
                {
                    Assert.IsTrue(rememberedSdrs.Count == numOfElems);

                    for (int k = 0; k < numOfElems - 1; k++)
                    {
                        Assert.IsTrue(rememberedSdrs[k][0] == rememberedSdrs[k + 1][0] + 1);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void CalcSimilarityMatrixTest()
        {
            Dictionary<string, int[]> sdrs = new Dictionary<string, int[]>();
            sdrs.Add("sdr1", new int[] { 1, 2, 3, 0 });
            sdrs.Add("sdr2", new int[] { 1, 4, 6, 8 });
            sdrs.Add("sdr3", new int[] { 2, 9, 11, 10 });
            sdrs.Add("sdr4", new int[] { 0, 14, 20, 21 });

            var res = MathHelpers.CalculateSimilarityMatrix(sdrs);

            for (int i = 0; i < sdrs.Count; i++)
            {
                Assert.IsTrue(res[i, i] == 100);
            }
        }

        /// <summary>
        /// Makes sure that CalulateDimilarity computes the proper result.
        /// </summary>
        [TestMethod]
        [TestCategory("test")]
        public void CalcArraySimilarityTest()
        {
            int[] arr1 = new int[] { 1, 3, 4, 5, 6 };
            int[] arr2 = new int[] { 1, 3, 4, 5, 6 };

            var res = MathHelpers.CalcArraySimilarity(arr1, arr2);

            Assert.IsTrue(res == 100.0);

            arr1 = new int[] { 1, 3, 4, 5, 6 };
            arr2 = new int[] { 5, 6 };

            res = MathHelpers.CalcArraySimilarity(arr1, arr2);

            Assert.IsTrue(res == 40);
        }
        [TestMethod]
        [DataRow(0)]
        [DataRow(10)]
        [DataRow(42)]
        [DataRow(255)]
        public void RandomGenTests(int seed)
        {
            Random rnd1 = new Random(seed);
            Random rnd2 = new Random(seed);

            Console.WriteLine($"Test with the seed {seed}");

            for (int i = 0; i < 100; i++)
            {
                var r1 = rnd1.Next();
                var r2 = rnd2.Next();

                Console.WriteLine($"{r1}\t{r2} / rnd1 == rnd2: {r1 == r2}");
            }

            Console.WriteLine("");
        }
    }
}
