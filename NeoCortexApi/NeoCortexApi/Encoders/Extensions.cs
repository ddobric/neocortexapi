using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public static class Extensions
    {
        public static List<double> Sublist(this List<double> list, int from, int to)
        {
            List<double> lst = new List<double>();

            if (from < list.Count && to < list.Count && from < to - 1)
            {
                for (int i = from; i < to; i++)
                {
                    lst.Add(list[i]);
                }

                return lst;
            }

            throw new ArgumentException("Invalid arguments from/to.");
        }
    }
}
