// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;


namespace NeoCortexApi
{
    public class MovingAverage : IEquatable<MovingAverage>
    {
        private Calculation m_Calc;

        private int m_WindowSize;

        /// <summary>
        /// Construct a new <see cref="MovingAverage"/>
        /// </summary>
        /// <param name="historicalValues">list of entry values</param>
        /// <param name="windowSize">length over which to take the average</param>
        public MovingAverage(List<double> historicalValues, int windowSize) : this(historicalValues, -1, windowSize)
        {

        }

        /// <summary>
        /// Construct a new <see cref="MovingAverage"/>
        /// </summary>
        /// <param name="historicalValues">list of entry values</param>
        /// <param name="windowSize">length over which to take the average</param>
        /// <exception cref="ArgumentException">Throws if <paramref name="windowSize"/> is less than 1</exception>
        public MovingAverage(List<double> historicalValues, double total, int windowSize)
        {
            if (windowSize <= 0)
            {
                throw new ArgumentException("Window size must be > 0");
            }

            this.m_WindowSize = windowSize;

            m_Calc = new Calculation
            {
                HistoricalValues = historicalValues == null || historicalValues.Count < 1 ? new List<double>(windowSize) : historicalValues,
                Total = total != -1 ? total : m_Calc.HistoricalValues.Sum()
            };
        }

        /// <summary>
        /// Routine for computing a moving average
        /// </summary>
        /// <param name="slidingWindow">a list of previous values to use in the computation that will be modified and returned</param>
        /// <param name="total">total the sum of the values in the  slidingWindow to be used in the calculation of the moving average</param>
        /// <param name="newVal">newVal a new number to compute the new windowed average</param>
        /// <param name="windowSize">windowSize how many values to use in the moving window</param>
        /// <returns></returns>
        public static Calculation Compute(List<double> slidingWindow, double total, double newVal, int windowSize)
        {
            return Compute(null, slidingWindow, total, newVal, windowSize);
        }

        /// <summary>
        /// Internal method which does actual calculation
        /// </summary>
        /// <param name="calc">Re-used calculation object</param>
        /// <param name="slidingWindow">a list of previous values to use in the computation that will be modified and returned</param>
        /// <param name="total">total the sum of the values in the  slidingWindow to be used in the calculation of the moving average</param>
        /// <param name="newVal">newVal a new number to compute the new windowed average</param>
        /// <param name="windowSize">windowSize how many values to use in the moving window</param>
        /// <returns></returns>
        private static Calculation Compute(
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

            return CopyInto(calc, slidingWindow, total / (double)slidingWindow.Count, total);
        }

        public int MyProperty { get; set; }
        /// <summary>
        /// Called to compute the next moving average value.
        /// </summary>
        /// <param name="newValue">new point data</param>
        /// <returns></returns>
        public double Next(double newValue)
        {
            Compute(m_Calc, m_Calc.HistoricalValues, m_Calc.Total, newValue, m_WindowSize);
            return m_Calc.Average;
        }

        /// <summary>
        /// Returns the sliding window buffer used to calculate the moving average.
        /// </summary>
        /// <returns></returns>
        public List<double> GetSlidingWindow()
        {
            return m_Calc.HistoricalValues;
        }

        /// <summary>
        /// Returns the current running total
        /// </summary>
        /// <returns></returns>
        public double GetTotal()
        {
            return m_Calc.Total;
        }

        /// <summary>
        /// Returns the size of the window over which the moving average is computed.
        /// </summary>
        /// <returns></returns>
        public int GetWindowSize()
        {
            return m_WindowSize;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((m_Calc == null) ? 0 : m_Calc.GetHashCode());
            result = prime * result + m_WindowSize;
            return result;
        }


        public bool Equals(MovingAverage obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            MovingAverage other = obj;
            if (m_Calc == null)
            {
                if (other.m_Calc != null)
                    return false;
            }
            else if (!m_Calc.Equals(other.m_Calc))
                return false;
            if (m_WindowSize != other.m_WindowSize)
                return false;
            return true;
        }

        /// <summary>
        /// Internal method to update running totals.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="slidingWindow"></param>
        /// <param name="average"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private static Calculation CopyInto(Calculation c, List<double> slidingWindow, double average, double total)
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

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns></returns>
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

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
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
