// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeoCortexApi.Entities
{
    public interface IDistributedArray //:// ICollection//ICollection, IEnumerable//, IList
    {
        //
        // Summary:
        //     Gets or sets the element at the specified index.
        //
        // Parameters:
        //   index:
        //     The zero-based index of the element to get or set.
        //
        // Returns:
        //     The element at the specified index.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     index is not a valid index in the System.Collections.IList.
        //
        //   T:System.NotSupportedException:
        //     The property is set and the System.Collections.IList is read-only.
        object this[int row, int col] { get; set; }


        object this[int index] { get; set; }

        long Count { get; }

        int AggregateArray(int row);

        void SetValue(int value, int[] indexes);

        /// <summary>
        /// Gets value from single dimension array.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object GetValue(int index);

        /// <summary>
        /// Get value from two-dim array.
        /// </summary>
        /// <param name="indexes"></param>
        /// <returns></returns>
        object GetValue(int[] indexes);

        int GetUpperBound(int v);

        double Max();

        /// <summary>
        /// Number of dimensions
        /// </summary>
        int Rank { get; }

        /// <summary>
        /// Dimensions
        /// </summary>
        int[] Dimensions { get; }

        /// <summary>
        /// Sets all values of specified row on specified value.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="newVal"></param>
        void SetRowValuesTo(int rowIndex, object newVal);

        public virtual void Serialize(StreamWriter writer)
        {
            throw new NotImplementedException();
        }

    }
}
