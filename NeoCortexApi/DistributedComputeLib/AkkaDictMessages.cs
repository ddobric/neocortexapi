using NeoCortexApi.Entities;
using System.Collections.Generic;

namespace NeoCortexApi.DistributedComputeLib
{
    /// <summary>
    /// HTM required configuration sent from Akka-client to Akka Actor.
    /// </summary>
    public class ActorConfig
    {
        public int[] ColumnDimensions { get; set; }

        public bool IsColumnMajor { get; set; } = false;

        public int[] InputDimensions { get; set; }

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }
    }

    internal class CreateDictNodeMsg
    {
        public ActorConfig HtmAkkaConfig { get; set; }
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
}