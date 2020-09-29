// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Types;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// 
    /// Contains the definition of the interconnected structural state of the {@link SpatialPooler}
    /// and
    /// { @link TemporalMemory} as well as the state of all support structures
    /// (i.e. Cells, Columns, Segments, Synapses etc.).
    /// 
    /// In the separation of data from logic, this class represents the data/state.
    ///
    /// </summary>
    [Serializable]
    public class Connections //implements Persistable
    {

        public static readonly double EPSILON = 0.00001;

        /////////////////////////////////////// Spatial Pooler Vars ///////////////////////////////////////////
        /** <b>WARNING:</b> potentialRadius **must** be set to 
         * the inputWidth if using "globalInhibition" and if not 
         * using the Network API (which sets this automatically) 
         */

        //private int potentialRadius = 16;
        //private double potentialPct = 0.5;
        //private bool m_GlobalInhibition = false;
        //private double m_LocalAreaDensity = -1.0;
        //private double m_NumActiveColumnsPerInhArea;
        //private double m_StimulusThreshold = 0;
        //private double synPermInactiveDec = 0.008;
        //private double synPermActiveInc = 0.05;
        //private double synPermConnected = 0.10;
        //private double synPermBelowStimulusInc;// = synPermConnected / 10.0;
        //private double minPctOverlapDutyCycles = 0.001;
        //private double minPctActiveDutyCycles = 0.001;
        //private double predictedSegmentDecrement = 0.0;
        //private int dutyCyclePeriod = 1000;
        //private double maxBoost = 10.0;
        //private bool wrapAround = true;
        //private bool isBumpUpWeakColumnsDisabled = false;

        //private int numInputs = 1;  //product of input dimensions
        private int numColumns = 1; //product of column dimensions

        //Extra parameter settings
        //private double synPermMin = 0.0;
        //private double synPermMax = 1.0;
        //private double synPermTrimThreshold;// = synPermActiveInc / 2.0;
        //private int updatePeriod = 50;
        //private double initConnectedPct = 0.5;

        //Internal state
        private double version = 1.0;
        public int spIterationNum = 0;
        public int spIterationLearnNum = 0;
        public long tmIteration = 0;

        public double[] m_BoostedmOverlaps;
        public int[] m_Overlaps;

        /// <summary>
        /// Manages input neighborhood transformations
        /// </summary>
        //private Topology inputTopology;
        /// <summary>
        /// Manages column neighborhood transformations
        /// </summary>
        //private Topology columnTopology;
        /// <summary>
        /// A matrix representing the shape of the input.
        /// </summary>
        //protected ISparseMatrix<int> inputMatrix;
        /**
         * Store the set of all inputs that are within each column's potential pool.
         * 'potentialPools' is a matrix, whose rows represent cortical columns, and
         * whose columns represent the input bits. if potentialPools[i][j] == 1,
         * then input bit 'j' is in column 'i's potential pool. A column can only be
         * connected to inputs in its potential pool. The indices refer to a
         * flattened version of both the inputs and columns. Namely, irrespective
         * of the topology of the inputs and columns, they are treated as being a
         * one dimensional array. Since a column is typically connected to only a
         * subset of the inputs, many of the entries in the matrix are 0. Therefore
         * the potentialPool matrix is stored using the SparseObjectMatrix
         * class, to reduce memory footprint and computation time of algorithms that
         * require iterating over the data structure.
         */
        //private IFlatMatrix<Pool> potentialPools;

        /// <summary>
        /// Initialize a tiny random tie breaker. This is used to determine winning
        /// columns where the overlaps are identical.
        /// </summary>
        private double[] tieBreaker;

        /// <summary>
        /// Stores the number of connected synapses for each column. This is simply
        /// a sum of each row of 'connectedSynapses'. again, while this
        /// information is readily available from 'connectedSynapses', it is
        /// stored separately for efficiency purposes.
        /// </summary>
        private AbstractSparseBinaryMatrix connectedCounts2;

        /// <summary>
        /// All cells. Initialized during initialization of the TemporalMemory.
        /// </summary>
        public Cell[] Cells { get; set; }

        /// <summary>
        /// The inhibition radius determines the size of a column's local
        /// neighborhood. of a column. A cortical column must overcome the overlap
        /// score of columns in its neighborhood in order to become actives. This
        /// radius is updated every learning round. It grows and shrinks with the
        /// average number of connected synapses per column.
        /// </summary>
        //private int m_InhibitionRadius = 0;

        //private double[] overlapDutyCycles;
        //private double[] activeDutyCycles;
        //private volatile double[] minOverlapDutyCycles;
        //private volatile double[] minActiveDutyCycles;
        private double[] m_BoostFactors;

        /////////////////////////////////////// Temporal Memory Vars ///////////////////////////////////////////

        protected ISet<Cell> activeCells = new LinkedHashSet<Cell>();
        protected ISet<Cell> winnerCells = new LinkedHashSet<Cell>();
        protected ISet<Cell> predictiveCells = new LinkedHashSet<Cell>();
        protected List<DistalDendrite> activeSegments = new List<DistalDendrite>();
        protected List<DistalDendrite> matchingSegments = new List<DistalDendrite>();

        /// <summary>
        /// Total number of columns
        /// </summary>
        //protected int[] columnDimensions = new int[] { 2048 };
        /// <summary>
        /// Total number of cells per column
        /// </summary>
        //protected int cellsPerColumn = 32;
        /// <summary>
        /// What will comprise the Layer input. Input (i.e. from encoder)
        /// </summary>
        //protected int[] inputDimensions = new int[] { 100 };
        /// <summary>
        /// If the number of active connected synapses on a segment
        /// is at least this threshold, the segment is said to be active.
        /// </summary>
        //private int activationThreshold = 13;
        /// <summary>
        /// Radius around cell from which it can
        /// sample to form distal {@link DistalDendrite} connections.
        /// </summary>
        //private int learningRadius = 2048;
        /// <summary>
        /// If the number of synapses active on a segment is at least this 
        /// threshold, it is selected as the best matching 
        /// cell in a bursting column. 
        /// </summary>
        //private int minThreshold = 10;
        /// <summary>
        /// The maximum number of synapses added to a segment during learning.
        /// </summary>
        //private int maxNewSynapseCount = 20;
        /// <summary>
        /// The maximum number of segments (distal dendrites) allowed on a cell
        /// </summary>
        //private int maxSegmentsPerCell = 255;
        /// <summary>
        /// The maximum number of synapses allowed on a given segment (distal dendrite)
        /// </summary>
        //private int maxSynapsesPerSegment = 255;
        /// <summary>
        /// Initial permanence of a new synapse
        /// </summary>
        //private double initialPermanence = 0.21;
        /// <summary>
        /// If the permanence value for a synapse
        /// is greater than this value, it is said
        /// to be connected.
        /// </summary>
        //private double connectedPermanence = 0.50;
        /// <summary>
        /// Amount by which permanences of synapses
        /// are incremented during learning.
        /// </summary>
        //private double permanenceIncrement = 0.10;
        /// <summary>
        /// Amount by which permanences of synapses
        /// are decremented during learning.
        /// </summary>
        //private double permanenceDecrement = 0.10;

        /// <summary>
        /// The main data structure containing columns, cells, and synapses
        /// </summary>
        //private AbstractSparseMatrix<Column> memory;

        //public HtmModuleTopology ColumnTopology
        //{
        //    get
        //    {
        //        return this.HtmConfig.Memory?.ModuleTopology;
        //    }
        //}

        //public HtmModuleTopology InputTopology
        //{
        //    get
        //    {
        //        return this.HtmConfig.InputMatrix?.ModuleTopology;
        //    }
        //}

        private HtmConfig m_HtmConfig = new HtmConfig();

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

                m_HtmConfig.SynPermBelowStimulusInc = m_HtmConfig.SynPermConnected / 10.0;
                m_HtmConfig.SynPermTrimThreshold = m_HtmConfig.SynPermActiveInc / 2.0;
                m_HtmConfig.ColumnModuleTopology = m_HtmConfig.Memory?.ModuleTopology;
                m_HtmConfig.InputModuleTopology = m_HtmConfig.InputMatrix?.ModuleTopology;
                //m_HtmConfig.InputTopology = this.InputTopology;
                //m_HtmConfig.IsWrapAround = this.isWrapAround();
                //m_HtmConfig.NumInputs = this.NumInputs;
                m_HtmConfig.NumColumns = m_HtmConfig.Memory != null ? m_HtmConfig.Memory.getMaxIndex() + 1 : -1;
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
        }

       

        ///////////////////////   Structural Elements /////////////////////////
        
        /// <summary>
        /// Reverse mapping from source cell to {@link Synapse}
        /// </summary>
        public Dictionary<Cell, LinkedHashSet<Synapse>> receptorSynapses;

        protected Dictionary<Cell, List<DistalDendrite>> distalSegments;

        /// <summary>
        /// Synapses, which belong to some distal dentrite segment.
        /// </summary>
        public Dictionary<Segment, List<Synapse>> distalSynapses;

        //protected Dictionary<Segment, List<Synapse>> proximalSynapses;

        /** Helps index each new proximal Synapse */
        //protected int proximalSynapseCounter = -1;

        /// <summary>
        /// Global tracker of the next available segment index
        /// </summary>
        protected int nextFlatIdx;
        /// <summary>
        /// Global counter incremented for each DD segment creation
        /// </summary>
        protected int nextSegmentOrdinal;
        /// <summary>
        /// Global counter incremented for each DD synapse creation
        /// </summary>
        protected int nextSynapseOrdinal;
        /// <summary>
        /// Total number of synapses
        /// </summary>
        protected long NumSynapses { get; set; }

        /// <summary>
        /// Used for destroying of indexes.
        /// </summary>
        protected List<int> freeFlatIdxs = new List<int>();

        /// <summary>
        /// Indexed segments by their global index (can contain nulls)
        /// </summary>
        protected List<DistalDendrite> m_SegmentForFlatIdx = new List<DistalDendrite>();

        /// <summary>
        /// Stores each cycle's most recent activity
        /// </summary>
        public SegmentActivity lastActivity;

        /// <summary>
        /// The default random number seed
        /// </summary>
        //protected int seed = 42;

        /// <summary>
        /// The random number generator
        /// </summary>
        public Random random;

        public int getNextSegmentOrdinal()
        {
            return nextSegmentOrdinal;
        }

        ///** Sorting Lambda used for sorting active and matching segments */
        //public IComparer<DistalDendrite> segmentPositionSortKey = (s1, s2) =>
        //        {
        //            double c1 = s1.getParentCell().getIndex() + ((double)(s1.getOrdinal() / (double)nextSegmentOrdinal));
        //            double c2 = s2.getParentCell().getIndex() + ((double)(s2.getOrdinal() / (double)nextSegmentOrdinal));
        //            return c1 == c2 ? 0 : c1 > c2 ? 1 : -1;
        //        };



        /** Sorting Lambda used for SpatialPooler inhibition */

        //public Comparator<Pair<Integer, Double>> inhibitionComparator = (Comparator<Pair<Integer, Double>> & Serializable)

        //    (p1, p2)-> { 

        //    int p1key = p1.getFirst();

        //int p2key = p2.getFirst();

        //double p1val = p1.getSecond();

        //double p2val = p2.getSecond();

        //    if(Math.abs(p2val - p1val) < 0.000000001) {

        //        return Math.abs(p2key - p1key) < 0.000000001 ? 0 : p2key > p1key? -1 : 1;

        //    } else {

        //        return p2val > p1val? -1 : 1;

        //    }

        //};

        #region Connections Constructor
        /// <summary>
        /// Constructs a new {@code OldConnections} object. This object
        /// is usually configured via the {@link Parameters#apply(Object)}
        /// method.
        /// </summary>
        public Connections()
        {
            //synPermTrimThreshold = synPermActiveInc / 2.0;
            //synPermBelowStimulusInc = synPermConnected / 10.0;
            //random = new Random(seed);
        }

        public Connections(HtmConfig prms)
        {
            //this.permanenceDecrement = (double)prms[KEY.PERMANENCE_DECREMENT];
            //this.permanenceDecrement = prms.TemporalMemory.PermanenceDecrement;
        }

        ///**
        // * Returns a deep copy of this {@code Connections} object.
        // * @return a deep copy of this {@code Connections}
        // */
        //public Connections copy() //todo this will fail. Many objects are not marked as serializable
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(stream, this);
        //        stream.Position = 0;
        //        return (Connections)formatter.Deserialize(stream);
        //    }
        //}
        #endregion

        #region General Methods

        /// <summary>
        /// Sets the derived values of the <see cref="SpatialPoller"/> {@link SpatialPooler}'s initialization.
        /// </summary>
        public void DoSpatialPoolerPostInit()
        {
            //synPermBelowStimulusInc = synPermConnected / 10.0;
            //synPermTrimThreshold = synPermActiveInc / 2.0;
            if (HtmConfig.PotentialRadius == -1)
            {
                this.HtmConfig.PotentialRadius = ArrayUtils.Product(this.HtmConfig.InputDimensions);
            }
        }

        ///// <summary>
        ///// Sets the seed used for the internal random number generator.
        ///// If the generator has been instantiated, this method will initialize
        ///// a new random generator with the specified seed.
        ///// </summary>
        ///// <param name="seed"></param>
        //public void setSeed(int seed)
        //{
        //    this.seed = seed;
        //}

        ///// <summary>
        ///// Returns the configured random number seed
        ///// </summary>
        ///// <returns></returns>
        //public int getSeed()
        //{
        //    return seed;
        //}

        /// <summary>
        /// Returns the thread specific {@link Random} number generator.
        /// </summary>
        /// <returns></returns>
        public Random getRandom()
        {
            return random;
        }

        /// <summary>
        /// Sets the random number generator.
        /// </summary>
        /// <param name="random"></param>
        public void setRandom(Random random)
        {
            this.random = random;
        }

        /**
         * Returns the {@link Cell} specified by the index passed in.
         * @param index     of the specified cell to return.
         * @return
         */
        public Cell getCell(int index)
        {
            return Cells[index];
        }

      
        /**
         * Returns an array containing the {@link Cell}s specified
         * by the passed in indexes.
         *
         * @param cellIndexes   indexes of the Cells to return
         * @return
         */
        public Cell[] getCells(int[] cellIndexes)
        {
            Cell[] retVal = new Cell[cellIndexes.Length];
            for (int i = 0; i < cellIndexes.Length; i++)
            {
                retVal[i] = Cells[cellIndexes[i]];
            }
            return retVal;
        }

        /**
         * Returns a {@link LinkedHashSet} containing the {@link Cell}s specified
         * by the passed in indexes.
         *
         * @param cellIndexes   indexes of the Cells to return
         * @return
         */
        public LinkedHashSet<Cell> getCellSet(int[] cellIndexes)
        {
            LinkedHashSet<Cell> retVal = new LinkedHashSet<Cell>();
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

        /**
         * Returns the cycle count.
         * @return
         */
        public int getIterationNum()
        {
            return spIterationNum;
        }

        /**
         * Sets the iteration count.
         * @param num
         */
        public void setIterationNum(int num)
        {
            this.spIterationNum = num;
        }

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


        /// <summary>
        /// Returns the total numbe rof columns across all dimensions.
        /// </summary>
        public int NumColumns
        {
            get
            {
                return this.numColumns;
            }
        }

        /**
         * Sets the product of the column dimensions to be
         * the column count.
         * @param n
         */
        public void setNumColumns(int n)
        {
            this.numColumns = n;
        }

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

        /**
         * Sets the {@link SparseObjectMatrix} which represents the
         * proximal dendrite permanence values.
         *
         * @param s the {@link SparseObjectMatrix}
         */
        public void setProximalPermanences(AbstractSparseMatrix<double[]> s)
        {
            foreach (int idx in s.getSparseIndices())
            {
                this.HtmConfig.Memory.getObject(idx).setPermanences(this.HtmConfig, s.getObject(idx));
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
            int[] counts = new int[NumColumns];
            for (int i = 0; i < NumColumns; i++)
            {
                counts[i] = getColumn(i).ConnectedInputCounterMatrix.getTrueCounts()[0];
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

        /**
         * Sets the indexed count of synapses connected at the columns in each index.
         * @param counts
         */
        public void setConnectedCounts(int[] counts)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                getColumn(i).ConnectedInputCounterMatrix.setTrueCount(0, counts[i]);
                //connectedCounts.setTrueCount(i, counts[i]);
            }
        }

        /**
         * Sets the connected count {@link AbstractSparseBinaryMatrix}, 
         * which defines how synapses are connected to input.
         * @param columnIndex
         * @param count
         */
        public void setConnectedMatrix(AbstractSparseBinaryMatrix matrix)
        {
            for (int col = 0; col < this.NumColumns; col++)
            {
                var colMatrix = this.getColumn(col).ConnectedInputCounterMatrix = new SparseBinaryMatrix(new int[] { 1, this.HtmConfig.NumInputs });

                int[] row = (int[])matrix.getSlice(col);

                for (int j = 0; j < row.Length; j++)
                {
                    colMatrix.set(row[j], 0, j);
                }
            }

            // this.connectedCounts = matrix;
        }


        /**
         * Sets the array holding the random noise added to proximal dendrite overlaps.
         *
         * @param tieBreaker	random values to help break ties
         */
        public void setTieBreaker(double[] tieBreaker)
        {
            this.tieBreaker = tieBreaker;
        }

        /**
         * Returns the array holding random values used to add to overlap scores
         * to break ties.
         *
         * @return
         */
        public double[] getTieBreaker()
        {
            return tieBreaker;
        }


        /// <summary>
        /// Enforses using of global inhibition process.
        /// </summary>
        //public bool GlobalInhibition { get => m_GlobalInhibition; set => this.m_GlobalInhibition = value; }


        /**
         * The desired density of active columns within a local
         * inhibition area (the size of which is set by the
         * internally calculated inhibitionRadius, which is in
         * turn determined from the average size of the
         * connected potential pools of all columns). The
         * inhibition logic will insure that at most N columns
         * remain ON within a local inhibition area, where N =
         * localAreaDensity * (total number of columns in
         * inhibition area).
         *
         * @param localAreaDensity
         */
        //public void setLocalAreaDensity(double localAreaDensity)
        //{
        //    this.m_LocalAreaDensity = localAreaDensity;
        //}

        /**
         * Returns the configured local area density
         * @return  the configured local area density
         * @see setLocalAreaDensity
         */

        ///// <summary>
        /////     The desired density of active columns within a local
        /////     inhibition area(the size of which is set by the
        /////     internally calculated inhibitionRadius, which is in
        /////     turn determined from the average size of the
        /////
        /////     connected potential pools of all columns). The
        /////     inhibition logic will insure that at most N columns
        /////         remain ON within a local inhibition area, where N =
        /////         localAreaDensity * (total number of columns in
        /////         inhibition area).

        ///// </summary>
        //public double LocalAreaDensity
        //{
        //    get
        //    {
        //        return m_LocalAreaDensity;
        //    }
        //    set
        //    {
        //        m_LocalAreaDensity = value;
        //    }
        //}

        /**
         * Returns the configured number of active columns per
         * inhibition area.
         * @return  the configured number of active columns per
         * inhibition area.
         * @see setNumActiveColumnsPerInhArea
         */
        /**
 * An alternate way to control the density of the active
 * columns. If numActivePerInhArea is specified then
 * localAreaDensity must be less than 0, and vice versa.
 * When using numActivePerInhArea, the inhibition logic
 * will insure that at most 'numActivePerInhArea'
 * columns remain ON within a local inhibition area (the
 * size of which is set by the internally calculated
 * inhibitionRadius, which is in turn determined from
 * the average size of the connected receptive fields of
 * all columns). When using this method, as columns
 * learn and grow their effective receptive fields, the
 * inhibitionRadius will grow, and hence the net density
 * of the active columns will *decrease*. This is in
 * contrast to the localAreaDensity method, which keeps
 * the density of active columns the same regardless of
 * the size of their receptive fields.
 *
 * @param numActiveColumnsPerInhArea
 */
        //public double NumActiveColumnsPerInhArea { get => m_NumActiveColumnsPerInhArea; set => this.m_NumActiveColumnsPerInhArea = value; }


        /// <summary>
        /// Minimum number of connected synapses to make column active. Specified as a percent of a fully grown synapse.
        /// </summary>
        //public double StimulusThreshold { get => m_StimulusThreshold; set => this.m_StimulusThreshold = value; }

        ///**
        // * The amount by which an inactive synapse is
        // * decremented in each round. Specified as a percent of
        // * a fully grown synapse.
        // *
        // * @param synPermInactiveDec
        // */
        //public void setSynPermInactiveDec(double synPermInactiveDec)
        //{
        //    this.synPermInactiveDec = synPermInactiveDec;
        //}

        ///**
        // * Returns the synaptic permanence inactive decrement.
        // * @return  the synaptic permanence inactive decrement.
        // * @see setSynPermInactiveDec
        // */
        //public double getSynPermInactiveDec()
        //{
        //    return synPermInactiveDec;
        //}

        ///**
        // * The amount by which an active synapse is incremented
        // * in each round. Specified as a percent of a
        // * fully grown synapse.
        // *
        // * @param synPermActiveInc
        // */
        //public void setSynPermActiveInc(double synPermActiveIncValue)
        //{
        //    synPermActiveInc = synPermActiveIncValue;
        //}

        ///**
        // * Returns the configured active permanence increment
        // * @return the configured active permanence increment
        // * @see setSynPermActiveInc
        // */
        //public double getSynPermActiveInc()
        //{
        //    return synPermActiveInc;
        //}

        ///**
        // * The default connected threshold. Any synapse whose
        // * permanence value is above the connected threshold is
        // * a "connected synapse", meaning it can contribute to
        // * the cell's firing.
        // *
        // * @param synPermConnected
        // */
        //public void setSynPermConnected(double synPermConnectedValue)
        //{
        //    this.synPermConnected = synPermConnectedValue;
        //}

        ///**
        // * Returns the synapse permanence connected threshold
        // * @return the synapse permanence connected threshold
        // * @see setSynPermConnected
        // */
        //public double getSynPermConnected()
        //{
        //    return synPermConnected;
        //}

        ///**
        // * Sets the stimulus increment for synapse permanences below
        // * the measured threshold.
        // * @param stim
        // */
        //public void setSynPermBelowStimulusInc(double stim)
        //{
        //    this.synPermBelowStimulusInc = stim;
        //}

        ///**
        // * Returns the stimulus increment for synapse permanences below
        // * the measured threshold.
        // *
        // * @return
        // */
        //public double getSynPermBelowStimulusInc()
        //{
        //    return synPermBelowStimulusInc;
        //}

        /**
         * A number between 0 and 1.0, used to set a floor on
         * how often a column should have at least
         * stimulusThreshold active inputs. Periodically, each
         * column looks at the overlap duty cycle of
         * all other columns within its inhibition radius and
         * sets its own internal minimal acceptable duty cycle
         * to: minPctDutyCycleBeforeInh * max(other columns'
         * duty cycles).
         * On each iteration, any column whose overlap duty
         * cycle falls below this computed value will  get
         * all of its permanence values boosted up by
         * synPermActiveInc. Raising all permanences in response
         * to a sub-par duty cycle before  inhibition allows a
         * cell to search for new inputs when either its
         * previously learned inputs are no longer ever active,
         * or when the vast majority of them have been
         * "hijacked" by other columns.
         *
         * @param minPctOverlapDutyCycle
         */
        //public void setMinPctOverlapDutyCycles(double minPctOverlapDutyCycle)
        //{
        //    this.minPctOverlapDutyCycles = minPctOverlapDutyCycle;
        //}

        ///**
        // * see {@link #setMinPctOverlapDutyCycles(double)}
        // * @return
        // */
        //public double getMinPctOverlapDutyCycles()
        //{
        //    return minPctOverlapDutyCycles;
        //}

        ///// <summary>
        ///// NEW
        ///// </summary>
        ///// <param name="val"></param>
        //public void updateMinPctOverlapDutyCycles(double val)
        //{
        //    minPctOverlapDutyCycles = val;
        //}
        /**
         * A number between 0 and 1.0, used to set a floor on
         * how often a column should be activate.
         * Periodically, each column looks at the activity duty
         * cycle of all other columns within its inhibition
         * radius and sets its own internal minimal acceptable
         * duty cycle to:
         *   minPctDutyCycleAfterInh *
         *   max(other columns' duty cycles).
         * On each iteration, any column whose duty cycle after
         * inhibition falls below this computed value will get
         * its internal boost factor increased.
         *
         * @param minPctActiveDutyCycle
         */
        //public void setMinPctActiveDutyCycles(double minPctActiveDutyCycle)
        //{
        //    this.minPctActiveDutyCycles = minPctActiveDutyCycle;
        //}

        ///**
        // * Returns the minPctActiveDutyCycle
        // * see {@link #setMinPctActiveDutyCycles(double)}
        // * @return  the minPctActiveDutyCycle
        // */
        //public double getMinPctActiveDutyCycles()
        //{
        //    return minPctActiveDutyCycles;
        //}

        ///**
        // * The period used to calculate duty cycles. Higher
        // * values make it take longer to respond to changes in
        // * boost or synPerConnectedCell. Shorter values make it
        // * more unstable and likely to oscillate.
        // *
        // * @param dutyCyclePeriod
        // */
        //public void setDutyCyclePeriod(int dutyCyclePeriod)
        //{
        //    this.dutyCyclePeriod = dutyCyclePeriod;
        //}

        ///**
        // * Returns the configured duty cycle period
        // * see {@link #setDutyCyclePeriod(double)}
        // * @return  the configured duty cycle period
        // */
        //public int getDutyCyclePeriod()
        //{
        //    return dutyCyclePeriod;
        //}

        ///**
        // * The maximum overlap boost factor. Each column's
        // * overlap gets multiplied by a boost factor
        // * before it gets considered for inhibition.
        // * The actual boost factor for a column is number
        // * between 1.0 and maxBoost. A boost factor of 1.0 is
        // * used if the duty cycle is &gt;= minOverlapDutyCycle,
        // * maxBoost is used if the duty cycle is 0, and any duty
        // * cycle in between is linearly extrapolated from these
        // * 2 end points.
        // *
        // * @param maxBoost
        // */
        //public void setMaxBoost(double maxBoost)
        //{
        //    this.maxBoost = maxBoost;
        //}

        ///**
        // * Returns the max boost
        // * see {@link #setMaxBoost(double)}
        // * @return  the max boost
        // */
        //public double getMaxBoost()
        //{
        //    return maxBoost;
        //}

        ///**
        // * Specifies whether neighborhoods wider than the 
        // * borders wrap around to the other side.
        // * @param b
        // */
        //public void setWrapAround(bool b)
        //{
        //    this.wrapAround = b;
        //}

        ///**
        // * Returns a flag indicating whether neighborhoods
        // * wider than the borders, wrap around to the other
        // * side.
        // * @return
        // */
        //public bool isWrapAround()
        //{
        //    return wrapAround;
        //}

        /**
         * Returns the boosted overlap score for each column
         * @return the boosted overlaps
         */
        /**
 * Sets and Returns the boosted overlap score for each column
 * @param boostedOverlaps
 * @return
 * 
 * 
        ///**
        // * Sets the synPermTrimThreshold
        // * @param threshold
        // */
        //public void setSynPermTrimThreshold(double threshold)
        //{
        //    this.synPermTrimThreshold = threshold;
        //}

        ///**
        // * Returns the synPermTrimThreshold
        // * @return
        // */
        //public double getSynPermTrimThreshold()
        //{
        //    return synPermTrimThreshold;
        //}

        /**
         * Sets the {@link FlatMatrix} which holds the mapping
         * of column indexes to their lists of potential inputs.
         *
         * @param pools		{@link FlatMatrix} which holds the pools.
         */
        //public void setPotentialPools(IFlatMatrix<Pool> pools)
        //{
        //    this.potentialPools = pools;
        //}

        /**
         * Returns the {@link FlatMatrix} which holds the mapping
         * of column indexes to their lists of potential inputs.
         * @return	the potential pools
         */
        //public IFlatMatrix<Pool> getPotentialPoolsOld()
        //{
        //    return this.potentialPools;
        //}

        ///**
        // * Returns the minimum {@link Synapse} permanence.
        // * @return
        // */
        //public double getSynPermMin()
        //{
        //    return synPermMin;
        //}

        ///**
        // * Returns the maximum {@link Synapse} permanence.
        // * @return
        // */
        //public double getSynPermMax()
        //{
        //    return synPermMax;
        //}

        ///**
        // * Sets the number of {@link Column}.
        // *
        // * @param columnDimensions
        // */
        //public void setColumnDimensions(int[] columnDimensions)
        //{
        //    this.columnDimensions = columnDimensions;
        //}

        ///**
        // * Gets the number of {@link Column}.
        // *
        // * @return columnDimensions
        // */
        //public int[] getColumnDimensions()
        //{
        //    return this.columnDimensions;
        //}

        ///**
        // * A list representing the dimensions of the input
        // * vector. Format is [height, width, depth, ...], where
        // * each value represents the size of the dimension. For a
        // * topology of one dimension with 100 inputs use 100, or
        // * [100]. For a two dimensional topology of 10x5 use
        // * [10,5].
        // *
        // * @param inputDimensions
        // */
        //public void setInputDimensions(int[] inputDimensions)
        //{
        //    this.inputDimensions = inputDimensions;
        //}

        ///**
        // * Returns the configured input dimensions
        // * see {@link #setInputDimensions(int[])}
        // * @return the configured input dimensions
        // */
        //public int[] getInputDimensions()
        //{
        //    return inputDimensions;
        //}

        ///**
        // * Sets the number of {@link Cell}s per {@link Column}
        // * @param cellsPerColumn
        // */
        //public void setCellsPerColumn(int cellsPerColumn)
        //{
        //    this.cellsPerColumn = cellsPerColumn;
        //}

        ///**
        // * Gets the number of {@link Cell}s per {@link Column}.
        // *
        // * @return cellsPerColumn
        // */
        //public int getCellsPerColumn()
        //{
        //    return this.cellsPerColumn;
        //}

        ///**
        // * Sets the activation threshold.
        // *
        // * If the number of active connected synapses on a segment
        // * is at least this threshold, the segment is said to be active.
        // *
        // * @param activationThreshold
        // */
        //public void setActivationThreshold(int activationThreshold)
        //{
        //    this.activationThreshold = activationThreshold;
        //}

        ///**
        // * Returns the activation threshold.
        // * @return
        // */
        //public int getActivationThreshold()
        //{
        //    return activationThreshold;
        //}

        ///**
        // * Radius around cell from which it can
        // * sample to form distal dendrite connections.
        // *
        // * @param   learningRadius
        // */
        //public void setLearningRadius(int learningRadius)
        //{
        //    this.learningRadius = learningRadius;
        //}

        ///**
        // * Returns the learning radius.
        // * @return
        // */
        //public int getLearningRadius()
        //{
        //    return learningRadius;
        //}

        ///**
        // * If the number of synapses active on a segment is at least this
        // * threshold, it is selected as the best matching
        // * cell in a bursting column.
        // *
        // * @param   minThreshold
        // */
        //public void setMinThreshold(int minThreshold)
        //{
        //    this.minThreshold = minThreshold;
        //}

        ///**
        // * Returns the minimum threshold of the number of active synapses to be picked as best.
        // * @return
        // */
        //public int getMinThreshold()
        //{
        //    return minThreshold;
        //}

        /**
         * The maximum number of synapses added to a segment during learning.
         *
         * @param   maxNewSynapseCount
         */
        //public void setMaxNewSynapseCount(int maxNewSynapseCount)
        //{
        //    this.maxNewSynapseCount = maxNewSynapseCount;
        //}

        ///**
        // * Returns the maximum number of synapses added to a segment during
        // * learning.
        // *
        // * @return
        // */
        //public int getMaxNewSynapseCount()
        //{
        //    return maxNewSynapseCount;
        //}

        /**
         * The maximum number of segments allowed on a given cell
         * @param maxSegmentsPerCell
         */
        //public void setMaxSegmentsPerCell(int maxSegmentsPerCell)
        //{
        //    this.maxSegmentsPerCell = maxSegmentsPerCell;
        //}

        ///**
        // * Returns the maximum number of segments allowed on a given cell
        // * @return
        // */
        //public int getMaxSegmentsPerCell()
        //{
        //    return maxSegmentsPerCell;
        //}

        ///**
        // * The maximum number of synapses allowed on a given segment
        // * @param maxSynapsesPerSegment
        // */
        //public void setMaxSynapsesPerSegment(int maxSynapsesPerSegment)
        //{
        //    this.maxSynapsesPerSegment = maxSynapsesPerSegment;
        //}

        ///**
        // * Returns the maximum number of synapses allowed per segment
        // * @return
        // */
        //public int getMaxSynapsesPerSegment()
        //{
        //    return maxSynapsesPerSegment;
        //}

        ///**
        // * Initial permanence of a new synapse
        // *
        // * @param   initialPermanence
        // */
        //public void setInitialPermanence(double initialPermanence)
        //{
        //    this.initialPermanence = initialPermanence;
        //}

        ///**
        // * Returns the initial permanence setting.
        // * @return
        // */
        //public double getInitialPermanence()
        //{
        //    return initialPermanence;
        //}

        ///**
        // * If the permanence value for a synapse
        // * is greater than this value, it is said
        // * to be connected.
        // *
        // * @param connectedPermanence
        // */
        //public void setConnectedPermanence(double connectedPermanence)
        //{
        //    this.connectedPermanence = connectedPermanence;
        //}

        ///**
        // * If the permanence value for a synapse
        // * is greater than this value, it is said
        // * to be connected.
        // *
        // * @return
        // */
        //public double getConnectedPermanence()
        //{
        //    return connectedPermanence;
        //}

        ///**
        // * Amount by which permanences of synapses
        // * are incremented during learning.
        // *
        // * @param   permanenceIncrement
        // */
        //public void setPermanenceIncrement(double permanenceIncrement)
        //{
        //    this.permanenceIncrement = permanenceIncrement;
        //}

        ///**
        // * Amount by which permanences of synapses
        // * are incremented during learning.
        // */
        //public double getPermanenceIncrement()
        //{
        //    return this.permanenceIncrement;
        //}

        ///**
        // * Amount by which permanences of synapses
        // * are decremented during learning.
        // *
        // * @param   permanenceDecrement
        // */
        //public void setPermanenceDecrement(double permanenceDecrement)
        //{
        //    this.permanenceDecrement = permanenceDecrement;
        //}

        ///**
        // * Amount by which permanences of synapses
        // * are decremented during learning.
        // */
        //public double getPermanenceDecrement()
        //{
        //    return this.permanenceDecrement;
        //}

        ///**
        // * Amount by which active permanences of synapses of previously predicted but inactive segments are decremented.
        // * @param predictedSegmentDecrement
        // */
        //public void setPredictedSegmentDecrement(double predictedSegmentDecrement)
        //{
        //    this.predictedSegmentDecrement = predictedSegmentDecrement;
        //}

        ///**
        // * Returns the predictedSegmentDecrement amount.
        // * @return
        // */
        //public double getPredictedSegmentDecrement()
        //{
        //    return this.predictedSegmentDecrement;
        //}



        public double[] BoostedOverlaps { get => m_BoostedmOverlaps; set => this.m_BoostedmOverlaps = value; }


        /// <summary>
        /// Set/Get ovrlapps for each column.
        /// </summary>
        public int[] Overlaps { get => m_Overlaps; set => this.m_Overlaps = value; }


        /**
         * Returns the version number
         * @return
         */
        public double getVersion()
        {
            return version;
        }

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

        /**
         * Applies the dense array values which aren't -1 to the array containing
         * the active duty cycles of the column corresponding to the index specified.
         * The length of the specified array must be as long as the configured number
         * of columns of this {@code OldConnections}' column configuration.
         *
         * @param	denseActiveDutyCycles	a dense array containing values to set.
         */
        public void updateActiveDutyCycles(double[] denseActiveDutyCycles)
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

            // Every receptor synapse on active cell, which has permanence over threshold is by default connected.
            //int[] numActiveConnectedSynapsesForSegment = new int[nextFlatIdx]; // not needed

            // Every receptor synapse on active cell is active-potential one.
            //int[] numActivePotentialSynapsesForSegment = new int[nextFlatIdx]; // not needed

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
                // Synapse processed here starts with the given 'cell' and points to some other cell that owns some segment in some other column.
                // The segment owner cell in other column pointed by synapse sourced by this 'cell' is depolirized (in predicting state).
                foreach (Synapse synapse in getReceptorSynapses(cell))
                {
                    // Now, we get the segment of the synapse of the pre-synaptic cell.
                    int segFlatIndx = synapse.SegmentIndex;
                    if (numOfPotentialSynapses.ContainsKey(segFlatIndx) == false)
                        numOfPotentialSynapses.Add(segFlatIndx, 0);

                    numOfPotentialSynapses[segFlatIndx] = numOfPotentialSynapses[segFlatIndx] + 1;

                    //++numActivePotentialSynapsesForSegment[segFlatIndx];

                    if (synapse.getPermanence() > threshold)
                    {
                        if (numOfActiveSynapses.ContainsKey(segFlatIndx) == false)
                            numOfActiveSynapses.Add(segFlatIndx, 0);

                        numOfActiveSynapses[segFlatIndx] = numOfActiveSynapses[segFlatIndx] + 1;
                        //++numActiveConnectedSynapsesForSegment[segFlatIndx];
                    }
                }
            }

            return new SegmentActivity() { ActiveSynapses = numOfActiveSynapses, PotentialSynapses = numOfPotentialSynapses };
        }


        /// <summary>
        /// Returns the last activity computed during the most recent cycle.
        /// </summary>
        /// <returns></returns>
        public SegmentActivity getLastActivity()
        {
            return lastActivity;
        }

        /**
         * Record the fact that a segment had some activity. This information is
         * used during segment cleanup.
         * 
         * @param segment		the segment for which to record activity
         */
        public void recordSegmentActivity(DistalDendrite segment)
        {
            segment.setLastUsedIteration(tmIteration);
        }

        /**
         * Mark the passage of time. This information is used during segment
         * cleanup.
         */
        public void startNewIteration()
        {
            ++tmIteration;
        }


        /////////////////////////////////////////////////////////////////
        //     Segment (Specifically, Distal Dendrite) Operations      //
        /////////////////////////////////////////////////////////////////

        /**
         * Adds a new {@link DistalDendrite} segment on the specified {@link Cell},
         * or reuses an existing one.
         * 
         * @param cell  the Cell to which a segment is added.
         * @return  the newly created segment or a reused segment
         */
        public DistalDendrite CreateDistalSegment(Cell cell)
        {
            //
            // If there are more segments than maximal allowed number of segments per cell,
            // least used segments will be destroyed.
            while (numSegments(cell) >= this.HtmConfig.MaxSegmentsPerCell)
            {
                destroySegment(leastRecentlyUsedSegment(cell));
            }

            int flatIdx;
            int len;
            if ((len = freeFlatIdxs.Count()) > 0)
            {
                flatIdx = freeFlatIdxs[len - 1];
                freeFlatIdxs.RemoveRange(len - 1, 1);
            }
            else
            {
                flatIdx = nextFlatIdx;
                m_SegmentForFlatIdx.Add(null);
                ++nextFlatIdx;
            }

            int ordinal = nextSegmentOrdinal;
            ++nextSegmentOrdinal;

            DistalDendrite segment = new DistalDendrite(cell, flatIdx, tmIteration, ordinal, this.HtmConfig.SynPermConnected, this.HtmConfig.NumInputs);
            getSegments(cell, true).Add(segment);
            m_SegmentForFlatIdx[flatIdx] = segment;

            return segment;
        }

        /**
         * Destroys a segment ({@link DistalDendrite})
         * @param segment   the segment to destroy
         */
        public void destroySegment(DistalDendrite segment)
        {
            // Remove the synapses from all data structures outside this Segment.
            List<Synapse> synapses = getSynapses(segment);
            int len = synapses.Count;

            //getSynapses(segment).stream().forEach(s->removeSynapseFromPresynapticMap(s));
            foreach (var s in getSynapses(segment))
            {
                removeSynapseFromPresynapticMap(s);
            }

            NumSynapses -= len;

            // Remove the segment from the cell's list.
            getSegments(segment.ParentCell).Remove(segment);

            // Remove the segment from the map
            distalSynapses.Remove(segment);

            // Free the flatIdx and remove the final reference so the Segment can be
            // garbage-collected.
            freeFlatIdxs.Add(segment.getIndex());
            m_SegmentForFlatIdx[segment.getIndex()] = null;
        }

        /**
         * Used internally to return the least recently activated segment on 
         * the specified cell
         * 
         * @param cell  cell to search for segments on
         * @return  the least recently activated segment on 
         *          the specified cell
         */
        private DistalDendrite leastRecentlyUsedSegment(Cell cell)
        {
            List<DistalDendrite> segments = getSegments(cell, false);
            DistalDendrite minSegment = null;
            long minIteration = long.MaxValue;

            foreach (DistalDendrite dd in segments)
            {
                if (dd.getLastUsedIteration() < minIteration)
                {
                    minSegment = dd;
                    minIteration = dd.getLastUsedIteration();
                }
            }

            return minSegment;
        }

        /**
         * Returns the total number of {@link DistalDendrite}s
         * 
         * @return  the total number of segments
         */
        public int numSegments()
        {
            return numSegments(null);
        }

        /**
         * Returns the number of {@link DistalDendrite}s on a given {@link Cell}
         * if specified, or the total number if the "optionalCellArg" is null.
         * 
         * @param cell   an optional Cell to specify the context of the segment count.
         * @return  either the total number of segments or the number on a specified cell.
         */
        public int numSegments(Cell cell)
        {
            if (cell != null)
            {
                return getSegments(cell).Count;
            }

            return nextFlatIdx - freeFlatIdxs.Count;
        }

        /**
         * Returns the mapping of {@link Cell}s to their {@link DistalDendrite}s.
         *
         * @param cell      the {@link Cell} used as a key.
         * @return          the mapping of {@link Cell}s to their {@link DistalDendrite}s.
         */
        public List<DistalDendrite> getSegments(Cell cell)
        {
            return getSegments(cell, false);
        }

        /**
         * Returns the mapping of {@link Cell}s to their {@link DistalDendrite}s.
         *
         * @param cell              the {@link Cell} used as a key.
         * @param doLazyCreate      create a container for future use if true, if false
         *                          return an orphaned empty set.
         * @return          the mapping of {@link Cell}s to their {@link DistalDendrite}s.
         */
        public List<DistalDendrite> getSegments(Cell cell, bool doLazyCreate)
        {
            if (cell == null)
            {
                throw new ArgumentException("Cell was null");
            }

            if (distalSegments == null)
            {
                distalSegments = new Dictionary<Cell, List<DistalDendrite>>();
            }

            List<DistalDendrite> retVal;
            if ((distalSegments.TryGetValue(cell, out retVal)) == false)
            {
                if (!doLazyCreate) return new List<DistalDendrite>();
                distalSegments.Add(cell, retVal = new List<DistalDendrite>());
            }

            return retVal;
        }

        /**
         * Get the segment with the specified flatIdx.
         * @param index		The segment's flattened list index.
         * @return	the {@link DistalDendrite} who's index matches.
         */
        public DistalDendrite GetSegmentForFlatIdx(int index)
        {
            return m_SegmentForFlatIdx[index];
        }

        /**
         * Returns the index of the {@link Column} owning the cell which owns 
         * the specified segment.
         * @param segment   the {@link DistalDendrite} of the cell whose column index is desired.
         * @return  the owning column's index
         */
        public int columnIndexForSegment(DistalDendrite segment)
        {
            return segment.ParentCell.Index / this.HtmConfig.CellsPerColumn;
        }

        /**
         * <b>FOR TEST USE ONLY</b>
         * @return
         */
        public Dictionary<Cell, List<DistalDendrite>> getSegmentMapping()
        {
            return new Dictionary<Cell, List<DistalDendrite>>(distalSegments);
        }

        /**
         * Set by the {@link TemporalMemory} following a compute cycle.
         * @param l
         */
        public void setActiveSegments(List<DistalDendrite> l)
        {
            this.activeSegments = l;
        }

        /**
         * Retrieved by the {@link TemporalMemorty} prior to a compute cycle.
         * @return
         */
        public List<DistalDendrite> getActiveSegments()
        {
            return activeSegments;
        }

        /**
         * Set by the {@link TemporalMemory} following a compute cycle.
         * @param l
         */
        public void setMatchingSegments(List<DistalDendrite> l)
        {
            this.matchingSegments = l;
        }

        /**
         * Retrieved by the {@link TemporalMemorty} prior to a compute cycle.
         * @return
         */
        public List<DistalDendrite> getMatchingSegments()
        {
            return matchingSegments;
        }


        /////////////////////////////////////////////////////////////////
        //                    Synapse Operations                       //
        /////////////////////////////////////////////////////////////////

        /**
         * Creates a new synapse on a segment.
         * 
         * @param segment               the {@link DistalDendrite} segment to which a {@link Synapse} is 
         *                              being created
         * @param presynapticCell       the source {@link Cell}
         * @param permanence            the initial permanence
         * @return  the created {@link Synapse}
         */
        public Synapse createSynapse(DistalDendrite segment, Cell presynapticCell, double permanence)
        {
            while (GetNumSynapses(segment) >= this.HtmConfig.MaxSynapsesPerSegment)
            {
                destroySynapse(minPermanenceSynapse(segment), segment);
            }

            Synapse synapse = null;
            getSynapses(segment).Add(
                synapse = new Synapse(
                    presynapticCell, segment.getIndex(), nextSynapseOrdinal, permanence));

            getReceptorSynapses(presynapticCell, true).Add(synapse);

            ++nextSynapseOrdinal;

            ++NumSynapses;

            return synapse;
        }

        /**
         * Destroys the specified {@link Synapse}
         * @param synapse   the Synapse to destroy
         */
        public void destroySynapse(Synapse synapse, DistalDendrite segment)
        {
            --NumSynapses;

            removeSynapseFromPresynapticMap(synapse);

            //segment.Synapses.Remove(synapse);
            getSynapses(segment).Remove(synapse);
        }

        /**
         * Removes the specified {@link Synapse} from its
         * pre-synaptic {@link Cell}'s map of synapses it 
         * activates.
         * 
         * @param synapse   the synapse to remove
         */
        public void removeSynapseFromPresynapticMap(Synapse synapse)
        {
            LinkedHashSet<Synapse> presynapticSynapses;
            Cell cell = synapse.getPresynapticCell();
            (presynapticSynapses = getReceptorSynapses(cell, false)).Remove(synapse);

            if (presynapticSynapses.Count == 0)
            {
                receptorSynapses.Remove(cell);
            }
        }

        /**
         * Used internally to find the synapse with the smallest permanence
         * on the given segment.
         * 
         * @param dd    Segment object to search for synapses on
         * @return  Synapse object on the segment with the minimal permanence
         */
        private Synapse minPermanenceSynapse(DistalDendrite dd)
        {
            //List<Synapse> synapses = getSynapses(dd).stream().sorted().collect(Collectors.toList());
            List<Synapse> synapses = getSynapses(dd);
            Synapse min = null;
            double minPermanence = Double.MaxValue;

            foreach (Synapse synapse in synapses)
            {
                if (!synapse.destroyed() && synapse.getPermanence() < minPermanence - EPSILON)
                {
                    min = synapse;
                    minPermanence = synapse.getPermanence();
                }
            }

            return min;
        }

        /**
         * Returns the total number of {@link Synapse}s
         * 
         * @return  either the total number of synapses
         */
        public long GetNumSynapses()
        {
            return GetNumSynapses(null);
        }


        /**
         * Returns the number of {@link Synapse}s on a given {@link DistalDendrite}
         * if specified, or the total number if the "optionalSegmentArg" is null.
         * 
         * @param optionalSegmentArg    an optional Segment to specify the context of the synapse count.
         * @return  either the total number of synapses or the number on a specified segment.
         */
        public long GetNumSynapses(DistalDendrite optionalSegmentArg)
        {
            if (optionalSegmentArg != null)
            {
                return getSynapses(optionalSegmentArg).Count;
            }

            return NumSynapses;
        }

        /**
         * Returns the mapping of {@link Cell}s to their reverse mapped
         * {@link Synapse}s.
         *
         * @param cell      the {@link Cell} used as a key.
         * @return          the mapping of {@link Cell}s to their reverse mapped
         *                  {@link Synapse}s.
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public LinkedHashSet<Synapse> getReceptorSynapses(Cell cell)
        {
            return getReceptorSynapses(cell, false);
        }

        /**
         * Returns synapses which hold the specified cell as their source cell.
         * Returns the mapping of {@link Cell}s to their reverse mapped
         * {@link Synapse}s.
         *
         * @param cell              the {@link Cell} used as a key.
         * @param doLazyCreate      create a container for future use if true, if false
         *                          return an orphaned empty set.
         * @return          the mapping of {@link Cell}s to their reverse mapped
         *                  {@link Synapse}s.
         */
        public LinkedHashSet<Synapse> getReceptorSynapses(Cell cell, bool doLazyCreate)
        {
            if (cell == null)
            {
                throw new ArgumentException("Cell was null");
            }

            if (receptorSynapses == null)
            {
                receptorSynapses = new Dictionary<Cell, LinkedHashSet<Synapse>>();
            }

            LinkedHashSet<Synapse> retVal = null;
            if (receptorSynapses.TryGetValue(cell, out retVal) == false)
            {
                if (!doLazyCreate) return new LinkedHashSet<Synapse>();
                receptorSynapses.Add(cell, retVal = new LinkedHashSet<Synapse>());
            }

            return retVal;
        }



        /// <summary>
        /// Returns synapeses of specified dentrite segment.
        /// </summary>
        /// <param name="segment">Distal Dentrite segment.</param>
        /// <returns>List of segment synapeses.</returns>
        public List<Synapse> getSynapses(DistalDendrite segment)
        {
            if (segment == null)
            {
                throw new ArgumentException("Segment cannot be null");
            }

            if (distalSynapses == null)
            {
                distalSynapses = new Dictionary<Segment, List<Synapse>>();
            }

            List<Synapse> retVal = null;
            if (distalSynapses.TryGetValue(segment, out retVal) == false)
            {
                distalSynapses.Add(segment, retVal = new List<Synapse>());
            }

            return retVal;
        }

        /**
         * Returns the mapping of {@link ProximalDendrite}s to their {@link Synapse}s.
         *
         * @param segment   the {@link ProximalDendrite} used as a key.
         * @return          the mapping of {@link ProximalDendrite}s to their {@link Synapse}s.
         */
        //public List<Synapse> getSynapses(ProximalDendrite segment)
        //{
        //    if (segment == null)
        //    {
        //        throw new ArgumentException("Segment was null");
        //    }

        //    if (proximalSynapses == null)
        //    {
        //        proximalSynapses = new Dictionary<Segment, List<Synapse>>();
        //    }

        //    List<Synapse> retVal = null;
        //    if (proximalSynapses.ContainsKey(segment) == false)
        //    {
        //        proximalSynapses.Add(segment, retVal = new List<Synapse>());
        //    }

        //    retVal = proximalSynapses[segment];

        //    return retVal;
        //}

        /**
         * <b>FOR TEST USE ONLY<b>
         * @return
         */
        public Dictionary<Cell, LinkedHashSet<Synapse>> getReceptorSynapseMapping()
        {
            return new Dictionary<Cell, LinkedHashSet<Synapse>>(receptorSynapses);
        }

        /**
         * Clears all {@link TemporalMemory} state.
         */
        public void clear()
        {
            activeCells.Clear();
            winnerCells.Clear();
            predictiveCells.Clear();
        }

        /**
         * Returns the current {@link Set} of active {@link Cell}s
         *
         * @return  the current {@link Set} of active {@link Cell}s
         */
        public ISet<Cell> getActiveCells()
        {
            return activeCells;
        }

        /**
         * Sets the current {@link Set} of active {@link Cell}s
         * @param cells
         */
        public void setActiveCells(ISet<Cell> cells)
        {
            this.activeCells = cells;
        }

        /**
         * Returns the current {@link Set} of winner cells
         *
         * @return  the current {@link Set} of winner cells
         */
        public ISet<Cell> getWinnerCells()
        {
            return winnerCells;
        }

        /**
         * Sets the current {@link Set} of winner {@link Cell}s
         * @param cells
         */
        public void setWinnerCells(ISet<Cell> cells)
        {
            this.winnerCells = cells;
        }


        /// <summary>
        /// Generates the list of predictive cells from parent cells of active segments.
        /// </summary>
        /// <returns></returns>
        public ISet<Cell> getPredictiveCells()
        {
            if (predictiveCells.Count == 0)
            {
                Cell previousCell = null;
                Cell currCell = null;

                List<DistalDendrite> temp = new List<DistalDendrite>(activeSegments);
                foreach (DistalDendrite activeSegment in temp)
                {
                    if ((currCell = activeSegment.ParentCell) != previousCell)
                    {
                        predictiveCells.Add(previousCell = currCell);
                    }
                }
            }
            return predictiveCells;
        }

        /**
         * Clears the previous predictive cells from the list.
         */
        public void clearPredictiveCells()
        {
            this.predictiveCells.Clear();
        }

        /**
         * Returns the column at the specified index.
         * @param index
         * @return
         */
        public Column getColumn(int index)
        {
            return this.HtmConfig.Memory.getObject(index);
        }

       

        /**
         * Converts a {@link Collection} of {@link Cell}s to a list
         * of cell indexes.
         *
         * @param cells
         * @return
         */
        public static List<Integer> asCellIndexes(Collection<Cell> cells)
        {
            List<Integer> ints = new List<Integer>();
            foreach (Cell cell in cells)
            {
                ints.Add(cell.Index);
            }

            return ints;
        }

        /**
         * Converts a {@link Collection} of {@link Column}s to a list
         * of column indexes.
         *
         * @param columns
         * @return
         */
        public static List<Integer> asColumnIndexes(Collection<Column> columns)
        {
            List<Integer> ints = new List<Integer>();
            foreach (Column col in columns)
            {
                ints.Add(col.getIndex());
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

        /**
         * Returns a {@link Set} view of the {@link Column}s specified by
         * the indexes passed in.
         *
         * @param indexes		the indexes of the Columns to return
         * @return				a set view of the specified columns
         */
        public LinkedHashSet<Column> getColumnSet(int[] indexes)
        {
            LinkedHashSet<Column> retVal = new LinkedHashSet<Column>();
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal.Add(this.HtmConfig.Memory.getObject(indexes[i]));
            }
            return retVal;
        }

        /**
         * Returns a {@link List} view of the {@link Column}s specified by
         * the indexes passed in.
         *
         * @param indexes		the indexes of the Columns to return
         * @return				a List view of the specified columns
         */
        public List<Column> getColumnList(int[] indexes)
        {
            List<Column> retVal = new List<Column>();
            for (int i = 0; i < indexes.Length; i++)
            {
                retVal.Add(this.HtmConfig.Memory.getObject(indexes[i]));
            }
            return retVal;
        }

        /**
         * High 
         * e output useful for debugging
         */
        public void printParameters()
        {
            Console.WriteLine("------------ SpatialPooler Parameters ------------------");
            Console.WriteLine("numInputs                  = " + this.HtmConfig.NumInputs);
            Console.WriteLine("numColumns                 = " + NumColumns);
            Console.WriteLine("cellsPerColumn             = " + this.HtmConfig.CellsPerColumn);
            Console.WriteLine("columnDimensions           = " + this.HtmConfig.ColumnDimensions.ToString());
            Console.WriteLine("numActiveColumnsPerInhArea = " + this.HtmConfig.NumActiveColumnsPerInhArea);
            Console.WriteLine("potentialPct               = " + this.HtmConfig.PotentialPct);
            Console.WriteLine("potentialRadius            = " + this.HtmConfig.PotentialRadius);
            Console.WriteLine("globalInhibition           = " + this.HtmConfig.GlobalInhibition);
            Console.WriteLine("localAreaDensity           = " + this.HtmConfig.LocalAreaDensity);
            Console.WriteLine("inhibitionRadius           = " + this.HtmConfig.InhibitionRadius);
            Console.WriteLine("stimulusThreshold          = " + this.HtmConfig.StimulusThreshold);
            Console.WriteLine("synPermActiveInc           = " + this.HtmConfig.SynPermActiveInc);
            Console.WriteLine("synPermInactiveDec         = " + this.HtmConfig.SynPermInactiveDec);
            Console.WriteLine("synPermConnected           = " + this.HtmConfig.SynPermConnected);
            Console.WriteLine("minPctOverlapDutyCycle     = " + this.HtmConfig.MinPctOverlapDutyCycles);
            Console.WriteLine("minPctActiveDutyCycle      = " + this.HtmConfig.MinPctActiveDutyCycles);
            Console.WriteLine("dutyCyclePeriod            = " + this.HtmConfig.DutyCyclePeriod);
            Console.WriteLine("maxBoost                   = " + this.HtmConfig.MaxBoost);
            Console.WriteLine("version                    = " + getVersion());

            Console.WriteLine("\n------------ TemporalMemory Parameters ------------------");
            Console.WriteLine("activationThreshold        = " + this.HtmConfig.ActivationThreshold);
            Console.WriteLine("learningRadius             = " + this.HtmConfig.LearningRadius);
            Console.WriteLine("minThreshold               = " + this.HtmConfig.MinThreshold);
            Console.WriteLine("maxNewSynapseCount         = " + HtmConfig.MaxNewSynapseCount);
            Console.WriteLine("maxSynapsesPerSegment      = " + this.HtmConfig.MaxSynapsesPerSegment);
            Console.WriteLine("maxSegmentsPerCell         = " + this.HtmConfig.MaxSegmentsPerCell);
            Console.WriteLine("initialPermanence          = " + this.HtmConfig.InitialPermanence);
            Console.WriteLine("connectedPermanence        = " + this.HtmConfig.ConnectedPermanence);
            Console.WriteLine("permanenceIncrement        = " + this.HtmConfig.PermanenceIncrement);
            Console.WriteLine("permanenceDecrement        = " + this.HtmConfig.PermanenceDecrement);
            Console.WriteLine("predictedSegmentDecrement  = " + this.HtmConfig.PredictedSegmentDecrement);
        }

        ///**
        // * High verbose output useful for debugging
        // */
        //public String getPrintString()
        //{
        //    StringWriter pw;
        //    //PrintWriter pw = new PrintWriter(sw = new StringWriter());

        //    pw.println("---------------------- General -------------------------");
        //    pw.println("columnDimensions           = " + Arrays.toString(getColumnDimensions()));
        //    pw.println("inputDimensions            = " + Arrays.toString(getInputDimensions()));
        //    pw.println("cellsPerColumn             = " + getCellsPerColumn());

        //    pw.println("random                     = " + getRandom());
        //    pw.println("seed                       = " + getSeed());

        //    pw.println("\n------------ SpatialPooler Parameters ------------------");
        //    pw.println("numInputs                  = " + getNumInputs());
        //    pw.println("numColumns                 = " + getNumColumns);
        //    pw.println("numActiveColumnsPerInhArea = " + getNumActiveColumnsPerInhArea());
        //    pw.println("potentialPct               = " + getPotentialPct());
        //    pw.println("potentialRadius            = " + getPotentialRadius());
        //    pw.println("globalInhibition           = " + getGlobalInhibition());
        //    pw.println("localAreaDensity           = " + getLocalAreaDensity());
        //    pw.println("inhibitionRadius           = " + getInhibitionRadius());
        //    pw.println("stimulusThreshold          = " + getStimulusThreshold());
        //    pw.println("synPermActiveInc           = " + getSynPermActiveInc());
        //    pw.println("synPermInactiveDec         = " + getSynPermInactiveDec());
        //    pw.println("synPermConnected           = " + getSynPermConnected());
        //    pw.println("synPermBelowStimulusInc    = " + getSynPermBelowStimulusInc());
        //    pw.println("synPermTrimThreshold       = " + getSynPermTrimThreshold());
        //    pw.println("minPctOverlapDutyCycles    = " + getMinPctOverlapDutyCycles());
        //    pw.println("minPctActiveDutyCycles     = " + getMinPctActiveDutyCycles());
        //    pw.println("dutyCyclePeriod            = " + getDutyCyclePeriod());
        //    pw.println("wrapAround                 = " + isWrapAround());
        //    pw.println("maxBoost                   = " + getMaxBoost());
        //    pw.println("version                    = " + getVersion());

        //    pw.println("\n------------ TemporalMemory Parameters ------------------");
        //    pw.println("activationThreshold        = " + getActivationThreshold());
        //    pw.println("learningRadius             = " + getLearningRadius());
        //    pw.println("minThreshold               = " + getMinThreshold());
        //    pw.println("maxNewSynapseCount         = " + getMaxNewSynapseCount());
        //    pw.println("maxSynapsesPerSegment      = " + getMaxSynapsesPerSegment());
        //    pw.println("maxSegmentsPerCell         = " + getMaxSegmentsPerCell());
        //    pw.println("initialPermanence          = " + getInitialPermanence());
        //    pw.println("connectedPermanence        = " + getConnectedPermanence());
        //    pw.println("permanenceIncrement        = " + getPermanenceIncrement());
        //    pw.println("permanenceDecrement        = " + getPermanenceDecrement());
        //    pw.println("predictedSegmentDecrement  = " + getPredictedSegmentDecrement());

        //    return sw.toString();
        //}

        /**
         * Returns a 2 Dimensional array of 1's and 0's indicating
         * which of the column's pool members are above the connected
         * threshold, and therefore considered "connected"
         * @return
         */
        public int[][] getConnecteds()
        {
            int[][] retVal = new int[NumColumns][];
            for (int i = 0; i < NumColumns; i++)
            {
                //Pool pool = getPotentialPools().get(i);
                Pool pool = getColumn(i).ProximalDendrite.RFPool;
                int[] indexes = pool.getDenseConnected();
                retVal[i] = indexes;
            }

            return retVal;
        }

        /**
         * Returns a 2 Dimensional array of 1's and 0's indicating
         * which input bits belong to which column's pool.
         * @return
         */
        public int[][] getPotentials()
        {
            int[][] retVal = new int[NumColumns][];
            for (int i = 0; i < NumColumns; i++)
            {
                //Pool pool = getPotentialPools().get(i);
                Pool pool = getColumn(i).ProximalDendrite.RFPool;
                int[] indexes = pool.getDensePotential(this);
                retVal[i] = indexes;
            }

            return retVal;
        }

        /**
         * Returns a 2 Dimensional array of the permanences for SP
         * proximal dendrite column pooled connections.
         * @return
         */
        public double[][] getPermanences()
        {
            double[][] retVal = new double[NumColumns][];
            for (int i = 0; i < NumColumns; i++)
            {
                //Pool pool = getPotentialPools().get(i);
                Pool pool = getColumn(i).ProximalDendrite.RFPool;
                double[] perm = pool.getDensePermanences(this.HtmConfig.NumInputs);
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
            result = prime * result + ((activeCells == null) ? 0 : activeCells.GetHashCode());
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
            result = prime * result + spIterationLearnNum;
            result = prime * result + spIterationNum;
            //result = prime * result + (new Long(tmIteration)).intValue();
            result = prime * result + (int)tmIteration;
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
            result = prime * result + numColumns;
            result = prime * result + this.HtmConfig.NumInputs;
            temp = NumSynapses;
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
            result = prime * result + ((predictiveCells == null) ? 0 : predictiveCells.GetHashCode());
            result = prime * result + ((random == null) ? 0 : random.GetHashCode());
            result = prime * result + ((receptorSynapses == null) ? 0 : receptorSynapses.GetHashCode());
            result = prime * result + this.HtmConfig.RandomGenSeed;
            result = prime * result + ((distalSegments == null) ? 0 : distalSegments.GetHashCode());
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
            result = prime * result + ((distalSynapses == null) ? 0 : distalSynapses.GetHashCode());
            result = prime * result + tieBreaker.GetHashCode();
            result = prime * result + this.HtmConfig.UpdatePeriod;
            temp = BitConverter.DoubleToInt64Bits(version);
            result = prime * result + (int)(temp ^ (temp >> 32));
            result = prime * result + ((winnerCells == null) ? 0 : winnerCells.GetHashCode());
            return result;
        }

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
            if (activeCells == null)
            {
                if (other.activeCells != null)
                    return false;
            }
            else if (!activeCells.Equals(other.activeCells))
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
            if (spIterationLearnNum != other.spIterationLearnNum)
                return false;
            if (spIterationNum != other.spIterationNum)
                return false;
            if (tmIteration != other.tmIteration)
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
            if (numColumns != other.numColumns)
                return false;
            if (this.HtmConfig.NumInputs != other.HtmConfig.NumInputs)
                return false;
            if (NumSynapses != other.NumSynapses)
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
            if (predictiveCells == null)
            {
                if (other.predictiveCells != null)
                    return false;
            }
            else if (!getPredictiveCells().Equals(other.getPredictiveCells()))
                return false;
            if (receptorSynapses == null)
            {
                if (other.receptorSynapses != null)
                    return false;
            }
            else if (!receptorSynapses.ToString().Equals(other.receptorSynapses.ToString()))
                return false;
            if (this.HtmConfig.RandomGenSeed != other.HtmConfig.RandomGenSeed)
                return false;
            if (distalSegments == null)
            {
                if (other.distalSegments != null)
                    return false;
            }
            else if (!distalSegments.Equals(other.distalSegments))
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
            if (distalSynapses == null)
            {
                if (other.distalSynapses != null)
                    return false;
            }
            else if (!distalSynapses.Equals(other.distalSynapses))
                return false;
            if (!Array.Equals(tieBreaker, other.tieBreaker))
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
        #endregion
    }
}
