using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoCortexApi.Entities
{
    public interface ISerializable
    {
        void Serialize(object obj, string name, StreamWriter sw);
        static object Deserialize<T>(StreamReader sr, string name) => throw new NotImplementedException();
        double[] Encode(object inputData);
        //   List<NeoCortexApi.Encoders.EncoderResult> GetBucketInfo(int[] buckets);
        //   List<NeoCortexApi.Encoders.EncoderResult> TopDownCompute(int[] encoded);


    }
}
