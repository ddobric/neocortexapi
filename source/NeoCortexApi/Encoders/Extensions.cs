// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;

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

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dest, IDictionary<TKey, TValue> src)
        {
            foreach (var item in src)
            {
                dest.Add(item.Key, item.Value);
            }
        }
    }
}
