#if !USE_AKKA

using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /**
 * Implementation of a sparse matrix which contains binary integer
 * values only.
 * 
 * @author cogmission
 *
 */
    public class SparseBinaryMatrix : AbstractSparseBinaryMatrix
    {
        
        /// <summary>
        /// Holds the matrix with connections between columns and inputs.
        /// 
        /// </summary>
        private Array backingArray;

        /**
         * Constructs a new {@code SparseBinaryMatrix} with the specified
         * dimensions (defaults to row major ordering)
         * 
         * @param dimensions    each indexed value is a dimension size
         */
        public SparseBinaryMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /**
         * Constructs a new {@code SparseBinaryMatrix} with the specified dimensions,
         * allowing the specification of column major ordering if desired. 
         * (defaults to row major ordering)
         * 
         * @param dimensions                each indexed value is a dimension size
         * @param useColumnMajorOrdering    if true, indicates column first iteration, otherwise
         *                                  row first iteration is the default (if false).
         */
        public SparseBinaryMatrix(int[] dimensions, bool useColumnMajorOrdering) : base(dimensions, useColumnMajorOrdering)
        {
            this.backingArray = Array.CreateInstance(typeof(int), dimensions);
        }

        /**
         * Sets the value on specified call in array and automattically calculates number of '1' bits as TrueCount.
         * Called during mutation operations to simultaneously set the value
         * on the backing array dynamically.
         * @param val
         * @param coordinates
         */
        private void back(int val, params int[] coordinates)
        {
            //update true counts
            ArrayUtils.setValue(this.backingArray, val, coordinates);
            int aggVal = -1;
            //int aggregateVal = ArrayUtils.aggregateArray(((System.Int32[,])this.backingArray)[coordinates[0],1]);
            if (this.backingArray is System.Int32[,])
            {
                var row = ArrayUtils.GetRow<Int32>((System.Int32[,])this.backingArray, coordinates[0]);
                aggVal = ArrayUtils.aggregateArray(row);
            }
            else
                throw new NotSupportedException();

            setTrueCount(coordinates[0], aggVal);
            // setTrueCount(coordinates[0], ArrayUtils.aggregateArray(((Object[])this.backingArray)[coordinates[0]]));
        }

        /**
         * Returns the slice specified by the passed in coordinates.
         * The array is returned as an object, therefore it is the caller's
         * responsibility to cast the array to the appropriate dimensions.
         * 
         * @param coordinates	the coordinates which specify the returned array
         * @return	the array specified
         * @throws	IllegalArgumentException if the specified coordinates address
         * 			an actual value instead of the array holding it.
         */
        // @Override

        public override Object getSlice(params int[] coordinates)
        {
            //Object slice = ArrayUtils.getValue(this.backingArray, coordinates);
            Object slice;
            if (coordinates.Length == 1)
                slice = ArrayUtils.GetRow<int>((int[,])this.backingArray, coordinates[0]);
            //else if (coordinates.Length == 1)
            //    slice = ((int[])this.backingArray)[coordinates[0]];
            else
                throw new ArgumentException();

            //Ensure return value is of type Array
            if (!slice.GetType().IsArray)
            {
                sliceError(coordinates);
            }

            return slice;
        }

        /**
         * Fills the specified results array with the result of the 
         * matrix vector multiplication.
         * 
         * @param inputVector		the right side vector
         * @param results			the results array
         */
        public override void rightVecSumAtNZ(int[] inputVector, int[] results)
        {
            for (int i = 0; i < dimensions[0]; i++)
            {
                int[] slice = (int[])(dimensions.Length > 1 ? getSlice(i) : backingArray);
                for (int j = 0; j < slice.Length; j++)
                {
                    results[i] += (inputVector[j] * slice[j]);
                }
            }
        }

        /**
         * Fills the specified results array with the result of the 
         * matrix vector multiplication.
         * 
         * @param inputVector       the right side vector
         * @param results           the results array
         */
        public override void rightVecSumAtNZ(int[] inputVector, int[] results, double stimulusThreshold)
        {
            for (int i = 0; i < dimensions[0]; i++)
            {
                // Gets the synapse mapping between column-i with input vector.
                int[] slice = (int[])(dimensions.Length > 1 ? getSlice(i) : backingArray);

                // Go through all connections (synapses) between column-i and input vector.
                for (int j = 0; j < slice.Length; j++)
                {
                    // Result (overlapp) is 1 if 
                    results[i] += (inputVector[j] * slice[j]);
                    if (j == slice.Length - 1)
                    {
                        // If the overlap (num of connected synapses to TRUE input) is less than stimulusThreshold then we set result on 0.
                        // If the overlap (num of connected synapses to TRUE input) is greather than stimulusThreshold then result remains as calculated.
                        // This ensures that only overlaps are calculated, which are over the stimulusThreshold. All less than stimulusThreshold are set on 0.
                        results[i] -= results[i] < stimulusThreshold ? results[i] : 0;
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


        //protected override AbstractSparseMatrix<int> set(int index, int value)
        //{
        //    int[] coordinates = computeCoordinates(index);
        //    return set(value, coordinates);
        //}


        //        /**
        //         * Sets the value at the specified index.
        //         * 
        //         * @param index     the index the object will occupy
        //         * @param object    the object to be indexed.
        //         */
        //        // @Override
        //#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        //        public AbstractSparseBinaryMatrix set(int index, int value)
        //#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        //        {
        //            int[] coordinates = computeCoordinates(index);
        //            return set(value, coordinates);
        //        }

        /**
         * Sets the value to be indexed at the index
         * computed from the specified coordinates.
         * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
         * @param object        the object to be indexed.
         */
        //@Override
        public override AbstractSparseBinaryMatrix set(int value, params int[] coordinates)
        {
            back(value, coordinates);
            return this;
        }

        /**
         * Sets the specified values at the specified indexes.
         * 
         * @param indexes   indexes of the values to be set
         * @param values    the values to be indexed.
         * 
         * @return this {@code SparseMatrix} implementation
         */
        public AbstractSparseBinaryMatrix set(int[] indexes, int[] values)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                set(indexes[i], values[i]);
            }
            return this;
        }

        /**
         * Clears the true counts prior to a cycle where they're
         * being set
         */
        public override void clearStatistics(int row)
        {
            if (backingArray.Rank != 2)
                throw new InvalidOperationException("Currently supported 2D arrays only");

            var cols = backingArray.GetLength(1);
            for (int i = 0; i < cols; i++)
            {
                backingArray.SetValue(0, row, i);
            }

            this.setTrueCount(row, 0);
            //int[] slice = (int[])backingArray.GetValue(row);
            //    int[] slice = (int[])Array.get(backingArray, row);
            //ArrayUtils.Fill(slice, 0);
            // Array.fill(slice, 0);
        }




        //  @Override
        public override AbstractSparseBinaryMatrix setForTest(int index, int value)
        {
            ArrayUtils.setValue(this.backingArray, value, computeCoordinates(index));
            return this;
        }


        //public override Integer Get(int index)
        //{
        //    return (Integer)get(index);
        //}


        // @Override
        public override int get(int index)
        {
            int[] coordinates = computeCoordinates(index);
            if (coordinates.Length == 1)
            {
                return (Integer)backingArray.GetValue(index);
                // return Array.getInt(this.backingArray, index);
            }

            else return (int)ArrayUtils.getValue(this.backingArray, coordinates);
        }

   
     

    }
}
#endif