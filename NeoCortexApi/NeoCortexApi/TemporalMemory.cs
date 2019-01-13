using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Utility;
using static NeoCortexApi.Entities.Connections;


namespace NeoCortexApi
{

    /**
     * Temporal Memory implementation in Java.
     * 
     * @author Numenta
     * @author cogmission
     */
    public class TemporalMemory : IHtmModule//: IComputeDecorator
    {
        /** simple serial version id */
        private static readonly long serialVersionUID = 1L;

        private static readonly double EPSILON = 0.00001;

        private static readonly int cIndexofACTIVE_COLUMNS = 0;

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


        public static void init(Connections c)
        {
            SparseObjectMatrix<Column> matrix = c.getMemory() == null ?
                new SparseObjectMatrix<Column>(c.getColumnDimensions()) :
                    c.getMemory();
            c.setMemory(matrix);

            int numColumns = matrix.getMaxIndex() + 1;
            c.setNumColumns(numColumns);
            int cellsPerColumn = c.getCellsPerColumn();
            Cell[] cells = new Cell[numColumns * cellsPerColumn];

            //Used as flag to determine if Column objects have been created.
            Column colZero = matrix.getObject(0);
            for (int i = 0; i < numColumns; i++)
            {
                Column column = colZero == null ? new Column(cellsPerColumn, i) : matrix.getObject(i);
                for (int j = 0; j < cellsPerColumn; j++)
                {
                    cells[i * cellsPerColumn + j] = column.getCell(j);
                }
                //If columns have not been previously configured
                if (colZero == null)
                    matrix.set(i, column);
            }
            //Only the TemporalMemory initializes cells so no need to test for redundancy
            c.setCells(cells);
        }


        public ComputeCycle Compute(Connections connections, int[] activeColumns, bool learn)
        {
            ComputeCycle cycle = new ComputeCycle();
            activateCells(connections, cycle, activeColumns, learn);
            activateDendrites(connections, cycle, learn);

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

        public void activateCells(Connections conn, ComputeCycle cycle, int[] activeColumnIndices, bool learn)
        {
            ColumnData columnData = new ColumnData();

            ISet<Cell> prevActiveCells = conn.getActiveCells();
            ISet<Cell> prevWinnerCells = conn.getWinnerCells();


            // The list of active columns.
            List<Column> activeColumns = new List<Column>();

            foreach (var indx in activeColumnIndices.OrderBy(i => i))
            {
                activeColumns.Add(conn.getColumn(indx));
            }

            Func<Object, Column> segToCol = segment => ((DistalDendrite)segment).getParentCell().getColumn();

            Func<object, Column> times1Fnc = x => (Column)x;

            var list = new Pair<List<object>, Func<object, Column>>[3];
            list[0] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(activeColumns.ToArray(), item => (object)item).ToList(), times1Fnc);
            list[1] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(conn.getActiveSegments().ToArray(), item => (object)item).ToList(), segToCol);
            list[2] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(conn.getMatchingSegments().ToArray(), item => (object)item).ToList(), segToCol);

            GroupBy2<Column> grouper = GroupBy2<Column>.of(list);

            double permanenceIncrement = conn.getPermanenceIncrement();
            double permanenceDecrement = conn.getPermanenceDecrement();

