
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using NeoCortexApi.Entities;

namespace NeoCortexApi.DistributedComputeLib
{
   
    /**
     * Utilities to match some of the functionality found in Python's Numpy.
     * @author David Ray
     */
    public static class DistributedArrayHelpers
    {
        /** Empty array constant */
        private static int[] EMPTY_ARRAY = new int[0];

        //public static string ArrToString(this IDistributedArray arr)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var item in arr)
        //    {
        //        sb.Append(item.ToString());
        //        sb.Append(",");
        //    }
        //   // var value = string.Join(",", arr.Select(x => x.ToString()).ToArray());
        //    return sb.ToString();
        //}

        //public static string ArrToString(this /*int[][]*/ IDistributedArray<int> arr)
        //{
        //    var value = string.Join(" [ ", arr.Select(x => x.ArrToString()).ToArray(), " ] ");
        //    return value;
        //}

        /**
         * Returns the product of each integer in the specified array.
         * 
         * @param dims
         * @return
         */
        public static int product(int[] dims) // No distribution needed.
        {
            int retVal = 1;
            for (int i = 0; i < dims.Length; i++)
            {
                retVal *= dims[i];
            }

            return retVal;
        }

        /**
         * Returns an array containing the successive elements of each
         * argument array as in [ first[0], second[0], first[1], second[1], ... ].
         * 
         * Arrays may be of zero length, and may be of different sizes, but may not be null.
         * 
         * @param first     the first array
         * @param second    the second array
         * @return
         */
        //public static Object[] interleave<F, S>(F first, S second) where F : IList<F> where S : IList<S>
        //{
        //    int flen, slen;

        //    flen = first.Count;
        //    slen = second.Count;

        //    Object[] retVal = new Object[flen + slen];
        //    for (int i = 0, j = 0, k = 0; i < flen || j < slen;)
        //    {
        //        if (i < flen)
        //        {
        //            retVal[k++] = first[i++];
        //        }
        //        if (j < slen)
        //        {
        //            retVal[k++] = second[j++];
        //        }
        //    }

        //    return retVal;
        //}

        /**
         * <p>
         * Return a new double[] containing the difference of each element and its
         * succeding element.
         * </p><p>
         * The first order difference is given by ``out[n] = a[n+1] - a[n]``
         * along the given axis, higher order differences are calculated by using `diff`
         * recursively.
         *
         * @param d
         * @return
         */
        //public static double[] diff(double[] d)
        //{
        //    double[] retVal = new double[d.Length - 1];
        //    for (int i = 0; i < retVal.Length; i++)
        //    {
        //        retVal[i] = d[i + 1] - d[i];
        //    }

        //    return retVal;
        //}

        /**
         * Returns a flag indicating whether the container list contains an
         * array which matches the specified match array.
         *
         * @param match     the array to match
         * @param container the list of arrays to test
         * @return true if so, false if not
         */
        //public static bool contains(int[] match, List<int[]> container)
        //{
        //    for (int i = 0; i < container.Count; i++)
        //    {
        //        bool isSubset = !container[i].Except(match).Any();
        //        return isSubset;
        //        //if (Arrays.equals(match, container.get(i)))
        //        //{
        //        //    return true;
        //        //}
        //    }
        //    return false;
        //}

        /**
         * Returns a new array of size first.length + second.length, with the
         * contents of the first array loaded into the returned array starting
         * at the zero'th index, and the contents of the second array appended
         * to the returned array beginning with index first.length.
         * 
         * This method is fail fast, meaning that it depends on the two arrays
         * being non-null, and if not, an exception is thrown.
         *  
         * @param first     the data to load starting at index 0
         * @param second    the data to load starting at index first.length;
         * @return  a concatenated array
         * @throws NullPointerException if either first or second is null
         */
        //public static double[] concat(double[] first, double[] second)
        //{
        //    double[] retVal = Arrays.copyOf(first, first.length + second.length);
        //    for (int i = first.length, j = 0; i < retVal.length; i++, j++)
        //    {
        //        retVal[i] = second[j];
        //    }
        //    return retVal;
        //}

        //public static int maxIndex(int[] shape)
        //{
        //    return shape[0] * Math.max(1, initDimensionMultiples(shape)[0]) - 1;
        //}

        /**
         * Returns an array of coordinates calculated from
         * a flat index.
         * 
         * @param   index   specified flat index
         * @param   shape   the array specifying the size of each dimension
         * @param   isColumnMajor   increments row first then column (default: false)
         * 
         * @return  a coordinate array
         */
        //public static int[] toCoordinates(int index, int[] shape, boolean isColumnMajor)
        //{
        //    int[] dimensionMultiples = initDimensionMultiples(shape);
        //    int[] returnVal = new int[shape.length];
        //    int base = index;
        //    for (int i = 0; i < dimensionMultiples.length; i++)
        //    {
        //        int quotient = base / dimensionMultiples[i];
        //        base %= dimensionMultiples[i];
        //        returnVal[i] = quotient;
        //    }
        //    return isColumnMajor ? reverse(returnVal) : returnVal;
        //}

        /**
         * Utility to compute a flat index from coordinates.
         *
         * @param coordinates an array of integer coordinates
         * @return a flat index
         */
        //public static int fromCoordinate(int[] coordinates, int[] shape)
        //{
        //    int[] localMults = initDimensionMultiples(shape);
        //    int base = 0;
        //    for (int i = 0; i < coordinates.length; i++)
        //    {
        //        base += (localMults[i] * coordinates[i]);
        //    }
        //    return base;
        //}

        /**
         * Utility to compute a flat index from coordinates.
         *
         * @param coordinates an array of integer coordinates
         * @return a flat index
         */
        //public static int fromCoordinate(int[] coordinates)
        //{
        //    int[] localMults = initDimensionMultiples(coordinates);
        //    int base = 0;
        //    for (int i = 0; i < coordinates.length; i++)
        //    {
        //        base += (localMults[i] * coordinates[i]);
        //    }
        //    return base;
        //}

        /**
         * Initializes internal helper array which is used for multidimensional
         * index computation.
         *
         * @param shape     an array specifying sizes of each dimension
         * @return
         */
        //public static int[] initDimensionMultiples(int[] shape)
        //{
        //    int holder = 1;
        //    int len = shape.length;
        //    int[] dimensionMultiples = new int[shape.length];
        //    for (int i = 0; i < len; i++)
        //    {
        //        holder *= (i == 0 ? 1 : shape[len - i]);
        //        dimensionMultiples[len - 1 - i] = holder;
        //    }
        //    return dimensionMultiples;
        //}

        /**
         * Takes a two-dimensional input array and returns a new array which is "rotated"
         * a quarter-turn clockwise.
         * 
         * @param array The array to rotate.
         * @return The rotated array.
         */
        //public static int[][] rotateRight(int[][] array)
        //{
        //    int r = array.length;
        //    if (r == 0)
        //    {
        //        return new int[0][0]; // Special case: zero-length array
        //    }
        //    int c = array[0].length;
        //    int[][] result = new int[c][r];
        //    for (int i = 0; i < r; i++)
        //    {
        //        for (int j = 0; j < c; j++)
        //        {
        //            result[j][r - 1 - i] = array[i][j];
        //        }
        //    }
        //    return result;
        //}


        /**
         * Takes a two-dimensional input array and returns a new array which is "rotated"
         * a quarter-turn counterclockwise.
         * 
         * @param array The array to rotate.
         * @return The rotated array.
         */
        //public static int[][] rotateLeft(int[][] array)
        //{
        //    int r = array.length;
        //    if (r == 0)
        //    {
        //        return new int[0][0]; // Special case: zero-length array
        //    }
        //    int c = array[0].length;
        //    int[][] result = new int[c][r];
        //    for (int i = 0; i < r; i++)
        //    {
        //        for (int j = 0; j < c; j++)
        //        {
        //            result[c - 1 - j][i] = array[i][j];
        //        }
        //    }
        //    return result;
        //}

