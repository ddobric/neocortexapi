using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{

    /**
     * Base interface for Matrices.
     * 
     * @author Jose Luis Martin.
     * 
     * @param <T> element type
     */
    public interface IMatrix<T>
    {

        /**
         * Returns the array describing the dimensionality of the configured array.
         * @return  the array describing the dimensionality of the configured array.
         */
        int[] getDimensions();

        /**
         * Returns the configured number of dimensions.
         * @return  the configured number of dimensions.
         */
        int getNumDimensions();

        /**
         * Gets element at supplied index.
         * @param index index to retrieve.
         * @return element at index.
         */
        T get(params int[] index);

        /**
         * Puts an element to supplied index.
         * @param index index to put on.
         * @param value value element.
         */
        IMatrix<T> set(int[] index, T value);
    }
}