            //
            // Grouping by active columns.
            foreach (var t in grouper)
            {
                columnData = columnData.set(t);

                if (columnData.isExistsAnyActiveCol(cIndexofACTIVE_COLUMNS))
                {
                    // If there some active segment already...
                    if (columnData.activeSegments != null && columnData.activeSegments.Count > 0)
                    {
                        List<Cell> cellsToAdd = activatePredictedColumn(conn, columnData.activeSegments,
                            columnData.matchingSegments, prevActiveCells, prevWinnerCells,
                                permanenceIncrement, permanenceDecrement, learn);

                        foreach (var item in cellsToAdd)
                        {
                            cycle.activeCells.Add(item);
                        }

                        foreach (var item in cellsToAdd)
                        {
                            cycle.winnerCells.Add(item);
                        }
                    }
                    else
                    {
                        //
                        // If no active segments are detected (start of learning) then all cells are activated
                        // and a random single cell is chosen as a winner.
                        //
                        BurstingTupple burstingResult = BurstColumn(conn, columnData.Column(), columnData.matchingSegments,
                            prevActiveCells, prevWinnerCells, permanenceIncrement, permanenceDecrement, conn.getRandom(),
                               learn);

                        // Here we activate all cells by putting them to list of active cells.
                        foreach (var item in burstingResult.Cells)
                        {
                            cycle.activeCells.Add(item);
                        }

                        cycle.winnerCells.Add((Cell)burstingResult.BestCell);
                    }
                }
                else
                {
                    if (learn)
                    {
                        punishPredictedColumn(conn, columnData.activeSegments, columnData.matchingSegments,
                            prevActiveCells, prevWinnerCells, conn.getPredictedSegmentDecrement());
                    }
                }
            }
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
        public void activateDendrites(Connections conn, ComputeCycle cycle, bool learn)
        {
            SegmentActivity activity = conn.computeActivity(cycle.activeCells, conn.getConnectedPermanence());

            var activeSegments = new List<DistalDendrite>();
            foreach (var item in activity.Active)
            {
                if (item.Value >= conn.getActivationThreshold())
                    activeSegments.Add(conn.GetSegmentForFlatIdx(item.Key));
            }

            //
            // Step through all synapses on active cells and find involved segments.         
            //var activeSegments = activity.numActiveConnected.Where(i => i >= conn.getActivationThreshold()).
            //        Select(indx => conn.GetSegmentForFlatIdx(indx)).ToList();

            var matchingSegments = new List<DistalDendrite>();
            foreach (var item in activity.Potential)
            {
                if (item.Value >= conn.getMinThreshold())
                    matchingSegments.Add(conn.GetSegmentForFlatIdx(item.Key));
            }

            //
            // Step through all synapses on active cells with permanence over threshold (conencted synapses)
            // and find involved segments.         
            //var matchingSegments = activity.numActiveConnected.Where(
            //    i => activity.numActivePotential[i] >= conn.getMinThreshold()).
            //        Select(indx => conn.GetSegmentForFlatIdx(indx)).ToList();


            activeSegments.Sort(conn.GetComparer());
            //Collections.sort(activeSegments, conn.segmentPositionSortKey);
            matchingSegments.Sort(conn.GetComparer());
            //Collections.sort(matchingSegments, conn.segmentPositionSortKey);

            cycle.activeSegments = activeSegments;
            cycle.matchingSegments = matchingSegments;

            conn.lastActivity = activity;
            conn.setActiveCells(new HashSet<Cell>(cycle.activeCells));
            conn.setWinnerCells(new HashSet<Cell>(cycle.winnerCells));
            conn.setActiveSegments(activeSegments);
            conn.setMatchingSegments(matchingSegments);
            // Forces generation of the predictive cells from the above active segments
            conn.clearPredictiveCells();
            conn.getPredictiveCells();

            if (learn)
            {
                foreach (var segment in activeSegments)
                {
                    conn.recordSegmentActivity(segment);
                }

                conn.startNewIteration();
            }
        }

