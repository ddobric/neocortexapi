using NeoCortexApi.Entities;
using System.Collections.Generic;

namespace NeoCortexApi.DistributedComputeLib
{


    internal class CreateDictNodeMsg
    {
        public HtmConfig HtmAkkaConfig { get; set; }
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

    internal class GetElementsMsg
    {
        public object[] Keys { get; set; }
    }



    internal class ContainsKeyMsg
    {
        public object Key { get; set; }
    }

    internal class GetCountMsg
    {

    }


    internal class Result
    {
        public bool IsError { get; set; }

        public object Value { get; set; }
    }



    public class InitColumnsMsg
    {
        public IList<KeyPair> Elements { get; set; }
    }

    public class ConnectAndConfigureColumnsMsg
    {
        
    }

    public class CalculateOverlapMsg
    {
        public int[] InputVector { get; set; }
    }
}