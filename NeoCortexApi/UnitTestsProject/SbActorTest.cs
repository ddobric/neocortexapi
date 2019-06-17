using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    public class SbAkkaTest
    {

        [TestMethod]
        [DataRow(new int[] { 2048, 6 })]
        [DataRow(new int[] { 100, 20 })]
        public void InitActorSystem(int[] data)
        {
         

        }
    }
}
