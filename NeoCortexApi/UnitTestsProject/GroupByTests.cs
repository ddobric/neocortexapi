using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;


namespace UnitTestsProject
{
    /// <summary>
    /// Tests for implementation of grouping.
    /// </summary>
    [TestClass]
    public class GroupByTests
    {
        [TestMethod]
        public void IntegerCompare()
        {
            var i1 = new Integer(1);
            var i2 = new Integer(1);
                     
            Assert.IsTrue(i1 ==  i2);
            Assert.IsTrue(i1.Equals(i2));
        }

        [TestMethod]
        public void PairCompare()
        {
            var p1 = new Pair<Integer, Integer>(7, 7);
            var p2 = new Pair<Integer, Integer>(7, 7);

            Assert.IsTrue(p1 == p2);
            Assert.IsTrue(p1.Equals(p2));
        }

        [TestMethod]
        public void testIntegerGroup()
        {
            List<Integer> l = new List<Integer>(new Integer[] { 7, 12, 16 });

            List<Pair<Integer, Integer>> expected = new List<Pair<Integer, Integer>>(
                new Pair<Integer, Integer>[] {
                    new Pair<Integer, Integer>(7, 7),
                    new Pair<Integer, Integer>(12, 12),
                    new Pair<Integer, Integer>(16, 16)
                });

            GroupBy<Integer, Integer> grouper = GroupBy<Integer, Integer>.From(l, (el) => el);

            int i = 0;
            int pairCount = 0;
            foreach (Pair<Integer, Integer> p in grouper)
            {
                Assert.IsTrue(expected[i] == p);
                pairCount++;
                i++;
            }

            Assert.AreEqual(3, pairCount);

            pairCount = 0;

            l = new List<Integer>(new Integer[] { 2, 4, 4, 5 });

            List<Pair<Integer, Integer>> expected2 = new List<Pair<Integer, Integer>>(
                new Pair<Integer, Integer>[] {
                    new Pair<Integer, Integer>(2, 6),
                    new Pair<Integer, Integer>(4, 12),
                    new Pair<Integer, Integer>(4, 12),
                    new Pair<Integer, Integer>(5, 15)
                });

            grouper = GroupBy<Integer, Integer>.From(l, (n) => n * 3);

            i = 0;
            foreach (Pair<Integer, Integer> p in grouper)
            {
                Assert.IsTrue(expected2[i++] == p);
                pairCount++;
            }

            Assert.AreEqual(4, pairCount);
        }

        [TestMethod]
        public void testObjectGroup()
        {
            Column c0 = new Column(9, 0);
            Column c1 = new Column(9, 1);

            // Illustrates the Cell's actual index = colIndex * cellsPerColumn + indexOfCellWithinCol
            Assert.AreEqual(7, c0.Cells[7].Index);
            Assert.AreEqual(12, c1.Cells[3].Index);
            Assert.AreEqual(16, c1.Cells[7].Index);

            DistalDendrite dd0 = new DistalDendrite(c0.Cells[7], 0, 0, 0);
            DistalDendrite dd1 = new DistalDendrite(c1.Cells[3] /* Col 1's Cells start at 9 */, 1, 0, 1);
            DistalDendrite dd2 = new DistalDendrite(c1.Cells[7] /* Col 1's Cells start at 9 */, 2, 0, 2);

            List<DistalDendrite> l = new List<DistalDendrite>(
                new DistalDendrite[] { dd0, dd1, dd2 });


            List<Pair<DistalDendrite, Column>> expected = new List<Pair<DistalDendrite, Column>>(
                new Pair<DistalDendrite, Column>[] {
                new Pair<DistalDendrite, Column>(dd0, c0),
                new Pair<DistalDendrite, Column>(dd1, c1),
                new Pair<DistalDendrite, Column>(dd2, c1)
                });

            GroupBy<DistalDendrite, Column> grouper = GroupBy<DistalDendrite, Column>.From(l, c => c.getParentCell().getParentColumn());

            int i = 0;

            foreach (Pair<DistalDendrite, Column> p in grouper)
            {
                Assert.IsTrue(expected[i++] == p);
            }
        }
    }
}
