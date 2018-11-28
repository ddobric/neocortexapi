using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public class EncoderTuple
    {
        public string Name { get; set; }

        public EncoderBase Encoder { get; set; }

        public int Offset { get; set; }
    }
}
