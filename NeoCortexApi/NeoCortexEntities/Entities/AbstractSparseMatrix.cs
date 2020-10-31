// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    // TODO naming convention with override method
    /// <summary>
    /// Allows storage of array data in sparse form, meaning that the indexes
    /// of the data stored are maintained while empty indexes are not. This allows
    /// savings in memory and computational efficiency because iterative algorithms
    /// need only query indexes containing valid data. The dimensions of matrix defined
    /// at construction time and immutable - matrix fixed size data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>    
    /// <remarks>
    /// Authors of the JAVA implementation: David Ray, Jose Luis Martin
    /// </remarks>
    //[Serializable]
    public abstract class AbstractSparseMatrix<T> : AbstractFlatMatrix<T>, ISparseMatrix<T>
    {

        public AbstractSparseMatrix()
        {

        }

        /// <summary>
        /// Constructs a new <see cref="AbstractSparseMatrix{T}"/> with the specified dimensions (defaults to row major ordering)
        /// </summary>
        /// <param name="dimensions">each indexed value is a dimension size</param>
        public AbstractSparseMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /// <summary>
        /// Constructs a new {@code AbstractSparseMatrix} with the specified dimensions, allowing the specification of column major ordering if desired. 
        /// (defaults to row major ordering)
        /// </summary>
        /// <param name="dimensions">each indexed value is a dimension size</param>
        /// <param name="useColumnMajorOrdering">if true, indicates column first iteration, otherwise row first iteration is the default (if false).</param>
        public AbstractSparseMatrix(int[] dimensions, bool useColumnMajorOrdering) : base(dimensions, useColumnMajorOrdering)
        {

        }

        // protected <S extends AbstractSparseMatrix<T>> S set(int index, int value) { return null; }

        // TODO naming convention with override method
        /// <summary>
        /// Sets the object to occupy the specified index.
        /// </summary>
        /// <param name="index">the index the object will occupy</param>
        /// <param name="value">the value to be indexed.</param>
        /// <returns>this <see cref="AbstractSparseMatrix{T}"/> implementation</returns>
        public override AbstractFlatMatrix<T> set(int index, T value)
        {
            return null;
        }
        //        public override AbstractFlatMatrix<T> set(int index, int value)
        //#pragma warning restore IDE1006 // Naming Styles
        //        { return null; }

        /// <summary>
        /// Sets the object to occupy the specified index.
        /// </summary>
        /// <param name="index">the index the object will occupy</param>
        /// <param name="value">the value to be indexed.</param>
        /// <returns></returns>
        protected virtual AbstractSparseMatrix<T> Set(int index, double value)
        { return null; }

        /// <summary>
        /// Sets the specified object to be indexed at the index computed from the specified coordinates.
        /// </summary>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns>this <see cref="AbstractSparseMatrix{T}"/> implementation</returns>
        public AbstractSparseMatrix<T> Set(int[] coordinates, T obj) { return null; }

        // TODO naming convention with override method
        /// <summary>
        /// Sets the specified object to be indexed at the index computed from the specified coordinates.
        /// </summary>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns>this <see cref="AbstractSparseMatrix{T}"/> implementation</returns>
        protected virtual AbstractSparseMatrix<T> set(int value, int[] coordinates) { return null; }

        /// <summary>
        /// Sets the specified object to be indexed at the index computed from the specified coordinates.
        /// </summary>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns>this <see cref="AbstractSparseMatrix{T}"/> implementation</returns>
        protected virtual AbstractSparseMatrix<T> Set(double value, int[] coordinates) { return null; }

        /// <summary>
        /// Returns the T at the specified index.
        /// </summary>
        /// <param name="index">the index of the T to return</param>
        /// <returns>the T at the specified index.</returns>
        public virtual T getObject(int index)
        {
            return default(T);
        }

        public abstract ICollection<KeyPair> GetObjects(int[] indexes);


        /// <summary>
        /// Returns the T at the specified index.
        /// </summary>
        /// <param name="index">the index of the T to return</param>
        /// <returns>the T at the specified index.</returns>
        protected int GetIntValue(int index) { return -1; }

        /// <summary>
        /// Returns the T at the specified index.
        /// </summary>
        /// <param name="index">the index of the T to return</param>
        /// <returns>the T at the specified index.</returns>
        protected double GetDoubleValue(int index) { return -1.0; }

        /// <summary>
        /// Returns the T at the index computed from the specified coordinates
        /// </summary>
        /// <param name="coordinates">the coordinates from which to retrieve the indexed object</param>
        /// <returns>the indexed object</returns>
        public override T get(int[] coordinates) { return default(T); }

        /// <summary>
        /// Returns the int value at the index computed from the specified coordinates
        /// </summary>
        /// <param name="coordinates">the coordinates from which to retrieve the indexed object</param>
        /// <returns>the indexed object</returns>
        protected int GetIntValue(int[] coordinates) { return -1; }

        /// <summary>
        /// Returns the double value at the index computed from the specified coordinates
        /// </summary>
        /// <param name="coordinates">the coordinates from which to retrieve the indexed object</param>
        /// <returns>the indexed object</returns>
        protected double GetDoubleValue(int[] coordinates) { return -1.0; }

        // @Override
        public override int[] GetSparseIndices()
        {
            return null;
        }

        //  @Override
        public override int[] Get1DIndexes()
        {
            List<int> results = new List<int>(GetMaxIndex() + 1);
            Visit(GetDimensions(), 0, new int[GetNumDimensions()], results);
            return results.ToArray();
        }

        /// <summary>
        /// Recursively loops through the matrix dimensions to fill the results array with flattened computed array indexes.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="currentDimension"></param>
        /// <param name="p"></param>
        /// <param name="results"></param>
        private void Visit(int[] bounds, int currentDimension, int[] p, List<int> results)
        {
            for (int i = 0; i < bounds[currentDimension]; i++)
            {
                p[currentDimension] = i;
                if (currentDimension == p.Length - 1)
                {
                    results.Add(ComputeIndex(p));
                }
                else Visit(bounds, currentDimension + 1, p, results);
            }
        }


        //public override T[] asDense(ITypeFactory<T> factory)
        //{
        //    throw NotImplementedException();

        //    int[] dimensions = getDimensions();
        //    T[] retVal = (T[])Array.CreateInstance(typeof(T), dimensions);

        //    fill(factory, 0, dimensions, dimensions[0], (object[])(object)retVal);

        //    return retVal;
        //}

        /// <summary>
        /// Uses reflection to create and fill a dynamically created multidimensional array.
        /// </summary>
        /// <param name="f">the <see cref="ITypeFactory{T}"/></param>
        /// <param name="dimensionIndex">the current index into <em>this class's</em> configured dimensions array <em>*NOT*</em> the dimensions used as this method's argument</param>
        /// <param name="dimensions">the array specifying remaining dimensions to create</param>
        /// <param name="count">the current dimensional size</param>
        /// <param name="arr">the array to fill</param>
        //@SuppressWarnings("unchecked")
        protected Object[] fill(ITypeFactory<T> f, int dimensionIndex, int[] dimensions, int count, Object[] arr)
        {
            if (dimensions.Length == 1)
            {
                for (int i = 0; i < count; i++)
                {
                    arr[i] = f.make(GetDimensions());
                    // arr[i] = new 
                }
                return arr;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int[] inner = CopyInnerArray(dimensions);
                    //T[] r = (T[])Array.newInstance(f.typeClass(), inner);
                    T[] r = (T[])Array.CreateInstance(typeof(T), inner);
                    arr[i] = fill(f, dimensionIndex + 1, inner, GetDimensions()[dimensionIndex + 1], (object[])(object)r);
                }
                return arr;
            }
        }

    }
}
