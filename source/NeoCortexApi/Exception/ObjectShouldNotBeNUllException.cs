using System;

namespace NeoCortexApi.Exception
{
    public class ObjectShouldNotBeNUllException : System.Exception
    {
        public ObjectShouldNotBeNUllException(String message)
            : base(message)
        {

        }
    }
}