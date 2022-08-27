// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
#if USE_AKKA

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Implementation of a sparse matrix which contains binary integer values only.
    /// </summary>
    /// <remarks>
    /// Author cogmission
    /// </remarks>
    public class SparseBinaryMatrix : AbstractSparseBinaryMatrix/*, ISerializable*/
    {
        /// <summary>
        /// Holds the matrix with connections between columns and inputs.
        /// </summary>
        public IDistributedArray backingArray;
        

        public SparseBinaryMatrix()
        {

        }

        /// <summary>
        /// Constructs a new <see cref="SparseBinaryMatrix"/> with the specified dimensions (defaults to row major ordering)
        /// </summary>
        /// <param name="dimensions">each indexed value is a dimension size</param>
        public SparseBinaryMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /// <summary>
        /// Constructs a new <see cref="SparseBinaryMatrix"/> with the specified dimensions, allowing the specification of column major ordering if desired. 
        /// (defaults to row major ordering)
        /// </summary>
        /// <param name="dimensions">each indexed value is a dimension size</param>
        /// <param name="useColumnMajorOrdering">if true, indicates column first iteration, otherwise row first iteration is the default (if false).</param>
        /// <param name="distArray"></param>
        public SparseBinaryMatrix(int[] dimensions, bool useColumnMajorOrdering, IDistributedArray distArray = null) : base(dimensions, useColumnMajorOrdering)
        {
            // We  create here a simple array on a single node.
            if (distArray == null)
                this.backingArray = new InMemoryArray(1, typeof(int), dimensions);
            else
                this.backingArray = distArray;
        }

        /**
         * Sets the value on specified call in array and automattically calculates number of '1' bits as TrueCount.
         * Called during mutation operations to simultaneously set the value
         * on the backing array dynamically.
         * @param val
         * @param coordinates
         */
        //private void back(int val, int[] coordinates)
        //{
        //    //update true counts
        //    this.backingArray.SetValue(val, coordinates);

        //    var aggVal = this.backingArray.AggregateArray(coordinates[0]);

        //    setTrueCount(coordinates[0], aggVal);
        //    // setTrueCount(coordinates[0], DistributedArrayHelpers.aggregateArray(((Object[])this.backingArray)[coordinates[0]]));
        //}

        /// <summary>
        /// Returns the slice specified by the passed in coordinates. The array is returned as an object, therefore it is the caller's
        /// responsibility to cast the array to the appropriate dimensions.
        /// </summary>
        /// <param name="coordinates">the coordinates which specify the returned array</param>
        /// <returns>the array specified. Throws <see cref="ArgumentException"/> if the specified coordinates address an actual value instead of the array holding it.</returns>
        /// <exception cref="ArgumentException"/>
        public override Object GetSlice(params int[] coordinates)
        {
            //Object slice = DistributedArrayHelpers.getValue(this.backingArray, coordinates);
            Object slice;
            if (coordinates.Length == 1)
                slice = GetRow<int>(this.backingArray, coordinates[0]);

            // DistributedArrayHelpers.GetRow<int>((int[,])this.backingArray, coordinates[0]);
            //else if (coordinates.Length == 1)
            //    slice = ((int[])this.backingArray)[coordinates[0]];
            else
                throw new ArgumentException();

            //Ensure return value is of type Array
            if (!slice.GetType().IsArray)
            {
                SliceError(coordinates);
            }

            return slice;
        }

        /// <summary>
        /// Gets the access to a row inside of multidimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        //public static T[] GetRow<T>(this T[,] array, int row)
        private static T[] GetRow<T>(IDistributedArray array, int row)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            for (int i = 0; i < cols; i++)
            {
                result[i] = (T)array[row, i];
            }

            return result;
        }

        /// <summary>
        /// Fills the specified results array with the result of the matrix vector multiplication.
        /// </summary>
        /// <param name="inputVector">the right side vector</param>
        /// <param name="results">the results array</param>
        public override void RightVecSumAtNZ(int[] inputVector, int[] results)
        {
            for (int i = 0; i < this.ModuleTopology.Dimensions[0]; i++)
            {
                int[] slice = (int[])(this.ModuleTopology.Dimensions.Length > 1 ? GetSlice(i) : backingArray);
                for (int j = 0; j < slice.Length; j++)
                {
                    results[i] += (inputVector[j] * slice[j]);
                }
            }
        }

        /// <summary>
        /// Fills the specified results array with the result of the 
        /// matrix vector multiplication.
        /// </summary>
        /// <param name="inputVector">the right side vector</param>
        /// <param name="results">the result array</param>
        /// <param name="stimulusThreshold"></param>
        public override void RightVecSumAtNZ(int[] inputVector, int[] results, double stimulusThreshold)
        {
            for (int colIndx = 0; colIndx < this.ModuleTopology.Dimensions[0]; colIndx++)
            {
                // Gets the synapse mapping between column-i with input vector.
                int[] slice = (int[])(this.ModuleTopology.Dimensions.Length > 1 ? GetSlice(colIndx) : backingArray);

                // Go through all connections (synapses) between column and input vector.
                for (int inpBit = 0; inpBit < slice.Length; inpBit++)
                {
                    //Debug.WriteLine($"Slice {i} - {String.Join("","", slice )}");

                    // Result (overlapp) is 1 if 
                    results[colIndx] += (inputVector[inpBit] * slice[inpBit]);
                    if (inpBit == slice.Length - 1)
                    {
                        // If the overlap (num of connected synapses to TRUE input) is less than stimulusThreshold then we set result on 0.
                        // If the overlap (num of connected synapses to TRUE input) is greather than stimulusThreshold then result remains as calculated.
                        // This ensures that only overlaps are calculated, which are over the stimulusThreshold. All less than stimulusThreshold are set on 0.
                        results[colIndx] -= results[colIndx] < stimulusThreshold ? results[colIndx] : 0;
                    }
                }
            }
        }


        //public AbstractSparseBinaryMatrix set(int index, Object value)
        //{
        //    set(index, ((Integer)value).Value);
        //    return this;
        //}

        public override AbstractFlatMatrix<int> set(int index, int value)
        {
            set(index, value);
            return (AbstractFlatMatrix<int>)this;
        }

        /// <summary>
        /// Sets the value to be indexed at the index
        /// computed from the specified coordinates.
        /// </summary>
        /// <param name="value">the object to be indexed.</param>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <returns></returns>
        public override AbstractSparseBinaryMatrix set(int value, int[] coordinates)
        {
            //back(value, coordinates);

            //update true counts
            this.backingArray.SetValue(value, coordinates);

            var aggVal = this.backingArray.AggregateArray(coordinates[0]);

            SetTrueCount(coordinates[0], aggVal);

            return this;
        }

        // TODO Override?s
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

        /// <summary>
        /// Clears the true counts prior to a cycle where they're being set
        /// </summary>
        /// <param name="row"></param>
        public override void ClearStatistics(int row)
        {
            if (backingArray.Rank != 2)
                throw new InvalidOperationException("Currently supported 2D arrays only");

            backingArray.SetRowValuesTo(row, 0);

            this.SetTrueCount(row, 0);
        }




        //  @Override
        public override AbstractSparseBinaryMatrix setForTest(int index, int value)
        {
            this.backingArray.SetValue(value, ComputeCoordinates(GetNumDimensions(), GetDimensionMultiples(), ModuleTopology.IsMajorOrdering, index));

            //DistributedArrayHelpers.setValue(this.backingArray, value,
            //    ComputeCoordinates(getNumDimensions(), getDimensionMultiples(), ModuleTopology.IsMajorOrdering, index));
            return this;
        }


        //public override Integer Get(int index)
        //{
        //    return (Integer)get(index);
        //}


        // @Override
        public override int GetColumn(int index)
        {
            int[] coordinates = ComputeCoordinates(GetNumDimensions(), GetDimensionMultiples(), this.ModuleTopology.IsMajorOrdering, index);
            if (coordinates.Length == 1)
            {
                return (Int32)backingArray.GetValue(index);
            }
            else
                return (Int32)backingArray.GetValue(coordinates);
        }

        public override AbstractFlatMatrix<int> set(List<KeyPair> updatingValues)
        {
            throw new NotImplementedException();
        }

        public override ICollection<KeyPair> GetObjects(int[] indexes)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            var matrix = obj as SparseBinaryMatrix;
            if (matrix == null)
                return false;
            return this.Equals(matrix);
        }
        public bool Equals(SparseBinaryMatrix obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (backingArray == null)
            {
                if (obj.backingArray != null)
                    return false;
            }
            else if (!backingArray.Equals(obj.backingArray))
                return false;

            if (ModuleTopology == null)
            {
                if (obj.ModuleTopology != null)
                    return false;
            }
            else if (!ModuleTopology.Equals(obj.ModuleTopology))
                return false;
            if (!this.trueCounts.SequenceEqual(obj.trueCounts))
                return false;

            return true;
        }
        #region Serialization
        public override void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(SparseBinaryMatrix), writer);

            ser.SerializeValue(this.trueCounts, writer);
            
            if(this.ModuleTopology != null)
            { this.ModuleTopology.Serialize(writer); }
            
            if (this.backingArray != null)
            { this.backingArray.Serialize(writer); }

            ser.SerializeEnd(nameof(SparseBinaryMatrix), writer);
        }
        public static SparseBinaryMatrix Deserialize(StreamReader sr)
        {
            SparseBinaryMatrix sparse = new SparseBinaryMatrix();
            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(SparseBinaryMatrix)))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(InMemoryArray)))
                {
                    sparse.backingArray = InMemoryArray.Deserialize(sr);
                }
                else if (data == ser.ReadBegin(nameof(HtmModuleTopology)))
                {
                    sparse.ModuleTopology = HtmModuleTopology.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(SparseBinaryMatrix)))
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
                                    sparse.trueCounts = ser.ReadArrayInt(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return sparse;
        }

        //public new void Serialize(object obj, string name, StreamWriter sw)
        //{
        //    HtmSerializer2.SerializeObject(obj, name, sw);
        //}

        //public static new object Deserialize(StreamReader sr, string name)
        //{
        //    return HtmSerializer2.DeserializeObject<SparseBinaryMatrix>(sr, name);
        //}

        #endregion
    }
}
#endif