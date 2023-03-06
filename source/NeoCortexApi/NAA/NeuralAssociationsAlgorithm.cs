using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using static NeoCortexApi.TemporalMemory;



namespace NeoCortexApi
{
    /// <summary>
    /// See PhD Chapter Neural Associations Algorithm.
    /// </summary>
    public class NeuralAssociationAlgorithm
    {
        /// <summary>
        /// Every _area has its own cycle results. Cellst that are acrive from the point of view of area1 might be different than 
        /// from the point of view of area2.
        /// </summary>
        //private Dictionary<string, ComputeCycle> _cycleResults;

        private int _iteration;

        private CorticalArea _area;

        private HtmConfig _cfg;

        private Random _rnd;

        /// <summary>
        /// Stores each cycle's most recent activity
        /// </summary>
        public SegmentActivity LastActivity { get; set; }


        /// <summary>
        /// Get Active Apical Segments of currentlly active cells in the area.
        /// </summary>
        public List<ApicalDendrite> ActiveApicalSegments
        {
            get
            {
                List<ApicalDendrite> actSegs = new List<ApicalDendrite>();

                foreach (var cell in this._area.ActiveCells)
                {
                    foreach (var seg in cell.ApicalDendrites)
                    {
                        if (seg.NumConnectedSynapses >= _cfg.ActivationThreshold)
                            actSegs.Add(seg);
                    }
                }

                return actSegs;
            }
        }

        /// <summary>
        /// Get Matchin Apical Segments of currentlly active cells in the area.
        /// Segment is the mathcing one if it has less connected synapses than _cfg.ActivationThreshold and
        /// more connected synapses than _cfg.MinThreshold.
        /// </summary>
        public List<ApicalDendrite> MatchingApicalSegments
        {
            get
            {
                List<ApicalDendrite> matchSegs = new List<ApicalDendrite>();

                foreach (var cell in this._area.ActiveCells)
                {
                    foreach (var seg in cell.ApicalDendrites)
                    {
                        if (seg.Synapses.Count >= _cfg.MinThreshold && seg.NumConnectedSynapses < _cfg.ActivationThreshold)
                            matchSegs.Add(seg);
                    }
                }

                return matchSegs;
            }
        }


        /// <summary>
        /// Get all currently active cells that have no apical segments.
        /// </summary>
        public List<Cell> ActiveCellsWithoutApicalSegments
        {
            get
            {
                List<Cell> passiveCells = new List<Cell>();

                foreach (var cell in this._area.ActiveCells)
                {
                    if (cell.ApicalDendrites.Count == 0)
                        passiveCells.Add(cell);
                }

                return passiveCells;
            }
        }

        /// <summary>
        /// Get Inactive Apical Segments of currentlly active cells in the area.
        /// </summary>
        public List<ApicalDendrite> InactiveApicalSegments
        {
            get
            {
                List<ApicalDendrite> matchSegs = new List<ApicalDendrite>();

                foreach (var cell in this._area.ActiveCells)
                {
                    foreach (var seg in cell.ApicalDendrites)
                    {
                        if (seg.Synapses.Count < _cfg.MinThreshold && seg.NumConnectedSynapses < _cfg.ActivationThreshold)
                            matchSegs.Add(seg);
                    }
                }

                return matchSegs;
            }
        }


        public NeuralAssociationAlgorithm(HtmConfig cfg, CorticalArea area, Random random = null)
        {
            this._cfg = cfg;
            this._area = area;
            if (random == null)
            {
                this._rnd = new Random();
            }
            else
                _rnd = random;
        }

        public ComputeCycle Compute(CorticalArea associatedArea, bool learn)
        {
            return Compute(new CorticalArea[] { associatedArea }, learn);
        }

        public ComputeCycle Compute(CorticalArea[] associatedAreas, bool learn)
        {
            foreach (var area in associatedAreas)
            {
                //if (!_cycleResults.ContainsKey(_area.Name))
                //    _cycleResults.Add(_area.Name, new ComputeCycle());

                ActivateCells(area, learn: learn);

                _iteration++;
            }

            return null;
        }

        private static bool isValidated = false;

