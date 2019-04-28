using System;
using System.Collections.Generic;
using System.Text;

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
