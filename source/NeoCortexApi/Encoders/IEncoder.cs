// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System.Collections.Generic;

namespace NeoCortexApi.Encoders
{
    internal interface IEncoder
    {
        object Name { get; }
        object Encoders { get; set; }

        object GetBucketIndices(object value);
        List<EncoderBase.EncoderResult> GetBucketInfo(List<int> bucketIndices);
    }
}