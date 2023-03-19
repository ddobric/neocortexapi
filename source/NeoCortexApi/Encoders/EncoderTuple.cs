// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace NeoCortexApi.Encoders
{
    public class EncoderTuple
    {
        public string Name { get; set; }

        public EncoderBase Encoder { get; set; }

        public int Offset { get; set; }
    }
}
