// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Utility
{
    public class BitHistory
    {

        private double alpha;

        Dictionary<int, double> stats;

        int lastTotalUpdate = -1;

        private const int DUTY_CYCLE_UPDATE_INTERVAL = Int32.MaxValue;

        /// <summary>
        /// add or update value to particular key
        /// </summary>
        /// <param name="dic">dictionary updated value is needed</param>
        /// <param name="key">Updating key</param>
        /// <param name="newValue">new value</param>
        private static void AddOrUpdate(Dictionary<int, double> dic, int key, double newValue)
        {
            double val;
            if (dic.TryGetValue(key, out val))
            {
                // value exists! update value
                dic[key] = newValue;
            }
            else
            {
                //add the value
                dic.Add(key, newValue);
            }
        }

        /// <summary>
        /// Contructor for assigning alpha value
        /// </summary>
        /// <param name="alpha">The alpha used to compute running averages of the bucket duty
        ///cycles for each activation pattern bit.A lower alpha results
        ///in longer term memory.</param>
        public BitHistory(double alpha)
        {
            this.alpha = alpha;
            stats = new Dictionary<int, double>();
        }



        /// <summary>
        /// Store a new item in our history.
        /// This gets called for a bit whenever it is active and learning is enabled
        /// Save duty cycle by normalizing it to the same iteration as
        /// the rest of the duty cycles which is lastTotalUpdate.
        /// This is done to speed up computation in inference since all of the duty
        /// cycles can now be scaled by a single number.
        ///  The duty cycle is brought up to the current iteration only at inference and
        ///  only when one of the duty cycles gets too large (to avoid overflow to
        ///  larger data type) since the ratios between the duty cycles are what is
        ///  important.As long as all of the duty cycles are at the same iteration
        ///  their ratio is the same as it would be for any other iteration, because the
        ///  update is simply a multiplication by a scalar that depends on the number of
        ///  steps between the last update of the duty cycle and the current iteration.
        /// </summary>
        /// <param name="iteration">the learning iteration number, which is only incremented
        ////when learning is enabled
        ///</param>
        /// <param name="bucketIdx"> the bucket index to store
        /// </param>

        public void store(int iteration, int bucketIdx)
        {
            // If lastTotalUpdate has not been set, set it to the current iteration.
            if (lastTotalUpdate == -1)
            {
                lastTotalUpdate = iteration;
            }

            // Get the duty cycle stored for this bucket.
            int statsLen = stats.Count - 1;
            if (bucketIdx > statsLen)
            {
                for (int i = stats.Count; i <= bucketIdx; i++)
                {
                    AddOrUpdate(stats, i, 0);
                }
            }


            double dc = stats[bucketIdx];
            // To get the duty cycle from n iterations ago that when updated to the
            // current iteration would equal the dc of the current iteration we simply
            // divide the duty cycle by (1-alpha)**(n). This results in the formula
            // dc'{-n} = dc{-n} + alpha/(1-alpha)**n where the apostrophe symbol is used
            // to denote that this is the new duty cycle at that iteration. This is
            // equivalent to the duty cycle dc{-n}
            double denom = Math.Pow((1.0 - alpha), (iteration - lastTotalUpdate));

            double dcNew = 0;
            if (denom > 0) dcNew = dc + (alpha / denom);

            // This is to prevent errors associated with infinite rescale if too large
            if (denom == 0 || dcNew > DUTY_CYCLE_UPDATE_INTERVAL)
            {
                double exp = Math.Pow((1.0 - alpha), (iteration - lastTotalUpdate));
                double dcT = 0;
                for (int i = 0; i < stats.Count; i++)
                {
                    dcT = stats[i];
                    dcT *= exp;
                    AddOrUpdate(stats, i, dcT);
                }

                // Reset time since last update
                lastTotalUpdate = iteration;

                // Add alpha since now exponent is 0
                dc = stats[bucketIdx] + alpha;
            }
            else
            {
                dc = dcNew;
            }
            AddOrUpdate(stats, bucketIdx, dc);
        }

        /// <summary>
        /// Look up and return the votes for each bucketIdx for this bit.
        /// </summary>
        /// <param name="votes">array, initialized to all 0's, that should be filled
        ///in with the votes for each bucket.The vote for bucket index N
        ///should go into votes[N].</param>
        public void Infer(double[] votes)
        {
            // Place the duty cycle into the votes and update the running total for
            // normalization
            if (votes == null)
            {
                throw new ArgumentNullException(nameof(votes));
            }
            double total = 0;
            for (int i = 0; i < stats.Count; i++)
            {
                double dc = stats[i];
                if (dc > 0.0)
                {
                    //set state value in decrement position
                    votes[i] = dc;
                    total += dc;
                }
            }

            // Experiment... try normalizing the votes from each bit
            if (total > 0)
            {
                double[] temp = ArrayUtils.Divide(votes, total);
                for (int i = 0; i < temp.Length; i++) votes[i] = temp[i];
            }
        }
    }
}