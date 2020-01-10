// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Utility;
using static NeoCortexApi.Entities.Connections;
using System.Diagnostics;

namespace NeoCortexApi
{
    /// <summary>
    /// Implementation of Temporal Memory algorithm.
    /// </summary>
    public class TemporalMemory : IHtmAlgorithm<int[], ComputeCycle>//: IComputeDecorator
    {
        private static readonly double EPSILON = 0.00001;

        private static readonly int cIndexofACTIVE_COLUMNS = 0;

        private Connections connections;

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /**
         * Uses the specified {@link Connections} object to Build the structural 
         * anatomy needed by this {@code TemporalMemory} to implement its algorithms.
         * 
         * The connections object holds the {@link Column} and {@link Cell} infrastructure,
         * and is used by both the {@link SpatialPooler} and {@link TemporalMemory}. Either of
         * these can be used separately, and therefore this Connections object may have its
         * Columns and Cells initialized by either the init method of the SpatialPooler or the
         * init method of the TemporalMemory. We check for this so that complete initialization
         * of both Columns and Cells occurs, without either being redundant (initialized more than
         * once). However, {@link Cell}s only get created when initializing a TemporalMemory, because
         * they are not used by the SpatialPooler.
         * 
         * @param   c       {@link Connections} object
         */


        public void init(Connections conn)
        {
            this.connections = conn;

            SparseObjectMatrix<Column> matrix = this.connections.getMemory() == null ?
                new SparseObjectMatrix<Column>(this.connections.getColumnDimensions()) :
                    (SparseObjectMatrix<Column>)this.connections.getMemory();

            this.connections.setMemory(matrix);

            int numColumns = matrix.getMaxIndex() + 1;
            this.connections.setNumColumns(numColumns);
            int cellsPerColumn = this.connections.getCellsPerColumn();
            Cell[] cells = new Cell[numColumns * cellsPerColumn];

            //Used as flag to determine if Column objects have been created.
            Column colZero = matrix.getObject(0);
            for (int i = 0; i < numColumns; i++)
            {
                Column column = colZero == null ? new Column(cellsPerColumn, i, this.connections.getSynPermConnected(), this.connections.NumInputs) : matrix.getObject(i);
                for (int j = 0; j < cellsPerColumn; j++)
                {
                    cells[i * cellsPerColumn + j] = column.Cells[j];
                }
                //If columns have not been previously configured
                if (colZero == null)
                    matrix.set(i, column);

            }
            //Only the TemporalMemory initializes cells so no need to test for redundancy
            this.connections.setCells(cells);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeColumns"></param>
        /// <param name="learn"></param>
        /// <returns></returns>
        public ComputeCycle Compute(int[] activeColumns, bool learn)
        {  
            ComputeCycle cycle = ActivateCells(this.connections, activeColumns, learn);
            ActivateDendrites(this.connections, cycle, learn);
            Debug.WriteLine("");
            return cycle;
        }


        /**
         * Calculate the active cells, using the current active columns and dendrite
         * segments. Grow and reinforce synapses.
         * 
         * <pre>
         * Pseudocode:
         *   for each column
         *     if column is active and has active distal dendrite segments
         *       call activatePredictedColumn
         *     if column is active and doesn't have active distal dendrite segments
         *       call burstColumn
         *     if column is inactive and has matching distal dendrite segments
         *       call punishPredictedColumn
         *      
         * </pre>
         * 
         * @param conn                     
         * @param activeColumnIndices
         * @param learn
         */

        protected ComputeCycle ActivateCells(Connections conn, int[] activeColumnIndices, bool learn)
        {
            ComputeCycle cycle = new ComputeCycle();

            ColumnData activeColumnData = new ColumnData();

            ISet<Cell> prevActiveCells = conn.getActiveCells();
            ISet<Cell> prevWinnerCells = conn.getWinnerCells();

            // The list of active columns.
            List<Column> activeColumns = new List<Column>();

            foreach (var indx in activeColumnIndices.OrderBy(i => i))
            {
                activeColumns.Add(conn.getColumn(indx));
            }

            //Func<Object, Column> segToCol = segment => ((DistalDendrite)segment).getParentCell().getParentColumnIndex();

            Func<Object, Column> segToCol = (segment) =>
            {
                var colIndx = ((DistalDendrite)segment).GetParentCell().getParentColumnIndex();
                var parentCol = this.connections.getMemory().GetColumn(colIndx);
                return parentCol;
            };

            Func<object, Column> times1Fnc = x => (Column)x;

            var list = new Pair<List<object>, Func<object, Column>>[3];
            list[0] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(activeColumns.ToArray(), item => (object)item).ToList(), times1Fnc);
            list[1] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(conn.getActiveSegments().ToArray(), item => (object)item).ToList(), segToCol);
            list[2] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(conn.getMatchingSegments().ToArray(), item => (object)item).ToList(), segToCol);

