using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using System.Threading.Tasks;
using System.Diagnostics;
using NeoCortexApi.Entities;
using System.Collections.Concurrent;
using NeoCortexApi.Utility;

namespace NeoCortexApi.DistributedComputeLib
{
    public abstract class AkkaDistributedDictionaryBase<TKey, TValue> : IDistributedDictionary<TKey, TValue>, IRemotelyDistributed
    {

        // not used!
        private Dictionary<TKey, TValue>[] dictList;


        /// <summary>
        /// List of actors, which hold partitions.
        /// </summary>
        //private IActorRef[] dictActors;

        private List<Placement<TKey>> actorMap;

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

        private ActorSystem actSystem;

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
        public AkkaDistributedDictionaryBase(AkkaDistributedDictConfig config)
        {
            if (config == null)
                throw new ArgumentException("Configuration must be specified.");

            this.Config = config;

            actSystem = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
                akka {  
                    loglevel=Info
                    actor{
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""  		               
                    }
                    remote {
                        connection-timeout = 120 s
                        helios.tcp {
                            maximum-frame-size = 326000000b
		                    port = 0
		                    hostname = localhost                            
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
                this.ActorMap[i].ActorRef =
                 actSystem.ActorOf(Props.Create(() => new DictNodeActor())
                 .WithDeploy(Deploy.None.WithScope(new RemoteScope(Address.Parse(this.ActorMap[i].NodeUrl)))),
                 $"{nameof(DictNodeActor)}-{this.ActorMap[i].NodeIndx}-{this.ActorMap[i].PartitionIndx}");

                var result = this.ActorMap[i].ActorRef.Ask<int>(new CreateDictNodeMsg()
                {
                    HtmAkkaConfig = this.HtmConfig,
                }, this.Config.ConnectionTimout).Result;
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

                }, this.Config.ConnectionTimout).Result;

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
                    tasks[indx++] = actor.Ask<int>(new GetCountMsg(), this.Config.ConnectionTimout);
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
                foreach (var item in this.dictList)
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
                foreach (var item in this.dictList)
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
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = this.ActorMap.Count;

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
                            tasks.Add(item.Key.Ask<int>(item.Value, TimeSpan.FromMinutes(3)));
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

            // We get here keys grouped to actors, which host partitions.
            //var partitions = GetPartitionsForKeyset(keyValuePairs);
            var partitions = GetPartitions();
            // Run overlap calculation on all actors(in all partitions)
            Parallel.ForEach(this.ActorMap, opts, (placement) =>
            {
                var res = placement.ActorRef.Ask<int>(new InitColumnsMsg()
                {
                    MinKey = (int)(object)placement.MinKey,
                    MaxKey = (int)(object)placement.MaxKey
                }, this.Config.ConnectionTimout).Result;
            });

            /*
             * //
            // Here is upload performed in context of every actor (partition).
            // Because keys are grouped by partitions (actors) parallel upload can be done here.
            Parallel.ForEach(partitions, opts, (partition) =>
            {
              
                Dictionary<IActorRef, InitColumnsMsg> list = new Dictionary<IActorRef, InitColumnsMsg>();

                int pageSize = this.Config.PageSize;
                int alreadyProcessed = 0;

                while (true)
                {
                    foreach (var item in partition.Value.Skip(alreadyProcessed).Take(pageSize))
                    {
                        var actorRef = GetPartitionActorFromKey((TKey)item.Key);
                        if (!list.ContainsKey(actorRef))
                            list.Add(actorRef, new InitColumnsMsg() { Elements = new List<KeyPair>() });

                        list[actorRef].Elements.Add(new KeyPair { Key = item.Key, Value = item.Value });
                    }

                    if (list.Count > 0)
                    {
                        List<Task<int>> tasks = new List<Task<int>>();

                        foreach (var item in list)
                        {
                            tasks.Add(item.Key.Ask<int>(item.Value, TimeSpan.FromMinutes(3)));
                            alreadyProcessed += item.Value.Elements.Count;
                        }

                        Task.WaitAll(tasks.ToArray());

                        list.Clear();
                    }
                    else
                        break;
                }
                
            });*/
        }


        /// <summary>
        /// Performs remote initialization and configuration of all coulms in all partitions.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <returns>List of average spans of all columns on this node in this partition.
        /// This list is agreggated by caller to estimate average span for all system.</returns>
        public List<double> ConnectAndConfigureInputsDist(HtmConfig htmConfig)
        {
            // List of results.
            ConcurrentDictionary<int, double> aggLst = new ConcurrentDictionary<int, double>();

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = this.ActorMap.Count;

            Parallel.ForEach(this.ActorMap, opts, (placement) =>
            {
                var avgSpanOfPart = placement.ActorRef.Ask<double>(new ConnectAndConfigureColumnsMsg(), this.Config.ConnectionTimout).Result;
                aggLst.TryAdd(placement.PartitionIndx, avgSpanOfPart);
            });

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
                var partitionOverlaps = placement.ActorRef.Ask<List<KeyPair>>(new CalculateOverlapMsg { InputVector = inputVector }, this.Config.ConnectionTimout).Result;
                overlapList.TryAdd(placement.PartitionIndx, partitionOverlaps);
            });

            //foreach (var placement  in this.ActorMap)
            //{
            //    var partitionOverlaps = placement.ActorRef.Ask<List<KeyPair>>(new CalculateOverlapMsg { InputVector = inputVector }, this.Config.ConnectionTimout).Result;
            //    overlapList.TryAdd(placement.PartitionIndx, partitionOverlaps);
            //}

            List<int> overlaps = new List<int>();
            foreach (var item in overlapList.OrderBy(i => i.Key))
            {
                foreach (var keyPair in item.Value)
                {
                    overlaps.Add((int)keyPair.Value);
                }
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
                    ColumnKeys = placement.Value }, 
                    this.Config.ConnectionTimout).Result;
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

            }, this.Config.ConnectionTimout).Result;

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

            Result result = partActor.Ask<Result>(new GetElementMsg { Key = key }, this.Config.ConnectionTimout).Result;

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
            foreach (var item in this.dictList)
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
            for (int i = 0; i < this.dictList.Length; i++)
            {
                if (this.dictList[i].ContainsKey(key))
                {
                    return this.dictList[i].Remove(key);
                }
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            for (int i = 0; i < this.dictList.Length; i++)
            {
                if (this.dictList[i].ContainsKey(item.Key))
                {
                    return this.dictList[i].Remove(item.Key);
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

        public object Current => this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => this.dictList[this.currentDictIndex].ElementAt(currentIndex);

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
