using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    public class InMemoryDistributedArray : IDistributedArray
    {
        private int numOfNodes;

        public object this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public object this[int row, int col] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public InMemoryDistributedArray(int numOfNodes)
        {
            this.numOfNodes = numOfNodes;
        }

        public IDistributedArray CreateInstance(Type type, int[] dimensions)
        {
            // this.backingArray = InMemoryDistributedArray.CreateInstance(typeof(int), dimensions);
            throw new NotImplementedException();
        }

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


        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

       

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public int AggregateArray()
        {
            throw new NotImplementedException();
        }

        public void SetValue(int value, int[] indexes)
        {
            throw new NotImplementedException();
        }

        public int GetUpperBound(int v)
        {
            throw new NotImplementedException();
        }

     
    }
}
