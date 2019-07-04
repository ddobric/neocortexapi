using System;
using System.Collections.Generic;
using System.Text;

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
