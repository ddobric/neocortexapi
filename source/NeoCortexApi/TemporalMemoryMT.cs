using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NeoCortexApi
{
    /// <summary>
    /// Multicore implementation of the Temporal Memory algorithm
    /// </summary>
    public class TemporalMemoryMT : TemporalMemory
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="activeColumnIndices"></param>
        /// <param name="learn"></param>
        /// <returns></returns>
        protected override ComputeCycle ActivateCells(Connections conn, int[] activeColumnIndices, bool learn)
        {
            ComputeCycle cycle = new ComputeCycle
            {
                ActivColumnIndicies = activeColumnIndices
            };

            ConcurrentDictionary<int, ComputeCycle> cycles = new ConcurrentDictionary<int, ComputeCycle>();

            ISet<Cell> prevActiveCells = conn.ActiveCells;
            ISet<Cell> prevWinnerCells = conn.WinnerCells;

            // The list of active columns.
            List<Column> activeColumns = new List<Column>();

            foreach (var indx in activeColumnIndices.OrderBy(i => i))
            {
                activeColumns.Add(conn.GetColumn(indx));
            }

            Func<Object, Column> segToCol = (segment) =>
            {
                var colIndx = ((DistalDendrite)segment).ParentCell.ParentColumnIndex;
                var parentCol = this.connections.Memory.GetColumn(colIndx);
                return parentCol;
            };

            Func<object, Column> times1Fnc = x => (Column)x;

            var list = new Pair<List<object>, Func<object, Column>>[3];
            list[0] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(activeColumns.ToArray(), item => (object)item).ToList(), times1Fnc);
            list[1] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(conn.ActiveSegments.ToArray(), item => (object)item).ToList(), segToCol);
            list[2] = new Pair<List<object>, Func<object, Column>>(Array.ConvertAll(conn.MatchingSegments.ToArray(), item => (object)item).ToList(), segToCol);

            GroupBy2<Column> grouper = GroupBy2<Column>.Of(list);

            double permanenceIncrement = conn.HtmConfig.PermanenceIncrement;
            double permanenceDecrement = conn.HtmConfig.PermanenceDecrement;

            ParallelOptions opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            //
            // Grouping by columns, which have active and matching segments.
            Parallel.ForEach(grouper, opts, (tuple) =>
            {
                ActivateCellsInColumn(conn, learn, tuple, cycle, cycles, prevActiveCells, prevWinnerCells, permanenceIncrement, permanenceDecrement);
            });

            foreach (var colCycle in cycles.Values)
            {
                cycle.ActiveCells.AddRange(colCycle.ActiveCells);
                cycle.WinnerCells.AddRange(colCycle.WinnerCells);
            }

            return cycle;
        }

        private void ActivateCellsInColumn(Connections conn, bool learn, Pair<Column, List<List<object>>> tuple, ComputeCycle cycle, ConcurrentDictionary<int, ComputeCycle> cycles, ISet<Cell> prevActiveCells, ISet<Cell> prevWinnerCells, double permanenceIncrement, double permanenceDecrement)
        {
            ColumnData activeColumnData = new ColumnData();

            activeColumnData.Set(tuple);

            if (activeColumnData.IsExistAnyActiveCol(cIndexofACTIVE_COLUMNS))
            {
                // If there are some active segments on the column already...
                if (activeColumnData.ActiveSegments != null && activeColumnData.ActiveSegments.Count > 0)
                {
                    Debug.Write("A");

                    List<Cell> cellsOwnersOfActSegs = ActivatePredictedColumn(conn, activeColumnData.ActiveSegments,
                        activeColumnData.MatchingSegments, prevActiveCells, prevWinnerCells,
                            permanenceIncrement, permanenceDecrement, learn, cycle.ActiveSynapses);

                    ComputeCycle colCycle = new ComputeCycle();
                    cycles[tuple.Key.Index] = colCycle;

                    foreach (var item in cellsOwnersOfActSegs)
                    {
                        colCycle.ActiveCells.Add(item);
                        colCycle.WinnerCells.Add(item);
                    }
                }
                else
                {
                    Debug.Write("B");

                    //
                    // If no active segments are detected (start of learning) then all cells are activated
                    // and a random single cell is chosen as a winner.
                    BurstingResult burstingResult = BurstColumn(conn, activeColumnData.Column(), activeColumnData.MatchingSegments,
                        prevActiveCells, prevWinnerCells, permanenceIncrement, permanenceDecrement, conn.HtmConfig.Random,
                           learn);


                    ComputeCycle colCycle = new ComputeCycle();
                    cycles[tuple.Key.Index] = colCycle;

                    //
                    // Here we activate all cells by putting them to list of active cells.
                    foreach (var item in burstingResult.Cells)
                    {
                        colCycle.ActiveCells.Add(item);
                    }

                    colCycle.WinnerCells.Add((Cell)burstingResult.BestCell);
                }
            }
            else
            {
                if (learn)
                {
                    PunishPredictedColumn(conn, activeColumnData.ActiveSegments, activeColumnData.MatchingSegments,
                        prevActiveCells, prevWinnerCells, conn.HtmConfig.PredictedSegmentDecrement);
                }
            }
        }

    }
}
