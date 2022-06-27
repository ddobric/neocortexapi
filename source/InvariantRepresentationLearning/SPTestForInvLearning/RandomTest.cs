using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTestForInvLearning
{
    [TestClass]
    public class RandomTest
    {
        [TestMethod]
        [TestCategory("Invariant Learning")]
        public void RandomTestRun()
        {
            Random a = new ThreadSafeRandom(42);
            Random b = new ThreadSafeRandom(42);
            for (int i = 0; i < 10; i++)
            {
                Debug.WriteLine($"a is {a.NextDouble()}");
                Debug.WriteLine($"b is {a.NextDouble()}");
                Debug.WriteLine($"are equal: {((a == b) ? "yes" : "no")}");
                Debug.WriteLine("");
            }
        }
    }
}
