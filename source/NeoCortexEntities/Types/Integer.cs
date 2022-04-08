// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;

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
            if (0 == Comparer<object>.Default.Compare(x, null) && 0 != Comparer<object>.Default.Compare(y, null))
                return false;

            if (0 != Comparer<object>.Default.Compare(x, null) && 0 == Comparer<object>.Default.Compare(y, null))
                return false;

            return x.Value == y.Value;
        }

        public static bool operator !=(Integer x, Integer y)
        {
            if (0 == Comparer<object>.Default.Compare(x, null) && 0 != Comparer<object>.Default.Compare(y, null))
                return true;

            if (0 != Comparer<object>.Default.Compare(x, null) && 0 == Comparer<object>.Default.Compare(y, null))
                return true;

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

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Integer), writer);
            
            ser.SerializeValue(this.Value,writer);

            ser.SerializeEnd(nameof(Integer), writer);
        }


        public static Integer Deserialize(StreamReader sr)
        {
            Integer inte = new Integer();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(Integer)))
                {
                    continue;
                }
                else if (data == ser.ReadEnd(nameof(Integer)))
                {
                    break;
                }
                else
                {
                    string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    inte.Value = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return inte;

        }

        #endregion

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
    }
}
