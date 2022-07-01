using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static NeoCortexApi.TemporalMemory;

namespace NeoCortexApi
{
    /// <summary>
    /// See PhD Chapter Neural Associations Algorithm.
    /// </summary>
    public class NeuralAssociationsAlgorithm
    { 

        private int m_TMIteration;

        private CorticalArea area;

        private HtmConfig cfg;

        /// <summary>
        /// Stores each cycle's most recent activity
        /// </summary>
        public SegmentActivity LastActivity { get; set; }

        public NeuralAssociationsAlgorithm(HtmConfig cfg, CorticalArea area)
        {
            this.cfg = cfg;
            this.area = area;
        }


        public ComputeCycle Compute(int[] activeColumns, ComputeCycleInput inp, bool learn)
        {
            ComputeCycle cycle = ActivateCells(activeColumns, inp, learn: learn);

            return cycle;
        }

        private Column GetColumnFromIndex(int index)
        {
            var col = this.area.Columns.FirstOrDefault(i => i.Index == index);

            if (col == null)
                throw new ArgumentException($"The column with the index {index} does not exist in the area {area}");

            return col;
        }

        protected virtual ComputeCycle ActivateCells(int[] activeColumnIndices, ComputeCycleInput inp, bool learn)
        {
            ColumnData activeColumnData = new ColumnData();

            ComputeCycle newComputeCycle = new ComputeCycle
            {
                ActivColumnIndicies = activeColumnIndices
            };


            // The list of active columns.
            List<Column> activeColumns = new List<Column>();

            foreach (var indx in activeColumnIndices.OrderBy(i => i))
            {
                activeColumns.Add(GetColumnFromIndex(indx));
            }

            //
            // Gets the mini-columns that owns the segment.
            Func<object, Column> segToCol = (segment) =>
            {
                var colIndx = ((DistalDendrite)segment).ParentCell.ParentColumnIndex;
                var parentCol = GetColumnFromIndex(colIndx);
                return parentCol;
            };

            Func<object, Column> times1Fnc = x => (Column)x;

            var list = new Pair<List<object>, Func<object, Column>>[3];
            list[0] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(activeColumns.ToArray(), item => (object)item).ToList(), times1Fnc);
            list[1] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(inp.ActiveSegments.ToArray(), item => (object)item).ToList(), segToCol);
            list[2] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(inp.MatchingSegments.ToArray(), item => (object)item).ToList(), segToCol);

            GroupBy2<Column> grouper = GroupBy2<Column>.Of(list);

            double permanenceIncrement = this.cfg.PermanenceIncrement;
            double permanenceDecrement = this.cfg.PermanenceDecrement;

            //
            // Grouping by columns, which have active and matching segments.
            foreach (var tuple in grouper)
            {
                activeColumnData.Set(tuple);

                if (activeColumnData.IsExistAnyActiveCol(0))
                {
                    // If there are some active segments on the column already...
                    if (activeColumnData.ActiveSegments != null && activeColumnData.ActiveSegments.Count > 0)
                    {

                        List<Cell> cellsOwnersOfActSegs = ActivatePredictedColumn(activeColumnData.ActiveSegments,
                            activeColumnData.MatchingSegments, inp.ActiveCells, inp.WinnerCells,
                                permanenceIncrement, permanenceDecrement, learn, newComputeCycle.ActiveSynapses);

                        foreach (var item in cellsOwnersOfActSegs)
                        {
                            newComputeCycle.ActiveCells.Add(item);
                            newComputeCycle.WinnerCells.Add(item);
                        }
                    }
                    else
                    {
                        //
                        // If no active segments are detected (start of learning) then all cells are activated
                        // and a random single cell is chosen as a winner.
                        BurstingResult burstingResult = BurstColumn(activeColumnData.Column(), activeColumnData.MatchingSegments,
                            inp.ActiveCells, inp.WinnerCells, permanenceIncrement, permanenceDecrement, this.cfg.Random,
                               learn);

                        // Here we activate all cells by putting them to list of active cells.
                        newComputeCycle.ActiveCells.AddRange(burstingResult.Cells);

                        // Test was done. Better performance is when BestCell is used only instead of adding all cells.
                        //cycle.WinnerCells.AddRange(burstingResult.Cells);

                        // The winner cell is added to th elots of winner cells in the cycle.
                        newComputeCycle.WinnerCells.Add(burstingResult.BestCell);
                    }
                }
                else
                {
                    if (learn)
                    {
                        PunishPredictedColumn(activeColumnData.ActiveSegments, activeColumnData.MatchingSegments,
                            inp.ActiveCells, inp.WinnerCells, this.cfg.PredictedSegmentDecrement);
                    }
                }
            }

            return newComputeCycle;
        }


        /// <summary>
        /// Calculate dendrite segment activity, using the current active cells.
        /// 
        /// <para>
        /// Pseudocode:<br/>
        ///   for each distal dendrite segment with number of active synapses >= activationThreshold<br/>
        ///     mark the segment as active<br/>
        ///   for each distal dendrite segment with unconnected activity >= minThreshold<br/>
        ///     mark the segment as matching<br/>
        /// </para>
        /// </summary>
        /// <param name="conn">the Connectivity</param>
        /// <param name="cycle">Stores current compute cycle results</param>
        /// <param name="learn">If true, segment activations will be recorded. This information is used during segment cleanup.</param>
        /// <seealso cref="">https://github.com/htm-community/htm.core/blob/master/src/htm/algorithms/TemporalMemory.cpp</seealso>
        protected void ActivateDendrites(Connections conn, ComputeCycle cycle, bool learn, int[] externalPredictiveInputsActive = null, int[] externalPredictiveInputsWinners = null)
        {
            //if (externalPredictiveInputsActive != null)
            //    cycle.ActiveCells.AddRange(externalPredictiveInputsActive);

            //if (externalPredictiveInputsWinners != null)
            //    cycle.WinnerCells.AddRange(externalPredictiveInputsActive);

            SegmentActivity activity = Connections.ComputeActivity(cycle.ActiveCells, conn.HtmConfig.ConnectedPermanence);

            var activeSegments = new List<DistalDendrite>();
            foreach (var item in activity.ActiveSynapses)
            {
                if (item.Value >= conn.HtmConfig.ActivationThreshold)
                {
                    var seg = conn.GetSegmentForFlatIdx(item.Key);
                    if (seg != null)
                        activeSegments.Add(seg);
                }
            }

            //
            // Step through all synapses on active cells and find involved segments.         
            var matchingSegments = new List<DistalDendrite>();
            foreach (var item in activity.PotentialSynapses)
            {
                var seg = conn.GetSegmentForFlatIdx(item.Key);
                if (seg != null && item.Value >= conn.HtmConfig.MinThreshold)
                    matchingSegments.Add(seg);
            }

            //
            // Step through all synapses on active cells with permanence over threshold (conencted synapses)
            // and find involved segments.         
            activeSegments.Sort(GetComparer(conn.NextSegmentOrdinal));

            matchingSegments.Sort(GetComparer(conn.NextSegmentOrdinal));

            cycle.ActiveSegments = activeSegments;
            cycle.MatchingSegments = matchingSegments;

            //conn.LastActivity = activity;
            this.LastActivity = activity;

            conn.ActiveCells = new HashSet<Cell>(cycle.ActiveCells);
            conn.WinnerCells = new HashSet<Cell>(cycle.WinnerCells);
            conn.ActiveSegments = activeSegments;
            conn.MatchingSegments = matchingSegments;

            // Forces generation of the predictive cells from the above active segments
            conn.ClearPredictiveCells();
            //cycle.DepolirizeCells(conn);

            if (learn)
            {
                foreach (var segment in activeSegments)
                {
                    conn.RecordSegmentActivity(segment);
                }

                conn.StartNewIteration();
            }

            Debug.WriteLine($"\nActive segments: {activeSegments.Count}, Matching segments: {matchingSegments.Count}");
        }

        protected BurstingResult BurstColumn(Column column, List<DistalDendrite> matchingSegments,
         ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells, double permanenceIncrement, double permanenceDecrement,
             Random random, bool learn)
        {
            IList<Cell> cells = column.Cells;
            Cell leastUsedOrMaxPotentialCell = null;

            //
            // Matching segments result from number of potential synapses. These are segments with number of potential
            // synapses permanence higher than some minimum threshold value.
            // Potential synapses are synapses from presynaptc cells connected to the active cell.
            // In other words, synapse permanence between presynaptic cell and the active cell defines a statistical prediction that active cell will become the active in the next cycle.
            // Bursting will create new segments if there are no matching segments until some matching segments appear. 
            // Once that happen, segment adoption will start.
            // If some matching segments exist, bursting will grab the segment with most potential synapses and adapt it.
            if (matchingSegments != null && matchingSegments.Count > 0)
            {
                // Debug.Write($"B.({matchingSegments.Count})");

                DistalDendrite maxPotentialSeg = HtmCompute.GetSegmentwithHighesPotential(matchingSegments, prevActiveCells, this.LastActivity.PotentialSynapses);

                leastUsedOrMaxPotentialCell = maxPotentialSeg.ParentCell;

                if (learn)
                {
                    AdaptSegment(maxPotentialSeg, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    int nGrowDesired = this.cfg.MaxNewSynapseCount - this.LastActivity.PotentialSynapses[maxPotentialSeg.SegmentIndex];

                    if (nGrowDesired > 0)
                    {
                        GrowSynapses(prevWinnerCells, maxPotentialSeg, this.cfg.InitialPermanence, nGrowDesired, random);
                    }
                }
            }
            else
            {
                // Debug.Write("B.0");

                leastUsedOrMaxPotentialCell = HtmCompute.GetLeastUsedCell(cells, random);
                if (learn)
                {
                    // This can be optimized. Right now, we assume that every winner cell has a single synaptic connection to the segment.
                    // This is why we substract number of cells from the MaxNewSynapseCount.
                    int nGrowExact = Math.Min(this.cfg.MaxNewSynapseCount, prevWinnerCells.Count);
                    if (nGrowExact > 0)
                    {
                        DistalDendrite newSegment = CreateDistalSegment(leastUsedOrMaxPotentialCell);
                        GrowSynapses(prevWinnerCells, newSegment, this.cfg.InitialPermanence,
                            nGrowExact, random);
                    }
                }
            }

            return new BurstingResult(cells, leastUsedOrMaxPotentialCell);
        }

        /// <summary>
        /// Returns the number of <see cref="DistalDendrite"/>s on a given <see cref="Cell"/> if specified, or the total number if the <see cref="Cell"/> is null.
        /// </summary>
        /// <param name="cell">an optional Cell to specify the context of the segment count.</param>
        /// <returns>either the total number of segments or the number on a specified cell.</returns>
        public int NumSegments(Cell cell = null)
        {
            if (cell != null)
            {
                //DD
                //return GetSegments(cell).Count;
                return cell.DistalDendrites.Count;
            }

         
            lock ("segmentindex")
            {
                return this.area.AllDistalDendrites.Length;
                //return m_NextFlatIdx - m_FreeFlatIdxs.Count;
            }
        }

        /// <summary>
        /// Used internally to return the least recently activated segment on the specified cell
        /// </summary>
        /// <param name="cell">cell to search for segments on.</param>
        /// <returns>the least recently activated segment on the specified cell.</returns>
        private DistalDendrite LeastRecentlyUsedSegment(Cell cell)
        {
            List<DistalDendrite> segments = cell.DistalDendrites;

            DistalDendrite minSegment = null;
            long minIteration = long.MaxValue;

            foreach (DistalDendrite dd in segments)
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
        /// Adds a new <see cref="DistalDendrite"/> segment on the specified <see cref="Cell"/>, or reuses an existing one.
        /// </summary>
        /// <param name="segmentParentCell">the Cell to which a segment is added.</param>
        /// <returns>the newly created segment or a reused segment.</returns>
        public DistalDendrite CreateDistalSegment(Cell segmentParentCell)
        {
            //
            // If there are more segments than maximal allowed number of segments per cell,
            // least used segments will be destroyed.
            while (NumSegments(segmentParentCell) >= this.cfg.MaxSegmentsPerCell)
            {
                DestroyDistalDendrite(LeastRecentlyUsedSegment(segmentParentCell));
            }

            int flatIdx;

            lock ("segmentindex")
            {
                int len;
                if ((len = m_FreeFlatIdxs.Count()) > 0)
                {
                    flatIdx = m_FreeFlatIdxs[len - 1];
                    m_FreeFlatIdxs.RemoveRange(len - 1, 1);
                    //if (!m_FreeFlatIdxs.TryRemove(len - 1, out flatIdx))
                    //    throw new Exception("Object cannot be removed!");
                }
                else
                {
                    flatIdx = m_NextFlatIdx;
                    //m_SegmentForFlatIdx.TryAdd(flatIdx, null);
                    m_SegmentForFlatIdx[flatIdx] = null;
                    //m_SegmentForFlatIdx.Add(null);
                    ++m_NextFlatIdx;
                }

                int ordinal = m_NextSegmentOrdinal;
                ++m_NextSegmentOrdinal;

                DistalDendrite segment = new DistalDendrite(segmentParentCell, flatIdx, m_TMIteration, ordinal, this.HtmConfig.SynPermConnected, this.HtmConfig.NumInputs);
                segmentParentCell.DistalDendrites.Add(segment);
                //GetSegments(segmentParentCell, true).Add(segment);
                m_SegmentForFlatIdx[flatIdx] = segment;

                return segment;

            }
        }

        /// <summary>
        /// Increments the permanence of the segment's synapse if the synapse's presynaptic cell was active in the previous cycle.
        /// If it was not active, then it will decrement the permanence value. 
        /// If the permamence is below EPSILON, synapse is destroyed.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="segment">The segment to adapt.</param>
        /// <param name="prevActiveCells">List of active cells in the current cycle (calculated in the previous cycle).</param>
        /// <param name="permanenceIncrement"></param>
        /// <param name="permanenceDecrement"></param>
        public void AdaptSegment(DistalDendrite segment, ICollection<Cell> prevActiveCells,
            double permanenceIncrement, double permanenceDecrement)
        {

            // Destroying a synapse modifies the set that we're iterating through.
            List<Synapse> synapsesToDestroy = new List<Synapse>();

            foreach (Synapse presynapticCellSynapse in segment.Synapses)
            {
                double permanence = presynapticCellSynapse.Permanence;

                //
                // If synapse's presynaptic cell was active in the previous cycle then streng it.
                if (prevActiveCells.Contains(presynapticCellSynapse.GetPresynapticCell()))
                {
                    permanence += permanenceIncrement;
                }
                else
                {
                    permanence -= permanenceDecrement;
                }

                // Keep permanence within min/max bounds
                permanence = permanence < 0 ? 0 : permanence > 1.0 ? 1.0 : permanence;

                // Use this to examine issues caused by subtle floating point differences
                // be careful to set the scale (1 below) to the max significant digits right of the decimal point
                // between the permanenceIncrement and initialPermanence
                //
                // permanence = new BigDecimal(permanence).setScale(1, RoundingMode.HALF_UP).doubleValue(); 

                double EPSILON = 0.00001;

                if (permanence < EPSILON)
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
                DestroySynapse(syn, segment);
            }

            if (segment.Synapses.Count == 0)
            {
                DestroyDistalDendrite(segment);
            }
        }

        /// <summary>
        /// Destroys the specified <see cref="Synapse"/> in specific <see cref="DistalDendrite"/> segment and in the source cell.
        /// Every synapse instance is stored at two places: The source cell (receptor synapse) and the segment.
        /// </summary>
        /// <param name="synapse">the Synapse to destroy</param>
        /// <param name="segment"></param>
        private void DestroySynapse(Synapse synapse, DistalDendrite segment)
        {
            lock ("synapses")
            {
                synapse.SourceCell.ReceptorSynapses.Remove(synapse);

                segment.Synapses.Remove(synapse);
            }
        }

        /// <summary>
        /// Destroys a segment <see cref="DistalDendrite"/>
        /// </summary>
        /// <param name="segment">the segment to destroy</param>
        public void DestroyDistalDendrite(DistalDendrite segment)
        {
            lock ("segmentindex")
            {
                // Remove the synapses from all data structures outside this Segment.
                //DD List<Synapse> synapses = GetSynapses(segment);
                List<Synapse> synapses = segment.Synapses;
                int len = synapses.Count;

                lock ("synapses")
                {
                    //getSynapses(segment).stream().forEach(s->removeSynapseFromPresynapticMap(s));
                    //DD foreach (var s in GetSynapses(segment))
                    foreach (var s in segment.Synapses)
                    {
                        DestroySynapse(s, segment);
                    }

                    //m_NumSynapses -= len;
                }

                // Remove the segment from the cell's list.
                //DD
                //GetSegments(segment.ParentCell).Remove(segment);
                segment.ParentCell.DistalDendrites.Remove(segment);

                // Remove the segment from the map
                //DD m_DistalSynapses.Remove(segment);

                // Free the flatIdx and remove the final reference so the Segment can be
                // garbage-collected.
                m_FreeFlatIdxs.Add(segment.SegmentIndex);
                //m_FreeFlatIdxs[segment.SegmentIndex] = segment.SegmentIndex;
                m_SegmentForFlatIdx[segment.SegmentIndex] = null;
            }
        }


        /// <summary>
        /// TM activated segments on the column in the previous cycle. This method locates such segments and 
        /// adapts them and return owner cells of active segments.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="columnActiveSegments">Active segments as calculated (activated) in the previous step.</param>
        /// <param name="matchingSegments"></param>
        /// <param name="prevActiveCells">Cells active in the current cycle.</param>
        /// <param name="prevWinnerCells"></param>
        /// <param name="permanenceIncrement"></param>
        /// <param name="permanenceDecrement"></param>
        /// <param name="learn"></param>
        /// <returns>Cells which own active column segments as calculated in the previous step.</returns>
        protected List<Cell> ActivatePredictedColumn(List<DistalDendrite> columnActiveSegments,
            List<DistalDendrite> matchingSegments, ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells,
                double permanenceIncrement, double permanenceDecrement, bool learn, IList<Synapse> activeSynapses)
        {
            // List of cells that owns active segments. These cells will be activated in this cycle.
            // In previous cycle they are depolarized.
            List<Cell> cellsOwnersOfActiveSegments = new List<Cell>();

            foreach (DistalDendrite segment in columnActiveSegments)
            {
                if (!cellsOwnersOfActiveSegments.Contains(segment.ParentCell))
                {
                    cellsOwnersOfActiveSegments.Add(segment.ParentCell);
                }

                if (learn)
                {
                    AdaptSegment(segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    //
                    // Even if the segment is active, new synapses can be added that connect previously active cells with the segment.
                    int numActive = this.LastActivity.PotentialSynapses[segment.SegmentIndex];
                    int nGrowDesired = this.cfg.MaxNewSynapseCount - numActive;

                    if (nGrowDesired > 0)
                    {
                        // Create new synapses on the segment from winner (pre-synaptic cells) cells.
                        GrowSynapses(prevWinnerCells, segment, this.cfg.InitialPermanence,
                            nGrowDesired, this.cfg.Random);
                    }
                    else
                    {
                        // for debugging.
                    }
                }
            }

            return cellsOwnersOfActiveSegments;
        }

        /// <summary>
        /// Creates nDesiredNewSynapes synapses on the segment passed in if possible, choosing random cells from the previous winner cells that are
        /// not already on the segment.
        /// </summary>
        /// <param name="prevWinnerCells">Winner cells in `t-1`</param>
        /// <param name="segment">Segment to grow synapses on. </param>
        /// <param name="initialPermanence">Initial permanence of a new synapse.</param>
        /// <param name="nDesiredNewSynapses">Desired number of synapses to grow</param>
        /// <param name="random"><see cref="TemporalMemory"/> object used to generate random numbers</param>
        /// <remarks>
        /// <b>Notes:</b> The process of writing the last value into the index in the array that was most recently changed is to ensure the same results that 
        /// we get in the c++ implementation using iter_swap with vectors.
        /// </remarks>
        public void GrowSynapses(ICollection<Cell> prevWinnerCells, DistalDendrite segment,
            double initialPermanence, int nDesiredNewSynapses, Random random)
        {
            random = new Random();
            List<Cell> removingCandidates = new List<Cell>(prevWinnerCells);
            removingCandidates = removingCandidates.OrderBy(c => c).ToList();

            //
            // Enumarates all synapses in a segment and remove winner-cells from
            // list of removingCandidates if they are presynaptic winners cells.
            // So, we will create synapses only from cells, which do not already have synaptic connection to the segment. 
            // DD: Removing of this loop creates same result as with the loop.
            // This should be investigated.
            foreach (Synapse synapse in segment.Synapses)
            {
                Cell presynapticCell = synapse.GetPresynapticCell();
                int index = removingCandidates.IndexOf(presynapticCell);
                if (index != -1)
                {
                    removingCandidates.RemoveAt(index); ;
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
                CreateSynapse(segment, removingCandidates[rndIndex], initialPermanence);
                removingCandidates.RemoveAt(rndIndex);
            }
        }

        /// <summary>
        /// Creates a new synapse on a distal segment.
        /// </summary>
        /// <param name="segment">the <see cref="DistalDendrite"/> segment to which a <see cref="Synapse"/> is being created.</param>
        /// <param name="presynapticCell">the source <see cref="Cell"/>.</param>
        /// <param name="permanence">the initial permanence.</param>
        /// <returns>the created <see cref="Synapse"/>.</returns>
        public Synapse CreateSynapse(DistalDendrite segment, Cell presynapticCell, double permanence)
        {
            while (segment.Synapses.Count >= this.cfg.MaxSynapsesPerSegment)
            {
                DestroySynapse(MinPermanenceSynapse(segment), segment);
            }

            lock ("synapses")
            {
                Synapse synapse = null;
                //DD GetSynapses(segment).Add(
                segment.Synapses.Add(synapse = new Synapse(presynapticCell, segment.SegmentIndex, -1, permanence));

                presynapticCell.ReceptorSynapses.Add(synapse);
                //DD GetReceptorSynapses(presynapticCell, true).Add(synapse);

                //++m_NextSynapseOrdinal;

                //++m_NumSynapses;

                return synapse;
            }
        }
    }
}
