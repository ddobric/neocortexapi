// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NeoCortexApi.Classifiers
{
    public class CLAClassifier<T> where T : struct
    {
        /// <summary>
        ///The alpha used to compute running averages of the bucket duty
        ///cycles for each activation pattern bit.A lower alpha results
        /// in longer term memory.
        /// </summary>
        private double alpha = 0.001;
        double actValueAlpha = 0.3;

        private readonly IFormatProvider enusFormatProvider = new CultureInfo("en-us").NumberFormat;

        /// <summary>
        /// The bit's learning iteration. This is updated each time store() gets called on this bit.
        /// </summary>
        int learnIteration;
        /// <summary>
        /// This contains the offset between the recordNum (provided by caller) and learnIteration(internal only, always starts at 0).
        /// </summary>
        int recordNumMinusLearnIteration = -1;
        /// <summary>
        ///This contains the value of the highest bucket index we've ever seen
        ///It is used to pre-allocate fixed size arrays that hold the weights of
        ///each bucket index during inference
        /// </summary>
        int maxBucketIdx;
        /// <summary>
        /// The sequence different steps of multi-step predictions
        /// </summary>
        IList<int> steps = new List<int>();

        /// <summary>
        ///History of the last _maxSteps activation patterns. We need to keep
        ///these so that we can associate the current iteration's classification
        ///with the activationPattern from N steps ago
        /// </summary>
        IList<Tuple<int, int[]>> patternNZHistory = new List<Tuple<int, int[]>>();


        /// <summary> 
        ///These are the bit histories.Each one is a BitHistory instance, stored in
        ///this dict, where the key is (bit, nSteps). The 'bit' is the index of the
        ///bit in the activation pattern and nSteps is the number of steps of
        ///prediction desired for that bit.
        /// </summary>
        Dictionary<Tuple<int, int>, BitHistory> activeBitHistory = new Dictionary<Tuple<int, int>, BitHistory>();

        /// <summary>
        /// This keeps track of the actual value to use for each bucket index. We
        /// start with 1 bucket, no actual value so that the first infer has something
        /// to return
        /// </summary>
        IList<T> actualValues = new List<T>();

        /// <summary>
        /// set up values from constructor and intialize global value 
        /// </summary>
        /// <param name="steps">sequence of the different steps of multi-step predictions to learn</param>
        /// <param name="alpha">The alpha used to compute running averages of the bucket duty
        ///cycles for each activation pattern bit.A lower alpha results
        ///in longer term memory.</param>
        /// <param name="actValueAlpha"></param>
        private void Initialize(IList<int> steps, double alpha, double actValueAlpha)
        {
            this.steps = steps;
            this.alpha = alpha;
            this.actValueAlpha = actValueAlpha;
            //for null value add -1 instead of 0
            actualValues.Add(IsNumericType(default(T)) ? (-1).Convert<T>() : default);
        }

        /// <summary>
        /// check the type of an instance is numeric type or not
        /// </summary>
        /// <param name="o">instance of an object</param>
        /// <returns>if numeric type returns true otherwise false</returns>
        static bool IsNumericType(object o)
        {
            if (o == null) return false;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Default constructor for setting up default data
        /// </summary>
        public CLAClassifier()
        {
            Initialize(new List<int> { 1 }, 0.001, 0.3);
        }

        /// <summary>
        /// Constructor where value can be assigned
        /// </summary>
        /// <param name="steps">sequence of the different steps of multi-step predictions to learn</param>
        /// <param name="alpha">The alpha used to compute running averages of the bucket duty
        ///cycles for each activation pattern bit.A lower alpha results
        ///in longer term memory.</param>
        /// <param name="actValueAlpha"></param>
        public CLAClassifier(IList<int> steps, double alpha, double actValueAlpha)
        {
            Initialize(steps, alpha, actValueAlpha);
        }

        /// <summary>
        /// Method computes the result after the data is provided by the temporal memory and encoder
        /// </summary>
        /// <param name="recordNum">Record number of this input pattern. 
        /// Record numbers should normally increase sequentially by 1 each time unless there are missing records in the dataset.
        /// Knowing this information insures that we don't get confused by missing records.</param>
        /// <param name="classification">{@link Map} of the classification information:
        /// bucketIdx: index of the encoder bucket</param>
        /// <param name="patternNZ">list of the active indices from the output below</param>
        /// <param name="learn">if true, learn this sample</param>
        /// <param name="infer">if true, perform inference</param>
        /// <returns>ClassificationExperiment Object</returns>

        public ClassificationExperiment<T> Compute(int recordNum, Dictionary<string, object> classification, int[] patternNZ, bool learn, bool infer)
        {
            if (classification == null)
                throw new ArgumentNullException(nameof(classification));
            if (patternNZ == null)
                throw new ArgumentNullException(nameof(patternNZ));

            ClassificationExperiment<T> retVal = new ClassificationExperiment<T>();
            List<T> actualValues = (List<T>)this.actualValues;


            // Save the offset between recordNum and learnIteration if this is the first
            // compute
            if (recordNumMinusLearnIteration == -1)
            {
                recordNumMinusLearnIteration = recordNum - learnIteration;
            }

            // Update the learn iteration
            learnIteration = recordNum - recordNumMinusLearnIteration;


            if (patternNZHistory.Any(x => x.Item1 == learnIteration))
            {
                patternNZHistory.Add(Tuple.Create(learnIteration, patternNZ));
            }

            //-------------------------------------------------------------
            // For each active bit in the activationPattern, get the classification
            // votes
            //
            // Return value dict. For buckets which we don't have an actual value
            // for yet, just plug in any valid actual value. It doesn't matter what
            // we use because that bucket won't have non-zero likelihood anyways.
            if (infer)
            {
                // If doing 0-step prediction, we shouldn't use any knowledge
                // of the classification input during inference.
                object defaultValue = default(T);
                if (steps[0] == 0)
                {
                    defaultValue = 0;
                }
                else
                {
                    defaultValue = classification["actValue"];
                    if (defaultValue == null)
                    {
                        if (IsNumericType(default(T)))
                        {
                            defaultValue = (-1).Convert<T>();
                        }
                    }
                }

                T[] actValues = new T[this.actualValues.Count];

                for (int i = 0; i < actualValues.Count; i++)
                {
                    if (IsNumericType(default(T)))
                    {
                        actValues[i] = actualValues[i].Equals((-1).Convert<T>()) ? defaultValue.Convert<T>() : actualValues[i];
                    }
                    else
                    {
                        actValues[i] = (T)(actualValues[i].Equals(default(T)) ? defaultValue : actualValues[i]);
                    }
                }

                retVal.setActualValues(actValues);

                // For each n-step prediction...
                foreach (int nSteps in steps)
                {
                    // Accumulate bucket index votes and actValues into these arrays
                    double[] sumVotes = new double[maxBucketIdx + 1];
                    double[] bitVotes = new double[maxBucketIdx + 1];

                    foreach (var bit in patternNZ)
                    {
                        var key = Tuple.Create(bit, nSteps);
                        BitHistory history = null;
                        activeBitHistory.TryGetValue(key, out history);
                        if (history == null) continue;

                        history.Infer(bitVotes);

                        sumVotes = ArrayUtils.AddOffset(sumVotes, bitVotes);
                    }

                    // Return the votes for each bucket, normalized
                    double total = sumVotes.Sum();
                    if (total > 0)
                    {
                        sumVotes = ArrayUtils.Divide(sumVotes, total);
                    }
                    else
                    {
                        // If all buckets have zero probability then simply make all of the
                        // buckets equally likely. There is no actual prediction for this
                        // timestep so any of the possible predictions are just as good.
                        if (sumVotes.Length > 0)
                        {
                            double val = 1.0 / sumVotes.Length;
                            for (int i = 0; i < sumVotes.Length; i++)
                            {
                                sumVotes[i] = val;
                            }
                        }
                    }

                    retVal.setStats(nSteps, sumVotes);
                }

            }
            // ------------------------------------------------------------------------
            // Learning:
            // For each active bit in the activationPattern, store the classification
            // info. If the bucketIdx is None, we can't learn. This can happen when the
            // field is missing in a specific record.
            if (learn && classification["bucketIdx"] != null)
            {
                // Get classification info
                int bucketIdx = (int)classification["bucketIdx"];
                object actValue = null;
                if (classification["actValue"] == null)
                {
                    if (IsNumericType(default(T)))
                    {
                        actValue = (-1).Convert<T>();
                    }
                }
                else
                {
                    actValue = classification["actValue"].Convert<T>();
                }

                // Update maxBucketIndex
                maxBucketIdx = Math.Max(maxBucketIdx, bucketIdx);

                // Update rolling average of actual values if it's a scalar. If it's
                // not, it must be a category, in which case each bucket only ever
                // sees one category so we don't need a running average.
                while (maxBucketIdx > actualValues.Count - 1)
                {
                    actualValues.Add(IsNumericType(default(T)) ? (-1).Convert<T>() : default);
                }
                if (actualValues[bucketIdx].Equals(IsNumericType(default(T)) ? (-1).Convert<T>() : default))
                {
                    actualValues[bucketIdx] = (T)actValue;
                }
                else
                {
                    if (IsNumericType(actValue))
                    {
                        double val = (1.0 - actValueAlpha) * Convert.ToDouble(actualValues[bucketIdx], enusFormatProvider) +
                                        actValueAlpha * Convert.ToDouble(actValue, enusFormatProvider);
                        actualValues[bucketIdx] = val.Convert<T>();
                    }
                    else
                    {
                        actualValues[bucketIdx] = (T)actValue;
                    }
                }

                // Train each pattern that we have in our history that aligns with the
                // steps we have in steps
                int nSteps = -1;
                int iteration = 0;
                int[] learnPatternNZ = null;
                foreach (var n in steps)
                {
                    nSteps = n;
                    // Do we have the pattern that should be assigned to this classification
                    // in our pattern history? If not, skip it
                    bool found = false;
                    foreach (var t in patternNZHistory)
                    {
                        iteration = t.Item1;
                        learnPatternNZ = t.Item2;
                        if (iteration == learnIteration - nSteps)
                        {
                            found = true;
                            break;
                        }
                        iteration++;
                    }
                    if (!found) continue;

                    // Store classification info for each active bit from the pattern
                    // that we got nSteps time steps ago.
                    foreach (int bit in learnPatternNZ)
                    {
                        // Get the history structure for this bit and step
                        var key = Tuple.Create(bit, nSteps);
                        BitHistory history = null;
                        activeBitHistory.TryGetValue(key, out history);
                        if (history == null)
                        {
                            history = new BitHistory(alpha);
                            activeBitHistory.Add(key, history);
                        }
                        history.store(learnIteration, bucketIdx);
                    }
                }
            }

            return retVal;
        }

    }
}