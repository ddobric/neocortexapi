// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoCortexApi.Entities
{
    // TODO naming convention for interface method
    
    /// <summary>
    /// Definition of the sparse matrix.
    /// </summary>
    public interface ISparseMatrix
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISparseMatrix<T> : ISparseMatrix, IFlatMatrix<T>
    {
        /// <summary>
        /// Returns a sorted array of occupied indexes.
        /// </summary>
        /// <returns>a sorted array of occupied indexes.</returns>
        int[] GetSparseIndices();


        /// <summary>
        /// Returns an array of all the flat indexes that can be computed from the current configuration.
        /// </summary>
        /// <returns></returns>
        int[] Get1DIndexes();

     
        public void Serialize(StreamWriter sw)
        {
            throw new NotImplementedException();
        }
    }
}


