// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Entities
{
    public static class KEY
    {
        /// <summary>
        /// If set on true, no bump-up of weak columns will be done.
        /// Note that bumping-up can cause Spatial Pooler to get instable.
        /// </summary>
        public const string IS_BUMPUP_WEAKCOLUMNS_DISABLED = "isBumpUpWeakColumnsDisabled";

        #region Universal Parameters

        /// <summary>
        /// Total number of columns.
        /// </summary>
        public const string COLUMN_DIMENSIONS = "columnDimensions";//, int[].class),

        /// <summary>
        /// Total number of cells per column
        /// </summary>
        public const string CELLS_PER_COLUMN = "cellsPerColumn";//, Integer.class, 1, null),

        /// <summary>
        /// Learning variable
        /// </summary>
        public const string LEARN = "learn";//, Boolean.class),
        /// <summary>
        /// Random Number Generator
        /// </summary>
        public const string RANDOM = "random";//, Random.class),
        /// <summary>
        /// Seed for random number generator
        /// </summary>
        public const string SEED = "seed";//, Integer.class),

        #endregion

        #region Temporal Memory Parameters

        /// <summary>
        /// If the number of active connected synapses on a segment is at least this threshold, the segment is said to be active.
        /// </summary>
        public const string ACTIVATION_THRESHOLD = "activationThreshold";//, Integer.class, 0, null),
        /// <summary>
        /// Radius around cell from which it can sample to form distal <see cref="DistalDendrite"/> connections.
        /// </summary>
        public const string LEARNING_RADIUS = "learningRadius";// Integer.class, 0, null),
        /// <summary>
        /// If the number of synapses active on a segment is at least this threshold, it is selected as the best matching cell in a bursting column.
        /// </summary>
        public const string MIN_THRESHOLD = "minThreshold";//, Integer.class, 0, null),
        /// <summary>
        /// The maximum number of synapses added to a segment during learning.
        /// </summary>
        public const string MAX_NEW_SYNAPSE_COUNT = "maxNewSynapseCount";//, Integer.class),
        /// <summary>
        /// The maximum number of synapses that can be added to a segment.
        /// </summary>
        public const string MAX_SYNAPSES_PER_SEGMENT = "maxSynapsesPerSegment";//, Integer.class),
        /// <summary>
        /// The maximum number of <see cref="Segment"/>s a <see cref="Cell"/> can have.
        /// </summary>
        public const string MAX_SEGMENTS_PER_CELL = "maxSegmentsPerCell";//, Integer.class),
        /// <summary>
        /// Initial permanence of a new synapse
        /// </summary>
        public const string INITIAL_PERMANENCE = "initialPermanence";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// If the permanence value for a synapse is greater than this value, it is said to be connected.
        /// </summary>
        public const string CONNECTED_PERMANENCE = "connectedPermanence";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// Amount by which permanence of synapses are incremented during learning.
        /// </summary>
        public const string PERMANENCE_INCREMENT = "permanenceIncrement";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// Amount by which permanences of synapses
        /// are decremented during learning.
        /// </summary>
        public const string PERMANENCE_DECREMENT = "permanenceDecrement";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// Amount by which active permanences of synapses of previously predicted but inactive segments are decremented.
        /// </summary>
        public const string PREDICTED_SEGMENT_DECREMENT = "predictedSegmentDecrement";//, Double.class, 0.0, 9.0),
        /** TODO: Remove this and add Logging (slf4j) */
        //TM_VERBOSITY("tmVerbosity", Integer.class, 0, 10),

        #endregion

        #region Spatial Pooler Parameters

        public const string INPUT_DIMENSIONS = "inputDimensions";//, int[].class),
        /// <summary>
        /// <b>WARNING:</b> potentialRadius <b>must</b> be set to the inputWidth if using <see cref="GLOBAL_INHIBITION"/> and if not using 
        /// the Network API (which sets this automatically) 
        /// </summary>
        public const string POTENTIAL_RADIUS = "potentialRadius";//, Integer.class),
        /// <summary>
        /// The percent of the inputs, within a column's potential radius, that a column can be connected to.  If set to 1, the column will be connected
        /// to every input within its potential radius. This parameter is used to give each column a unique potential pool when a large potentialRadius
        /// causes overlap between the columns. At initialization time we choose ((2*potentialRadius + 1)^(# inputDimensions) * potentialPct) input bits
        /// to comprise the column's potential pool.
        /// </summary>
        public const string POTENTIAL_PCT = "potentialPct";//, Double.class), //TODO add range here?

        /// <summary>
        /// If true, then during inhibition phase the winning columns are selected as the most active columns from the region as a whole. Otherwise, the
        /// winning columns are selected with respect to their local neighborhoods. Using global inhibition boosts performance x60.
        /// </summary>
        public const string GLOBAL_INHIBITION = "globalInhibition";//, Boolean.class),


        /// <summary>
        /// The inhibition radius determines the size of a column's local neighborhood. A cortical column must overcome the overlap score of columns in its
        /// neighborhood in order to become active. This radius is updated every learning round. It grows and shrinks with the average number of connected
        /// synapses per column.
        /// </summary>
        public const string INHIBITION_RADIUS = "inhibitionRadius";

        /// <summary>
        /// The desired density of active columns within a local inhibition area (the size of which is set by the internally calculated inhibitionRadius,
        /// which is in turn determined from the average size of the connected potential pools of all columns). The inhibition logic will insure that
        /// at most N columns remain ON within a local inhibition area, where N = localAreaDensity * (total number of columns in inhibition area).
        /// </summary>
        public const string LOCAL_AREA_DENSITY = "localAreaDensity";//, Double.class), //TODO add range here?
        /// <summary>
        /// An alternate way to control the density of the active columns. If numActiveColumnsPerInhArea is specified then localAreaDensity must be
        /// less than 0, and vice versa.  When using numActiveColumnsPerInhArea, the inhibition logic will insure that at most 'numActiveColumnsPerInhArea'
        /// columns remain ON within a local inhibition area (the size of which is set by the internally calculated inhibitionRadius, which is in turn
        /// determined from the average size of the connected receptive fields of all columns). When using this method, as columns learn and grow their
        /// effective receptive fields, the inhibitionRadius will grow, and hence the net density of the active columns will *decrease*. This is in contrast to
        /// the localAreaDensity method, which keeps the density of active columns the same regardless of the size of their receptive fields.
        /// </summary>
        public const string NUM_ACTIVE_COLUMNS_PER_INH_AREA = "numActiveColumnsPerInhArea";//, Double.class),//TODO add range here?

        /// <summary>
        /// This is a number specifying the minimum number of synapses that must be
        /// on in order for a columns to turn ON.The purpose of this is to prevent 
        /// noise input from activating columns.Specified as a percent of a fully grown synapse.
        /// </summary>
        public const string STIMULUS_THRESHOLD = "stimulusThreshold";//, Double.class), //TODO add range here?

        /// <summary>
        /// The amount by which an inactive synapse is decremented in each round. Specified as a percent of a fully grown synapse.
        /// </summary>
        public const string SYN_PERM_INACTIVE_DEC = "synPermInactiveDec";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// The amount by which an active synapse is incremented in each round. Specified as a percent of a fully grown synapse.
        /// </summary>
        public const string SYN_PERM_ACTIVE_INC = "synPermActiveInc";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// The default connected threshold. Any synapse whose permanence value is above the connected threshold is a "connected synapse", meaning it can
        /// contribute to the cell's firing.
        /// </summary>
        public const string SYN_PERM_CONNECTED = "synPermConnected";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// <b>WARNING:</b> This is a <i><b>derived</b></i> value, and is overwritten by the SpatialPooler algorithm's initialization.
        /// The permanence increment amount for columns that have not been recently active
        /// </summary>
        public const string SYN_PERM_BELOW_STIMULUS_INC = "synPermBelowStimulusInc";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// <b>WARNING:</b> This is a <i><b>derived</b></i> value, and is overwritten by the SpatialPooler algorithm's initialization.
        /// Values below this are "clipped" and zero'd out.
        /// </summary>
        public const string SYN_PERM_TRIM_THRESHOLD = "synPermTrimThreshold";//, Double.class, 0.0, 1.0),
        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should have at least stimulusThreshold active inputs. Periodically, each
        /// column looks at the overlap duty cycle of all other columns within its inhibition radius and sets its own internal minimal acceptable duty cycle
        /// to: minPctDutyCycleBeforeInh * max(other columns' duty cycles).  On each iteration, any column whose overlap duty cycle falls below this computed
        /// value will  get all of its permanence values boosted up by synPermActiveInc. Raising all permanences in response to a sub-par duty
        /// cycle before  inhibition allows a cell to search for new inputs when either its previously learned inputs are no longer ever active, or when
        /// the vast majority of them have been "hijacked" by other columns.
        /// </summary>
        public const string MIN_PCT_OVERLAP_DUTY_CYCLES = "minPctOverlapDutyCycles";//, Double.class),//TODO add range here?
        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should be activate.  Periodically, each column looks at the activity duty
        /// cycle of all other columns within its inhibition radius and sets its own internal minimal acceptable duty cycle to: minPctDutyCycleAfterInh *
        /// max(other columns' duty cycles).  On each iteration, any column whose duty cycle after inhibition falls below this computed value will get its
        /// internal boost factor increased.
        /// </summary>
        public const string MIN_PCT_ACTIVE_DUTY_CYCLES = "minPctActiveDutyCycles";//, Double.class),//TODO add range here?
        /// <summary>
        /// The period used to calculate duty cycles. Higher values make it take longer to respond to changes in boost or synPerConnectedCell. Shorter
        /// values make it more unstable and likely to oscillate.
        /// </summary>
        public const string DUTY_CYCLE_PERIOD = "dutyCyclePeriod";//, Integer.class),//TODO add range here?
        /// <summary>
        /// The maximum overlap boost factor. Each column's overlap gets multiplied by a boost factor before it gets considered for inhibition.  The actual
        /// boost factor for a column is number between 1.0 and maxBoost. A boost factor of 1.0 is used if the duty cycle is >= minOverlapDutyCycle,
        /// maxBoost is used if the duty cycle is 0, and any duty cycle in between is linearly extrapolated from these 2 endpoints.
        /// </summary>
        public const string MAX_BOOST = "maxBoost";//, Double.class), //TODO add range here?
        /// <summary>
        /// Determines if inputs at the beginning and end of an input dimension should be considered neighbors when mapping columns to inputs.
        /// </summary>
        public const string WRAP_AROUND = "wrapAround";//, Boolean.class),

        #region SpatialPooler / Network Parameter(s)

        /// <summary>
        /// Number of cycles to send through the SP before forwarding data to the rest of the network.
        /// </summary>
        public const string SP_PRIMER_DELAY = "sp_primer_delay";//, Integer.class),

        #endregion

        #endregion

        #region Encoder Parameters

        /// <summary>
        /// number of bits in the representation (must be &gt;= w)
        /// </summary>
        public const string N = "n";//, Integer.class),
        /// <summary>
        /// The number of bits that are set to encode a single value - the "width" of the output signal
        /// </summary>
        public const string W = "w";//, Integer.class),
        /// <summary>
        /// The minimum value of the input signal.
        /// </summary>
        public const string MIN_VAL = "minVal";//, Double.class),
        /// <summary>
        /// The maximum value of the input signal.
        /// </summary>
        public const string MAX_VAL = "maxVal";//, Double.class),
        /// <summary>
        /// inputs separated by more than, or equal to this distance will have non-overlapping representations
        /// </summary>
        public const string RADIUS = "radius";//, Double.class),
        /// <summary>
        /// inputs separated by more than, or equal to this distance will have different representations
        /// </summary>
        public const string RESOLUTION = "resolution";//, Double.class),
        /// <summary>
        /// If true, then the input value "wraps around" such that minval = maxval. For a periodic value, the input must be strictly less than maxval,
        /// otherwise maxval is a true upper bound.
        /// </summary>
        public const string PERIODIC = "periodic";//, Boolean.class),
        /// <summary>
        /// if true, non-periodic inputs smaller than minval or greater than maxval will be clipped to minval/maxval 
        /// </summary>
        public const string CLIP_INPUT = "clipInput";//, Boolean.class),
        /// <summary>
        /// If true, skip some safety checks (for compatibility reasons), default false  Mostly having to do with being able to set the window size &lt; 21 
        /// </summary>
        public const string FORCED = "forced";//, Boolean.class),
        /// <summary>
        /// Name of the field being encoded
        /// </summary>
        public const string FIELD_NAME = "fieldName";//, String.class),
        /// <summary>
        /// Primitive type of the field, used to auto-configure the type of encoder
        /// </summary>
        public const string FIELD_TYPE = "fieldType";//, String.class),
        /// <summary>
        /// Encoder name
        /// </summary>
        public const string ENCODER = "encoderType";//, String.class),
        /// <summary>
        /// Designates holder for the Multi Encoding Map
        /// </summary>
        public const string FIELD_ENCODING_MAP = "fieldEncodings";//, Map.class),

        public const string CATEGORY_LIST = "categoryList";//, List.class),

        /// <summary>
        /// Network Layer indicator for auto classifier generation
        /// </summary>
        public const string AUTO_CLASSIFY = "hasClassifiers";//, Boolean.class),

        /// <summary>
        /// Maps encoder input field name to type of classifier to be used for them
        /// </summary>
        public const string INFERRED_FIELDS = "inferredFields";//, Map.class), // Map<String, Classifier.class>

        //
        // How many bits to use if encoding the respective date fields.
        // e.g. Tuple(bits to use:int, radius:double)
        public const string DATEFIELD_SEASON = "season";//, Tuple.class), 
        public const string DATEFIELD_DOFW = "dayOfWeek";//, Tuple.class),
        public const string DATEFIELD_WKEND = "weekend";//, Tuple.class),
        public const string DATEFIELD_HOLIDAY = "holiday";//, Tuple.class),
        public const string DATEFIELD_TOFD = "timeOfDay";//, Tuple.class),
        public const string DATEFIELD_CUSTOM = "customDays";//, Tuple.class), // e.g. Tuple(bits:int, List<String>:"mon,tue,fri")
        public const string DATEFIELD_PATTERN = "formatPattern";//, String.class);

        #endregion
    }
}
