// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnitTestsProject
{
    [TestClass]
    public class ThreadSafeRandomTests
    {
        [TestMethod]
        [TestCategory("Invariant Learning")]
        [TestCategory("Prod")]
        public void RandomTestRun()
        {
            Random tsr1 = new ThreadSafeRandom(42);
            Random tsr2 = new ThreadSafeRandom(42);

            Random r1 = new Random(42);
            Random r2 = new Random(42);


            for (int i = 0; i < 10; i++)
            {
                double val1 = tsr1.NextDouble();
                double val2 = tsr2.NextDouble();

                Debug.WriteLine($"ThreadsafeRandom: {val1} \t {val2}");
                
                Debug.WriteLine($"are equal: {((val1== val2) ? "yes" : "no")}");
                
                Assert.IsTrue(val1 == val2);


                val1 = r1.NextDouble();
                val2 = r2.NextDouble();
                
                Debug.WriteLine($"Random:  {val1} \t {val2}");

                Debug.WriteLine($"are equal: {((val1 == val2) ? "yes" : "no")}");

                Assert.IsTrue(val1==val2);
                
                Debug.WriteLine("");
            }
        }
    }
}
