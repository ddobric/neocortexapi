// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeoCortexApi.Entities
{
    //TODO see type object
    // [Serializable]
    public abstract class AbstractSparseBinaryMatrix : AbstractSparseMatrix<int>, IEquatable<object>
    {
        /** keep it simple */
        private static readonly long serialVersionUID = 1L;

        public int[] trueCounts;

        public AbstractSparseBinaryMatrix()
        {

        }
        
        /// <summary>
        /// Constructs a new <see cref="AbstractSparseBinaryMatrix"/> with the specified dimensions (defaults to row major ordering)
        /// </summary>
        /// <param name="dimensions">each indexed value is a dimension size</param>
        public AbstractSparseBinaryMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /// <summary>
        /// Constructs a new <see cref="AbstractSparseBinaryMatrix"/> with the specified dimensions, allowing the specification of 
        /// column major ordering if desired. (defaults to row major ordering) 
        /// </summary>
        /// <param name="dimensions">each indexed value is a dimension size</param>
        /// <param name="useColumnMajorOrdering">if true, indicates column first iteration, otherwise row first iteration is the default (if false).</param>
        public AbstractSparseBinaryMatrix(int[] dimensions, bool useColumnMajorOrdering) : base(dimensions, useColumnMajorOrdering)
        {

            this.trueCounts = new int[dimensions[0]];
        }

        /// <summary>
        /// Returns the slice specified by the passed in coordinates. The array is returned as an object, therefore it is the caller's
        /// responsibility to cast the array to the appropriate dimensions.
        /// </summary>
        /// <param name="coordinates">the coordinates which specify the returned array</param>
        /// <returns>the array specified. Throw <see cref="ArgumentException"/> if the specified coordinates address an actual value instead of the array holding it.</returns>
        /// <exception cref="ArgumentException">Throws if the specified coordinates address an actual value instead of the array holding it.</exception>
        public abstract object GetSlice(params int[] coordinates);

        /// <summary>
        /// Launch getSlice error, to share it with subclass <see cref="GetSlice(int[])"/> implementations.
        /// </summary>
        /// <param name="coordinates"></param>
        protected void SliceError(int[] coordinates)
        {
            throw new ArgumentException(
                "This method only returns the array holding the specified maximum index: " +
                        "dimensions.ToString()");
        }

        /// <summary>
        /// Calculate the flat indexes of a slice
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns>the flat indexes array</returns>
        protected int[] GetSliceIndexes(int[] coordinates)
        {
            int[] dimensions = GetDimensions();
            // check for valid coordinates
            if (coordinates.Length >= dimensions.Length)
            {
                SliceError(coordinates);
            }

            int sliceDimensionsLength = dimensions.Length - coordinates.Length;
            int[] sliceDimensions = (int[])Array.CreateInstance(typeof(int), sliceDimensionsLength);

            for (int i = coordinates.Length; i < dimensions.Length; i++)
            {
                sliceDimensions[i - coordinates.Length] = dimensions[i];
            }

            int[] elementCoordinates = new int[coordinates.Length + 1];
            Array.Copy(coordinates, elementCoordinates, coordinates.Length + 1);

            int sliceSize = 1;
            foreach (var item in sliceDimensions)
            {
                sliceSize *= item;
            }

            //int sliceSize = Arrays.stream(sliceDimensions).reduce((n, i)->n * i).getAsInt();

            int[] slice = new int[sliceSize];

            if (coordinates.Length + 1 == dimensions.Length)
            {
                // last slice 
                for (int i = 0; i < dimensions[coordinates.Length]; i++)
                {
                    elementCoordinates[coordinates.Length] = i;
                    slice[i] = ComputeIndex(elementCoordinates);
                    // Array.set(slice, i, computeIndex(elementCoordinates));
                }
            }
            else
            {
                for (int i = 0; i < dimensions[sliceDimensionsLength]; i++)
                {
                    elementCoordinates[coordinates.Length] = i;
                    int[] indexes = GetSliceIndexes(elementCoordinates);
                    Array.Copy(indexes, 0, slice, i * indexes.Length, indexes.Length);
                    //System.arraycopy(indexes, 0, slice, i* indexes.length, indexes.length);
                }
            }

            return slice;
        }

        /// <summary>
        /// Fills the specified results array with the result of the matrix vector multiplication.
        /// </summary>
        /// <param name="inputVector">the right side vector</param>
        /// <param name="results">the results array</param>
        public abstract void RightVecSumAtNZ(int[] inputVector, int[] results);

        /// <summary>
        /// Fills the specified results array with the result of the matrix vector multiplication.
        /// </summary>
        /// <param name="inputVector">the right side vector</param>
        /// <param name="results">the results array</param>
        /// <param name="stimulusThreshold"></param>
        public abstract void RightVecSumAtNZ(int[] inputVector, int[] results, double stimulusThreshold);

        /// <summary>
        /// Sets the value at the specified index.
        /// </summary>
        /// <param name="index">the index the object will occupy</param>
        /// <param name="value">the value to be indexed.</param>
        /// <returns></returns>
        public override AbstractFlatMatrix<int> set(int index, int value)
        {
            int[] coordinates = ComputeCoordinates(GetNumDimensions(), GetDimensionMultiples(), this.ModuleTopology.IsMajorOrdering, index);
            return set(value, coordinates);
        }

        // TODO naming convention with override method
        /// <summary>
        /// Sets the value to be indexed at the index computed from the specified coordinates.
        /// </summary>
        /// <param name="value">the value to be indexed</param>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <returns></returns>
        public abstract AbstractSparseBinaryMatrix set(int value, params int[] coordinates);

        // TODO naming convention with override method: make method as virtual??
        /// <summary>
        /// Sets the specified values at the specified indexes.
        /// </summary>
        /// <param name="indexes">indexes of the values to be set</param>
        /// <param name="values">the values to be indexed.</param>
        /// <returns>this <see cref="AbstractSparseBinaryMatrix"/> implementation</returns>
        public AbstractSparseBinaryMatrix set(int[] indexes, int[] values)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                set(indexes[i], values[i]);
            }
            return this;
        }

        // TODO naming convention with override method
        // TODO Integer class ??
        public Integer get(int[] coordinates)
        {
            return GetColumn(ComputeIndex(coordinates));
        }



