using System;
using Akka.Pattern;

namespace NeoCortexApi.Exception
{
    public class EmptyBucketException : System.Exception
    {
        public EmptyBucketException(String message)
          : base(message)
        {
            
        }
    }
}