// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Entities
{
    // TODO naming convention with interface method
    /// <summary>
    /// Base interface for Matrices.
    /// </summary>
    /// <typeparam name="T">element type</typeparam>
    /// <remarks>
    /// Authors of the JAVA implementation: Jose Luis Martin
    /// </remarks>
    public interface IMatrix<T>
    {
        /// <summary>
        /// Returns the array describing the dimensionality of the configured array.
        /// </summary>
        /// <returns>the array describing the dimensionality of the configured array.</returns>
        int[] GetDimensions();

        /// <summary>
        /// Returns the configured number of dimensions.
        /// </summary>
        /// <returns>the configured number of dimensions.</returns>
        int GetNumDimensions();

        /// <summary>
        /// Gets element at supplied index.
        /// </summary>
        /// <param name="index">index index to retrieve.</param>
        /// <returns>element at index.</returns>
        T get(params int[] index);

        /// <summary>
        /// Puts an element to supplied index.
        /// </summary>
        /// <param name="index">index to put on.</param>
        /// <param name="value">value element.</param>
        /// <returns></returns>
        IMatrix<T> set(int[] index, T value);
    }
}
