// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;

namespace NeoCortexApi.Utility
{
    public class ValueComparator<T> : System.Collections.IComparer where T : IComparable
    {
        public int Compare(object ob1, object ob2)
        {
            if (ob1 is T c1 && ob2 is T c2)
            {
                return c1.CompareTo(c2);
            }
            else
            {
                throw new InvalidCastException("ValueComparator: Illegal arguments!");
            }
        }
    }
}
