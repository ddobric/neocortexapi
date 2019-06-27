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
using AkkaSb.Net;
using Microsoft.Extensions.Logging;

namespace NeoCortexApi.DistributedComputeLib
{
    public abstract class ActorSbDistributedDictionaryBase<TKey, TValue> : IDistributedDictionary<TKey, TValue>, IHtmDistCalculus
    {
        private List<Placement<TKey>> actorMap;

        private AkkaSb.Net.ActorSystem actorSystem;

        /// <summary>
        /// Maps from Actor partition identifier to actor index in <see cref="dictActors" list./>
        /// </summary>
        protected List<Placement<TKey>> ActorMap
        {
            get
            {
                if (actorMap == null)
                    InitPartitionActorsDist();

                return this.actorMap;
            }
            set
            {
                this.actorMap = value;
            }
        }

        private int numElements = 0;


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
        public ActorSbDistributedDictionaryBase(object cfg, ILogger logger)
        {
            ActorSbConfig config = (ActorSbConfig)cfg;

            if (config == null)
                throw new ArgumentException("Configuration must be specified.");

            this.Config = config;

            this.actorSystem = new ActorSystem("HtmCalculusDriver", config, logger);
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

                ActorReference actorRef1 = actorSystem.CreateActor<HtmActor>(1);

                this.ActorMap[i].ActorRef = actorSystem.CreateActor<HtmActor>(new ActorId(this.ActorMap[i].PartitionIndx));
                 
                var result = ((ActorReference)this.ActorMap[i].ActorRef).Ask<int>(new CreateDictNodeMsg()
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
        protected abstract ActorReference GetPartitionActorFromKey(TKey key);

        /// <summary>
        /// Gets partitions (nodes) with assotiated indexes.
        /// </summary>
        /// <returns></returns>
        public abstract List<(int partId, int minKey, int maxKey)> GetPartitions();

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

        public ICollection<TKey> Keys
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

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = this.ActorMap.Count;

            // We get here keys grouped to actors, which host partitions.
            var partitions = GetPartitionsForKeyset(keyValuePairs);

            //
            // Here is upload performed in context of every actor (partition).
            // Because keys are grouped by partitions (actors) parallel upload can be done here.
            Parallel.ForEach(partitions, opts, (partition) =>
            {
                Dictionary<ActorReference, AddOrUpdateElementsMsg> list = new Dictionary<ActorReference, AddOrUpdateElementsMsg>();

                int pageSize = 100;
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
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Environment.ProcessorCount;

            runBatched((batchOfElements) =>
            {
                // Run overlap calculation on all actors(in all partitions)
                Parallel.ForEach(batchOfElements, opts, (placement) =>
                {
                    var res = ((ActorReference)placement.ActorRef).Ask<int>(new InitColumnsMsg()
                    {
                        PartitionKey = placement.PartitionIndx,
                        MinKey = (int)(object)placement.MinKey,
                        MaxKey = (int)(object)placement.MaxKey
                    }, this.Config.ConnectionTimeout).Result;
                });
            }, this.ActorMap, this.Config.BatchSize);

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

        private void runBatched(Action<List<Placement<TKey>>> p, List<Placement<TKey>> actorMap, object batchSize)
        {
            throw new NotImplementedException();
        }

        private void runBatched(Action<List<Placement<TKey>>> func, List<Placement<TKey>> list, int batchSize)
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

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Environment.ProcessorCount;

            runBatched((batchOfElements) =>
            {
                Parallel.ForEach(batchOfElements, opts, (placement) =>
                {
                    while (true)
                    {
                        try
                        {
                            //Debug.WriteLine($"C: {placement.ActorRef.Path}");
                            var avgSpanOfPart = ((ActorReference)placement.ActorRef).Ask<double>(new ConnectAndConfigureColumnsMsg(), this.Config.ConnectionTimeout).Result;
                            aggLst.TryAdd(placement.PartitionIndx, avgSpanOfPart);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                });
            }, this.ActorMap, this.Config.BatchSize / 2);


            return aggLst.Values.ToList();
        }

        public int[] CalculateOverlapDist(int[] inputVector)
        {
            ConcurrentDictionary<int, List<KeyPair>> overlapList = new ConcurrentDictionary<int, List<KeyPair>>();

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = this.ActorMap.Count;

            // Run overlap calculation on all actors(in all partitions)
            Parallel.ForEach(this.ActorMap, opts, (placement) =>
            {
                var partitionOverlaps = ((ActorReference)placement.ActorRef).Ask<List<KeyPair>>(new CalculateOverlapMsg { InputVector = inputVector }, this.Config.ConnectionTimeout).Result;
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

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = this.ActorMap.Count;

            Parallel.ForEach(partitions, opts, (placement) =>
            {
                // Result here should ensure reliable messaging. No semantin meaning.
                var res = placement.Key.Ask<int>(new AdaptSynapsesMsg
                {
                    PermanenceChanges = permChanges,
                    ColumnKeys = placement.Value
                },
                    this.Config.ConnectionTimeout).Result;
            });
        }


        public void BumpUpWeakColumnsDist(int[] weakColumns)
        {
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = weakColumns.Length;

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
        public abstract Dictionary<ActorReference, List<KeyPair>> GetPartitionsForKeyset(ICollection<KeyPair> keyValuePairs);

        /// <summary>
        /// Gets partitions grouped by nodes.
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<ActorReference, List<KeyPair>> GetPartitionsByNode();

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
                value = default(TValue);
                return false;
            }
        }



        public void Clear()
        {
            throw new NotImplementedException();

            //foreach (var item in this.dictList)
            //{
            //    item.Clear();
            //}
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

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
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

        public object Current => throw new NotImplementedException()//this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => throw new NotImplementedException();//this.dictList[this.currentDictIndex].ElementAt(currentIndex);

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

            if (this.currentDictIndex + 1 < this.dictList.Length)
            {
                this.currentDictIndex++;

                if (this.dictList[this.currentDictIndex].Count > 0 && this.dictList[this.currentDictIndex].Count > this.currentIndex)
                    return true;
                else
                    return false;
            }
            else
            {
                this.currentDictIndex = 0;

                if (this.currentIndex + 1 < this.dictList[this.currentDictIndex].Count)
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
            this.dictList = null;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="keys">All keys must belong to same partition. Search object (caller of this method)
        /// should sot keys </param>
        /// <returns></returns>
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
