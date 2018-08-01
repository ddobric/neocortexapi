using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;


namespace UnitTestsProject
{
    [TestClass]
    public class GroupBy2Tests
    {
        //private List<Slot> none = new List<Slot>(new Slot[] { Slot.empty() });


        private Integer m_None = default(Integer);

        private Func<Object, Integer> m_Times1Fnc = x => (Integer)x;

        private Func<Object, Integer> m_Times3Fnc = x => (Integer)x * 3;

        public List<Integer> list(int i)
        {
            return new List<Integer>(new Integer[] { i });
        }

        public List<Integer> list(int i, int j)
        {
            return new List<Integer>(new Integer[] { i, j });
        }

        [TestMethod]
        public void TestOneSequence()
        {
            List<Object> sequence0 = new List<Object>(new Integer[] { 7, 12, 12, 16 });

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            GroupBy2<Integer> m = GroupBy2<Integer>.of(list.ToArray());

            List<Pair<Object, List<Integer>>> expectedValues = new List<Pair<Object, List<Integer>>>(
                new Pair<Object, List<Integer>>[]
                {
                    new Pair<Object, List<Integer>>(7, new List<Integer>(new Integer[] {new Integer(7)})),
                    new Pair<Object, List<Integer>>(12, new List<Integer>(new Integer[] {new Integer(12), new Integer(12)})),
                    new Pair<Object, List<Integer>>(16, new List<Integer>(new Integer[] {new Integer(16)})),
                });

            int i = 0;
            foreach (Pair<Object, List<Integer>> t in m)
            {
                int j = 0;
                foreach (Integer o in t.Value)
                {
                    Assert.IsTrue(o == expectedValues[i].Value[j]);
                    j++;
                }
                i++;
            }
        }


        /// <summary>
        /// We take two lists: 
        /// L1 = {7, 12, 16}
        /// L2 = {3, 4, 5}
        /// Specified functions are x=>x for list L2 and x=>x*3 forlist 2.
        /// This means, we expect following key values for list1: 7, 12, 16 and
        /// for list two 9, 12, 15.
        /// Sorted keys: 7, 9, 12, 15, 16.
        /// Result: 7-[7], 9-[none, 3], 12-[12,4], 15-[none,5], 16-[16,none].
        /// </summary>
        [TestMethod]
        public void testTwoSequences()
        {
            List<Object> sequence0 = new List<Object>(new Object[] { new Integer(7), new Integer(12), new Integer(16) });
            List<Object> sequence1 = new List<Object>(new Object[] { new Integer(3), new Integer(4), new Integer(5)});

            List<Pair<List<Object>, Func<Object, Integer>>> list = new List<Pair<List<object>, Func<object, Integer>>>();
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence0, m_Times1Fnc));
            list.Add(new Pair<List<Object>, Func<Object, Integer>>(sequence1, m_Times3Fnc));
            GroupBy2<Integer> m = GroupBy2<Integer>.of(list.ToArray());

            List<Pair<Object, List<Object>>> expectedValues = new List<Pair<Object, List<Object>>>(
            new Pair<Object, List<Object>>[]
            {
                    new Pair<Object, List<Object>>(7, new List<Object>(new Object[] {new Integer(7), m_None })),
                    new Pair<Object, List<Object>>(9, new List<Object>(new Object[] {m_None, new Integer(3)})),
                    new Pair<Object, List<Object>>(12, new List<Object>(new Object[] {new Integer(12), new Integer(4)})),
                    new Pair<Object, List<Object>>(15, new List<Object>(new Object[] {m_None, new Integer(5)})),
                    new Pair<Object, List<Object>>(16, new List<Object>(new Object[] {16, m_None})),
            });
    
