// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Entities
{
    public class Pair<TKey, TValue> : IEquatable<Pair<TKey, TValue>>
    {
        /// <summary>
        /// Creates an empty key-pair.
        /// </summary>

        public Pair()
        {

        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.KeyValuePair`2 structure
        //     with the specified key and value.
        //
        // Parameters:
        //   key:
        //     The object defined in each key/value pair.
        //
        //   value:
        //     The definition associated with key.
        public Pair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        //
        // Summary:
        //     Gets the key in the key/value pair.
        //
        // Returns:
        //     A TKey that is the key of the System.Collections.Generic.KeyValuePair`2.
        public TKey Key { get; }
        //
        // Summary:
        //     Gets the value in the key/value pair.
        //
        // Returns:
        //     A TValue that is the value of the System.Collections.Generic.KeyValuePair`2.
        public TValue Value { get; }


        public static bool operator ==(Pair<TKey, TValue> x, Pair<TKey, TValue> y)
        {
            if (EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, x)
                && EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, y))
                return true;

            else if (!EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, x)
               && EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, y))
                return false;
            else if (EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, x)
               && !EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, y))
                return false;

            return EqualityComparer<TKey>.Default.Equals(x.Key, y.Key) && EqualityComparer<TValue>.Default.Equals(x.Value, y.Value);
        }

        public static bool operator !=(Pair<TKey, TValue> x, Pair<TKey, TValue> y)
        {
            if (EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, x)
                && EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, y))
                return false;
            else if (EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, x)
              && !EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, y))
                return true;
            else if (!EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, x)
              && EqualityComparer<Pair<TKey, TValue>>.Default.Equals(null, y))
                return true;

            return !EqualityComparer<TKey>.Default.Equals(x.Key, y.Key) ||
                !EqualityComparer<TValue>.Default.Equals(x.Value, y.Value);
        }

        //
        // Summary:
        //     Returns a string representation of the System.Collections.Generic.KeyValuePair`2,
        //     using the string representations of the key and value.
        //
        // Returns:
        //     A string representation of the System.Collections.Generic.KeyValuePair`2, which
        //     includes the string representations of the key and value.
        public override string ToString()
        {
            return $"[{this.Key}, {this.Value}]";
        }

        public bool Equals(Pair<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(this.Key, other.Key) && EqualityComparer<TValue>.Default.Equals(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Pair<TKey, TValue> pair)
            {
                return this == pair;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