            GroupBy2<Column> grouper = GroupBy2<Column>.of(list);

            double permanenceIncrement = conn.HtmConfig.PermanenceIncrement;
            double permanenceDecrement = conn.HtmConfig.PermanenceDecrement;

            //
            // Grouping by columns, which have active and matching segments.
            foreach (var tuple in grouper)
            {
                Console.Write(":");
                activeColumnData = activeColumnData.Set(tuple);

                if (activeColumnData.IsExistAnyActiveCol(cIndexofACTIVE_COLUMNS))
                {
                    // If there are some active segments on the column already...
                    if (activeColumnData.ActiveSegments != null && activeColumnData.ActiveSegments.Count > 0)
                    {
                        Debug.Write(".");
                        
                        List<Cell> cellsOwnersOfActSegs = ActivatePredictedColumn(conn, activeColumnData.ActiveSegments,
                            activeColumnData.MatchingSegments, prevActiveCells, prevWinnerCells,
                                permanenceIncrement, permanenceDecrement, learn);

                        foreach (var item in cellsOwnersOfActSegs)
                        {
                            cycle.ActiveCells.Add(item);
                            cycle.WinnerCells.Add(item);
                        }

                        //foreach (var item in cellsOwnersOfActSegs)
                        //{
                        //    cycle.WinnerCells.Add(item);
                        //}
                    }
                    else
                    {
                        Debug.Write("B.");
                        //
                        // If no active segments are detected (start of learning) then all cells are activated
                        // and a random single cell is chosen as a winner.
                        BurstingResult burstingResult = BurstColumn(conn, activeColumnData.Column(), activeColumnData.MatchingSegments,
                            prevActiveCells, prevWinnerCells, permanenceIncrement, permanenceDecrement, conn.getRandom(),
                               learn);

                        //
                        // Here we activate all cells by putting them to list of active cells.
                        foreach (var item in burstingResult.Cells)
                        {
                            cycle.ActiveCells.Add(item);
                        }

                        cycle.WinnerCells.Add((Cell)burstingResult.BestCell);
                    }
                }
                else
                {
                    if (learn)
                    {
                        punishPredictedColumn(conn, activeColumnData.ActiveSegments, activeColumnData.MatchingSegments,
                            prevActiveCells, prevWinnerCells, conn.getPredictedSegmentDecrement());
                    }
                }
            }


            //int[] arr = new int[cycle.winnerCells.Count];
            //int count = 0;
            //foreach (Cell activeCell in cycle.winnerCells)
            //{
            //    arr[count] = activeCell.Index;
            //    count++;
            //}

