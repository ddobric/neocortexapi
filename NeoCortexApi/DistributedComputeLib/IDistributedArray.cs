using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    public interface IDistributedArray : ICollection, IEnumerable, IList
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

        int AggregateArray();
        void SetValue(int value, int[] indexes);
        int GetUpperBound(int v);

        double Max();
    }
}
