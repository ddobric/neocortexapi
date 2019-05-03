using System.Collections.Generic;

namespace NeoCortexApi.DistributedComputeLib
{
    internal class CreateDictNodeMsg
    {
    }

    internal class ContainsMsg
    {
        public object Key { get; set; }
    }

    internal class AddOrUpdateElementsMsg
    {
        public IList<KeyPair> Elements { get; set; }
    }

    internal class AddElementsMsg
    {
        public IList<KeyPair> Elements { get; set; }
    }
    internal class UpdateElementsMsg
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

    public class KeyPair
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