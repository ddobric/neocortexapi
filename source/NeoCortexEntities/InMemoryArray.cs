// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using NeoCortexApi.Entities;

namespace NeoCortexApi
{
    public class InMemoryArray : IDistributedArray, ISerializable
    {
        public int[] dimensions;

        public Array backingArray;

        public int numOfNodes;

        public int Rank => backingArray.Rank;

        long IDistributedArray.Count => backingArray.Length;

        /// <summary>
        /// Gets dimensions of distributed array.
        /// </summary>
        public int[] Dimensions
        {
            get
            {
                int[] dims = new int[Rank];
                for (int i = 0; i < dims.Length; i++)
                {
                    dims[i] = backingArray.GetUpperBound(i);
                }

                return dims;
            }
        }

        public object this[int row, int col]
        {
            get
            {
                return backingArray.GetValue(row, col);
            }
            set
            {
                backingArray.SetValue(value, row, col);
            }
        }


        public object this[int index]
        {
            get => backingArray.GetValue(index);

            set => backingArray.SetValue(value, index);
        }

        public InMemoryArray(int numOfNodes, Type type, int[] dimensions)
        {
            this.numOfNodes = numOfNodes;
            backingArray = Array.CreateInstance(typeof(int), dimensions);
            this.dimensions = dimensions;
        }

        public InMemoryArray()
        {
        }

        //public static IDistributedArray CreateInstance(Type type, int[] dimensions)
        //{
        //    var arr = new InMemoryArray(1, type, dimensions);
        //    arr.backingArray = Array.CreateInstance(typeof(int), dimensions);
        //    arr.dimensions = dimensions;
        //    return arr;
        //}

        public double Max()
        {
            throw new NotImplementedException();
            //double max = Double.MinValue;
            //for (int i = 0; i < array.Length; i++)
            //{
            //    if (array[i] > max)
            //    {
            //        max = array[i];
            //    }
            //}
            //return max;
        }


        //public int Add(object value)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Clear()
        //{
        //    throw new NotImplementedException();
        //}

        //public bool Contains(object value)
        //{
        //    throw new NotImplementedException();
        //}

        //public void CopyTo(Array array, int index)
        //{
        //    throw new NotImplementedException();
        //}

        public IEnumerator GetEnumerator()
        {
            return backingArray.GetEnumerator();
            //throw new NotImplementedException();
        }

        //public int IndexOf(object value)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Insert(int index, object value)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Remove(object value)
        //{
        //    throw new NotImplementedException();
        //}



        //public void RemoveAt(int index)
        //{
        //    throw new NotImplementedException();
        //}

        public int AggregateArray(int row)
        {
            int cols = backingArray.GetUpperBound(1) + 1;

            if (backingArray is int[,])
            {
                int[,] arr = (int[,])backingArray;

                int sum = 0;
                for (int i = 0; i < cols; i++)
                {
                    sum += arr[row, i];
                }

                return sum;
            }
            else
                throw new InvalidOperationException("Unsupported array type. Array MUST be of type int[,]!");
        }

        public int AggregateArraySlow(int row)
        {
            int cols = backingArray.GetUpperBound(1) + 1;

            int sum = 0;
            for (int i = 0; i < cols; i++)
            {
                sum += (int)backingArray.GetValue(row, i);
            }

            return sum;
        }

        public object GetValue(int index)
        {
            return backingArray.GetValue(index);
        }

        public object GetValue(int[] indexes)
        {
            return backingArray.GetValue(indexes);
        }

        public void SetValue(int value, int[] indexes)
        {
            backingArray.SetValue(value, indexes);
        }

        public int GetUpperBound(int v)
        {
            return backingArray.GetUpperBound(v);
        }

        public void SetRowValuesTo(int rowIndex, object newVal)
        {
            var cols = backingArray.GetLength(1);
            for (int i = 0; i < cols; i++)
            {
                backingArray.SetValue(0, rowIndex, i);
            }
        }
        public override bool Equals(object obj)
        {
            var array = obj as InMemoryArray;
            if (array == null)
                return false;
            return this.Equals(array);
        }

        public bool Equals(InMemoryArray obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;
            //if (!this.dimensions.SequenceEqual(obj.dimensions))
            //    return false;
            else if (!ArrayEquals(backingArray, obj.backingArray))
                return false;
            else if (numOfNodes != obj.numOfNodes)
                return false;
            else if (Rank != obj.Rank)
                return false;
            else if (!Dimensions.SequenceEqual(obj.Dimensions))
                return false;

            return true;
        }

        private bool ArrayEquals(Array array1, Array array2)
        {
            if (array1 == null)
                return array2 == null;
            if (array2 == null)
                return false;

            if (array1.Rank != array2.Rank)
                return false;
            for (int r = 0; r < array1.Rank; r++)
            {
                if (array1.GetLength(r) != array2.GetLength(r))
                    return false;
            }

            var arrayType = array1.GetType();
            var elementType = arrayType.GetElementType();

            var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(elementType);
            if (castMethod == null)
            {
                throw new Exception("No cast method. This should not happen!");
            }

            var list1 = castMethod.Invoke(null, new object[] { array1 });
            var list2 = castMethod.Invoke(null, new object[] { array2 });


            var sequenceEqualMethod = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == nameof(Enumerable.SequenceEqual) && m.GetParameters().Length == 2)?.MakeGenericMethod(elementType);

            var isSequenceEqual = (bool)sequenceEqualMethod.Invoke(null, new object[] { list1, list2 });

            if (!isSequenceEqual)
                return false;

            return true;
        }
        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer ser = new HtmSerializer();

            ser.SerializeBegin(nameof(InMemoryArray), writer);

            ser.SerializeValue(backingArray, writer);
            ser.SerializeValue(dimensions, writer);
            ser.SerializeValue(numOfNodes, writer);

            ser.SerializeEnd(nameof(InMemoryArray), writer);
        }
        public static InMemoryArray Deserialize(StreamReader sr)
        {
            InMemoryArray array = new InMemoryArray();

            HtmSerializer ser = new HtmSerializer();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == string.Empty || data == ser.ReadBegin(nameof(InMemoryArray)))
                {
                    continue;
                }
                else if (data == ser.ReadEnd(nameof(InMemoryArray)))
                {
                    break;
                }
                else
                {
                    string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    array.dimensions = ser.ReadArrayInt(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    array.numOfNodes = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return array;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var ignoreMembers = new List<string>
            {
                "Item",
                //nameof(Dimensions),
                //nameof(IDistributedArray.Count),
                //nameof(Rank)
            };
            HtmSerializer.SerializeObject(obj, name, sw, ignoreMembers);
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            var ignoreMembers = new List<string> { /*"Item"*/ };
            return HtmSerializer.DeserializeObject<T>(sr, name, ignoreMembers);
        }
        #endregion
    }
}