        /**
         * Takes a one-dimensional input array of m  n  numbers and returns a two-dimensional
         * array of m rows and n columns. The first n numbers of the given array are copied
         * into the first row of the new array, the second n numbers into the second row,
         * and so on. This method throws an IllegalArgumentException if the length of the input
         * array is not evenly divisible by n.
         * 
         * @param array The values to put into the new array.
         * @param n The number of desired columns in the new array.
         * @return The new m  n array.
         * @throws IllegalArgumentException If the length of the given array is not
         *  a multiple of n.
         */
        //        public static int[][] ravel(int[] array, int n) throws IllegalArgumentException
        //        {
        //        if (array.length % n != 0) {
        //        throw new IllegalArgumentException(array.length + " is not evenly divisible by " + n);
        //    }
        //    int length = array.length;
        //    int[]
        //[]
        //result = new int[length / n][n];
        //        for (int i = 0; i<length; i++) {
        //            result[i / n][i % n] = array[i];
        //        }
        //        return result;
        //    }

        /**
         * Takes a m by n two dimensional array and returns a one-dimensional array of size m  n
         * containing the same numbers. The first n numbers of the new array are copied from the
         * first row of the given array, the second n numbers from the second row, and so on.
         * 
         * @param array The array to be unraveled.
         * @return The values in the given array.
         */
        //    public static int[] unravel(int[][] array)
        //{
        //    int r = array.length;
        //    if (r == 0)
        //    {
        //        return new int[0]; // Special case: zero-length array
        //    }
        //    int c = array[0].length;
        //    int[] result = new int[r * c];
        //    int index = 0;
        //    for (int i = 0; i < r; i++)
        //    {
        //        for (int j = 0; j < c; j++)
        //        {
        //            result[index] = array[i][j];
        //            index++;
        //        }
        //    }
        //    return result;
        //}

        /**
         * Takes a two-dimensional array of r rows and c columns and reshapes it to
         * have (r*c)/n by n columns. The value in location [i][j] of the input array
         * is copied into location [j][i] of the new array.
         * 
         * @param array The array of values to be reshaped.
         * @param n The number of columns in the created array.
         * @return The new (r*c)/n by n array.
         * @throws IllegalArgumentException If r*c  is not evenly divisible by n.
         */
        //public static int[][] reshape(int[][] array, int n) throws IllegalArgumentException
        //{
        //        int r = array.length;
        //        if (r == 0) {
        //        return new int[0][0]; // Special case: zero-length array
        //    }
        //        if ((array.length * array [0].length) % n != 0) {
        //        int size = array.length * array[0].length;
        //        throw new IllegalArgumentException(size + " is not evenly divisible by " + n);
        //    }
        //        int c = array [0].length;
        //        int[]
        //    []
        //    result = new int[(r * c) / n][n];
        //        int ii = 0;
        //int jj = 0;

        //        for (int i = 0; i<r; i++) {
        //            for (int j = 0; j<c; j++) {
        //                result[ii][jj] = array[i][j];
        //                jj++;
        //                if (jj == n) {
        //                    jj = 0;
        //                    ii++;
        //                }
        //            }
        //        }
        //        return result;
        //    }

        /**
         * Returns an int[] with the dimensions of the input.
         * @param inputArray
         * @return
         */
        //    public static int[] shape(Object inputArray)
        //{
        //    int nr = 1 + inputArray.getClass().getName().lastIndexOf('[');
        //    Object oa = inputArray;
        //    int[] l = new int[nr];
        //    for (int i = 0; i < nr; i++)
        //    {
        //        int len = l[i] = Array.getLength(oa);
        //        if (0 < len) { oa = Array.get(oa, 0); }
        //    }

        //    return l;
        //}

        /**
         * Sorts the array, then returns an array containing the indexes of
         * those sorted items in the original array.
         * <p>
         * int[] args = argsort(new int[] { 11, 2, 3, 7, 0 });
         * contains:
         * [4, 1, 2, 3, 0]
         * 
         * @param in
         * @return
         */
        //public static int[] argsort(int[] inp)
        //{
        //    return argsort(inp, -1, -1);
        //}

        /**
         * Sorts the array, then returns an array containing the indexes of
         * those sorted items in the original array which are between the
         * given bounds (start=inclusive, end=exclusive)
         * <p>
         * int[] args = argsort(new int[] { 11, 2, 3, 7, 0 }, 0, 3);
         * sorted = {0,2,3,7,11}
         * contains:
         * [4, 1, 2], which are indexes of [0, 2 and 3]
         * 
         * @param in
         * @return  the indexes of input elements filtered in the way specified
         * 
         * @see #argsort(int[])
         */
        //public static int[] argsort(int[] inp, int start, int end)
        //{
        //    var sorted = inp.OrderBy(e => e).ToArray();
        //    var final = new int[end - start];
        //    if (start == -1)
        //        start = 0;

        //    if (end == -1)
        //        end = inp.Length;

        //    for (int i = start; i < end; i++)
        //    {
        //        final[i] = Array.IndexOf(sorted, sorted[i]);
        //    }

        //    return final;
        //    //if (start == -1 || end == -1)
        //    //{
        //    //    Array.IndexOf()
        //    //    inp.OrderBy(e=>e).Select(e=> inp)
        //    //    return IntStream.of(inp).sorted().map(i->
        //    //        Arrays.stream(inp).boxed().collect(Collectors.toList()).indexOf(i)).toArray();
        //    //}

        //    //return IntStream.of(inp).sorted().map(i->
        //    //    Arrays.stream(inp).boxed().collect(Collectors.toList()).indexOf(i))
        //    //        .skip(start).limit(end).toArray();
        //}

        /**
        * Transforms 2D matrix of doubles to 1D by concatenation
        * @param A
        * @return
*/
        //public static double[] to1D(double[][] A)
        //{

        //    double[] B = new double[A.length * A[0].length];
        //    int index = 0;

        //    for (int i = 0; i < A.length; i++)
        //    {
        //        for (int j = 0; j < A[0].length; j++)
        //        {
        //            B[index++] = A[i][j];
        //        }
        //    }
        //    return B;
        //}

        /**
         * Transforms 2D matrix of integers to 1D by concatenation
         * @param A
         * @return
         */
        //public static int[] to1D(int[][] A)
        //{

        //    int[] B = new int[A.length * A[0].length];
        //    int index = 0;

        //    for (int i = 0; i < A.length; i++)
        //    {
        //        for (int j = 0; j < A[0].length; j++)
        //        {
        //            B[index++] = A[i][j];
        //        }
        //    }
        //    return B;
        //}

        /**
          * Returns a string representing an array of 0's and 1's
          *
          * @param arr an binary array (0's and 1's only)
          * @return
          */
        //public static String bitsToString(int[] arr)
        //{
        //    char[] s = new char[arr.Length + 1];

        //    for (int i = 0; i < s.Length; i++)
        //    {
        //        s[i] = '.';
        //    }

        //    s[0] = 'c';
        //    for (int i = 0; i < arr.Length; i++)
        //    {
        //        if (arr[i] == 1)
        //        {
        //            s[i + 1] = '*';
        //        }
        //    }

        //    return new String(s);
        //}

        /**
         * Return a list of tuples, where each tuple contains the i-th element
         * from each of the argument sequences.  The returned list is
         * truncated in length to the length of the shortest argument sequence.
         *
         * @param arg1 the first list to be the zero'th entry in the returned tuple
         * @param arg2 the first list to be the one'th entry in the returned tuple
         * @return a list of tuples
         */
        //public static List<Tuple<T1,T2>> Zip<T1,T2>(List<T1> arg1, List<T2> arg2)
        //{
        //    List<Tuple<T1, T2>> tuples = new List<Tuple<T1, T2>>();
        //    int len = Math.Min(arg1.Count, arg2.Count);
        //    for (int i = 0; i < len; i++)
        //    {
        //        tuples.Add(new Tuple<T1, T2>(arg1[i], arg2[i]));
        //    }

        //    return tuples;
        //}

        /**
         * Return a list of tuples, where each tuple contains the i-th element
         * from each of the argument sequences.  The returned list is
         * truncated in length to the length of the shortest argument sequence.
         *
         * @param args  the array of Objects to be wrapped in {@link Tuple}s
         * @return a list of tuples
         */
        //public static List<Tuple> zip(List<?>...args)
        //{
        //    List<Tuple> tuples = new ArrayList<Tuple>();

