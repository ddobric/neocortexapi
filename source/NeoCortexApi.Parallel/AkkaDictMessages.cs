// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System.Collections.Generic;

//TODO File name and classes do not match
namespace NeoCortexApi.DistributedComputeLib
{

    public class PingNodeMsg
    {
        public string Msg { get; set; }
    }

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
        public int PartitionKey { get; set; }

        public int MinKey { get; set; }
        public int MaxKey { get; set; }
        public IList<KeyPair> Elements { get; set; }
    }

    public class ConnectAndConfigureColumnsMsg
    {

    }

    public class CalculateOverlapMsg
    {
        public int[] InputVector { get; set; }
    }

    public class AdaptSynapsesMsg
    {
        public List<KeyPair> ColumnKeys { get; set; }
        public double[] PermanenceChanges { get; set; }
    }

    public class BumUpWeakColumnsMsg
    {
        public List<KeyPair> ColumnKeys { get; set; }

    }
}