// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi.Entities
{
    public class MyDictionary<K, V> : Dictionary<K, V>
    {
        public new V this[K key]
        {
            get
            {
                return base[key];
            }
            set
            {
                addOrSet(key, value);
            }
        }
        public new void Add(K key, V value)
        {
            addOrSet(key, value);
        }

        private void addOrSet(K key, V value)
        {
            if (base.ContainsKey(key))
                base[key] = value;
            else
                base.Add(key, value);
        }
    }

    public class Parameters
    {

        private readonly MyDictionary<string, Object> paramMap = new MyDictionary<string, object>();

        private static readonly MyDictionary<string, Object> DEFAULTS_ALL;
        private static readonly MyDictionary<string, Object> DEFAULTS_TEMPORAL;
        private static readonly MyDictionary<string, Object> DEFAULTS_SPATIAL;
        private static readonly MyDictionary<string, Object> DEFAULTS_ENCODER;

        public static Random GetDefaultRandomGen(int seed)
        {
            return new ThreadSafeRandom(seed);

        }
        static Dictionary<string, Object> defaultParams;

        /////////// Universal Parameters ///////////
        static Parameters()
        {
            DEFAULTS_ALL = new MyDictionary<string, Object>();
            defaultParams = new MyDictionary<string, Object>();
            defaultParams.Add(KEY.SEED, 42);
            defaultParams.Add(KEY.RANDOM, GetDefaultRandomGen((int)defaultParams[KEY.SEED]));// new MersenneTwister((int) defaultParams.get(KEY.SEED)));

            #region Temporal Memory Parameters

            MyDictionary<string, Object> defaultTemporalParams = new MyDictionary<string, object>();
            defaultTemporalParams.Add(KEY.COLUMN_DIMENSIONS, new int[] { 2048 });
            defaultTemporalParams.Add(KEY.CELLS_PER_COLUMN, 32);
            defaultTemporalParams.Add(KEY.ACTIVATION_THRESHOLD, 10);
            defaultTemporalParams.Add(KEY.LEARNING_RADIUS, 10);
            defaultTemporalParams.Add(KEY.MIN_THRESHOLD, 9);
            defaultTemporalParams.Add(KEY.MAX_NEW_SYNAPSE_COUNT, 20);
            defaultTemporalParams.Add(KEY.MAX_SYNAPSES_PER_SEGMENT, 225);
            defaultTemporalParams.Add(KEY.MAX_SEGMENTS_PER_CELL, 225);
            defaultTemporalParams.Add(KEY.INITIAL_PERMANENCE, 0.21);
            defaultTemporalParams.Add(KEY.CONNECTED_PERMANENCE, 0.5);
            defaultTemporalParams.Add(KEY.PERMANENCE_INCREMENT, 0.10);
            defaultTemporalParams.Add(KEY.PERMANENCE_DECREMENT, 0.10);
            defaultTemporalParams.Add(KEY.PREDICTED_SEGMENT_DECREMENT, 0.1);
            defaultTemporalParams.Add(KEY.LEARN, true);

            DEFAULTS_TEMPORAL = defaultTemporalParams;
            add(DEFAULTS_ALL, defaultTemporalParams);
            //DEFAULTS_TEMPORAL = Collections.unmodifiableMap(defaultTemporalParams);
            //defaultParams.putAll(DEFAULTS_TEMPORAL);

            #endregion

            #region Spatial Pooler Parameters

            MyDictionary<string, Object> defaultSpatialParams = new MyDictionary<string, object>();
            defaultSpatialParams.Add(KEY.INPUT_DIMENSIONS, new int[] { 100 });
            defaultSpatialParams.Add(KEY.POTENTIAL_RADIUS, 15);
            defaultSpatialParams.Add(KEY.POTENTIAL_PCT, 0.75);
            defaultSpatialParams.Add(KEY.GLOBAL_INHIBITION, true);
            defaultSpatialParams.Add(KEY.INHIBITION_RADIUS, 15);
            defaultSpatialParams.Add(KEY.LOCAL_AREA_DENSITY, -1.0);
            defaultSpatialParams.Add(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * 2048);
            defaultSpatialParams.Add(KEY.STIMULUS_THRESHOLD, 5.0);
            defaultSpatialParams.Add(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            defaultSpatialParams.Add(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            defaultSpatialParams.Add(KEY.SYN_PERM_CONNECTED, 0.10);
            defaultSpatialParams.Add(KEY.SYN_PERM_BELOW_STIMULUS_INC, 0.01);
            defaultSpatialParams.Add(KEY.SYN_PERM_TRIM_THRESHOLD, 0.05);
            defaultSpatialParams.Add(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            defaultSpatialParams.Add(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            defaultSpatialParams.Add(KEY.DUTY_CYCLE_PERIOD, 1000);
            defaultSpatialParams.Add(KEY.MAX_BOOST, 10.0);
            defaultSpatialParams.Add(KEY.WRAP_AROUND, true);
            defaultSpatialParams.Add(KEY.LEARN, true);

            DEFAULTS_SPATIAL = defaultSpatialParams;
            add(DEFAULTS_ALL, defaultSpatialParams);
            //DEFAULTS_SPATIAL = Collections.unmodifiableMap(defaultSpatialParams);
            //defaultParams.putAll(DEFAULTS_SPATIAL);

            #endregion

            #region Encoder Parameters

            MyDictionary<string, Object> defaultEncoderParams = new MyDictionary<string, object>();
            defaultEncoderParams.Add(KEY.N, 500);
            defaultEncoderParams.Add(KEY.W, 21);
            defaultEncoderParams.Add(KEY.MIN_VAL, 0.0);
            defaultEncoderParams.Add(KEY.MAX_VAL, 1000.0);
            defaultEncoderParams.Add(KEY.RADIUS, 21.0);
            defaultEncoderParams.Add(KEY.RESOLUTION, 1.0);
            defaultEncoderParams.Add(KEY.PERIODIC, false);
            defaultEncoderParams.Add(KEY.CLIP_INPUT, false);
            defaultEncoderParams.Add(KEY.FORCED, false);
            defaultEncoderParams.Add(KEY.FIELD_NAME, "UNSET");
            defaultEncoderParams.Add(KEY.FIELD_TYPE, "int");
            defaultEncoderParams.Add(KEY.ENCODER, "CategoryEncoder");
            //defaultEncoderParams.Add(KEY.FIELD_ENCODING_MAP, Collections.emptyMap());
            defaultEncoderParams.Add(KEY.AUTO_CLASSIFY, false);

            DEFAULTS_ENCODER = defaultEncoderParams;
            //DEFAULTS_ENCODER = Collections.unmodifiableMap(defaultEncoderParams);
            //defaultParams.putAll(DEFAULTS_ENCODER);
            add(DEFAULTS_ALL, defaultEncoderParams);
            //DEFAULTS_ALL = 
            //DEFAULTS_ALL = Collections.unmodifiableMap(defaultParams);

            #endregion
        }


        /// <summary>
        /// Clones parameters.
        /// </summary>
        /// <returns></returns>
        public Parameters Clone()
        {
            Parameters newParams = new Parameters();
            foreach (var item in this.paramMap)
            {
                Parameters.add(newParams.paramMap, this.paramMap);
            }

            return newParams;
        }

        private static void add(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            foreach (var item in dict2)
            {
                dict1[item.Key] = item.Value;
            }
        }

        public void Set(string key, object val)
        {
            paramMap[key] = val;
        }

        public T Get<T>(string key)
        {
            return (T)paramMap[key];
        }

        public object this[string key]
        {
            get
            {
                return paramMap[key];
            }

            set
            {
                paramMap[key] = value;
            }
        }

        /// <summary>
        /// Returns the size of the internal parameter storage.
        /// </summary>
        /// <returns></returns>
        public int size()
        {
            return paramMap.Count;
        }

        public int MyProperty { get; set; }
        /// <summary>
        /// Factory method. Return global <see cref="Parameters"/> object with default values
        /// </summary>
        /// <returns><see cref="Parameters"/></returns>
        public static Parameters getAllDefaultParameters()
        {
            return getParameters(DEFAULTS_ALL);
        }

        /// <summary>
        /// Factory method. Return temporal <see cref="Parameters"/> object with default values
        /// </summary>
        /// <returns><see cref="Parameters"/> object</returns>
        public static Parameters getTemporalDefaultParameters()
        {
            return getParameters(DEFAULTS_TEMPORAL);
        }

        /// <summary>
        /// Factory method. Return spatial <see cref="Parameters"/> object with default values
        /// </summary>
        /// <returns><see cref="Parameters"/></returns>
        public static Parameters getSpatialDefaultParameters()
        {
            return getParameters(DEFAULTS_SPATIAL);
        }

        /// <summary>
        /// Factory method. Return Encoder <see cref="Parameters"/> object with default values
        /// </summary>
        /// <returns></returns>
        public static Parameters getEncoderDefaultParameters()
        {
            return getParameters(DEFAULTS_ENCODER);
        }

        /// <summary>
        /// Called internally to populate a <see cref="Parameters"/> object with the keys and values specified in the passed in map.
        /// </summary>
        /// <param name="map"></param>
        /// <returns><see cref="Parameters"/></returns>
        private static Parameters getParameters(Dictionary<string, Object> map)
        {
            Parameters result = new Parameters();
            foreach (var key in map)
            {
                result.Set(key.Key, map[key.Key]);
            }
            return result;
        }

        /// <summary>
        /// Sets the fields specified by this <see cref="Parameters"/> on the specified <see cref="Connections"/> object.
        /// </summary>
        /// <param name="cn"></param>
        public void apply(Object cn)
        {
            var methods = cn.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // BeanUtil beanUtil = BeanUtil.getInstance();
            //Set<KEY> presentKeys = paramMap.keySet();
            //synchronized(paramMap) {
            foreach (var key in new List<string>(paramMap.Keys))
            {
                if ((cn is Connections) &&
                     (key == KEY.SYN_PERM_BELOW_STIMULUS_INC || key == KEY.SYN_PERM_TRIM_THRESHOLD))
                {
                    continue;
                }
                if (key == KEY.RANDOM)
                {
                    if (paramMap.ContainsKey(KEY.SEED))
                        paramMap[key] = GetDefaultRandomGen((int)paramMap[KEY.SEED]);
                    else
                        paramMap[key] = new ThreadSafeRandom(42);
                }

                string methodName = $"set{key.First().ToString().ToUpper()}{key.Substring(1)}";
                var method = methods.FirstOrDefault(m => m.Name == methodName);
                if (method != null)
                {
                    try
                    {
                        method.Invoke(cn, new object[] { paramMap[key] });
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error when setting parameter '{key}'", ex);
                    }
                }
                else
                {
                    var properties = cn.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var prop = properties.FirstOrDefault(m => m.Name == $"{key.First().ToString().ToUpper()}{key.Substring(1)}");
                    if (prop != null)
                    {
                        if (prop.CanWrite)
                            prop.SetValue(cn, paramMap[key]);
                    }
                    else
                    {
                        var htmCfgProperty = properties.FirstOrDefault(m => m.Name == "HtmConfig");
                        if (htmCfgProperty != null)
                        {
                            prop = htmCfgProperty.PropertyType.GetProperties().FirstOrDefault(m => m.Name == $"{key.First().ToString().ToUpper()}{key.Substring(1)}");
                            if (prop != null)
                            {
                                var htmCfgInst = htmCfgProperty.GetValue(cn);
                                prop.SetValue(htmCfgInst, paramMap[key]);
                            }
                            else
                            {

                            }
                        }
                    }
                }
                //beanUtil.setSimpleProperty(cn, key.fieldName, get(key));
                //}
            }
        }


        //    /**
        //     * Constant values representing configuration parameters for the {@link TemporalMemory}
        //     */
        //    public  enum KEY
        //{
        //    /////////// Universal Parameters ///////////
        //    /**
        //     * Total number of columns
        //     */
        //    COLUMN_DIMENSIONS("columnDimensions", int[].class),
        //        /**
        //         * Total number of cells per column
        //         */
        //        CELLS_PER_COLUMN("cellsPerColumn", Integer.class, 1, null),
        //        /**
        //         * Learning variable
        //         */
        //        LEARN("learn", Boolean.class),
        //        /**
        //         * Random Number Generator
        //         */
        //        RANDOM("random", Random.class),
        //        /**
        //         * Seed for random number generator
        //         */
        //        SEED("seed", Integer.class),

        //        /////////// Temporal Memory Parameters ///////////
        //        /**
        //         * If the number of active connected synapses on a segment
        //         * is at least this threshold, the segment is said to be active.
        //         */
        //        ACTIVATION_THRESHOLD("activationThreshold", Integer.class, 0, null),
        //        /**
        //         * Radius around cell from which it can
        //         * sample to form distal {@link DistalDendrite} connections.
        //         */
        //        LEARNING_RADIUS("learningRadius", Integer.class, 0, null),
        //        /**
        //         * If the number of synapses active on a segment is at least this
        //         * threshold, it is selected as the best matching
        //         * cell in a bursting column.
        //         */
        //        MIN_THRESHOLD("minThreshold", Integer.class, 0, null),
        //        /**
        //         * The maximum number of synapses added to a segment during learning.
        //         */
        //        MAX_NEW_SYNAPSE_COUNT("maxNewSynapseCount", Integer.class),
        //        /**
        //         * The maximum number of synapses that can be added to a segment.
        //         */
        //        MAX_SYNAPSES_PER_SEGMENT("maxSynapsesPerSegment", Integer.class),
        //        /**
        //         * The maximum number of {@link Segment}s a {@link Cell} can have.
        //         */
        //        MAX_SEGMENTS_PER_CELL("maxSegmentsPerCell", Integer.class),
        //        /**
        //         * Initial permanence of a new synapse
        //         */
        //        INITIAL_PERMANENCE("initialPermanence", Double.class, 0.0, 1.0),
        //        /**
        //         * If the permanence value for a synapse
        //         * is greater than this value, it is said
        //         * to be connected.
        //         */
        //        CONNECTED_PERMANENCE("connectedPermanence", Double.class, 0.0, 1.0),
        //        /**
        //         * Amount by which permanence of synapses
        //         * are incremented during learning.
        //         */
        //        PERMANENCE_INCREMENT("permanenceIncrement", Double.class, 0.0, 1.0),
        //        /**
        //         * Amount by which permanences of synapses
        //         * are decremented during learning.
        //         */
        //        PERMANENCE_DECREMENT("permanenceDecrement", Double.class, 0.0, 1.0),
        //        /**
        //         * Amount by which active permanences of synapses of previously 
        //         * predicted but inactive segments are decremented.
        //         */
        //        PREDICTED_SEGMENT_DECREMENT("predictedSegmentDecrement", Double.class, 0.0, 9.0),
        //        /** TODO: Remove this and add Logging (slf4j) */
        //        //TM_VERBOSITY("tmVerbosity", Integer.class, 0, 10),


        //        /////////// Spatial Pooler Parameters ///////////
        //        INPUT_DIMENSIONS("inputDimensions", int[].class),
        //        /** <b>WARNING:</b> potentialRadius **must** be set to 
        //         * the inputWidth if using "globalInhibition" and if not 
        //         * using the Network API (which sets this automatically) 
        //         */
        //        POTENTIAL_RADIUS("potentialRadius", Integer.class),
        //        /**
        //         * The percent of the inputs, within a column's potential radius, that a
        //         * column can be connected to.  If set to 1, the column will be connected
        //         * to every input within its potential radius. This parameter is used to
        //         * give each column a unique potential pool when a large potentialRadius
        //         * causes overlap between the columns. At initialization time we choose
        //         * ((2*potentialRadius + 1)^(# inputDimensions) * potentialPct) input bits
        //         * to comprise the column's potential pool.
        //         */
        //        POTENTIAL_PCT("potentialPct", Double.class), //TODO add range here?
        //        /**
        //         * If true, then during inhibition phase the winning columns are selected
        //         * as the most active columns from the region as a whole. Otherwise, the
        //         * winning columns are selected with respect to their local neighborhoods.
        //         * Using global inhibition boosts performance x60.
        //         */
        //        GLOBAL_INHIBITION("globalInhibition", Boolean.class),
        //        /**
        //         * The inhibition radius determines the size of a column's local
        //         * neighborhood.  A cortical column must overcome the overlap score of
        //         * columns in its neighborhood in order to become active. This radius is
        //         * updated every learning round. It grows and shrinks with the average
        //         * number of connected synapses per column.
        //         */
        //        INHIBITION_RADIUS("inhibitionRadius", Integer.class, 0, null),
        //        /**
        //         * The desired density of active columns within a local inhibition area
        //         * (the size of which is set by the internally calculated inhibitionRadius,
        //         * which is in turn determined from the average size of the connected
        //         * potential pools of all columns). The inhibition logic will insure that
        //         * at most N columns remain ON within a local inhibition area, where
        //         * N = localAreaDensity * (total number of columns in inhibition area).
        //         */
        //        LOCAL_AREA_DENSITY("localAreaDensity", Double.class), //TODO add range here?
        //        /**
        //         * An alternate way to control the density of the active columns. If
        //         * numActiveColumnsPerInhArea is specified then localAreaDensity must be
        //         * less than 0, and vice versa.  When using numActiveColumnsPerInhArea, the
        //         * inhibition logic will insure that at most 'numActiveColumnsPerInhArea'
        //         * columns remain ON within a local inhibition area (the size of which is
        //         * set by the internally calculated inhibitionRadius, which is in turn
        //         * determined from the average size of the connected receptive fields of all
        //         * columns). When using this method, as columns learn and grow their
        //         * effective receptive fields, the inhibitionRadius will grow, and hence the
        //         * net density of the active columns will *decrease*. This is in contrast to
        //         * the localAreaDensity method, which keeps the density of active columns
        //         * the same regardless of the size of their receptive fields.
        //         */
        //        NUM_ACTIVE_COLUMNS_PER_INH_AREA("numActiveColumnsPerInhArea", Double.class),//TODO add range here?
        //        /**
        //         * This is a number specifying the minimum number of synapses that must be
        //         * on in order for a columns to turn ON. The purpose of this is to prevent
        //         * noise input from activating columns. Specified as a percent of a fully
        //         * grown synapse.
        //         */
        //        STIMULUS_THRESHOLD("stimulusThreshold", Double.class), //TODO add range here?
        //        /**
        //         * The amount by which an inactive synapse is decremented in each round.
        //         * Specified as a percent of a fully grown synapse.
        //         */
        //        SYN_PERM_INACTIVE_DEC("synPermInactiveDec", Double.class, 0.0, 1.0),
        //        /**
        //         * The amount by which an active synapse is incremented in each round.
        //         * Specified as a percent of a fully grown synapse.
        //         */
        //        SYN_PERM_ACTIVE_INC("synPermActiveInc", Double.class, 0.0, 1.0),
        //        /**
        //         * The default connected threshold. Any synapse whose permanence value is
        //         * above the connected threshold is a "connected synapse", meaning it can
        //         * contribute to the cell's firing.
        //         */
        //        SYN_PERM_CONNECTED("synPermConnected", Double.class, 0.0, 1.0),
        //        /**
        //         * <b>WARNING:</b> This is a <i><b>derived</b><i> value, and is overwritten
        //         * by the SpatialPooler algorithm's initialization.
        //         * 
        //         * The permanence increment amount for columns that have not been
        //         * recently active
        //         */
        //        SYN_PERM_BELOW_STIMULUS_INC("synPermBelowStimulusInc", Double.class, 0.0, 1.0),
        //        /**
        //         * <b>WARNING:</b> This is a <i><b>derived</b><i> value, and is overwritten
        //         * by the SpatialPooler algorithm's initialization.
        //         * 
        //         * Values below this are "clipped" and zero'd out.
        //         */
        //        SYN_PERM_TRIM_THRESHOLD("synPermTrimThreshold", Double.class, 0.0, 1.0),
        //        /**
        //         * A number between 0 and 1.0, used to set a floor on how often a column
        //         * should have at least stimulusThreshold active inputs. Periodically, each
        //         * column looks at the overlap duty cycle of all other columns within its
        //         * inhibition radius and sets its own internal minimal acceptable duty cycle
        //         * to: minPctDutyCycleBeforeInh * max(other columns' duty cycles).  On each
        //         * iteration, any column whose overlap duty cycle falls below this computed
        //         * value will  get all of its permanence values boosted up by
        //         * synPermActiveInc. Raising all permanences in response to a sub-par duty
        //         * cycle before  inhibition allows a cell to search for new inputs when
        //         * either its previously learned inputs are no longer ever active, or when
        //         * the vast majority of them have been "hijacked" by other columns.
        //         */
        //        MIN_PCT_OVERLAP_DUTY_CYCLES("minPctOverlapDutyCycles", Double.class),//TODO add range here?
        //        /**
        //         * A number between 0 and 1.0, used to set a floor on how often a column
        //         * should be activate.  Periodically, each column looks at the activity duty
        //         * cycle of all other columns within its inhibition radius and sets its own
        //         * internal minimal acceptable duty cycle to: minPctDutyCycleAfterInh *
        //         * max(other columns' duty cycles).  On each iteration, any column whose duty
        //         * cycle after inhibition falls below this computed value will get its
        //         * internal boost factor increased.
        //         */
        //        MIN_PCT_ACTIVE_DUTY_CYCLES("minPctActiveDutyCycles", Double.class),//TODO add range here?
        //        /**
        //         * The period used to calculate duty cycles. Higher values make it take
        //         * longer to respond to changes in boost or synPerConnectedCell. Shorter
        //         * values make it more unstable and likely to oscillate.
        //         */
        //        DUTY_CYCLE_PERIOD("dutyCyclePeriod", Integer.class),//TODO add range here?
        //        /**
        //         * The maximum overlap boost factor. Each column's overlap gets multiplied
        //         * by a boost factor before it gets considered for inhibition.  The actual
        //         * boost factor for a column is number between 1.0 and maxBoost. A boost
        //         * factor of 1.0 is used if the duty cycle is >= minOverlapDutyCycle,
        //         * maxBoost is used if the duty cycle is 0, and any duty cycle in between is
        //         * linearly extrapolated from these 2 endpoints.
        //         */
        //        MAX_BOOST("maxBoost", Double.class), //TODO add range here?
        //        /**
        //         * Determines if inputs at the beginning and end of an input dimension should
        //         * be considered neighbors when mapping columns to inputs.
        //         */
        //        WRAP_AROUND("wrapAround", Boolean.class),

        //        ///////////// SpatialPooler / Network Parameter(s) /////////////
        //        /** Number of cycles to send through the SP before forwarding data to the rest of the network. */
        //        SP_PRIMER_DELAY("sp_primer_delay", Integer.class),

        //        ///////////// Encoder Parameters //////////////
        //        /** number of bits in the representation (must be &gt;= w) */
        //        N("n", Integer.class),
        //        /** 
        //         * The number of bits that are set to encode a single value - the
        //         * "width" of the output signal
        //         */
        //        W("w", Integer.class),
        //        /** The minimum value of the input signal.  */
        //        MIN_VAL("minVal", Double.class),
        //        /** The maximum value of the input signal. */
        //        MAX_VAL("maxVal", Double.class),
        //        /**
        //         * inputs separated by more than, or equal to this distance will have non-overlapping
        //         * representations
        //         */
        //        RADIUS("radius", Double.class),
        //        /** inputs separated by more than, or equal to this distance will have different representations */
        //        RESOLUTION("resolution", Double.class),
        //        /**
        //         * If true, then the input value "wraps around" such that minval = maxval
        //         * For a periodic value, the input must be strictly less than maxval,
        //         * otherwise maxval is a true upper bound.
        //         */
        //        PERIODIC("periodic", Boolean.class),
        //        /** 
        //         * if true, non-periodic inputs smaller than minval or greater
        //         * than maxval will be clipped to minval/maxval 
        //         */
        //        CLIP_INPUT("clipInput", Boolean.class),
        //        /** 
        //         * If true, skip some safety checks (for compatibility reasons), default false 
        //         * Mostly having to do with being able to set the window size &lt; 21 
        //         */
        //        FORCED("forced", Boolean.class),
        //        /** Name of the field being encoded */
        //        FIELD_NAME("fieldName", String.class),
        //        /** Primitive type of the field, used to auto-configure the type of encoder */
        //        FIELD_TYPE("fieldType", String.class),
        //        /** Encoder name */
        //        ENCODER("encoderType", String.class),
        //        /** Designates holder for the Multi Encoding Map */
        //        FIELD_ENCODING_MAP("fieldEncodings", Map.class),
        //        CATEGORY_LIST("categoryList", List.class),

        //        // Network Layer indicator for auto classifier generation
        //        AUTO_CLASSIFY("hasClassifiers", Boolean.class),

        //        /** Maps encoder input field name to type of classifier to be used for them */
        //        INFERRED_FIELDS("inferredFields", Map.class), // Map<String, Classifier.class>

        //        // How many bits to use if encoding the respective date fields.
        //        // e.g. Tuple(bits to use:int, radius:double)
        //        DATEFIELD_SEASON("season", Tuple.class), 
        //        DATEFIELD_DOFW("dayOfWeek", Tuple.class),
        //        DATEFIELD_WKEND("weekend", Tuple.class),
        //        DATEFIELD_HOLIDAY("holiday", Tuple.class),
        //        DATEFIELD_TOFD("timeOfDay", Tuple.class),
        //        DATEFIELD_CUSTOM("customDays", Tuple.class), // e.g. Tuple(bits:int, List<String>:"mon,tue,fri")
        //        DATEFIELD_PATTERN("formatPattern", String.class);


        //private static final Map<String, KEY> fieldMap = new HashMap<>();

        //static {
        //    for (KEY key : KEY.values()) {
        //        fieldMap.put(key.getFieldName(), key);
        //    }


        //public static KEY getKeyByFieldName(String fieldName)
        //{
        //    return fieldMap.get(fieldName);
        //}

        //final private String fieldName;
        //final private Class<?> fieldType;
        //final private Number min;
        //final private Number max;

        /**
         * Constructs a new KEY
         *
         * @param fieldName
         * @param fieldType
         */
        //private KEY(String fieldName, Class<?> fieldType)
        //{
        //    this(fieldName, fieldType, null, null);
        //}

        /**
         * Constructs a new KEY with range check
         *
         * @param fieldName
         * @param fieldType
         * @param min
         * @param max
         */
        //private KEY(String fieldName, Class<?> fieldType, Number min, Number max)
        //{
        //    this.fieldName = fieldName;
        //    this.fieldType = fieldType;
        //    this.min = min;
        //    this.max = max;
        //}

        //public Class<?> getFieldType()
        //{
        //    return fieldType;
        //}

        //public String getFieldName()
        //{
        //    return fieldName;
        //}

        //public Number getMin()
        //{
        //    return min;
        //}

        //public Number getMax()
        //{
        //    return max;
        //}

        //public bool checkRange(Number value)
        //{
        //    if (value == null)
        //    {
        //        throw new ArgumentException("checkRange argument can not be null");
        //    }
        //    return (min == null && max == null) ||
        //           (min != null && max == null && value.doubleValue() >= min.doubleValue()) ||
        //           (max != null && min == null && value.doubleValue() <= max.doubleValue()) ||
        //           (min != null && value.doubleValue() >= min.doubleValue() && max != null && value.doubleValue() <= max.doubleValue());
        //}

        //    }

        //    /**
        //     * Save guard decorator around params map
        //     */
        //private static class ParametersMap //extends EnumMap<KEY, Object> 
        //{
        //    /**
        //     * Default serialvers
        //     */
        //    private static final long serialVersionUID = 1L;

        //    ParametersMap()
        //    {
        //        super(Parameters.KEY.class);
        //        }

        //@Override public Object put(KEY key, Object value)
        //{
        //    if (value != null)
        //    {
        //        if (!key.getFieldType().isInstance(value))
        //        {
        //            throw new IllegalArgumentException(
        //                    "Can not set Parameters Property '" + key.getFieldName() + "' because of type mismatch. The required type is " + key.getFieldType());
        //        }
        //        if (value instanceof Number && !key.checkRange((Number)value)) {
        //            throw new IllegalArgumentException(
        //                    "Can not set Parameters Property '" + key.getFieldName() + "' because of value '" + value + "' not in range. Range[" + key.getMin() + "-" + key.getMax() + "]");
        //        }
        //    }
        //    return super.put(key, value);
        //}
        //    }



        //TODO apply from container to parameters



        ///**
        // * Factory method. Return global {@link Parameters} object with default values
        // *
        // * @return {@link Parameters} object
        // */
        //public static Parameters getAllDefaultParameters()
        //{
        //    return getParameters(DEFAULTS_ALL);
        //}

        ///**
        // * Factory method. Return temporal {@link Parameters} object with default values
        // *
        // * @return {@link Parameters} object
        // */
        //public static Parameters getTemporalDefaultParameters()
        //{
        //    return getParameters(DEFAULTS_TEMPORAL);
        //}


        ///**
        // * Factory method. Return spatial {@link Parameters} object with default values
        // *
        // * @return {@link Parameters} object
        // */
        //public static Parameters getSpatialDefaultParameters()
        //{
        //    return getParameters(DEFAULTS_SPATIAL);
        //}

        ///**
        // * Factory method. Return Encoder {@link Parameters} object with default values
        // * @return
        // */
        //public static Parameters getEncoderDefaultParameters()
        //{
        //    return getParameters(DEFAULTS_ENCODER);
        //}
        ///**
        // * Called internally to populate a {@link Parameters} object with the keys
        // * and values specified in the passed in map.
        // *
        // * @return {@link Parameters} object
        // */
        //private static Parameters getParameters(Dictionary<string, Object> map)
        //{
        //    Parameters result = new Parameters();
        //    foreach (var key in map)
        //    {
        //        result.set(key, map[key.Key]);
        //    }
        //    return result;
        //}


        /**
         * Constructs a new {@code Parameters} object.
         * It is private. Only allow instantiation with Factory methods.
         * This way we will never have erroneous Parameters with missing attributes
         */
        //private Parameters()
        //{
        //}



        /**
         * Copies the specified parameters into this {@code Parameters}
         * object over writing the intersecting keys and values.
         * @param p     the Parameters to perform a union with.
         * @return      this Parameters object combined with the specified
         *              Parameters object.
         */
        //public Parameters union(Parameters p)
        //{
        //    for (KEY k : p.paramMap.keySet())
        //    {
        //        set(k, p.get(k));
        //    }
        //    return this;
        //}

        /**
         * Returns a Set view of the keys in this {@code Parameter}s 
         * object
         * @return
         */
        //public Set<KEY> keys()
        //{
        //    Set<KEY> retVal = paramMap.keySet();
        //    return retVal;
        //}

        /**
         * Returns a separate instance of the specified {@code Parameters} object.
         * @return      a unique instance.
         */
        //public Parameters copy()
        //{
        //    return new Parameters().union(this);
        //}

        /**
         * Returns an empty instance of {@code Parameters};
         * @return
         */
        //public static Parameters empty()
        //{
        //    return new Parameters();
        //}

        /**
         * Set parameter by Key{@link KEY}
         *
         * @param key
         * @param value
         */
        //public void set(KEY key, Object value)
        //{
        //    paramMap.put(key, value);
        //}

        /**
         * Get parameter by Key{@link KEY}
         *
         * @param key
         * @return
         */
        //public Object get(KEY key)
        //{
        //    return paramMap.get(key);
        //}

        /**
         * @param key IMPORTANT! This is a nuclear option, should be used with care. 
         * Will knockout key's parameter from map and compromise integrity
         */
        //public void clearParameter(KEY key)
        //{
        //    paramMap.remove(key);
        //}

        /**
         * Convenience method to log difference this {@code Parameters} and specified
         * {@link Connections} object.
         *
         * @param cn
         * @return true if find it different
         */
        //public bool logDiff(Object cn)
        //{
        //    if (cn == null)
        //    {
        //        throw new ArgumentException("cn Object is required and can not be null");
        //    }
        //    bool result = false;
        //    BeanUtil beanUtil = BeanUtil.getInstance();
        //    BeanUtil.PropertyInfo[] properties = beanUtil.getPropertiesInfoForBean(cn.getClass());
        //    for (int i = 0; i < properties.length; i++)
        //    {
        //        BeanUtil.PropertyInfo property = properties[i];
        //        String fieldName = property.getName();
        //        KEY propKey = KEY.getKeyByFieldName(property.getName());
        //        if (propKey != null)
        //        {
        //            Object paramValue = this.get(propKey);
        //            Object cnValue = beanUtil.getSimpleProperty(cn, fieldName);

        //            // KEY.POTENTIAL_RADIUS is defined as Math.min(cn.numInputs, potentialRadius) so just log...
        //            if (propKey == KEY.POTENTIAL_RADIUS)
        //            {
        //                System.out.println(
        //                    "Difference is OK: Property:" + fieldName + " is different - CN:" + cnValue + " | PARAM:" + paramValue);
        //            }
        //            else if ((paramValue != null && !paramValue.equals(cnValue)) || (paramValue == null && cnValue != null))
        //            {
        //                result = true;
        //                System.out.println(
        //                    "Property:" + fieldName + " is different - CONNECTIONS:" + cnValue + " | PARAMETERS:" + paramValue);
        //            }
        //        }
        //    }
        //    return result;
        //}

        //TODO I'm not sure we need maintain implicit setters below. Kinda contradict unified access with KEYs

        /// <summary>
        /// Returns the seeded random number generator.
        /// </summary>
        /// <param name="r"></param>
        public void setRandom(Random r)
        {
            paramMap.Add(KEY.RANDOM, r);
        }

        /// <summary>
        /// Sets the number of <see cref="Column"/>.
        /// </summary>
        /// <param name="columnDimensions"></param>
        public void setColumnDimensions(int[] columnDimensions)
        {
            paramMap.Add(KEY.COLUMN_DIMENSIONS, columnDimensions);
        }

        /// <summary>
        /// Sets the number of <see cref="Cell"/>s per <see cref="Column"/>
        /// </summary>
        /// <param name="cellsPerColumn"></param>
        public void setCellsPerColumn(int cellsPerColumn)
        {
            paramMap.Add(KEY.CELLS_PER_COLUMN, cellsPerColumn);
        }

        /// <summary>
        /// Sets the activation threshold.<br/>
        /// If the number of active connected synapses on a segment is at least this threshold, the segment is said to be active.
        /// </summary>
        /// <param name="activationThreshold"></param>
        public void setActivationThreshold(int activationThreshold)
        {
            paramMap.Add(KEY.ACTIVATION_THRESHOLD, activationThreshold);
        }

        /// <summary>
        /// Radius around cell from which it can sample to form distal dendrite connections.
        /// </summary>
        /// <param name="learningRadius"></param>
        public void setLearningRadius(int learningRadius)
        {
            paramMap.Add(KEY.LEARNING_RADIUS, learningRadius);
        }

        /// <summary>
        /// If the number of synapses active on a segment is at least this threshold, it is selected as the best matching cell in a bursting column.
        /// </summary>
        /// <param name="minThreshold"></param>
        public void setMinThreshold(int minThreshold)
        {
            paramMap.Add(KEY.MIN_THRESHOLD, minThreshold);
        }

        /// <summary>
        /// The maximum number of synapses added to a segment during learning.
        /// </summary>
        /// <param name="maxSynapsesPerSegment"></param>
        public void setMaxSynapsesPerSegment(int maxSynapsesPerSegment)
        {
            paramMap.Add(KEY.MAX_SYNAPSES_PER_SEGMENT, maxSynapsesPerSegment);
        }

        /// <summary>
        /// The maximum number of <see cref="Segment"/>s a <see cref="Cell"/> can have.
        /// </summary>
        /// <param name="maxSegmentsPerCell"></param>
        public void setMaxSegmentsPerCell(int maxSegmentsPerCell)
        {
            paramMap.Add(KEY.MAX_SEGMENTS_PER_CELL, maxSegmentsPerCell);
        }

        /// <summary>
        /// The maximum number of new synapses per segment.
        /// </summary>
        /// <param name="count"></param>
        public void setMaxNewSynapsesPerSegmentCount(int count)
        {
            paramMap.Add(KEY.MAX_NEW_SYNAPSE_COUNT, count);
        }

        /// <summary>
        /// Seed for random number generator 
        /// </summary>
        /// <param name="seed"></param>
        public void setSeed(int seed)
        {
            paramMap.Add(KEY.SEED, seed);
        }

        /// <summary>
        /// Initial permanence of a new synapse
        /// </summary>
        /// <param name="initialPermanence"></param>
        public void setInitialPermanence(double initialPermanence)
        {
            paramMap.Add(KEY.INITIAL_PERMANENCE, initialPermanence);
        }

        /// <summary>
        /// If the permanence value for a synapse is greater than this value, it is said to be connected.
        /// </summary>
        /// <param name="connectedPermanence"></param>
        public void setConnectedPermanence(double connectedPermanence)
        {
            paramMap.Add(KEY.CONNECTED_PERMANENCE, connectedPermanence);
        }

        /// <summary>
        /// Amount by which permanences of synapses are incremented during learning.
        /// </summary>
        /// <param name="permanenceIncrement"></param>
        public void setPermanenceIncrement(double permanenceIncrement)
        {
            paramMap.Add(KEY.PERMANENCE_INCREMENT, permanenceIncrement);
        }

        /// <summary>
        /// Amount by which permanences of synapses are decremented during learning.
        /// </summary>
        /// <param name="permanenceDecrement"></param>
        public void setPermanenceDecrement(double permanenceDecrement)
        {
            paramMap.Add(KEY.PERMANENCE_DECREMENT, permanenceDecrement);
        }

        #region SPATIAL POOLER PARAMS

        /// <summary>
        /// A list representing the dimensions of the input vector. Format is [height, width, depth, ...], where each value represents the size of
        /// the dimension. For a topology of one dimension with 100 inputs use 100, or [100]. For a two dimensional topology of 10x5 use [10,5].
        /// </summary>
        /// <param name="inputDimensions"></param>
        public void setInputDimensions(int[] inputDimensions)
        {
            paramMap.Add(KEY.INPUT_DIMENSIONS, inputDimensions);
        }

        /// <summary>
        /// This parameter determines the extent of the input that each column can potentially be connected to. This can be thought of as 
        /// the input bits that are visible to each column, or a 'receptiveField' of the field of vision. A large enough value will result in 
        /// 'global coverage', meaning that each column can potentially be connected to every input bit. This parameter defines a square 
        /// (or hyper square) area: a column will have a max square potential pool with sides of length 2 * potentialRadius + 1.
        /// </summary>
        /// <param name="potentialRadius"></param>
        /// <remarks>
        /// <b>WARNING:</b> potentialRadius <i>must</i> be set to 
        /// the inputWidth if using "globalInhibition" and if not 
        /// using the Network API (which sets this automatically) 
        /// 
        /// </remarks>
        public void setPotentialRadius(int potentialRadius)
        {
            if (!paramMap.ContainsKey(KEY.POTENTIAL_RADIUS))
                paramMap.Add(KEY.POTENTIAL_RADIUS, potentialRadius);
            else
                paramMap[KEY.POTENTIAL_RADIUS] = potentialRadius;
        }

        /// <summary>
        /// The inhibition radius determines the size of a column's local neighborhood. of a column. A cortical column must overcome the overlap
        /// score of columns in his neighborhood in order to become actives. This radius is updated every learning round. It grows and shrinks with the
        /// average number of connected synapses per column.
        /// </summary>
        /// <param name="inhibitionRadius">the local group size</param>
        public void setInhibitionRadius(int inhibitionRadius)
        {
            paramMap.Add(KEY.INHIBITION_RADIUS, inhibitionRadius);
        }

        /// <summary>
        /// The percent of the inputs, within a column's potential radius, that a column can be connected to. If set to 1, the column will be connected
        /// to every input within its potential radius. This parameter is used to give each column a unique potential pool when a large potentialRadius
        /// causes overlap between the columns. At initialization time we choose ((2*potentialRadius + 1)^(# inputDimensions) * potentialPct) input bits 
        /// to comprise the column's potential pool.
        /// </summary>
        /// <param name="potentialPct"></param>
        public void setPotentialPct(double potentialPct)
        {
            paramMap.Add(KEY.POTENTIAL_PCT, potentialPct);
        }

        /// <summary>
        /// If true, then during inhibition phase the winning columns are selected as the most active columns from the region as a whole. 
        /// Otherwise, the winning columns are selected with respect to their local neighborhoods. Using global inhibition boosts performance x60.
        /// </summary>
        /// <param name="globalInhibition"></param>
        public void setGlobalInhibition(bool globalInhibition)
        {
            paramMap.Add(KEY.GLOBAL_INHIBITION, globalInhibition);
        }

        /// <summary>
        /// The desired density of active columns within a local inhibition area (the size of which is set by the internally calculated inhibitionRadius,
        /// which is in turn determined from the average size of the connected potential pools of all columns). The inhibition logic will insure that 
        /// at most N columns remain ON within a local inhibition area, where N = localAreaDensity * (total number of columns in inhibition area).
        /// </summary>
        /// <param name="localAreaDensity"></param>
        public void setLocalAreaDensity(double localAreaDensity)
        {
            paramMap.Add(KEY.LOCAL_AREA_DENSITY, localAreaDensity);
        }

        /// <summary>
        /// An alternate way to control the density of the active columns. If numActivePerInhArea is specified then localAreaDensity must be less than 0,
        /// and vice versa. When using numActivePerInhArea, the inhibition logic will insure that at most 'numActivePerInhArea' columns remain ON within
        /// a local inhibition area (the size of which is set by the internally calculated inhibitionRadius, which is in turn determined from 
        /// the average size of the connected receptive fields of all columns). When using this method, as columns learn and grow their effective receptive
        /// fields, the inhibitionRadius will grow, and hence the net density of the active columns will *decrease*. This is in contrast to 
        /// the localAreaDensity method, which keeps the density of active columns the same regardless of the size of their receptive fields.
        /// </summary>
        /// <param name="numActiveColumnsPerInhArea"></param>
        public void setNumActiveColumnsPerInhArea(double numActiveColumnsPerInhArea)
        {
            paramMap.Add(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, numActiveColumnsPerInhArea);
        }

        /// <summary>
        /// This is a number specifying the minimum number of synapses that must be on in order for a columns to turn ON. The purpose of this is 
        /// to prevent noise input from activating columns. Specified as a percent of a fully grown synapse.
        /// </summary>
        /// <param name="stimulusThreshold"></param>
        public void setStimulusThreshold(double stimulusThreshold)
        {
            paramMap.Add(KEY.STIMULUS_THRESHOLD, stimulusThreshold);
        }

        /// <summary>
        /// The amount by which an inactive synapse is decremented in each round. Specified as a percent of a fully grown synapse.
        /// </summary>
        /// <param name="synPermInactiveDec"></param>
        public void setSynPermInactiveDec(double synPermInactiveDec)
        {
            paramMap.Add(KEY.SYN_PERM_INACTIVE_DEC, synPermInactiveDec);
        }

        /// <summary>
        /// The amount by which an active synapse is incremented in each round. Specified as a percent of a fully grown synapse.
        /// </summary>
        /// <param name="synPermActiveInc"></param>
        public void setSynPermActiveInc(double synPermActiveInc)
        {
            paramMap.Add(KEY.SYN_PERM_ACTIVE_INC, synPermActiveInc);
        }

        /// <summary>
        /// The default connected threshold. Any synapse whose permanence value is above the connected threshold is a "connected synapse", 
        /// meaning it can contribute to the cell's firing.
        /// </summary>
        /// <param name="synPermConnected"></param>
        public void setSynPermConnected(double synPermConnected)
        {
            paramMap.Add(KEY.SYN_PERM_CONNECTED, synPermConnected);
        }

        /// <summary>
        /// Sets the increment of synapse permanences below the stimulus threshold
        /// </summary>
        /// <param name="synPermBelowStimulusInc"></param>
        public void setSynPermBelowStimulusInc(double synPermBelowStimulusInc)
        {
            paramMap.Add(KEY.SYN_PERM_BELOW_STIMULUS_INC, synPermBelowStimulusInc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="synPermTrimThreshold"></param>
        public void setSynPermTrimThreshold(double synPermTrimThreshold)
        {
            paramMap.Add(KEY.SYN_PERM_TRIM_THRESHOLD, synPermTrimThreshold);
        }

        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should have at least stimulusThreshold active inputs. Periodically, each
        /// column looks at the overlap duty cycle of all other columns within its inhibition radius and sets its own internal minimal acceptable duty cycle
        /// to: minPctDutyCycleBeforeInh * max(other columns' duty cycles).<br/>
        /// On each iteration, any column whose overlap duty cycle falls below this computed value will  get all of its permanence values boosted up by
        /// synPermActiveInc. Raising all permanences in response to a sub-par duty cycle before  inhibition allows a cell to search for new inputs when 
        /// either its previously learned inputs are no longer ever active, or when the vast majority of them have been "hijacked" by other columns.
        /// </summary>
        /// <param name="minPctOverlapDutyCycles"></param>
        public void setMinPctOverlapDutyCycles(double minPctOverlapDutyCycles)
        {
            paramMap.Add(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minPctOverlapDutyCycles);
        }

        /// <summary>
        /// A number between 0 and 1.0, used to set a floor on how often a column should be activate. Periodically, each column looks at the activity duty
        /// cycle of all other columns within its inhibition radius and sets its own internal minimal acceptable duty cycle to:<br/>
        /// minPctDutyCycleAfterInh * max(other columns' duty cycles).<br/>
        /// On each iteration, any column whose duty cycle after inhibition falls below this computed value will get its internal boost factor increased.
        /// </summary>
        /// <param name="minPctActiveDutyCycles"></param>
        public void setMinPctActiveDutyCycles(double minPctActiveDutyCycles)
        {
            paramMap.Add(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, minPctActiveDutyCycles);
        }

        /// <summary>
        /// The period used to calculate duty cycles. Higher values make it take longer to respond to changes in boost or synPerConnectedCell. 
        /// Shorter values make it more unstable and likely to oscillate.
        /// </summary>
        /// <param name="dutyCyclePeriod"></param>
        public void setDutyCyclePeriod(int dutyCyclePeriod)
        {
            paramMap.Add(KEY.DUTY_CYCLE_PERIOD, dutyCyclePeriod);
        }

        /// <summary>
        /// The maximum overlap boost factor. Each column's overlap gets multiplied by a boost factor before it gets considered for inhibition.
        /// The actual boost factor for a column is number between 1.0 and maxBoost. A boost factor of 1.0 is used if the duty cycle 
        /// is &gt;= minOverlapDutyCycle, maxBoost is used if the duty cycle is 0, and any duty cycle in between is linearly extrapolated from 
        /// these 2 end points.
        /// </summary>
        /// <param name="maxBoost"></param>
        public void setMaxBoost(double maxBoost)
        {
            paramMap.Add(KEY.MAX_BOOST, maxBoost);
        }

        /**
         * {@inheritDoc}
         */
        #endregion

        public override String ToString()
        {
            StringBuilder result = new StringBuilder("{\n");
            StringBuilder spatialInfo = new StringBuilder();
            StringBuilder temporalInfo = new StringBuilder();
            StringBuilder otherInfo = new StringBuilder();
            //this.paramMap.keySet();
            foreach (string key in paramMap.Keys)
            {
                if (DEFAULTS_SPATIAL.ContainsKey(key))
                {
                    buildParamStr(spatialInfo, key);
                }
                else if (DEFAULTS_TEMPORAL.ContainsKey(key))
                {
                    buildParamStr(temporalInfo, key);
                }
                else
                {
                    buildParamStr(otherInfo, key);
                }
            }
            if (spatialInfo.Length > 0)
            {
                result.Append("\tSpatial: {\n").Append(spatialInfo).Append("\t}\n");
            }
            if (temporalInfo.Length > 0)
            {
                result.Append("\tTemporal: {\n").Append(temporalInfo).Append("\t}\n");
            }
            if (otherInfo.Length > 0)
            {
                result.Append("\tOther: {\n").Append(otherInfo).Append("\t}\n");
            }
            return result.Append("}").ToString();
        }

        private void buildParamStr(StringBuilder spatialInfo, string key)
        {
            Object value = paramMap[key];
            if (value is int[])
            {
                value = string.Join(",",
                    ((int[])value).Select(x => x.ToString()).ToArray());
                //value = ArrayUtils.intArrayToString(value);
            }
            spatialInfo.Append("\t\t").Append(key).Append(":").Append(value).Append("\n");
        }

        //public Parameters readForNetwork(FSTObjectInput in) throws Exception
        //{
        //    Parameters result = (Parameters)in.readObject(Parameters.class);
        //        return result;
        //    }

        //    public void writeForNetwork(FSTObjectOutput out) throws IOException
        //{
        //        out.writeObject(this, Parameters.class);
        //        out.close();
        //    }

        /**
         * Usage of {@link DeepEquals} in order to ensure the same hashcode
         * for the same equal content regardless of cycles.
         */

        public override int GetHashCode()
        {

            Random rnd = (Random)paramMap[KEY.RANDOM];

            unchecked
            {
                int hash = 17;

                // get hash code for all items in array
                foreach (var item in paramMap.Values)
                {
                    hash = hash * 23 + ((item != null) ? item.GetHashCode() : 0);
                }

                return hash;
            }

            //int hc = DeepEquals.deepHashCode(paramMap);
            //paramMap.Add(KEY.RANDOM, rnd);

            //return hc;

        }
    }
}

/**
 * This implementation skips over any native comparisons (i.e. "==")
 * because their hashcodes will not be equal.
 */
//@Override
//    public bool Equals(Object obj)
//    {
//        if (this == obj)
//            return true;
//        if (obj == null)
//            return false;
//        if (this.GetType() != obj.GetType())
//            return false;

//        Parameters other = (Parameters)obj;
//        if (paramMap == null)
//        {
//            if (other.paramMap != null)
//                return false;
//        }
//        else
//        {
//            Class <?>[] classArray = new Class[] { Object.class };
//        try {
//            for(KEY key : paramMap.keySet()) {
//                if(paramMap.get(key) == null || other.paramMap.get(key) == null) continue;

//                Class<?> thisValueClass = paramMap.get(key).getClass();
//Class<?> otherValueClass = other.paramMap.get(key).getClass();
//boolean isSpecial = isSpecial(key, thisValueClass);
//                if(!isSpecial && (thisValueClass.getMethod("equals", classArray).getDeclaringClass() != thisValueClass ||
//                    otherValueClass.getMethod("equals", classArray).getDeclaringClass() != otherValueClass)) {
//                        continue;
//                }else if(isSpecial) {
//                    if(int[].class.isAssignableFrom(thisValueClass)) {
//                        if(!Arrays.equals((int[]) paramMap.get(key), (int[]) other.paramMap.get(key))) return false;
//                    }else if(key == KEY.FIELD_ENCODING_MAP) {
//                        if(!DeepEquals.deepEquals(paramMap.get(key), other.paramMap.get(key))) {
//                            return false;
//                        }
//                    }
//                }else if(!other.paramMap.containsKey(key) || !paramMap.get(key).equals(other.paramMap.get(key))) {
//                    return false;
//                }
//            }
//        }catch(Exception e) { return false; }
//    }
//    return true;
//}

/**
 * Returns a flag indicating whether the type is an equality
 * special case.
 * @param key       the {@link KEY}
 * @param klazz     the class of the type being considered.
 * @return
 */
//    private bool isSpecial(KEY key, Class<?> klazz)
//{
//    if (int[].class.isAssignableFrom(klazz) ||
//            key == KEY.FIELD_ENCODING_MAP) {

//            return true;
//        }
//        return false;
//    }
//}

