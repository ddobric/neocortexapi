// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;

namespace NeoCortexApi.DistributedComputeLib
{
    public class DistributedException : Exception
    {
        public DistributedException()
        {
        }

        public DistributedException(string message) : base(message)
        {
        }

        public DistributedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
