// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;

namespace NeoCortexApi.Entities
{

    /// <summary>
    /// Provides common generic independent calculation functions.
    /// </summary>
    public class AbstractFlatMatrix /*: ISerializable*/
    {
        public AbstractFlatMatrix()
        {

        }

        /// <summary>
        /// Reverses the array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int[] Reverse(int[] input)
        {
            int[] retVal = new int[input.Length];
            for (int i = input.Length - 1, j = 0; i >= 0; i--, j++)
            {
                retVal[j] = input[i];
            }
            return retVal;
        }

        /// <summary>
        /// Computes multidimensional coordinats from flat index.
        /// </summary>
        /// <param name="numDims"></param>
        /// <param name="dimensionMultiples"></param>
        /// <param name="isColumnMajor"></param>
        /// <param name="synapseFlatIndex">Flat intdex of the synapse.</param>
        /// <returns></returns>
        public static int[] ComputeCoordinates(int numDims, int[] dimensionMultiples, bool isColumnMajor, int synapseFlatIndex)
        {
            int[] returnVal = new int[numDims];
            int @base = synapseFlatIndex;
            for (int i = 0; i < dimensionMultiples.Length; i++)
            {
                int quotient = @base / dimensionMultiples[i];
                @base %= dimensionMultiples[i];
                returnVal[i] = quotient;
            }
            return isColumnMajor ? Reverse(returnVal) : returnVal;
        }

        public static int ComputeIndex(int[] coordinates, int[] dimensions, int numDimensions, int[] dimensionMultiples, bool isColumnMajor, bool doCheck)
        {
            if (doCheck) CheckDims(dimensions, numDimensions, coordinates);

            int[] localMults = isColumnMajor ? Reverse(dimensionMultiples) : dimensionMultiples;
            int @base = 0;
            for (int i = 0; i < coordinates.Length; i++)
            {
                @base += (localMults[i] * coordinates[i]);
            }
            return @base;
        }

  
        /// <summary>
        /// Checks the indexes specified to see whether they are within the configured bounds and size parameters of this array configuration.
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="numDimensions"></param>
        /// <param name="coordinates"></param>
        public static void CheckDims(int[] dimensions, int numDimensions, int[] coordinates)
        {
            if (coordinates.Length != numDimensions)
            {
                throw new ArgumentException("Specified coordinates exceed the configured array dimensions " +
                        "input dimensions: " + coordinates.Length + " > number of configured dimensions: " + numDimensions);
            }
            for (int i = 0; i < coordinates.Length - 1; i++)
            {
                if (coordinates[i] >= dimensions[i])
                {
                    throw new ArgumentException("Specified coordinates exceed the configured array dimensions " +
                            ArrayToString(coordinates) + " > " + ArrayToString(dimensions));
                }
            }
        }

        /// <summary>
        /// Prints the specified array to a returned String.
        /// </summary>
        /// <param name="arr">the array to be converted to string</param>
        /// <returns>the array in string form suitable for display.</returns>
        public static string ArrayToString(int[] arr)
        {
            string res = string.Join(",", arr);
            return res;
        }

        /// <summary>
        /// Initializes internal helper array which is used for multidimensional index computation.
        /// </summary>
        /// <param name="dimensions">matrix dimensions</param>
        /// <returns>array for use in coordinates to flat index computation.</returns>
        public static int[] InitDimensionMultiples(int[] dimensions)
        {
            int holder = 1;
            int len = dimensions.Length;
            int[] dimensionMultiples = new int[dimensions.Length];

            for (int i = 0; i < len; i++)
            {
                holder *= (i == 0 ? 1 : dimensions[len - i]);
                dimensionMultiples[len - 1 - i] = holder;
            }
            return dimensionMultiples;
        }

        //public void Serialize(object obj, string name, StreamWriter sw)
        //{
        //    HtmSerializer2.SerializeObject(obj, name, sw);
        //}

        //public static object Deserialize(StreamReader sr, string name)
        //{
        //    return HtmSerializer2.DeserializeObject<AbstractFlatMatrix>(sr, name);
        //}
    }


