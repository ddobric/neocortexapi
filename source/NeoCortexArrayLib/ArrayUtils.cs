// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace NeoCortexApi.Utility
{
    /// <summary>
    /// Utilities to match some of the functionality found in Python's Numpy.
    /// </summary>
    /// <remarks>author David Ray</remarks>
    public static class ArrayUtils
    {
        ///// <summary>
        ///// Empty array constant
        ///// </summary>
        //private static int[] EMPTY_ARRAY = new int[0];

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static string ArrToString(this int[] arr)
        {
            var value = string.Join(",", arr.Select(x => x.ToString()).ToArray());
            return value;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static string ArrToString(this int[][] arr)
        {
            var value = string.Join(" [ ", arr.Select(x => x.ArrToString()).ToArray(), " ] ");
            return value;
        }


        /// <summary>
        /// Returns the product of each integer in the specified array.
        /// </summary>
        /// <param name="dims"></param>
        /// <returns></returns>
        public static int Product(int[] dims)
        {
            int retVal = 1;
            for (int i = 0; i < dims.Length; i++)
            {
                retVal *= dims[i];
            }

            return retVal;
        }


        /// <summary>
        /// Returns an array with the same shape and the contents converted to integers.
        /// </summary>
        /// <param name="doubs">an array of doubles.</param>
        /// <returns></returns>
        public static int[] ToIntArray(double[] doubs)
        {
            int[] retVal = new int[doubs.Length];
            for (int i = 0; i < doubs.Length; i++)
            {
                retVal[i] = (int)doubs[i];
            }
            return retVal;
        }

        /// <summary>
        /// Returns an array with the same shape and the contents converted to doubles.
        /// </summary>
        /// <param name="ints">an array of ints.</param>
        /// <returns></returns>
        public static double[] ToDoubleArray(int[] ints)
        {
            double[] retVal = new double[ints.Length];
            for (int i = 0; i < ints.Length; i++)
            {
                retVal[i] = ints[i];
            }
            return retVal;
        }


        /// <summary>
        /// Performs a modulus operation in Python style.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Modulo.</returns>
        public static int Modulo(int a, int b)
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

        /// <summary>
        /// Performs a modulus on every index of the first argument using the second argument and places the result in the same index of
        /// the first argument.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int[] Modulo(int[] a, int b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = Modulo(a[i], b);
            }
            return a;
        }

        /// <summary>
        /// Returns a double array whose values are the maximum of the value in the array and the max value argument.
        /// </summary>
        /// <param name="doubs"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static double[] Maximum(double[] doubs, double maxValue)
        {
            double[] retVal = new double[doubs.Length];
            for (int i = 0; i < doubs.Length; i++)
            {
                retVal[i] = Math.Max(doubs[i], maxValue);
            }
            return retVal;
        }

        /// <summary>
        /// Returns an array of identical shape containing the maximum of the values between each corresponding index.Input arrays
        /// must be the same length.
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public static int[] MaxBetween(int[] arr1, int[] arr2)
        {
            int[] retVal = new int[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                retVal[i] = Math.Max(arr1[i], arr2[i]);
            }
            return retVal;
        }

        /// <summary>
        /// Returns an array of identical shape containing the minimum of the values between each corresponding index. Input arrays
        /// must be the same length.
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public static int[] MinBetween(int[] arr1, int[] arr2)
        {
            int[] retVal = new int[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                retVal[i] = Math.Min(arr1[i], arr2[i]);
            }
            return retVal;
        }

        /// <summary>
        /// TODO-Do we need this method here???
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<int> ReadCsvFileTest(String path)
        {
            string fileContent = File.ReadAllText(path);
            string[] integerStrings = fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> intList = new List<int>();
            for (int n = 0; n < integerStrings.Length; n++)
            {
                String s = integerStrings[n];
                char[] sub = s.ToCharArray();
                for (int j = 0; j < sub.Length; j++)
                {
                    intList.Add(int.Parse(sub[j].ToString()));
                }
            }
            return intList;
        }


        /// <summary>
        /// Returns an array whose members are the quotient of the dividend array values and the divisor array values.
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="dividendAdjustment"></param>
        /// <param name="divisorAdjustment"></param>
        /// <returns>throw ArgumentException if the two argument arrays are not the same length.</returns>
        public static double[] Divide(double[] dividend, double[] divisor,
                                      double dividendAdjustment, double divisorAdjustment)
        {

            if (dividend.Length != divisor.Length)
            {
                throw new ArgumentException(
                        "The dividend array and the divisor array must be the same length");
            }
            double[] quotient = new double[dividend.Length];
            double denom;
            for (int i = 0; i < dividend.Length; i++)
            {
                quotient[i] = (dividend[i] + dividendAdjustment) /
                              ((denom = divisor[i] + divisorAdjustment) == 0 ? 1 : denom); //Protect against division by 0
            }
            return quotient;
        }

        /// <summary>
        /// Returns an array whose members are the quotient of the dividend array values and the divisor array values.
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns>throw ArgumentException if the two argument arrays are not the same length.</returns>
        public static double[] Divide(int[] dividend, int[] divisor)
        {

            if (dividend.Length != divisor.Length)
            {
                throw new ArgumentException(
                        "The dividend array and the divisor array must be the same length");
            }
            double[] quotient = new double[dividend.Length];
            double denom;
            for (int i = 0; i < dividend.Length; i++)
            {
                quotient[i] = (dividend[i]) /
                              (double)((denom = divisor[i]) == 0 ? 1 : denom); //Protect against division by 0
            }
            return quotient;
        }

        /// <summary>
        /// Returns an array whose members are the quotient of the dividend array values and the divisor value.
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static double[] Divide(double[] dividend, double divisor)
        {
            double[] quotient = new double[dividend.Length];
            double denom;
            for (int i = 0; i < dividend.Length; i++)
            {
                quotient[i] = (dividend[i]) /
                              (double)((denom = divisor) == 0 ? 1 : denom); //Protect against division by 0
            }
            return quotient;
        }



        /// <summary>
        /// Returns an array whose members are the product of the multiplicand array values and the factor array values.
        /// </summary>
        /// <param name="multiplicand"></param>
        /// <param name="factor"></param>
        /// <param name="multiplicandAdjustment"></param>
        /// <param name="factorAdjustment"></param>
        /// <returns>throw ArgumentException if the two argument arrays are not the same length.</returns>
        public static double[] Multiply(
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
        public static int[] IndexWhere<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            List<int> retVal = new List<int>();
            //int len = source.Count();
            int indx = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    retVal.Add(indx);

                indx++;
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Gets index of item in array, which satisfies specified condition.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int[] IndexesWithNonZeros(this IEnumerable<int> source)
        {
            List<int> retVal = new List<int>();
            
            int indx = 0;
            foreach (var item in source)
            {
                if (item == 1)
                    retVal.Add(indx);

                indx++;
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Returns an array whose members are the product of the multiplicand array values and the factor array values.
        /// </summary>
        /// <param name="multiplicand"></param>
        /// <param name="factor"></param>
        /// <returns>throw ArgumentException if the two argument arrays are not the same length.</returns>
        public static double[] Multiply(double[] multiplicand, int[] factor)
        {
            if (multiplicand.Length != factor.Length)
            {
                throw new ArgumentException(
                        "The multiplicand array and the factor array must be the same length");
            }
            double[] product = new double[multiplicand.Length];
            for (int i = 0; i < multiplicand.Length; i++)
            {
                product[i] = multiplicand[i] * factor[i];
            }
            return product;
        }

        /// <summary>
        /// Returns a new array containing the result of multiplying each index of the specified array by the 2nd parameter.</summary>
        /// <param name="array"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int[] Multiply(int[] array, int d)
        {
            int[] product = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                product[i] = array[i] * d;
            }
            return product;
        }

        /// <summary>
        /// Returns a new array containing the result of multiplying each index of the specified array by the 2nd parameter.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double[] Multiply(double[] array, double d)
        {
            double[] product = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                product[i] = array[i] * d;
            }
            return product;
        }

        /// <summary>
        /// Returns an integer array containing the result of subtraction operations between corresponding indexes of the specified arrays.
        /// </summary>
        /// <param name="minuend"></param>
        /// <param name="subtrahend"></param>
        /// <returns></returns>
        public static int[] Subtract(int[] minuend, int[] subtrahend)
        {
            int[] retVal = new int[minuend.Length];
            for (int i = 0; i < minuend.Length; i++)
            {
                retVal[i] = minuend[i] - subtrahend[i];
            }
            return retVal;
        }



        /// <summary>
        /// Returns the average of all the specified array contents.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static double Average(int[] arr)
        {
            int sum = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            return sum / (double)arr.Length;
        }

        /// <summary>
        /// Returns the average of all the specified array contents.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static double Average(double[] arr)
        {
            if (arr.Length == 0)
                return 0;

            double sum = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            return sum / (double)arr.Length;
        }

        /// <summary>
        /// Computes and returns the variance.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        public static double Variance(double[] arr, double mean)
        {
            double accum = 0.0;
            double dev;
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

        /// <summary>
        /// Computes and returns the variance.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static double Variance(double[] arr)
        {
            return Variance(arr, Average(arr));
        }

        /// <summary>
        /// Returns the passed in array with every value being altered by the addition of the specified amount.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static int[] Add(int[] arr, int amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount;
            }
            return arr;
        }

        /// <summary>
        /// Returns the passed in array with every value being altered by the addition of the specified double amount at the same index.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static int[] I_add(int[] arr, int[] amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount[i];
            }
            return arr;
        }

        /// <summary>
        /// Returns the passed in array with every value being altered by the addition of the specified double amount at the same index.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static double[] AddOffset(double[] arr, double[] offset)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += offset[i];
            }
            return arr;
        }

        /// <summary>
        /// Returns the passed in array with every value being altered by the addition of the specified double amount.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double[] AddAmount(double[] arr, double amount)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += amount;
            }
            return arr;
        }

        /// <summary>
        /// Returns the sum of all contents in the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int Sum(int[] array)
        {
            int sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }


        /// <summary>
        /// Returns the sum of all contents in the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Sum(double[] array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }

        /// <summary>
        /// Another utility to account for the difference between Python and Java. Here the modulo operator is defined differently.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static double PositiveRemainder(double n, double divisor)
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

        /// <summary>
        /// Returns an array which starts from lowerBounds (inclusive) and ends at the upperBounds (exclusive).
        /// </summary>
        /// <param name="lowerBounds"></param>
        /// <param name="upperBounds"></param>
        /// <returns></returns>
        public static int[] Range(int lowerBounds, int upperBounds)
        {
            List<int> ints = new List<int>();
            for (int i = lowerBounds; i < upperBounds; i++)
            {
                ints.Add(i);
            }
            return ints.ToArray();
        }

        /// <summary>
        /// Fisher-Yates implementation which shuffles the array contents.
        /// </summary>
        /// <param name="array">the array of ints to shuffle.</param>
        /// <returns>shuffled array</returns>
        public static int[] Shuffle(int[] array)
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

        /// <summary>
        /// Suffles the list of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IList<T> Shuffle<T>(IList<T> array)
        {
            int index;
            Random random = new Random();
            for (int i = array.Count - 1; i > 0; i--)
            {
                index = random.Next(i + 1);
                if (index != i)
                {
                    T value = array[i];
                    array[i] = array[index];
                    array[index] = value;
                }
            }
            return array;
        }

        /// <summary>
        /// Replaces the range specified by "start" and "end" of "orig" with the array of replacement ints found in "replacement".
        /// </summary>
        /// <param name="start">start index of "orig" to be replaced.</param>
        /// <param name="end">end index of "orig" to be replaced.</param>
        /// <param name="orig">the array containing entries to be replaced by "replacement".</param>
        /// <param name="replacement">the array of ints to put in "orig" in the indicated indexes.</param>
        /// <returns></returns>
        public static int[] Replace(int start, int end, int[] orig, int[] replacement)
        {
            for (int i = start, j = 0; i < end; i++, j++)
            {
                orig[i] = replacement[j];
            }
            return orig;
        }


        /// <summary>
        /// Returns a sorted unique (dupicates removed) array of integers.
        /// </summary>
        /// <param name="array">an unsorted array of integers with possible duplicates.</param>
        /// <returns>sorted unique (dupicates removed) array of integers.</returns>
        public static int[] Unique(int[] array)
        {
            HashSet<int> set = new HashSet<int>(array);
            int[] result = new int[set.Count];
            set.CopyTo(result);

            Array.Sort(result);
            return result;
        }

        /// <summary>
        /// Helper Class for recursive coordinate assembling
        /// </summary>
        private class CoordinateAssembler
        {
            readonly private int[] position;
            readonly private List<int[]> dimensions;
            readonly List<int[]> result = new List<int[]>();

            /// <summary>
            /// TODO to be added
            /// </summary>
            /// <param name="dimensions"></param>
            /// <returns></returns>
            public static List<int[]> Assemble(List<int[]> dimensions)
            {
                CoordinateAssembler assembler = new CoordinateAssembler(dimensions);
                assembler.Process(dimensions.Count);
                return assembler.result;
            }

            /// <summary>
            /// TODO to be added
            /// </summary>
            /// <param name="dimensions"></param>
            private CoordinateAssembler(List<int[]> dimensions)
            {
                this.dimensions = dimensions;
                position = new int[dimensions.Count];
            }

            /// <summary>
            /// TODO to be added
            /// </summary>
            /// <param name="level"></param>
            private void Process(int level)
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
                        Process(level - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Called to merge a list of dimension arrays into a sequential row-major indexed list of coordinates.
        /// </summary>
        /// <param name="dimensions">a list of dimension arrays, each array being a dimension of an n-dimensional array.</param>
        /// <returns>a list of n-dimensional coordinates in row-major format.</returns>
        public static List<int[]> DimensionsToCoordinateList(List<int[]> dimensions)
        {
            return CoordinateAssembler.Assemble(dimensions);
        }

        /// <summary>
        /// Sets the values in the specified values array at the indexes specified,
        /// to the value "setTo".
        /// </summary>
        /// <param name="valuesToSet">The values to alter if at the specified indexes.</param>
        /// <param name="indexes">the indexes of the values array to alter.</param>
        /// <param name="val">the value to set at the specified indexes.</param>
        public static void SetIndexesTo(double[] valuesToSet, int[] indexes, double val)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                valuesToSet[indexes[i]] = val;
            }
        }

        /// <summary>
        /// Sets the values in the specified values array at the indexes specified, to the value <paramref name="val"/>.
        /// </summary>
        /// <param name="values">the values to alter if at the specified indexes.</param>
        /// <param name="indexes">the indexes of the values array to alter.</param>
        /// <param name="val">the value to set at the specified indexes.</param>
        public static void SetIndexesTo(int[] values, int[] indexes, int val)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                values[indexes[i]] = val;
            }
        }

        /// <summary>
        /// Sets the values in range start to stop to the value specified. If stop &lt; 0, then stop indicates the number of places
        /// counting from the length of <paramref name="values"/> back.
        /// </summary>
        /// <param name="values">the array to alter.</param>
        /// <param name="start">the start index (inclusive).</param>
        /// <param name="stop">the end index (exclusive).</param>
        /// <param name="setTo">the value to set the indexes to.</param>
        public static void SetRangeTo(int[] values, int start, int stop, int setTo)
        {
            stop = stop < 0 ? values.Length + stop : stop;
            for (int i = start; i < stop; i++)
            {
                values[i] = setTo;
            }
        }


        /// <summary>
        /// Returns a random, sorted, and  unique array of the specified sample size of selections from the specified list of choices.
        /// </summary>
        /// <param name="choices">the list of choices to select from.</param>
        /// <param name="selectedIndices">the number of selections in the returned sample.</param>
        /// <param name="random">a random number generator.</param>
        /// <returns>a sample of numbers of the specified size.</returns>
        public static int[] Sample(int[] choices, int[] selectedIndices, Random random)
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

        /// <summary>
        /// Returns a double[] filled with random doubles of the specified size.
        /// </summary>
        /// <param name="sampleSize"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static double[] Sample(int sampleSize, Random random)
        {
            double[] sample = new double[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                sample[i] = random.NextDouble();
            }
            return sample;
        }

        /// <summary>
        /// Ensures that each entry in the specified array has a min value equal to or greater than the specified min and a maximum value less
        /// than or equal to the specified max. For example, if min = 0, then negative permanence values will be rounded to 0.
        /// Similarly, high permanences will be rounded by maximal value.
        /// </summary>
        /// <param name="values">the values to clip.</param>
        /// <param name="min">the minimum value.</param>
        /// <param name="max">the maximum value.</param>
        /// <returns></returns>
        /// TODO min max ???
        public static double[] EnsureBetweenMinAndMax(double[] values, double min, double max)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Min(1, Math.Max(0, values[i]));
            }
            return values;
        }

        /// <summary>
        /// Ensures that each entry in the specified array has a min value equal to or greater than the min at the specified index
        /// and a maximum value less than or equal to the max at the specified index.
        /// </summary>
        /// <param name="values">the values to clip.</param>
        /// <param name="min">the minimum value.</param>
        /// <param name="max">the maximum value.</param>
        /// <returns></returns>
        public static int[] Clip(int[] values, int[] min, int[] max)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Max(min[i], Math.Min(max[i], values[i]));
            }
            return values;
        }

        /// <summary>
        /// Ensures that each entry in the specified array has a min value equal to or greater than the min at the specified index 
        /// and a maximum value less than or equal to the max at the specified index. 
        /// </summary>
        /// <param name="values">the values to clip.</param>
        /// <param name="max">the minimum value.</param>
        /// <param name="adj">the adjustment amount.</param>
        /// <returns></returns>
        public static int[] Clip(int[] values, int[] max, int adj)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Max(0, Math.Min(max[i] + adj, values[i]));
            }
            return values;
        }

        /// <summary>
        /// Returns the count of values in the specified array that are greater than the specified compare value.
        /// </summary>
        /// <param name="compare">the value to compare to.</param>
        /// <param name="array">the values being compared.</param>
        /// <returns>the count of values greater.</returns>
        public static int ValueGreaterCount(double compare, double[] array)
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

        /// <summary>
        /// Returns the count of values in the specified array that are greater than or equal to, the specified compare value.
        /// </summary>
        /// <param name="compare">the value to compare to.</param>
        /// <param name="array">the values being compared.</param>
        /// <returns></returns>
        public static int ValueGreaterOrEqualCount(double compare, double[] array)
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

        /// <summary>
        /// Returns the number of values in the specified array that are greater than the specified 'compare' value.
        /// </summary>
        /// <param name="compareValue">the value to compare to.</param>
        /// <param name="array">the values being compared.</param>
        /// <param name="indexes">indices of array being compared.</param>
        /// <returns>the count of values greater.</returns>
        public static int ValueGreaterThanCountAtIndex(double compareValue, double[] array, int[] indexes)
        {
            int count = 0;
            for (int i = 0; i < indexes.Length; i++)
            {
                if (array[indexes[i]] > compareValue)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Returns an array containing the n greatest values.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int[] NGreatest(double[] array, int n)
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

        /// <summary>
        /// Raises the values in the specified array by the amount specified.
        /// </summary>
        /// <param name="amount">the amount to raise the values.</param>
        /// <param name="values">the values to raise.</param>
        public static void RaiseValuesBy(double amount, double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] += amount;
            }
        }

        /// <summary>
        /// Raises the values at the indexes specified by the amount specified.
        /// </summary>
        /// <param name="amount">the amount to raise the values.</param>
        /// <param name="values">the values to raise.</param>
        /// <param name="indicesToRaise">indices of values to be raised.</param>
        public static void RaiseValuesBy(double amount, double[] values, int[] indicesToRaise)
        {
            for (int i = 0; i < indicesToRaise.Length; i++)
            {
                values[indicesToRaise[i]] += amount;
            }
        }

        /// <summary>
        /// Raises the values at the indexes specified by the amount specified.
        /// </summary>
        /// <param name="amounts">the amounts to raise the values.</param>
        /// <param name="values">the values to raise.</param>
        public static void RaiseValuesBy(double[] amounts, double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] += amounts[i];
            }
        }

        /// <summary>
        /// Raises the values at the indicated indexes, by the amount specified.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="indices"></param>
        /// <param name="values"></param>
        public static void RaiseValuesBy(int amount, int[] indices, int[] values)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                values[indices[i]] += amount;
            }
        }


        /// <summary>
        /// Returns a flag indicating whether the specified array is a sparse array of 0's and 1's or not.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsSparse(int[] array)
        {
            if (array == null || array.Length < 3)
                return false;

            int end = array[array.Length - 1];

            for (int i = array.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (array[i] > 1)
                    return true;
                else if (j > 0 && array[i] == end)
                    return false;
            }

            return false;
        }


        /// <summary>
        /// Makes all values in the specified array which are less than or equal to the specified <paramref name="comparingValue"/> value,
        /// equal to the specified <paramref name="valueToBeSet"/>.
        /// </summary>
        /// <param name="array">Traverses through this array and set the element on 'y' if the current value of the element is less than 'x'. </param>
        /// <param name="comparingValue">the comparison.</param>
        /// <param name="valueToBeSet">the value to set if the comparison fails.</param>
        public static void LessOrEqualXThanSetToY(double[] array, double comparingValue, double valueToBeSet)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] <= comparingValue) 
                    array[i] = valueToBeSet;
            }
        }


        /// <summary>
        /// Sets value to "y" in "targetB" if the value in the same index in "sourceA" is bigger than "x".
        /// </summary>
        /// <param name="sourceA">array to compare elements with X.</param>
        /// <param name="targetB">array to set elements to Y.</param>
        /// <param name="x">the comparison.</param>
        /// <param name="y">the value to set if the comparison fails.</param>
        public static void GreaterThanXThanSetToYInB(int[] sourceA, double[] targetB, int x, double y)
        {
            for (int i = 0; i < sourceA.Length; i++)
            {
                if (sourceA[i] > x)
                    targetB[i] = y;
            }
        }


        //
        //Returns the index of the max value in the specified array
        //@param array the array to find the max value index in
        //@return the index of the max value
        //
        //public static int argmax(int[] array)
        //{
        //    int index = -1;
        //    int max = Integer.MinValue;
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

        //
        //Returns a boxed Integer[] from the specified primitive array
        //@param ints      the primitive int array
        //@return
        //
        //public static Integer[] toBoxed(int[] ints)
        //{
        //    return IntStream.of(ints).boxed().collect(Collectors.toList()).toArray(new Integer[ints.length]);
        //}


        //Returns a boxed Double[] from the specified primitive array
        //@param doubles       the primitive double array
        //@return

        //public static Double[] toBoxed(double[] doubles)
        //{
        //    return DoubleStream.of(doubles).boxed().collect(Collectors.toList()).toArray(new Double[doubles.length]);
        //}


        //Returns a byte array transformed from the specified boolean array.
        //@param input     the boolean array to transform to a byte array
        //@return          a byte array

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


        //Converts an array of Integer objects to an array of its
        //primitive form.

        //@param doubs
        //@return

        //public static int[] toPrimitive(Integer[] ints)
        //{
        //    int[] retVal = new int[ints.Length];
        //    for (int i = 0; i < retVal.Length; i++)
        //    {
        //        retVal[i] = ints[i].Value;
        //    }
        //    return retVal;
        //}


        //Converts an array of Double objects to an array of its
        //primitive form.

        //@param doubs
        //@return

        //public static double[] toPrimitive(Double[] doubs)
        //{
        //    double[] retVal = new double[doubs.Length];
        //    for (int i = 0; i < retVal.Length; i++)
        //    {
        //        retVal[i] = doubs[i];
        //    }
        //    return retVal;
        //}


        //Returns the index of the max value in the specified array
        //@param array the array to find the max value index in
        //@return the index of the max value

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

        /// <summary>
        /// Returns the maximum value in the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int Max(int[] array)
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

        /// <summary>
        /// Returns the maximum value in the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Max(double[] array)
        {
            double max = double.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Returns a new array containing the items specified from the source array by the indexes specified.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static double[] ListOfValuesByIndicies(double[] source, int[] indexes)
        {
            double[] retVal = new double[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal[i] = source[indexes[i]];
            }
            return retVal;
        }

        /// <summary>
        /// Returns a new array containing the items specified from the source array by the indexes specified.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static int[] Sub(int[] source, int[] indices)
        {
            int[] retVal = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                retVal[i] = source[indices[i]];
            }
            return retVal;
        }

        /// <summary>
        /// Returns a new 2D array containing the items specified from the source array by the indexes specified.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static int[][] Sub(int[][] source, int[] indices)
        {
            int[][] retVal = new int[indices.Length][];
            for (int i = 0; i < indices.Length; i++)
            {
                retVal[i] = source[indices[i]];
            }
            return retVal;
        }

        /// <summary>
        /// Returns the minimum value in the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int Min(int[] array)
        {
            int min = int.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < min)
                {
                    min = array[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Returns the minimum value in the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Min(double[] array)
        {
            double min = double.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < min)
                {
                    min = array[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Returns a copy of the specified integer array in reverse order.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int[] Reverse(int[] array)
        {
            int[] ret = new int[array.Length];
            for (int i = 0, j = array.Length - 1; j >= 0; i++, j--)
            {
                ret[i] = array[j];
            }
            return ret;
        }

        /// <summary>
        /// Returns a copy of the specified double array in
        /// reverse order.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] Reverse(double[] array)
        {
            double[] ret = new double[array.Length];
            for (int i = 0, j = array.Length - 1; j >= 0; i++, j--)
            {
                ret[i] = array[j];
            }
            return ret;
        }

        /// <summary>
        /// Returns a new int array containing the or'd on bits of both arg1 and arg2.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static int[] Or(int[] arg1, int[] arg2)
        {
            int[] retVal = new int[Math.Max(arg1.Length, arg2.Length)];
            for (int i = 0; i < arg1.Length; i++)
            {
                retVal[i] = arg1[i] > 0 || arg2[i] > 0 ? 1 : 0;
            }
            return retVal;
        }

        /// <summary>
        /// Returns a new int array containing the and'd bits of both arg1 and arg2.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static int[] And(int[] arg1, int[] arg2)
        {
            int[] retVal = new int[Math.Max(arg1.Length, arg2.Length)];
            for (int i = 0; i < arg1.Length; i++)
            {
                retVal[i] = arg1[i] > 0 && arg2[i] > 0 ? 1 : 0;
            }
            return retVal;
        }

        /// <summary>
        /// Copies the passed array <tt>original</tt> into a new array except first element and returns it.
        /// </summary>
        /// <param name="original">the array from which a tail is taken.</param>
        /// <returns>a new array containing the tail from the original array.</returns>
        public static int[] Tail(int[] original)
        {
            int[] destination = new int[original.Length - 1];
            Array.Copy(original, 1, destination, 0, destination.Length);
            return destination;
        }


        /// <summary>
        /// Set <tt>value</tt> for <tt>array</tt> at specified position <tt>indexes</tt>.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="indexes"></param>
        public static void SetValue(Array array, int value, params int[] indexes)
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

        /// <summary>
        /// Get <tt>value</tt> for <tt>array</tt> at specified position <tt>indexes</tt>.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static object GetValue(Array array, params int[] indexes)
        {
            return array.GetValue(indexes);
            //Array slice = array;
            //for (int i = 0; i < indexes.Length; i++)
            //{       
            //    slice = Array.get(slice, indexes[i]);
            //}

            //return slice;
        }

        /// <summary>
        /// Assigns the specified int value to each element of the specified any dimensional array of ints.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void FillArray(Object array, int value)
        {
            if (array is int[] intArray)
            {
                for (int i = 0; i < intArray.Length; i++)
                {
                    intArray[i] = value;
                }
            }
            else if (array is double[] doubleArray)
            {
                for (int i = 0; i < doubleArray.Length; i++)
                {
                    doubleArray[i] = value;
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
        /// Initializes the array with the specific value.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        public static void InitArray<T>(T[] array, T val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = val;
            }
        }

        ///// <summary>
        ///// Fills all elements of the array with the given value.
        ///// </summary>
        ///// <param name="array"></param>
        ///// <param name="value"></param>
        //public static void FillArray(object array, double value)
        //{
        //    if (array is double[] doubleArray)
        //    {
        //        for (int i = 0; i < doubleArray.Length; i++)
        //        {
        //            doubleArray[i] = value;
        //        }
        //    }
        //    else if (array is int[])
        //    {
        //        throw new NotSupportedException();
        //    }
        //    else
        //    {
        //        throw new NotSupportedException();
        //    }
        //}


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

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
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
        public static T[] GetRow<T>(this T[,] array, int row)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            for (int i = 0; i < cols; i++)
            {
                //Console.WriteLine($"{i}");
                result[i] = array[row, i];
            }

            return result;
        }

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="array"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static int[] GetRow2(this int[,] array, int row)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            int[] result = new int[cols];

            for (int i = 0; i < cols; i++)
            {
                //Console.WriteLine($"{i}");
                result[i] = array[row, i];
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

        /// <summary>
        /// Aggregates all element of multi dimensional array of ints.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>sum of all array elements</returns>
        public static int AggregateArray(object array)
        {
            int sum = 0;
            if (array.GetType() == typeof(int))
            {
                return (int)array;
            }
            else if (array.GetType() == typeof(int[]))
            {
                int[] set = (int[])array;
                foreach (int element in set)
                {
                    sum += element;
                }
                return sum;
            }
            else
            {
                foreach (object agr in (object[])array)
                {
                    sum += AggregateArray(agr);
                }
                return sum;
            }
        }


        //Convert multidimensional array to readable String
        //@param array
        //@return String representation of array

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


        //Return True if all elements of the  <tt>values</tt> have evaluated to true with <tt>condition</tt>
        //@param values
        //@param condition
        //@param <T>
        //@return

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


        //Concat arrays

        //@return The concatenated array

        //http://stackoverflow.com/a/784842


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


        //Concat int arrays

        //@return The concatenated array

        //http://stackoverflow.com/a/784842

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

        /// <summary>
        /// TODO to be added
        /// </summary>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static bool AreEqual<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
        {
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var pair in dict1)
            {
                if (dict2.TryGetValue(pair.Key, out TValue value))
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

        /// <summary>
        /// This method is used to flip bits of an binary array.
        /// </summary>
        /// <param name="oriArr"></param>
        /// <param name="bitPerc"></param>
        /// <returns></returns>
        public static int[] FlipBit(int[] oriArr, double bitPerc)
        {
            int[] result = new List<int>(oriArr).ToArray();
            List<int> arr = new List<int>();
            Random random = new Random();
            int num;
            int numOfFlipBit = (int)(bitPerc * oriArr.Length);
            if (bitPerc == 1.0)
            {
                for (int i = 0; i < oriArr.Length; i++)
                {
                    if (result[i] == 1)
                    {
                        result[i] = 0;
                    }
                    else
                    {
                        result[i] = 1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < numOfFlipBit; i++)
                {
                    do
                    {
                        num = random.Next(0, oriArr.Length - 1);
                    }
                    while (arr.Contains(num));
                    arr.Add(num);
                    if (result[num] == 1)
                    {
                        result[num] = 0;
                    }
                    else
                    {
                        result[num] = 1;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove value At Index in Array
        /// </summary>
        /// <param name="IndicesArray"></param>
        /// <param name="RemoveAt"></param>
        /// <returns></returns>
        public static double[] RemoveIndices(double[] IndicesArray, int RemoveAt)
        {
            double[] newIndicesArray = new double[IndicesArray.Length - 1];

            int i = 0;
            int j = 0;
            while (i < IndicesArray.Length)
            {
                if (i != RemoveAt)
                {
                    newIndicesArray[j] = IndicesArray[i];
                    j++;
                }

                i++;
            }

            return newIndicesArray;
        }

        /// <summary>
        /// Convert 1D array to 2D array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static T[,] Make2DArray<T>(T[] input, int height, int width)
        {
            T[,] output = new T[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // This is required, because some input and output arrays are not alligned to each other.
                    // For example arraut of 2048 (= 45.35*45.25) will be rounded to height 46 * width 46. In that case input will be 2048 and output 2116.
                    // In that case  i * width + j will not exist.
                    if (input.Length > i * width + j)
                        output[i, j] = input[i * width + j];
                }
            }
            return output;
        }

        /// <summary>
        /// Transpose 2D array
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static int[,] Transpose(int[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            int[,] result = new int[h, w];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// Adds the new element to the list, which must contain the specified number of elements. Old elements are replaced.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="maxNumOfElements">Maximum allowed number of elements.</param>
        /// <param name="newElement">The new value.</param>
        public static int[] PushToInterval(int[] list, int maxNumOfElements, int newElement)
        {
            //
            // Shift all elements.
            for (int i = maxNumOfElements - 1; i > 0; i--)
            {
                //Shift the element to the next position.
                list[i] = list[i - 1];
            }

            list[0] = newElement;

            return list;
        }


        /// <summary>
        /// Adds the new array to the list of arrays. The list of arrays must contain the specified number of arrays. Old elements are replaced.
        /// </summary>
        /// <param name="listOfArrays">The list of arrays.</param>
        /// <param name="maxNumOfArrays">Maximum allowed number of elements.</param>
        /// <param name="newArray">The new value.</param>
        public static List<T[]> RememberArray<T>(List<T[]> listOfArrays, int maxNumOfArrays, T[] newArray)
        {
            listOfArrays.Insert(0, newArray);

            if (listOfArrays.Count > maxNumOfArrays)
                listOfArrays.RemoveAt(listOfArrays.Count - 1);

            return listOfArrays;
        }



        /// <summary>
        /// Calculates the average delta over the list. 1/N * (x2-x1 + X3-X2, + .. + XN-XN-1)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double AvgDelta(int[] list)
        {
            double avgDerivation;

            //
            // We calculate here the derivation of the the active column function across 
            // all inputs.
            double sum = 0;
            for (int i = 0; i < list.Length - 1; i++)
            {
                // Sum of derivations
                sum += Math.Abs(list[i] - list[i + 1]);
            }

            avgDerivation = sum / (double)list.Length;

            return avgDerivation;
        }


        /// <summary>
        /// Fills up the array with the specified value at the specified index.
        /// </summary>
        /// <param name="arr">Array of indexes at which the specified value has to be set.</param>
        /// <param name="arrSize">The size of resulting array.</param>
        /// <param name="val">The value to be set at index positions.</param>
        /// <returns></returns>
        public static int[] FillAtIndexes(int[] arr, int arrSize, int val)
        {
            int[] arrRes = new int[arrSize];

            foreach (var item in arr)
            {
                arrRes[item] = val;
            }

            return arrRes;
        }



    }
}
