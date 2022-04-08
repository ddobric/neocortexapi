// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Entities
{
    // TODO naming convention with interface method
    /// <summary>
    /// Allows storage of array data in sparse form, meaning that the indexes of the data stored are maintained while empty indexes are not. This allows
    /// savings in memory and computational efficiency because iterative algorithms need only query indexes containing valid data. The dimensions of matrix 
    /// defined at construction time and immutable - matrix fixed size data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Authors of the JAVA implementation: David Ray, Jose Luis Martin
    /// </remarks>
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

        int ComputeIndex(int[] coordinates);

        /// <summary>
        /// Returns the maximum accessible flat index.
        /// </summary>
        /// <returns>the maximum accessible flat index.</returns>
        int GetMaxIndex();

        int ComputeIndex(int[] coordinates, bool doCheck);

        /**
         * Returns an integer array representing the coordinates of the specified index
         * in terms of the configuration of this {@code SparseMatrix}.
         * @param index the flat index to be returned as coordinates
         * @return  coordinates
         */
        //int[] computeCoordinates(int index);

        //bool IsColumnMajorOrdering {get;set;}

        HtmModuleTopology ModuleTopology { get; set; }

        int[] GetDimensionMultiples();
    }
}
