// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace NeoCortexApi.Encoders
{
    internal interface IEncoder
    {
        object Name { get; }

        object GetBucketIndices(object value);
    }
}