// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Holds all important states calculated during a TM computational cycle.
    /// </summary>
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
        private List<Cell> m_PredictiveCells = new List<Cell>();

        /// <summary>
        /// Gets the list of active cells.
        /// </summary>
        public List<Cell> ActiveCells { get; set; } = new List<Cell>();

        /// <summary>
        /// Gets the list of winner cells.
        /// </summary>
        public List<Cell> WinnerCells { get; set; } = new List<Cell>();

        /// <summary>
        /// Synapses that create connections to currentlly active cells owners of active segments.
        /// </summary>
        public List<Synapse> ActiveSynapses { get; set; } = new List<Synapse>();


        public int[] ActivColumnIndicies { get; set; }

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
            this.ActiveCells = new List<Cell>(c.ActiveCells);//TODO potential bug. activeCells or winnerCells?!
            this.WinnerCells = new List<Cell>(c.WinnerCells);
            this.m_PredictiveCells = new List<Cell>(c.GetPredictiveCells());
            this.ActiveSegments = new List<DistalDendrite>(c.ActiveSegments);
            this.MatchingSegments = new List<DistalDendrite>(c.MatchingSegments);
        }


        //public IList<Cell> PredictiveCellsCellDraft
        //{
        //    get
        //    {
        //        if (m_PredictiveCells.Count == 0)
        //        {
        //            Cell previousCell = null;
        //            Cell currCell = null;

        //            foreach (DistalDendrite activeSegment in ActiveSegments)
        //            {
        //                if ((currCell = activeSegment.ParentCell) != previousCell && this.ActivColumnIndicies.Contains(activeSegment.ParentCell.ParentColumnIndex))
        //                {
        //                    m_PredictiveCells.Add(previousCell = currCell);
        //                }
        //            }
        //        }

        //        return m_PredictiveCells;
        //    }
        //}

        ///// <summary>
        ///// Gets the list of cells in predictive state for the current compute cycle.
        ///// It traverses all active segments (<see cref="ActiveSegments"/>) and declares their parent cells as predictive cells.
        ///// The TM algorithm does not calculate PredictiveCells. It activates instead distal segments
        ///// </summary>
        //public IList<Cell> PredictiveCellsSynapsesTry
        //{
        //    //get
        //    //{
        //    //    if (m_PredictiveCells == null || m_PredictiveCells.Count == 0)
        //    //    {
        //    //        foreach (Synapse syn in this.ActiveSynapses)
        //    //        {
        //    //            m_PredictiveCells.Add(syn.SourceCell);
        //    //        }
        //    //    }

        //    //    return m_PredictiveCells;
        //    //}

        //    get
        //    {
        //        if (m_PredictiveCells.Count == 0)
        //        {
        //            Cell previousCell = null;
        //            Cell currCell = null;

        //            foreach (DistalDendrite activeSegment in ActiveSegments)
        //            {
        //                if ((currCell = activeSegment.ParentCell) != previousCell && this.ActiveSynapses.Count(s => s.SegmentIndex == activeSegment.SegmentIndex) > 0)
        //                {
        //                    m_PredictiveCells.Add(previousCell = currCell);
        //                }
        //            }
        //        }

        //        return m_PredictiveCells;
        //    }
        //}

        //public void DepolirizeCells(Connections conn)
        //{
        //    //if (m_PredictiveCells == null || m_PredictiveCells.Count == 0)
        //    //{
        //    //    foreach (Synapse syn in this.ActiveSynapses)
        //    //    {
        //    //        var seg = conn.GetSegmentForFlatIdx(syn.SegmentIndex);
        //    //        if (seg != null)
        //    //            m_PredictiveCells.Add(seg.ParentCell);
        //    //        else
        //    //        { 

        //    //        }
        //    //    }
        //    //}

        //    Cell previousCell = null;
        //    Cell currCell = null;

        //    foreach (DistalDendrite activeSegment in ActiveSegments)
        //    {
        //        if ((currCell = activeSegment.ParentCell) != previousCell && this.ActiveSynapses.Count(s=>s.SegmentIndex == activeSegment.SegmentIndex) > 0)
        //        {
        //            m_PredictiveCells.Add(previousCell = currCell);
        //        }
        //    }
        //}

        /// <summary>
        /// Gets the list of depolirized cells.
        /// </summary>
        public IList<Cell> PredictiveCells
        {
            get
            {
                if (m_PredictiveCells == null || m_PredictiveCells.Count == 0)
                {
                    Cell previousCell = null;
                    Cell currCell = null;

                    foreach (DistalDendrite activeSegment in ActiveSegments)
                    {
                        if ((currCell = activeSegment.ParentCell) != previousCell)
                        {
                            m_PredictiveCells.Add(previousCell = currCell);
                        }
                    }
                }

                return m_PredictiveCells;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((ActiveCells == null) ? 0 : ActiveCells.GetHashCode());
            result = prime * result + ((PredictiveCells == null) ? 0 : PredictiveCells.GetHashCode());
            result = prime * result + ((WinnerCells == null) ? 0 : WinnerCells.GetHashCode());
            result = prime * result + ((ActiveSegments == null) ? 0 : ActiveSegments.GetHashCode());
            result = prime * result + ((MatchingSegments == null) ? 0 : MatchingSegments.GetHashCode());
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */

        public override bool Equals(object obj)
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
            if (PredictiveCells == null)
            {
                if (other.PredictiveCells != null)
                    return false;
            }
            else if (!PredictiveCells.Equals(other.PredictiveCells))
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
