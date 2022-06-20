using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
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
        private CorticalArea area;

        private HtmConfig cfg

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
            var col = this.area.Columns.FirstOrDefault(i=>i.Index==index);

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
                        //Debug.Write("A");1    

                        List<Cell> cellsOwnersOfActSegs = ActivatePredictedColumn(conn, activeColumnData.ActiveSegments,
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
                        BurstingResult burstingResult = BurstColumn(conn, activeColumnData.Column(), activeColumnData.MatchingSegments,
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
                        PunishPredictedColumn(conn, activeColumnData.ActiveSegments, activeColumnData.MatchingSegments,
                            inp.ActiveCells, inp.WinnerCells, this.cfg.PredictedSegmentDecrement);
                    }
                }
            }

            return newComputeCycle;
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
        protected List<Cell> ActivatePredictedColumn(Connections conn, List<DistalDendrite> columnActiveSegments,
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
                    AdaptSegment(conn, segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

                    //
                    // Even if the segment is active, new synapses can be added that connect previously active cells with the segment.
                    int numActive = this.LastActivity.PotentialSynapses[segment.SegmentIndex];
                    int nGrowDesired = conn.HtmConfig.MaxNewSynapseCount - numActive;

                    if (nGrowDesired > 0)
                    {
                        // Create new synapses on the segment from winner (pre-synaptic cells) cells.
                        GrowSynapses(conn, prevWinnerCells, segment, conn.HtmConfig.InitialPermanence,
                            nGrowDesired, conn.HtmConfig.Random);
                    }
                    else
                    {
                        // for debugging.
                    }
                }
            }

            return cellsOwnersOfActiveSegments;
        }
    }
}