            int i = 0;
            foreach (var t in m)
            {
                int j = 0;
                foreach (var o in t.Value)
                {
                    if(o is Integer)
                        Assert.IsTrue(o == (Integer)expectedValues[i].Value[j]);
                    else
                        Assert.IsTrue(o == (Object)expectedValues[i].Value[j]);
                    j++;
                }
                i++;
            }
        }

        //    [TestMethod]
        //    public void testThreeSequences()
        //    {
        //        List<Integer> sequence0 = Arrays.asList(new Integer[] { 7, 12, 16 });
        //        List<Integer> sequence1 = Arrays.asList(new Integer[] { 3, 4, 5 });
        //        List<Integer> sequence2 = Arrays.asList(new Integer[] { 3, 3, 4, 5 });

        //        Function<Integer, Integer> identity = x->x;//Function.identity();
        //        Function<Integer, Integer> times3 = x->x * 3;
        //        Function<Integer, Integer> times4 = x->x * 4;

        //        @SuppressWarnings({ "unchecked", "rawtypes" })
        //    GroupBy2<Integer> m = GroupBy2.of(
        //        new Pair(sequence0, identity),
        //        new Pair(sequence1, times3),
        //        new Pair(sequence2, times4));

        //        List<Tuple> expectedValues = Arrays.asList(new Tuple[] {
        //        new Tuple(7, list(7), none, none),
        //        new Tuple(9, none, list(3), none),
        //        new Tuple(12, list(12), list(4), list(3, 3)),
        //        new Tuple(15, none, list(5), none),
        //        new Tuple(16, list(16), none, list(4)),
        //        new Tuple(20, none, none, list(5))
        //    });

        //        int i = 0;
        //        for (Tuple t : m)
        //        {
        //            int j = 0;
        //            for (Object o : t.all())
        //            {
        //                assertEquals(o, expectedValues.get(i).get(j));
        //                j++;
        //            }
        //            i++;
        //        }
        //    }

        //    @Test
        //public void testFourSequences()
        //    {
        //        List<Integer> sequence0 = Arrays.asList(new Integer[] { 7, 12, 16 });
        //        List<Integer> sequence1 = Arrays.asList(new Integer[] { 3, 4, 5 });
        //        List<Integer> sequence2 = Arrays.asList(new Integer[] { 3, 3, 4, 5 });
        //        List<Integer> sequence3 = Arrays.asList(new Integer[] { 3, 3, 4, 5 });

        //        Function<Integer, Integer> identity = Function.identity();
        //        Function<Integer, Integer> times3 = x->x * 3;
        //        Function<Integer, Integer> times4 = x->x * 4;
        //        Function<Integer, Integer> times5 = x->x * 5;

        //        @SuppressWarnings({ "unchecked", "rawtypes" })
        //    GroupBy2<Integer> m = GroupBy2.of(
        //        new Pair(sequence0, identity),
        //        new Pair(sequence1, times3),
        //        new Pair(sequence2, times4),
        //        new Pair(sequence3, times5));

        //        List<Tuple> expectedValues = Arrays.asList(new Tuple[] {
        //        new Tuple(7, list(7), none, none, none),
        //        new Tuple(9, none, list(3), none, none),
        //        new Tuple(12, list(12), list(4), list(3, 3), none),
        //        new Tuple(15, none, list(5), none, list(3, 3)),
        //        new Tuple(16, list(16), none, list(4), none),
        //        new Tuple(20, none, none, list(5), list(4)),
        //        new Tuple(25, none, none, none, list(5))
        //    });

        //        int i = 0;
        //        for (Tuple t : m)
        //        {
        //            int j = 0;
        //            for (Object o : t.all())
        //            {
        //                assertEquals(o, expectedValues.get(i).get(j));
        //                j++;
        //            }
        //            i++;
        //        }
        //    }

        //    @Test
        //public void testFiveSequences()
        //    {
        //        List<Integer> sequence0 = Arrays.asList(new Integer[] { 7, 12, 16 });
        //        List<Integer> sequence1 = Arrays.asList(new Integer[] { 3, 4, 5 });
        //        List<Integer> sequence2 = Arrays.asList(new Integer[] { 3, 3, 4, 5 });
        //        List<Integer> sequence3 = Arrays.asList(new Integer[] { 3, 3, 4, 5 });
        //        List<Integer> sequence4 = Arrays.asList(new Integer[] { 2, 2, 3 });

        //        Function<Integer, Integer> identity = Function.identity();
        //        Function<Integer, Integer> times3 = x->x * 3;
        //        Function<Integer, Integer> times4 = x->x * 4;
        //        Function<Integer, Integer> times5 = x->x * 5;
        //        Function<Integer, Integer> times6 = x->x * 6;

        //        @SuppressWarnings({ "unchecked", "rawtypes" })
        //    GroupBy2<Integer> m = GroupBy2.of(
        //        new Pair(sequence0, identity),
        //        new Pair(sequence1, times3),
        //        new Pair(sequence2, times4),
        //        new Pair(sequence3, times5),
        //        new Pair(sequence4, times6));

        //        List<Tuple> expectedValues = Arrays.asList(new Tuple[] {
        //        new Tuple(7, list(7), none, none, none, none),
        //        new Tuple(9, none, list(3), none, none, none),
        //        new Tuple(12, list(12), list(4), list(3, 3), none, list(2, 2)),
        //        new Tuple(15, none, list(5), none, list(3, 3), none),
        //        new Tuple(16, list(16), none, list(4), none, none),
        //        new Tuple(18, none, none, none, none, list(3)),
        //        new Tuple(20, none, none, list(5), list(4), none),
        //        new Tuple(25, none, none, none, list(5), none)
        //    });

        //        int i = 0;
        //        for (Tuple t : m)
        //        {
        //            int j = 0;
        //            for (Object o : t.all())
        //            {
        //                assertEquals(o, expectedValues.get(i).get(j));
        //                j++;
        //            }
        //            i++;
        //        }
        //    }
    }

}