    /// <summary>
    /// Imlements flat calculations on matrix.
    /// Originally authored by: David Ray and  Jose Luis Martin.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractFlatMatrix<T> : AbstractFlatMatrix, IFlatMatrix<T>
    {
        public AbstractFlatMatrix()
        {

        }

        /// <summary>
        /// Gets/Sets the topology of the HTM model.
        /// </summary>
        public HtmModuleTopology ModuleTopology { get; set; }

        //protected int[] dimensions;

        //protected int[] dimensionMultiples;
        //public bool IsColumnMajorOrdering { get; set; }

        //protected int numDimensions;

        /**
         * Constructs a new {@link AbstractFlatMatrix} object to be configured with specified
         * dimensions and major ordering.
         * @param dimensions  the dimensions of this matrix	
         */
        //public AbstractFlatMatrix(int[] dimensions) : this(dimensions, false)
        //{

        //}

        /// <summary>
        /// Constructs a new <see cref="AbstractFlatMatrix"/> object to be configured with specified dimensions and major ordering.
        /// </summary>
        /// <param name="dimensions">the dimensions of this sparse array</param>
        /// <param name="useColumnMajorOrdering">flag indicating whether to use column ordering or row major ordering. 
        ///                                      if false (the default), then row major ordering will be used.If true, 
        ///                                      then column major ordering will be used.</param>
        public AbstractFlatMatrix(int[] dimensions, bool useColumnMajorOrdering)
        {
            this.ModuleTopology = new HtmModuleTopology(dimensions, useColumnMajorOrdering);

            //this.dimensions = dimensions;
            //this.numDimensions = dimensions.Length;
            //this.dimensionMultiples = InitDimensionMultiples(
            //        useColumnMajorOrdering ? Reverse(dimensions) : dimensions);
            //IsColumnMajorOrdering = useColumnMajorOrdering;
        }

        /// <summary>
        /// Compute the flat index of a multidimensional array.
        /// </summary>
        /// <param name="indexes">multidimensional indexes</param>
        /// <returns>the flat array index.</returns>
        public int ComputeIndex(int[] indexes)
        {
            return ComputeIndex(indexes, true);
        }

        /// <summary>
        /// Returns a flat index computed from the specified coordinates which represent a "dimensioned" index.
        /// </summary>
        /// <param name="coordinates">an array of coordinates</param>
        /// <param name="doCheck">enforce validated comparison to locally stored dimensions</param>
        /// <returns>a flat index</returns>
        public int ComputeIndex(int[] coordinates, bool doCheck)
        {
            if (doCheck) CheckDims(GetDimensions(), GetNumDimensions(), coordinates);

            int[] localMults = this.ModuleTopology.IsMajorOrdering ?
                Reverse(this.ModuleTopology.DimensionMultiplies) : this.ModuleTopology.DimensionMultiplies;
            int @base = 0;
            for (int i = 0; i < coordinates.Length; i++)
            {
                @base += (localMults[i] * coordinates[i]);
            }
            return @base;
        }



        /**
         * Returns an array of coordinates calculated from
         * a flat index.
         * 
         * @param   index   specified flat index
         * @return  a coordinate array
         */
        //@Override
        //public int[] computeCoordinatesOLD(int index)
        //{
        //    return ComputeCoordinates(getNumDimensions(), dimensionMultiples, IsColumnMajorOrdering, index);            
        //}

        /// <summary>
        /// Utility method to shrink a single dimension array by one index.
        /// </summary>
        /// <param name="array">the array to shrink</param>
        /// <returns></returns>
        protected int[] CopyInnerArray(int[] array)
        {
            if (array.Length == 1) return array;

            int[] retVal = new int[array.Length - 1];
            Array.Copy(array, 1, retVal, 0, array.Length - 1);
            return retVal;
        }




        public abstract T GetColumn(int index);

        public abstract T get(params int[] index);

        public abstract AbstractFlatMatrix<T> set(int index, T value);

        /// <summary>
        /// Sets batcvh of values.
        /// </summary>
        /// <param name="updatingValues"></param>
        /// <returns></returns>
        public abstract AbstractFlatMatrix<T> set(List<KeyPair> updatingValues);

        /// <summary>
        /// Sets same value to multiple indexes.
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual AbstractFlatMatrix<T> set(int[] indexes, T value)
        {
            set(ComputeIndex(indexes), value);
            return this;
        }


        IFlatMatrix<T> IFlatMatrix<T>.set(int index, T value)
        {
            return set(index, value);
            //  throw new NotImplementedException();
        }

        IMatrix<T> IMatrix<T>.set(int[] index, T value)
        {
            return set(index, value);
            // throw new NotImplementedException();
        }

        //@Override
        //public virtual T get(int indexes)
        //{
        //    return get(computeIndex(indexes));
        //}


        public int GetSize()
        {
            int partialResult = 0;

            foreach (var dim in this.ModuleTopology.Dimensions)
            {
                partialResult *= dim;
            }

            return partialResult;
            //return Arrays.stream(this.dimensions).reduce((n, i)->n * i).getAsInt();
        }

        //@Override
        public virtual int GetMaxIndex()
        {
            return GetDimensions()[0] * Math.Max(1, GetDimensionMultiples()[0]) - 1;
        }

        //@Override
        public virtual int[] GetDimensions()
        {
            return this.ModuleTopology.Dimensions;
        }

        public void SetDimensions(int[] dimensions)
        {
            this.ModuleTopology.Dimensions = dimensions;
        }

        //@Override
        public virtual int GetNumDimensions()
        {
            return this.ModuleTopology.Dimensions.Length;
        }

        //@Override
        public int[] GetDimensionMultiples()
        {
            return this.ModuleTopology.DimensionMultiplies;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */
        //@Override
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + this.ModuleTopology.DimensionMultiplies.GetHashCode();
            result = prime * result + this.ModuleTopology.Dimensions.GetHashCode();
            result = prime * result + (this.ModuleTopology.IsMajorOrdering ? 1231 : 1237);
            result = prime * result + this.ModuleTopology.NumDimensions;
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */
        //@SuppressWarnings("rawtypes")
        //@Override
        public virtual bool Equals(AbstractFlatMatrix<T> obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if ((obj.GetType() != this.GetType()))
                return false;
            AbstractFlatMatrix<T> other = obj;
            if (ModuleTopology == null)
            {
                if (obj.ModuleTopology != null)
                    return false;
            }
            else if (!ModuleTopology.Equals(obj.ModuleTopology))
                return false;
            return true;
        }

        public abstract int[] GetSparseIndices();


        public abstract int[] Get1DIndexes();



        //public abstract T[] asDense(ITypeFactory<T> factory);

        // public abstract IFlatMatrix<T> set(int index, T value);



    }
}
