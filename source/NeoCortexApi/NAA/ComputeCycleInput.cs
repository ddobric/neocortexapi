using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class ComputeCycleInput
    {
        public int[] ExternalPredictiveInputsActive { get; set; }
        
        public int[] ExternalPredictiveInputsWinners { get; set; }

        /// <summary>
        /// Gets the list of active cells.
        /// </summary>
        public List<Cell> ActiveCells { get; set; }

        /// <summary>
        /// Gets the list of winner cells.
        /// </summary>
        public List<Cell> WinnerCells { get; set; }

        /// <summary>
        /// Segment is understood as active one if the number of connected synapses (with permanence value higher than specified connected permanence threshold) 
        /// of active cells on that segment, is higher than segment activation threshold.
        /// </summary>
        public List<DistalDendrite> ActiveSegments = new List<DistalDendrite>();

        /// <summary>
        /// Segment is understood as matching one if number of synapses of active cells on that segment 
        /// is higher than specified segment minimum threshold value.
        /// </summary>
        public List<DistalDendrite> MatchingSegments = new List<DistalDendrite>();
    }
}