        protected virtual void ActivateCells(CorticalArea associatedArea, bool learn)
        {
            if (!isValidated)
            {
                // This makes sure that always a batch of synapses is created in the learning step until MaxSynapsesPerSegment is reached.
                if (this._cfg.MaxNewSynapseCount >= _cfg.MaxSynapsesPerSegment)
                    throw new ArgumentException("MaxNewSynapseCount must be less than MaxSynapsesPerSegment.");

                // This makes sure that every active cell will be synaptically connected during learning. With this no information lose will happen.
                if (this._cfg.MaxSynapsesPerSegment < associatedArea.ActiveCells.Count)
                    throw new ArgumentException("associatedArea.ActiveCells.Count must be less than MaxSynapsesPerSegment.");

                isValidated = true;
            }

            int numSynapses = Math.Min(this._cfg.MaxNewSynapseCount, Math.Min(this._cfg.MaxSynapsesPerSegment, associatedArea.ActiveCells.Count));

            ComputeCycle newComputeCycle = new ComputeCycle
            {
                ActivColumnIndicies = null,
            };

            AdaptActiveSegments(associatedArea, learn);

            // In HTM instead of associatedArea.ActiveCells, WinnerCells are used.
            // Because there is curretnly no temporal dependency in the NAA
            AdaptMatchingSegments(associatedArea, learn);

            AdaptIncativeSegments(associatedArea, learn);

        }



        /// <summary>
        /// TM activated inactiveSegments on the column in the previous cycle. This method locates such inactiveSegments and 
        /// adapts them and return owner cells of active inactiveSegments.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="activeSegments">Active inactiveSegments as calculated (activated) in the previous step.</param>
        /// <param name="matchingSegments"></param>
        /// <param name="associatingCells">Cells active in the current cycle.</param>
        /// <param name="prevWinnerCells"></param>
        /// <param name="permanenceIncrement"></param>
        /// <param name="permanenceDecrement"></param>
        /// <param name="learn"></param>
        /// <returns>Cells which owns active column inactiveSegments as calculated in the previous step.</returns>
        protected void AdaptActiveSegments(CorticalArea associatedArea, bool learn)
        {
            if (learn == false)
                return;

            Segment[] activeSegments = DistalOrApical(associatedArea, this._area) ? throw new NotImplementedException() : ActiveApicalSegments.ToArray();

            foreach (Segment segment in activeSegments)
            {
                AdaptSegment(segment, associatedArea.ActiveCells);

                int numSynapses = Math.Min(this._cfg.MaxNewSynapseCount, Math.Min(this._cfg.MaxSynapsesPerSegment, associatedArea.ActiveCells.Count));

                //
                // Even if the segment is active, new synapses can be added that connect previously active cells with the segment.
                int nGrowDesired = numSynapses - segment.Synapses.Count;

                if (nGrowDesired > 0)
                {
                    // Create new synapses on the segment from winner (pre-synaptic cells) cells.
                    GrowSynapses(associatedArea.ActiveCells, segment, this._cfg.InitialPermanence,
                        nGrowDesired, this._cfg.MaxSynapsesPerSegment, this._cfg.Random);
                }
                else
                {
                    // Segment has already maximum number of synapses.
                    // for debugging.
                }

            }
        }

