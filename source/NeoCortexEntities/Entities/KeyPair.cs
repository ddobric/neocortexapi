// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Entities
{

    public class KeyPair
    {
        public object Key { get; set; }

        public object Value { get; set; }

        public override string ToString()
        {
            return $"Key: {this.Key} - Val: {Value}";
        }
    }
}
