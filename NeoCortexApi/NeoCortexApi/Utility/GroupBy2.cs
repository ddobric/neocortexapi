using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using NeoCortexApi.Entities;

namespace NeoCortexApi.Utility
{



    /**
     * An Java extension to groupby in Python's itertools. Allows to walk across n sorted lists
     * with respect to their key functions and yields a {@link Tuple} of n lists of the
     * members of the next *smallest* group.
     * 
     * @author cogmission
     * @param <R>   The return type of the user-provided {@link Function}s
     */
    public class GroupBy2<R> : IEnumerable<Pair<Object, List<List<R>>>>, IEnumerator<Pair<Object, List<List<R>>>> where R : class//, Generator<Tuple> 
                                                                                                                                 //  where R : IComparable<R>
    {
        /** serial version */
        private static long serialVersionUID = 1L;

        /** stores the user inputted pairs */
        private Pair<List<Object>, Func<Object, R>>[] entries;

        /** stores the {@link GroupBy} {@link Generator}s created from the supplied lists */
        private List<GroupBy<Object, R>> generatorList;

        /** the current interation's minimum key value */
        //private R minKeyVal;


        ///////////////////////
        //    Control Lists  //
        ///////////////////////
        private bool[] advanceList;

        private Slot<Pair<object, R>>[] nextList;

        public Pair<object, List<List<R>>> Current { get; set; }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        // private int numEntries;

        /**
         * Private internally used constructor. To instantiate objects of this
         * class, please see the static factory method {@link #of(Pair...)}
         * 
         * @param entries   a {@link Pair} of lists and their key-producing functions
         */
        private GroupBy2(Pair<List<Object>, Func<Object, R>>[] entries)
        {
            this.entries = entries;
        }

        /**
         * <p>
         * Returns a {@code GroupBy2} instance which is used to group lists of objects
         * in ascending order using keys supplied by their associated {@link Function}s.
         * </p><p>
         * <b>Here is an example of the usage and output of this object: (Taken from {@link GroupBy2Test})</b>
         * </p>
         * <pre>
         *  List<Integer> sequence0 = Arrays.asList(new Integer[] { 7, 12, 16 });
         *  List<Integer> sequence1 = Arrays.asList(new Integer[] { 3, 4, 5 });
         *  
         *  Function<Integer, Integer> identity = Function.identity();
         *  Function<Integer, Integer> times3 = x -> x * 3;
         *  
         *  @SuppressWarnings({ "unchecked", "rawtypes" })
         *  GroupBy2<Integer> groupby2 = GroupBy2.of(
         *      new Pair(sequence0, identity), 
         *      new Pair(sequence1, times3));
         *  
         *  for(Tuple tuple : groupby2) {
         *      System.out.println(tuple);
         *  }
         * </pre>
         * <br>
         * <b>Will output the following {@link Tuple}s:</b>
         * <pre>
         *  '7':'[7]':'[NONE]'
         *  '9':'[NONE]':'[3]'
         *  '12':'[12]':'[4]'
         *  '15':'[NONE]':'[5]'
         *  '16':'[16]':'[NONE]'
         *  
         *  From row 1 of the output:
         *  Where '7' == Tuple.get(0), 'List[7]' == Tuple.get(1), 'List[NONE]' == Tuple.get(2) == empty list with no members
         * </pre>
         * 
         * <b>Note: Read up on groupby here:</b><br>
         *   https://docs.python.org/dev/library/itertools.html#itertools.groupby
         * <p> 
         * @param entries
         * @return  a n + 1 dimensional tuple, where the first element is the
         *          key of the group and the other n entries are lists of
         *          objects that are a member of the current group that is being
         *          iterated over in the nth list passed in. Note that this
         *          is a generator and a n+1 dimensional tuple is yielded for
         *          every group. If a list has no members in the current
         *          group, {@link Slot#NONE} is returned in place of a generator.
         */
        //@SuppressWarnings("unchecked")
        public static GroupBy2<R> of(Pair<List<Object>, Func<Object, R>>[] entries)
        {
            return new GroupBy2<R>(entries);
        }

        private List<R> m_Keys = new List<R>();

        private int m_CurrentKey = 0;


        /// <summary>
        /// Populates generator list with entries and fills the next(List with empty elements.
        /// </summary>
        private void reset()
        {
            generatorList = new List<GroupBy<Object, R>>();

            int i = 0;
            foreach (var item in entries)
            {
                generatorList.Add(GroupBy<Object, R>.From(item.Key, item.Value));
                generatorList[i].MoveNext();

                foreach (var item1 in item.Key)
                {
                    var key = item.Value(item1);
                    if (!m_Keys.Contains(key, EqualityComparer<R>.Default))
                        m_Keys.Add(key);
                }
                i++;
            }

            m_Keys = m_Keys.OrderBy(k => k).ToList();

            advanceList = new bool[entries.Length];

            ArrayUtils.Fill(advanceList, true);

            nextList = new Slot<Pair<Object, R>>[entries.Length];

            ArrayUtils.Fill(nextList, Slot<Pair<Object, R>>.NONE);
        }

        //private bool m_IsLastElementReached = false;

        public bool MoveNext()
        {
            //if (m_IsLastElementReached)
            //    return false;

            if (generatorList == null)
            {
                reset();
            }

            //for (int i = 0; i < entries.Length; i++)
            //{
            //    if (advanceList[i])
            //    {
            //        // Indicates that we are on the end of the pair list.
            //        // if (generatorList[i].NextPair == null)
            //        //  nextList[i] = Slot<Pair<object, R>>.of(null);
            //        // Add current pair.
            //        //else
            //        nextList[i] = Slot<Pair<object, R>>.of((Pair<object, R>)(object)generatorList[i].Current);
            //    }
            //}

            //R minKeyVal = getNextMinKey();
            if (m_CurrentKey >= m_Keys.Count)
                return false;

            R minKeyVal = m_Keys[m_CurrentKey++];
            //if (minKeyVal == null)
            //{
            //    m_IsLastElementReached = true;            
            //}

            this.Current = (Pair<Object, List<List<R>>>)next(minKeyVal);

            return true;
        }

        /**
         * Returns a {@link Tuple} containing the current key in the
         * zero'th slot, and a list objects which are members of the
         * group specified by that key.
         * 
         * {@inheritDoc}
         */
        //    @SuppressWarnings("unchecked")
        //@Override
        private Pair<object, List<List<R>>> next(R minKeyVal)
        {
            List<Pair<R, List<List<R>>>> objs = new List<Pair<R, List<List<R>>>>();

            // If minKeyVal is null, means we have reached end of the list and KEY of current any element  in gnerator list can be returned.
            Pair<object, List<List<R>>> retVal = new Pair<object, List<List<R>>>(minKeyVal != null ? minKeyVal : generatorList.First().Current.Key, new List<List<R>>());

            for (int i = 0; i < entries.Length; i++)
            {
                List<R> list = new List<R>();
                retVal.Value.Add(list);

                //if (minKeyVal != null)
                //{
                R key = getKeyFromList(i, minKeyVal);
                if (key != null)
                //if (isEligibleList(i, minKeyVal))
                {
                    //list.Add((R)nextList[i].get().Key);
                    list.Add(key);

                    drainKey(list, i, minKeyVal);
                }
                else
                {
                    list.Add(default(R));
                }

                //if (generatorList[i].NextPair == null)
                //    advanceList[i] = false;
                //else
                //    advanceList[i] = true;
                //}
                //else
                //{
                //    // Append last record.
                //    list.Add(generatorList[i].Current.Value);
                //}
            }

            return retVal;
        }



        /**
         * Returns the next smallest generated key.
         * 
         * @return  the next smallest generated key.
         */
        private R getNextMinKey()
        {
            // Get min key of all of none-null slots.
            return nextList.Where(slot => slot != null).Min(slot => slot.get() == null ? null : slot.get().Value);

            // return minKeyVal != null;
            //minKeyVal = nextList.Min(slot => slot.get() == null? null : slot.get().Value);

            //return minSlot.isPresent();
            //return !EqualityComparer<R>.Default.Equals(minKeyVal, default(R));
        }

        private R getKeyFromList(int listIdx, Object targetKey)
        {
            foreach (var elem in entries[listIdx].Key)
            {
                var key = entries[listIdx].Value(elem);
                if (EqualityComparer<R>.Default.Equals(key, (R)targetKey))
                    return (R)elem;
            }

            return null;
        }

        /**
         * Returns a flag indicating whether the list currently pointed
         * to by the specified index contains a key which matches the
         * specified "targetKey".
         * 
         * @param listIdx       the index pointing to the {@link GroupBy} being
         *                      processed.
         * @param targetKey     the specified key to match.
         * @return  true if so, false if not
         */
        private bool isEligibleList(int listIdx, Object targetKey)
        {
            //return nextList[listIdx].isPresent() && nextList[listIdx].get().Value.Equals((R)targetKey);

            return nextList[listIdx].isPresent() && EqualityComparer<R>.Default.Equals(nextList[listIdx].get().Value, (R)targetKey);
        }

        /**
         * Each input grouper may generate multiple members which match the
         * specified "targetVal". This method guarantees that all members 
         * are added to the list residing at the specified Tuple index.
         * 
         * @param retVal        the Tuple being added to
         * @param listIdx       the index specifying the list within the 
         *                      tuple which will have members added to it
         * @param targetVal     the value to match in order to be an added member
         */
        // @SuppressWarnings("unchecked")
        private void drainKey(List<R> list, int listIdx, R targetVal)
        {
            while (generatorList[listIdx].NextPair != null)
            //while (generatorList[listIdx].Current != null)
            {
                if (EqualityComparer<R>.Default.Equals(generatorList[listIdx].NextPair.Value, targetVal))
                {
                    var elm = generatorList[listIdx].Current;
                    //nextList[listIdx] = Slot<Pair<object, R>>.of((Pair<object, R>)(object)elm);
                    //list.Add((R)nextList[listIdx].get().Key);
                    list.Add((R)generatorList[listIdx].Current.Key);
                    generatorList[listIdx].MoveNext();
                }
                else
                {
                    //nextList[listIdx] = Slot<Pair<object, R>>.empty();
                    generatorList[listIdx].MoveNext();
                    break;
                }
            }
        }



        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<Pair<Object, List<R>>>)this;
        }

        IEnumerator<Pair<object, List<List<R>>>> IEnumerable<Pair<object, List<List<R>>>>.GetEnumerator()
        {
            return this;
        }

        public class Slot
        {

        }

        /**
         * A minimal {@link Serializable} version of an {@link Slot}
         * @param <T>   the value held within this {@code Slot}
         */
        public class Slot<T> : Slot, IEquatable<object>, IComparer<T>, IComparable<T>
            where T : class
        {
            /** Default Serial */
            private static readonly long serialVersionUID = 1L;

            /**
         * Common instance for {@code empty()}.
         */
            public static readonly Slot<T> NONE = new Slot<T>();

            /**
            * Returns an empty {@code Slot} instance.  No value is present for this
            * Slot.
            *
            * @param <T> Type of the non-existent value
            * @return an empty {@code Slot}
            */
            public static Slot<T> empty()
            {
                return NONE;
            }

            /**
             * If non-null, the value; if null, indicates no value is present
             */
            private readonly T value;

            internal Slot() { this.value = null; }

            /**
             * Constructs an instance with the value present.
             *
             * @param value the non-null value to be present
             * @throws NullPointerException if value is null
             */
            private Slot(T value)
            {
                //if (value == null)
                //    throw new ArgumentException(); //TODO.
                this.value = value;
            }

            /**
             * Returns an {@code Slot} with the specified present non-null value.
             *
             * @param <T> the class of the value
             * @param value the value to be present, which must be non-null
             * @return an {@code Slot} with the value present
             * @throws NullPointerException if value is null
             */
            public static Slot<T> of(T value)
            {
                return new Slot<T>(value);
            }

            /**
             * Returns an {@code Slot} describing the specified value, if non-null,
             * otherwise returns an empty {@code Slot}.
             *
             * @param <T> the class of the value
             * @param value the possibly-null value to describe
             * @return an {@code Slot} with a present value if the specified value
             * is non-null, otherwise an empty {@code Slot}
             */
            //@SuppressWarnings("unchecked")
            public static Slot<T> ofNullable(T value)
            {
                return value == null ? (Slot<T>)NONE : of(value);
            }

            /**
             * If a value is present in this {@code Slot}, returns the value,
             * otherwise throws {@code NoSuchElementException}.
             *
             * @return the non-null value held by this {@code Slot}
             * @throws NoSuchElementException if there is no value present
             *
             * @see Slot#isPresent()
             */
            public T get()
            {
                if (value == null)
                {
                    //  throw new ArgumentException("No value present");
                }
                return value;
            }



            /**
             * Return {@code true} if there is a value present, otherwise {@code false}.
             *
             * @return {@code true} if there is a value present, otherwise {@code false}
             */
            public bool isPresent()
            {
                return value != null;
            }




            /**
             * Indicates whether some other object is "equal to" this Slot. The
             * other object is considered equal if:
             * <ul>
             * <li>it is also an {@code Slot} and;
             * <li>both instances have no value present or;
             * <li>the present values are "equal to" each other via {@code equals()}.
             * </ul>
             *
             * @param obj an object to be tested for equality
             * @return {code true} if the other object is "equal to" this object
             * otherwise {@code false}
             */
            // @Override
            public bool Equals(Object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is Slot))
                {
                    return false;
                }

                Slot<Object> other = (Slot<Object>)obj;
                return value.Equals(other.value);
            }

            /**
             * Returns the hash code value of the present value, if any, or 0 (zero) if
             * no value is present.
             *
             * @return hash code value of the present value or 0 if no value is present
             */
            // @Override
            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            /**
             * Returns a non-empty string representation of this Slot suitable for
             * debugging. The exact presentation format is unspecified and may vary
             * between implementations and versions.
             *
             * @implSpec If a value is present the result must include its string
             * representation in the result. Empty and present Slots must be
             * unambiguously differentiable.
             *
             * @return the string representation of this instance
             */
            // @Override
            public override String ToString()
            {
                return value != null ? $"Slot[{value}s]" : "NONE";
            }


            /// <summary>
            /// Compares two slots.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(T x, T y)
            {
                return Comparer<T>.Default.Compare(x, y);
            }

            public int CompareTo(T other)
            {
                return Comparer<T>.Default.Compare(this.value, other);
            }
        }
    }

}
