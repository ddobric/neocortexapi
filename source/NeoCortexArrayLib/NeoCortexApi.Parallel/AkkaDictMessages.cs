// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
#pragma warning disable CS0234 // The type or namespace name 'Entities' does not exist in the namespace 'NeoCortexApi' (are you missing an assembly reference?)
using NeoCortexApi.Entities;
#pragma warning restore CS0234 // The type or namespace name 'Entities' does not exist in the namespace 'NeoCortexApi' (are you missing an assembly reference?)
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
#pragma warning disable CS0246 // The type or namespace name 'HtmConfig' could not be found (are you missing a using directive or an assembly reference?)
        public HtmConfig HtmAkkaConfig { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'HtmConfig' could not be found (are you missing a using directive or an assembly reference?)
    }

    internal class ContainsMsg
    {
        public object Key { get; set; }
    }

    internal class AddOrUpdateElementsMsg
    {
#pragma warning disable CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public IList<KeyPair> Elements { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
    }

    internal class AddElementsMsg
    {
#pragma warning disable CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public IList<KeyPair> Elements { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
    }
    internal class UpdateElementsMsg
    {
#pragma warning disable CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public IList<KeyPair> Elements { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
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
#pragma warning disable CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public IList<KeyPair> Elements { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
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
#pragma warning disable CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public List<KeyPair> ColumnKeys { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public double[] PermanenceChanges { get; set; }
    }

    public class BumUpWeakColumnsMsg
    {
#pragma warning disable CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)
        public List<KeyPair> ColumnKeys { get; set; }
#pragma warning restore CS0246 // The type or namespace name 'KeyPair' could not be found (are you missing a using directive or an assembly reference?)

    }
}