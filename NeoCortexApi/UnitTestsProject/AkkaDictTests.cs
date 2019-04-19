
using NeoCortexApi.DataMappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi.Sensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NeoCortexApi.DistributedComputeLib;
using System.Threading;
using NeoCortexApi.DistributedCompute;

namespace UnitTestsProject
{
    [TestClass]
    public class AkkDictTests
    {
        public List<string> Nodes
        {
            get
            {
                var nodes = new List<string>()
                {
                      "akka.tcp://HtmCluster@localhost:8081",
                };

                return nodes;
            }
        }

        /// <summary>
        /// Examples:
        /// Nodes = 2, Cols = 7 => Node 0: {0,1,2,3}, Node 1: {4,5,6}
        /// Nodes = 3, Cols = 7 => Node 0: {0,1,2}, Node 1: {3,4,5}, Node 1: {6}
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="elements"></param>
        /// <param name="placingElement"></param>
        [TestMethod]
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
            var targetNode = HtmSparseIntDictionary.GetNode(nodes, elements, placingElement);

            Assert.IsTrue(targetNode == expectedNode);
        }


        [TestMethod]
        public void InitAkkaDictionaryTest()
       {
            Thread.Sleep(5000);

            var akkaDict = new HtmSparseIntDictionary(new HtmSparseIntDictionaryConfig()
            {
               NumColumns = 20148,
               Nodes = this.Nodes,
            });


        }
    }
}
