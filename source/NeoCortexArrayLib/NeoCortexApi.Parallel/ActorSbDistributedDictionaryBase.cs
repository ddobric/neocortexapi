// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeoCortexApi.DistributedComputeLib
{
    public class ActorSbDistributedDictionaryBase<TValue> : IDistributedDictionary<int, TValue>, IHtmDistCalculus
    {
        private List<Placement<int>> m_ActorMap;

        private AkkaSb.Net.ActorSystem m_ActorSystem;

        /// <summary>
        /// Maps from Actor partition identifier to actor index in <see cref="dictActors" list./>
        /// </summary>
        protected List<Placement<int>> ActorMap
        {
            get
            {
                if (m_ActorMap == null)
                    InitPartitionActorsDist();

                return this.m_ActorMap;
            }
            set
            {
                this.m_ActorMap = value;
            }
        }

        //private int m_NumElements = 0;


        #region Properties
        /// <summary>
        /// Actor cluster configuration.
        /// </summary>
        public ActorSbConfig Config { get; set; }

        /// <summary>
        /// Configuration used to initialize HTM calculus in actor.
        /// </summary>
        public HtmConfig HtmConfig { get; set; }
        #endregion

        /// <summary>
        /// Creates all required actors, which host partitions.
        /// </summary>
        /// <param name="config"></param>
        public ActorSbDistributedDictionaryBase(ActorSbConfig config, ILogger logger)
        {
            if (config == null)
                throw new ArgumentException("Configuration must be specified.");

            this.Config = config;

            this.m_ActorSystem = new ActorSystem("HtmCalculusDriver", config, logger);
        }

        /// <summary>
        /// Creates partition actors on cluster.
        /// </summary>
        protected void InitPartitionActorsDist()
        {
            this.ActorMap = CreatePartitionMap();

            for (int i = 0; i < this.ActorMap.Count; i++)
            {
                //string actorName = $"{nameof(DictNodeActor)}-{Guid.NewGuid()}-{this.ActorMap[i].NodeIndx}-{this.ActorMap[i].PartitionIndx}";

                ActorReference actorRef1 = m_ActorSystem.CreateActor<HtmActor>(1);

                try
                {
                    this.ActorMap[i].ActorRef = m_ActorSystem.CreateActor<HtmActor>(new ActorId(this.ActorMap[i].PartitionIndx));

                    var result = ((ActorReference)this.ActorMap[i].ActorRef).Ask<int>(new CreateDictNodeMsg()
                    {
                        HtmAkkaConfig = this.HtmConfig,
                    }, this.Config.ConnectionTimeout, this.m_ActorMap[i].NodePath).Result;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<Placement<int>> CreatePartitionMap()
        {
            return this.ActorMap = CreatePartitionMap(this.HtmConfig.NumColumns, this.Config.NumOfElementsPerPartition, this.Config.NumOfPartitions, this.Config.Nodes);
        }

        /// <summary>
        /// Calculates partition placements on nodes.
        /// </summary>
        /// <returns></returns>
        public static List<Placement<int>> CreatePartitionMap(int numElements, int numOfElementsPerPartition, int numOfPartitions = -1, List<string> nodes = null)
        {
            List<Placement<int>> map = new List<Placement<int>>();

            if (numOfPartitions == -1)
                numOfPartitions = (numOfElementsPerPartition == -1 && nodes != null && nodes.Count > 0) ? nodes.Count : (int)(1 + ((double)numElements / (double)numOfElementsPerPartition));

            if (numOfElementsPerPartition == -1)
            {
                var ratio = (float)numElements / (float)numOfPartitions;

                if (ratio > 0)
                    numOfElementsPerPartition = (int)(1.0 + ratio);
                else
                    numOfElementsPerPartition = (int)ratio;
            }

            int destNodeIndx = 0;
            string destinationNode = null;

            for (int partIndx = 0; partIndx < numOfPartitions; partIndx++)
            {
                var min = numOfElementsPerPartition * partIndx;
                var maxPartEl = numOfElementsPerPartition * (partIndx + 1) - 1;
                var max = maxPartEl < numElements ? maxPartEl : numElements - 1;

                if (min >= numElements)
                    break;

                if (nodes != null && nodes.Count > 0)
                {
                    destNodeIndx %= nodes.Count;
                    destinationNode = nodes[destNodeIndx++];
                }

                map.Add(new Placement<int>() { NodeIndx = -1, NodePath = destinationNode, PartitionIndx = partIndx, MinKey = min, MaxKey = max, ActorRef = null });
            }

            return map;
        }

        /// <summary>
        /// Depending on usage (Key type) different mechanism can be used to partition keys.
        /// This method returns the index of the node, whish should hold specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected ActorReference GetPartitionActorFromKey(int key)
        {
            return this.ActorMap.Where(p => p.MinKey <= key && p.MaxKey >= key).First().ActorRef as ActorReference;
        }

        /// <summary>
        /// Gets partitions (nodes) with assotiated indexes.
        /// </summary>
        /// <returns></returns>
        //public abstract List<(int partId, int minKey, int maxKey)> GetPartitions();

        public TValue this[int key]
        {
            get
            {
                var partActor = GetPartitionActorFromKey(key);

                TValue val = partActor.Ask<TValue>("").Result;

                return val;
            }
            set
            {
                var partActor = GetPartitionActorFromKey(key);

                var isSet = partActor.Ask<int>(new UpdateElementsMsg()
                {
                    Elements = new List<KeyPair>
                    {
                        new KeyPair { Key=key, Value=value }
                    }

                }, this.Config.ConnectionTimeout).Result;

                if (isSet != 1)
                    throw new ArgumentException("Cannot find the element with specified key!");
            }
        }


        public int Count
        {
            get
            {
                int cnt = 0;

                Task<int>[] tasks = new Task<int>[this.ActorMap.Count];
                int indx = 0;

                foreach (var actor in this.ActorMap.Select(el => el.ActorRef))
                {
                    tasks[indx++] = ((ActorReference)actor).Ask<int>(new GetCountMsg(), this.Config.ConnectionTimeout);
                }

                Task.WaitAll(tasks);

                for (int i = 0; i < tasks.Length; i++)
                {
                    if (!tasks[i].IsFaulted)
                        cnt += tasks[i].Result;
                    else
                        throw new DistributedException("An error has ocurred.", tasks[i].Exception);
                }

                return cnt;
            }
        }

        public ICollection<int> Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public ICollection<TValue> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }



        public bool IsReadOnly => false;

        /// <summary>
        /// Adds/Updates batch of elements to remote nodes.
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public void AddOrUpdate(ICollection<KeyPair> keyValuePairs)
        {
            throw new NotImplementedException();

            //ParallelOptions opts = new ParallelOptions();
            //opts.MaxDegreeOfParallelism = this.ActorMap.Count;

            //// We get here keys grouped to actors, which host partitions.
            //var partitions = GetPartitionsForKeyset(keyValuePairs);

            ////
            //// Here is upload performed in context of every actor (partition).
            //// Because keys are grouped by partitions (actors) parallel upload can be done here.
            //Parallel.ForEach(partitions, opts, (partition) =>
            //{
            //    Dictionary<ActorReference, AddOrUpdateElementsMsg> list = new Dictionary<ActorReference, AddOrUpdateElementsMsg>();

            //    int pageSize = 100;
            //    int alreadyProcessed = 0;

            //    while (true)
            //    {
            //        foreach (var item in partition.Value.Skip(alreadyProcessed).Take(pageSize))
            //        {
            //            var actorRef = GetPartitionActorFromKey((TKey)item.Key);
            //            if (!list.ContainsKey(actorRef))
            //                list.Add(actorRef, new AddOrUpdateElementsMsg() { Elements = new List<KeyPair>() });

            //            list[actorRef].Elements.Add(new KeyPair { Key = item.Key, Value = item.Value });
            //        }

            //        if (list.Count > 0)
            //        {
            //            List<Task<int>> tasks = new List<Task<int>>();

            //            foreach (var item in list)
            //            {
            //                tasks.Add(item.Key.Ask<int>(item.Value, this.Config.ConnectionTimeout));
            //                alreadyProcessed += item.Value.Elements.Count;
            //            }

            //            Task.WaitAll(tasks.ToArray());

            //            list.Clear();
            //        }
            //        else
            //            break;
            //    }
            //});
        }


        /// <summary>
        /// Adds/Updates batch of elements to remote nodes.
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public void InitializeColumnPartitionsDist(ICollection<KeyPair> keyValuePairs)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            RunBatched((batchOfElements) =>
            {
                // Run overlap calculation on all actors(in all partitions)
                Parallel.ForEach(batchOfElements, opts, (placement) =>
                {
                    try
                    {
                        var res = ((ActorReference)placement.ActorRef).Ask<int>(new InitColumnsMsg()
                        {
                            PartitionKey = placement.PartitionIndx,
                            MinKey = (int)(object)placement.MinKey,
                            MaxKey = (int)(object)placement.MaxKey
                        }, this.Config.ConnectionTimeout, placement.NodePath).Result;
                    }
                    catch (Exception)
                    {

                    }
                });
            }, this.ActorMap, this.Config.BatchSize);

        }

        private void RunBatched(Action<List<Placement<int>>> p, List<Placement<int>> actorMap, object batchSize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="func"></param>
        /// <param name="list"></param>
        /// <param name="batchSize"></param>
        private void RunBatched(Action<List<Placement<int>>> func, List<Placement<int>> list, int batchSize)
        {
            int processedItems = 0;
            int batchCnt = 0;

            var shuffledList = ArrayUtils.Shuffle(list);

            List<Placement<int>> batchedList = new List<Placement<int>>();

            while (processedItems <= this.ActorMap.Count)
            {
                if (batchCnt < batchSize && processedItems < shuffledList.Count)
                {
                    batchedList.Add(shuffledList[processedItems]);
                    processedItems++;
                    batchCnt++;
                }
                else
                {
                    if (batchedList.Count > 0)
                    {
                        func(batchedList);

                        batchCnt = 0;
                        batchedList = new List<Placement<int>>();
                    }
                    else
                        break;
                }
            }
        }

        /// <summary>
        /// Performs remote initialization and configuration of all coulmns in all partitions.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <returns>List of average spans of all columns on this node in this partition.
        /// This list is agreggated by caller to estimate average span for all system.</returns>
        public List<double> ConnectAndConfigureInputsDist(HtmConfig htmConfig2)
        {
            // List of results.
            ConcurrentDictionary<int, double> aggLst = new ConcurrentDictionary<int, double>();

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            RunBatched((batchOfElements) =>
            {
                Parallel.ForEach(batchOfElements, opts, (placement) =>
                {
                    while (true)
                    {
                        try
                        {
                            //Debug.WriteLine($"C: {placement.ActorRef.Path}");
                            var avgSpanOfPart = ((ActorReference)placement.ActorRef).Ask<double>(new ConnectAndConfigureColumnsMsg(), this.Config.ConnectionTimeout, placement.NodePath).Result;
                            aggLst.TryAdd(placement.PartitionIndx, avgSpanOfPart);
                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                });
            }, this.ActorMap, this.Config.BatchSize / 2);

            return aggLst.Values.ToList();
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        public int[] CalculateOverlapDist(int[] inputVector)
        {
            ConcurrentDictionary<int, List<KeyPair>> overlapList = new ConcurrentDictionary<int, List<KeyPair>>();

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            // Run overlap calculation on all actors(in all partitions)
            Parallel.ForEach(this.ActorMap, opts, (placement) =>
            {
                var partitionOverlaps = ((ActorReference)placement.ActorRef).Ask<List<KeyPair>>(new CalculateOverlapMsg { InputVector = inputVector }, this.Config.ConnectionTimeout, placement.NodePath).Result;
                overlapList.TryAdd(placement.PartitionIndx, partitionOverlaps);
            });

            List<int> overlaps = new List<int>();

            int cnt = 0;

            foreach (var item in overlapList.OrderBy(i => i.Key))
            {
                foreach (var keyPair in item.Value)
                {
                    cnt++;
                    overlaps.Add((int)(long)keyPair.Value);
                }

                Debug.WriteLine($"cnt: {cnt} - key:{item.Key} - cnt:{item.Value.Count}");

            }

            return overlaps.ToArray();
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="inputVector"></param>
        /// <param name="permChanges"></param>
        /// <param name="activeColumns"></param>
        public void AdaptSynapsesDist(int[] inputVector, double[] permChanges, int[] activeColumns)
        {
            List<KeyPair> list = new List<KeyPair>();
            foreach (var colIndx in activeColumns)
            {
                list.Add(new KeyPair() { Key = colIndx, Value = colIndx });
            }

            var partitions = GetPartitionsForKeyset(list);

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.ActorMap.Count
            };

            Parallel.ForEach(partitions, opts, (placement) =>
            {
                // Result here should ensure reliable messaging. No semantin meaning.
                var res = ((ActorReference)placement.Key.ActorRef).Ask<int>(new AdaptSynapsesMsg
                {
                    PermanenceChanges = permChanges,
                    ColumnKeys = placement.Value
                },
                    this.Config.ConnectionTimeout, placement.Key.NodePath).Result;
            });
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="weakColumns"></param>
        public void BumpUpWeakColumnsDist(int[] weakColumns)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = weakColumns.Length
            };

            List<KeyPair> list = new List<KeyPair>();
            foreach (var weakColIndx in weakColumns)
            {
                list.Add(new KeyPair() { Key = weakColIndx, Value = weakColIndx });
            }

            var partitionMap = GetPartitionsForKeyset(list);

            Parallel.ForEach(partitionMap, opts, (placement) =>
            {
                var res = ((ActorReference)placement.Key.ActorRef).Ask<int>(new BumUpWeakColumnsMsg { ColumnKeys = placement.Value }, this.Config.ConnectionTimeout, placement.Key.NodePath).Result;
            });
        }


        /// <summary>
        /// Groups keys by partitions (actors).
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public Dictionary<Placement<int>, List<KeyPair>> GetPartitionsForKeyset(ICollection<KeyPair> keyValuePairs)
        {
            Dictionary<Placement<int>, List<KeyPair>> res = new Dictionary<Placement<int>, List<KeyPair>>();

            foreach (var partition in this.ActorMap)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (partition.MinKey <= (int)pair.Key && partition.MaxKey >= (int)pair.Key)
                    {
                        if (res.ContainsKey(partition) == false)
                            res.Add(partition, new List<KeyPair>());

                        res[partition].Add(pair);
                    }
                }
            }

            return res;
        }



        /// <summary>
        /// Gets partitions grouped by nodes.
        /// </summary>
        /// <returns></returns>
        //public abstract Dictionary<ActorReference, List<KeyPair>> GetPartitionsByNode();

        /// <summary>
        /// Ads the value with specified keypair.
        /// </summary>
        /// <param name="item">Keypair of the new item.</param>
        public void Add(KeyValuePair<int, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Adds the value with secified key to the right parition.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(int key, TValue value)
        {
            var partActor = GetPartitionActorFromKey(key);

            var isSet = partActor.Ask<int>(new AddElementsMsg()
            {
                Elements = new List<KeyPair>
                    {
                        new KeyPair { Key=key, Value=value }
                    }

            }, this.Config.ConnectionTimeout).Result;

            if (isSet != 1)
                throw new ArgumentException("Cannot add the element with specified key!");
        }

        /// <summary>
        /// Tries to return value from target partition.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>time
        /// <returns></returns>
        public bool TryGetValue(int key, out TValue value)
        {
            var partActor = GetPartitionActorFromKey(key);

            Result result = partActor.Ask<Result>(new GetElementMsg { Key = key }, this.Config.ConnectionTimeout).Result;

            if (result.IsError == false)
            {
                value = (TValue)result.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        public void Clear()
        {
            //TODO Implememtation
            throw new NotImplementedException();

            //foreach (var item in this.dictList)
            //{
            //    item.Clear();
            //}
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<int, TValue> item)
        {
            var actorRef = GetPartitionActorFromKey(item.Key);

            if (ContainsKey(item.Key))
            {
                var val = actorRef.Ask<TValue>(new GetElementMsg()).Result;
                if (EqualityComparer<TValue>.Default.Equals(val, item.Value))
                    return true;
                else
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Checks if element with specified key exists in any partition in cluster.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(int key)
        {
            var actorRef = GetPartitionActorFromKey(key);

            if (actorRef.Ask<bool>(new ContainsMsg { Key = key }).Result)
                return true;
            else
                return false;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<int, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(int key)
        {
            // TODO Implementation
            throw new NotImplementedException();
            //for (int i = 0; i < this.dictList.Length; i++)
            //{
            //    if (this.dictList[i].ContainsKey(key))
            //    {
            //        return this.dictList[i].Remove(key);
            //    }
            //}

            //return false;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<int, TValue> item)
        {
            // TODO implementation
            throw new NotImplementedException();
            //for (int i = 0; i < this.dictList.Length; i++)
            //{
            //    if (this.dictList[i].ContainsKey(item.Key))
            //    {
            //        return this.dictList[i].Remove(item.Key);
            //    }
            //}

            //return false;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #region Enumerators

        /// <summary>
        /// Current dictionary list in enemerator.
        /// </summary>
        private int currentDictIndex = -1;

        /// <summary>
        /// Current index in currentdictionary
        /// </summary>
        private int currentIndex = -1;

        /// <summary>
        /// TODO to be added
        /// </summary>
        public object Current => throw new NotImplementedException();//this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        /// <summary>
        /// TODO to be added
        /// </summary>
        KeyValuePair<int, TValue> IEnumerator<KeyValuePair<int, TValue>>.Current => throw new NotImplementedException();//this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        /// <summary>
        /// Gets number of physical nodes in cluster.
        /// </summary>
        public int Nodes => throw new NotSupportedException();

        public HtmConfig htmConfig { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            throw new NotSupportedException();
            //if (this.currentIndex == -1)
            //    this.currentIndex = 0;

            //if (this.currentDictIndex + 1 < this.dictList.Length)
            //{
            //    this.currentDictIndex++;

            //    if (this.dictList[this.currentDictIndex].Count > 0 && this.dictList[this.currentDictIndex].Count > this.currentIndex)
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    this.currentDictIndex = 0;

            //    if (this.currentIndex + 1 < this.dictList[this.currentDictIndex].Count)
            //    {
            //        this.currentIndex++;
            //        return true;
            //    }
            //    else
            //        return false;
            //}
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        public void Reset()
        {
            this.currentDictIndex = -1;
            this.currentIndex = -1;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        public void Dispose()
        {
            // TODO Implementation
            throw new NotSupportedException();
            //this.dictList = null;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="keys">All keys must belong to same partition. Search object (caller of this method)
        /// should sot keys </param>
        /// <returns></returns>
        /// TODO Unreachable code ??
        public ICollection<KeyPair> GetObjects(int[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentException("Argument 'keys' must be specified!");

            var actorRef = GetPartitionActorFromKey(keys[0]);

            int pageSize = 100;// this.Config.PageSize;
            int alreadyProcessed = 0;

            //while (true)
            //{
            List<object> keysToGet = new List<object>();

            foreach (var key in keys.Skip(alreadyProcessed).Take(pageSize))
            {
                keysToGet.Add(key);
            }

            var batchResult = actorRef.Ask<List<KeyPair>>(new GetElementsMsg()
            {
                Keys = keysToGet.ToArray(),
            }).Result;

            keysToGet.Clear();

            return batchResult;
            //}

            //return null;
        }

        #endregion
    }
}