        //    int min = Arrays.stream(args).mapToInt(i->i.size()).min().orElse(0);

        //    int len = args.length;
        //    for (int j = 0; j < min; j++)
        //    {
        //        MutableTuple mt = new MutableTuple(len);
        //        for (int i = 0; i < len; i++)
        //        {
        //            mt.set(i, args[i].get(j));
        //        }
        //        tuples.add(mt);
        //    }

        //    return tuples;
        //}

        /**
         * Return a list of tuples, where each tuple contains the i-th element
         * from each of the argument sequences.  The returned list is
         * truncated in length to the length of the shortest argument sequence.
         *
         * @param args  the array of Objects to be wrapped in {@link Tuple}s
         * @return a list of tuples
         */
        //public static List<Tuple> zip(int[]... args)
        //{
        //    List<Tuple> tuples = new ArrayList<Tuple>();

        //    int min = Arrays.stream(args).mapToInt(i->i.length).min().orElse(0);

        //    int len = args.length;
        //    for (int j = 0; j < min; j++)
        //    {
        //        MutableTuple mt = new MutableTuple(len);
        //        for (int i = 0; i < len; i++)
        //        {
        //            mt.set(i, args[i][j]);
        //        }
        //        tuples.add(mt);
        //    }

        //    return tuples;
        //}

        /**
         * Return a list of tuples, where each tuple contains the i-th element
         * from each of the argument sequences.  The returned list is
         * truncated in length to the length of the shortest argument sequence.
         *
         * @param args  the array of Objects to be wrapped in {@link Tuple}s
         * @return a list of tuples
         */
        //public static List<Tuple> zip(Object[]... args)
        //{
        //    List<Tuple> tuples = new ArrayList<Tuple>();

        //    int min = Integer.MAX_VALUE;
        //    for (Object[] oa : args)
        //    {
        //        if (oa.length < min)
        //        {
        //            min = oa.length;
        //        }
        //    }

        //    int len = args.length;
        //    for (int j = 0; j < min; j++)
        //    {
        //        MutableTuple mt = new MutableTuple(2);
        //        for (int i = 0; i < len; i++)
        //        {
        //            mt.set(i, args[i][j]);
        //        }
        //        tuples.add(mt);
        //    }

        //    return tuples;
        //}

        /**
         * Returns an array with the same shape and the contents
         * converted to integers.
         *
         * @param doubs an array of doubles.
         * @return
         */
        public static int[] toIntArray(IDistributedArray doubs)
        {
            int[] retVal = new int[doubs.Count];
            for (int i = 0; i < doubs.Count; i++)
            {
                retVal[i] = (int)doubs[i];
            }
            return retVal;
        }

        /**
         * Returns an array with the same shape and the contents
         * converted to doubles.
         *
         * @param ints an array of ints.
         * @return
         */
        public static double[] toDoubleArray(IDistributedArray ints)
        {
            double[] retVal = new double[ints.Count];
            for (int i = 0; i < ints.Count; i++)
            {
                retVal[i] = (double)ints[i];
            }
            return retVal;
        }

        /**
         * Performs a modulus operation in Python style.
         *
         * @param a
         * @param b
         * @return
         */
        public static int modulo(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            if (a > 0 && b > 0 && b > a) return a;
            bool isMinus = Math.Abs(b - (a - b)) < Math.Abs(b - (a + b));
            if (isMinus)
            {
                while (a >= b)
                {
                    a -= b;
                }
            }
            else
            {
                if (a % b == 0) return 0;

                while (a + b < b)
                {
                    a += b;
                }
            }
            return a;
        }

        /**
         * Performs a modulus on every index of the first argument using
         * the second argument and places the result in the same index of
         * the first argument.
         *
         * @param a
         * @param b
         * @return
         */
        public static int[] modulo(int[] a, int b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = modulo(a[i], b);
            }
            return a;
        }

        /**
         * Returns a double array whose values are the maximum of the value
         * in the array and the max value argument.
         * @param doubs
         * @param maxValue
         * @return
         */
        public static double[] maximum(double[] doubs, double maxValue)
        {
            double[] retVal = new double[doubs.Length];
            for (int i = 0; i < doubs.Length; i++)
            {
                retVal[i] = Math.Max(doubs[i], maxValue);
            }
            return retVal;
        }

        /**
         * Returns an array of identical shape containing the maximum
         * of the values between each corresponding index. Input arrays
         * must be the same length.
         *
         * @param arr1
         * @param arr2
         * @return
         */
        public static int[] maxBetween(int[] arr1, int[] arr2) // DIstribution not needed.
        {
            int[] retVal = new int[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                retVal[i] = Math.Max(arr1[i], arr2[i]);
            }
            return retVal;
        }

        /**
         * Returns an array of identical shape containing the minimum
         * of the values between each corresponding index. Input arrays
         * must be the same length.
         *
         * @param arr1
         * @param arr2
         * @return
         */
        public static int[] minBetween(int[] arr1, int[] arr2)
        {
            int[] retVal = new int[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                retVal[i] = Math.Min(arr1[i], arr2[i]);
            }
            return retVal;
        }

        /**
         * Returns an array of values that test true for all of the
         * specified {@link Condition}s.
         *
         * @param values
         * @param conditions
         * @return
         */
        //public static int[] retainLogicalAnd(int[] values, Condition<?>[] conditions)
        //{
        //    TIntArrayList l = new TIntArrayList();
        //    for (int i = 0; i < values.length; i++)
        //    {
        //        boolean result = true;
        //        for (int j = 0; j < conditions.length && result; j++)
        //        {
        //            result &= conditions[j].eval(values[i]);
        //        }
        //        if (result) l.add(values[i]);
        //    }
        //    return l.toArray();
        //}

        /**
         * Returns an array of values that test true for all of the
         * specified {@link Condition}s.
         *
         * @param values
         * @param conditions
         * @return
         */
        //public static double[] retainLogicalAnd(double[] values, Condition<?>[] conditions)
        //{
        //    TDoubleArrayList l = new TDoubleArrayList();
        //    for (int i = 0; i < values.length; i++)
        //    {
        //        boolean result = true;
        //        for (int j = 0; j < conditions.length && result; j++)
        //        {
        //            result &= conditions[j].eval(values[i]);
        //        }
        //        if (result) l.add(values[i]);
        //    }
        //    return l.toArray();
        //}

        /**
         * Returns an array whose members are the quotient of the dividend array
         * values and the divisor array values.
         *
         * @param dividend
         * @param divisor
         * @param dividend adjustment
         * @param divisor  adjustment
         *
         * @return
         * @throws IllegalArgumentException if the two argument arrays are not the same length
         */
        public static double[] divide(double[] dividend, double[] divisor,
                                      double dividendAdjustment, double divisorAdjustment)
        {

            if (dividend.Length != divisor.Length)
            {
                throw new ArgumentException(
                        "The dividend array and the divisor array must be the same length");
            }
            double[] quotient = new double[dividend.Length];
            double denom = 1;
            for (int i = 0; i < dividend.Length; i++)
            {
                quotient[i] = (dividend[i] + dividendAdjustment) /
                              ((denom = divisor[i] + divisorAdjustment) == 0 ? 1 : denom); //Protect against division by 0
            }
            return quotient;
        }

        /**
         * Returns an array whose members are the quotient of the dividend array
         * values and the divisor array values.
         *
         * @param dividend
         * @param divisor
         * @param dividend adjustment
         * @param divisor  adjustment
         *
         * @return
         * @throws IllegalArgumentException if the two argument arrays are not the same length
         */
        public static double[] divide(int[] dividend, int[] divisor)
        {

            if (dividend.Length != divisor.Length)
            {
                throw new ArgumentException(
                        "The dividend array and the divisor array must be the same length");
            }
            double[] quotient = new double[dividend.Length];
            double denom = 1;
            for (int i = 0; i < dividend.Length; i++)
            {
                quotient[i] = (dividend[i]) /
                              (double)((denom = divisor[i]) == 0 ? 1 : denom); //Protect against division by 0
            }
            return quotient;
        }

