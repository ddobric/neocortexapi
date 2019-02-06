using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class Anomaly : IHtmAlgorithm
    {
        /** Modes to use for factory creation method */
        public enum Mode { PURE, LIKELIHOOD, WEIGHTED };

        // Instantiation keys
        public static readonly int VALUE_NONE = -1;
        public static readonly String KEY_MODE = "mode";
        public static readonly String KEY_LEARNING_PERIOD = "claLearningPeriod";
        public static readonly String KEY_ESTIMATION_SAMPLES = "estimationSamples";
        public static readonly String KEY_USE_MOVING_AVG = "useMovingAverage";
        public static readonly String KEY_WINDOW_SIZE = "windowSize";
        public static readonly String KEY_IS_WEIGHTED = "isWeighted";

        // Configs
        public static readonly String KEY_DIST = "distribution";
        public static readonly String KEY_MVG_AVG = "movingAverage";
        public static readonly String KEY_HIST_LIKE = "historicalLikelihoods";
        public static readonly String KEY_HIST_VALUES = "historicalValues";
        public static readonly String KEY_TOTAL = "total";

        // Computational argument keys
        public readonly static String KEY_MEAN = "mean";
        public readonly static String KEY_STDEV = "stdev";
        public readonly static String KEY_VARIANCE = "variance";

        protected MovingAverage movingAverage;

        protected bool useMovingAverage;

        /**
         * Constructs a new {@code Anomaly}
         */
        public Anomaly() : this(false, -1)
        {
         
        }

        /**
         * Constructs a new {@code Anomaly}
         * 
         * @param useMovingAverage  indicates whether to apply and store a moving average
         * @param windowSize        size of window to average over
         */
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
