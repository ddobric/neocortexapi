// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.DistributedCompute;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Linq;



#if USE_AKKA
namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to Akka dictionary implementation.
    /// </summary>
    [TestClass]
    public class AkkDictTests
    {


        /// <summary>
        /// Defines placement 
        /// Examples:
        /// Nodes = 2, Cols = 7 => Node 0: {0,1,2,3}, Node 1: {4,5,6}
        /// Nodes = 3, Cols = 7 => Node 0: {0,1,2}, Node 1: {3,4,5}, Node 1: {6}
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="elements"></param>
        /// <param name="placingElement"></param>
        [TestMethod]
        [DataRow(2, 6, 3, 1)]
        [DataRow(2, 6, 5, 1)]
        [DataRow(2, 6, 6, 2)]

        [DataRow(2, 7, 3, 0)]
        [DataRow(2, 7, 0, 0)]
        [DataRow(2, 7, 1, 0)]
        [DataRow(2, 7, 2, 0)]
        [DataRow(2, 7, 4, 1)]
        [DataRow(2, 7, 5, 1)]
        [DataRow(2, 7, 6, 1)]

        [DataRow(3, 7, 2, 0)]
        [DataRow(3, 7, 3, 1)]
        [DataRow(3, 7, 5, 1)]
        [DataRow(3, 7, 6, 2)]

        public void UniformPartitioningTest(int nodes, int elements, int placingElement, int expectedNode)
        {
            var targetNode = HtmSparseIntDictionary<Column>.GetPlacementSlotForElement(nodes, elements, placingElement);

            Assert.IsTrue(targetNode == expectedNode);
        }


        /// <summary>
        /// Generates the map of partitions and key placements.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="partitionsPerNode"></param>
        /// <param name="elements"></param>
        /// <param name="placingElement"></param>
        /// <param name="expectedNode"></param>
        /// <param name="expectedPartition"></param>
        [TestMethod]
        [DataRow(new string[] { "url1" }, 200, 1024)]
        [DataRow(new string[] { "url1", "url2", "url3" }, 5, 17)]
        [DataRow(new string[] { "url1", "url2", "url3" }, 5, 31)]
        [DataRow(new string[] { "url1" }, 10, 4096)]
        public void PartitionMapTest(string[] nodes, int partitionsPerNode, int elements)
        {
            var nodeList = new List<string>();
            nodeList.AddRange(nodes);
            var map = HtmSparseIntDictionary<Column>.CreatePartitionMap(nodeList, elements, partitionsPerNode);

            // System can allocate less partitions than requested.
            Assert.IsTrue(map.Count <= nodes.Length * partitionsPerNode);

            Assert.IsTrue((map[7].MinKey == 14 && map[7].MaxKey == 15)
                || (map[7].MinKey == 21 && map[7].MaxKey == 23)
                || (map[9].MinKey == 3690 && map[9].MaxKey == 4095)
                || (map[170].MinKey == 1020 && map[170].MaxKey == 1023));

            Assert.IsTrue((int)(object)map.Last().MaxKey >= (elements - 1));

            foreach (var item in map)
            {
                // Partition can hold a single element.
                Assert.IsTrue((int)(object)item.MaxKey >= (int)(object)item.MinKey);
            }
        }


        [TestMethod]
        [DataRow(new string[] { "url1", "url2", "url3" }, 3, 17, 15, 2, 7)]
        [DataRow(new string[] { "url1", "url2", "url3" }, 5, 17, 0, 0, 0)]
        [DataRow(new string[] { "url1", "url2", "url3" }, 5, 17, 16, 1, 8)]
        [DataRow(new string[] { "url1", "url2", "url3" }, 5, 33, 22, 1, 7)]
        [DataRow(new string[] { "url1", "url2", "url3" }, 5, 31, 22, 1, 7)]
        public void PartitionLookupTest(string[] nodes, int partitionsPerNode, int elements, int key, int expectedNode, int expectedPartition)
        {
            var nodeList = new List<string>();
            nodeList.AddRange(nodes);
            var map = HtmSparseIntDictionary<Column>.CreatePartitionMap(nodeList, elements, partitionsPerNode);

            var part = HtmSparseIntDictionary<Column>.GetPlacementSlotForElement(map, key);

            Assert.IsTrue(part.NodeIndx == expectedNode && part.PartitionIndx == expectedPartition);

        }

        /// <summary>
        /// Writes and reads coulmns to distributed dictionary.
        /// </summary>
        [TestMethod]
        [TestCategory("AkkaHostRequired")]
        public void InitAkkaDictionaryTest()
        {
            //Thread.Sleep(5000);

            var akkaDict = new HtmSparseIntDictionary<Column>(new HtmSparseIntDictionaryConfig()
            {
                //HtmActorConfig = new HtmConfig()
                //{
                //     ColumnTopology = new HtmModuleTopology()
                //     {
                //          Dimensions = new int[] { 100, 200 },
                //           IsMajorOrdering = false,
                //     }                  
                //},

                Nodes = Helpers.DefaultNodeList,
            });

            for (int i = 0; i < 100; i++)
            {
                akkaDict.Add(i, new Column(32, i, 0.0, 0));
            }

            for (int i = 0; i < 100; i++)
            {
                Column col;
                Assert.IsTrue(akkaDict.TryGetValue(i, out col));
            }

            Column col2;
            Assert.IsFalse(akkaDict.TryGetValue(-1, out col2));

            Assert.IsTrue(100 == akkaDict.Count);
        }


        [TestMethod]
        [TestCategory("AkkaHostRequired")]
        [TestCategory("LongRunning")]
        [DataRow(true)]
        [DataRow(false)]
        public void InitDistributedTest(bool isMultinode)
        {
            Thread.Sleep(5000);

            for (int test = 0; test < 3; test++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                int inputBits = 1024;
                int numOfColumns = 4096;

                ThreadSafeRandom rnd = new ThreadSafeRandom(42);

                var parameters = Parameters.getAllDefaultParameters();
                parameters.Set(KEY.POTENTIAL_RADIUS, inputBits);
                parameters.Set(KEY.POTENTIAL_PCT, 1.0);
                parameters.Set(KEY.GLOBAL_INHIBITION, true);

                parameters.Set(KEY.RANDOM, rnd);

                parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * numOfColumns);
                parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);

                parameters.Set(KEY.POTENTIAL_RADIUS, inputBits);
                parameters.Set(KEY.POTENTIAL_PCT, 1.0);

                parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);       //***
                parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);   //***
                parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);      //***

                parameters.Set(KEY.INHIBITION_RADIUS, (int)0.025 * inputBits);

                parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);

                parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
                parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
                parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
                parameters.Set(KEY.MAX_BOOST, 100);
                parameters.Set(KEY.WRAP_AROUND, true);
                parameters.Set(KEY.SEED, 1956);

                int[] inputDims = new int[] { (int)Math.Sqrt(inputBits), (int)Math.Sqrt(inputBits) };
                parameters.setInputDimensions(inputDims);

                int[] colDims = new int[] { (int)Math.Sqrt(numOfColumns), (int)Math.Sqrt(numOfColumns) };
                parameters.setColumnDimensions(colDims);

                SpatialPooler sp;

                if (isMultinode)
                    sp = new SpatialPoolerParallel();
                else
                    sp = new SpatialPoolerMT();

                var mem = new Connections();

                parameters.apply(mem);

                if (isMultinode)
                    sp.Init(mem, UnitTestHelpers.GetMemory(mem.HtmConfig));
                else
                    sp.Init(mem, UnitTestHelpers.GetMemory());

                sw.Stop();
                Console.Write($"{(float)sw.ElapsedMilliseconds / (float)1000} | ");
            }
        }

        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output directory.
        /// </summary>
        /// <param name="mnistImage">original Image directory used in the test</param>
        /// <param name="imageSizes">list of sizes used for testing. Image would have same value for width and length</param>
        /// <param name="topologies">list of sparse space size. Sparse space has same width and length</param>
        [TestMethod]
        [TestCategory("AkkaHostRequired")]
        [TestCategory("LongRunning")]
        [DataRow("MnistPng28x28\\training", "5", 28, 300)]
        public void SparseSingleMnistImageTest(string trainingFolder, string digit, int imageSize, int columnTopology)
        {
            //Thread.Sleep(3000);

            string TestOutputFolder = $"Output-{nameof(SparseSingleMnistImageTest)}";

            var trainingImages = Directory.GetFiles(Path.Combine(trainingFolder, digit));

            //if (Directory.Exists(TestOutputFolder))
            //    Directory.Delete(TestOutputFolder, true);

            Directory.CreateDirectory(TestOutputFolder);

            Directory.CreateDirectory($"{TestOutputFolder}\\{digit}");

            int counter = 0;
            var numOfActCols = columnTopology * columnTopology;

            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, imageSize * imageSize);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            parameters.Set(KEY.GLOBAL_INHIBITION, true);

            parameters.Set(KEY.RANDOM, rnd);

            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * 64 * 64);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);

            parameters.Set(KEY.POTENTIAL_RADIUS, imageSize * imageSize);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);

            parameters.Set(KEY.STIMULUS_THRESHOLD, 50.0);       //***
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);   //***
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);      //***

            //parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);       //***
            //parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.0);   //***
            //parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.0);      //***

            parameters.Set(KEY.INHIBITION_RADIUS, (int)0.025 * imageSize * imageSize);

            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.2);

            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000);
            parameters.Set(KEY.MAX_BOOST, 100);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.SEED, -1);
            parameters.setInputDimensions(new int[] { imageSize, imageSize });
            parameters.setColumnDimensions(new int[] { columnTopology, columnTopology });

            var sp = new SpatialPoolerParallel();

            var mem = new Connections();

            parameters.apply(mem);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            sp.Init(mem, UnitTestHelpers.GetMemory());
            sw.Stop();
            Debug.WriteLine($"Init time: {sw.ElapsedMilliseconds}");
            int actiColLen = numOfActCols;

            int[] activeArray = new int[actiColLen];

            string outFolder = $"{TestOutputFolder}\\{digit}\\{columnTopology}x{columnTopology}";

            Directory.CreateDirectory(outFolder);

            string outputHamDistFile = $"{outFolder}\\digit{digit}_{columnTopology}_hamming.txt";

            string outputActColFile = $"{outFolder}\\digit{digit}_{columnTopology}_activeCol.txt";

            using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
            {
                using (StreamWriter swActCol = new StreamWriter(outputActColFile))
                {
                    foreach (var mnistImage in trainingImages)
                    {
                        FileInfo fI = new FileInfo(mnistImage);

                        string outputImage = $"{outFolder}\\digit_{digit}_cycle_{counter}_{columnTopology}_{fI.Name}";

                        string testName = $"{outFolder}\\digit_{digit}_{fI.Name}_{columnTopology}";

                        string inputBinaryImageFile = Helpers.BinarizeImage($"{mnistImage}", imageSize, testName);

                        //Read input csv file into array
                        int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                        int numIterationsPerImage = 5;
                        int[] oldArray = new int[activeArray.Length];
                        List<double[,]> overlapArrays = new List<double[,]>();
                        List<double[,]> bostArrays = new List<double[,]>();

                        for (int k = 0; k < numIterationsPerImage; k++)
                        {
                            sw.Reset();
                            sw.Start();
                            sp.compute(inputVector, activeArray, true);
                            sw.Stop();
                            Debug.WriteLine($"Compute time: {sw.ElapsedMilliseconds}");

                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                            var distance = MathHelpers.GetHammingDistance(oldArray, activeArray);
                            swHam.WriteLine($"{counter++}|{distance} ");

                            oldArray = new int[actiColLen];
                            activeArray.CopyTo(oldArray, 0);

                            //var mem = sp.GetMemory(layer);
                            overlapArrays.Add(ArrayUtils.Make2DArray<double>(ArrayUtils.ToDoubleArray(mem.Overlaps), columnTopology, columnTopology));
                            bostArrays.Add(ArrayUtils.Make2DArray<double>(mem.BoostedOverlaps, columnTopology, columnTopology));
                        }

                        var activeStr = Helpers.StringifyVector(activeArray);
                        swActCol.WriteLine("Active Array: " + activeStr);

                        int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(activeArray, columnTopology, columnTopology);
                        twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
                        List<int[,]> arrays = new List<int[,]>();
                        arrays.Add(twoDimenArray);
                        arrays.Add(ArrayUtils.Transpose(ArrayUtils.Make2DArray<int>(inputVector, (int)Math.Sqrt(inputVector.Length), (int)Math.Sqrt(inputVector.Length))));

                        const int OutImgSize = 1024;
                        NeoCortexUtils.DrawBitmaps(arrays, outputImage, Color.Yellow, Color.Gray, OutImgSize, OutImgSize);
                        //NeoCortexUtils.DrawHeatmaps(overlapArrays, $"{outputImage}_overlap.png", OutImgSize, OutImgSize, 150, 50, 5);
                        //NeoCortexUtils.DrawHeatmaps(bostArrays, $"{outputImage}_boost.png", OutImgSize, OutImgSize, 150, 50, 5);
                    }
                }
            }
        }


        /// <summary>
        /// Ensures that pool instance inside of Column.DentrideSegment.Synapses[n].Pool is correctlly serialized.
        /// </summary>
        [TestMethod]
        public void TestColumnSerialize()
        {
            //Thread.Sleep(5000);

            //Pool pool = new Pool(10, 10);

            Cell cell = new Cell();

            DistalDendrite dist = new DistalDendrite(cell, 0, 0, 0, 0.5, 10);
            dist.Synapses.Add(new Synapse(cell, -1, 0, 0.5));
            Synapse syn = new Synapse(cell, dist.SegmentIndex, 0, 0);

            var dict = UnitTestHelpers.GetMemory();

            Column col = new Column(10, 0, 0.01, 10);
            col.ProximalDendrite = new ProximalDendrite(0, 0.01, 10);
            col.ProximalDendrite.Synapses = new List<Synapse>();
            col.ProximalDendrite.Synapses.Add(syn);

            dict.ColumnDictionary.Add(0, col);

            var serCol = AkkaSb.Net.ActorReference.SerializeMsg(col);
            Assert.IsTrue(dict.ColumnDictionary[0].ProximalDendrite.Synapses.Count> 0);
        }

        [TestMethod]
        public void CreateDistributedArrayTest()
        {
            int[] dims;

            var arr = new InMemoryArray(1, typeof(int), dims = new int[] { 100, 100 });

            SparseBinaryMatrix m = new SparseBinaryMatrix(dims, true, arr);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    m.set(7, i, j);
                }
            }
        }


        public class MockedActor : IActorRef
        {
            string Url;

            public MockedActor(string id)
            {
                this.Url = id;
            }

            public ActorPath Path => throw new NotImplementedException();

            public int CompareTo(IActorRef other)
            {
                throw new NotImplementedException();
            }

            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }

            public bool Equals(IActorRef other)
            {
                return ((MockedActor)other).Url == this.Url;
            }

            public void Tell(object message, IActorRef sender)
            {
                throw new NotImplementedException();
            }

            public ISurrogate ToSurrogate(ActorSystem system)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return this.Url;
            }
        }

        /// <summary>
        /// Test if the list of partitions is correctly grouped by nodes.
        /// </summary>
        [TestMethod]
        [DataRow(new string[] { "url1", "url2", "url3" })]
        public void TestGroupingByNode(string[] nodes)
        {
            var nodeList = new List<string>(nodes);

            List<IActorRef> list = new List<IActorRef>();
            for (int k = 0; k < nodes.Length; k++)
            {
                list.Add(new MockedActor(nodes[k]));
            }

            var map = HtmSparseIntDictionary<Column>.CreatePartitionMap(nodeList, 4096, 10);
            int i = 0;
            foreach (var item in map)
            {
                item.ActorRef = list[i++ % nodes.Length];
            }

            var groupedNodes = HtmSparseIntDictionary<object>.GetPartitionsByNode(map);
        }

        [TestMethod]
        [DataRow(10, 4096)]
        [DataRow(5, 20)]
      
        public void ActorSbPartitionMapTest(int elementsPerPartition, int totalElements)
        {
            var map = ActorSbDistributedDictionaryBase<Column>.CreatePartitionMap(totalElements, elementsPerPartition);

            int lastMax = -1;
            int elementCnt = 0;

            foreach (var item in map)
            {
                Assert.IsTrue(item.MinKey == lastMax + 1);
                Assert.IsTrue(item.MaxKey > item.MinKey);

                elementCnt += item.MaxKey - item.MinKey + 1;

                lastMax = item.MaxKey;
            }

            Assert.IsTrue(elementCnt == totalElements);

        }

        [TestMethod]
        //[DataRow(-1, 90001, 35, new string[] { "node1", "node2", "node3" })]
        [DataRow(-1, 90000, 35, new string[] { "node1", "node2", "node3" })]
        [DataRow(-1, 20, -1, new string[] { "node1", "node2", "node3" })]
        [DataRow(-1, 20, -1, new string[] { "node1", "node2" })]
        public void ActorSbPartitionMapTestWithNodes(int elementsPerPartition, int totalElements, int numOfPartitions, string[] nodes)
        {
            var nodeList = new List<string>();
            if (nodes != null)
                nodeList.AddRange(nodes);

            var map = ActorSbDistributedDictionaryBase<Column>.CreatePartitionMap(totalElements, elementsPerPartition, numOfPartitions, nodeList);

            int lastMax = -1;
            int elementCnt = 0;

            foreach (var item in map)
            {
                Assert.IsTrue(item.MinKey == lastMax + 1);
                Assert.IsTrue(item.MaxKey > item.MinKey);

                elementCnt += item.MaxKey - item.MinKey + 1;

                lastMax = item.MaxKey;
            }

            Assert.IsTrue(elementCnt == totalElements);

        }

        ///// <summary>
        ///// Ensures that all keys (items in the list) are grouped in pages inside of a single partition.
        ///// </summary>
        //[TestMethod()]
        //public void TestPagedPartitioning()
        //{
        //    Dictionary<int, List<int>> partitions = new Dictionary<int, List<int>>();
        //    partitions.Add(0, new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        //    partitions.Add(1, new List<int>() { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });
        //    partitions.Add(2, new List<int>() { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 });

        //    var res = SpatialPoolerParallel.SplitPartitionsToPages(3, partitions);

        //    Assert.IsTrue(res.Count == 12);
        //    Assert.IsTrue(res[0].First().Value.Count == 3);
        //    Assert.IsTrue(res[1].First().Value.Count == 3);
        //    Assert.IsTrue(res[2].First().Value.Count == 3);
        //    Assert.IsTrue(res[3].First().Value.Count == 1);

        //    Assert.IsTrue(res[4].First().Value.Count == 3);
        //    Assert.IsTrue(res[5].First().Value.Count == 3);
        //    Assert.IsTrue(res[6].First().Value.Count == 3);
        //    Assert.IsTrue(res[7].First().Value.Count == 1);

        //    Assert.IsTrue(res[8].First().Value.Count == 3);
        //    Assert.IsTrue(res[9].First().Value.Count == 3);
        //    Assert.IsTrue(res[10].First().Value.Count == 3);
        //    Assert.IsTrue(res[11].First().Value.Count == 1);

        //    res = SpatialPoolerParallel.SplitPartitionsToPages(4, partitions);

        //    Assert.IsTrue(res.Count == 9);
        //    Assert.IsTrue(res[0].First().Value.Count == 4);
        //    Assert.IsTrue(res[1].First().Value.Count == 4);
        //    Assert.IsTrue(res[2].First().Value.Count == 2);

        //    Assert.IsTrue(res[3].First().Value.Count == 4);
        //    Assert.IsTrue(res[4].First().Value.Count == 4);
        //    Assert.IsTrue(res[5].First().Value.Count == 2);

        //    Assert.IsTrue(res[6].First().Value.Count == 4);
        //    Assert.IsTrue(res[7].First().Value.Count == 4);
        //    Assert.IsTrue(res[8].First().Value.Count == 2);
        //}
    }
}
#endif


