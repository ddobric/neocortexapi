// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;

namespace UnitTestsProject
{
    [TestClass]
    public class GroupBy2Tests
    {
        private Integer m_None = default(Integer);

        private Func<Object, Integer> m_Times1Fnc = x => (Integer)x;

        private Func<Object, Integer> m_Times3Fnc = x => (Integer)x * 3;

        private Func<Object, Integer> m_Times4Fnc = x => (Integer)x * 4;

        private Func<Object, Integer> m_Times5Fnc = x => (Integer)x * 5;

        public List<Object> getList(int i)
        {
            return new List<Object>(new Integer[] { i });
        }

        public List<Object> getList(Integer i)
        {
            return new List<Object>(new Integer[] { i });
        }

        public List<Object> getList(Integer i, Integer j)
        {
            return new List<Object>(new Integer[] { i, j });
        }

        public List<Object> getList(int i, int j)
        {
            return new List<Object>(new Integer[] { i, j });
        }

        public List<List<Object>> getListOfList(List<Object> li)
        {
            return new List<List<Object>>(new List<Object>[] { li });
        }

        public List<List<Object>> getListOfList(List<Object> li, List<Object> lj)
        {
            return new List<List<Object>>(new List<Object>[] { li, lj });
        }

        public List<List<Object>> getListOfList(List<Object> li, List<Object> lj, List<Object> lk)
        {
            return new List<List<Object>>(new List<Object>[] { li, lj, lk });
        }


        public List<List<Object>> getListOfList(List<Object> li, List<Object> lj, List<Object> lk, List<Object> lm)
        {
            return new List<List<Object>>(new List<Object>[] { li, lj, lk, lm });
        }

        [TestMethod]
        public void TestOneSequence()
        {
            List<Object> sequence0 = new List<Object>(new Integer[] { 7, 12, 12, 16 });

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            GroupBy2<Integer> m = GroupBy2<Integer>.Of(list.ToArray());

            List<Pair<Object, List<List<Object>>>> expectedValues = new List<Pair<Object, List<List<Object>>>>(
                new Pair<Object, List<List<Object>>>[]
                {
                    new Pair<Object, List<List<Object>>>(7, getListOfList(getList(7))),
                    new Pair<Object, List<List<Object>>>(12, getListOfList(getList(12, 12))),
                    new Pair<Object, List<List<Object>>>(16, getListOfList(getList(16))),
                });

            assertResults(m, expectedValues);
        }


        /// <summary>
        /// We take two lists: 
        /// L1 = {7, 12, 16}
        /// L2 = {3, 4, 5}
        /// Specified functions are x => x for list 1 and x => x * 3 for list 2.
        /// This means, we expect following key values 
        /// for list1: 7, 12, 16 and
        /// for list2  9, 12, 15.
        /// Sorted keys: 7, 9, 12, 15, 16.
        /// Result: 7-[7,none], 9-[none, 3], 12-[12,4], 15-[none,5], 16-[16, none].
        /// </summary>
        [TestMethod]
        public void TestTwoSequences1()
        {
            List<Object> sequence0 = new List<Object>(new Object[] { new Integer(7), new Integer(12), new Integer(16) });
            List<Object> sequence1 = new List<Object>(new Object[] { new Integer(3), new Integer(4), new Integer(5) });

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence1, m_Times3Fnc));
            GroupBy2<Integer> group = GroupBy2<Integer>.Of(list.ToArray());

            List<Pair<Object, List<List<Object>>>> expectedValues = new List<Pair<Object, List<List<Object>>>>(
            new Pair<Object, List<List<Object>>>[]
            {
                    new Pair<Object, List<List<Object>>>(7, getListOfList(getList(new Integer(7)), getList(m_None))),
                    new Pair<Object, List<List<Object>>>(9, getListOfList(getList(m_None), getList(3))),
                    new Pair<Object, List<List<Object>>>(12, getListOfList(getList(12), getList(4))),
                    new Pair<Object, List<List<Object>>>(15, getListOfList(getList(m_None), getList(5))),
                    new Pair<Object, List<List<Object>>>(16, getListOfList(getList(16), getList(m_None))),
            });