            return cycle;
        }

        /**
         * Calculate dendrite segment activity, using the current active cells.
         * 
         * <pre>
         * Pseudocode:
         *   for each distal dendrite segment with activity >= activationThreshold
         *     mark the segment as active
         *   for each distal dendrite segment with unconnected activity >= minThreshold
         *     mark the segment as matching
         * </pre>
         * 
         * @param conn     the Connectivity
         * @param cycle    Stores current compute cycle results
         * @param learn    If true, segment activations will be recorded. This information is used
         *                 during segment cleanup.
         */
        protected void ActivateDendrites(Connections conn, ComputeCycle cycle, bool learn)
        {
            SegmentActivity activity = conn.ComputeActivity(cycle.ActiveCells, conn.getConnectedPermanence());

            int i = 0;
            var activeSegments = new List<DistalDendrite>();
            foreach (var item in activity.ActiveSynapses)
            {
                if (item.Value >= conn.getActivationThreshold())
                    activeSegments.Add(conn.GetSegmentForFlatIdx(item.Key));
            }

            //
            // Step through all synapses on active cells and find involved segments.         
            var matchingSegments = new List<DistalDendrite>();
            foreach (var item in activity.PotentialSynapses)
            {
                if (item.Value >= conn.getMinThreshold())
                    matchingSegments.Add(conn.GetSegmentForFlatIdx(item.Key));
            }

            //
            // Step through all synapses on active cells with permanence over threshold (conencted synapses)
            // and find involved segments.         
            activeSegments.Sort(GetComparer(conn.getNextSegmentOrdinal()));
           
            matchingSegments.Sort(GetComparer(conn.getNextSegmentOrdinal()));
           
            cycle.ActiveSegments = activeSegments;
            cycle.MatchingSegments = matchingSegments;

            conn.lastActivity = activity;
            conn.setActiveCells(new HashSet<Cell>(cycle.ActiveCells));
            conn.setWinnerCells(new HashSet<Cell>(cycle.WinnerCells));
            conn.setActiveSegments(activeSegments);
            conn.setMatchingSegments(matchingSegments);

            // Forces generation of the predictive cells from the above active segments
            conn.clearPredictiveCells();

            //ISet<Cell> predictiveCells = conn.getPredictiveCells();
            //string[] arr = new string[predictiveCells.Count];
            //foreach (Cell c in predictiveCells)
            //{
            //    arr[i] = c.Index + "-" + c.ParentColumnIndex;
            //    i++;
            //}

            //Debug.WriteLine($"ACT: {activity.Active.Count}, POT: {activity.Potential.Count}");
            //string output = string.Join("", predictiveCells);
            //Debug.WriteLine($"Active Segs: {activeSegments.Count} Matching segs: {matchingSegments.Count}, Predicted cells: {Helpers.StringifyVector(arr)}");
            //Debug.WriteLine("-----------------------------------------------------\n-----------------------------------------------------");
         
            if (learn)
            {
                foreach (var segment in activeSegments)
                {
                    conn.recordSegmentActivity(segment);
                }

                conn.startNewIteration();
            }
        }


        /// <summary>
        /// Indicates the start of a new sequence. 
        /// Clears any predictions and makes sure synapses don't grow to the currently active cells in the next time step.
        /// </summary>
        /// <param name="connections"></param>
        public void reset(Connections connections)
        {
            connections.getActiveCells().Clear();
            connections.getWinnerCells().Clear();
            connections.getActiveSegments().Clear();
            connections.getMatchingSegments().Clear();
        }

        /**
 * Determines which cells in a predicted column should be added to winner cells
 * list, and learns on the segments that correctly predicted this column.
 * 
 * @param conn                 the connections
 * @param activeSegments       Active segments in the specified column
 * @param matchingSegments     Matching segments in the specified column
 * @param prevActiveCells      Active cells in `t-1`
 * @param prevWinnerCells      Winner cells in `t-1`
 * @param learn                If true, grow and reinforce synapses
 * 
 * <pre>
 * Pseudocode:
 *   for each cell in the column that has an active distal dendrite segment
 *     mark the cell as active
 *     mark the cell as a winner cell
 *     (learning) for each active distal dendrite segment
 *       strengthen active synapses
 *       weaken inactive synapses
 *       grow synapses to previous winner cells
 * </pre>
 * 
 * @return A list of predicted cells that will be added to active cells and winner
 *         cells.
 */


        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="columnActiveSegments">Active segments as calculated (activated) in the previous step.</param>
        /// <param name="matchingSegments"></param>
        /// <param name="prevActiveCells"></param>
        /// <param name="prevWinnerCells"></param>
        /// <param name="permanenceIncrement"></param>
        /// <param name="permanenceDecrement"></param>
        /// <param name="learn"></param>
        /// <returns>Cells, which own active columnActiveSegments as calculated in the previous step.</returns>
        private List<Cell> ActivatePredictedColumn(Connections conn, List<DistalDendrite> columnActiveSegments,
            List<DistalDendrite> matchingSegments, ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells,
                double permanenceIncrement, double permanenceDecrement, bool learn)
        {
            List<Cell> cellsOwnersOfActiveSegments = new List<Cell>();
            Cell previousCell = null;
            Cell segmOwnerCell;
                    
            foreach (DistalDendrite segment in columnActiveSegments)
            {              
                // TODO
                // Review this. not only previous cell should be consiered.
                // We should rather consider all current list and look if the cell is already in.
                segmOwnerCell = segment.GetParentCell();
                if (segmOwnerCell != previousCell)
                {
                    cellsOwnersOfActiveSegments.Add(segmOwnerCell);
                    previousCell = segmOwnerCell;
                }
                else
                { 
                
                }

                if (learn)
                {
                    adaptSegment(conn, segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    int numActive = conn.getLastActivity().PotentialSynapses[segment.getIndex()];
                    int nGrowDesired = conn.HtmConfig.MaxNewSynapseCount - numActive;

                    if (nGrowDesired > 0)
                    {
                        growSynapses(conn, prevWinnerCells, segment, conn.getInitialPermanence(),
                            nGrowDesired, conn.getRandom());
                    }
                }
            }

            return cellsOwnersOfActiveSegments;
        }

        /**
         * Activates all of the cells in an unpredicted active column,
         * chooses a winner cell, and, if learning is turned on, either adapts or
         * creates a segment. growSynapses is invoked on this segment.
         * </p><p>
         * <b>Pseudocode:</b>
         * </p><p>
         * <pre>
         *  mark all cells as active
         *  if there are any matching distal dendrite segments
         *      find the most active matching segment
         *      mark its cell as a winner cell
         *      (learning)
         *      grow and reinforce synapses to previous winner cells
         *  else
         *      find the cell with the least segments, mark it as a winner cell
         *      (learning)
         *      (optimization) if there are previous winner cells
         *          add a segment to this winner cell
         *          grow synapses to previous winner cells
         * </pre>
         * </p>
         * 
         * @param conn                      Connections instance for the TM
         * @param column                    Bursting {@link Column}
         * @param matchingSegments          List of matching {@link DistalDendrite}s
         * @param prevActiveCells           Active cells in `t-1`
         * @param prevWinnerCells           Winner cells in `t-1`
         * @param permanenceIncrement       Amount by which permanences of synapses
         *                                  are decremented during learning
         * @param permanenceDecrement       Amount by which permanences of synapses
         *                                  are incremented during learning
         * @param random                    Random number generator
         * @param learn                     Whether or not learning is enabled
         * 
         * @return  Tuple containing:
         *                  cells       list of the processed column's cells
         *                  bestCell    the best cell
         */
        public BurstingResult BurstColumn(Connections conn, Column column, List<DistalDendrite> matchingSegments,
            ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells, double permanenceIncrement, double permanenceDecrement,
                Random random, bool learn)
        {

            IList<Cell> cells = column.Cells;
            Cell leastUsedCell = null;

            //
            // Matching segments result from number of potential synapses. These are segments with number of potential
            // synapses permanence higher than some minimum threshold value.
            // Potential synapses are synapses from presynaptc cells connected to the active cell.
            // In other words, presynaptic cells define a statistical prediction that active cell will become the active in the next cycle.
            // Bursting will create new segments if there are no matching segments until some matching segments appear. 
            // Once that happen, segment adoption will start.
            // If some matching segments exist, bursting will grab the segment with most potential synapses and adapt it.
            if (matchingSegments != null && matchingSegments.Count > 0)
            {
                DistalDendrite maxPotentialSeg = getSegmentwithHighesPotential(conn, matchingSegments);

                for (int i = 0; i < matchingSegments.Count; i++)
                {
                    matchingSegments[i].getIndex();
                }

                leastUsedCell = maxPotentialSeg.GetParentCell();

                if (learn)
                {
                    adaptSegment(conn, maxPotentialSeg, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    int nGrowDesired = conn.getMaxNewSynapseCount() - conn.getLastActivity().PotentialSynapses[maxPotentialSeg.getIndex()];

                    if (nGrowDesired > 0)
                    {
                        growSynapses(conn, prevWinnerCells, maxPotentialSeg, conn.getInitialPermanence(),
                            nGrowDesired, random);
                    }
                }
            }
            else
            {
                leastUsedCell = this.GetLeastUsedCell(conn, cells, random);
                if (learn)
                {
                    int nGrowExact = Math.Min(conn.getMaxNewSynapseCount(), prevWinnerCells.Count);
                    if (nGrowExact > 0)
                    {
                        DistalDendrite bestSegment = conn.CreateDistalSegment(leastUsedCell);
                        growSynapses(conn, prevWinnerCells, bestSegment, conn.getInitialPermanence(),
                            nGrowExact, random);
                    }
                }
            }

            return new BurstingResult(cells, leastUsedCell);
        }


        /// <summary>
        /// Gets the segment with maximal potential. Segment's potential is measured by number of potential synapses.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="matchingSegments"></param>
        /// <returns></returns>
        private DistalDendrite getSegmentwithHighesPotential(Connections conn, List<DistalDendrite> matchingSegments)
        {
            // int[] numActPotential = conn.getLastActivity().numActivePotential;

            DistalDendrite maxSeg = matchingSegments[0];

            for (int i = 0; i < matchingSegments.Count - 1; i++)
            {
                if (conn.getLastActivity().PotentialSynapses[matchingSegments[i + 1].getIndex()] > conn.getLastActivity().PotentialSynapses[matchingSegments[i].getIndex()])
                    maxSeg = matchingSegments[i + 1];
            }
            return maxSeg;
        }

        /**
         * Punishes the Segments that incorrectly predicted a column to be active.
         * 
         * <p>
         * <pre>
         * Pseudocode:
         *  for each matching segment in the column
         *    weaken active synapses
         * </pre>
         * </p>
         *   
         * @param conn                              Connections instance for the tm
         * @param activeSegments                    An iterable of {@link DistalDendrite} actives
         * @param matchingSegments                  An iterable of {@link DistalDendrite} matching
         *                                          for the column compute is operating on
         *                                          that are matching; None if empty
         * @param prevActiveCells                   Active cells in `t-1`
         * @param prevWinnerCells                   Winner cells in `t-1`
         *                                          are decremented during learning.
         * @param predictedSegmentDecrement         Amount by which segments are punished for incorrect predictions
         */
        public void punishPredictedColumn(Connections conn, List<DistalDendrite> activeSegments,
            List<DistalDendrite> matchingSegments, ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells,
               double predictedSegmentDecrement)
        {

            if (predictedSegmentDecrement > 0)
            {
                foreach (DistalDendrite segment in matchingSegments)
                {
                    adaptSegment(conn, segment, prevActiveCells, -conn.getPredictedSegmentDecrement(), 0);
                }
            }
        }


        ////////////////////////////
        //     Helper Methods     //
        ////////////////////////////


        /// <summary>
        /// Gets the cell with the smallest number of segments.
        /// </summary>
        /// <param name="conn">Connections instance currentlly in use.</param>
        /// <param name="cells">List of cells.</param>
        /// <param name="random">Random generator.</param>
        /// <returns></returns>
        public Cell GetLeastUsedCell(Connections conn, IList<Cell> cells, Random random)
        {
            List<Cell> leastUsedCells = new List<Cell>();
            int minNumSegments = Integer.MaxValue;
            foreach (Cell cell in cells)
            {
                int numSegments = conn.numSegments(cell);

                if (numSegments < minNumSegments)
                {
                    minNumSegments = numSegments;
                    leastUsedCells.Clear();
                }

                if (numSegments == minNumSegments)
                {
                    leastUsedCells.Add(cell);
                }
            }
            random = new Random();
            int i = random.Next(leastUsedCells.Count);
            return leastUsedCells[i];
        }

        /**
         * Creates nDesiredNewSynapes synapses on the segment passed in if
         * possible, choosing random cells from the previous winner cells that are
         * not already on the segment.
         * <p>
         * <b>Notes:</b> The process of writing the last value into the index in the array
         * that was most recently changed is to ensure the same results that we get
         * in the c++ implementation using iter_swap with vectors.
         * </p>
         * 
         * @param conn                      Connections instance for the tm
         * @param prevWinnerCells           Winner cells in `t-1`
         * @param segment                   Segment to grow synapses on.     
         * @param initialPermanence         Initial permanence of a new synapse.
         * @param nDesiredNewSynapses       Desired number of synapses to grow
         * @param random                    Tm object used to generate random
         *                                  numbers
         */
        public void growSynapses(Connections conn, ICollection<Cell> prevWinnerCells, DistalDendrite segment,
            double initialPermanence, int nDesiredNewSynapses, Random random)
        {
            random = new Random();
            List<Cell> removingCandidates = new List<Cell>(prevWinnerCells);
            removingCandidates = removingCandidates.OrderBy(c => c).ToList();

            //
            // Enumarates all synapses in a segment and remove winner-cells from
            // list of removingCandidates if they are presynaptic winners cells.
            // So, we will recreate only synapses on cells, which are not winners in the previous step.
            foreach (Synapse synapse in conn.getSynapses(segment))
            {
                Cell presynapticCell = synapse.getPresynapticCell();
                int index = removingCandidates.IndexOf(presynapticCell);
                if (index != -1)
                {
                    removingCandidates.RemoveAt(index);
                }
            }

            int candidatesLength = removingCandidates.Count();

            // We take here eather wanted growing number of desired synapes of num of candidates
            // if too many growing synapses requested.
            int nActual = nDesiredNewSynapses < candidatesLength ? nDesiredNewSynapses : candidatesLength;

            //
            // Finally we randomly create new synapses. 
            for (int i = 0; i < nActual; i++)
            {
                int rndIndex = random.Next(removingCandidates.Count());
                conn.createSynapse(segment, removingCandidates[rndIndex], initialPermanence);
                removingCandidates.RemoveAt(rndIndex);
            }
        }

        /**
         * Updates synapses on segment.
         * Strengthens active synapses; weakens inactive synapses.
         *  
         * @param conn                      {@link Connections} instance for the tm
         * @param segment                   {@link DistalDendrite} to adapt
         * @param prevActiveCells           Active {@link Cell}s in `t-1`
         * @param permanenceIncrement       Amount to increment active synapses    
         * @param permanenceDecrement       Amount to decrement inactive synapses
         */
        public void adaptSegment(Connections conn, DistalDendrite segment, ICollection<Cell> prevActiveCells,
            double permanenceIncrement, double permanenceDecrement)
        {

            // Destroying a synapse modifies the set that we're iterating through.
            List<Synapse> synapsesToDestroy = new List<Synapse>();

            foreach (Synapse synapse in conn.getSynapses(segment))
            {
                double permanence = synapse.getPermanence();

                //
                // If synapse's presynaptic cell was active in the previous cycle then streng it.
                if (prevActiveCells.Contains(synapse.getPresynapticCell()))
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

                if (permanence < EPSILON)
                {
                    synapsesToDestroy.Add(synapse);
                }
                else
                {
                    synapse.setPermanence(conn.getSynPermConnected(), permanence);
                }
            }

            foreach (Synapse s in synapsesToDestroy)
            {
                conn.destroySynapse(s, segment);
            }

            if (conn.GetNumSynapses(segment) == 0)
            {
                conn.destroySegment(segment);
            }
        }

        /**
         * Used in the {@link TemporalMemory#compute(Connections, int[], boolean)} method
         * to make pulling values out of the {@link GroupBy2} more readable and named.
         */

        public class ColumnData
        {
            private Pair<Column, List<List<Object>>> m_Pair;

            public ColumnData() { }


            public ColumnData Set(Pair<Column, List<List<Object>>> t)
            {
                m_Pair = t;

                return this;
            }

            public Column Column() { return (Column)m_Pair.Key; }

            public List<Column> activeColumns() { return (List<Column>)m_Pair.Value[0].Cast<Column>(); }

            public List<DistalDendrite> ActiveSegments
            {
                get
                {
                    if (m_Pair.Value.Count == 0 ||
                        m_Pair.Value[1].Count == 0)
                        return new List<DistalDendrite>();
                    else
                        return m_Pair.Value[1].Cast<DistalDendrite>().ToList();
                }
            }

            public List<DistalDendrite> MatchingSegments
            {
                get
                {
                    if (m_Pair.Value.Count == 0 ||
                         m_Pair.Value[2].Count == 0)
                        return new List<DistalDendrite>();
                    else
                        return m_Pair.Value[2].Cast<DistalDendrite>().ToList();
                }
            }


            /// <summary>
            /// Result indicates whether the slot at the specified index is empty</summary>
            /// indicator.<param name="memberIndex">Index of slot.</param>
            /// <returns></returns>
            public bool IsExistAnyActiveCol(int memberIndex)
            {
                if (m_Pair.Value.Count == 0 ||
                    m_Pair.Value[memberIndex].Count == 0)
                    return false;
                else
                    return true;
            }
        }

        public DentriteComparer GetComparer(int nextSegmentOrdinal)
        {
            return new DentriteComparer(nextSegmentOrdinal);
        }
    }
}
