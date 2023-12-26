using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Reconstructs the input from the SDR produced by Spatial Pooler.
    /// </summary>
    public class SPSdrReconstructor
    {
        private readonly Connections _mem;

        /// <summary>
        /// Creates the constructor for SDR reconstruction.
        /// </summary>
        /// <param name="mem">The HTM memory state.</param>
        public SPSdrReconstructor(Connections mem)
        {
            _mem = mem;
        }

        /// <summary>
        /// Reconstructs the input from the SDR produced by Spatial Pooler.
        /// </summary>
        /// <param name="activeMiniColumns">The array of active mini columns.</param>
        /// <returns>Dictionary of inputs, with permanences resulted from acurrently active mini-columns.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Dictionary<int, double> Reconstruct(int[] activeMiniColumns)
        {
            if(activeMiniColumns == null)
            {
                throw new ArgumentNullException(nameof(activeMiniColumns));
            }

            var cols = _mem.GetColumnList(activeMiniColumns);
           
            Dictionary<int, double> result = new Dictionary<int, double>();

            //
            // Iterate through all columns and collect all synapses.
            foreach (var col in cols)
            {
                col.ProximalDendrite.Synapses.ForEach(s =>
                {
                    double currPerm = 0.0;

                    // Check if the key already exists
                    if (result.TryGetValue(s.InputIndex, out currPerm))
                    {
                        // Key exists, update the value
                        result[s.InputIndex] = s.Permanence + currPerm;
                    }
                    else
                    {
                        // Key doesn't exist, add a new key-value pair
                        result[s.InputIndex] = s.Permanence;
                    }
                });
            }

            return result;
        }
    }
}