            assertResults(group, expectedValues);
        }


        /// <summary>
        /// We take two lists: 
        /// L1 = {9,}
        /// L2 = {3, 4}
        /// Specified functions are x => x for list 1 and x => x * 3 for list 2.
        /// This means, we expect following key values 
        /// for list1: 9 
        /// for list2  9, 12.
        /// Sorted keys: 9, 12
        /// Result: 9-[9, 3], 12-[none,4]
        /// </summary>
        [TestMethod]
        public void TestTwoSequences2()
        {
            List<Object> sequence0 = new List<Object>(new Object[] { new Integer(9) });
            List<Object> sequence1 = new List<Object>(new Object[] { new Integer(3), new Integer(4) });

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence1, m_Times3Fnc));
            GroupBy2<Integer> group = GroupBy2<Integer>.Of(list.ToArray());

            List<Pair<Object, List<List<Object>>>> expectedValues = new List<Pair<Object, List<List<Object>>>>(
            new Pair<Object, List<List<Object>>>[]
            {
                   new Pair<Object, List<List<Object>>>(9, getListOfList(getList(new Integer(9)), getList(3))),
                   new Pair<Object, List<List<Object>>>(12, getListOfList(getList(m_None), getList(4))),
            });

            assertResults(group, expectedValues);
        }


        /// <summary>
        /// { 7, 12, 16}    with x=>x gives keys { 7, 12, 16 }
        /// { 3, 4, 5 }     with x=>x*3 gives keys { 9, 12, 15 }
        /// { 3, 3, 4, 5}   with x=>x*4 gives keys { 12, 12, 16, 20 }
        /// 
        /// 
        /// Result:
        /// 
        /// Keys: 7,9,12,15,16,20
        /// Groups:
        /// 7   - 7 - null, null
        /// 9   - null - 3, null
        /// 12  - 12 - 4, [3,3]
        /// 15  - null, 5, null
        /// 16  - 16, null, 4
        /// 20  - null, null, 5
        /// 
        /// </summary>
        [TestMethod]
        public void TestThreeSequences()
        {
            List<Object> sequence0 = new List<Object>(new Integer[] { 7, 12, 16 });
            List<Object> sequence1 = new List<Object>(new Integer[] { 3, 4, 5 });
            List<Object> sequence2 = new List<Object>(new Integer[] { 3, 3, 4, 5 });

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence1, m_Times3Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence2, m_Times4Fnc));

            GroupBy2<Integer> group = GroupBy2<Integer>.Of(list.ToArray());

            List<Pair<Object, List<List<Object>>>> expectedValues = new List<Pair<Object, List<List<Object>>>>(
           new Pair<Object, List<List<Object>>>[]
           {
                       new Pair<Object,List<List<Object>>>(7, new List<List<Object>>(getListOfList(getList(7), getList(m_None), getList(m_None)))),
                       new Pair<Object,List<List<Object>>>(9, new List<List<Object>>(getListOfList(getList(m_None), getList(3), getList(m_None)))),
                       new Pair<Object,List<List<Object>>>(12, new List<List<Object>>(getListOfList(getList(12), getList(4), getList(3,3)))),
                       new Pair<Object,List<List<Object>>>(15, new List<List<Object>>(getListOfList(getList(m_None), getList(5), getList(m_None)))),
                       new Pair<Object,List<List<Object>>>(16, new List<List<Object>>(getListOfList(getList(16), getList(m_None), getList(4)))),
                       new Pair<Object,List<List<Object>>>(20, new List<List<Object>>(getListOfList(getList(m_None), getList(m_None), getList(5)))),
           });

            assertResults(group, expectedValues);
        }

        private static void assertResults(GroupBy2<Integer> grp, List<Pair<object, List<List<object>>>> expectedValues)
        {
            int i = 0;
            foreach (var t in grp)
            {
                if (t.Key is Integer)
                    Assert.IsTrue(((Integer)t.Key).Value == (int)expectedValues[i].Key);

                int j = 0;
                foreach (var o in t.Value)
                {
                    for (int k = 0; k < o.Count; k++)
                    {
                        if (o[k] is Integer)
                            Assert.IsTrue((Integer)o[k] == (Integer)expectedValues[i].Value[j][k]);
                        else
                            Assert.IsTrue(o[k] == (Object)expectedValues[i].Value[j][k]);
                    }

                    j++;
                }
                i++;
            }
        }


        [TestMethod]

        public void TestFourSequences()
        {
            List<Object> sequence0 = new List<Object>(new Integer[] { 7, 12, 16 });
            List<Object> sequence1 = new List<Object>(new Integer[] { 3, 4, 5 });
            List<Object> sequence2 = new List<Object>(new Integer[] { 3, 3, 4, 5 });
            List<Object> sequence3 = new List<Object>(new Integer[] { 3, 3, 4, 5 });

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence1, m_Times3Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence2, m_Times4Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence3, m_Times5Fnc));

            GroupBy2<Integer> m = GroupBy2<Integer>.Of(list.ToArray());

            List<Pair<Object, List<List<Object>>>> expectedValues = new List<Pair<Object, List<List<Object>>>>(
           new Pair<Object, List<List<Object>>>[]
           {
                new Pair<Object,List<List<Object>>>(7, new List<List<Object>>(getListOfList(getList(7), getList(m_None), getList(m_None), getList(m_None)))),
                new Pair<Object,List<List<Object>>>(9, new List<List<Object>>(getListOfList(getList(m_None), getList(3), getList(m_None), getList(m_None)))),
                new Pair<Object,List<List<Object>>>(12, new List<List<Object>>(getListOfList(getList(12), getList(4), getList(3,3), getList(m_None)))),
                new Pair<Object,List<List<Object>>>(15, new List<List<Object>>(getListOfList(getList(m_None), getList(5), getList(m_None), getList(3,3)))),
                new Pair<Object,List<List<Object>>>(16, new List<List<Object>>(getListOfList(getList(16), getList(m_None), getList(4), getList(m_None)))),
                new Pair<Object,List<List<Object>>>(20, new List<List<Object>>(getListOfList(getList(m_None), getList(m_None), getList(5), getList(4)))),
                new Pair<Object,List<List<Object>>>(25, new List<List<Object>>(getListOfList(getList(m_None), getList(m_None),  getList(m_None), getList(5)))),
           });

            assertResults(m, expectedValues);
        }
    }
}


