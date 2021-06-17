// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using NeoCortexApi.Entities;
using System.Collections.Concurrent;
using NeoCortexApi.Utility;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NeoCortexApi.DistributedComputeLib
{
    public abstract class AkkaDistributedDictionaryBase<TKey, TValue> : IDistributedDictionary<TKey, TValue>, IHtmDistCalculus
    {

        // not used!
        private Dictionary<TKey, TValue>[] m_DictList;


        /// <summary>
        /// List of actors, which hold partitions.
        /// </summary>
        //private IActorRef[] dictActors;

        private List<Placement<TKey>> m_ActorMap;

        /// <summary>
        /// Maps from Actor partition identifier to actor index in <see cref="dictActors" list./>
        /// </summary>
        protected List<Placement<TKey>> ActorMap
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

        private int m_NumElements = 0;

       
        #region Properties
        /// <summary>
        /// Akka cluster configuration.
        /// </summary>
        public AkkaDistributedDictConfig Config { get; set; }

        /// <summary>
        /// Configuration used to initialize HTM actor.
        /// </summary>
        public HtmConfig HtmConfig { get; set; }
        #endregion

        /// <summary>
        /// Creates all required actors, which host partitions.
        /// </summary>
        /// <param name="config"></param>
        public AkkaDistributedDictionaryBase(object cfg, ILogger logger = null)
        {
            AkkaDistributedDictConfig config = (AkkaDistributedDictConfig)cfg;
            if (config == null)
                throw new ArgumentException("Configuration must be specified.");

            this.Config = config;

            m_ActSystem = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
                akka {  
                    loglevel=DEBUG
                    actor{
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""  		               
                    }
                    remote {
		                connection-timeout = 120 s
		                transport-failure-detector {
			                heartbeat-interval = 10 s 
			                acceptable-heartbeat-pause = 60 s 
			                unreachable-nodes-reaper-interval = 10 s
			                expected-response-after = 30 s
			                retry-gate-closed-for = 5 s
			                prune-quarantine-marker-after = 2 d
			                system-message-ack-piggyback-timeout = 3 s
		                }
                        dot-netty.tcp {
                            maximum-frame-size = 64000000b
		                    port = 8080
		                    hostname = 0.0.0.0
                            public-hostname = DADO-SR1
                        }
                    }
                }"));

        }

        /// <summary>
        /// Creates partition actors on cluster.
        /// </summary>
        protected void InitPartitionActorsDist()
        {
            // Creates partition placements.
            this.ActorMap = CreatePartitionMap();

            for (int i = 0; i < this.ActorMap.Count; i++)
            {
                string actorName = $"{nameof(DictNodeActor)}-{Guid.NewGuid()}-{this.ActorMap[i].NodeIndx}-{this.ActorMap[i].PartitionIndx}";
                
                //var sel = actSystem.ActorSelection($"/user/{actorName}");
                //this.ActorMap[i].ActorRef = sel.ResolveOne(TimeSpan.FromSeconds(1)).Result;
                //if (this.ActorMap[i].ActorRef == null)
                {
                    this.ActorMap[i].ActorRef =
                     m_ActSystem.ActorOf(Props.Create(() => new DictNodeActor())
                     .WithDeploy(Deploy.None.WithScope(new RemoteScope(Address.Parse(this.ActorMap[i].NodePath)))),
                     actorName);
                }
              
                var result = ((IActorRef)this.ActorMap[i].ActorRef).Ask<int>(new CreateDictNodeMsg()
                {
                    HtmAkkaConfig = this.HtmConfig,
                }, this.Config.ConnectionTimeout).Result;
            }
        }

        /// <summary>
        /// Calculates partition placements on nodes.
        /// </summary>
        /// <returns></returns>
        public abstract List<Placement<TKey>> CreatePartitionMap();

        /// <summary>
        /// Depending on usage (Key type) different mechanism can be used to partition keys.
        /// This method returns the index of the node, whish should hold specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract IActorRef GetPartitionActorFromKey(TKey key);

        /// <summary>
        /// Gets partitions (nodes) with assotiated indexes.
        /// </summary>
        /// <returns></returns>
        //public abstract List<(int partId, int minKey, int maxKey)> GetPartitions();

        public TValue this[TKey key]
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
                    tasks[indx++] = ((IActorRef)actor).Ask<int>(new GetCountMsg(), this.Config.ConnectionTimeout);
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

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>();
                foreach (var item in this.m_DictList)
                {
                    foreach (var k in item.Keys)
                    {
                        keys.Add(k);
                    }
                }

                return keys;
            }
        }


        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> keys = new List<TValue>();
                foreach (var item in this.m_DictList)
                {
                    foreach (var k in item.Values)
                    {
                        keys.Add(k);
                    }
                }

                return keys;
            }
        }



        public bool IsReadOnly => false;

        /// <summary>
        /// Adds/Updates batch of elements to remote nodes.
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public void AddOrUpdate(ICollection<KeyPair> keyValuePairs)
        {
            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.ActorMap.Count
            };

            // We get here keys grouped to actors, which host partitions.
            var partitions = GetPartitionsForKeyset(keyValuePairs);

            //
            // Here is upload performed in context of every actor (partition).
            // Because keys are grouped by partitions (actors) parallel upload can be done here.
            Parallel.ForEach(partitions, opts, (partition) =>
            {
                Dictionary<IActorRef, AddOrUpdateElementsMsg> list = new Dictionary<IActorRef, AddOrUpdateElementsMsg>();

                int pageSize = this.Config.PageSize;
                int alreadyProcessed = 0;

                while (true)
                {
                    foreach (var item in partition.Value.Skip(alreadyProcessed).Take(pageSize))
                    {
                        var actorRef = GetPartitionActorFromKey((TKey)item.Key);
                        if (!list.ContainsKey(actorRef))
                            list.Add(actorRef, new AddOrUpdateElementsMsg() { Elements = new List<KeyPair>() });

                        list[actorRef].Elements.Add(new KeyPair { Key = item.Key, Value = item.Value });
                    }

                    if (list.Count > 0)
                    {
                        List<Task<int>> tasks = new List<Task<int>>();

                        foreach (var item in list)
                        {
                            tasks.Add(item.Key.Ask<int>(item.Value, this.Config.ConnectionTimeout));
                            alreadyProcessed += item.Value.Elements.Count;
                        }

                        Task.WaitAll(tasks.ToArray());

                        list.Clear();
                    }
                    else
                        break;
                }
            });
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

            //foreach (var item in this.ActorMap)
            //{
            //    if ((int)(object)item.MinKey > (int)(object)item.MaxKey)
            //    {

            //    }
            //}

            RunBatched((batchOfElements)=> {
                // Run overlap calculation on all actors(in all partitions)
                Parallel.ForEach(batchOfElements, opts, (placement) =>
                {
                    //if (((int)(object)placement.MinKey) >= ((int)(object)placement.MaxKey))
                    //{

                    //}

                    var res = ((IActorRef)placement.ActorRef).Ask<int>(new InitColumnsMsg()
                    {
                        PartitionKey = placement.PartitionIndx,
                        MinKey = (int)(object)placement.MinKey,
                        MaxKey = (int)(object)placement.MaxKey
                    }, this.Config.ConnectionTimeout).Result;
                });
            }, this.ActorMap, this.Config.ProcessingBatch);

            //int processedItems = 0;
            //int batchCnt = 0;

            //var shuffledList = ArrayUtils.Shuffle(this.ActorMap);

            //List<Placement<TKey>> batchedList = new List<Placement<TKey>>();

            //while (processedItems < this.ActorMap.Count)
            //{              
            //    if (batchCnt < this.Config.ProcessingBatch && processedItems < this.ActorMap.Count)
            //    {
            //        batchedList.Add(shuffledList[processedItems]);
            //        processedItems++;
            //        batchCnt++;
            //    }
            //    else
            //    {
          
            //        // Run overlap calculation on all actors(in all partitions)
            //        Parallel.ForEach(batchedList, opts, (placement) =>
            //        {
            //            var res = placement.ActorRef.Ask<int>(new InitColumnsMsg()
            //            {
            //                MinKey = (int)(object)placement.MinKey,
            //                MaxKey = (int)(object)placement.MaxKey
            //            }, this.Config.ConnectionTimeout).Result;
            //        });

            //        batchCnt = 0;
            //        batchedList = new List<Placement<TKey>>();
            //    }             
            //}
        }

        private void RunBatched(Action<List<Placement<TKey>>> func, List<Placement<TKey>> list, int batchSize)
        {
            int processedItems = 0;
            int batchCnt = 0;

            var shuffledList = ArrayUtils.Shuffle(list);

            List<Placement<TKey>> batchedList = new List<Placement<TKey>>();

            while (processedItems < this.ActorMap.Count)
            {
                if (batchCnt < batchSize && processedItems < this.ActorMap.Count)
                {
                    batchedList.Add(shuffledList[processedItems]);
                    processedItems++;
                    batchCnt++;
                }
                else
                {
                    func(batchedList);

                    batchCnt = 0;
                    batchedList = new List<Placement<TKey>>();
                }
            }
        }

        /// <summary>
        /// Performs remote initialization and configuration of all coulms in all partitions.
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

            //var avgSpanOfPart = this.ActorMap.First().ActorRef.Ask<double>(new ConnectAndConfigureColumnsMsg(), this.Config.ConnectionTimeout).Result;
            //aggLst.TryAdd(this.ActorMap.First().PartitionIndx, avgSpanOfPart);

            RunBatched((batchOfElements) => {
                Parallel.ForEach(batchOfElements, opts, (placement) =>
                {
                    while (true)
                    {
                        try
                        {
                            //Debug.WriteLine($"C: {placement.ActorRef.Path}");
                            var avgSpanOfPart = ((IActorRef)placement.ActorRef).Ask<double>(new ConnectAndConfigureColumnsMsg(), this.Config.ConnectionTimeout).Result;
                            aggLst.TryAdd(placement.PartitionIndx, avgSpanOfPart);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                });
            }, this.ActorMap, this.Config.ProcessingBatch / 2);
          

            return aggLst.Values.ToList();
        }

        public int[] CalculateOverlapDist(int[] inputVector)
        {
            ConcurrentDictionary<int, List<KeyPair>> overlapList = new ConcurrentDictionary<int, List<KeyPair>>();

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.ActorMap.Count
            };

            // Run overlap calculation on all actors(in all partitions)
            Parallel.ForEach(this.ActorMap, opts, (placement) =>
            {
                var partitionOverlaps = ((IActorRef)placement.ActorRef).Ask<List<KeyPair>>(new CalculateOverlapMsg { InputVector = inputVector }, this.Config.ConnectionTimeout).Result;
                overlapList.TryAdd(placement.PartitionIndx, partitionOverlaps);
            });

            List<int> overlaps = new List<int>();

            int cnt = 0;

            foreach (var item in overlapList.OrderBy(i => i.Key))
            {
                foreach (var keyPair in item.Value)
                {
                    cnt++;
                    overlaps.Add((int)keyPair.Value);
                }

                Debug.WriteLine($"cnt: {cnt} - key:{item.Key} - cnt:{item.Value.Count}");

            }

            return overlaps.ToArray();
        }

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
                var res = placement.Key.Ask<int>(new AdaptSynapsesMsg
                {
                    PermanenceChanges = permChanges,
                    ColumnKeys = placement.Value }, 
                    this.Config.ConnectionTimeout).Result;
            });
        }


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
                placement.Key.Ask<int>(new BumUpWeakColumnsMsg { ColumnKeys = placement.Value }, this.Config.ConnectionTimeout);
            });
        }

        /// <summary>
        /// Gets list of partitions, which host specified keys.
        /// </summary>
        /// <param name="keyValuePairs">Keyvalue pairs.</param>
        /// <returns></returns>
        public abstract Dictionary<IActorRef, List<KeyPair>> GetPartitionsForKeyset(ICollection<KeyPair> keyValuePairs);

        /// <summary>
        /// Gets partitions grouped by nodes.
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<IActorRef, List<KeyPair>> GetPartitionsByNode();

        /// <summary>
        /// Ads the value with specified keypair.
        /// </summary>
        /// <param name="item">Keypair of the new item.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Ads the value with secified key to the right parition.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
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
        public bool TryGetValue(TKey key, out TValue value)
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



        public void Clear()
        {
            foreach (var item in this.m_DictList)
            {
                item.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
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
        public bool ContainsKey(TKey key)
        {
            var actorRef = GetPartitionActorFromKey(key);

            if (actorRef.Ask<bool>(new ContainsMsg { Key = key }).Result)
                return true;
            else
                return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            for (int i = 0; i < this.m_DictList.Length; i++)
            {
                if (this.m_DictList[i].ContainsKey(key))
                {
                    return this.m_DictList[i].Remove(key);
                }
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            for (int i = 0; i < this.m_DictList.Length; i++)
            {
                if (this.m_DictList[i].ContainsKey(item.Key))
                {
                    return this.m_DictList[i].Remove(item.Key);
                }
            }

            return false;
        }



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

        public object Current => this.m_DictList[this.currentDictIndex].ElementAt(currentIndex);

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => this.m_DictList[this.currentDictIndex].ElementAt(currentIndex);

        /// <summary>
        /// Gets number of physical nodes in cluster.
        /// </summary>
        public int Nodes => this.Config.Nodes.Count;


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this;
        }


        public bool MoveNext()
        {
            if (this.currentIndex == -1)
                this.currentIndex = 0;

            if (this.currentDictIndex + 1 < this.m_DictList.Length)
            {
                this.currentDictIndex++;

                if (this.m_DictList[this.currentDictIndex].Count > 0 && this.m_DictList[this.currentDictIndex].Count > this.currentIndex)
                    return true;
                else
                    return false;
            }
            else
            {
                this.currentDictIndex = 0;

                if (this.currentIndex + 1 < this.m_DictList[this.currentDictIndex].Count)
                {
                    this.currentIndex++;
                    return true;
                }
                else
                    return false;
            }
        }


        public void Reset()
        {
            this.currentDictIndex = -1;
            this.currentIndex = -1;
        }

        public void Dispose()
        {
            this.m_DictList = null;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="keys">All keys must belong to same partition. Search object (caller of this method)
        /// should sot keys </param>
        /// <returns></returns>
        /// TODO Unreachable code ??
        public ICollection<KeyPair> GetObjects(TKey[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentException("Argument 'keys' must be specified!");

            var actorRef = GetPartitionActorFromKey(keys[0]);

            int pageSize = this.Config.PageSize;
            int alreadyProcessed = 0;

            while (true)
            {
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
            }

            return null;
        }

        #endregion
    }
}
