using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

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
        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                sb.AppendLine();
                sb.Append("<?");

                sb.Append(property.Name);
                sb.Append(": ");
                if (property.GetIndexParameters().Length > 0)
                {
                    sb.Append("Indexed Property cannot be used");
                }
                else
                {
                    sb.Append(property.GetValue(this, null));
                }


                sb.Append(Environment.NewLine);
                sb.Append("<?");
            }

            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                sb.AppendLine();
                sb.Append("<?");

                sb.Append(field.Name);
                sb.Append(": ");

                sb.Append(field.GetValue(this));

                sb.Append(Environment.NewLine);
                sb.Append("<?");

            }
            sb.AppendLine("}");

            return sb.ToString();


        }
    }
}
