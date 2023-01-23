// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// HTM configuration.
    /// Also sent from Akka-client to Akka Actor.
    /// </summary>
    public class HtmConfig : ISerializable
    {
        public static readonly double EPSILON = 0.00001;

        /// <summary>
        /// Default constructor with the default set of parameters.
        /// </summary>
        /// <param name="inputDims"></param>
        /// <param name="columnDims"></param>
        public HtmConfig(int[] inputDims, int[] columnDims)
        {
            SetHtmConfigDefaultParameters(inputDims, columnDims);
        }
        public HtmConfig()
        {

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
        
        private ISparseMatrix<int> inputMatrix;

        public TemporalMemoryConfig TemporalMemory { get; set; } = new TemporalMemoryConfig();

        public SpatialPoolerConfig SpatialPooler { get; set; } = new SpatialPoolerConfig();

        #region Spatial Pooler Variables


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
        /// </summary>
        /// <remarks>It must be set to the inputWidth if using <see cref="GlobalInhibition"/>.</remarks>
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
        /// Minimum number of connected synapses (mini-columns with active synapses) to the input to declare the mini-column active. 
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
        /// Connected permanence Threshold. Any synapse whose permanence value is above the connected prtmanence threshold value is
        /// a "connected synapse", meaning it can contribute to the cell's firing.
        /// </summary>
        public double SynPermConnected { get => synPermConnected; set { synPermConnected = value; SynPermBelowStimulusInc = value / 10.0; } }

        /// <summary>
        /// If the permanence value for a synapse is greater than this value, it is said to be connected = the potential synapse.
        /// Synapses that exceeds this value are used in computation of active segments.
        /// </summary>
        [Obsolete("Use SynPermConnected instead.")]
        public double ConnectedPermanence { get; set; } = 0.5;

        /// <summary>
        /// Specifies whether neighborhoods wider than the borders wrap around to the other side.
        /// </summary>
        public bool WrapAround { get; set; } = true;

        /// <summary>
        /// This value is used by SP. When some permanence is under this value, it is set on zero.
        /// In this case the synapse remains the potential one and still can participate in learning.
        /// By following structural plasticity principal the synapse would become disconnected from the mini-column.
        /// </summary>
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
        /// Enforses using of global inhibition process.
        /// </summary>
        public bool GlobalInhibition { get; set; } = false;

        /// <summary>
        /// The configured number of active columns per inhibition area.<br/>
        /// An alternate way to control the density of the active columns. If this value is specified then
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
        /// The desired density of active columns within a local inhibition area (the size of which is set by the
        /// internally calculated <see cref="InhibitionRadius"/>, which is in turn determined from the average size of the
        /// connected potential pools of all columns). The inhibition logic will insure that at most N columns
        /// remain ON within a local inhibition area, where N = <see cref="LocalAreaDensity"/> * (total number of columns in
        /// inhibition area).
        /// Higher values increase similarity of inputs.
        /// </summary>
        public double LocalAreaDensity { get; set; } = -1.0;

        /// <summary>
        /// Maximum allowed inhibtion density.
        /// </summary>
        public double MaxInibitionDensity { get; set; } = 0.5;

        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should be activated, 
        /// when learning spatial patterns. Periodically, each column looks at the overlap duty cycle of
        /// all other columns within its inhibition radius and sets its own internal minimal acceptable duty cycle
        /// to: minPctDutyCycleBeforeInh * max(other columns' duty cycles).
        /// On each iteration, any column whose overlap duty cycle falls below this computed value will  get
        /// all of its permanence values boosted up by <see cref="SynPermActiveInc"/>. 
        /// Raising all permanences in response
        /// to a sub-par duty cycle before  inhibition allows a cell to search for new inputs when either its
        /// previously learned inputs are no longer ever active, or when the vast majority of them have been
        /// "hijacked" by other columns.
        /// </summary>
        public double MinPctOverlapDutyCycles { get; set; } = 0.001;

        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should be activated.
        /// Periodically, each column looks at the activity duty cycle of all other columns within its inhibition
        /// radius and sets its own internal minimal acceptable duty cycle to:<br/>
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
        /// The period used to calculate duty cycles. Higher values make it take longer to respond to changes in
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
        //public bool IsBumpUpWeakColumnsDisabled { get; set; } = false;

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
        /// TODO: Should be removed to Connections class.
        /// The dense (size=numColumns) array of duty cycle stats.
        /// </summary>
        public double[] ActiveDutyCycles { get; set; }

        /// <summary>
        /// TODO property documentation
        /// TODO: Should be removed to Connections class.
        /// </summary>
        public double[] MinOverlapDutyCycles { get; set; }

        /// <summary>
        /// TODO property documentation
        /// TODO: Should be removed to Connections class.
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
        /// The topology of the input.
        /// </summary>
        public HtmModuleTopology InputModuleTopology { get; set; }

        /// <summary>
        /// Activation threshold used in sequence learning. If the number of active connected synapses on a distal segment is at least this threshold, the segment is declared as active one.
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
            //this.InhibitionRadius = 15;
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
            this.InputMatrix = null;
        }
        public bool Equals(HtmConfig obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            //if (memory == null)
            //{
            //    if (obj.memory != null)
            //        return false;
            //}
            //else if (!memory.Equals(obj.memory))
            //    return false;
            if (inputMatrix == null)
            {
                if (obj.inputMatrix != null)
                    return false;
            }
            else if (!inputMatrix.Equals(obj.inputMatrix))
                return false;
            if (InputTopology == null)
            {
                if (obj.InputTopology != null)
                    return false;
            }
            else if (!InputTopology.Equals(obj.InputTopology))
                return false;
            if (ColumnTopology == null)
            {
                if (obj.ColumnTopology != null)
                    return false;
            }
            else if (!ColumnTopology.Equals(obj.ColumnTopology))
                return false;
            if (InputMatrix == null)
            {
                if (obj.InputMatrix != null)
                    return false;
            }
            else if (!InputMatrix.Equals(obj.InputMatrix))
                return false;
            if (ColumnModuleTopology == null)
            {
                if (obj.ColumnModuleTopology != null)
                    return false;
            }
            else if (!ColumnModuleTopology.Equals(obj.ColumnModuleTopology))
                return false;
            if (InputModuleTopology == null)
            {
                if (obj.InputModuleTopology != null)
                    return false;
            }
            else if (!InputModuleTopology.Equals(obj.InputModuleTopology))
                return false;
            //if (Memory == null)
            //{
            //    if (obj.Memory != null)
            //        return false;
            //}
            //else if (!Memory.Equals(obj.Memory))
            //    return false;
            if (synPermActiveInc != obj.SynPermActiveInc)
                return false;
            if (synPermConnected != obj.synPermConnected)
                return false;
            //if (InhibitionRadius != obj.InhibitionRadius)
            //    return false;
            if (NumInputs != obj.NumInputs)
                return false;
            if (NumColumns != obj.NumColumns)
                return false;
            if (PotentialRadius != obj.PotentialRadius)
                return false;
            if (PotentialPct != obj.PotentialPct)
                return false;
            if (StimulusThreshold != obj.StimulusThreshold)
                return false;
            if (SynPermBelowStimulusInc != obj.SynPermBelowStimulusInc)
                return false;
            if (SynPermInactiveDec != obj.SynPermInactiveDec)
                return false;
            if (SynPermActiveInc != obj.SynPermActiveInc)
                return false;
            if (SynPermConnected != obj.SynPermConnected)
                return false;
            if (WrapAround != obj.WrapAround)
                return false;
            if (GlobalInhibition != obj.GlobalInhibition)
                return false;
            if (LocalAreaDensity != obj.LocalAreaDensity)
                return false;
            if (SynPermTrimThreshold != obj.SynPermTrimThreshold)
                return false;
            if (SynPermMax != obj.SynPermMax)
                return false;
            if (SynPermMin != obj.SynPermMin)
                return false;
            if (InitialSynapseConnsPct != obj.InitialSynapseConnsPct)
                return false;
            if (NumActiveColumnsPerInhArea != obj.NumActiveColumnsPerInhArea)
                return false;
            if (MinPctOverlapDutyCycles != obj.MinPctOverlapDutyCycles)
                return false;
            if (MinPctActiveDutyCycles != obj.MinPctActiveDutyCycles)
                return false;
            if (PredictedSegmentDecrement != obj.PredictedSegmentDecrement)
                return false;
            if (DutyCyclePeriod != obj.DutyCyclePeriod)
                return false;
            if (MaxBoost != obj.MaxBoost)
                return false;

            if (UpdatePeriod != obj.UpdatePeriod)
                return false;
            if (OverlapDutyCycles != null && this.OverlapDutyCycles != null)
            {

                if (!obj.OverlapDutyCycles.SequenceEqual(this.OverlapDutyCycles))
                    return false;
            }
            if (ActiveDutyCycles != null && ActiveDutyCycles != null)
            {

                if (!obj.ActiveDutyCycles.SequenceEqual(ActiveDutyCycles))
                    return false;
            }
            if (MinOverlapDutyCycles != null && MinOverlapDutyCycles != null)
            {

                if (!obj.MinOverlapDutyCycles.SequenceEqual(MinOverlapDutyCycles))
                    return false;
            }
            if (MinActiveDutyCycles != null && MinActiveDutyCycles != null)
            {

                if (!obj.MinActiveDutyCycles.SequenceEqual(MinActiveDutyCycles))
                    return false;
            }
            if (ColumnDimensions != null && ColumnDimensions != null)
            {

                if (!obj.ColumnDimensions.SequenceEqual(ColumnDimensions))
                    return false;
            }
            if (CellsPerColumn != obj.CellsPerColumn)
                return false;
            if (InputDimensions != null && InputDimensions != null)
            {

                if (!obj.InputDimensions.SequenceEqual(InputDimensions))
                    return false;
            }
            if (MaxNewSynapseCount != obj.MaxNewSynapseCount)
                return false;
            if (MaxSegmentsPerCell != obj.MaxSegmentsPerCell)
                return false;
            if (MaxSynapsesPerSegment != obj.MaxSynapsesPerSegment)
                return false;
            if (PermanenceIncrement != obj.PermanenceIncrement)
                return false;
            if (PermanenceDecrement != obj.PermanenceDecrement)
                return false;
            if (ActivationThreshold != obj.ActivationThreshold)
                return false;
            if (LearningRadius != obj.LearningRadius)
                return false;
            if (MinThreshold != obj.MinThreshold)
                return false;
            if (InitialPermanence != obj.InitialPermanence)
                return false;
            if (ConnectedPermanence != obj.ConnectedPermanence)
                return false;
            if (RandomGenSeed != obj.RandomGenSeed)
                return false;
            if (Name != obj.Name)
                return false;
            if (Random != null && obj.Random != null)
            {
                if (!obj.Random.Equals(Random))
                    return false;
            }

            return true;

        }
        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer ser = new HtmSerializer();

            ser.SerializeBegin(nameof(HtmConfig), writer);

            ser.SerializeValue(this.synPermActiveInc, writer);
            ser.SerializeValue(this.SynPermConnected, writer);
            //Spatial Pooler Variables
            //ser.SerializeValue(this.InhibitionRadius, writer);
            ser.SerializeValue(-1, writer);
            ser.SerializeValue(this.NumInputs, writer);
            ser.SerializeValue(this.NumColumns, writer);
            ser.SerializeValue(this.PotentialRadius, writer);
            ser.SerializeValue(this.PotentialPct, writer);
            ser.SerializeValue(this.StimulusThreshold, writer);
            ser.SerializeValue(this.SynPermBelowStimulusInc, writer);
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
            writer.WriteLine();
            ser.SerializeValue(this.MinPctOverlapDutyCycles, writer);
            ser.SerializeValue(this.MinPctActiveDutyCycles, writer);
            ser.SerializeValue(this.PredictedSegmentDecrement, writer);
            ser.SerializeValue(this.DutyCyclePeriod, writer);
            ser.SerializeValue(this.MaxBoost, writer);

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
            ser.SerializeValue(this.ActivationThreshold, writer);
            ser.SerializeValue(this.LearningRadius, writer);
            ser.SerializeValue(this.MinThreshold, writer);
            ser.SerializeValue(this.InitialPermanence, writer);
            ser.SerializeValue(this.ConnectedPermanence, writer);
            ser.SerializeValue(this.RandomGenSeed, writer);
            ser.SerializeValue(this.Name, writer);
            ser.SerializeValue(this.RandomGenSeed, writer);

            //if (this.memory != null)
            //{
            //    this.memory.Serialize(writer);
            //}
            if (this.inputMatrix != null)
            {
                this.inputMatrix.Serialize(writer);
            }
            if (this.InputTopology != null)
            {
                this.InputTopology.Serialize(writer);
            }
            if (this.ColumnTopology != null)
            {
                this.ColumnTopology.Serialize(writer);
            }
            if (this.ColumnModuleTopology != null)
            {
                this.ColumnModuleTopology.Serialize(writer);
            }

            if (this.InputModuleTopology != null)
            {
                this.InputModuleTopology.Serialize(writer);
            }
            if (this.InputMatrix != null)
            {
                this.InputMatrix.Serialize(writer);
            }
            //if (this.Memory != null)
            //{
            //    this.Memory.Serialize(writer);
            //}

            ser.SerializeEnd(nameof(HtmConfig), writer);
        }
        public static HtmConfig Deserialize(StreamReader sr)
        {
            HtmConfig htmConfig = new HtmConfig();
            HtmSerializer ser = new HtmSerializer();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(HtmConfig)))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(Topology)))
                {
                    htmConfig.InputTopology = Topology.Deserialize(sr);
                    htmConfig.ColumnTopology = Topology.Deserialize(sr);
                }
                else if (data == ser.ReadBegin(nameof(HtmModuleTopology)))
                {
                    htmConfig.ColumnModuleTopology = HtmModuleTopology.Deserialize(sr);
                    htmConfig.InputModuleTopology = HtmModuleTopology.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(HtmConfig)))
                {
                    break;
                }
                else
                {
                    int count = data.Count(ch => ch == HtmSerializer.ParameterDelimiter);
                    if (count == 20)
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    {
                                        htmConfig.synPermActiveInc = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 1:
                                    {
                                        htmConfig.SynPermConnected = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 2:
                                    {
                                        // htmConfig.InhibitionRadius = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 3:
                                    {
                                        htmConfig.NumInputs = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 4:
                                    {
                                        htmConfig.NumColumns = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 5:
                                    {
                                        htmConfig.PotentialRadius = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 6:
                                    {
                                        htmConfig.PotentialPct = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 7:
                                    {
                                        htmConfig.StimulusThreshold = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 8:
                                    {
                                        htmConfig.SynPermBelowStimulusInc = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 9:
                                    {
                                        htmConfig.SynPermInactiveDec = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }

                                case 10:
                                    {
                                        htmConfig.SynPermActiveInc = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 11:
                                    {
                                        htmConfig.SynPermConnected = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 12:
                                    {
                                        htmConfig.WrapAround = ser.ReadBoolValue(str[i]);
                                        break;
                                    }
                                case 13:
                                    {
                                        htmConfig.GlobalInhibition = ser.ReadBoolValue(str[i]);
                                        break;
                                    }
                                case 14:
                                    {
                                        htmConfig.LocalAreaDensity = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 15:
                                    {
                                        htmConfig.SynPermTrimThreshold = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 16:
                                    {
                                        htmConfig.SynPermMax = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 17:
                                    {
                                        htmConfig.SynPermMin = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 18:
                                    {
                                        htmConfig.InitialSynapseConnsPct = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 19:
                                    {
                                        htmConfig.NumActiveColumnsPerInhArea = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    {
                                        htmConfig.MinPctActiveDutyCycles = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 1:
                                    {
                                        htmConfig.MinPctActiveDutyCycles = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 2:
                                    {
                                        htmConfig.PredictedSegmentDecrement = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 3:
                                    {
                                        htmConfig.DutyCyclePeriod = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 4:
                                    {
                                        htmConfig.MaxBoost = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 5:
                                    {
                                        //htmConfig.IsBumpUpWeakColumnsDisabled = ser.ReadBoolValue(str[i]);
                                        break;
                                    }
                                case 6:
                                    {
                                        htmConfig.UpdatePeriod = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 7:
                                    {
                                        htmConfig.OverlapDutyCycles = ser.ReadArrayDouble(str[i]);
                                        break;
                                    }
                                case 8:
                                    {
                                        htmConfig.ActiveDutyCycles = ser.ReadArrayDouble(str[i]);
                                        break;
                                    }
                                case 9:
                                    {
                                        htmConfig.MinOverlapDutyCycles = ser.ReadArrayDouble(str[i]);
                                        break;
                                    }

                                case 10:
                                    {
                                        htmConfig.MinActiveDutyCycles = ser.ReadArrayDouble(str[i]);
                                        break;
                                    }

                                case 11:
                                    {
                                        htmConfig.ColumnDimensions = ser.ReadArrayInt(str[i]);
                                        break;
                                    }
                                case 12:
                                    {
                                        htmConfig.CellsPerColumn = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 13:
                                    {
                                        htmConfig.InputDimensions = ser.ReadArrayInt(str[i]);
                                        break;
                                    }
                                case 14:
                                    {
                                        htmConfig.MaxNewSynapseCount = ser.ReadIntValue(str[i]);
                                        break;
                                    }

                                case 15:
                                    {
                                        htmConfig.MaxSegmentsPerCell = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 16:
                                    {
                                        htmConfig.MaxSynapsesPerSegment = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 17:
                                    {
                                        htmConfig.PermanenceIncrement = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 18:
                                    {
                                        htmConfig.PermanenceDecrement = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 19:
                                    {
                                        htmConfig.ActivationThreshold = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 20:
                                    {
                                        htmConfig.LearningRadius = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 21:
                                    {
                                        htmConfig.MinThreshold = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 22:
                                    {
                                        htmConfig.InitialPermanence = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 23:
                                    {
                                        htmConfig.ConnectedPermanence = ser.ReadDoubleValue(str[i]);
                                        break;
                                    }

                                case 24:
                                    {
                                        htmConfig.RandomGenSeed = ser.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 25:
                                    {
                                        htmConfig.Name = ser.ReadStringValue(str[i]);
                                        break;
                                    }
                                case 26:
                                    {
                                        htmConfig.Random = ser.ReadRandomValue(str[i]);
                                        break;
                                    }
                                default:
                                    { break; }

                            }
                        }

                    }

                }
            }
            return htmConfig;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var excludeMembers = new List<string>
            {
                nameof(HtmConfig.inputMatrix),
                nameof(HtmConfig.synPermActiveInc),
                nameof(HtmConfig.synPermConnected)
            };
            HtmSerializer.SerializeObject(obj, name, sw, excludeMembers);
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            var htmConfig = HtmSerializer.DeserializeObject<HtmConfig>(sr, name);
            return htmConfig;
        }
        #endregion

    }

    public class test

    {
        public test()
        {
            HtmConfig htm = new HtmConfig(new int[] { 100 }, new int[] { 1024 });

            Connections c = new Connections(htm);

            HtmConfig.TemporalMemoryConfig x = new HtmConfig.TemporalMemoryConfig();

        }
    }
}


