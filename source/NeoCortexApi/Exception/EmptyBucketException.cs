using System;


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