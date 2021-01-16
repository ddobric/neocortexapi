// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
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

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Integer), writer);

            ser.SerializeValue(this.Value,writer);
            //ser.SerializeValue(Integer.MaxValue, writer);
            //ser.SerializeValue(Integer.MinValue, writer);

            ser.SerializeEnd(nameof(Integer), writer);
        }
        #endregion

        #region Deserialization
        public static Integer Deserialize(StreamReader sr)
        {
            Integer inte = new Integer();

            HtmSerializer2 ser = new HtmSerializer2();
            string data = sr.ReadToEnd();
            string[] str = data.Split('\n');

            foreach (string i in str)
            {
                if (i == "" || i == "  BEGIN 'Integer'  " || i == "  END 'Integer'  ")
                { continue; }
                else
                {
                    string[] istr = i.Split('|');
                    int j;
                    for (j = 0; j < istr.Length; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                {
                                    inte.Value = ser.ReadIntValue(istr[j]);
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
        }
    }
