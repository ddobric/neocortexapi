using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Utility
{
    public class ValueComparator<T> : System.Collections.IComparer where T : IComparable
    {
        public int Compare(object ob1, object ob2)
        {
            if (ob1 is T && ob2 is T)
            {
                T c1 = (T)ob1;
                T c2 = (T)ob2;

                return c1.CompareTo(c2);
            }
            else
            {
                throw new InvalidCastException("ValueComparator: Illegal arguments!");
            }
        }
    }
}
