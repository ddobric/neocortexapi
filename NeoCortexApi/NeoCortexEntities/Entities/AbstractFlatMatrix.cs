
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /**
 * Base class for flat {@link Matrix} implementations.
 * 
 * @author David Ray
 * @author Jose Luis Martin
 * 
 * @param <T> element type
 */

    //[Serializable]
    public abstract class AbstractFlatMatrix<T> : IFlatMatrix<T>
    {

        private static long serialVersionUID = 1L;

        protected int[] dimensions;
        protected int[] dimensionMultiples;
        protected bool isColumnMajor;
        protected int numDimensions;

        /**
         * Constructs a new {@link AbstractFlatMatrix} object to be configured with specified
         * dimensions and major ordering.
         * @param dimensions  the dimensions of this matrix	
         */
        public AbstractFlatMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /**
         * Constructs a new {@link AbstractFlatMatrix} object to be configured with specified
         * dimensions and major ordering.
         * 
         * @param dimensions				the dimensions of this sparse array	
         * @param useColumnMajorOrdering	flag indicating whether to use column ordering or
         * 									row major ordering. if false (the default), then row
         * 									major ordering will be used. If true, then column major
         * 									ordering will be used.
         */
        public AbstractFlatMatrix(int[] dimensions, bool useColumnMajorOrdering)
        {
            this.dimensions = dimensions;
            this.numDimensions = dimensions.Length;
            this.dimensionMultiples = initDimensionMultiples(
                    useColumnMajorOrdering ? reverse(dimensions) : dimensions);
            isColumnMajor = useColumnMajorOrdering;
        }

        /**
         * Compute the flat index of a multidimensional array.
         * @param indexes multidimensional indexes
         * @return the flat array index;
         */
        public int computeIndex(int[] indexes)
        {
            return computeIndex(indexes, true);
        }

        /**
         * Returns a flat index computed from the specified coordinates
         * which represent a "dimensioned" index.
         * 
         * @param   coordinates     an array of coordinates
         * @param   doCheck         enforce validated comparison to locally stored dimensions
         * @return  a flat index
         */
        public int computeIndex(int[] coordinates, bool doCheck)
        {
            if (doCheck) checkDims(coordinates);

            int[] localMults = isColumnMajor ? reverse(dimensionMultiples) : dimensionMultiples;
            int @base = 0;
            for (int i = 0; i < coordinates.Length; i++)
            {
                @base += (localMults[i] * coordinates[i]);
            }
            return @base;
        }

        /**
         * Checks the indexes specified to see whether they are within the
         * configured bounds and size parameters of this array configuration.
         * 
         * @param index the array dimensions to check
         */
        protected void checkDims(int[] index)
        {
            if (index.Length != numDimensions)
            {
                throw new ArgumentException("Specified coordinates exceed the configured array dimensions " +
                        "input dimensions: " + index.Length + " > number of configured dimensions: " + numDimensions);
            }
            for (int i = 0; i < index.Length - 1; i++)
            {
                if (index[i] >= dimensions[i])
                {
                    throw new ArgumentException("Specified coordinates exceed the configured array dimensions " +
                            print1DArray(index) + " > " + print1DArray(dimensions));
                }
            }
        }

        /**
         * Returns an array of coordinates calculated from
         * a flat index.
         * 
         * @param   index   specified flat index
         * @return  a coordinate array
         */
        //@Override
        public int[] computeCoordinates(int index)
        {
            int[] returnVal = new int[getNumDimensions()];
            int @base = index;
            for (int i = 0; i < dimensionMultiples.Length; i++)
            {
                int quotient = @base / dimensionMultiples[i];
                @base %= dimensionMultiples[i];
                returnVal[i] = quotient;
            }
            return isColumnMajor ? reverse(returnVal) : returnVal;
        }

        /**
         * Initializes internal helper array which is used for multidimensional
         * index computation.
         * @param dimensions matrix dimensions
         * @return array for use in coordinates to flat index computation.
         */
        protected int[] initDimensionMultiples(int[] dimensions)
        {
            int holder = 1;
            int len = dimensions.Length;
            int[] dimensionMultiples = new int[getNumDimensions()];
            for (int i = 0; i < len; i++)
            {
                holder *= (i == 0 ? 1 : dimensions[len - i]);
                dimensionMultiples[len - 1 - i] = holder;
            }
            return dimensionMultiples;
        }

        /**
         * Utility method to shrink a single dimension array by one index.
         * @param array the array to shrink
         * @return
         */
        protected int[] copyInnerArray(int[] array)
        {
            if (array.Length == 1) return array;

            int[] retVal = new int[array.Length - 1];
            Array.Copy(array, 1, retVal, 0, array.Length - 1);
            return retVal;
        }

        /**
         * Reverses the specified array.
         * @param input
         * @return
         */
        public static int[] reverse(int[] input)
        {
            int[] retVal = new int[input.Length];
            for (int i = input.Length - 1, j = 0; i >= 0; i--, j++)
            {
                retVal[j] = input[i];
            }
            return retVal;
        }

        /**
         * Prints the specified array to a returned String.
         * 
         * @param aObject   the array object to print.
         * @return  the array in string form suitable for display.
         */
        public static String print1DArray(Object aObject)
        {
            if (aObject is Array)
            {
                if (!typeof(T).IsValueType) // can we cast to Object[]
                    return aObject.ToString();
                else
                {  // we can't cast to Object[] - case of primitive arrays
                    int length = ((Array)aObject).Length;
                    Object[] objArr = new Object[length];
                    for (int i = 0; i < length; i++)
                        objArr[i] = ((Array)aObject).GetValue(i);
                    return objArr.ToString();
                }
            }
            return "[]";
        }

        public abstract T get(int index);

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
            set(computeIndex(indexes), value);
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


        public int getSize()
        {
            int partialResult = 0;

            foreach (var dim in dimensions)
            {
                partialResult = partialResult * dim;
            }

            return partialResult;
            //return Arrays.stream(this.dimensions).reduce((n, i)->n * i).getAsInt();
        }

        //@Override
        public virtual int getMaxIndex()
        {
            return getDimensions()[0] * Math.Max(1, getDimensionMultiples()[0]) - 1;
        }

        //@Override
        public virtual int[] getDimensions()
        {
            return this.dimensions;
        }

        public void setDimensions(int[] dimensions)
        {
            this.dimensions = dimensions;
        }

        //@Override
        public virtual int getNumDimensions()
        {
            return this.dimensions.Length;
        }

        //@Override
        public int[] getDimensionMultiples()
        {
            return this.dimensionMultiples;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */
        //@Override
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + dimensionMultiples.GetHashCode();
            result = prime * result + dimensions.GetHashCode();
            result = prime * result + (isColumnMajor ? 1231 : 1237);
            result = prime * result + numDimensions;
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */
        //@SuppressWarnings("rawtypes")
        //@Override
        public override bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            //if (getClass() != obj.getClass())
            if ((obj.GetType() != this.GetType()))
                return false;
            AbstractFlatMatrix<T> other = (AbstractFlatMatrix<T>)obj;

            if (!Array.Equals(dimensionMultiples, other.dimensionMultiples))
                return false;
            if (!Array.Equals(dimensions, other.dimensions))
                return false;
            if (isColumnMajor != other.isColumnMajor)
                return false;
            if (numDimensions != other.numDimensions)
                return false;
            return true;
        }

        public abstract int[] getSparseIndices();


        public abstract int[] get1DIndexes();

       

        //public abstract T[] asDense(ITypeFactory<T> factory);

        // public abstract IFlatMatrix<T> set(int index, T value);



    }
}
