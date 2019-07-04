using System;
using System.Collections.Generic;
using System.Text;

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
    //[Serializable]
    public abstract class AbstractSparseMatrix<T> : AbstractFlatMatrix<T>, ISparseMatrix<T>
    {
        
        public AbstractSparseMatrix()
        {

        }

    /**
     * Constructs a new {@code AbstractSparseMatrix} with the specified
     * dimensions (defaults to row major ordering)
     * 
     * @param dimensions    each indexed value is a dimension size
     */
    public AbstractSparseMatrix(int[] dimensions) : this(dimensions, false)
    {

    }

    /**
     * Constructs a new {@code AbstractSparseMatrix} with the specified dimensions,
     * allowing the specification of column major ordering if desired. 
     * (defaults to row major ordering)
     * 
     * @param dimensions                each indexed value is a dimension size
     * @param useColumnMajorOrdering    if true, indicates column first iteration, otherwise
     *                                  row first iteration is the default (if false).
     */
    public AbstractSparseMatrix(int[] dimensions, bool useColumnMajorOrdering) : base(dimensions, useColumnMajorOrdering)
    {

    }


    /**
     * Sets the object to occupy the specified index.
     * 
     * @param index     the index the object will occupy
     * @param value     the value to be indexed.
     * 
     * @return this {@code SparseMatrix} implementation
     */
    // protected <S extends AbstractSparseMatrix<T>> S set(int index, int value) { return null; }

    public override AbstractFlatMatrix<T> set(int index, T value)
    {
        return null;
    }
    //        public override AbstractFlatMatrix<T> set(int index, int value)
    //#pragma warning restore IDE1006 // Naming Styles
    //        { return null; }

    /**
     * Sets the object to occupy the specified index.
     * 
     * @param index     the index the object will occupy
     * @param value     the value to be indexed.
     * 
     * @return this {@code SparseMatrix} implementation
     */
    protected virtual AbstractSparseMatrix<T> Set(int index, double value)
    { return null; }

    /**
     * Sets the specified object to be indexed at the index
     * computed from the specified coordinates.
     * @param object        the object to be indexed.
     * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
     * 
     * @return this {@code SparseMatrix} implementation
     */

    public AbstractSparseMatrix<T> Set(int[] coordinates, T obj) { return null; }

    /**
     * Sets the specified object to be indexed at the index
     * computed from the specified coordinates.
     * @param value         the value to be indexed.
     * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
     * 
     * @return this {@code SparseMatrix} implementation
     */
    protected virtual AbstractSparseMatrix<T> set(int value, int[] coordinates) { return null; }

    /**
     * Sets the specified object to be indexed at the index
     * computed from the specified coordinates.
     * @param value         the value to be indexed.
     * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
     * 
     * @return this {@code SparseMatrix} implementation
     */
    protected virtual AbstractSparseMatrix<T> Set(double value, int[] coordinates) { return null; }

    /**
     * Returns the T at the specified index.
     * 
     * @param index     the index of the T to return
     * @return  the T at the specified index.
     */
    public virtual T getObject(int index)
    {
        return default(T);
    }

    public abstract ICollection<KeyPair> GetObjects(int[] indexes);


    /**
     * Returns the T at the specified index.
     * 
     * @param index     the index of the T to return
     * @return  the T at the specified index.
     */
    protected int getIntValue(int index) { return -1; }

    /**
     * Returns the T at the specified index.
     * 
     * @param index     the index of the T to return
     * @return  the T at the specified index.
     */
    protected double getDoubleValue(int index) { return -1.0; }

    /**
     * Returns the T at the index computed from the specified coordinates
     * @param coordinates   the coordinates from which to retrieve the indexed object
     * @return  the indexed object
     */
    public override T get(int[] coordinates) { return default(T); }

    /**
     * Returns the int value at the index computed from the specified coordinates
     * @param coordinates   the coordinates from which to retrieve the indexed object
     * @return  the indexed object
     */
    protected int getIntValue(int[] coordinates) { return -1; }

    /**
     * Returns the double value at the index computed from the specified coordinates
     * @param coordinates   the coordinates from which to retrieve the indexed object
     * @return  the indexed object
     */
    protected double getDoubleValue(int[] coordinates) { return -1.0; }

    // @Override
    public override int[] getSparseIndices()
    {
        return null;
    }

    //  @Override
    public override int[] get1DIndexes()
    {
        List<int> results = new List<int>(getMaxIndex() + 1);
        visit(getDimensions(), 0, new int[getNumDimensions()], results);
        return results.ToArray();
    }

    /**
     * Recursively loops through the matrix dimensions to fill the results
     * array with flattened computed array indexes.
     * 
     * @param bounds
     * @param currentDimension
     * @param p
     * @param results
     */
    private void visit(int[] bounds, int currentDimension, int[] p, List<int> results)
    {
        for (int i = 0; i < bounds[currentDimension]; i++)
        {
            p[currentDimension] = i;
            if (currentDimension == p.Length - 1)
            {
                results.Add(computeIndex(p));
            }
            else visit(bounds, currentDimension + 1, p, results);
        }
    }


    //public override T[] asDense(ITypeFactory<T> factory)
    //{
    //    throw NotImplementedException();

    //    int[] dimensions = getDimensions();
    //    T[] retVal = (T[])Array.CreateInstance(typeof(T), dimensions);

    //    fill(factory, 0, dimensions, dimensions[0], (object[])(object)retVal);

    //    return retVal;
    //}

    /**
     * Uses reflection to create and fill a dynamically created multidimensional array.
     * 
     * @param f                 the {@link TypeFactory}
     * @param dimensionIndex    the current index into <em>this class's</em> configured dimensions array
     *                          <em>*NOT*</em> the dimensions used as this method's argument    
     * @param dimensions        the array specifying remaining dimensions to create
     * @param count             the current dimensional size
     * @param arr               the array to fill
     * @return a dynamically created multidimensional array
     */
    //@SuppressWarnings("unchecked")
    protected Object[] fill(ITypeFactory<T> f, int dimensionIndex, int[] dimensions, int count, Object[] arr)
    {
        if (dimensions.Length == 1)
        {
            for (int i = 0; i < count; i++)
            {
                arr[i] = f.make(getDimensions());
                // arr[i] = new 
            }
            return arr;
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                int[] inner = copyInnerArray(dimensions);
                //T[] r = (T[])Array.newInstance(f.typeClass(), inner);
                T[] r = (T[])Array.CreateInstance(typeof(T), inner);
                arr[i] = fill(f, dimensionIndex + 1, inner, getDimensions()[dimensionIndex + 1], (object[])(object)r);
            }
            return arr;
        }
    }

}
}
