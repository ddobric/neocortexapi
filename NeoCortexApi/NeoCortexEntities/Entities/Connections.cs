// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Types;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Contains the definition of the interconnected structural state of the SpatialPooler and
    /// TemporalMemory as well as the state of Cells, Columns, Segments, Synapses etc..
    /// </summary>
    public class Connections
    {

        public static readonly double EPSILON = 0.00001;

        //Internal state
        private double version = 1.0;

        /// <summary>
        /// The number of compute calls on the SP instance.
        /// </summary>
        public int SpIterationNum { get; set; } = 0;

        /// <summary>
        /// The number of compute calls of the SP instance with enabled learning.
        /// </summary>
        public int SpIterationLearnNum { get; set; } = 0;

        private long m_TMIteration = 0;

        private double[] m_BoostedmOverlaps;

        private int[] m_Overlaps;

        /// <summary>
        /// Initialize a tiny random tie breaker. This is used to determine winning
        /// columns where the overlaps are identical.
        /// </summary>
        private double[] m_TieBreaker;

        /// <summary>
        /// Stores the number of connected synapses for each column. This is simply
        /// a sum of each row of 'connectedSynapses'. again, while this
        /// information is readily available from 'connectedSynapses', it is
        /// stored separately for efficiency purposes.
        /// </summary>
        private AbstractSparseBinaryMatrix connectedCounts2;

        /// <summary>
        /// The cells currently active as a result of the TM compute.
        /// </summary>
        public ISet<Cell> ActiveCells { get => m_ActiveCells; set => m_ActiveCells = value; }

        /// <summary>
        /// The winner cells in the current TM compute cycle. Cctive cells are winner cells in the trained TM.
        /// If the TM is not trained, segment has no active cells and all cells will be activated (bursting).
        /// One of all active column cells will be selected as the winner cell.
        /// </summary>
        public ISet<Cell> WinnerCells { get => winnerCells; set => winnerCells = value; }

        /// <summary>
        /// All cells. Initialized during initialization of the TemporalMemory.
        /// </summary>
        public Cell[] Cells { get; set; }

        private double[] m_BoostFactors;

        private ISet<Cell> m_ActiveCells = new LinkedHashSet<Cell>();
        private ISet<Cell> winnerCells = new LinkedHashSet<Cell>();
        private ISet<Cell> m_PredictiveCells = new LinkedHashSet<Cell>();
        private List<DistalDendrite> m_ActiveSegments = new List<DistalDendrite>();
        private List<DistalDendrite> m_MatchingSegments = new List<DistalDendrite>();


        private HtmConfig m_HtmConfig;

        public HtmConfig HtmConfig
        {
            get
            {
                //if (m_HtmConfig == null)
                //{
                //    HtmConfig cfg = new HtmConfig();
                //    cfg.ColumnTopology = this.ColumnTopology;
                //    cfg.InputTopology = this.InputTopology;
                //    cfg.IsWrapAround = this.isWrapAround();
                //    cfg.NumInputs = this.NumInputs;
                //    cfg.NumColumns = this.getMemory() != null? this.getMemory().getMaxIndex() + 1 : -1;
                //    cfg.PotentialPct = getPotentialPct();
                //    cfg.PotentialRadius = getPotentialRadius();
                //    cfg.SynPermConnected = getSynPermConnected();
                //    cfg.InitialSynapseConnsPct = this.InitialSynapseConnsPct;
                //    cfg.SynPermTrimThreshold = this.getSynPermTrimThreshold();
                //    cfg.SynPermBelowStimulusInc = this.synPermBelowStimulusInc;
                //    cfg.SynPermMax = this.getSynPermMax();
                //    cfg.SynPermMin = this.getSynPermMin();
                //    cfg.StimulusThreshold = this.StimulusThreshold;
                //    cfg.CellsPerColumn = this.getCellsPerColumn();
                //    cfg.SynPermInactiveDec = this.getSynPermInactiveDec();
                //    cfg.PermanenceIncrement = this.getPermanenceIncrement();
                //    cfg.PermanenceDecrement = this.getPermanenceDecrement();
                //    //cfg.MaxNewSynapseCount = this.getMaxNewSynapseCount();

                //    cfg.RandomGenSeed = this.seed;

                //    m_HtmConfig = cfg;
                //}

                // TODO verify with unitTests
                //m_HtmConfig.SynPermBelowStimulusInc = m_HtmConfig.SynPermConnected / 10.0;
                //m_HtmConfig.SynPermTrimThreshold = m_HtmConfig.SynPermActiveInc / 2.0;
                //m_HtmConfig.ColumnModuleTopology = m_HtmConfig.Memory?.ModuleTopology;
                //m_HtmConfig.InputModuleTopology = m_HtmConfig.InputMatrix?.ModuleTopology;

                //m_HtmConfig.InputTopology = this.InputTopology;
                //m_HtmConfig.IsWrapAround = this.isWrapAround();
                //m_HtmConfig.NumInputs = this.NumInputs;
                //m_HtmConfig.NumColumns = m_HtmConfig.Memory != null ? m_HtmConfig.Memory.getMaxIndex() + 1 : -1;
                //m_HtmConfig.PotentialPct = getPotentialPct();
                //m_HtmConfig.PotentialRadius = getPotentialRadius();
                //m_HtmConfig.SynPermConnected = getSynPermConnected();
                //m_HtmConfig.InitialSynapseConnsPct = this.InitialSynapseConnsPct;
                //m_HtmConfig.SynPermTrimThreshold = this.getSynPermTrimThreshold();
                //m_HtmConfig.SynPermBelowStimulusInc = this.synPermBelowStimulusInc;
                //m_HtmConfig.SynPermMax = this.getSynPermMax();
                //m_HtmConfig.SynPermMin = this.getSynPermMin();
                //m_HtmConfig.StimulusThreshold = this.StimulusThreshold;
                //m_HtmConfig.CellsPerColumn = this.getCellsPerColumn();
                //m_HtmConfig.SynPermInactiveDec = this.getSynPermInactiveDec();
                //m_HtmConfig.PermanenceIncrement = this.getPermanenceIncrement();
                //m_HtmConfig.PermanenceDecrement = this.getPermanenceDecrement();
                //m_HtmConfig.RandomGenSeed = this.seed;       

                return m_HtmConfig;
            }
            private set
            {
                m_HtmConfig = value;
            }
        }



        ///////////////////////   Synapses and segments /////////////////////////

        /// <summary>
        /// Reverse mapping from source cell to <see cref="Synapse"/>
        /// </summary>
        //private Dictionary<Cell, LinkedHashSet<Synapse>> m_ReceptorSynapses = new Dictionary<Cell, LinkedHashSet<Synapse>>();

        /// <summary>
        /// Distal segments of cells.
        /// </summary>
        //protected Dictionary<Cell, List<DistalDendrite>> m_DistalSegments = new Dictionary<Cell, List<DistalDendrite>>();

        /// DD We moved this as a part of the segment.
        /// <summary>
        /// Synapses, which belong to some distal dentrite segment.
        /// </summary>
        private Dictionary<Segment, List<Synapse>> m_DistalSynapses;

        // Proximal synapses are a part of the column.
        //protected Dictionary<Segment, List<Synapse>> proximalSynapses;

        /** Helps index each new proximal Synapse */
        //protected int proximalSynapseCounter = -1;

        /// <summary>
        /// Global tracker of the next available segment index
        /// </summary>
        protected int m_NextFlatIdx;

        /// <summary>
        /// Global counter incremented for each DD segment creation
        /// </summary>
        protected int m_NextSegmentOrdinal;

        /// <summary>
        /// Global counter incremented for each DD synapse creation
        /// </summary>
        protected int m_NextSynapseOrdinal;

        /// <summary>
        /// Total number of synapses
        /// </summary>
        protected long m_NumSynapses;

        /// <summary>
        /// Used for destroying of indexes.
        /// </summary>
        protected List<int> m_FreeFlatIdxs = new List<int>();

        /// <summary>
        /// Indexed segments by their global index (can contain nulls).
        /// Indexed list of distal segments.
        /// </summary>
        //protected List<DistalDendrite> m_SegmentForFlatIdx = new List<DistalDendrite>();

        protected ConcurrentDictionary<int, DistalDendrite> m_SegmentForFlatIdx = new ConcurrentDictionary<int, DistalDendrite>();

        /// <summary>
        /// Stores each cycle's most recent activity
        /// </summary>
        public SegmentActivity LastActivity { get; set; }

        /// <summary>
        /// The segment creation number.
        /// </summary>
        public int NextSegmentOrdinal
        {
            get
            {
                lock ("segmentindex")
                {
                    return m_NextSegmentOrdinal;
                }
            }
        }

        #region Constructors and Initialization

        /// <summary>
        /// Constructs a new <see cref="Connections"/> object. This object
        /// is usually configured via the <see cref="Parameters.apply(object)"/>
        /// method. <b>(subjected to changes)</b>
        /// </summary>
        public Connections()
        {

            // TODO: Remove this when old way of parameter initialization is completely removed.
            this.m_HtmConfig = new HtmConfig(new int[100], new int[] { 2048 });
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="prms"></param>
        public Connections(HtmConfig prms)
        {
            this.HtmConfig = prms;
        }

        #endregion

        #region General Methods

        /// <summary>
        /// Returns the <see cref="Cell"/> specified by the index passed in.
        /// </summary>
        /// <param name="index">index of the specified cell to return.</param>
        /// <returns></returns>
        public Cell GetCell(int index)
        {
            return Cells[index];
        }

        /// <summary>
        /// Returns an array containing the <see cref="Cell"/>s specified by the passed in indexes.
        /// </summary>
        /// <param name="cellIndexes">indexes of the Cells to return</param>
        /// <returns></returns>
        public Cell[] GetCells(int[] cellIndexes)
        {
            Cell[] retVal = new Cell[cellIndexes.Length];
            for (int i = 0; i < cellIndexes.Length; i++)
            {
                retVal[i] = Cells[cellIndexes[i]];
            }
            return retVal;
        }

        /// <summary>
        /// Returns a <see cref="LinkedHashSet{T}"/> containing the <see cref="Cell"/>s specified by the passed in indexes.
        /// </summary>
        /// <param name="cellIndexes">indexes of the Cells to return</param>
        /// <returns></returns>
        public List<Cell> GetCellSet(int[] cellIndexes)
        {
            List<Cell> retVal = new List<Cell>();
            for (int i = 0; i < cellIndexes.Length; i++)
            {
                retVal.Add(Cells[cellIndexes[i]]);
            }
            return retVal;
        }

        ///**
        // * Sets the matrix containing the {@link Column}s
        // * @param mem
        // */
        //public void setMemory(AbstractSparseMatrix<Column> mem)
        //{
        //    this.memory = mem;
        //}

        ///**
        // * Returns the matrix containing the {@link Column}s
        // * @return
        // */
        //public AbstractSparseMatrix<Column> getMemory()
        //{
        //    return memory;
        //}

        ///**
        // * Returns the {@link Topology} overseeing input 
        // * neighborhoods.
        // * @return 
        // */
        //public Topology getInputTopology()
        //{
        //    return inputTopology;
        //}

        ///**
        // * Sets the {@link Topology} overseeing input 
        // * neighborhoods.
        // * 
        // * @param topology  the input Topology
        // */
        //public void setInputTopology(Topology topology)
        //{
        //    this.inputTopology = topology;
        //}

        ///**
        // * Returns the {@link Topology} overseeing {@link Column} 
        // * neighborhoods.
        // * @return
        // */
        //public Topology getColumnTopology()
        //{
        //    return columnTopology;
        //}

        ///**
        // * Sets the {@link Topology} overseeing {@link Column} 
        // * neighborhoods.
        // * 
        // * @param topology  the column Topology
        // */
        //public void setColumnTopology(Topology topology)
        //{
        //    this.columnTopology = topology;
        //}

        ///**
        // * Returns the input column mapping
        // */
        //public ISparseMatrix<int> getInputMatrix()
        //{
        //    return inputMatrix;
        //}

        ///**
        // * Sets the input column mapping matrix
        // * @param matrix
        // */
        //public void setInputMatrix(ISparseMatrix<int> matrix)
        //{
        //    this.inputMatrix = matrix;
        //}

        ////////////////////////////////////////
        //       SpatialPooler Methods        //
        ////////////////////////////////////////


        ///// <summary>
        ///// Percent of initially connected synapses. Typically 50%.
        ///// </summary>
        //public double InitialSynapseConnsPct
        //{
        //    get
        //    {
        //        return this.initConnectedPct;
        //    }
        //    set
        //    {
        //        this.initConnectedPct = value;
        //    }
        //}

        ///**
        // * Returns the cycle count.
        // * @return
        // */
        //public int getIterationNum()
        //{
        //    return SpIterationNum;
        //}

        ///**
        // * Sets the iteration count.
        // * @param num
        // */
        //public void setIterationNum(int num)
        //{
        //    this.SpIterationNum = num;
        //}

        ///**
        // * Returns the period count which is the number of cycles
        // * between meta information updates.
        // * @return
        // */
        //public int getUpdatePeriod()
        //{
        //    return updatePeriod;
        //}

        ///**
        // * Sets the update period
        // * @param period
        // */
        //public void setUpdatePeriod(int period)
        //{
        //    this.updatePeriod = period;
        //}



        ///// <summary>
        ///// Radius of inhibition area. Called when the density of inhibition area is calculated.
        ///// </summary>
        //public int InhibitionRadius
        //{
        //    get
        //    {
        //        return m_InhibitionRadius;
        //    }
        //    set
        //    {
        //        this.m_InhibitionRadius = value;
        //    }
        //}


        ///// <summary>
        ///// Gets/Sets the number of input neurons in 1D space. Mathematically, 
        ///// this is the product of the input dimensions.
        ///// </summary>
        //public int NumInputs
        //{
        //    get => numInputs;
        //    set => this.numInputs = value;
        //}


        ///// <summary>
        ///// Returns the total numbe rof columns across all dimensions.
        ///// </summary>
        //public int NumColumns
        //{
        //    get
        //    {
        //        return this.numColumns;
        //    }
        //}

        ///**
        // * Sets the product of the column dimensions to be
        // * the column count.
        // * @param n
        // */
        //public void setNumColumns(int n)
        //{
        //    this.numColumns = n;
        //}

        ///**
        // * This parameter determines the extent of the input
        // * that each column can potentially be connected to.
        // * This can be thought of as the input bits that
        // * are visible to each column, or a 'receptiveField' of
        // * the field of vision. A large enough value will result
        // * in 'global coverage', meaning that each column
        // * can potentially be connected to every input bit. This
        // * parameter defines a square (or hyper square) area: a
        // * column will have a max square potential pool with
        // * sides of length 2 * potentialRadius + 1.
        // * 
        // * <b>WARNING:</b> potentialRadius **must** be set to 
        // * the inputWidth if using "globalInhibition" and if not 
        // * using the Network API (which sets this automatically) 
        // *
        // *
        // * @param potentialRadius
        // */
        //public void setPotentialRadius(int potentialRadius)
        //{
        //    this.potentialRadius = potentialRadius;
        //}

        ///**
        // * Returns the configured potential radius
        // * 
        // * @return  the configured potential radius
        // * @see setPotentialRadius
        // */
        //public int getPotentialRadius()
        //{
        //    return potentialRadius;
        //}

        ///**
        // * The percent of the inputs, within a column's
        // * potential radius, that a column can be connected to.
        // * If set to 1, the column will be connected to every
        // * input within its potential radius. This parameter is
        // * used to give each column a unique potential pool when
        // * a large potentialRadius causes overlap between the
        // * columns. At initialization time we choose
        // * ((2*potentialRadius + 1)^(# inputDimensions) *
        // * potentialPct) input bits to comprise the column's
        // * potential pool.
        // *
        // * @param potentialPct
        // */
        //public void setPotentialPct(double potentialPct)
        //{
        //    this.potentialPct = potentialPct;
        //}

        ///**
        // * Returns the configured potential pct
        // *
        // * @return the configured potential pct
        // * @see setPotentialPct
        // */
        //public double getPotentialPct()
        //{
        //    return potentialPct;
        //}

        /// <summary>
        /// Sets the <see cref="AbstractSparseMatrix{T}"/> which represents the proximal dendrite permanence values.
        /// </summary>
        /// <param name="s">the <see cref="AbstractSparseMatrix{T}"/></param>
        public void SetProximalPermanences(AbstractSparseMatrix<double[]> s)
        {
            foreach (int idx in s.GetSparseIndices())
            {
                this.HtmConfig.Memory.getObject(idx).SetPermanences(this.HtmConfig, s.getObject(idx));
            }
        }

        /**
         * Returns the count of {@link Synapse}s on
         * {@link ProximalDendrite}s
         * @return
         */
        //public int getProximalSynapseCount()
        //{
        //    return proximalSynapseCounter + 1;
        //}

        /**
         * Sets the count of {@link Synapse}s on
         * {@link ProximalDendrite}s
         * @param i
         */
        //public void setProximalSynapseCount(int i)
        //{
        //    this.proximalSynapseCounter = i;
        //}

        /**
         * Increments and returns the incremented
         * proximal {@link Synapse} count.
         *
         * @return
         */
        //public int incrementProximalSynapses()
        //{
        //    return ++proximalSynapseCounter;
        //}

        /**
         * Decrements and returns the decremented
         * proximal {link Synapse} count
         * @return
         */
        //public int decrementProximalSynapses()
        //{
        //    return --proximalSynapseCounter;
        //}

        /**
         * Returns the indexed count of connected synapses per column.
         * @return
         */
        //public AbstractSparseBinaryMatrix getConnectedCounts()
        //{
        //    return connectedCounts;
        //}

        public int[] GetTrueCounts()
        {
            int[] counts = new int[this.HtmConfig.NumColumns];
            for (int i = 0; i < this.HtmConfig.NumColumns; i++)
            {
                counts[i] = GetColumn(i).ConnectedInputCounterMatrix.GetTrueCounts()[0];
            }

            return counts;
        }

        /**
         * Returns the connected count for the specified column.
         * @param columnIndex
         * @return
         */
        //public int getConnectedCount(int columnIndex)
        //{
        //    return connectedCounts.getTrueCount(columnIndex);
        //}

        /// <summary>
        /// Sets the indexed count of synapses connected at the columns in each index.
        /// </summary>
        /// <param name="counts"></param>
        public void SetConnectedCounts(int[] counts)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                GetColumn(i).ConnectedInputCounterMatrix.SetTrueCount(0, counts[i]);
                //connectedCounts.setTrueCount(i, counts[i]);
            }
        }

        /// <summary>
        /// Sets the connected count <see cref="AbstractSparseBinaryMatrix"/>, which defines how synapses are connected to input.
        /// </summary>
        /// <param name="matrix"></param>
        public void SetConnectedMatrix(AbstractSparseBinaryMatrix matrix)
        {
            for (int col = 0; col < this.HtmConfig.NumColumns; col++)
            {
                var colMatrix = this.GetColumn(col).ConnectedInputCounterMatrix = new SparseBinaryMatrix(new int[] { 1, this.HtmConfig.NumInputs });

                int[] row = (int[])matrix.GetSlice(col);

                for (int j = 0; j < row.Length; j++)
                {
                    colMatrix.set(row[j], 0, j);
                }
            }

            // this.connectedCounts = matrix;
        }


        ///**
        // * Sets the array holding the random noise added to proximal dendrite overlaps.
        // *
        // * @param tieBreaker	random values to help break ties
        // */
        //public void setTieBreaker(double[] tieBreaker)
        //{
        //    this.tieBreaker = tieBreaker;
        //}

        ///**
        // * Returns the array holding random values used to add to overlap scores
        // * to break ties.
        // *
        // * @return
        // */
        //public double[] getTieBreaker()
        //{
        //    return tieBreaker;
        //}

        /// <summary>
        /// Array holding the random noise added to proximal dendrite overlaps.
        /// </summary>
        public double[] TieBreaker { get => m_TieBreaker; set => m_TieBreaker = value; }


        public double[] BoostedOverlaps { get => m_BoostedmOverlaps; set => this.m_BoostedmOverlaps = value; }


        /// <summary>
        /// Set/Get ovrlaps for each column.
        /// </summary>
        public int[] Overlaps { get => m_Overlaps; set => this.m_Overlaps = value; }


        ///**
        // * Returns the version number
        // * @return
        // */
        //public double getVersion()
        //{
        //    return version;
        //}

        ///**
        // * Returns the overlap duty cycles.
        // * @return
        // */
        //public double[] getOverlapDutyCycles()
        //{
        //    return overlapDutyCycles;
        //}

        ///**
        // * Sets the overlap duty cycles
        // * @param overlapDutyCycles
        // */
        //public void setOverlapDutyCycles(double[] overlapDutyCycles)
        //{
        //    this.overlapDutyCycles = overlapDutyCycles;
        //}

        ///**
        // * Returns the dense (size=numColumns) array of duty cycle stats.
        // * @return	the dense array of active duty cycle values.
        // */
        //public double[] getActiveDutyCycles()
        //{
        //    return activeDutyCycles;
        //}

        ///**
        // * Sets the dense (size=numColumns) array of duty cycle stats.
        // * @param activeDutyCycles
        // */
        //public void setActiveDutyCycles(double[] activeDutyCycles)
        //{
        //    this.activeDutyCycles = activeDutyCycles;
        //}

        /// <summary>
        /// Applies the dense array values which aren't -1 to the array containing the active duty cycles of the column corresponding to the index specified.
        /// The length of the specified array must be as long as the configured number of columns of this <see cref="Connections"/>' column configuration.
        /// </summary>
        /// <param name="denseActiveDutyCycles">a dense array containing values to set.</param>
        public void UpdateActiveDutyCycles(double[] denseActiveDutyCycles)
        {
            for (int i = 0; i < denseActiveDutyCycles.Length; i++)
            {
                if (denseActiveDutyCycles[i] != -1)
                {
                    this.HtmConfig.ActiveDutyCycles[i] = denseActiveDutyCycles[i];
                }
            }
        }

        ///**
        // * Returns the minOverlapDutyCycles.
        // * @return	the minOverlapDutyCycles.
        // */
        //public double[] getMinOverlapDutyCycles()
        //{
        //    return minOverlapDutyCycles;
        //}

        ///**
        // * Sets the minOverlapDutyCycles
        // * @param minOverlapDutyCycles	the minOverlapDutyCycles
        // */
        //public void setMinOverlapDutyCycles(double[] minOverlapDutyCycles)
        //{
        //    this.minOverlapDutyCycles = minOverlapDutyCycles;
        //}

        ///**
        // * Returns the minActiveDutyCycles
        // * @return	the minActiveDutyCycles
        // */
        //public double[] getMinActiveDutyCycles()
        //{
        //    return minActiveDutyCycles;
        //}

        ///**
        // * Sets the minActiveDutyCycles
        // * @param minActiveDutyCycles	the minActiveDutyCycles
        // */
        //public void setMinActiveDutyCycles(double[] minActiveDutyCycles)
        //{
        //    this.minActiveDutyCycles = minActiveDutyCycles;
        //}

        /**
         * Returns the array of boost factors
         * @return	the array of boost factors
         */
        /**
 * Sets the array of boost factors
 * @param boostFactors	the array of boost factors
 */
        public double[] BoostFactors { get => m_BoostFactors; set => this.m_BoostFactors = value; }

        /// <summary>
        /// Controls if bumping-up of weak columns shell be done.
        /// </summary>
        //public bool IsBumpUpWeakColumnsDisabled { get => isBumpUpWeakColumnsDisabled; set => isBumpUpWeakColumnsDisabled = value; }


        ////////////////////////////////////////
        //       TemporalMemory Methods       //
        ////////////////////////////////////////

        #region TemporalMemory Methods

        /// <summary>
        /// Computes the number of active and potential synapses of the each segment for a given input.
        /// </summary>
        /// <param name="activeCellsInCurrentCycle"></param>
        /// <param name="connectedPermanence"></param>
        /// <returns></returns>
        public SegmentActivity ComputeActivity(ICollection<Cell> activeCellsInCurrentCycle, double connectedPermanence)
        {
            Dictionary<int, int> numOfActiveSynapses = new Dictionary<int, int>();
            Dictionary<int, int> numOfPotentialSynapses = new Dictionary<int, int>();

            double threshold = connectedPermanence - EPSILON;

            //
            // Step through all currently active cells.
            // Find synapses that points to this cell. (receptor synapses)
            foreach (Cell cell in activeCellsInCurrentCycle)
            {
                //
                // This cell is the active in the current cycle. 
                // We step through all receptor synapses and check the permanence value of related synapses.
                // Receptor synapses are synapses whose source cell (pre-synaptic cell) is the given cell.
                // Receptor synapses connect their axons to distal dendrite segments of other cells.
                // The permanence value of this connection indicates the the cell owner of connected distal dendrite is expected
                // to be activated in the next cycle.
                // The segment owner cell in other column pointed by synapse sourced by this 'cell' is depolirized (in predicting state).
                foreach (Synapse synapse in cell.ReceptorSynapses)
                {
                    // Now, we get the segment of the synapse of the pre-synaptic cell.
                    int segFlatIndx = synapse.SegmentIndex;
                    if (numOfPotentialSynapses.ContainsKey(segFlatIndx) == false)
                        numOfPotentialSynapses.Add(segFlatIndx, 0);

                    numOfPotentialSynapses[segFlatIndx] = numOfPotentialSynapses[segFlatIndx] + 1;

                    if (synapse.Permanence > threshold)
                    {
                        if (numOfActiveSynapses.ContainsKey(segFlatIndx) == false)
                            numOfActiveSynapses.Add(segFlatIndx, 0);

                        numOfActiveSynapses[segFlatIndx] = numOfActiveSynapses[segFlatIndx] + 1;
                    }
                }
            }

            return new SegmentActivity() { ActiveSynapses = numOfActiveSynapses, PotentialSynapses = numOfPotentialSynapses };
        }


        /// <summary>
        /// Record the fact that a segment had some activity. This information is used during segment cleanup.
        /// </summary>
        /// <param name="segment">the segment for which to record activity</param>
        public void RecordSegmentActivity(DistalDendrite segment)
        {
            segment.LastUsedIteration = m_TMIteration;
        }

        /// <summary>
        /// Mark the passage of time. This information is used during segment
        /// cleanup.
        /// </summary>
        public void StartNewIteration()
        {
            ++m_TMIteration;
        }

        #endregion

        /////////////////////////////////////////////////////////////////
        //     Segment (Specifically, Distal Dendrite) Operations      //
        /////////////////////////////////////////////////////////////////

        #region Segment (Specifically, Distal Dendrite) methods
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
            while (NumSegments(segmentParentCell) >= this.HtmConfig.MaxSegmentsPerCell)
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
                        RemoveSynapseFromPresynapticMap(s);
                    }

                    m_NumSynapses -= len;
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
        /// Used internally to return the least recently activated segment on the specified cell
        /// </summary>
        /// <param name="cell">cell to search for segments on.</param>
        /// <returns>the least recently activated segment on the specified cell.</returns>
        private DistalDendrite LeastRecentlyUsedSegment(Cell cell)
        {
            //DD
            //List<DistalDendrite> segments = GetSegments(cell, false);
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

        ///**
        // * Returns the total number of {@link DistalDendrite}s
        // * 
        // * @return  the total number of segments
        // */
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public int NumSegments()
        //{
        //    return NumSegments(null);
        //}

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
                return m_NextFlatIdx - m_FreeFlatIdxs.Count;
            }
        }

        ///// <summary>
        ///// Returns the mapping of <see cref="Cell"/>s to their <see cref="DistalDendrite"/>s.
        ///// </summary>
        ///// <param name="cell">the {@link Cell} used as a key.</param>
        ///// <returns>the mapping of {@link Cell}s to their {@link DistalDendrite}s.</returns>
        //public List<DistalDendrite> GetSegments(Cell cell)
        //{
        //    return GetSegments(cell, false);
        //}

        //DD
        /// <summary>
        /// Returns the mapping of <see cref="Cell"/>s to their <see cref="DistalDendrite"/>s.
        /// </summary>
        /// <param name="cell">the <see cref="Cell"/> used as a key.</param>
        /// <param name="doLazyCreate">create a container for future use if true, if false return an orphaned empty set.</param>
        /// <returns>the mapping of <see cref="Cell"/>s to their <see cref="DistalDendrite"/>s.</returns>
        //public List<DistalDendrite> GetSegments(Cell cell, bool doLazyCreate = false)
        //{
        //    if (cell == null)
        //    {
        //        throw new ArgumentException("Cell was null");
        //    }

        //    //if (m_DistalSegments == null)
        //    //{
        //    //    m_DistalSegments = new Dictionary<Cell, List<DistalDendrite>>();
        //    //}

        //    List<DistalDendrite> retVal;
        //    if ((m_DistalSegments.TryGetValue(cell, out retVal)) == false)
        //    {
        //        if (!doLazyCreate) return new List<DistalDendrite>();
        //        m_DistalSegments.Add(cell, retVal = new List<DistalDendrite>());
        //    }

        //    return retVal;
        //}

        /// <summary>
        /// Get the segment with the specified flatIdx.
        /// </summary>
        /// <param name="index">The segment's flattened list index.</param>
        /// <returns>the <see cref="DistalDendrite"/> who's index matches.</returns>
        public DistalDendrite GetSegmentForFlatIdx(int index)
        {
            return m_SegmentForFlatIdx[index];
        }

        /// <summary>
        /// Returns the index of the <see cref="Column"/> owning the cell which owns 
        /// the specified segment.
        /// </summary>
        /// <param name="segment">the <see cref="DistalDendrite"/> of the cell whose column index is desired.</param>
        /// <returns>the owning column's index</returns>
        public int ColumnIndexForSegment(DistalDendrite segment)
        {
            return segment.ParentCell.Index / this.HtmConfig.CellsPerColumn;
        }

        /// <summary>
        /// <b>FOR TEST USE ONLY</b>
        /// </summary>
        /// <returns></returns>
        //public Dictionary<Cell, List<DistalDendrite>> GetSegmentMapping()
        //{
        //    return new Dictionary<Cell, List<DistalDendrite>>(m_DistalSegments);
        //}

        /// <summary>
        /// Set/retrieved by the <see cref="TemporalMemory"/> following a compute cycle.
        /// </summary>
        public List<DistalDendrite> ActiveSegments { get => m_ActiveSegments; set => m_ActiveSegments = value; }

        /// <summary>
        /// Set/retrieved by the <see cref="TemporalMemory"/> prior to a compute cycle.
        /// </summary>
        public List<DistalDendrite> MatchingSegments { get => m_MatchingSegments; set => m_MatchingSegments = value; }
        #endregion
        #region Synapse Operations
        /////////////////////////////////////////////////////////////////
        //                    Synapse Operations                       //
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new synapse on a segment.
        /// </summary>
        /// <param name="segment">the <see cref="DistalDendrite"/> segment to which a <see cref="Synapse"/> is being created.</param>
        /// <param name="presynapticCell">the source <see cref="Cell"/>.</param>
        /// <param name="permanence">the initial permanence.</param>
        /// <returns>the created <see cref="Synapse"/>.</returns>
        public Synapse CreateSynapse(DistalDendrite segment, Cell presynapticCell, double permanence)
        {
            while (segment.Synapses.Count >= this.HtmConfig.MaxSynapsesPerSegment)
            {
                DestroySynapse(MinPermanenceSynapse(segment), segment);
            }

            lock ("synapses")
            {
                Synapse synapse = null;
                //DD GetSynapses(segment).Add(
                segment.Synapses.Add(
                synapse = new Synapse(
                    presynapticCell, segment.SegmentIndex, m_NextSynapseOrdinal, permanence));

                presynapticCell.ReceptorSynapses.Add(synapse);
                //DD GetReceptorSynapses(presynapticCell, true).Add(synapse);

                ++m_NextSynapseOrdinal;

                ++m_NumSynapses;

                return synapse;
            }
        }

        /// <summary>
        /// Destroys the specified <see cref="Synapse"/> in specific <see cref="DistalDendrite"/> segment
        /// </summary>
        /// <param name="synapse">the Synapse to destroy</param>
        /// <param name="segment"></param>
        public void DestroySynapse(Synapse synapse, DistalDendrite segment)
        {
            lock ("synapses")
            {
                --m_NumSynapses;

                RemoveSynapseFromPresynapticMap(synapse);

                //segment.Synapses.Remove(synapse);
                //DD GetSynapses(segment).Remove(synapse);
                segment.Synapses.Remove(synapse);
            }
        }

        /// <summary>
        /// Removes the specified <see cref="Synapse"/> from its
        /// pre-synaptic <see cref="Cell"/>'s map of synapses it 
        /// activates.
        /// </summary>
        /// <param name="synapse">the synapse to remove</param>
        public void RemoveSynapseFromPresynapticMap(Synapse synapse)
        {
            Cell cell = synapse.getPresynapticCell();
            cell.ReceptorSynapses.Remove(synapse);
            //DD
            //LinkedHashSet<Synapse> presynapticSynapses;
            //Cell cell = synapse.getPresynapticCell();
            //(presynapticSynapses = GetReceptorSynapses(cell, false)).Remove(synapse);

            //if (presynapticSynapses.Count == 0)
            //{
            //    m_ReceptorSynapses.Remove(cell);
            //}
        }

        /// <summary>
        /// Used internally to find the synapse with the smallest permanence
        /// on the given segment.
        /// </summary>
        /// <param name="dd">Segment object to search for synapses on</param>
        /// <returns>Synapse object on the segment with the minimal permanence</returns>
        private Synapse MinPermanenceSynapse(DistalDendrite dd)
        {
            //List<Synapse> synapses = getSynapses(dd).stream().sorted().collect(Collectors.toList());
            //DD List<Synapse> synapses = GetSynapses(dd);
            List<Synapse> synapses = dd.Synapses;
            Synapse min = null;
            double minPermanence = Double.MaxValue;

            foreach (Synapse synapse in synapses)
            {
                if (!synapse.IsDestroyed && synapse.Permanence < minPermanence - EPSILON)
                {
                    min = synapse;
                    minPermanence = synapse.Permanence;
                }
            }

            return min;
        }


        ///// <summary>
        ///// Returns the number of <see cref="Synapse"/>s on a given <see cref="DistalDendrite"/>
        ///// if specified, or the total number if the "optionalSegmentArg" is null.
        ///// </summary>
        ///// <param name="optionalSegmentArg">An optional Segment to specify the context of the synapse count.</param>
        ///// <returns>Either the total number of synapses or the number on a specified segment.</returns>
        //public long GetNumSynapses(DistalDendrite optionalSegmentArg)
        //{
        //    // DD return GetSynapses(optionalSegmentArg).Count;
        //    return optionalSegmentArg.Synapses.Count;
        //}


        /// <summary>
        /// Returns synapses which hold the specified cell as their source cell.
        /// Returns the mapping of <see cref="Cell"/>s to their reverse mapped
        /// <see cref="Synapse"/>s.
        /// </summary>
        /// <param name="cell">the <see cref="Cell"/> used as a key.</param>
        /// <param name="doLazyCreate">create a container for future use if true, if false return an orphaned empty set.</param>
        /// <returns>the mapping of <see cref="Cell"/>s to their reverse mapped</returns>
        //public LinkedHashSet<Synapse> GetReceptorSynapses(Cell cell, bool doLazyCreate = false)
        //{
        //    if (cell == null)
        //    {
        //        throw new ArgumentException("Cell was null");
        //    }

        //    //if (m_ReceptorSynapses == null)
        //    //{
        //    //    m_ReceptorSynapses = new Dictionary<Cell, LinkedHashSet<Synapse>>();
        //    //}

        //    LinkedHashSet<Synapse> retVal = null;
        //    if (m_ReceptorSynapses.TryGetValue(cell, out retVal) == false)
        //    {
        //        if (!doLazyCreate) return new LinkedHashSet<Synapse>();
        //        m_ReceptorSynapses.Add(cell, retVal = new LinkedHashSet<Synapse>());
        //    }

        //    return retVal;
        //}

        /// <summary>
        /// Returns synapeses of specified dentrite segment.
        /// </summary>
        /// <param name="segment">Distal Dentrite segment.</param>
        /// <returns>List of segment synapeses.</returns>
        //public List<Synapse> GetSynapses(DistalDendrite segment)
        //{
        //    if (segment == null)
        //    {
        //        throw new ArgumentException("Segment cannot be null");
        //    }

        //    if (m_DistalSynapses == null)
        //    {
        //        m_DistalSynapses = new Dictionary<Segment, List<Synapse>>();
        //    }

        //    List<Synapse> retVal = null;
        //    if (m_DistalSynapses.TryGetValue(segment, out retVal) == false)
        //    {
        //        m_DistalSynapses.Add(segment, retVal = new List<Synapse>());
        //    }

        //    return retVal;
        //}


        //DD 
        /// <summary>
        /// For testing only.
        /// </summary>
        /// <returns>Copy of dictionary.</returns>
        //public Dictionary<Cell, LinkedHashSet<Synapse>> GetReceptorSynapses()
        //{
        //    return new Dictionary<Cell, LinkedHashSet<Synapse>>(m_ReceptorSynapses);
        //}

        /// <summary>
        /// Clears the sequence learning state.
        /// </summary>
        public void Clear()
        {
            m_ActiveCells.Clear();
            winnerCells.Clear();
            m_PredictiveCells.Clear();
        }


        /// <summary>
        /// Generates the list of predictive cells from parent cells of active segments.
        /// </summary>
        /// <returns></returns>
        public ISet<Cell> GetPredictiveCells()
        {
            if (m_PredictiveCells.Count == 0)
            {
                Cell previousCell = null;
                Cell currCell = null;

                List<DistalDendrite> temp = new List<DistalDendrite>(m_ActiveSegments);
                foreach (DistalDendrite activeSegment in temp)
                {
                    if ((currCell = activeSegment.ParentCell) != previousCell)
                    {
                        m_PredictiveCells.Add(previousCell = currCell);
                    }
                }
            }
            return m_PredictiveCells;
        }

        /// <summary>
        /// Clears the previous predictive cells from the list.
        /// </summary>
        public void ClearPredictiveCells()
        {
            this.m_PredictiveCells.Clear();
        }

        /// <summary>
        /// Returns the column at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Column GetColumn(int index)
        {
            return this.HtmConfig.Memory.getObject(index);
        }

        /// <summary>
        /// Converts a <see cref="Collection{T}"/> of <see cref="Cell"/>s to a list of cell indexes.
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static List<Integer> AsCellIndexes(Collection<Cell> cells)
        {
            List<Integer> ints = new List<Integer>();
            foreach (Cell cell in cells)
            {
                ints.Add(cell.Index);
            }

            return ints;
        }

        /// <summary>
        /// Converts a <see cref="Collection{T}"/> of <see cref="Column"/>s to a list of column indexes.
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static List<Integer> AsColumnIndexes(Collection<Column> columns)
        {
            List<Integer> ints = new List<Integer>();
            foreach (Column col in columns)
            {
                ints.Add(col.Index);
            }

            return ints;
        }

        /**
         * Returns a list of the {@link Cell}s specified.
         * @param cells		the indexes of the {@link Cell}s to return
         * @return	the specified list of cells
         */
        //public List<Cell> asCellObjects(Collection<Integer> cells)
        //{
        //    List<Cell> objs = new List<Cell>();
        //    foreach (int i in cells)
        //    {
        //        objs.Add(this.cells[i]);
        //    }
        //    return objs;
        //}

        /**
         * Returns a list of the {@link Column}s specified.
         * @param cols		the indexes of the {@link Column}s to return
         * @return		the specified list of columns
         */
        //public List<Column> asColumnObjects(Collection<Integer> cols)
        //{
        //    List<Column> objs = new List<Column>();
        //    foreach (int i in cols)
        //    {
        //        objs.Add(this.memory.getObject(i));
        //    }
        //    return objs;
        //}

        /// <summary>
        /// Returns a <see cref="LinkedHashSet{T}"/> view of the <see cref="Column"/>s specified by the indexes passed in.
        /// </summary>
        /// <param name="indexes">the indexes of the Columns to return</param>
        /// <returns>a set view of the specified columns</returns>
        public LinkedHashSet<Column> GetColumnSet(int[] indexes)
        {
            LinkedHashSet<Column> retVal = new LinkedHashSet<Column>();
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal.Add(this.HtmConfig.Memory.getObject(indexes[i]));
            }
            return retVal;
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> view of the <see cref="Column"/>s specified by the indexes passed in.
        /// </summary>
        /// <param name="indexes">the indexes of the Columns to return</param>
        /// <returns>a List view of the specified columns</returns>
        public List<Column> GetColumnList(int[] indexes)
        {
            List<Column> retVal = new List<Column>();
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal.Add(this.HtmConfig.Memory.getObject(indexes[i]));
            }
            return retVal;
        }
        #endregion
        /**
         * High 
         * e output useful for debugging
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("------------ SpatialPooler Parameters ------------------");
            sb.Append("numInputs                  = " + this.HtmConfig.NumInputs);
            sb.Append("numColumns                 = " + this.HtmConfig.NumColumns);
            sb.Append("cellsPerColumn             = " + this.HtmConfig.CellsPerColumn);
            sb.Append("columnDimensions           = " + this.HtmConfig.ColumnDimensions.ToString());
            sb.Append("numActiveColumnsPerInhArea = " + this.HtmConfig.NumActiveColumnsPerInhArea);
            sb.Append("potentialPct               = " + this.HtmConfig.PotentialPct);
            sb.Append("potentialRadius            = " + this.HtmConfig.PotentialRadius);
            sb.Append("globalInhibition           = " + this.HtmConfig.GlobalInhibition);
            sb.Append("localAreaDensity           = " + this.HtmConfig.LocalAreaDensity);
            sb.Append("inhibitionRadius           = " + this.HtmConfig.InhibitionRadius);
            sb.Append("stimulusThreshold          = " + this.HtmConfig.StimulusThreshold);
            sb.Append("synPermActiveInc           = " + this.HtmConfig.SynPermActiveInc);
            sb.Append("synPermInactiveDec         = " + this.HtmConfig.SynPermInactiveDec);
            sb.Append("synPermConnected           = " + this.HtmConfig.SynPermConnected);
            sb.Append("minPctOverlapDutyCycle     = " + this.HtmConfig.MinPctOverlapDutyCycles);
            sb.Append("minPctActiveDutyCycle      = " + this.HtmConfig.MinPctActiveDutyCycles);
            sb.Append("dutyCyclePeriod            = " + this.HtmConfig.DutyCyclePeriod);
            sb.Append("maxBoost                   = " + this.HtmConfig.MaxBoost);
            sb.Append("version                    = " + version);

            sb.Append("\n------------ TemporalMemory Parameters ------------------");
            sb.Append("activationThreshold        = " + this.HtmConfig.ActivationThreshold);
            sb.Append("learningRadius             = " + this.HtmConfig.LearningRadius);
            sb.Append("minThreshold               = " + this.HtmConfig.MinThreshold);
            sb.Append("maxNewSynapseCount         = " + this.HtmConfig.MaxNewSynapseCount);
            sb.Append("maxSynapsesPerSegment      = " + this.HtmConfig.MaxSynapsesPerSegment);
            sb.Append("maxSegmentsPerCell         = " + this.HtmConfig.MaxSegmentsPerCell);
            sb.Append("initialPermanence          = " + this.HtmConfig.InitialPermanence);
            sb.Append("connectedPermanence        = " + this.HtmConfig.ConnectedPermanence);
            sb.Append("permanenceIncrement        = " + this.HtmConfig.PermanenceIncrement);
            sb.Append("permanenceDecrement        = " + this.HtmConfig.PermanenceDecrement);
            sb.Append("predictedSegmentDecrement  = " + this.HtmConfig.PredictedSegmentDecrement);

            return sb.ToString();
        }

        /// <summary>
        /// Returns a 2 Dimensional array of 1's and 0's indicating which of the column's pool members are above the connected
        /// threshold, and therefore considered "connected"
        /// </summary>
        /// <returns></returns>
        public int[][] GetConnecteds()
        {
            int[][] retVal = new int[this.HtmConfig.NumColumns][];
            for (int i = 0; i < this.HtmConfig.NumColumns; i++)
            {
                //Pool pool = getPotentialPools().get(i);
                Pool pool = GetColumn(i).ProximalDendrite.RFPool;
                int[] indexes = pool.GetDenseConnected();
                retVal[i] = indexes;
            }

            return retVal;
        }

        /// <summary>
        /// Returns a 2 Dimensional array of 1's and 0's indicating which input bits belong to which column's pool.
        /// </summary>
        /// <returns></returns>
        public int[][] GetPotentials()
        {
            int[][] retVal = new int[this.HtmConfig.NumColumns][];
            for (int i = 0; i < this.HtmConfig.NumColumns; i++)
            {
                //Pool pool = getPotentialPools().get(i);
                Pool pool = GetColumn(i).ProximalDendrite.RFPool;
                int[] indexes = pool.GetDensePotential(this);
                retVal[i] = indexes;
            }

            return retVal;
        }

        /// <summary>
        /// Returns a 2 Dimensional array of the permanences for SP proximal dendrite column pooled connections.
        /// </summary>
        /// <returns></returns>
        public double[][] GetPermanences()
        {
            double[][] retVal = new double[this.HtmConfig.NumColumns][];
            for (int i = 0; i < this.HtmConfig.NumColumns; i++)
            {
                //Pool pool = getPotentialPools().get(i);
                Pool pool = GetColumn(i).ProximalDendrite.RFPool;
                double[] perm = pool.GetDensePermanences(this.HtmConfig.NumInputs);
                retVal[i] = perm;
            }

            return retVal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + this.HtmConfig.ActivationThreshold;
            result = prime * result + ((m_ActiveCells == null) ? 0 : m_ActiveCells.GetHashCode());
            result = prime * result + this.HtmConfig.ActiveDutyCycles.GetHashCode();
            result = prime * result + m_BoostFactors.GetHashCode();
            result = prime * result + Cells.GetHashCode();
            result = prime * result + this.HtmConfig.CellsPerColumn;
            result = prime * result + this.HtmConfig.ColumnDimensions.GetHashCode();
            //result = prime * result + ((connectedCounts == null) ? 0 : connectedCounts.GetHashCode());
            long temp;
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.ConnectedPermanence);
            result = prime * result + (int)(temp ^ (temp >> 32));//it was temp >>> 32
            result = prime * result + this.HtmConfig.DutyCyclePeriod;
            result = prime * result + (this.HtmConfig.GlobalInhibition ? 1231 : 1237);
            result = prime * result + this.HtmConfig.InhibitionRadius;
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.InitialSynapseConnsPct);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.InitialPermanence);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + this.HtmConfig.InputDimensions.GetHashCode();
            result = prime * result + ((this.HtmConfig.InputMatrix == null) ? 0 : this.HtmConfig.InputMatrix.GetHashCode());
            result = prime * result + SpIterationLearnNum;
            result = prime * result + SpIterationNum;
            //result = prime * result + (new Long(tmIteration)).intValue();
            result = prime * result + (int)m_TMIteration;
            result = prime * result + this.HtmConfig.LearningRadius;
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.LocalAreaDensity);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.MaxBoost);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + this.HtmConfig.MaxNewSynapseCount;
            result = prime * result + ((this.HtmConfig.Memory == null) ? 0 : this.HtmConfig.Memory.GetHashCode());
            result = prime * result + this.HtmConfig.MinActiveDutyCycles.GetHashCode();
            result = prime * result + this.HtmConfig.MinOverlapDutyCycles.GetHashCode();
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.MinPctActiveDutyCycles);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.MinPctOverlapDutyCycles);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + this.HtmConfig.MinThreshold;
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.NumActiveColumnsPerInhArea);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + this.HtmConfig.NumColumns;
            result = prime * result + this.HtmConfig.NumInputs;
            temp = m_NumSynapses;
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + this.HtmConfig.OverlapDutyCycles.GetHashCode();
            temp = this.HtmConfig.PermanenceDecrement.GetHashCode();
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.PermanenceIncrement);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.PotentialPct);
            result = prime * result + (int)(temp ^ (temp >> 32));
            //result = prime * result + ((potentialPools == null) ? 0 : potentialPools.GetHashCode());
            result = prime * result + this.HtmConfig.PotentialRadius;
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.PredictedSegmentDecrement);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + ((m_PredictiveCells == null) ? 0 : m_PredictiveCells.GetHashCode());
            result = prime * result + ((this.HtmConfig.Random == null) ? 0 : this.HtmConfig.Random.GetHashCode());
            //result = prime * result + ((m_ReceptorSynapses == null) ? 0 : m_ReceptorSynapses.GetHashCode());
            result = prime * result + this.HtmConfig.RandomGenSeed;
            //result = prime * result + ((m_DistalSegments == null) ? 0 : m_DistalSegments.GetHashCode());
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.StimulusThreshold);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermActiveInc);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermBelowStimulusInc);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermConnected);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermInactiveDec);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermMax);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermMin);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermTrimThreshold);
            result = prime * result + (int)(temp ^ (temp >> 32));
            //result = prime * result + proximalSynapseCounter;
            //result = prime * result + ((proximalSynapses == null) ? 0 : proximalSynapses.GetHashCode());
            //DD result = prime * result + ((m_DistalSynapses == null) ? 0 : m_DistalSynapses.GetHashCode());
            result = prime * result + m_TieBreaker.GetHashCode();
            result = prime * result + this.HtmConfig.UpdatePeriod;
            temp = BitConverter.DoubleToInt64Bits(version);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + ((winnerCells == null) ? 0 : winnerCells.GetHashCode());
            return result;
        }

        /*
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if ((obj.GetType() != this.GetType()))
                return false;

            Connections other = (Connections)obj;
            if (this.HtmConfig.ActivationThreshold != other.HtmConfig.ActivationThreshold)
                return false;
            if (m_ActiveCells == null)
            {
                if (other.m_ActiveCells != null)
                    return false;
            }
            else if (!m_ActiveCells.Equals(other.m_ActiveCells))
                return false;
            if (!Array.Equals(this.HtmConfig.ActiveDutyCycles, other.HtmConfig.ActiveDutyCycles))
                return false;
            if (!Array.Equals(m_BoostFactors, other.m_BoostFactors))
                return false;
            if (!Array.Equals(Cells, other.Cells))
                return false;
            if (this.HtmConfig.CellsPerColumn != other.HtmConfig.CellsPerColumn)
                return false;
            if (!Array.Equals(this.HtmConfig.ColumnDimensions, other.HtmConfig.ColumnDimensions))
                return false;
            //if (connectedCounts == null)
            //{
            //    if (other.connectedCounts != null)
            //        return false;
            //}
            //else if (!connectedCounts.Equals(other.connectedCounts))
            //    return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.ConnectedPermanence) != BitConverter.DoubleToInt64Bits(other.HtmConfig.ConnectedPermanence))
                return false;
            if (this.HtmConfig.DutyCyclePeriod != other.HtmConfig.DutyCyclePeriod)
                return false;
            if (this.HtmConfig.GlobalInhibition != other.HtmConfig.GlobalInhibition)
                return false;
            if (this.HtmConfig.InhibitionRadius != other.HtmConfig.InhibitionRadius)
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.InitialSynapseConnsPct) != BitConverter.DoubleToInt64Bits(other.HtmConfig.InitialSynapseConnsPct))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.InitialPermanence) != BitConverter.DoubleToInt64Bits(other.HtmConfig.InitialPermanence))
                return false;
            if (!Array.Equals(this.HtmConfig.InputDimensions, other.HtmConfig.InputDimensions))
                return false;
            if (this.HtmConfig.InputMatrix == null)
            {
                if (other.HtmConfig.InputMatrix != null)
                    return false;
            }
            else if (!this.HtmConfig.InputMatrix.Equals(other.HtmConfig.InputMatrix))
                return false;
            if (SpIterationLearnNum != other.SpIterationLearnNum)
                return false;
            if (SpIterationNum != other.SpIterationNum)
                return false;
            if (m_TMIteration != other.m_TMIteration)
                return false;
            if (this.HtmConfig.LearningRadius != other.HtmConfig.LearningRadius)
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.LocalAreaDensity) != BitConverter.DoubleToInt64Bits(other.HtmConfig.LocalAreaDensity))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.MaxBoost) != BitConverter.DoubleToInt64Bits(other.HtmConfig.MaxBoost))
                return false;
            if (this.HtmConfig.MaxNewSynapseCount != other.HtmConfig.MaxNewSynapseCount)
                return false;
            if (this.HtmConfig.Memory == null)
            {
                if (other.HtmConfig.Memory != null)
                    return false;
            }
            else if (!this.HtmConfig.Memory.Equals(other.HtmConfig.Memory))
                return false;
            if (!Array.Equals(this.HtmConfig.MinActiveDutyCycles, other.HtmConfig.MinActiveDutyCycles))
                return false;
            if (!Array.Equals(this.HtmConfig.MinOverlapDutyCycles, other.HtmConfig.MinOverlapDutyCycles))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.MinPctActiveDutyCycles) != BitConverter.DoubleToInt64Bits(other.HtmConfig.MinPctActiveDutyCycles))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.MinPctOverlapDutyCycles) != BitConverter.DoubleToInt64Bits(other.HtmConfig.MinPctOverlapDutyCycles))
                return false;
            if (this.HtmConfig.MinThreshold != other.HtmConfig.MinThreshold)
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.NumActiveColumnsPerInhArea) != BitConverter.DoubleToInt64Bits(other.HtmConfig.NumActiveColumnsPerInhArea))
                return false;
            if (this.HtmConfig.NumColumns != other.HtmConfig.NumColumns)
                return false;
            if (this.HtmConfig.NumInputs != other.HtmConfig.NumInputs)
                return false;
            if (m_NumSynapses != other.m_NumSynapses)
                return false;
            if (!Array.Equals(this.HtmConfig.OverlapDutyCycles, other.HtmConfig.OverlapDutyCycles))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.PermanenceDecrement) != BitConverter.DoubleToInt64Bits(other.HtmConfig.PermanenceDecrement))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.PermanenceIncrement) != BitConverter.DoubleToInt64Bits(other.HtmConfig.PermanenceIncrement))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.PotentialPct) != BitConverter.DoubleToInt64Bits(other.HtmConfig.PotentialPct))
                return false;
            //if (potentialPools == null)
            //{
            //    if (other.potentialPools != null)
            //        return false;
            //}
            //else if (!potentialPools.Equals(other.potentialPools))
            //    return false;
            if (this.HtmConfig.PotentialRadius != other.HtmConfig.PotentialRadius)
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.PredictedSegmentDecrement) != BitConverter.DoubleToInt64Bits(other.HtmConfig.PredictedSegmentDecrement))
                return false;
            if (m_PredictiveCells == null)
            {
                if (other.m_PredictiveCells != null)
                    return false;
            }
            else if (!GetPredictiveCells().Equals(other.GetPredictiveCells()))
                return false;
            if (m_ReceptorSynapses == null)
            {
                if (other.m_ReceptorSynapses != null)
                    return false;
            }
            else if (!m_ReceptorSynapses.ToString().Equals(other.m_ReceptorSynapses.ToString()))
                return false;
            if (this.HtmConfig.RandomGenSeed != other.HtmConfig.RandomGenSeed)
                return false;
            if (m_DistalSegments == null)
            {
                if (other.m_DistalSegments != null)
                    return false;
            }
            else if (!m_DistalSegments.Equals(other.m_DistalSegments))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.StimulusThreshold) != BitConverter.DoubleToInt64Bits(other.HtmConfig.StimulusThreshold))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermActiveInc) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermActiveInc))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermBelowStimulusInc) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermBelowStimulusInc))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermConnected) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermConnected))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermInactiveDec) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermInactiveDec))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermMax) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermMax))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermMin) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermMin))
                return false;
            if (BitConverter.DoubleToInt64Bits(this.HtmConfig.SynPermTrimThreshold) != BitConverter.DoubleToInt64Bits(other.HtmConfig.SynPermTrimThreshold))
                return false;
            //if (proximalSynapseCounter != other.proximalSynapseCounter)
            //    return false;
            //if (proximalSynapses == null)
            //{
            //    if (other.proximalSynapses != null)
            //        return false;
            //}
            //else if (!proximalSynapses.Equals(other.proximalSynapses))
            //    return false;
            if (m_DistalSynapses == null)
            {
                if (other.m_DistalSynapses != null)
                    return false;
            }
            else if (!m_DistalSynapses.Equals(other.m_DistalSynapses))
                return false;
            if (!Array.Equals(m_TieBreaker, other.m_TieBreaker))
                return false;
            if (this.HtmConfig.UpdatePeriod != other.HtmConfig.UpdatePeriod)
                return false;
            if (BitConverter.DoubleToInt64Bits(version) != BitConverter.DoubleToInt64Bits(other.version))
                return false;
            if (winnerCells == null)
            {
                if (other.winnerCells != null)
                    return false;
            }
            else if (!winnerCells.Equals(other.winnerCells))
                return false;
            return true;
        }
        */
        #endregion

        /// <summary>
        /// Traces out the potential of input bits.
        /// </summary>
        public void TraceInputPotential(bool traceAllValues = false)
        {
            int[] inputPotential = new int[this.HtmConfig.NumInputs];

            for (int i = 0; i < this.HtmConfig.NumColumns; i++)
            {
                Column col = GetColumn(i);
                for (int k = 0; k < col.ProximalDendrite.ConnectedInputs.Length; k++)
                {
                    int inpIndx = col.ProximalDendrite.ConnectedInputs[k];
                    inputPotential[inpIndx] = inputPotential[inpIndx] + 1;
                }
            }

            if (traceAllValues)
            {
                for (int i = 0; i < inputPotential.Length; i++)
                {
                    Debug.WriteLine($"{i} - {inputPotential[i]}");
                }
            }

            Debug.WriteLine($"Max: {inputPotential.Max()} - Min: {inputPotential.Min()}, AVG: {inputPotential.Average()}");
        }

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Connections), writer);

            ser.SerializeValue(Connections.EPSILON, writer);
            ser.SerializeValue(this.version, writer);
            ser.SerializeValue(this.SpIterationNum, writer);
            ser.SerializeValue(this.SpIterationLearnNum, writer);
            ser.SerializeValue(this.m_TMIteration, writer);
            ser.SerializeValue(this.m_BoostedmOverlaps, writer);
            ser.SerializeValue(this.m_Overlaps, writer);
            ser.SerializeValue(this.m_TieBreaker, writer);

            this.connectedCounts2.Serialize(writer);



            ser.SerializeValue(this.Cells, writer);
            ser.SerializeValue(this.m_BoostFactors, writer);



            ser.SerializeValue(this.m_ActiveSegments, writer);
            ser.SerializeValue(this.m_MatchingSegments, writer);

            this.m_HtmConfig.Serialize(writer);


            //ser.SerializeValue(this.m_DistalSegments, writer);
            ser.SerializeValue(this.m_DistalSynapses, writer);
            ser.SerializeValue(this.m_NextFlatIdx, writer);
            ser.SerializeValue(this.m_NextSegmentOrdinal, writer);
            ser.SerializeValue(this.m_NextSynapseOrdinal, writer);
            ser.SerializeValue(this.m_NumSynapses, writer);
            ser.SerializeValue(this.m_FreeFlatIdxs, writer);

            // TODO!!!
            //ser.SerializeValue(this.m_SegmentForFlatIdx, writer);

            this.LastActivity.Serialize(writer);

            ser.SerializeValue(this.NextSegmentOrdinal, writer);
            ser.SerializeValue(this.TieBreaker, writer);
            ser.SerializeValue(this.BoostedOverlaps, writer);
            ser.SerializeValue(this.Overlaps, writer);
            ser.SerializeValue(this.BoostFactors, writer);
            ser.SerializeValue(this.ActiveSegments, writer);
            ser.SerializeValue(this.MatchingSegments, writer);

            ser.SerializeEnd(nameof(Connections), writer);
        }

        public static Connections Deserialize(StreamReader reader)
        {
            Connections mem = new Connections();
            // |T|ODO
            return mem;
        }
        #endregion

    }
}
