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

        private static readonly long serialVersionUID = 1L;

        public List<DistalDendrite> CctiveSegments = new List<DistalDendrite>();

        public List<DistalDendrite> MatchingSegments = new List<DistalDendrite>();

        public ISet<Cell> m_predictiveCells = new LinkedHashSet<Cell>();

        /// <summary>
        /// Gets the list of active cells.
        /// </summary>
        public ISet<Cell> activeCells { get; set; } = new LinkedHashSet<Cell>();

        /// <summary>
        /// Gets the list of winner cells.
        /// </summary>
        public ISet<Cell> winnerCells { get; set; } = new LinkedHashSet<Cell>();

        /**
         * Constructs a new {@code ComputeCycle}
         */
        public ComputeCycle() { }

        /**
         * Constructs a new {@code ComputeCycle} initialized with
         * the connections relevant to the current calling {@link Thread} for
         * the specified {@link TemporalMemory}
         * 
         * @param   c       the current connections state of the TemporalMemory
         */
        public ComputeCycle(Connections c)
        {
            this.activeCells = new LinkedHashSet<Cell>(c.getWinnerCells());//TODO potential bug. activeCells or winnerCells?!
            this.winnerCells = new LinkedHashSet<Cell>(c.getWinnerCells());
            this.m_predictiveCells = new LinkedHashSet<Cell>(c.getPredictiveCells());
            this.CctiveSegments = new List<DistalDendrite>(c.getActiveSegments());
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
                if (m_predictiveCells == null || m_predictiveCells.Count == 0)
                {
                    Cell previousCell = null;
                    Cell currCell = null;

                    foreach (DistalDendrite activeSegment in CctiveSegments)
                    {
                        if ((currCell = activeSegment.getParentCell()) != previousCell)
                        {
                            m_predictiveCells.Add(previousCell = currCell);
                        }
                    }
                }

                return m_predictiveCells;
            }
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((activeCells == null) ? 0 : activeCells.GetHashCode());
            result = prime * result + ((predictiveCells == null) ? 0 : predictiveCells.GetHashCode());
            result = prime * result + ((winnerCells == null) ? 0 : winnerCells.GetHashCode());
            result = prime * result + ((CctiveSegments == null) ? 0 : CctiveSegments.GetHashCode());
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
            if (activeCells == null)
            {
                if (other.activeCells != null)
                    return false;
            }
            else if (!activeCells.Equals(other.activeCells))
                return false;
            if (predictiveCells == null)
            {
                if (other.predictiveCells != null)
                    return false;
            }
            else if (!predictiveCells.Equals(other.predictiveCells))
                return false;
            if (winnerCells == null)
            {
                if (other.winnerCells != null)
                    return false;
            }
            else if (!winnerCells.Equals(other.winnerCells))
                return false;
            if (CctiveSegments == null)
            {
                if (other.CctiveSegments != null)
                    return false;
            }
            else if (!CctiveSegments.Equals(other.CctiveSegments))
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
