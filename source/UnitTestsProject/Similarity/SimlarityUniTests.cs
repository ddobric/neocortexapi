using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace NeoCortexApi.Experiments
{
    /// <summary>
    /// Test for similarity calculation.
    /// </summary>
    [TestClass]
    public class SimilarityUnitTests
    {
        /// <summary>
        /// Compares the similarity of different arrays.
        /// </summary>
        [DataRow(new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0 }, 100.0)]
        [DataRow(new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, }, 75.0)]
        [DataRow(new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, }, 75.0)]
        [DataRow(new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, }, 25.0)]
        [DataRow(new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, }, 25.0)]
        [DataRow(new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, }, 0.0)]
        [TestMethod]
        [TestCategory("Prod")]
        public void ScalarSimilaritiesTest(int[] arr1, int[] arr2, double expectedSimilarity)
        {
            var calculatedSimilarity = MathHelpers.CalcArraySimilarity(ArrayUtils.IndexesWithNonZeros(arr1), ArrayUtils.IndexesWithNonZeros(arr2));

            Assert.IsTrue(calculatedSimilarity == expectedSimilarity);

            Console.WriteLine($"{calculatedSimilarity}");
            Console.WriteLine($"{Helpers.StringifyVector(arr1)}");
            Console.WriteLine($"{Helpers.StringifyVector(arr2)}");
            Console.WriteLine();
        }
    }
}