        /**
         * Returns an array whose members are the quotient of the dividend array
         * values and the divisor value.
         *
         * @param dividend
         * @param divisor
         * @param dividend adjustment
         * @param divisor  adjustment
         *
         * @return
         * @throws IllegalArgumentException if the two argument arrays are not the same length
         */
        public static double[] divide(double[] dividend, double divisor)
        {
            double[] quotient = new double[dividend.Length];
            double denom = 1;
            for (int i = 0; i < dividend.Length; i++)
            {
                quotient[i] = (dividend[i]) /
                              (double)((denom = divisor) == 0 ? 1 : denom); //Protect against division by 0
            }
            return quotient;
        }

        /**
         * Returns an array whose members are the quotient of the dividend array
         * values and the divisor array values.
         *
         * @param dividend
         * @param divisor
         * @param dividend adjustment
         * @param divisor  adjustment
         * @return
         * @throws IllegalArgumentException if the two argument arrays are not the same length
         */
        //public static double[] roundDivide(double[] dividend, double[] divisor, int scale)
        //{

        //    if (dividend.Length != divisor.Length)
        //    {
        //        throw new ArgumentException(
        //                "The dividend array and the divisor array must be the same length");
        //    }
        //    double[] quotient = new double[dividend.Length];
        //    for (int i = 0; i < dividend.Length; i++)
        //    {
        //        quotient[i] = (dividend[i]) / (divisor[i] == 0 ? 1 : divisor[i]); //Protect against division by 0
        //        quotient[i] = new BigDecimal(quotient[i]).round(new MathContext(scale, RoundingMode.HALF_UP)).doubleValue();
        //    }
        //    return quotient;
        //}

        /**
         * Returns an array whose members are the product of the multiplicand array
         * values and the factor array values.
         *
         * @param multiplicand
         * @param factor
         * @param multiplicandAdjustment
         * @param factorAdjustment
         *
         * @return
         * @throws IllegalArgumentException if the two argument arrays are not the same length
         */
        public static double[] multiply(
            double[] multiplicand, double[] factor, double multiplicandAdjustment, double factorAdjustment)
        {

            if (multiplicand.Length != factor.Length)
            {
                throw new ArgumentException(
                    "The multiplicand array and the factor array must be the same length");
            }
            double[] product = new double[multiplicand.Length];
            for (int i = 0; i < multiplicand.Length; i++)
            {
                product[i] = (multiplicand[i] + multiplicandAdjustment) * (factor[i] + factorAdjustment);
            }
            return product;
        }


        /// <summary>
        /// Gets index of item in array, which satisfies specified condition.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int[] IndexWhere2<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            List<int> retVal = new List<int>();
            int len = source.Count();
            int indx = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    retVal.Add(indx);

