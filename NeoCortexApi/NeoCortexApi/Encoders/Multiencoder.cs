using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public class MultiEncoder : EncoderBase
    {     
        
        public override void Initialize(Dictionary<string, object> encoderSettings)
        {
            throw new NotImplementedException();
        }

        public override int[] Encode(object inputData)
        {
            throw new NotImplementedException();
        }

        public override List<T> getBucketValues<T>()
        {
            throw new NotImplementedException();
        }

        public override int Width
        {
            get
            {
                return this.Width;
            }
        }


        public override bool IsDelta
        {
            get
            {
                return false;
            }
           
        }
    }
}
