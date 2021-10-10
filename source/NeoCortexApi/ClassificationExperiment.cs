// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi
{
    public class ClassificationExperiment<T> : IEquatable<ClassificationExperiment<T>>
    {
        /** Array of actual values */
        private T[] actualValues;

        /** Map of step count -to- probabilities */
        Dictionary<int, double[]> probabilities = new Dictionary<int, double[]>();

        /**
         * Utility method to copy the contents of a ClassifierResult.
         * 
         * @return  a copy of this {@code ClassifierResult} which will not be affected
         * by changes to the original.
         */
        public ClassificationExperiment<T> copy()
        {
            ClassificationExperiment<T> retVal = new ClassificationExperiment<T>();
            retVal.actualValues = (T[])actualValues.Clone();
            //retVal.actualValues = Arrays.copyOf(actualValues.CopyTo(, actualValues.length);
            //retVal.probabilities = new TIntObjectHashMap<double[]>(probabilities);
            retVal.probabilities = new Dictionary<int, double[]>(probabilities);

            return retVal;
        }

        /**
         * Returns the actual value for the specified bucket index
         * 
         * @param bucketIndex
         * @return
         */
        public T getActualValue(int bucketIndex)
        {
            if (actualValues == null || actualValues.Length < bucketIndex + 1)
            {
                return default(T);
            }
            return (T)actualValues[bucketIndex];
        }

        /**
         * Returns all actual values entered
         * 
         * @return  array of type &lt;T&gt;
         */
        public T[] getActualValues()
        {
            return actualValues;
        }

        /**
         * Sets the array of actual values being entered.
         * 
         * @param values
         * @param &lt;T&gt;[]	the value array type
         */
        public void setActualValues(T[] values)
        {
            actualValues = values;
        }

        /**
         * Returns a count of actual values entered
         * @return
         */
        public int getActualValueCount()
        {
            return actualValues.Length;
        }

        /**
         * Returns the probability at the specified index for the given step
         * @param step
         * @param bucketIndex
         * @return
         */
        public double getStat(int step, int bucketIndex)
        {
            return probabilities[step][bucketIndex];
        }

        /**
         * Sets the array of probabilities for the specified step
         * @param step
         * @param votes
         */
        public void setStats(int step, double[] votes)
        {
            probabilities[step] = votes;
        }

        /**
         * Returns the probabilities for the specified step
         * @param step
         * @return
         */
        public double[] getStats(int step)
        {
            return probabilities[step];
        }

        /**
         * Returns the input value corresponding with the highest probability
         * for the specified step.
         * 
         * @param step		the step key under which the most probable value will be returned.
         * @return
         */
        public T getMostProbableValue(int step)
        {
            int idx = -1;
            if (probabilities[step] == null || (idx = getMostProbableBucketIndex(step)) == -1)
            {
                return default(T);
            }
            return getActualValue(idx);
        }

        /**
         * Returns the bucket index corresponding with the highest probability
         * for the specified step.
         * 
         * @param step		the step key under which the most probable index will be returned.
         * @return			-1 if there is no such entry
         */
        public int getMostProbableBucketIndex(int step)
        {
            if (probabilities[step] == null) return -1;

            double max = 0;
            int bucketIdx = -1;
            int i = 0;
            foreach (double d in probabilities[step])
            {
                if (d > max)
                {
                    max = d;
                    bucketIdx = i;
                }
                ++i;
            }
            return bucketIdx;
        }

        /**
         * Returns the count of steps
         * @return
         */
        public int getStepCount()
        {
            return probabilities.Count;
        }

        /**
         * Returns the count of probabilities for the specified step
         * @param	the step indexing the probability values
         * @return
         */
        public int getStatCount(int step)
        {
            return probabilities[step].Length;
        }

        /**
         * Returns a set of steps being recorded.
         * @return
         */
        public int[] stepSet()
        {
            return probabilities.Keys.ToArray();
        }

        // @Override
        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + actualValues.GetHashCode();
            result = prime * result + ((probabilities == null) ? 0 : probabilities.GetHashCode());
            return result;
        }


        public bool Equals(ClassificationExperiment<T> obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            ClassificationExperiment<T> other = (ClassificationExperiment<T>)obj;
            if (!actualValues.SequenceEqual(other.actualValues))
                return false;
            if (probabilities == null)
            {
                if (other.probabilities != null)
                    return false;
            }
            else
            {
                foreach (int key in probabilities.Keys)
                {
                    if (!probabilities[key].SequenceEqual((double[])other.probabilities[key]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
