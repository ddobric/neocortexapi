using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Decorator interface for main algorithms .
    /// </summary>
    public interface IComputeDecorator
    {

        /**
         * Feeds input record through TM, performing inferencing and learning
         * 
         * @param connections       the connection memory
         * @param activeColumns     direct activated column input
         * @param learn             learning mode flag
         * @return                  {@link ComputeCycle} container for one cycle of inference values.
         */
        ComputeCycle Compute(Connections connections, int[] activeColumns, bool learn);
        /**
         * Called to start the input of a new sequence, and
         * reset the sequence state of the TM.
         * 
         * @param   connections   the Connections state of the temporal memory
         */
        void Reset(Connections connections);
    }
}
