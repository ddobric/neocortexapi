
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

        [TestMethod]
        public void InitAkkaDictionaryTest()
        {
            Thread.Sleep(2000);

            AkkaDistributedDictionary<int, Column> akkaDict = new AkkaDistributedDictionary<int, Column>(new AkkaDistributedDictionary<int, Column>.AkkaDistributedDictConfig()
            {
               Nodes = this.Nodes,
            });


        }
    }
}
