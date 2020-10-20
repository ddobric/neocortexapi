// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class Anomaly : IHtmAlgorithm
    {
        /// <summary>
        /// Modes to use for factory creation method
        /// </summary>
        public enum Mode { PURE, LIKELIHOOD, WEIGHTED };

        #region Instantiation keys
        public static readonly int VALUE_NONE = -1;
        public static readonly String KEY_MODE = "mode";
        public static readonly String KEY_LEARNING_PERIOD = "claLearningPeriod";
        public static readonly String KEY_ESTIMATION_SAMPLES = "estimationSamples";
        public static readonly String KEY_USE_MOVING_AVG = "useMovingAverage";
        public static readonly String KEY_WINDOW_SIZE = "windowSize";
        public static readonly String KEY_IS_WEIGHTED = "isWeighted";
        #endregion

        #region Configs
        public static readonly String KEY_DIST = "distribution";
        public static readonly String KEY_MVG_AVG = "movingAverage";
        public static readonly String KEY_HIST_LIKE = "historicalLikelihoods";
        public static readonly String KEY_HIST_VALUES = "historicalValues";
        public static readonly String KEY_TOTAL = "total";
        #endregion

        #region Computational argument keys
        public readonly static String KEY_MEAN = "mean";
        public readonly static String KEY_STDEV = "stdev";
        public readonly static String KEY_VARIANCE = "variance";
        #endregion

        protected MovingAverage movingAverage;

        protected bool useMovingAverage;

        /// <summary>
        /// Constructs a new <see cref="Anomaly"/>
        /// </summary>
        public Anomaly() : this(false, -1)
        {
         
        }

        /// <summary>
        /// Construct a new <see cref="Anomaly"/>
        /// </summary>
        /// <param name="useMovingAverage">indicates whether to apply and store a moving average</param>
        /// <param name="windowSize">size of window to average over</param>
        /// <exception cref="ArgumentException">Throws if <paramref name="windowSize"/> is less than 1</exception>
        protected Anomaly(bool useMovingAverage, int windowSize)
        {
            this.useMovingAverage = useMovingAverage;
            if (this.useMovingAverage)
            {
                if (windowSize < 1)
                {
                    throw new ArgumentException(
                        "Window size must be > 0, when using moving average.");
                }
                movingAverage = new MovingAverage(null, windowSize);
            }
        }
    }
}
