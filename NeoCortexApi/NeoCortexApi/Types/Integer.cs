using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{

    //[Serializable]
    public class Integer : IEquatable<Integer>, IComparable<Integer>
    {
        public int Value { get; set; }
        public static int MaxValue { get { return int.MaxValue; } }

        public static int MinValue { get { return int.MinValue; } }

        public Integer() { }

        public Integer(int value) { Value = value; }


        // Custom cast from "int":
        public static implicit operator Integer(Int32 x) { return new Integer(x); }

        // Custom cast to "int":
        public static implicit operator Int32(Integer x) { return x.Value; }


        public override string ToString()
        {
            return string.Format("Integer({0})", Value);
        }

        public static bool operator ==(Integer x, Integer y)
        {
            return x.Value == y.Value;
        }

        public static bool operator !=(Integer x, Integer y)
        {
            return x.Value != y.Value;
        }

        public bool Equals(Integer other)
        {
            return this.Value == other.Value;
        }

        public int CompareTo(Integer other)
        {
            return Comparer<int>.Default.Compare(this.Value, other.Value);
        }
    }
}
