// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Entities;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Distributes huge dictionary across mutliple dictionaries. Used mainly for testing purposes.
    /// Special case of this dictionary is with number of nodes = 1. In this case dictionary is redused 
    /// to a single dictionary, which corresponds original none-distributed implementation of SP and TM.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class InMemoryDistributedDictionary<TKey, TValue> : IDistributedDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue>[] dictList;

        private int numElements = 0;

        public InMemoryDistributedDictionary(int numNodes)
        {
            dictList = new Dictionary<TKey, TValue>[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                dictList[i] = new Dictionary<TKey, TValue>();
            }
        }


        public ICollection<KeyPair> GetObjects(TKey[] keys)
        {
            List<KeyPair> objects = new List<KeyPair>();
            foreach (var key in keys)
            {
                objects.Add(new KeyPair { Value = this[key], Key = key });
            }

            return objects;
        }


        public TValue this[TKey key]
        {
            get
            {
                foreach (var item in this.dictList)
                {
                    if (item.ContainsKey(key))
                        return item[key];
                }

                throw new ArgumentException("No such key.");
            }
            set
            {
                bool isSet = false;
                for (int i = 0; i < this.dictList.Length; i++)
                {
                    if (this.dictList[i].ContainsKey(key))
                    {
                        this.dictList[i][key] = value;
                        isSet = true;
                        break;
                    }
                }

                if (!isSet)
                    throw new ArgumentException("Cannot find the element with specified key!");
            }
        }

        private int GetPartitionIndex(int elementIndx)
        {
            return elementIndx % this.dictList.Length;
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


        public int Count
        {
            get
            {
                int cnt = 0;

                foreach (var item in this.dictList)
                {
                    cnt += item.Values.Count;
                }

                return cnt;
            }
        }

        public bool IsReadOnly => false;

        /// <summary>
        /// Adds list of objects to dictioanary.
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public void AddOrUpdate(ICollection<KeyPair> keyValuePairs)
        {
            foreach (var item in keyValuePairs)
            {
                Add((TKey)item.Key, (TValue)item.Value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            int partitionInd = GetPartitionIndex(numElements++);
            this.dictList[partitionInd].Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            int partitionInd = GetPartitionIndex(++numElements);
            this.dictList[partitionInd].Add(item.Key, item.Value);
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
            for (int i = 0; i < this.dictList.Length; i++)
            {
                if (this.dictList[i].ContainsKey(item.Key))
                {
                    if (EqualityComparer<TValue>.Default.Equals(this.dictList[i][item.Key], item.Value))
                        return true;
                    else
                        return false;
                }
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < this.dictList.Length; i++)
            {
                if (this.dictList[i].ContainsKey(key))
                {
                        return true;
                }
            }

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

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (int i = 0; i < this.dictList.Length; i++)
            {
                if (this.dictList[i].ContainsKey(key))
                {
                    value = this.dictList[i][key];
                    return true;
                }
            }

            value = default;
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
        private int currentIndex =-1;

        public object Current => this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        /// <summary>
        /// Not used.
        /// </summary>
        public HtmConfig HtmConfig { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
