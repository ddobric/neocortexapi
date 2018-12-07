using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public class EncoderTuple<T>
    {
        public string Name { get; set; }

        public EncoderBase<T> Encoder { get; set; }

        public int Offset { get; set; }
    }
}