                indx++;
            }

            return retVal.ToArray();
        }

        /**
         * Returns an array whose members are the product of the multiplicand array
         * values and the factor array values.
         *
         * @param multiplicand
         * @param factor
         * @param multiplicand adjustment
         * @param factor       adjustment
         *
         * @return
         * @throws IllegalArgumentException if the two argument arrays are not the same length
         */
        public static double[] multiply(double[] multiplicand, int[] factor)
        {

            if (multiplicand.Length != factor.Length)
            {
                throw new ArgumentException(
                        "The multiplicand array and the factor array must be the same length");
            }
            double[] product = new double[multiplicand.Length];
            for (int i = 0; i < multiplicand.Length; i++)
            {
                product[i] = (multiplicand[i]) * (factor[i]);
            }
            return product;
        }

        /**
         * Returns a new array containing the result of multiplying
         * each index of the specified array by the 2nd parameter.
         *
         * @param array
         * @param d
         * @return
         */
        public static int[] multiply(int[] array, int d)
        {
            int[] product = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                product[i] = array[i] * d;
            }
            return product;
        }

        /**
         * Returns a new array containing the result of multiplying
         * each index of the specified array by the 2nd parameter.
         *
         * @param array
         * @param d
         * @return
         */
        public static double[] multiply(double[] array, double d)
        {
            double[] product = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                product[i] = array[i] * d;
            }
            return product;
        }

        /**
         * Returns an integer array containing the result of subtraction
         * operations between corresponding indexes of the specified arrays.
         *
         * @param minuend
         * @param subtrahend
         * @return
         */
        public static int[] subtract(int[] minuend, int[] subtrahend)
        {
            int[] retVal = new int[minuend.Length];
            for (int i = 0; i < minuend.Length; i++)
            {
                retVal[i] = minuend[i] - subtrahend[i];
            }
            return retVal;
        }

        /**
         * Subtracts the contents of the first argument from the last argument's list.
         *
         * <em>NOTE: Does not destroy/alter the argument lists. </em>
         *
         * @param minuend
         * @param subtrahend
         * @return
         */
        //public static List<Integer> subtract(List<Integer> subtrahend, List<Integer> minuend)
        //{
        //    return IntStream.range(0, minuend.size())
        //       .boxed()
        //       .map(i->minuend.get(i) - subtrahend.get(i))
        //       .collect(Collectors.toList());
        //}

        /**
         * Returns the average of all the specified array contents.
         * @param arr
         * @return
         */
        public static double average(int[] arr)
        {
            int sum = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            return sum / (double)arr.Length;
        }

        /**
         * Returns the average of all the specified array contents.
         * @param arr
         * @return
         */
        public static double average(double[] arr)
        {
            double sum = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            return sum / (double)arr.Length;
        }

        /**
         * Computes and returns the variance.
         * @param arr
         * @param mean
         * @return
         */
        public static double variance(double[] arr, double mean)
        {
            double accum = 0.0;
            double dev = 0.0;
            double accum2 = 0.0;
            for (int i = 0; i < arr.Length; i++)
            {
                dev = arr[i] - mean;
                accum += dev * dev;
                accum2 += dev;
            }

            double var = (accum - (accum2 * accum2 / arr.Length)) / arr.Length;

            return var;
        }

        /**
         * Computes and returns the variance.
         * @param arr
         * @return
         */
        public static double variance(double[] arr)
        {
            return variance(arr, average(arr));
        }

        /**
         * Returns the passed in array with every value being altered
         * by the addition of the specified amount.
         *
         * @param arr
         * @param amount
         * @return
         */
        public static int[] add(int[] arr, int amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount;
            }
            return arr;
        }

        /**
         * Returns the passed in array with every value being altered
         * by the addition of the specified double amount at the same
         * index
         *
         * @param arr
         * @param amount
         * @return
         */
        public static int[] i_add(int[] arr, int[] amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount[i];
            }
            return arr;
        }

        /**
         * Returns the passed in array with every value being altered
         * by the addition of the specified double amount at the same
         * index
         *
         * @param arr
         * @param amount
         * @return
         */
        public static double[] d_add(double[] arr, double[] amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount[i];
            }
            return arr;
        }

        /**
         * Returns the passed in array with every value being altered
         * by the addition of the specified double amount
         *
         * @param arr
         * @param amount
         * @return
         */
        public static double[] d_add(double[] arr, double amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount;
            }
            return arr;
        }

        /**
         * Returns the sum of all contents in the specified array.
         * @param array
         * @return
         */
        public static int sum(int[] array)
        {
            int sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }

        /**
         * Test whether each element of a 1-D array is also present in a second 
         * array.
         *
         * Returns a int array whose length is the number of intersections.
         * 
         * @param ar1   the array of values to find in the second array 
         * @param ar2   the array to test for the presence of elements in the first array.
         * @return  an array containing the intersections or an empty array if none are found.
         */
        //public static int[] in1d(int[] ar1, int[] ar2)
        //{
        //    if (ar1 == null || ar2 == null)
        //    {
        //        return EMPTY_ARRAY;
        //    }


        //    TIntSet retVal = new TIntHashSet(ar2);
        //    retVal.retainAll(ar1);
        //    return retVal.toArray();
        //}

        /**
         * Returns the sum of all contents in the specified array.
         * @param array
         * @return
         */
        public static double sum(double[] array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }

        /**
         * Sparse or due to the arrays containing the indexes of "on bits",
         * the <em>or</em> of which is equal to the mere combination of the two
         * arguments - eliminating duplicates and sorting.
         *
         * @param arg1
         * @param arg2
         * @return
         */
        //public static int[] sparseBinaryOr(int[] arg1, int[] arg2)
        //{
        //    HashSet<int> set = new HashSet<int>(arg1);

        //    TIntArrayList t = new TIntArrayList(arg1);
        //    t.addAll(arg2);
        //    return unique(t.toArray());
        //}

        /**
         * Prints the specified array to a returned String.
         *
         * @param aObject the array object to print.
         * @return the array in string form suitable for display.
         */
        //public static String print1DArray(Object aObject)
        //{
        //    if (aObject.getClass().isArray())
        //    {
        //        if (aObject instanceof Object[]) // can we cast to Object[]
        //    {
        //            return Arrays.toString((Object[])aObject);
        //        } else {  // we can't cast to Object[] - case of primitive arrays
        //            int length = Array.getLength(aObject);
        //            Object[] objArr = new Object[length];
        //            for (int i = 0; i < length; i++)
        //                objArr[i] = Array.get(aObject, i);
        //            return Arrays.toString(objArr);
        //        }
        //    }
        //    return "[]";
        //}

        /**
         * Another utility to account for the difference between Python and Java.
         * Here the modulo operator is defined differently.
         *
         * @param n
         * @param divisor
         * @return
         */
        public static double positiveRemainder(double n, double divisor)
        {
            if (n >= 0)
            {
                return n % divisor;
            }
            else
            {
                double val = divisor + (n % divisor);
                return val == divisor ? 0 : val;
            }
        }

        /**
         * Returns an array which starts from lowerBounds (inclusive) and
         * ends at the upperBounds (exclusive).
         *
         * @param lowerBounds
         * @param upperBounds
         * @return
         */
        public static int[] range(int lowerBounds, int upperBounds)
        {
            List<int> ints = new List<int>();
            for (int i = lowerBounds; i < upperBounds; i++)
            {
                ints.Add(i);
            }
            return ints.ToArray();
        }

        /**
         * Returns an array which starts from lowerBounds (inclusive) and
         * ends at the upperBounds (exclusive).
         *
         * @param lowerBounds the starting value
         * @param upperBounds the maximum value (exclusive)
         * @param interval    the amount by which to increment the values
         * @return
         */
        //public static double[] arange(double lowerBounds, double upperBounds, double interval)
        //{
        //    TDoubleList doubs = new TDoubleArrayList();
        //    for (double i = lowerBounds; i < upperBounds; i += interval)
        //    {
        //        doubs.add(i);
        //    }
        //    return doubs.toArray();
        //}

        /**
         * Returns an array which starts from lowerBounds (inclusive) and
         * ends at the upperBounds (exclusive).
         *
         * @param lowerBounds the starting value
         * @param upperBounds the maximum value (exclusive)
         * @param interval    the amount by which to increment the values
         * @return
         */
        //public static int[] xrange(int lowerBounds, int upperBounds, int interval)
        //{
        //    TIntList ints = new TIntArrayList();
        //    for (int i = lowerBounds; i < upperBounds; i += interval)
        //    {
        //        ints.add(i);
        //    }
        //    return ints.toArray();
        //}

        /**
         * Fisher-Yates implementation which shuffles the array contents.
         * 
         * @param array     the array of ints to shuffle.
         * @return shuffled array
         */
        public static int[] shuffle(int[] array)
        {
            int index;
            Random random = new Random(42);
            for (int i = array.Length - 1; i > 0; i--)
            {
                index = random.Next(i + 1);
                if (index != i)
                {
                    array[index] ^= array[i];
                    array[i] ^= array[index];
                    array[index] ^= array[i];
                }
            }
            return array;
        }

        /**
         * Replaces the range specified by "start" and "end" of "orig" with the 
         * array of replacement ints found in "replacement".
         * 
         * @param start         start index of "orig" to be replaced
         * @param end           end index of "orig" to be replaced
         * @param orig          the array containing entries to be replaced by "replacement"
         * @param replacement   the array of ints to put in "orig" in the indicated indexes
         * @return
         */
        public static int[] replace(int start, int end, int[] orig, int[] replacement)
        {
            for (int i = start, j = 0; i < end; i++, j++)
            {
                orig[i] = replacement[j];
            }
            return orig;
        }

        /**
         * Returns a new array containing the source array contents with 
         * substitutions from "substitutes" whose indexes reside in "substInds".
         * 
         * @param source        the original array
         * @param substitutes   the replacements whose indexes must be in substInds to be used.
         * @param substInds     the indexes of "substitutes" to replace in "source"
         * @return  a new array with the specified indexes replaced with "substitutes"
         */
        //public static int[] subst(int[] source, int[] substitutes, int[] substInds)
        //{
        //    List<Integer> l = Arrays.stream(substInds).boxed().collect(Collectors.toList());
        //    return IntStream.range(0, source.length).map(
        //        i->l.indexOf(i) == -1 ? source[i] : substitutes[i]).toArray();
        //}

        /**
         * Returns a sorted unique (dupicates removed) array of integers
         *
         * @param nums an unsorted array of integers with possible duplicates.
         * @return
         */
        public static int[] unique(int[] nums)
        {
            HashSet<int> set = new HashSet<int>(nums);
            int[] result = new int[set.Count];
            set.CopyTo(result);

            Array.Sort(result);
            return result;
        }

        /**
         * Helper Class for recursive coordinate assembling
         */
        private class CoordinateAssembler
        {
            readonly private int[] position;
            readonly private List<int[]> dimensions;
            readonly List<int[]> result = new List<int[]>();

            public static List<int[]> assemble(List<int[]> dimensions)
            {
                CoordinateAssembler assembler = new CoordinateAssembler(dimensions);
                assembler.process(dimensions.Count);
                return assembler.result;
            }

            private CoordinateAssembler(List<int[]> dimensions)
            {
                this.dimensions = dimensions;
                position = new int[dimensions.Count];
            }

            private void process(int level)
            {
                if (level == 0)
                {// terminating condition
                    int[] coordinates = new int[position.Length];
                    Array.Copy(position, 0, coordinates, 0, position.Length);
                    result.Add(coordinates);
                }
                else
                {// inductive condition
                    int index = dimensions.Count - level;
                    int[] currentDimension = dimensions[index];
                    for (int i = 0; i < currentDimension.Length; i++)
                    {
                        position[index] = currentDimension[i];
                        process(level - 1);
                    }
                }
            }
        }


        /**
         * Called to merge a list of dimension arrays into a sequential row-major indexed
         * list of coordinates.
         *
         * @param dimensions a list of dimension arrays, each array being a dimension
         *                   of an n-dimensional array.
         * @return a list of n-dimensional coordinates in row-major format.
         */
        public static List<int[]> dimensionsToCoordinateList(List<int[]> dimensions)
        {
            return CoordinateAssembler.assemble(dimensions);
        }

        /**
         * Sets the values in the specified values array at the indexes specified,
         * to the value "setTo".
         *
         * @param values  the values to alter if at the specified indexes.
         * @param indexes the indexes of the values array to alter
         * @param setTo   the value to set at the specified indexes.
         */
        public static void setIndexesTo(double[] values, int[] indexes, double setTo)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                values[indexes[i]] = setTo;
            }
        }

        /**
         * Sets the values in the specified values array at the indexes specified,
         * to the value "setTo".
         *
         * @param values  the values to alter if at the specified indexes.
         * @param indexes the indexes of the values array to alter
         * @param setTo   the value to set at the specified indexes.
         */
        public static void setIndexesTo(int[] values, int[] indexes, int setTo)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                values[indexes[i]] = setTo;
            }
        }

        /**
         * Sets the values in range start to stop to the value specified. If
         * stop &lt; 0, then stop indicates the number of places counting from the
         * length of "values" back.
         *
         * @param values the array to alter
         * @param start  the start index (inclusive)
         * @param stop   the end index (exclusive)
         * @param setTo  the value to set the indexes to
         */
        public static void setRangeTo(int[] values, int start, int stop, int setTo)
        {
            stop = stop < 0 ? values.Length + stop : stop;
            for (int i = start; i < stop; i++)
            {
                values[i] = setTo;
            }
        }

        /**
         * Returns a random, sorted, and  unique array of the specified sample size of
         * selections from the specified list of choices.
         *
         * @param sampleSize the number of selections in the returned sample
         * @param choices    the list of choices to select from
         * @param random     a random number generator
         * @return a sample of numbers of the specified size
         */
        //public static int[] sample(TIntArrayList choices, int[] selectedIndices, Random random)
        //{
        //    TIntArrayList choiceSupply = new TIntArrayList(choices);
        //    int upperBound = choices.size();
        //    for (int i = 0; i < selectedIndices.length; i++)
        //    {
        //        int randomIdx = random.nextInt(upperBound);
        //        selectedIndices[i] = (choiceSupply.removeAt(randomIdx));
        //        upperBound--;
        //    }
        //    Arrays.sort(selectedIndices);
        //    //System.out.println("sample: " + Arrays.toString(selectedIndices));
        //    return selectedIndices;
        //}

        /**
         * Returns a random, sorted, and  unique array of the specified sample size of
         * selections from the specified list of choices.
         *
         * @param sampleSize the number of selections in the returned sample
         * @param choices    the list of choices to select from
         * @param random     a random number generator
         * @return a sample of numbers of the specified size
         */
        public static int[] sample(int[] choices, int[] selectedIndices, Random random)
        {
            // TIntArrayList choiceSupply = new TIntArrayList(choices);
            List<int> choiceSupply = new List<int>(choices);
            int upperBound = choices.Length;
            for (int i = 0; i < selectedIndices.Length; i++)
            {
                int randomIdx = random.Next(upperBound);
                selectedIndices[i] = choiceSupply[randomIdx];
                choiceSupply.RemoveAt(randomIdx);
                upperBound--;
            }
            return selectedIndices.OrderBy(i => i).ToArray();
        }

        /**
         * Returns a double[] filled with random doubles of the specified size.
         * @param sampleSize
         * @param random
         * @return
         */
        public static double[] sample(int sampleSize, Random random)
        {
            double[] sample = new double[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                sample[i] = random.NextDouble();
            }
            return sample;
        }

        /**
         * Ensures that each entry in the specified array has a min value
         * equal to or greater than the specified min and a maximum value less
         * than or equal to the specified max.
         * For example, if min = 0, then negative permanence values will be rounded to 0.
         * Similarly, high permanences will be rounded by maximal value.
         *
         * @param values the values to clip
         * @param min    the minimum value
         * @param max    the maximum value
         */


        public static double[] clip(double[] values, double min, double max)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Min(1, Math.Max(0, values[i]));
            }
            return values;
        }

        /**
         * Ensures that each entry in the specified array has a min value
         * equal to or greater than the min at the specified index and a maximum value less
         * than or equal to the max at the specified index.
         *
         * @param values the values to clip
         * @param min    the minimum value
         * @param max    the maximum value
         */
        public static int[] clip(int[] values, int[] min, int[] max)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Max(min[i], Math.Min(max[i], values[i]));
            }
            return values;
        }

        /**
         * Ensures that each entry in the specified array has a min value
         * equal to or greater than the min at the specified index and a maximum value less
         * than or equal to the max at the specified index.
         *
         * @param values the values to clip
         * @param max    the minimum value
         * @param adj    the adjustment amount
         */
        public static int[] clip(int[] values, int[] max, int adj)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Max(0, Math.Min(max[i] + adj, values[i]));
            }
            return values;
        }

        /**
         * Returns the count of values in the specified array that are
         * greater than the specified compare value
         *
         * @param compare the value to compare to
         * @param array   the values being compared
         *
         * @return the count of values greater
         */
        public static int valueGreaterCount(double compare, double[] array)
        {
            int count = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > compare)
                {
                    count++;
                }
            }

            return count;
        }

        /**
         * Returns the count of values in the specified array that are
         * greater than or equal to, the specified compare value.
         *
         * @param compare the value to compare to
         * @param array   the values being compared
         *
         * @return the count of values greater
         */
        public static int valueGreaterOrEqualCount(double compare, double[] array)
        {
            int count = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] >= compare)
                {
                    count++;
                }
            }

            return count;
        }

        /**
         * Returns the number of values in the specified array that are greater than the specified 'compare' value.
         *
         * @param compare the value to compare to
         * @param array   the values being compared
         *
         * @return the count of values greater
         */
        public static int valueGreaterCountAtIndex(double compare, double[] array, int[] indexes)
        {
            int count = 0;
            for (int i = 0; i < indexes.Length; i++)
            {
                if (array[indexes[i]] > compare)
                {
                    count++;
                }
            }

            return count;
        }

        /**
         * Returns an array containing the n greatest values.
         * @param array
         * @param n
         * @return
         */
        public static int[] nGreatest(double[] array, int n)
        {
            Dictionary<double, int> places = new Dictionary<double, int>();
            int i;
            double key;
            for (int j = 1; j < array.Length; j++)
            {
                key = array[j];
                for (i = j - 1; i >= 0 && array[i] < key; i--)
                {
                    array[i + 1] = array[i];
                }
                array[i + 1] = key;
                places.Add(key, j);
            }

            int[] retVal = new int[n];
            for (i = 0; i < n; i++)
            {
                retVal[i] = places[array[i]];
            }
            return retVal;
        }

        /**
         * Raises the values in the specified array by the amount specified
         * @param amount the amount to raise the values
         * @param values the values to raise
         */
        public static void raiseValuesBy(double amount, double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] += amount;
            }
        }

        /**
         * Raises the values at the indexes specified by the amount specified.
         * @param amount the amount to raise the values
         * @param values the values to raise
         */
        public static void raiseValuesBy(double amount, double[] values, int[] indexesToRaise)
        {
            for (int i = 0; i < indexesToRaise.Length; i++)
            {
                values[indexesToRaise[i]] += amount;
            }
        }

        /**
         * Raises the values at the indexes specified by the amount specified.
         * @param amounts the amounts to raise the values
         * @param values the values to raise
         */
        public static void raiseValuesBy(double[] amounts, double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] += amounts[i];
            }
        }

        /**
         * Raises the values at the indicated indexes, by the amount specified
         *
         * @param amount
         * @param indexes
         * @param values
         */
        public static void raiseValuesBy(int amount, int[] indexes, int[] values)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                values[indexes[i]] += amount;
            }
        }

        /**
         * Scans the specified values and applies the {@link Condition} to each
         * value, returning the indexes of the values where the condition evaluates
         * to true.
         *
         * @param values the values to test
         * @param c      the condition used to test each value
         * @return
         */
        //public static <T> int[] where(double[] values, Condition<T> c)
        //{
        //    TIntArrayList retVal = new TIntArrayList();
        //    int len = values.length;
        //    for (int i = 0; i < len; i++)
        //    {
        //        if (c.eval(values[i]))
        //        {
        //            retVal.add(i);
        //        }
        //    }
        //    return retVal.toArray();
        //}

        /**
         * Scans the specified values and applies the {@link Condition} to each
         * value, returning the indexes of the values where the condition evaluates
         * to true.
         *
         * @param values the values to test
         * @param c      the condition used to test each value
         * @return
         */
        //public static <T> int[] where(int[] values, Condition<T> c)
        //{
        //    TIntArrayList retVal = new TIntArrayList();
        //    int len = values.length;
        //    for (int i = 0; i < len; i++)
        //    {
        //        if (c.eval(values[i]))
        //        {
        //            retVal.add(i);
        //        }
        //    }
        //    return retVal.toArray();
        //}

        /**
         * Returns a flag indicating whether the specified array
         * is a sparse array of 0's and 1's or not.
         * 
         * @param ia
         * @return
         */
        public static bool isSparse(int[] ia)
        {
            if (ia == null || ia.Length < 3)
                return false;

            int end = ia[ia.Length - 1];

            for (int i = ia.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (ia[i] > 1)
                    return true;
                else if (j > 0 && ia[i] == end)
                    return false;
            }

            return false;
        }

        /**
         * Returns a bit vector of the specified size whose "on" bit
         * indexes are specified in "in"; basically converting a sparse
         * array to a dense one.
         * 
         * @param inp       the sparse array specifying the on bits of the returned array
         * @param size    the size of the dense array to be returned.
         * @return
         */
        //public static int[] asDense(int[] inp, int size)
        //{
        //    int[] retVal = new int[size];
        //    for (int i = 0; i < inp.Length; i++)
        //    {
        //        retVal[i] = 1;
        //    }
        //    return retVal;
        //}

        /**
         * Scans the specified values and applies the {@link Condition} to each
         * value, returning the indexes of the values where the condition evaluates
         * to true.
         *
         * @param values the values to test
         * @param c      the condition used to test each value
         * @return
         */
        //public static <T> int[] where(List<T> values, Condition<T> c)
        //{
        //    TIntArrayList retVal = new TIntArrayList();
        //    int len = values.size();
        //    for (int i = 0; i < len; i++)
        //    {
        //        if (c.eval(values.get(i)))
        //        {
        //            retVal.add(i);
        //        }
        //    }
        //    return retVal.toArray();
        //}

        /**
         * Scans the specified values and applies the {@link Condition} to each
         * value, returning the indexes of the values where the condition evaluates
         * to true.
         *
         * @param values the values to test
         * @param c      the condition used to test each value
         * @return
         */
        //public static <T> int[] where(T[] values, Condition<T> c)
        //{
        //    TIntArrayList retVal = new TIntArrayList();
        //    for (int i = 0; i < values.length; i++)
        //    {
        //        if (c.eval(values[i]))
        //        {
        //            retVal.add(i);
        //        }
        //    }
        //    return retVal.toArray();
        //}

        /**
         * Makes all values in the specified array which are less than or equal to the specified
         * "x" value, equal to the specified "y".
         * @param array
         * @param x     the comparison
         * @param y     the value to set if the comparison fails
         */
        public static void lessThanOrEqualXThanSetToY(IDistributedArray array, double x, double y)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if ((double)array[i] <= x) array[i] = y;
            }
        }

        /**
         * Makes all values in the specified array which are less than the specified
         * "x" value, equal to the specified "y".
         * @param array
         * @param x     the comparison
         * @param y     the value to set if the comparison fails
         */
        public static void lessThanXThanSetToY(IDistributedArray array, double x, double y)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if ((double)array[i] < x) array[i] = y;
            }
        }

        /**
         * Makes all values in the specified array which are less than the specified
         * "x" value, equal to the specified "y".
         * @param array
         * @param x     the comparison
         * @param y     the value to set if the comparison fails
         */
        //public static void lessThanXThanSetToY(int[] array, int x, int y)
        //{
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        if (array[i] < x) array[i] = y;
        //    }
        //}

        /**
         * Makes all values in the specified array which are greater than or equal to the specified
         * "x" value, equal to the specified "y".
         * @param array
         * @param x     the comparison
         * @param y     the value to set if the comparison fails
         */
        //public static void greaterThanOrEqualXThanSetToY(double[] array, double x, double y)
        //{
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        if (array[i] >= x) array[i] = y;
        //    }
        //}

        /**
         * Makes all values in the specified array which are greater than the specified
         * "x" value, equal to the specified "y".
         *
         * @param array
         * @param x     the comparison
         * @param y     the value to set if the comparison fails
         */
        //public static void greaterThanXThanSetToY(double[] array, double x, double y)
        //{
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        if (array[i] > x) array[i] = y;
        //    }
        //}

        ///**
        // * Makes all values in the specified array which are greater than the specified
        // * "x" value, equal to the specified "y".
        // * @param array
        // * @param x     the comparison
        // * @param y     the value to set if the comparison fails
        // */
        //public static void greaterThanXThanSetToY(int[] array, int x, int y)
        //{
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        if (array[i] > x) array[i] = y;
        //    }
        //}

        /**
         * Sets value to "y" in "targetB" if the value in the same index in "sourceA" is bigger than "x".
         * @param sourceA array to compare elements with X
         * @param targetB array to set elements to Y
         * @param x     the comparison
         * @param y     the value to set if the comparison fails
         */
        public static void greaterThanXThanSetToYInB(IDistributedArray sourceA, IDistributedArray targetB, int x, double y)
        {
            for (int i = 0; i < sourceA.Count; i++)
            {
                if ((int)sourceA[i] > x)
                    targetB[i] = y;
            }
        }


        /**
         * Returns the index of the max value in the specified array
         * @param array the array to find the max value index in
         * @return the index of the max value
         */
        //public static int argmax(IDistributedArray array)
        //{
        //    int index = -1;
        //    int max = int.MinValue;
        //    for (int i = 0; i < array.Count; i++)
        //    {
        //        if ((int)array[i] > max)
        //        {
        //            max = (int)array[i];
        //            index = i;
        //        }
        //    }
        //    return index;
        //}

        /**
         * Returns a boxed Integer[] from the specified primitive array
         * @param ints      the primitive int array
         * @return
         */
        //public static Integer[] toBoxed(int[] ints)
        //{
        //    return IntStream.of(ints).boxed().collect(Collectors.toList()).toArray(new Integer[ints.length]);
        //}

        /**
         * Returns a boxed Double[] from the specified primitive array
         * @param doubles       the primitive double array
         * @return
         */
        //public static Double[] toBoxed(double[] doubles)
        //{
        //    return DoubleStream.of(doubles).boxed().collect(Collectors.toList()).toArray(new Double[doubles.length]);
        //}

        /**
         * Returns a byte array transformed from the specified boolean array.
         * @param input     the boolean array to transform to a byte array
         * @return          a byte array
         */
        //public static byte[] toBytes(bool[] input)
        //{
        //    byte[] toReturn = new byte[input.Length / 8];
        //    for (int entry = 0; entry < toReturn.Length; entry++)
        //    {
        //        for (int bit = 0; bit < 8; bit++)
        //        {
        //            if (input[entry * 8 + bit])
        //            {
        //                toReturn[entry] |= (byte)(128 >> bit);
        //            }
        //        }
        //    }

        //    return toReturn;
        //}

        /**
         * Converts an array of Integer objects to an array of its
         * primitive form.
         * 
         * @param doubs
         * @return
         */
        //public static int[] toPrimitive(Integer[] ints)
        //{
        //    int[] retVal = new int[ints.Length];
        //    for (int i = 0; i < retVal.Length; i++)
        //    {
        //        retVal[i] = ints[i].Value;
        //    }
        //    return retVal;
        //}

        /**
         * Converts an array of Double objects to an array of its
         * primitive form.
         * 
         * @param doubs
         * @return
         */
        //public static double[] toPrimitive(Double[] doubs)
        //{
        //    double[] retVal = new double[doubs.Length];
        //    for (int i = 0; i < retVal.Length; i++)
        //    {
        //        retVal[i] = doubs[i];
        //    }
        //    return retVal;
        //}

        /**
         * Returns the index of the max value in the specified array
         * @param array the array to find the max value index in
         * @return the index of the max value
         */
        //public static int argmax(double[] array)
        //{
        //    int index = -1;
        //    double max = Double.MinValue;
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        if (array[i] > max)
        //        {
        //            max = array[i];
        //            index = i;
        //        }
        //    }
        //    return index;
        //}

        /**
         * Returns the maximum value in the specified array
         * @param array
         * @return
         */
        public static int max(int[] array)
        {
            int max = int.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                }
            }
            return max;
        }

        /**
         * Returns the maximum value in the specified array
         * @param array
         * @return
         */
        public static double max(double[] array)
        {
            double max = Double.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                }
            }
            return max;
        }

        /**
         * Returns a new array containing the items specified from
         * the source array by the indexes specified.
         *
         * @param source
         * @param indexes
         * @return
         */
        public static double[] ListOfValuesByIndicies(double[] source, int[] indexes)
        {
            double[] retVal = new double[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal[i] = source[indexes[i]];
            }
            return retVal;
        }

        /**
         * Returns a new array containing the items specified from
         * the source array by the indexes specified.
         *
         * @param source
         * @param indexes
         * @return
         */
        public static int[] sub(int[] source, int[] indexes)
        {
            int[] retVal = new int[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal[i] = source[indexes[i]];
            }
            return retVal;
        }

        /**
         * Returns a new 2D array containing the items specified from
         * the source array by the indexes specified.
         *
         * @param source
         * @param indexes
         * @return
         */
        public static int[][] sub(int[][] source, int[] indexes)
        {
            int[][] retVal = new int[indexes.Length][];
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal[i] = source[indexes[i]];
            }
            return retVal;
        }

        /**
         * Returns the minimum value in the specified array
         * @param array
         * @return
         */
        public static int minInt(IDistributedArray array)
        {
            int min = int.MaxValue;
            for (int i = 0; i < array.Count; i++)
            {
                if ((int)array[i] < min)
                {
                    min = (int)array[i];
                }
            }
            return min;
        }

        /**
         * Returns the minimum value in the specified array
         * @param array
         * @return
         */
        public static double minDouble(IDistributedArray array)
        {
            double min = Double.MaxValue;
            for (int i = 0; i < array.Count; i++)
            {
                if ((double)array[i] < min)
                {
                    min = (double)array[i];
                }
            }
            return min;
        }

        /**
         * Returns a copy of the specified integer array in
         * reverse order
         *
         * @param d
         * @return
         */
        public static int[] reverse(int[] d)
        {
            int[] ret = new int[d.Length];
            for (int i = 0, j = d.Length - 1; j >= 0; i++, j--)
            {
                ret[i] = d[j];
            }
            return ret;
        }

        /**
         * Returns a copy of the specified double array in
         * reverse order
         *
         * @param d
         * @return
         */
        public static double[] reverse(double[] d)
        {
            double[] ret = new double[d.Length];
            for (int i = 0, j = d.Length - 1; j >= 0; i++, j--)
            {
                ret[i] = d[j];
            }
            return ret;
        }

        /**
         * Returns a new int array containing the or'd on bits of
         * both arg1 and arg2.
         *
         * @param arg1
         * @param arg2
         * @return
         */
        public static int[] or(int[] arg1, int[] arg2)
        {
            int[] retVal = new int[Math.Max(arg1.Length, arg2.Length)];
            for (int i = 0; i < arg1.Length; i++)
            {
                retVal[i] = arg1[i] > 0 || arg2[i] > 0 ? 1 : 0;
            }
            return retVal;
        }

        /**
         * Returns a new int array containing the and'd bits of
         * both arg1 and arg2.
         *
         * @param arg1
         * @param arg2
         * @return
         */
        public static int[] and(int[] arg1, int[] arg2)
        {
            int[] retVal = new int[Math.Max(arg1.Length, arg2.Length)];
            for (int i = 0; i < arg1.Length; i++)
            {
                retVal[i] = arg1[i] > 0 && arg2[i] > 0 ? 1 : 0;
            }
            return retVal;
        }

        /**
         * Copies the passed array <tt>original</tt>  into a new array except first element and returns it
         *
         * @param original the array from which a tail is taken
         * @return a new array containing the tail from the original array
         */
        public static int[] tail(int[] original)
        {
            int[] destination = new int[original.Length - 1];
            Array.Copy(original, 1, destination, 0, destination.Length);
            return destination;
        }

        /**
         * Set <tt></tt>value for <tt>array</tt> at specified position <tt>indexes</tt>
         *
         * @param array
         * @param value
         * @param indexes
         */
        public static void setValue(IDistributedArray array, int value, params int[] indexes)
        {
            array.SetValue(value, indexes);
            //if (indexes.Length == 1)
            //{
            //    ((int[])array)[indexes[0]] = value;
            //}
            //else
            //{
            //    setValue(Array.get(array, indexes[0]), value, tail(indexes));
            //}
        }

        /**
         * Get <tt>value</tt> for <tt>array</tt> at specified position <tt>indexes</tt>
         *
         * @param array
         * @param indexes
         */
        public static Object getValue(Array array, params int[] indexes)
        {
            return array.GetValue(indexes);
            //Array slice = array;
            //for (int i = 0; i < indexes.Length; i++)
            //{       
            //    slice = Array.get(slice, indexes[i]);
            //}

            //return slice;
        }


        /**
         *Assigns the specified int value to each element of the specified any dimensional array
         * of ints.
         * @param array
         * @param value
         */
        public static void fillArray(Object array, int value)
        {
            if (array is int[])
            {
                for (int i = 0; i < ((int[])array).Length; i++)
                {
                    ((int[])array)[i] = value;
                }
            }
            else if (array is double[])
            {
                for (int i = 0; i < ((double[])array).Length; i++)
                {
                    ((double[])array)[i] = value;
                }
            }
            else
            {
                //forea (Object agr in (Object[])array)
                //{
                //    fillArray(agr, value);
                //}
                throw new NotSupportedException();
            }
        }

        public static void fillArray(Object array, double value)
        {
            if (array is double[])
            {
                for (int i = 0; i < ((double[])array).Length; i++)
                {
                    ((double[])array)[i] = value;
                }
            }
            else if (array is int[])
            {
                throw new NotSupportedException();
            }
            else
            {
                //forea (Object agr in (Object[])array)
                //{
                //    fillArray(agr, value);
                //}
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Fills the array with specified value.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void Fill(Array array, object value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array.SetValue(value, i);
            }
        }


        public static void Fill(Array array, int value)
        {
            if (array is int[] || array is object[])
            {
                for (int i = 0; i < ((int[])array).Length; i++)
                {
                    ((int[])array)[i] = value;
                }
            }
            else
            {
                //forea (Object agr in (Object[])array)
                //{
                //    fillArray(agr, value);
                //}
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets the access to a row inside of multidimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        //public static T[] GetRow<T>(this T[,] array, int row)
        public static T[] GetRow<T>(this IDistributedArray array, int row)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            for (int i = 0; i < cols; i++)
            {
                result[i] = (T)array[row, i];
            }

            return result;
        }


        /// <summary>
        /// Gets the access to a row inside of multidimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T[] GetRowNotUsed<T>(this T[,] array, int row)
        {
            if (!typeof(T).IsPrimitive)
                throw new InvalidOperationException("Not supported for managed types.");

            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];
            int size = Marshal.SizeOf<T>();

            Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

            return result;
        }

        

        /**
         * Convert multidimensional array to readable String
         * @param array
         * @return String representation of array
         */
        //public static String intArrayToString(Object array)
        //{
        //    StringBuilder result = new StringBuilder();
        //    if (array is Object[]){
        //        result.Append(Arrays.deepToString((Object[])array));
        //    } else {
        //        //One dimension
        //        result.append(Arrays.toString((int[])array));
        //    }
        //    return result.toString();
        //}

        /**
         * Return True if all elements of the  <tt>values</tt> have evaluated to true with <tt>condition</tt>
         * @param values
         * @param condition
         * @param <T>
         * @return
         */
        //public static <T> boolean all(final int[] values, final Condition<T> condition)
        //{
        //    for (int element : values)
        //    {
        //        if (!condition.eval(element))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        /**
         * Concat arrays
         *
         * @return The concatenated array
         *
         * http://stackoverflow.com/a/784842
         */

        //public static <T> T[] concatAll(T[] first, T[]... rest)
        //    {
        //        int totalLength = first.length;
        //        for (T[] array : rest)
        //        {
        //            totalLength += array.length;
        //        }
        //        T[] result = Arrays.copyOf(first, totalLength);
        //        int offset = first.length;
        //        for (T[] array : rest)
        //        {
        //            System.arraycopy(array, 0, result, offset, array.length);
        //            offset += array.length;
        //        }
        //        return result;
        //    }

        /**
         * Concat int arrays
         *
         * @return The concatenated array
         *
         * http://stackoverflow.com/a/784842
         */
        //@SafeVarargs
        //public static int[] concatAll(int[] first, int[]... rest)
        //{
        //    int totalLength = first.length;
        //    for (int[] array : rest)
        //    {
        //        totalLength += array.length;
        //    }
        //    int[] result = Arrays.copyOf(first, totalLength);
        //    int offset = first.length;
        //    for (int[] array : rest)
        //    {
        //        System.arraycopy(array, 0, result, offset, array.length);
        //        offset += array.length;
        //    }
        //    return result;
        //}

        public static bool AreEqual<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2) 
        {          
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var pair in dict1)
            {
                TValue value;
                if (dict2.TryGetValue(pair.Key, out value))
                {
                    if (!EqualityComparer<TValue>.Default.Equals(pair.Value, value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
