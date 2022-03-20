// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeoCortexApi.Entities;
using System.IO;

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

        /// <summary>
        /// Used while partitioning.
        /// </summary>
        private int numElements = 0;

        public bool IsReadOnly => false;

        /// <summary>
        /// The number of elements in the dictionary.
        /// </summary>
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


        /// <summary>
        /// Initializes the inmemory dictionary that can be span over multiple nodes.
        /// </summary>
        /// <param name="numNodes">Default value is single node.</param>
        public InMemoryDistributedDictionary(int numNodes = 1)
        {
            if (numNodes <= 0)
                throw new ArgumentException("numNodes must be 1 or higher.");

            dictList = new Dictionary<TKey, TValue>[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                dictList[i] = new Dictionary<TKey, TValue>();
            }
        }

        public InMemoryDistributedDictionary()
        {
            
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
        private int currentIndex = -1;

        public object Current => this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => this.dictList[this.currentDictIndex].ElementAt(currentIndex);

        /// <summary>
        /// Not used.
        /// </summary>
        public HtmConfig htmConfig { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        public bool Equals (InMemoryDistributedDictionary<TKey, TValue> obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;
            if (htmConfig == null)
            {
                if (obj.htmConfig != null)
                    return false;
            }
            else if (!htmConfig.Equals(obj.htmConfig))
                return false;
            if (!dictList.SequenceEqual(obj.dictList))
                return false;
            if (numElements != obj.numElements)
                return false;
            if (IsReadOnly != obj.IsReadOnly)
                return false;
            if (!Keys.SequenceEqual(obj.Keys))
                return false;
            if (!Values.SequenceEqual(obj.Values))
                return false;
            if (Count != obj.Count)
                return false;
            if (currentDictIndex != obj.currentDictIndex)
                return false;
            if (currentIndex != obj.currentIndex)
                return false;
            if (Current != obj.Current)
                return false;

            return true;

        }
        public  void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();
            
            ser.SerializeBegin(nameof(InMemoryDistributedDictionary<TKey, TValue>), writer);
            //ser.SerializeValue(typeof(TKey).AssemblyQualifiedName, writer);
            //ser.SerializeValue(typeof(TValue).AssemblyQualifiedName, writer);
            //ser.SerializeValue(dictList, writer);

            // ser.SerializeValue(this.dictList, writer);
            ser.SerializeValue(this.numElements, writer);
            //ser.SerializeValue(this.IsReadOnly, writer);
            //ser.SerializeValue(this.Keys, writer);
            //ser.SerializeValue(this.Values, writer);
            //ser.SerializeValue(this.Count, writer);
            ser.SerializeValue(this.currentDictIndex, writer);
            ser.SerializeValue(this.currentIndex, writer);
            //ser.SerializeValue(this.Current, writer);

            int dictCnt = 0;

            foreach (var dict in dictList)
            {
                ser.SerializeBegin("dictionary", writer);
                
                ser.SerializeValue(dictCnt, writer);

                foreach (var item in dict)
                {
                    if (typeof(TKey) == typeof(int))
                        ser.SerializeValue(item.Key.ToString(), writer);
                    else
                        throw new NotSupportedException();
                }
            }
            if (this.htmConfig != null)
            { this.htmConfig.Serialize(writer); }
            
            ser.SerializeEnd(nameof(InMemoryDistributedDictionary<TKey, TValue>), writer);
        }
        public static InMemoryDistributedDictionary<TKey, TValue> Deserialize(StreamReader sr)
        {
                InMemoryDistributedDictionary<TKey, TValue> keyValues = new InMemoryDistributedDictionary<TKey, TValue>();

                HtmSerializer2 ser = new HtmSerializer2();

                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();
                    if (data == String.Empty || data == ser.ReadBegin(nameof(InMemoryDistributedDictionary<TKey, TValue>)))
                    { 
                        continue;
                    }
                    else if (data == ser.ReadBegin(nameof(HtmConfig)))
                    {
                        keyValues.htmConfig = HtmConfig.Deserialize(sr);
                    }
                    else if (data == ser.ReadEnd(nameof(InMemoryDistributedDictionary<TKey, TValue>)))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                        switch (i)
                        {
                            case 0:
                                {
                                    //keyValues.dictList = ;
                                    break;
                                }
                            case 1:
                                {
                                    keyValues.numElements = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    //keyValues.IsReadOnly = ser.ReadBoolValue(str[i]);
                                    break;
                                }
                            case 3:
                                {
                                    //keyValues.Keys = ;
                                    break;
                                }
                            case 4:
                                {
                                    //keyValues.Values = ;
                                    break;
                                }
                            case 5:
                                {
                                    //keyValues.Count = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 6:
                                {
                                    keyValues.currentDictIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 7:
                                {
                                    keyValues.currentIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 8:
                                {
                                    //keyValues.Current = ;
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                    }
                }
                return keyValues;
            
        }
    }
}
