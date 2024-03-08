// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi.Utility
{
    /// <summary>
    /// An Java extension to groupby in Python's itertools. Allows to walk across n sorted lists with respect to their key functions
    /// and yields a <see cref="Tuple"/> of n lists of the members of the next *smallest* group.
    /// </summary>
    /// <typeparam name="R">The return type of the user-provided <see cref="Func{T, TResult}"/>s</typeparam>
    /// <remarks>
    /// Author: cogmission, ddobric
    /// </remarks>
    public class GroupBy2<R> : IEnumerable<Pair<R, List<List<Object>>>>, IEnumerator<Pair<R, List<List<Object>>>> where R : class//, Generator<Tuple> 
                                                                                                                                 //  where R : IComparable<R>
    {
        /// <summary>
        /// stores the user inputted pairs
        /// </summary>
        private Pair<List<Object>, Func<Object, R>>[] m_Entries;

        /** stores the {@link GroupBy} {@link Generator}s created from the supplied lists */
        //private List<GroupBy<Object, R>> generatorList;

        /** the current interation's minimum key value */
        //private R minKeyVal;

        #region Control Lists
        ///////////////////////
        //    Control Lists  //
        ///////////////////////
        //private bool[] advanceList;

        #endregion

        // private Slot<Pair<object, R>>[] nextList;

        public Pair<R, List<List<Object>>> Current { get; set; }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        // private int numEntries;

        /// <summary>
        /// Private internally used constructor. To instantiate objects of this class, please see the static factory method <see cref="Of(Pair{List{object}, Func{object, R}}[])"/> 
        /// </summary>
        /// <param name="entries">a <see cref="Pair{TKey, TValue}"/> of lists and their key-producing functions</param>
        private GroupBy2(Pair<List<Object>, Func<Object, R>>[] entries)
        {
            this.m_Entries = entries;
        }


 
        /// <summary>
        /// TODO: change to C# code
        /// Returns a {@code GroupBy2} instance which is used to group lists of objects in ascending order using keys supplied by their associated {@link Function}s. <para/>
        /// <b>Here is an example of the usage and output of this object: (Taken from {@link GroupBy2Test})</b><br/>
        /// 
        ///  List&lt;Integer> sequence0 = Arrays.asList(new Integer[] { 7, 12, 16 });<br/>
        ///  List&lt;Integer> sequence1 = Arrays.asList(new Integer[] { 3, 4, 5 });<br/>
        ///  
        ///  Func&lt;Integer, Integer> identity = Function.identity();<br/>
        ///  Func&lt;Integer, Integer> times3 = x => x * 3;<br/>
        ///  
        ///  GroupBy2&lt;Integer> groupby2 = GroupBy2.Of(<br/>
        ///      new Pair(sequence0, identity), <br/>
        ///      new Pair(sequence1, times3));<br/>
        ///  
        ///  foreach (Tuple tuple in groupby2) {<br/>
        ///      Console.WriteLine(tuple);<br/>
        ///  }
        /// <br/>
        /// <b>Will output the following {@link Tuple}s:</b><br/>
        ///  '7':'[7]':'[NONE]'<br/>
        ///  '9':'[NONE]':'[3]'<br/>
        ///  '12':'[12]':'[4]'<br/>
        ///  '15':'[NONE]':'[5]'<br/>
        ///  '16':'[16]':'[NONE]'<br/>
        ///  
        ///  From row 1 of the output:<br/>
        ///  Where '7' == Tuple.get(0), 'List[7]' == Tuple.get(1), 'List[NONE]' == Tuple.get(2) == empty list with no members<br/>
        /// 
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <returns>
        /// a n + 1 dimensional tuple, where the first element is the key of the group and the other n entries are lists of
        /// objects that are a member of the current group that is being iterated over in the nth list passed in. Note that this
        /// is a generator and a n+1 dimensional tuple is yielded for every group.If a list has no members in the current 
        /// group, <see cref="Slot"/> is returned in place of a generator.
        /// </returns>
        /// <remarks>
        /// Note: Read up on groupby <a href="https://docs.python.org/dev/library/itertools.html#itertools.groupby">here</a>
        /// </remarks>
        public static GroupBy2<R> Of(Pair<List<Object>, Func<Object, R>>[] entries)
        {
            return new GroupBy2<R>(entries);
        }

        private List<R> m_Keys;

        private int m_CurrentKey = 0;

        // TODO same method name
        /// <summary>
        /// Populates generator list with entries and fills the next(List with empty elements.
        /// </summary>
        private void reset()
        {
            m_Keys = new List<R>();

            int i = 0;

            foreach (var item in m_Entries)
            {
                foreach (var item1 in item.Key)
                {
                    var key = item.Value(item1);
                    if (!m_Keys.Contains(key, EqualityComparer<R>.Default))
                        m_Keys.Add(key);
                }
                i++;
            }

            m_Keys = m_Keys.ConvertAll<R>(item => (R)item).OrderBy(k => (R)k).ToList();

        }

        public bool MoveNext()
        {
            if (m_Keys == null)
            {
                reset();
            }
            if (m_CurrentKey >= m_Keys.Count)
                return false;

            R minKeyVal = m_Keys[m_CurrentKey++];

            this.Current = (Pair<R, List<List<Object>>>)Next(minKeyVal);

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
        /// <summary>
        /// Returns a <see cref="Pair{TKey, TValue}"/> containing the current key in the zero'th slot, and a list objects which are members of the
        /// group specified by that key.
        /// </summary>
        /// <param name="minKeyVal"></param>
        /// <returns></returns>
        private Pair<R, List<List<Object>>> Next(R minKeyVal)
        {
            Pair<R, List<List<Object>>> retVal = new Pair<R, List<List<Object>>>(minKeyVal, new List<List<Object>>());

            for (int i = 0; i < m_Entries.Length; i++)
            {
                List<Object> list;
                list = getEntries(i, minKeyVal);

                retVal.Value.Add(list);

            }

            return retVal;
        }


        private R GetKeyFromList(int listIdx, Object targetKey)
        {
            foreach (var elem in m_Entries[listIdx].Key)
            {
                var key = m_Entries[listIdx].Value(elem);
                if (EqualityComparer<R>.Default.Equals(key, (R)targetKey))
                    return (R)elem;
            }

            return null;
        }


        private List<Object> getEntries(int listIdx, Object targetKey)
        {
            List<Object> list = new List<Object>();

            foreach (var elem in m_Entries[listIdx].Key)
            {
                var key = m_Entries[listIdx].Value(elem);
                if (EqualityComparer<R>.Default.Equals(key, (R)targetKey))
                    list.Add((Object)elem);
            }

            return list;
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
            return (IEnumerator<Pair<R, List<Object>>>)this;
        }

        IEnumerator<Pair<R, List<List<Object>>>> IEnumerable<Pair<R, List<List<Object>>>>.GetEnumerator()
        {
            return this;
        }

        public class Slot
        {

        }

        /// <summary>
        /// A minimal {@link Serializable} version of an <see cref="Slot"/>
        /// </summary>
        /// <typeparam name="T">the value held within this <see cref="Slot"/></typeparam>
        public class Slot<T> : Slot, IEquatable<object>, IComparer<T>, IComparable<T>
            where T : class
        {


            /// <summary>
            ///  Common instance for <see cref="Empty"/>.
            /// </summary>
            public static readonly Slot<T> NONE = new Slot<T>();

            /// <summary>
            /// Returns an empty <see cref="Slot{T}"/> instance.  No value is present for this Slot.
            /// </summary>
            /// <returns>an empty <see cref="Slot{T}"/></returns>
            public static Slot<T> Empty()
            {
                return NONE;
            }

            /// <summary>
            /// If non-null, the value; if null, indicates no value is present
            /// </summary>
            private readonly T value;

            internal Slot() { this.value = null; }

            /// <summary>
            /// Constructs an instance with the value present.
            /// </summary>
            /// <param name="value">the non-null value to be present</param>
            /// <exception cref="NullReferenceException">Throws if value is null.</exception>
            private Slot(T value)
            {
                //if (value == null)
                //    throw new ArgumentException(); //TODO.
                this.value = value;
            }

            /// <summary>
            /// Retruns an <see cref="Slot{T}"/> with the specified present non-null value.
            /// </summary>
            /// <param name="value">the value to be present, which must be non-null</param>
            /// <returns>a <see cref="Slot{T}"/> with the value present.</returns>
            /// <exception cref="NullReferenceException">Throws if value is null.</exception>
            public static Slot<T> Of(T value)
            {
                return new Slot<T>(value);
            }

            /// <summary>
            /// Returns an <see cref="Slot{T}"/> describing the specified value, if non-null, otherwise returns an empty <see cref="Slot{T}"/>.
            /// </summary>
            /// <param name="value">the possibly-null value to describe</param>
            /// <returns>a <see cref="Slot{T}"/> with a present value if the specified value</returns>
            public static Slot<T> OfNullable(T value)
            {
                return value == null ? (Slot<T>)NONE : Of(value);
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
            /// <summary>
            /// If a value is present in this <see cref="Slot{T}"/>, returns the value, otherwise throws <see cref="ArgumentException"/>.
            /// </summary>
            /// <returns>the non-null value held by this <see cref="Slot{T}"/></returns>
            /// <exception cref="ArgumentException">Throws if there is no value present.</exception>
            public T Get()
            {
                if (value == null)
                {
                    //  throw new ArgumentException("No value present");
                }
                return value;
            }

            /// <summary>
            /// Return <see cref="true"/> if there is a value present, otherwise <see cref="false"/>
            /// </summary>
            /// <returns></returns>
            public bool IsPresent()
            {
                return value != null;
            }

            /**
             * Indicates whether some other object is "equal to" this Slot. The
             * other object is considered equal if:
             * <ul>
             * <li>
             * <li>both instances have no value present or;
             * <li>the present values are "equal to" each other via {@code equals()}.
             * </ul>
             *
             * @param obj an object to be tested for equality
             * @return {code true} if the other object is "equal to" this object
             * otherwise {@code false}
             */
            // @Override

            // TODO override meothod??
            // add override tag cause unitTest CompareDentrites() failed
            /// <summary>
            /// Indicates whether some other object is "equal to" this Slot. The other object is considered equal if:
            /// <list type="bullet">
            /// <item>it is also an <see cref="Slot"/> and;</item>
            /// <item>both instances have no value present or;</item>
            /// <item>the present values are "equal to" each other via <se {@code equals()}.</item>
            /// </list>
            /// </summary>
            /// <param name="obj">an object to be tested for equality</param>
            /// <returns>true if the other object is "equal to" this object otherwise false</returns>
#pragma warning disable CS0114 // 'GroupBy2<R>.Slot<T>.Equals(object)' hides inherited member 'object.Equals(object)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword.
            public bool Equals(object obj)
#pragma warning restore CS0114 // 'GroupBy2<R>.Slot<T>.Equals(object)' hides inherited member 'object.Equals(object)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword.
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is Slot))
                {
                    return false;
                }

                Slot<object> other = (Slot<object>)obj;
                return value.Equals(other.value);
            }

            /// <summary>
            /// Returns the hash code value of the present value, if any, or 0 (zero) if
            /// no value is present.
            /// </summary>
            /// <returns>hash code value of the present value or 0 if no value is present</returns>
            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            /// <summary>
            /// Returns a non-empty string representation of this Slot suitable for debugging. The exact presentation format is unspecified and may vary
            /// between implementations and versions.
            /// 
            /// @implSpec If a value is present the result must include its string representation in the result. Empty and present Slots must be unambiguously differentiable.
            /// </summary>
            /// <returns>the string representation of this instance</returns>
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
