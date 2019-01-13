using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace NeoCortexApi
{
    public class MovingAverage : IEquatable<MovingAverage>
    {
        private Calculation calc;

        private int windowSize;

        /**
         * Constructs a new {@code MovingAverage}
         * 
         * @param historicalValues  list of entry values
         * @param windowSize        length over which to take the average
         */
        public MovingAverage(List<double> historicalValues, int windowSize) : this(historicalValues, -1, windowSize)
        {

        }

        /**
         * Constructs a new {@code MovingAverage}
         * 
         * @param historicalValues  list of entry values
         * @param windowSize        length over which to take the average
         */
        public MovingAverage(List<double> historicalValues, double total, int windowSize)
        {
            if (windowSize <= 0)
            {
                throw new ArgumentException("Window size must be > 0");
            }

            this.windowSize = windowSize;

            calc = new Calculation();
            calc.HistoricalValues =
                historicalValues == null || historicalValues.Count < 1 ?
                    new List<double>(windowSize) : historicalValues;
            calc.Total = total != -1 ? total : calc.HistoricalValues.Sum();
        }

        /**
         * Routine for computing a moving average
         * 
         * @param slidingWindow     a list of previous values to use in the computation that
         *                          will be modified and returned
         * @param total             total the sum of the values in the  slidingWindow to be used in the
         *                          calculation of the moving average
         * @param newVal            newVal a new number to compute the new windowed average
         * @param windowSize        windowSize how many values to use in the moving window
         * @return
         */
        public static Calculation compute(List<double> slidingWindow, double total, double newVal, int windowSize)
        {
            return compute(null, slidingWindow, total, newVal, windowSize);
        }

        /**
         * Internal method which does actual calculation
         * 
         * @param calc              Re-used calculation object
         * @param slidingWindow     a list of previous values to use in the computation that
         *                          will be modified and returned
         * @param total             total the sum of the values in the  slidingWindow to be used in the
         *                          calculation of the moving average
         * @param newVal            newVal a new number to compute the new windowed average
         * @param windowSize        windowSize how many values to use in the moving window
         * @return
         */
        private static Calculation compute(
            Calculation calc, List<double> slidingWindow, double total, double newVal, int windowSize)
        {

            if (slidingWindow == null)
            {
                throw new ArgumentException("SlidingWindow cannot be null.");
            }

            if (slidingWindow.Count == windowSize)
            {
                total -= slidingWindow[0];
                slidingWindow.RemoveAt(0);
            }

            slidingWindow.Add(newVal);
            total += newVal;

            if (calc == null)
            {
                return new Calculation(slidingWindow, total / (double)slidingWindow.Count, total);
            }

            return copyInto(calc, slidingWindow, total / (double)slidingWindow.Count, total);
        }

        /**
         * Called to compute the next moving average value.
         * 
         * @param newValue  new point data
         * @return
         */
        public double next(double newValue)
        {
            compute(calc, calc.HistoricalValues, calc.Total, newValue, windowSize);
            return calc.Average;
        }

        /**
         * Returns the sliding window buffer used to calculate the moving average.
         * @return
         */
        public List<double> getSlidingWindow()
        {
            return calc.HistoricalValues;
        }

        /**
         * Returns the current running total
         * @return
         */
        public double getTotal()
        {
            return calc.Total;
        }

        /**
         * Returns the size of the window over which the 
         * moving average is computed.
         * 
         * @return
         */
        public int getWindowSize()
        {
            return windowSize;
        }


        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((calc == null) ? 0 : calc.GetHashCode());
            result = prime * result + windowSize;
            return result;
        }


        public bool Equals(MovingAverage obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            MovingAverage other = (MovingAverage)obj;
            if (calc == null)
            {
                if (other.calc != null)
                    return false;
            }
            else if (!calc.Equals(other.calc))
                return false;
            if (windowSize != other.windowSize)
                return false;
            return true;
        }

        /**
         * Internal method to update running totals.
         * 
         * @param c
         * @param slidingWindow
         * @param value
         * @param total
         * @return
         */
        private static Calculation copyInto(Calculation c, List<double> slidingWindow, double average, double total)
        {
            c.HistoricalValues = slidingWindow;
            c.Average = average;
            c.Total = total;
            return c;
        }

        /// <summary>
        /// Calculated data.
        /// </summary>
        public class Calculation : IEquatable<Calculation>
        {
            /// <summary>
            /// Returns the current value at this point in the calculation.
            /// </summary>
            public double Average;

            /// <summary>
            /// Returns a list of calculated values in the order of their
            /// calculation.
            /// </summary>
            public List<double> HistoricalValues;

            /// <summary>
            /// Returns the total.
            /// </summary>
            public double Total;

            public Calculation()
            {

            }

            public Calculation(List<double> historicalValues, double currentValue, double total)
            {
                this.Average = currentValue;
                this.HistoricalValues = historicalValues;
                this.Total = total;
            }

            public override int GetHashCode()
            {
                int prime = 31;
                int result = 1;
                long temp;
                temp = BitConverter.DoubleToInt64Bits(Average);
                result = prime * result + (int)(temp ^ (temp >> 32));
                result = prime * result + ((HistoricalValues == null) ? 0 : HistoricalValues.GetHashCode());
                temp = BitConverter.DoubleToInt64Bits(Total);
                result = prime * result + (int)(temp ^ (temp >> 32));
                return result;
            }


            public bool Equals(Calculation obj)
            {
                if (this == obj)
                    return true;
                if (obj == null)
                    return false;

                Calculation other = (Calculation)obj;
                if (BitConverter.DoubleToInt64Bits(Average) != BitConverter.DoubleToInt64Bits(other.Average))
                    return false;

                if (HistoricalValues == null)
                {
                    if (other.HistoricalValues != null)
                        return false;
                }
                else if (!HistoricalValues.SequenceEqual(other.HistoricalValues))
                    return false;
                if (BitConverter.DoubleToInt64Bits(Total) != BitConverter.DoubleToInt64Bits(other.Total))
                    return false;
                return true;
            }

        }
    }
}
