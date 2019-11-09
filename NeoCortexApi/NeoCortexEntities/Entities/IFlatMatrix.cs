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


    public interface IFlatMatrix<T> : IMatrix<T>
    {

        T GetColumn(int index);

        /// <summary>
        /// Sets the single value in the array.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IFlatMatrix<T> set(int index, T value);

        int computeIndex(int[] coordinates);

        /**
         * Returns the maximum accessible flat index.
         * @return  the maximum accessible flat index.
         */
        int getMaxIndex();

        int computeIndex(int[] coordinates, bool doCheck);

        /**
         * Returns an integer array representing the coordinates of the specified index
         * in terms of the configuration of this {@code SparseMatrix}.
         * @param index the flat index to be returned as coordinates
         * @return  coordinates
         */
        //int[] computeCoordinates(int index);

        //bool IsColumnMajorOrdering {get;set;}

        HtmModuleTopology ModuleTopology {get;set;}

        int[] getDimensionMultiples();
    }
}
