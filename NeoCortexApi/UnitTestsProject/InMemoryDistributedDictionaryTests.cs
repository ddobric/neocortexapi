using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.DistributedComputeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnitTestsProject
{
    [TestClass]
    public class InMemoryDistributedDictionaryTests
    {
        [TestMethod]
        public void TestInMemoryDictionary()
        {
            InMemoryDistributedDictionary<int, int> dict = new InMemoryDistributedDictionary<int, int>(3);

            for (int i = 0; i < 90; i++)
            {
                dict.Add(i, i);
            }

            int n = 0;
            foreach (var item in dict)
            {
                Assert.AreEqual<int>(item.Key, n);
                Assert.AreEqual<int>(item.Value, n);

                n++;

                Debug.WriteLine(n);
            }
        }
    }
}
