// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Utility
{
    /// <summary>
    /// Generates a range of integers.
    /// </summary>
    /// <remarks>
    /// Author
    /// </remarks>
    public class IntGenerator
    {
        protected int m_CurrentValue;
        protected int lower;
        protected int upper;

        public IntGenerator(int lower, int upper)
        {
            this.lower = lower;
            this.m_CurrentValue = this.lower;
            this.upper = upper;
        }

        /// <summary>
        /// Returns the value returned by the last call to <see cref="Next"/> or the initial value if no previous call to <see cref="Next"/> was made. 
        /// </summary>
        /// <returns></returns>
        public int Get()
        {
            return m_CurrentValue;
        }

        /// <summary>
        /// Returns the configured size or distance between the initialized upper and lower bounds.
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return upper - lower;
        }

        /// <summary>
        /// Returns the state of this generator to its initial state so 
        /// that it can be reused.
        /// </summary>
        public void Reset()
        {
            this.m_CurrentValue = lower;
        }


        /// <summary>
        /// Moves iterator to the next value and returns the current value.
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            int retVal = m_CurrentValue;
            m_CurrentValue = ++m_CurrentValue > upper ? upper : m_CurrentValue;
            return retVal;
        }


        /// <summary>
        /// Gets the next value witout of incremmenting iterator.
        /// </summary>
        public int NextValue
        {
            get
            {
                var currVal = m_CurrentValue;
                int nexVal = ++currVal > upper ? upper : currVal;

                return nexVal;
            }
        }

        /**
         * {@inheritDoc}
         */
        // TODO no abstract method to be inherited
        public bool HasNext() { return m_CurrentValue < upper - 1; }

        /// <summary>
        /// Returns a <see cref="IntGenerator"/> which returns integers between
        /// the values specified (lower inclusive, upper exclusive)
        /// </summary>
        /// <param name="lower">the lower bounds or start value</param>
        /// <param name="upper">the upper bounds (exclusive)</param>
        /// <returns></returns>
        public static IntGenerator Of(int lower, int upper)
        {
            return new IntGenerator(lower, upper);
        }
    }

}
