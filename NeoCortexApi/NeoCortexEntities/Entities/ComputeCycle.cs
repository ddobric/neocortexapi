using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi;

namespace NeoCortexApi.Entities
{
    /**
     * Contains a snapshot of the state attained during one computational
     * call to the {@link TemporalMemory}. The {@code TemporalMemory} uses
     * data from previous compute cycles to derive new data for the current cycle
     * through a comparison between states of those different cycles, therefore
     * this state container is necessary.
     * 
     * @author David Ray
     */
    public class ComputeCycle : IEquatable<object>, NeoCortexApi.IModuleData
    {
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

        /// <summary>
        /// During temporal learning process, dendrite segments are declared as active, 
        /// if the number of active synapses (permanence higher than connectedPermanence) on that segment is higher than activationThreshold value.
        /// A Cell is by default in predictive state (depolarized state) if it owns the active dendrite segment.
        /// </summary>
        public ISet<Cell> PredictiveCells = new LinkedHashSet<Cell>();

        /// <summary>
        /// Gets the list of active cells.
        /// </summary>
        public ISet<Cell> ActiveCells { get; set; } = new LinkedHashSet<Cell>();

        /// <summary>
        /// Gets the list of winner cells.
        /// </summary>
        public ISet<Cell> WinnerCells { get; set; } = new LinkedHashSet<Cell>();

        /// <summary>
        /// 
        /// </summary>
        public ComputeCycle() { }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        public ComputeCycle(Connections c)
        {
            this.ActiveCells = new LinkedHashSet<Cell>(c.getWinnerCells());//TODO potential bug. activeCells or winnerCells?!
            this.WinnerCells = new LinkedHashSet<Cell>(c.getWinnerCells());
            this.PredictiveCells = new LinkedHashSet<Cell>(c.getPredictiveCells());
            this.ActiveSegments = new List<DistalDendrite>(c.getActiveSegments());
            this.MatchingSegments = new List<DistalDendrite>(c.getMatchingSegments());
        }

        /**
         * Returns the current {@link Set} of active cells
         * 
         * @return  the current {@link Set} of active cells
         */



        /**
         * Returns the {@link List} of sorted predictive cells.
         * @return
         */
        public ISet<Cell> predictiveCells
        {
            get
            {
                if (PredictiveCells == null || PredictiveCells.Count == 0)
                {
                    Cell previousCell = null;
                    Cell currCell = null;

                    foreach (DistalDendrite activeSegment in ActiveSegments)
                    {
                        if ((currCell = activeSegment.GetParentCell()) != previousCell)
                        {
                            PredictiveCells.Add(previousCell = currCell);
                        }
                    }
                }

                return PredictiveCells;
            }
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((ActiveCells == null) ? 0 : ActiveCells.GetHashCode());
            result = prime * result + ((predictiveCells == null) ? 0 : predictiveCells.GetHashCode());
            result = prime * result + ((WinnerCells == null) ? 0 : WinnerCells.GetHashCode());
            result = prime * result + ((ActiveSegments == null) ? 0 : ActiveSegments.GetHashCode());
            result = prime * result + ((MatchingSegments == null) ? 0 : MatchingSegments.GetHashCode());
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */

        public bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            ComputeCycle other = (ComputeCycle)obj;
            if (ActiveCells == null)
            {
                if (other.ActiveCells != null)
                    return false;
            }
            else if (!ActiveCells.Equals(other.ActiveCells))
                return false;
            if (predictiveCells == null)
            {
                if (other.predictiveCells != null)
                    return false;
            }
            else if (!predictiveCells.Equals(other.predictiveCells))
                return false;
            if (WinnerCells == null)
            {
                if (other.WinnerCells != null)
                    return false;
            }
            else if (!WinnerCells.Equals(other.WinnerCells))
                return false;
            if (ActiveSegments == null)
            {
                if (other.ActiveSegments != null)
                    return false;
            }
            else if (!ActiveSegments.Equals(other.ActiveSegments))
                return false;
            if (MatchingSegments == null)
            {
                if (other.MatchingSegments != null)
                    return false;
            }
            else if (!MatchingSegments.Equals(other.MatchingSegments))
                return false;
            return true;
        }
    }
}
