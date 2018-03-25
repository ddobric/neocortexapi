using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /**
   * Allows storage of array data in sparse form, meaning that the indexes
   * of the data stored are maintained while empty indexes are not. This allows
   * savings in memory and computational efficiency because iterative algorithms
   * need only query indexes containing valid data.
   * 
   * @author David Ray
   *
   * @param <T>
   */
    public class SparseObjectMatrix<T> : AbstractSparseMatrix<T>, IEquatable<T>  {
    /** keep it simple */
    private static final long serialVersionUID = 1L;

    private TIntObjectMap<T> sparseMap = new TIntObjectHashMap<T>();

    /**
     * Constructs a new {@code SparseObjectMatrix}
     * @param dimensions	the dimensions of this array
     */
    public SparseObjectMatrix(int[] dimensions)
    {
        super(dimensions, false);
    }

    /**
     * Constructs a new {@code SparseObjectMatrix}
     * @param dimensions					the dimensions of this array
     * @param useColumnMajorOrdering		where inner index increments most frequently
     */
    public SparseObjectMatrix(int[] dimensions, boolean useColumnMajorOrdering)
    {
        super(dimensions, useColumnMajorOrdering);
    }

    /**
     * Sets the object to occupy the specified index.
     * 
     * @param index     the index the object will occupy
     * @param object    the object to be indexed.
     */
    @Override
    public SparseObjectMatrix<T> set(int index, T object)
    {
        sparseMap.put(index, (T)object);
        return this;
    }

    /**
     * Sets the specified object to be indexed at the index
     * computed from the specified coordinates.
     * @param object        the object to be indexed.
     * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
     */
    @Override
    public SparseObjectMatrix<T> set(int[] coordinates, T object)
    {
        set(computeIndex(coordinates), object);
        return this;
    }

    /**
     * Returns the T at the specified index.
     * 
     * @param index     the index of the T to return
     * @return  the T at the specified index.
     */
    @Override
    public T getObject(int index)
    {
        return get(index);
    }

    /**
     * Returns the T at the index computed from the specified coordinates
     * @param coordinates   the coordinates from which to retrieve the indexed object
     * @return  the indexed object
     */
    @Override
    public T get(int... coordinates)
    {
        return get(computeIndex(coordinates));
    }

    /**
     * Returns the T at the specified index.
     * 
     * @param index     the index of the T to return
     * @return  the T at the specified index.
     */
    @Override
    public T get(int index)
    {
        return this.sparseMap.get(index);
    }

    /**
     * Returns a sorted array of occupied indexes.
     * @return  a sorted array of occupied indexes.
     */
    @Override
    public int[] getSparseIndices()
    {
        return reverse(sparseMap.keys());
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String toString()
    {
        return Arrays.toString(getDimensions());
    }

    /* (non-Javadoc)
     * @see java.lang.Object#hashCode()
     */
    @Override
    public int hashCode()
    {
        final int prime = 31;
        int result = super.hashCode();
        result = prime * result + ((sparseMap == null) ? 0 : sparseMap.hashCode());
        return result;
    }

    /* (non-Javadoc)
     * @see java.lang.Object#equals(java.lang.Object)
     */
   //SuppressWarnings("rawtypes")
   // @Override
    public bool Equals(Object obj)
    {
        if (this == obj)
            return true;
        if (!super.equals(obj))
            return false;
        if (getClass() != obj.getClass())
            return false;
        SparseObjectMatrix other = (SparseObjectMatrix)obj;
        if (sparseMap == null)
        {
            if (other.sparseMap != null)
                return false;
        }
        else if (!sparseMap.equals(other.sparseMap))
            return false;
        return true;
    }

}
}
