using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    /// <summary>
    /// Encodes input by using of multiple encoders.
    /// </summary>
    public class MultiEncoder : EncoderBase
    {
        /// <summary>
        /// List of encoders used by MultiEncoder.
        /// </summary>
        private List<EncoderBase> encoders = new List<EncoderBase>();

        public MultiEncoder(Dictionary<String, Dictionary<String, Object>> encoderSettings, CortexNetworkContext context = null)
        {
            if (context == null)
                throw new ArgumentException("Context must be specified.");

            foreach (var sett in encoderSettings)
            {
                if (sett.Value[EncoderProperties.EncoderQualifiedName] == null)
                    throw new ArgumentException("Context must be specified.");
                var encoder = context.CreateEncoder(EncoderProperties.EncoderQualifiedName, sett.Value);

                this.encoders.Add(encoder);
            }
        }

        public override void AfterInitialize()
        {

        }

        /// <summary>
        /// Encodes data from all underlying encoders.
        /// </summary>
        /// <param name="inputData">Dictionary of inputs for all underlying encoders.</param>
        /// <returns></returns>
        public override int[] Encode(object inputData)
        {
            Dictionary<string, object> input = inputData as Dictionary<string, object>;

            if (!(inputData is Dictionary<string, object>))
            {
                throw new ArgumentException($"{nameof(MultiEncoder)} must have a dictionary as input.");
            }

            List<int> output = new List<int>();

            foreach (var encoder in this.encoders)
            {
                if (input.ContainsKey(encoder.Name) == false)
                    throw new ArgumentException($"No settings specified for encoder '{encoder.Name}'.");

                int[] tempArray = new int[encoder.N];

                output.AddRange(encoder.Encode(input[encoder.Name]));
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