        /**
         * Indicates the start of a new sequence. Clears any predictions and makes sure
         * synapses don't grow to the currently active cells in the next time step.
         */

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
        public List<Cell> activatePredictedColumn(Connections conn, List<DistalDendrite> activeSegments,
            List<DistalDendrite> matchingSegments, ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells,
                double permanenceIncrement, double permanenceDecrement, bool learn)
        {

            List<Cell> cellsToAdd = new List<Cell>();
            Cell previousCell = null;
            Cell currCell;
            foreach (DistalDendrite segment in activeSegments)
            {
                if ((currCell = segment.getParentCell()) != previousCell)
                {
                    cellsToAdd.Add(currCell);
                    previousCell = currCell;
                }

                if (learn)
                {
                    adaptSegment(conn, segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    int numActive = conn.getLastActivity().Potential[segment.getIndex()];
                    int nGrowDesired = conn.getMaxNewSynapseCount() - numActive;

                    if (nGrowDesired > 0)
                    {
                        growSynapses(conn, prevWinnerCells, segment, conn.getInitialPermanence(),
                            nGrowDesired, conn.getRandom());
                    }
                }
            }

            return cellsToAdd;
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
        public BurstingTupple BurstColumn(Connections conn, Column column, List<DistalDendrite> matchingSegments,
            ICollection<Cell> prevActiveCells, ICollection<Cell> prevWinnerCells, double permanenceIncrement, double permanenceDecrement,
                Random random, bool learn)
        {

            IList<Cell> cells = column.getCells();
            Cell leastUsedCell = null;

            if (matchingSegments != null && matchingSegments.Count > 0)
            {
                //int[] numPotential = conn.getLastActivity().numActivePotential;
                //Comparator<DistalDendrite> cmp = (dd1, dd2)->numPoten[dd1.getIndex()] - numPoten[dd2.getIndex()];
                //DistalDendrite bestSegment = matchingSegments.stream().max(cmp).get();
                //    matchingSegments.Where((dd1, dd2) => numPotential[dd1.getIndex()] - numPotential[dd2.getIndex()]);

                DistalDendrite bestSegment = getSegmentwithHighesPotential(conn, matchingSegments);

                for (int i = 0; i < matchingSegments.Count; i++)
                {
                    matchingSegments[i].getIndex();
                }

                leastUsedCell = bestSegment.getParentCell();

                if (learn)
                {
                    adaptSegment(conn, bestSegment, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    int nGrowDesired = conn.getMaxNewSynapseCount() - conn.getLastActivity().Potential[bestSegment.getIndex()];

                    if (nGrowDesired > 0)
                    {
                        growSynapses(conn, prevWinnerCells, bestSegment, conn.getInitialPermanence(),
                            nGrowDesired, random);
                    }
                }
            }
            else
            {
                leastUsedCell = this.leastUsedCell(conn, cells, random);
                if (learn)
                {
                    int nGrowExact = Math.Min(conn.getMaxNewSynapseCount(), prevWinnerCells.Count);
                    if (nGrowExact > 0)
                    {
                        DistalDendrite bestSegment = conn.createSegment(leastUsedCell);
                        growSynapses(conn, prevWinnerCells, bestSegment, conn.getInitialPermanence(),
                            nGrowExact, random);
                    }
                }
            }

            return new BurstingTupple(cells, leastUsedCell);
        }


        /// <summary>
        /// Gets the segment with maximal potential.
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
                if (conn.getLastActivity().Potential[matchingSegments[i + 1].getIndex()] > conn.getLastActivity().Potential[matchingSegments[i].getIndex()])
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

        /**
         * Gets the cell with the smallest number of segments.
         * Break ties randomly.
         * 
         * @param conn      Connections instance for the tm
         * @param cells     List of {@link Cell}s
         * @param random    Random Number Generator
         * 
         * @return  the least used {@code Cell}
         */
        public Cell leastUsedCell(Connections conn, IList<Cell> cells, Random random)
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

            List<Cell> removingCandidates = new List<Cell>(prevWinnerCells);
            removingCandidates = removingCandidates.OrderBy(c => c).ToList();

            //
            // Enumarates all synapses in a segment and remove winner-cells from
            // list of removingCandidates if they are presynaptic winners cells.
            // So, we will recreate only synapses on cells, which are not winners.
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
                    synapse.setPermanence(conn, permanence);
                }
            }

            foreach (Synapse s in synapsesToDestroy)
            {
                conn.destroySynapse(s);
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
            /** Default Serial */
            private static readonly long serialVersionUID = 1L;

            private Pair<Column, List<List<Object>>> m_Pair;

            public ColumnData() { }


            public ColumnData set(Pair<Column, List<List<Object>>> t)
            {
                m_Pair = t;

                return this;
            }

            public Column Column() { return (Column)m_Pair.Key; }

            public List<Column> activeColumns() { return (List<Column>)m_Pair.Value[0].Cast<Column>(); }

            public List<DistalDendrite> activeSegments
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

            public List<DistalDendrite> matchingSegments
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


            /**
             * Returns a boolean flag indicating whether the slot contained by the
             * tuple at the specified index is filled with the special empty
             * indicator.
             * 
             * @param memberIndex   the index of the tuple to assess.
             * @return  true if <em><b>not</b></em> none, false if it <em><b>is none</b></em>.
             */
            public bool isExistsAnyActiveCol(int memberIndex)
            {
                if (m_Pair.Value.Count == 0 ||
                    m_Pair.Value[memberIndex].Count == 0 )
                   // m_Pair.Value[memberIndex][0] == NeoCortexApi.Utility.GroupBy2<Column>.Slot<Pair<object, List<Column>>>.empty() )
                    return false;
                else
                    return true;
            }
        }
    }
}
