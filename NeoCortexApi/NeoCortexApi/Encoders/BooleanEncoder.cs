// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
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

        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
