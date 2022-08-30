// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace NeoCortexApi.Entities
{

    public class KeyPair
    {
        public object Key { get; set; }

        public object Value { get; set; }

        public override bool Equals(object obj)
        {       
            if (obj == null)
                return false;

            if (!(obj is KeyPair))
                return false;
            else
                return Equals(obj);                   
        }

        public bool Equals(KeyPair kp)
        {
            if (ReferenceEquals(this, kp))
                return true;

            return EqualityComparer<object>.Default.Equals(Key, kp.Key) &&
                   EqualityComparer<object>.Default.Equals(Value, kp.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }

        public override string ToString()
        {
            return $"Key: {this.Key} - Val: {Value}";
        }



    }
}
