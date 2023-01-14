using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to Column.
    /// </summary>
    [TestClass]
    public class SparseBinaryMatrixsTests
    {
        /// <summary>
        /// Column Equal method tests.
        /// TODO: Compare ProximalDendrite and ConnectedInputCounterMatrix
        /// </summary>
        [TestMethod]
        public void CompareSparseBinaryMatrixs()
        {
            int[] dimensions = new int[] { 100, 100 };
            int[] dimensions2 = new int[] { 10000, 10000 };
            bool useColumnMajorOrdering = true;
            bool useColumnMajorOrdering2 = false;

            #region Test empty matrixs with different input parameters
            SparseBinaryMatrix matrix1 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);
            SparseBinaryMatrix matrix2 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);
            SparseBinaryMatrix matrix3 = new SparseBinaryMatrix(dimensions2, useColumnMajorOrdering);
            SparseBinaryMatrix matrix4 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering2);

            //Not same by reference
            Assert.IsFalse(matrix1 == matrix2);
            Assert.IsFalse(matrix1 == matrix3);
            Assert.IsFalse(matrix1 == matrix4);

            //matrix1 and matrix2 are same by value
            Assert.IsTrue(matrix1.Equals(matrix2));

            //matrix1 and matrix3/matrix4 are NOT same by value (different input parameters)
            Assert.IsFalse(matrix1.Equals(matrix3));
            Assert.IsFalse(matrix1.Equals(matrix4));
            #endregion

            #region Test matrixs with same input parameters but different values inside
            SparseBinaryMatrix matrix5 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);
            SparseBinaryMatrix matrix6 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);
            SparseBinaryMatrix matrix7 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);
            SparseBinaryMatrix matrix8 = new SparseBinaryMatrix(dimensions, useColumnMajorOrdering);

            for (int i = 0; i < 100; i++)
            {
                matrix5.set(7, new int[] { i, 1 });
                matrix6.set(7, new int[] { i, 1 });
                matrix7.set(7, new int[] { i, 2 });
                matrix8.set(9, new int[] { i, 1 });
            }

            //matrix5 and matrix6 should be equal
            Assert.IsTrue(matrix5.Equals(matrix6));

            //Check if values of both matrix5 and matrix6 are equal
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(matrix5.get(new int[] { i, 1 }).Equals(matrix6.get(new int[] { i, 1 })));
            }

            //matrix5 and matrix7 should not be equal
            Assert.IsFalse(matrix5.Equals(matrix7));
            Assert.IsFalse(matrix5.Equals(matrix8));

            //Check values
            Assert.IsTrue(matrix5.get(new int[] { 22, 1 }) == 7);
            Assert.IsTrue(matrix6.get(new int[] { 0, 1 }) == 7);
            Assert.IsTrue(matrix7.get(new int[] { 2, 2 }) == 7);
            Assert.IsTrue(matrix8.get(new int[] { 10, 1 }) == 9); 
            #endregion

        }
    }
}
