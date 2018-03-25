using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi;

namespace NeoCortexApi.Entities
{
    /**
 * Allows storage of array data in sparse form, meaning that the indexes
 * of the data stored are maintained while empty indexes are not. This allows
 * savings in memory and computational efficiency because iterative algorithms
 * need only query indexes containing valid data. The dimensions of matrix defined
 * at construction time and immutable - matrix fixed size data structure.
 * 
 * @author David Ray
 * @author Jose Luis Martin
 *
 * @param <T>
 */

    interface IFlatMatrix<T>
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
        T[] asDense(ITypeFactory<T> factory);

    }
}
