using NeoCortexApi.Network;
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Encoders
{
    /// <summary>
    /// Encodes input by using of multiple encoders on the same data stream.
    /// </summary>
    public class MultiEncoder : EncoderBase
    {
        /// <summary>
        /// List of encoders used by MultiEncoder.
        /// </summary>
        private List<EncoderBase> encoders = new List<EncoderBase>();

        public MultiEncoder(List<EncoderBase> encoders)
        {
            this.encoders = encoders;
        }


        /// <summary>
        /// Encodes data from all underlying encoders.
        /// </summary>
        /// <param name="inputData">Dictionary of inputs for all underlying encoders.</param>
        /// <returns></returns>
        public override int[] Encode(object inputData)
        {
            Dictionary<string, object> input = inputData as Dictionary<string, object>;

            List<int> output = new List<int>();

            foreach (var encoder in this.encoders)
            {              
                output.AddRange(encoder.Encode(input[encoder.Name]));

                output.AddRange(new int[encoder.Offset]);
            }

            return output.ToArray();
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
