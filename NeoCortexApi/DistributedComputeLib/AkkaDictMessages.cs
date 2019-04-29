using System.Collections.Generic;

namespace NeoCortexApi.DistributedComputeLib
{
    internal class CreateDictNodeMsg
    {
    }

    internal class AddElementMsg
    {
        public IList<KeyPair> Elements { get; set; }
    }

    internal class GetElementMsg
    {
        public object Key { get; set; }
    }

    internal class ContainsKeyMsg
    {
        public object Key { get; set; }
    }

    internal class GetCountMsg
    {
      
    }

    internal class KeyPair
    {
        public object Key { get; set; }

        public object Value { get; set; }
    }

    internal class Result
    {
        public bool IsError { get; set; }

        public object Value { get; set; }
    }
}