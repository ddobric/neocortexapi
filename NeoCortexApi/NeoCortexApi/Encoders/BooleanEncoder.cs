using System;
using System.Collections.Generic;

namespace NeoCortexApi.Encoders
{
    public class BooleanEncoder : EncoderBase
    {
        public override int Width { get; }

        public override bool IsDelta { get { return false; } }

        public override int[] Encode(object inputData)
        {

            throw new NotImplementedException();
        }

        public override List<T> getBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
