using NeoCortexApi.Encoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class DataStream : IEnumerator<int[]>
    {
        private MultiEncoder encoder;

        public DataStream(MultiEncoder encoder)
        {
            this.encoder = encoder;
        }

        public int[] Current => throw new NotImplementedException();

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            bool res = this.sensor.MoveNext();

            if (res)
            {
                this.currentOutput = this.encoder.Encode(this.sensor.Current);
            }

            return res;
        }

        public void Reset()
        {
            
        }
    }
}