#pragma warning disable IDE1006 // Naming Styles
        // public abstract Integer get(int index);
#pragma warning restore IDE1006 // Naming Styles

        // TODO naming convention with override method
        /// <summary>
        /// Sets the value at the specified index skipping the automatic truth statistic tallying of the real method.
        /// </summary>
        /// <param name="index">the index the object will occupy</param>
        /// <param name="value">the value to be indexed.</param>
        /// <returns></returns>
        public abstract AbstractSparseBinaryMatrix setForTest(int index, int value);

        // TODO naming convention with override method
        /// <summary>
        /// Call This for TEST METHODS ONLY.<br></br>
        /// Sets the specified values at the specified indexes.
        /// </summary>
        /// <param name="indexes">indexes of the values to be set</param>
        /// <param name="values">the values to be indexed.</param>
        /// <param name="isTest"></param>
        /// <returns>this <see cref="AbstractSparseBinaryMatrix"/> implementation</returns>
        public AbstractSparseBinaryMatrix set(int[] indexes, int[] values, bool isTest)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                if (isTest) setForTest(indexes[i], values[i]);
                else set(indexes[i], values[i]);
            }
            return this;
        }

        /// <summary>
        /// Returns the count of 1's set on the specified row.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetTrueCount(int index)
        {
            return trueCounts[index];
        }

        /// <summary>
        /// Sets the count of 1's on the specified row.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void SetTrueCount(int index, int count)
        {
            this.trueCounts[index] = count;
        }

        /// <summary>
        /// Get the true counts for all outer indexes.
        /// </summary>
        /// <returns></returns>
        public int[] GetTrueCounts()
        {
            return trueCounts;
        }

        /// <summary>
        /// Clears the true counts prior to a cycle where they're being set
        /// </summary>
        /// <param name="row"></param>
        public virtual void ClearStatistics(int row)
        {
            trueCounts[row] = 0;

            foreach (int index in GetSliceIndexes(new int[] { row }))
            {
                set(index, 0);
            }
        }

        // TODO naming convention with override method
        /// <summary>
        /// Returns the int value at the index computed from the specified coordinates. For example value {7, 21} is TRU if the column 7 is connected to input bit 21.
        /// </summary>
        /// <param name="coordinates">the coordinates from which to retrieve the indexed object</param>
        /// <returns>the indexed object</returns>
        public new int GetIntValue(params int[] coordinates)
        {
            return GetColumn(ComputeIndex(coordinates));
        }

        // TODO naming convention with override method
        /// <summary>
        /// Returns the T at the specified index.
        /// </summary>
        /// <param name="index">the index of the T to return</param>
        /// <returns>the T at the specified index.</returns>
        public new int GetIntValue(int index)
        {
            return GetColumn(index);
        }

        // TODO naming convention with override method
        /// <summary>
        /// Returns a sorted array of occupied indexes.
        /// </summary>
        /// <returns>a sorted array of occupied indexes.</returns>
        //@Override
        public override int[] GetSparseIndices()
        {
            List<int> indexes = new List<int>();
            //TIntList indexes = new TIntArrayList();
            for (int i = 0; i <= GetMaxIndex(); i++)
            {
                if (GetColumn(i) > 0)
                {
                    indexes.Add(i);
                }
            }

            return indexes.ToArray();
        }

        /// <summary>
        /// This <see cref="SparseBinaryMatrix"/> will contain the operation of or-ing the inputMatrix with the contents of this matrix; returning this matrix as the result. 
        /// </summary>
        /// <param name="inputMatrix">the matrix containing the "on" bits to or</param>
        /// <returns>this matrix</returns>
        public AbstractSparseBinaryMatrix Or(AbstractSparseBinaryMatrix inputMatrix)
        {
            int[] mask = inputMatrix.GetSparseIndices();
            int[] ones = new int[mask.Length];
            ArrayUtils.Fill(ones, 1);
            return set(mask, ones);
        }

        /// <summary>
        /// This <see cref="SparseBinaryMatrix"/> will contain the operation of or-ing the sparse list with the contents of this matrix; returning this matrix as the result.
        /// </summary>
        /// <param name="onBitIndexes">the matrix containing the "on" bits to or</param>
        /// <returns>this matrix</returns>
        public AbstractSparseBinaryMatrix Or(List<int> onBitIndexes)
        {
            int[] ones = new int[onBitIndexes.Count];
            Utility.ArrayUtils.Fill(ones, 1);
            return set(onBitIndexes.ToArray(), ones);
        }

        /// <summary>
        /// This <see cref="SparseBinaryMatrix"/> will contain the operation of or-ing the sparse array with the contents of this matrix; returning this matrix as the result.
        /// </summary>
        /// <param name="onBitIndexes"></param>
        /// <returns></returns>
        public AbstractSparseBinaryMatrix Or(int[] onBitIndexes)
        {
            int[] ones = new int[onBitIndexes.Length];
            Utility.ArrayUtils.Fill(ones, 1);
            return set(onBitIndexes, ones);
        }

        protected HashSet<int> GetSparseSet()
        {
            return new HashSet<int>(GetSparseIndices());
        }

        /// <summary>
        /// Returns true if the on bits of the specified matrix are matched by the on bits of this matrix. It is allowed that this matrix have more on bits than the specified matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public bool All(AbstractSparseBinaryMatrix matrix)
        {
            var sparseSet = GetSparseSet();
            bool hasAll = matrix.GetSparseIndices().All(itm2 => sparseSet.Contains(itm2));
            return hasAll;
            //return getSparseSet().Contains(
            //    containsAll(matrix.getSparseIndices());
        }

        /// <summary>
        /// Returns true if the on bits of the specified matrix are matched by the on bits of this matrix. It is allowed that this matrix have more on bits than the specified matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public bool All(List<int> onBits)
        {
            var sparseSet = GetSparseSet();
            bool hasAll = onBits.All(itm2 => sparseSet.Contains(itm2));
            return hasAll;
            //return getSparseSet().containsAll(onBits);
        }

        /// <summary>
        /// Returns true if the on bits of the specified matrix are matched by the on bits of this matrix. It is allowed that this matrix have more on bits than the specified matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public bool All(int[] onBits)
        {
            var sparseSet = GetSparseSet();
            bool hasAll = onBits.All(itm2 => sparseSet.Contains(itm2));
            return hasAll;
            //return getSparseSet().containsAll(onBits);
        }

        /// <summary>
        /// Returns true if any of the on bits of the specified matrix are matched by the on bits of this matrix. It is allowed that this matrix have more on bits than the specified matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public bool Any(AbstractSparseBinaryMatrix matrix)
        {
            var keySet = GetSparseSet();

            foreach (int i in matrix.GetSparseIndices())
            {
                if (keySet.Contains(i)) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if any of the on bits of the specified collection are matched by the on bits of this matrix. It is allowed that this matrix have more on bits than the specified matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public bool Any(HashSet<int> onBits)
        {
            var keySet = GetSparseSet();

            foreach (var i in onBits)
            {
                if (keySet.Contains(i)) return true;
            }
            //for (TIntIterator i = onBits.iterator(); i.hasNext();)
            //{
            //    if (keySet.contains(i.next())) return true;
            //}
            return false;
        }

        /// <summary>
        /// Returns true if any of the on bits of the specified matrix are matched by the on bits of this matrix. It is allowed that this matrix have more on bits than the specified matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public bool Any(int[] onBits)
        {
            var keySet = GetSparseSet();

            foreach (int i in onBits)
            {
                if (keySet.Contains(i)) return true;
            }
            return false;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */


        public override int GetHashCode()
        {
            const int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + trueCounts.GetHashCode();
            return result;
        }



        public bool Equals(AbstractSparseBinaryMatrix obj)
        {
            if (this == obj)
                return true;
            if (!base.Equals(obj))
                return false;
            if ((obj.GetType() != this.GetType()))
                return false;
            
            AbstractSparseBinaryMatrix other = (AbstractSparseBinaryMatrix)obj;
            if (other.trueCounts != null && trueCounts != null)
            {

                if (!other.trueCounts.SequenceEqual(trueCounts))
                    return false;
            }
            
            if (ModuleTopology == null)
            {
                if (obj.ModuleTopology != null)
                    return false;
            }
            else if (!ModuleTopology.Equals(obj.ModuleTopology))
                return false;

            return true;
        }
        #region Serialization
        public override void Serialize(StreamWriter writer)
        {
            throw new NotImplementedException();
            
        }
        #endregion
    }
}
