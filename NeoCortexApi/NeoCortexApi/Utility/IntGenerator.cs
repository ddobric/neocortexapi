using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Utility
{


    /**
     * Generates a range of integers.
     * 
     * @author 
     */
    public class IntGenerator
    {
        /** serial version */
        private static readonly long serialVersionUID = 1L;

        protected int m_NextIndex;
        protected int lower;
        protected int upper;

        public IntGenerator(int lower, int upper)
        {
            this.lower = lower;
            this.m_NextIndex = this.lower;
            this.upper = upper;
        }

        /**
         * Returns the value returned by the last call to {@link #next()}
         * or the initial value if no previous call to {@code #next()} was made.
         * @return
         */
        public int get()
        {
            return m_NextIndex;
        }

        /**
         * Returns the configured size or distance between the initialized
         * upper and lower bounds.
         * @return
         */
        public int size()
        {
            return upper - lower;
        }

        /**
         * Returns the state of this generator to its initial state so 
         * that it can be reused.
         */
        public void reset()
        {
            this.m_NextIndex = lower;
        }

        /**
         * {@inheritDoc}
         */

        public int next()
        {
            int retVal = m_NextIndex;
            m_NextIndex = ++m_NextIndex > upper ? upper : m_NextIndex;
            return retVal;
        }

        /**
         * {@inheritDoc}
         */

        public bool hasNext() { return m_NextIndex < upper - 1; }

        /**
         * Returns a {@link Generator} which returns integers between
         * the values specified (lower inclusive, upper exclusive)
         * @param lower     the lower bounds or start value
         * @param upper     the upper bounds (exclusive)
         * @return
         */
        public static IntGenerator of(int lower, int upper)
        {
            return new IntGenerator(lower, upper);
        }
    }

}
