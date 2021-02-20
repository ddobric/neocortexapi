// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// HTM configuration.
    /// Also sent from Akka-client to Akka Actor.
    /// </summary>
    public class HtmConfig
    {
        /// <summary>
        /// Default constructor with the default set of parameters.
        /// </summary>
        /// <param name="inputDims"></param>
        /// <param name="columnDims"></param>
        public HtmConfig(int[] inputDims,  int[] columnDims)
        {
            SetHtmConfigDefaultParameters(inputDims, columnDims);
        }

        /// <summary>
        /// Not used!
        /// </summary>
        public class TemporalMemoryConfig
        {

        }

        /// <summary>
        /// Not used
        /// </summary>
        public class SpatialPoolerConfig
        {

        }

        private double synPermActiveInc;
        private double synPermConnected;
        private AbstractSparseMatrix<Column> memory;
        private ISparseMatrix<int> inputMatrix;

        public TemporalMemoryConfig TemporalMemory { get; set; } = new TemporalMemoryConfig();

        public SpatialPoolerConfig SpatialPooler { get; set; } = new SpatialPoolerConfig();

        #region Spatial Pooler Variables
        /// <summary>
        /// The inhibition radius determines the size of a column's local neighborhood. of a column. A cortical column must overcome the overlap
        /// score of columns in its neighborhood in order to become actives. This radius is updated every learning round. It grows and shrinks with the
        /// average number of connected synapses per column.
        /// </summary>
        public int InhibitionRadius { get; set; } = 0;

        /// <summary>
        /// Manages input neighborhood transformations.
        /// </summary>
        public Topology InputTopology { get; set; }

        /// <summary>
        /// Manages column neighborhood transformations.
        /// </summary>
        public Topology ColumnTopology { get; set; }

        /// <summary>
        /// product of input dimensions.
        /// </summary>
        public int NumInputs { get; set; } = 1;

        /// <summary>
        /// Initialized after SP.Init.
        /// </summary>
        public int NumColumns { get; set; }

        /// <summary>
        /// This parameter determines the extent of the input that each column can potentially be connected to.
        /// This can be thought of as the input bits that are visible to each column, or a 'receptiveField' of
        /// the field of vision. A large enough value will result in 'global coverage', meaning that each column
        /// can potentially be connected to every input bit. This parameter defines a square (or hyper square) area: a
        /// column will have a max square potential pool with sides of length 2 * <see cref="PotentialRadius"/> + 1.<br/>
        /// 
        /// <b>WARNING:</b> <see cref="PotentialRadius"/> <b><i>must</i></b> be set to the inputWidth if using 
        /// <see cref="GlobalInhibition"/> and if not using the Network API (which sets this automatically).
        /// </summary>
        public int PotentialRadius { get; set; }

        /// <summary>
        /// The percent of the inputs, within a column's potential radius, that a column can be connected to.
        /// If set to 1, the column will be connected to every input within its potential radius. This parameter is
        /// used to give each column a unique potential pool when a large potentialRadius causes overlap between the
        /// columns. At initialization time we choose ((2*<see cref="PotentialRadius"/> + 1)^(# <see cref="InputDimensions"/>) *
        /// <see cref="PotentialPct"/>) input bits to comprise the column's potential pool.
        /// </summary>
        public double PotentialPct { get; set; }

        /// <summary>
        /// Minimum number of connected synapses to make column active. Specified as a percent of a fully grown synapse.
        /// </summary>
        public double StimulusThreshold { get; set; }

        /// <summary>
        /// Synapses of weak mini-columns will be stimulated by the boosting mechanism. The stimulation is done by adding of this increment value to the current permanence value of the synapse.
        /// </summary>
        public double SynPermBelowStimulusInc { get; set; }

        /// <summary>
        /// The amount by which an inactive synapse is decremented in each round. Specified as a percent of a fully grown synapse.
        /// </summary>
        public double SynPermInactiveDec { get; set; }

        /// <summary>
        /// The amount by which an active synapse is incremented in each round. Specified as a percent of a fully grown synapse.
        /// </summary>
        public double SynPermActiveInc { get => synPermActiveInc; set { this.synPermActiveInc = value; SynPermTrimThreshold = value / 2.0; } }

        /// <summary>
        /// The default connected threshold. Any synapse whose permanence value is above the connected threshold is
        /// a "connected synapse", meaning it can contribute to the cell's firing.
        /// </summary>
        public double SynPermConnected { get => synPermConnected; set { synPermConnected = value; SynPermBelowStimulusInc = value / 10.0; } }

        /// <summary>
        /// Specifies whether neighborhoods wider than the borders wrap around to the other side.
        /// </summary>
        public bool WrapAround { get; set; } = true;

        /// <summary>
        /// Enforses using of global inhibition process.
        /// </summary>
        public bool GlobalInhibition { get; set; } = false;

        /// <summary>
        /// The desired density of active columns within a local inhibition area (the size of which is set by the
        /// internally calculated <see cref="InhibitionRadius"/>, which is in turn determined from the average size of the
        /// connected potential pools of all columns). The inhibition logic will insure that at most N columns
        /// remain ON within a local inhibition area, where N = <see cref="LocalAreaDensity"/> * (total number of columns in
        /// inhibition area).
        /// </summary>
        public double LocalAreaDensity { get; set; } = -1.0;

        public double SynPermTrimThreshold { get; set; }

        /// <summary>
        /// Maximum <see cref="Synapse"/> permanence.
        /// </summary>
        public double SynPermMax { get; set; } = 1.0;

        /// <summary>
        /// Minimum <see cref="Synapse"/> permanence.
        /// </summary>
        public double SynPermMin { get; set; }

        /// <summary>
        /// Percent of initially connected synapses. Typically 50%.
        /// </summary>
        public double InitialSynapseConnsPct { get; set; } = 0.5;

        /// <summary>
        /// Input column mapping matrix.
        /// </summary>
        public ISparseMatrix<int> InputMatrix { get => inputMatrix; set { inputMatrix = value; InputModuleTopology = value?.ModuleTopology; } }

        /// <summary>
        /// The configured number of active columns per inhibition area.<br/>
        /// An alternate way to control the density of the active columns. If numActivePerInhArea is specified then
        /// localAreaDensity must be less than 0, and vice versa. When using numActivePerInhArea, the inhibition logic
        /// will insure that at most <see cref="NumActiveColumnsPerInhArea"/> columns remain ON within a local inhibition area (the
        /// size of which is set by the internally calculated inhibitionRadius, which is in turn determined from
        /// the average size of the connected receptive fields of all columns). When using this method, as columns
        /// learn and grow their effective receptive fields, the inhibitionRadius will grow, and hence the net density
        /// of the active columns will *decrease*. This is in contrast to the localAreaDensity method, which keeps
        /// the density of active columns the same regardless of the size of their receptive fields.
        /// </summary>
        public double NumActiveColumnsPerInhArea { get; set; }

        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should have at least
        /// stimulusThreshold active inputs. Periodically, each column looks at the overlap duty cycle of
        /// all other columns within its inhibition radius and sets its own internal minimal acceptable duty cycle
        /// to: minPctDutyCycleBeforeInh * max(other columns' duty cycles).
        /// On each iteration, any column whose overlap duty cycle falls below this computed value will  get
        /// all of its permanence values boosted up by <see cref="SynPermActiveInc"/>. Raising all permanences in response
        /// to a sub-par duty cycle before  inhibition allows a cell to search for new inputs when either its
        /// previously learned inputs are no longer ever active, or when the vast majority of them have been
        /// "hijacked" by other columns.
        /// </summary>
        public double MinPctOverlapDutyCycles { get; set; } = 0.001;

        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on  how often a column should be activate.
        /// Periodically, each column looks at the activity duty  cycle of all other columns within its inhibition
        /// radius and sets its own internal minimal acceptable  duty cycle to:<br/>
        /// minPctDutyCycleAfterInh * max(other columns' duty cycles).<br/>
        /// On each iteration, any column whose duty cycle after inhibition falls below this computed value will get
        /// its internal boost factor increased.
        /// </summary>
        public double MinPctActiveDutyCycles { get; set; } = 0.001;

        /// <summary>
        /// Amount by which active permanences of synapses of previously predicted but inactive segments are decremented.
        /// </summary>
        public double PredictedSegmentDecrement { get; set; } = 0.0;

        /// <summary>
        /// he period used to calculate duty cycles. Higher values make it take longer to respond to changes in
        /// boost or synPerConnectedCell. Shorter values make it more unstable and likely to oscillate.
        /// </summary>
        public int DutyCyclePeriod { get; set; } = 1000;

        /// <summary>
        /// The maximum overlap boost factor. Each column's overlap gets multiplied by a boost factor
        /// before it gets considered for inhibition. The actual boost factor for a column is number
        /// between 1.0 and maxBoost. A boost factor of 1.0 is used if the duty cycle is &gt;= minOverlapDutyCycle,
        /// maxBoost is used if the duty cycle is 0, and any duty cycle in between is linearly extrapolated from these
        /// 2 end points.
        /// </summary>
        public double MaxBoost { get; set; } = 10.0;

        /// <summary>
        /// Controls if bumping-up of weak columns shell be done.
        /// </summary>
        public bool IsBumpUpWeakColumnsDisabled { get; set; } = false;

        /// <summary>
        /// Period count which is the number of cycles between updates of inhibition radius and min. duty cycles.
        /// <see cref="SpatialPooler.compute"/>
        /// </summary>
        public int UpdatePeriod { get; set; } = 50;

        /// <summary>
        /// Overlap duty cycles.
        /// </summary>
        public double[] OverlapDutyCycles { get; set; }

        /// <summary>
        /// The dense (size=numColumns) array of duty cycle stats.
        /// </summary>
        public double[] ActiveDutyCycles { get; set; }

        /// <summary>
        /// TODO property documentation
        /// </summary>
        public double[] MinOverlapDutyCycles { get; set; }

        /// <summary>
        /// TODO property documentation
        /// </summary>
        public double[] MinActiveDutyCycles { get; set; }

        #endregion

        #region Temporal Memory Variables
        /// <summary>
        /// Number of <see cref="Column"/>
        /// </summary>
        public int[] ColumnDimensions { get; set; } = new int[] { 2048 };

        /// <summary>
        /// Nunmber of <see cref="Cell"/>s per <see cref="Column"/>
        /// </summary>
        public int CellsPerColumn { get; set; } = 32;

        /// <summary>
        /// A list representing the dimensions of the input vector. Format is [height, width, depth, ...], where
        /// each value represents the size of the dimension. For a topology of one dimension with 100 inputs use 100, or
        /// [100]. For a two dimensional topology of 10x5 use [10,5].
        /// </summary>
        public int[] InputDimensions { get; set; } = new int[] { 100 };

        /// <summary>
        /// The maximum number of synapses added to a segment during learning.
        /// </summary>
        public int MaxNewSynapseCount { get; set; }

        /// <summary>
        /// The maximum number of segments (distal dendrites) allowed on a cell.
        /// </summary>
        public int MaxSegmentsPerCell { get; set; }

        /// <summary>
        /// The maximum number of synapses allowed on a given segment (distal dendrite).
        /// </summary>
        public int MaxSynapsesPerSegment { get; set; }

        /// <summary>
        /// Amount by which permanences of synapses are incremented during learning.
        /// </summary>
        public double PermanenceIncrement { get; set; }

        /// <summary>
        /// Amount by which permanences of synapses are decremented during learning.
        /// </summary>
        public double PermanenceDecrement { get; set; }

        /// <summary>
        /// TODO property documentation
        /// </summary>
        public HtmModuleTopology ColumnModuleTopology { get; set; }

        /// <summary>
        /// TODO property documentation
        /// </summary>
        public HtmModuleTopology InputModuleTopology { get; set; }

        /// <summary>
        /// The main data structure containing columns, cells, and synapses.
        /// </summary>
        public AbstractSparseMatrix<Column> Memory { get => memory; set { memory = value; ColumnModuleTopology = value?.ModuleTopology; } }

        /// <summary>
        /// Activation threshold. If the number of active connected synapses on a segment is at least this threshold, the segment is said to be active.
        /// </summary>
        public int ActivationThreshold { get; set; } = 13;

        /// <summary>
        /// Radius around cell from which it can
        /// sample to form distal dendrite connections.
        /// </summary>
        public int LearningRadius { get; set; } = 2048;

        /// <summary>
        /// If the number of synapses active on a segment is at least this threshold, it is selected as the best matching
        /// cell in a bursting column.
        /// </summary>
        public int MinThreshold { get; set; } = 10;

        /// <summary>
        /// Initial permanence of a new synapse
        /// </summary>
        public double InitialPermanence { get; set; } = 0.21;

        /// <summary>
        /// If the permanence value for a synapse is greater than this value, it is said to be connected.
        /// </summary>
        public double ConnectedPermanence { get; set; } = 0.5;

        //public bool Learn { get; set; } = true;
        #endregion

        //public bool IsColumnMajor { get; set; } = false;

        /// <summary>
        /// Use -1 if real random generator has to be used with timestamp seed.
        /// </summary>
        public int RandomGenSeed { get; set; } = 42;

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The random number generator
        /// </summary>
        public Random Random { get; set; }

        /// <summary>
        /// Set default value for parameters of <see cref="HtmConfig"/>
        /// </summary>
        public void SetHtmConfigDefaultParameters(int[] inputDims, int[] columnDims)
        {
            // Temporal Memory parameters
            this.ColumnDimensions = columnDims;
            this.InputDimensions = inputDims;

            this.CellsPerColumn = 32;
            this.ActivationThreshold = 10;
            this.LearningRadius = 10;
            this.MinThreshold = 9;
            this.MaxNewSynapseCount = 20;
            this.MaxSynapsesPerSegment = 225;
            this.MaxSegmentsPerCell = 225;
            this.InitialPermanence = 0.21;
            this.ConnectedPermanence = 0.5;
            this.PermanenceIncrement = 0.10;
            this.PermanenceDecrement = 0.10;
            this.PredictedSegmentDecrement = 0.1;
            
            // Spatial Pooler parameters
          
            this.PotentialRadius = 15;
            this.PotentialPct = 0.75;
            this.GlobalInhibition = true;
            this.InhibitionRadius = 15;
            this.LocalAreaDensity = -1.0;
            this.NumActiveColumnsPerInhArea = 0.02 * 2048;
            this.StimulusThreshold = 5.0;
            this.SynPermInactiveDec = 0.008;
            this.SynPermActiveInc = 0.05;
            this.SynPermConnected = 0.1;
            this.SynPermBelowStimulusInc = 0.01;
            this.SynPermTrimThreshold = 0.05;
            this.MinPctOverlapDutyCycles = 0.001;
            this.MinPctActiveDutyCycles = 0.001;
            this.DutyCyclePeriod = 1000;
            this.MaxBoost = 10.0;
            this.WrapAround = true;
            this.Random = new ThreadSafeRandom(42);
        }

        public void ClearModuleTopology()
        {
            this.Memory = null;
            this.InputMatrix = null;
        }

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(HtmConfig), writer);

            ser.SerializeValue(this.synPermActiveInc, writer);
            ser.SerializeValue(this.SynPermConnected, writer);





            //Spatial Pooler Variables
            ser.SerializeValue(this.InhibitionRadius, writer);
            ser.SerializeValue(this.NumInputs, writer);
            ser.SerializeValue(this.NumColumns, writer);
            ser.SerializeValue(this.PotentialRadius, writer);
            ser.SerializeValue(this.PotentialPct, writer);
            ser.SerializeValue(this.StimulusThreshold, writer);
            ser.SerializeValue(this.SynPermBelowStimulusInc,writer);
            ser.SerializeValue(this.SynPermInactiveDec, writer);
            ser.SerializeValue(this.SynPermActiveInc, writer);
            ser.SerializeValue(this.SynPermConnected, writer);
            ser.SerializeValue(this.WrapAround, writer);
            ser.SerializeValue(this.GlobalInhibition, writer);
            ser.SerializeValue(this.LocalAreaDensity, writer);
            ser.SerializeValue(this.SynPermTrimThreshold, writer);
            ser.SerializeValue(this.SynPermMax, writer);
            ser.SerializeValue(this.SynPermMin, writer);
            ser.SerializeValue(this.InitialSynapseConnsPct, writer);
            ser.SerializeValue(this.NumActiveColumnsPerInhArea, writer);
            ser.SerializeValue(this.MinPctOverlapDutyCycles, writer);
            ser.SerializeValue(this.MinPctActiveDutyCycles, writer);
            ser.SerializeValue(this.PredictedSegmentDecrement, writer);
            ser.SerializeValue(this.DutyCyclePeriod, writer);
            ser.SerializeValue(this.MaxBoost, writer);
            ser.SerializeValue(this.IsBumpUpWeakColumnsDisabled, writer);
            ser.SerializeValue(this.UpdatePeriod, writer);
            ser.SerializeValue(this.OverlapDutyCycles, writer);
            ser.SerializeValue(this.ActiveDutyCycles, writer);
            ser.SerializeValue(this.MinOverlapDutyCycles, writer);
            ser.SerializeValue(this.MinActiveDutyCycles, writer);

            //TemporalMemoryVariables
            ser.SerializeValue(this.ColumnDimensions, writer);
            ser.SerializeValue(this.CellsPerColumn, writer);
            ser.SerializeValue(this.InputDimensions, writer);
            ser.SerializeValue(this.MaxNewSynapseCount, writer);
            ser.SerializeValue(this.MaxSegmentsPerCell, writer);
            ser.SerializeValue(this.MaxSynapsesPerSegment, writer);
            ser.SerializeValue(this.PermanenceIncrement, writer);
            ser.SerializeValue(this.PermanenceDecrement, writer);
            ser.SerializeValue(this.ActivationThreshold,writer);
            ser.SerializeValue(this.LearningRadius, writer);
            ser.SerializeValue(this.MinThreshold, writer);
            ser.SerializeValue(this.InitialPermanence, writer);
            ser.SerializeValue(this.ConnectedPermanence, writer);
            ser.SerializeValue(this.RandomGenSeed, writer);
            ser.SerializeValue(this.Name, writer);

            ser.SerializeEnd(nameof(HtmConfig), writer);
        }
        #endregion
    }

    public class test
    {
        public test()
        {
            HtmConfig htm = new HtmConfig(new int[] { 100}, new int[] { 1024 });

            Connections c = new Connections(htm);

            HtmConfig.TemporalMemoryConfig x = new HtmConfig.TemporalMemoryConfig();

        }
    }
}
