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

namespace NeoCortexApi.DistributedComputeLib
{
    public abstract class AkkaDistributedDictionaryBase<TKey, TValue> : IDistributedDictionary<TKey, TValue>, IRemotelyDistributed
    {
        protected AkkaDistributedDictConfig Config { get; }

        private Dictionary<TKey, TValue>[] dictList;

        private IActorRef[] dictActors;

        private int numElements = 0;

        private ActorSystem actSystem;

        public AkkaDistributedDictionaryBase(AkkaDistributedDictConfig config)
        {
            if (config == null)
                throw new ArgumentException("Configuration must be specified.");

            this.Config = config;

            dictActors = new IActorRef[config.Nodes.Count];

            actSystem = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
                akka {  
                    loglevel=Debug
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

            int nodeIndx = 0;

            foreach (var node in this.Config.Nodes)
            {
                dictActors[nodeIndx] =
                  actSystem.ActorOf(Props.Create(() => new DictNodeActor())
                  .WithDeploy(Deploy.None.WithScope(new RemoteScope(Address.Parse(node)))), $"{nameof(DictNodeActor)}-{nodeIndx}");

                //dictActors[nodeIndx] =
                //  actSystem.ActorOf(Props.Create(() => new DictNodeActor<TKey, TValue>())
                //  .WithDeploy(Deploy.None.WithScope(new RemoteScope(Address.Parse(node)))), $"{nameof(DictNodeActor<TKey,TValue>)}-{nodeIndx}");

                var result = dictActors[nodeIndx].Ask<int>(new CreateDictNodeMsg()
                {
                    HtmAkkaConfig = config.ActorConfig,                    
                }, this.Config.ConnectionTimout).Result;


                //result = dictActors[nodeIndx].Ask<int>("abc", this.Config.ConnectionTimout).Result;

                //result = dictActors[nodeIndx].Ask<int>(new CreateDictNodeMsg(), this.Config.ConnectionTimout).Result;
            }
        }

        /// <summary>
        /// Depending on usage (Key type) different mechanism can be used to partition keys.
        /// This method returns the index of the node, whish should hold specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract int GetPartitionNodeIndexFromKey(TKey key);

        public TValue this[TKey key]
        {
            get
            {
                var nodeIndex = GetPartitionNodeIndexFromKey(key);
                TValue val = this.dictActors[nodeIndex].Ask<TValue>("").Result;
                return val;
            }
            set
            {
                var nodeIndex = GetPartitionNodeIndexFromKey(key);

                var isSet = dictActors[nodeIndex].Ask<int>(new UpdateElementsMsg()
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

                Task<int>[] tasks = new Task<int>[this.dictActors.Length];
                int indx = 0;

                foreach (var actor in this.dictActors)
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
            Dictionary<int, AddOrUpdateElementsMsg> list = new Dictionary<int, AddOrUpdateElementsMsg>();

            int pageSize = 100;
            int alreadyProcessed = 0;

            while (true)
            {
                foreach (var item in keyValuePairs.Skip(alreadyProcessed).Take(pageSize))
                {
                    var partitionIndex = GetPartitionNodeIndexFromKey((TKey)item.Key);
                    if (!list.ContainsKey(partitionIndex))
                        list.Add(partitionIndex, new AddOrUpdateElementsMsg() { Elements = new List<KeyPair>() });

                    list[partitionIndex].Elements.Add(new KeyPair { Key = item.Key, Value = item.Value });
                }

                if (list.Count > 0)
                {
                    Task<int>[] tasks = new Task<int>[list.Count];

                    for (int partIndx = 0; partIndx < tasks.Length; partIndx++)
                    {
                        tasks[partIndx] = dictActors[partIndx].Ask<int>(list[partIndx], TimeSpan.FromMinutes(3));
                        alreadyProcessed += list[partIndx].Elements.Count;
                    }

                    Task.WaitAll(tasks);

                    list.Clear();
                }
                else
                    break;
            }
        }




        /// <summary>
        /// Adds/Updates batch of elements to remote nodes.
        /// </summary>
        /// <param name="keyValuePairs"></param>
        //public void AddOrUpdatePerf(ICollection<KeyPair> keyValuePairs)
        //{
        //    for (int k = 0; k < 10; k++)
        //    {
        //        Debug.WriteLine($"----------- {k} -----------");
        //        List<int> pages = new List<int>();
        //        pages.Add(1000);
        //        pages.Add(500);
        //        pages.Add(250);
        //        pages.Add(100);
        //        pages.Add(50);
        //        pages.Add(10);
        //        pages.Add(1);

        //        foreach (var pageSize in pages)
        //        {
        //            Stopwatch sw = new Stopwatch();
        //            sw.Start();

        //            Dictionary<int, AddOrUpdateElementsMsg> list = new Dictionary<int, AddOrUpdateElementsMsg>();

        //            //int pageSize = 10;
        //            int alreadyProcessed = 0;
        //            //Debug.WriteLine("-------------");
        //            while (true)
        //            {
        //                foreach (var item in keyValuePairs.Skip(alreadyProcessed).Take(pageSize))
        //                {
        //                    var partitionIndex = GetPartitionNodeIndexFromKey((TKey)item.Key);
        //                    if (!list.ContainsKey(partitionIndex))
        //                        list.Add(partitionIndex, new AddOrUpdateElementsMsg() { Elements = new List<KeyPair>() });

        //                    list[partitionIndex].Elements.Add(new KeyPair { Key = item.Key, Value = item.Value });
        //                }

        //                if (list.Count > 0)
        //                {
        //                    Task<int>[] tasks = new Task<int>[list.Count];

        //                    for (int partIndx = 0; partIndx < tasks.Length; partIndx++)
        //                    {
        //                        //Debug.Write(".");
        //                        tasks[partIndx] = dictActors[partIndx].Ask<int>(list[partIndx], TimeSpan.FromMinutes(3));
        //                        alreadyProcessed += list[partIndx].Elements.Count;
        //                    }

        //                    Task.WaitAll(tasks);

        //                    list.Clear();
        //                }
        //                else
        //                    break;
        //            }
        //            sw.Stop();
        //            Debug.WriteLine("");
        //            Debug.WriteLine($"{pageSize} | {sw.ElapsedMilliseconds}");
        //        }
        //    }
        //}

        /// <summary>
        /// Ads the value with secified key to the right parition.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            var nodeIndex = GetPartitionNodeIndexFromKey(key);

            var isSet = dictActors[nodeIndex].Ask<int>(new AddElementsMsg()
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
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var nodeIndex = GetPartitionNodeIndexFromKey(key);

            Result result = dictActors[nodeIndex].Ask<Result>(new GetElementMsg { Key = key }).Result;

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

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            int partitionId = GetPartitionNodeIndexFromKey(item.Key);
            this.dictList[partitionId].Add(item.Key, item.Value);
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
            int partitionId = GetPartitionNodeIndexFromKey(item.Key);

            if (ContainsKey(item.Key))
            {
                var val = this.dictActors[partitionId].Ask<TValue>(new GetElementMsg()).Result;
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
            int partitionId = GetPartitionNodeIndexFromKey(key);

            if (this.dictActors[partitionId].Ask<bool>(new ContainsMsg { Key = key }).Result)
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


        public bool MoveNextOLD()
        {
            if (this.currentDictIndex == -1)
                this.currentDictIndex++;

            if (this.currentIndex + 1 < this.dictList[this.currentDictIndex].Count)
            {
                this.currentIndex++;
                return true;
            }
            else
            {
                if (this.currentDictIndex < this.dictList.Length)
                {
                    this.currentDictIndex++;

                    if (this.dictList[this.currentDictIndex].Count > 0)
                        return true;
                    else
                        return false;
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
        #endregion
    }
}