        private void AdaptMatchingSegments(CorticalArea associatedArea, bool learn)
        {
            if (learn == false)
                return;

            // List of cells that owns active inactiveSegments. These cells will be activated in this cycle.
            // In previous cycle they are depolarized.
            //List<Cell> cellsOwnersOfActiveSegments = new List<Cell>();

            Segment[] matchingSegments = DistalOrApical(associatedArea, this._area) ? throw new NotImplementedException() : MatchingApicalSegments.ToArray();

            foreach (var matchSeg in matchingSegments)
            {
                //Segment maxPotentialSeg = HtmCompute.GetSegmentWithHighesPotential(MatchingApicalSegments.ToArray());

                if (learn)
                {
                    AdaptSegment(matchSeg, associatedArea.ActiveCells);

                    int nGrowDesired = this._cfg.MaxNewSynapseCount - matchSeg.Synapses.Count;

                    if (nGrowDesired > 0)
                    {
                        GrowSynapses(associatedArea.ActiveCells, matchSeg, this._cfg.InitialPermanence, nGrowDesired, this._cfg.MaxSynapsesPerSegment, _rnd);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="associatedArea"></param>
        /// <param name="learn"></param>
        /// <param name="permanenceIncrement"></param>
        /// <param name="permanenceDecrement"></param>
        /// <remarks>PHD ref: Algorithm 12 - Line 19-26.</remarks>
        private void AdaptIncativeSegments(CorticalArea associatedArea, bool learn)
        {
            if (learn == false)
                return;

            bool distalOrApical = DistalOrApical(associatedArea, this._area);

            Segment[] inactiveSegments = distalOrApical ? throw new NotImplementedException() : InactiveApicalSegments.ToArray();

            //
            // New segments are created on every cell owner of the inactive segment.
            // In a case of HTM-TM, new segment is created only at the leastUsedPotentialCell of the active mini-column.
            foreach (var inactiveSeg in inactiveSegments)
            {
                FormNewSynapses(associatedArea, inactiveSeg);

                //Cell segOwnerCell = inactiveSeg.ParentCell;

                ////if(segOwnerCell)
                ////
                //// Maximal number of segments per cell should not be exceeded.
                //int currNumSegments = distalOrApical ? segOwnerCell.DistalDendrites.Count : segOwnerCell.ApicalDendrites.Count;

                //if (currNumSegments >= _cfg.MaxSegmentsPerCell)
                //    continue;

                //// This is why we substract number of winner cells from the MaxNewSynapseCount.
                //int numSynapses = Math.Min(this._cfg.MaxNewSynapseCount, Math.Min(this._cfg.MaxSynapsesPerSegment, associatedArea.ActiveCells.Count));

                //CreateSegmentAtCell(associatedArea, inactiveSeg.ParentCell, numSynapses);
            }

            //
            // Create new segments at cells without apical segments and forms synapses from associating active cells to the new segment.
            // This code block is executed once only in the learning process.
            foreach (var cell in ActiveCellsWithoutApicalSegments)
            {
                // If MaxSegmentsPerCell < associatedArea.ActiveCells.Count then not all active cells will connect 
                // to this area. This is a lost of information. For this reason
                // MaxSegmentsPerCell>associatedArea.ActiveCells.Count should be satisfied.
                int numSynapses = Math.Min(this._cfg.MaxNewSynapseCount, Math.Min(this._cfg.MaxSynapsesPerSegment, associatedArea.ActiveCells.Count));

                // Creates the segment with synapses from associating active cells to this cell.
                CreateSegmentAtCell(associatedArea, cell, numSynapses);
            }
        }


        /// <summary>
        /// Creates/Forms new synapses at the segment if not all associating cells are connected to the segment.
        /// </summary>
        /// <param name="associatedArea"></param>
        /// <param name="inactiveSeg"></param>
        private void FormNewSynapses(CorticalArea associatedArea, Segment inactiveSeg)
        {
            foreach (var associatingCell in associatedArea.ActiveCells)
            {
                if (!AreConnected(associatingCell, inactiveSeg))
                {
                    int numNewSynapses = Math.Min(this._cfg.MaxNewSynapseCount, Math.Min(this._cfg.MaxSynapsesPerSegment, associatedArea.ActiveCells.Count));

                    GrowSynapses(associatedArea.ActiveCells, inactiveSeg, this._cfg.InitialPermanence, numNewSynapses, this._cfg.MaxSynapsesPerSegment, _rnd);
                }
            }
        }


        /// <summary>
        /// Checks if the presynaptic cell is connected to the segment owne by some post-synaptic cell.
        /// </summary>
        /// <param name="presynapticCell"></param>
        /// <param name="segment"></param>
        /// <returns>True if the cell forms a synapse to the segment.</returns>
        private bool AreConnected(Cell presynapticCell, Segment segment)
        {
            foreach (var syn1 in segment.Synapses)
            {
                foreach (var syn2 in presynapticCell.ReceptorSynapses)
                {
                    if (syn1 == syn2)
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Creates the segment at the cell with a given number of synapses.
        /// </summary>
        /// <param name="associatedArea"></param>
        /// <param name="segOwnerCell"></param>
        /// <param name="numSynapses"></param>
        private void CreateSegmentAtCell(CorticalArea associatedArea, Cell segOwnerCell, int numSynapses)
        {
            if (numSynapses > 0)
            {
                Segment newSegment;

                //
                // We will create distal inactiveSegments if associating cells are from the same _area.
                // For all cells out of this _area apical inactiveSegments will be created.
                if (this._area.Name == associatedArea.Name)
                    newSegment = CreateDistalSegment(segOwnerCell);
                else
                    newSegment = CreateApicalSegment(segOwnerCell);

                GrowSynapses(associatedArea.ActiveCells, newSegment, this._cfg.InitialPermanence, numSynapses, this._cfg.MaxSynapsesPerSegment, _rnd);
            }
        }

        private static bool DistalOrApical(CorticalArea area1, CorticalArea area2)
        {
            return area1.Name == area2.Name;
        }


        /// <summary>
        /// Used internally to return the least recently activated segment on the specified cell
        /// </summary>
        /// <param name="cell">cell to search for segments on.</param>
        /// <returns>the least recently activated segment on the specified cell.</returns>
        private static Segment GetLeastRecentlyUsedSegment(Segment[] segments)
        {

            Segment minSegment = null;
            long minIteration = long.MaxValue;

            foreach (Segment dd in segments)
            {
                if (dd.LastUsedIteration < minIteration)
                {
                    minSegment = dd;
                    minIteration = dd.LastUsedIteration;
                }
            }

            return minSegment;
        }


        /// <summary>
        /// Adds a new <see cref="Segment"/> segment on the specified <see cref="Cell"/>, or reuses an existing one.
        /// </summary>
        /// <param name="segmentParentCell">the Cell to which a segment is added.</param>
        /// <returns>the newly created segment or a reused segment.</returns>
        public ApicalDendrite CreateApicalSegment(Cell segmentParentCell)
        {
            //
            // If there are more inactiveSegments than maximal allowed number of inactiveSegments per cell,
            // least used inactiveSegments will be destroyed.
            while (segmentParentCell.ApicalDendrites.Count >= this._cfg.MaxSegmentsPerCell)
            {
                var lruSegment = GetLeastRecentlyUsedSegment(segmentParentCell.ApicalDendrites.ToArray());
                KillSegment<ApicalDendrite>(lruSegment as ApicalDendrite, segmentParentCell.ApicalDendrites);
            }

            int index = segmentParentCell.ApicalDendrites.Count;
            ApicalDendrite segment = new ApicalDendrite(segmentParentCell, index, _iteration, index, this._cfg.SynPermConnected, -1 /* For proximal inactiveSegments only.*/);
            segmentParentCell.ApicalDendrites.Add(segment);

            return segment;
        }

        public DistalDendrite CreateDistalSegment(Cell segmentParentCell)
        {
            //
            // If there are more inactiveSegments than maximal allowed number of inactiveSegments per cell,
            // least used inactiveSegments will be destroyed.
            while (segmentParentCell.DistalDendrites.Count >= this._cfg.MaxSegmentsPerCell)
            {
                var lruSegment = GetLeastRecentlyUsedSegment(segmentParentCell.DistalDendrites.ToArray());
                KillSegment<DistalDendrite>(lruSegment as DistalDendrite, segmentParentCell.DistalDendrites);
            }

            int index = segmentParentCell.DistalDendrites.Count;
            DistalDendrite segment = new DistalDendrite(segmentParentCell, index, _iteration, index, this._cfg.SynPermConnected, -1 /* For proximal inactiveSegments only.*/);
            segmentParentCell.DistalDendrites.Add(segment);

            return segment;

        }

        /// <summary>
        /// Increments the permanence of the segment's synapse if the synapse's presynaptic cell was active in the previous cycle.
        /// If it was not active, then it will decrement the permanence value. 
        /// If the permamence is below EPSILON, synapse is destroyed.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="segment">The segment to adapt. The typically belongs to this areas Y, where associating cells belong to area X.</param>
        /// <param name="associatingActiveCells">List of active cells in the current cycle that will be associated with the segment.</param>
        public void AdaptSegment(Segment segment, ICollection<Cell> associatingActiveCells)
        {
            // The list of synapses that will be destroyed.
            List<Synapse> synapsesToDestroy = new List<Synapse>();

            foreach (Synapse presynapticCellSynapse in segment.Synapses)
            {
                double permanence = presynapticCellSynapse.Permanence;

                //
                // If synapse's presynaptic cell was active in the previous cycle then streng it.
                if (associatingActiveCells.Contains(presynapticCellSynapse.GetPresynapticCell()))
                {
                    permanence += this._cfg.PermanenceIncrement;
                }
                else
                {
                    permanence -= this._cfg.PermanenceDecrement;
                }

                // Keep permanence within min/max bounds
                permanence = permanence < 0 ? 0 : permanence > 1.0 ? 1.0 : permanence;

                // Use this to examine issues caused by subtle floating point differences
                // be careful to set the scale (1 below) to the max significant digits right of the decimal point
                // between the permanenceIncrement and initialPermanence
                //
                // permanence = new BigDecimal(permanence).setScale(1, RoundingMode.HALF_UP).doubleValue(); 

                if (permanence < HtmConfig.EPSILON)
                {
                    synapsesToDestroy.Add(presynapticCellSynapse);
                }
                else
                {
                    presynapticCellSynapse.Permanence = permanence;
                }
            }

            foreach (Synapse syn in synapsesToDestroy)
            {
                segment.KillSynapse(syn);
            }

            if (segment.Synapses.Count == 0)
            {
                KillSegment(segment);
            }
        }


        /// <summary>
        /// Destroys the specified <see cref="Synapse"/> in specific <see cref="Segment"/> segment and in the source cell.
        /// Every synapse instance is stored at two places: The source cell (receptor synapse) and the segment.
        /// </summary>
        /// <param name="synapse">the Synapse to destroy</param>
        /// <param name="segment"></param>
        private static void DestroySynapse(Synapse synapse, Segment segment)
        {
            // lock ("synapses")
            {
                synapse.SourceCell.ReceptorSynapses.Remove(synapse);

                segment.Synapses.Remove(synapse);
            }
        }

        private void KillSegment(Segment segment)
        {
            if (segment.GetType() == typeof(ApicalDendrite))
                KillSegment<ApicalDendrite>(segment as ApicalDendrite, segment.ParentCell.ApicalDendrites);
            else if (segment.GetType() == typeof(DistalDendrite))
                KillSegment<DistalDendrite>(segment as DistalDendrite, segment.ParentCell.DistalDendrites);
            else
                throw new ArgumentException($"Unsuproted segment type: {segment.GetType().Name}");
        }

        /// <summary>
        /// Destroys a segment <see cref="Segment"/>
        /// </summary>
        /// <param name="segment">the segment to destroy</param>
        private void KillSegment<TSeg>(TSeg segment, List<TSeg> segments) where TSeg : Segment
        {
            lock ("segmentindex")
            {
                // Remove the synapses from all data structures outside this Segment.
                //DD List<Synapse> synapses = GetSynapses(segment);
                List<Synapse> synapses = segment.Synapses;
                int len = synapses.Count;


                //getSynapses(segment).stream().forEach(s->removeSynapseFromPresynapticMap(s));
                //DD foreach (var s in GetSynapses(segment))
                foreach (var s in segment.Synapses)
                {
                    DestroySynapse(s, segment);
                }

                //m_NumSynapses -= len;


                segments.Remove(segment);
            }
        }



        /// <summary>
        /// Creates nDesiredNewSynapes synapses on the segment passed in if possible, choosing random cells from the previous winner cells that are
        /// not already on the segment.
        /// </summary>
        /// <param name="associatingCells">Winner cells in `t-1`</param>
        /// <param name="segment">Segment to grow synapses on. </param>
        /// <param name="initialPermanence">Initial permanence of a new synapse.</param>
        /// <param name="nDesiredNewSynapses">Desired number of synapses to grow</param>
        /// <param name="random"><see cref="TemporalMemory"/> object used to generate random numbers</param>
        /// <remarks>
        /// <b>Notes:</b> The process of writing the last value into the index in the array that was most recently changed is to ensure the same results that 
        /// we get in the c++ implementation using iter_swap with vectors.
        /// </remarks>
        protected void GrowSynapses(ICollection<Cell> associatingCells, Segment segment,
            double initialPermanence, int nDesiredNewSynapses, int maxSynapsesPerSegment, Random random)
        {

            List<Cell> removingCandidates = new List<Cell>(associatingCells);
            removingCandidates = removingCandidates.OrderBy(c => c).ToList();

            //
            // So, we will create synapses only from cells, which do not already have synaptic connection to the segment.          
            foreach (Synapse synapse in segment.Synapses)
            {
                int index = removingCandidates.IndexOf(synapse.SourceCell);
                if (index != -1)
                {
                    removingCandidates.RemoveAt(index);
                }
            }

            int candidatesLength = removingCandidates.Count;

            // We take here eather wanted growing number of desired synapes or num of candidates
            // if too many growing synapses requested.
            int numMissingSynapses = nDesiredNewSynapses < candidatesLength ? nDesiredNewSynapses : candidatesLength;

            //
            // Finally we randomly create new synapses. 
            for (int i = 0; i < numMissingSynapses; i++)
            {
                int rndIndex = random.Next(removingCandidates.Count);
                var newSynapse = CreateSynapse(segment, removingCandidates[rndIndex], initialPermanence, maxSynapsesPerSegment);
                removingCandidates.RemoveAt(rndIndex);
            }
        }

        /// <summary>
        /// Creates a new synapse on a segment.
        /// </summary>
        /// <param name="segment">the <see cref="Segment"/> segment to which a <see cref="Synapse"/> is being created.</param>
        /// <param name="presynapticCell">the source <see cref="Cell"/>.</param>
        /// <param name="permanence">the initial permanence.</param>
        /// <returns>the created <see cref="Synapse"/>.</returns>
        protected Synapse CreateSynapse(Segment segment, Cell presynapticCell, double permanence, int maxSynapsesPerSegment)
        {
            while (segment.Synapses.Count >= maxSynapsesPerSegment)
            {
                DestroySynapse(segment.GetMinPermanenceSynapse(), segment);
            }

            //lock ("synapses")
            {
                Synapse synapse = null;

                segment.Synapses.Add(synapse = new Synapse(presynapticCell, synapseIndex: segment.Synapses.Count,
                    segmentIndex: segment.SegmentIndex, segmentCellIndex: segment.ParentCell.Index, this._area.Name,
                    permanence));

                presynapticCell.ReceptorSynapses.Add(synapse);

                return synapse;
            }
        }


        /// <summary>
        /// Calculates the synaptic energy of the segment. SUmmirizes all permanences of apical segments.
        /// </summary>
        /// <returns></returns>
        public double GetApicalSynapticEnergy()
        {
            double energy = 0;

            foreach (var seg in this.ActiveApicalSegments)
            {
                seg.Synapses.ForEach(s => energy += s.Permanence);
            }

            return energy;
        }


        /// <summary>
        /// Gets the trace of the _area in the current cycle.
        /// </summary>
        /// <returns></returns>
        public string TraceState()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Iteration {_iteration}");

            sb.AppendLine($"Active Apical Segments in area {this._area.Name}: {ActiveApicalSegments.Count}");
            sb.AppendLine($"Matching Apical Segments: {MatchingApicalSegments.Count}");
            sb.AppendLine($"Inactive Apical Segments: {InactiveApicalSegments.Count}");
            sb.AppendLine($"Active Cells without Apical Segments: {ActiveCellsWithoutApicalSegments.Count}.");
            sb.AppendLine($"Synaptic Energy = {GetApicalSynapticEnergy()}");

            foreach (var cell in this._area.ActiveCells)
            {
                sb.Append(cell.TraceCell());
            }

            return sb.ToString();
        }
    }
}
