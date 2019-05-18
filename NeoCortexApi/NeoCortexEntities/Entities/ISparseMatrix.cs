using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{

    public interface ISparseMatrix 
    {

    }
    
    public interface ISparseMatrix<T> : ISparseMatrix, IFlatMatrix<T>
    {
        /**
            * Returns a sorted array of occupied indexes.
            * @return  a sorted array of occupied indexes.
            */
        int[] getSparseIndices();

        /**
         * Returns an array of all the flat indexes that can be 
         * computed from the current configuration.
         * @return
         */
        int[] get1DIndexes();

        /**
         * Uses the specified {@link TypeFactory} to return an array
         * filled with the specified object type, according this {@code SparseMatrix}'s 
         * configured dimensions
         * 
         * @param factory   a factory to make a specific type
         * @return  the dense array
         */
        //T[] asDense(ITypeFactory<T> factory);

    }
}


