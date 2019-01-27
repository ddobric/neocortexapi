using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public class MultiEncoder<T> : EncoderBase<T>
    {
        public override int[] encodeIntoArray(T inputData)
        {
            throw new NotImplementedException();
        }

        public override List<B> getBucketValues<B>(B returnType)
        {
            throw new NotImplementedException();
        }

        public override int getWidth()
        {
            throw new NotImplementedException();
        }

        public override bool isDelta()
        {
            throw new NotImplementedException();
        }
    }
}
