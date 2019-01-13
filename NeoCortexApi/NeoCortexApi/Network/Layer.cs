using Microsoft.Extensions.Logging;
using NeoCortexApi.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoCortexApi
{
    /**
 * <p>
 * Implementation of the biological layer of a region in the neocortex. Here, a
 * {@code Layer} contains the physical structure (columns, cells, dendrites etc)
 * shared by a sequence of algorithms which serve to implement the predictive
 * inferencing present in this, the allegory to its biological equivalent.
 * </p>
 * <p>
 * <b>COMPOSITION:</b> A Layer is constructed with {@link Parameters} which
 * configure the behavior of most of the key algorithms a Layer may contain. It
 * is also <em>optionally</em> constructed with each of the algorithms in turn.
 * A Layer that includes an {@link Encoder} is always initially configured with
 * a {@link MultiEncoder}. The child encoders contained within the MultiEncoder
 * are configured from the Map included with the specified Parameters, keyed by
 * {@link Parameters.KEY#FIELD_ENCODING_MAP}.
 * </p>
 * <p>
 * A field encoding map consists of one map for each of the fields to be
 * encoded. Each individual map in the field encoding map contains the typical
 * {@link Encoder} parameters, plus a few "meta" parameters needed to describe
 * the field and its data type as follows:
 * </p>
 * 
 * <pre>
 *      Map&lt;String, Map&lt;String, Object&gt;&gt; fieldEncodings = new HashMap&lt;&gt;();
 *      
 *      Map&lt;String, Object&gt; inner = new HashMap&lt;&gt;();
 *      inner.put("n", n);
 *      inner.put("w", w);
 *      inner.put("minVal", min);
 *      inner.put("maxVal", max);
 *      inner.put("radius", radius);
 *      inner.put("resolution", resolution);
 *      inner.put("periodic", periodic);
 *      inner.put("clip", clip);
 *      inner.put("forced", forced);
 *      // These are meta info to aid in Encoder construction
 *      inner.put("fieldName", fieldName);
 *      inner.put("fieldType", fieldType); (see {@link FieldMetaType} for type examples)
 *      inner.put("encoderType", encoderType); (i.e. ScalarEncoder, SDRCategoryEncoder, DateEncoder...etc.)
 *      
 *      Map&lt;String, Object&gt; inner2 = new HashMap&lt;&gt;();
 *      inner.put("n", n);
 *      inner.put("w", w);
 *      inner.put("minVal", min);
 *      inner.put("maxVal", max);
 *      inner.put("radius", radius);
 *      inner.put("resolution", resolution);
 *      inner.put("periodic", periodic);
 *      inner.put("clip", clip);
 *      inner.put("forced", forced);
 *      // These are meta info to aid in Encoder construction
 *      inner.put("fieldName", fieldName);
 *      inner.put("fieldType", fieldType); (see {@link FieldMetaType} for type examples)
 *      inner.put("encoderType", encoderType); (i.e. ScalarEncoder, SDRCategoryEncoder, DateEncoder...etc.)
 *      
 *      fieldEncodings.put("consumption", inner);  // Where "consumption" is an example field name (field name is "generic" in above code)
 *      fieldEncodings.put("temperature", inner2);
 *      
 *      Parameters p = Parameters.getDefaultParameters();
 *      p.setParameterByKey(KEY.FIELD_ENCODING_MAP, fieldEncodings);
 * </pre>
 * 
 * For an example of how to create the field encodings map in a reusable way,
 * see NetworkTestHarness and its usage within the LayerTest class.
 * 
 * <p>
 * The following is an example of Layer construction with everything included
 * (i.e. Sensor, SpatialPooler, TemporalMemory, CLAClassifier, Anomaly
 * (computer))
 * 
 * <pre>
 * // See the test harness for more information
 * Parameters p = NetworkTestHarness.getParameters();
 * 
 * // How to merge (union) two {@link Parameters} objects. This one merges
 * // the Encoder parameters into default parameters.
 * p = p.union(NetworkTestHarness.getHotGymTestEncoderParams());
 * 
 * // You can overwrite parameters as needed like this
 * p.setParameterByKey(KEY.RANDOM, new MersenneTwister(42));
 * p.setParameterByKey(KEY.COLUMN_DIMENSIONS, new int[] { 2048 });
 * p.setParameterByKey(KEY.POTENTIAL_RADIUS, 200);
 * p.setParameterByKey(KEY.INHIBITION_RADIUS, 50);
 * p.setParameterByKey(KEY.GLOBAL_INHIBITIONS, true);
 * 
 * Map&lt;String, Object&gt; params = new HashMap&lt;&gt;();
 * params.put(KEY_MODE, Mode.PURE);
 * params.put(KEY_WINDOW_SIZE, 3);
 * params.put(KEY_USE_MOVING_AVG, true);
 * Anomaly anomalyComputer = Anomaly.create(params);
 * 
 * Layer&lt;?&gt; l = Network.createLayer(&quot;TestLayer&quot;, p).alterParameter(KEY.AUTO_CLASSIFY, true).add(anomalyComputer).add(new TemporalMemory()).add(new SpatialPooler())
 *                 .add(Sensor.create(FileSensor::create, SensorParams.create(Keys::path, &quot;&quot;, ResourceLocator.path(&quot;rec-center-hourly-small.csv&quot;))));
 * </pre>
 * 
 * 
 * 
 * 
 * @author David Ray
 */
    public class Layer<T>
    {
        #region Private Fields


        private ILogger logger;

        private bool isClosed;

        public Network ParentNetwork { get; set; }
        public string Name { get; private set; }

        protected Parameters parameters;

        protected SensorParameters sensorParams;

        protected Connections connections;

        protected int numColumns;

        protected Network parentNetwork;
        protected Region parentRegion;



        protected HTMSensor<?> sensor;
        protected MultiEncoder encoder;
        protected SpatialPooler spatialPooler;
        protected TemporalMemory temporalMemory;
        private Boolean autoCreateClassifiers;
        private Anomaly anomalyComputer;

        private readonly ConcurrentQueue<IObserver<IInference>> subscribers = new ConcurrentQueue<IObserver<IInference>>();
        private readonly PublishSubject<T> publisher = null;
        private readonly IObservable<IInference> userObservable;
        private readonly Subscription subscription;

        private readonly IInference currentInference;

        //FunctionFactory factory;

        /** Used to track and document the # of records processed */
        private int recordNum = -1;
        /** Keeps track of number of records to skip on restart */
        private int skip = -1;

        private String name;


        private volatile bool isHalted;
        private volatile bool isPostSerialized;
        protected volatile bool isLearn = true;

        private Layer<IInference> next;
        private Layer<IInference> previous;

        private readonly List<IObserver<IInference>> observers = new List<IObserver<IInference>>();
        private readonly ICheckPointOp<T> checkPointOp;
        private readonly List<IObserver<byte[]>> checkPointOpObservers = new List<IObserver<byte[]>>();

        #endregion
        public void setNetwork(Network network)
        {
            this.ParentNetwork = network;
        }


        /**
         * Creates a new {@code Layer} initialized with the specified algorithmic
         * components.
         * 
         * @param params                    A {@link Parameters} object containing configurations for a
         *                                  SpatialPooler, TemporalMemory, and Encoder (all or none may be used).
         * @param e                         (optional) The Network API only uses a {@link MultiEncoder} at
         *                                  the top level because of its ability to delegate to child encoders.
         * @param sp                        (optional) {@link SpatialPooler}
         * @param tm                        (optional) {@link TemporalMemory}
         * @param autoCreateClassifiers     (optional) Indicates that the {@link Parameters} object
         *                                  contains the configurations necessary to create the required encoders.
         * @param a                         (optional) An {@link Anomaly} computer.
         */
        public Layer(Parameters parameters, IHtmModule module)
        {

        }

        public Layer(Parameters parameters, ICollection<IHtmModule> modules)
        //MultiEncoder e = null, SpatialPooler sp = null, TemporalMemory tm = null, Boolean autoCreateClassifiers = null, Anomaly a = null)
        {
            // Make sure we have a valid parameters object
            if (parameters == null || modules == null || modules.Count == 0) {
                throw new ArgumentException("No parameters specified.");
            }

            var mulEncoder = modules.FirstOrDefault(m=>m.GetType() == typeof(MultiEncoder));
            // Check to see if the Parameters include the encoder configuration.
            if (parameters[KEY.FIELD_ENCODING_MAP] == null && modules != null) {
                throw new ArgumentException("The passed in Parameters must contain a field encoding map " +
                    "specified by org.numenta.nupic.Parameters.KEY.FIELD_ENCODING_MAP");
            }

            this.parameters = parameters;
            this.encoder = e;
            this.spatialPooler = sp;
            this.temporalMemory = tm;
            this.autoCreateClassifiers = autoCreateClassifiers;
            this.anomalyComputer = a;

            connections = new Connections();
            //factory = new FunctionFactory();

            observableDispatch = createDispatchMap();

            initializeMask();

           
                logger?.LogDebug("Layer successfully created containing: {}{}{}{}{}",
                    (encoder == null ? "" : "MultiEncoder,"),
                    (spatialPooler == null ? "" : "SpatialPooler,"),
                    (temporalMemory == null ? "" : "TemporalMemory,"),
                    (autoCreateClassifiers == null ? "" : "Auto creating Classifiers for each input field."),
                    (anomalyComputer == null ? "" : "Anomaly"));
      
        }


        /**
      * Processes a single element, sending the specified input up the configured
      * chain of algorithms or components within this {@code Layer}; resulting in
      * any {@link Subscriber}s or {@link Observer}s being notified of results
      * corresponding to the specified input (unless a {@link SpatialPooler}
      * "primer delay" has been configured).
      * 
      * The first input to the Layer invokes a method to resolve the transformer
      * at the bottom of the input chain, therefore the "type" (&lt;T&gt;) of the
      * input cannot be changed once this method is called for the first time.
      * 
      * @param t     the input object who's type is generic.
      */
        public void Compute(T t)
        {
            if (!isClosed)
            {
                close();
            }

            increment();

            if (!dispatchCompleted())
            {
                completeDispatch(t);
            }

            publisher.onNext(t);
        }

        internal ICheckPointOp<byte[]> delegateCheckPointCall()
        {
            if (ParentNetwork != null)
            {
                return ParentNetwork.getCheckPointOperator();
            }
            return null;
        }

        public Layer<T> close()
        {
            if (this.isClosed)
            {
                logger.LogWarning("Close called on Layer " + this.Name + " which is already closed.");
                return this;
            }

            parameters.apply(connections);

            if (sensor != null)
            {
                encoder = encoder == null ? sensor.getEncoder() : encoder;
                sensor.initEncoder(this.parameters);
                connections.setNumInputs(encoder.getWidth());
                if (parentNetwork != null && parentRegion != null)
                {
                    parentNetwork.setSensorRegion(parentRegion);

                    Object supplier;
                    if ((supplier = sensor.getSensorParams().get("ONSUB")) != null)
                    {
                        if (supplier instanceof PublisherSupplier) {
                            ((PublisherSupplier)supplier).setNetwork(parentNetwork);
                            parentNetwork.setPublisher(((PublisherSupplier)supplier).get());
                        }
                    }
                }
            }

            // Create Encoder hierarchy from definitions & auto create classifiers
            // if specified
            if (encoder != null)
            {
                if (encoder.getEncoders(encoder) == null || encoder.getEncoders(encoder).size() < 1)
                {
                    if (this.parameters.get(KEY.FIELD_ENCODING_MAP) == null || ((Map<String, Map<String, Object>>)params.get(KEY.FIELD_ENCODING_MAP)).size() < 1) {
                        LOGGER.error("No field encoding map found for specified MultiEncoder");
                        throw new IllegalStateException("No field encoding map found for specified MultiEncoder");
                    }

                    encoder.addMultipleEncoders((Map<String, Map<String, Object>>)this.parameters[KEY.FIELD_ENCODING_MAP]);
                }

                // Make the declared column dimensions match the actual input
                // dimensions retrieved from the encoder
                int product = 0, inputLength = 0, columnLength = 0;
                if (((inputLength = ((int[])this.parameters[KEY.INPUT_DIMENSIONS]).length) != 
                    (columnLength = ((int[])this.parameters[KEY.COLUMN_DIMENSIONS]).length))
                            || encoder.getWidth() != (product = ArrayUtils.product((int[])this.parameters[KEY.INPUT_DIMENSIONS]))) {

                    LOGGER.warn("The number of Input Dimensions (" + inputLength + ") != number of Column Dimensions " + "(" + columnLength + ") --OR-- Encoder width (" + encoder.getWidth()
                                    + ") != product of dimensions (" + product + ") -- now attempting to fix it.");

                    int[] inferredDims = inferInputDimensions(encoder.getWidth(), columnLength);
                    if (inferredDims != null && inferredDims.length > 0 && encoder.getWidth() == ArrayUtils.product(inferredDims))
                    {
                        LOGGER.info("Input dimension fix successful!");
                        LOGGER.info("Using calculated input dimensions: " + Arrays.toString(inferredDims));
                    }

                params.setInputDimensions(inferredDims);
                    connections.setInputDimensions(inferredDims);
                }
            }

            autoCreateClassifiers = autoCreateClassifiers != null && (autoCreateClassifiers | (Boolean)this.parameters[KEY.AUTO_CLASSIFY]);

            if (autoCreateClassifiers != null && autoCreateClassifiers.booleanValue() && (factory.inference.getClassifiers() == null || factory.inference.getClassifiers().size() < 1))
            {
                factory.inference.classifiers(makeClassifiers(encoder == null ? parentNetwork.getEncoder() : encoder));

                // Note classifier addition by setting content mask
                algo_content_mask |= CLA_CLASSIFIER;
            }

            // We must adjust this Layer's inputDimensions to the size of the input
            // received from the previous Region's output vector.
            if (parentRegion != null && parentRegion.getUpstreamRegion() != null)
            {
                int[] upstreamDims = new int[] { calculateInputWidth() };
            params.setInputDimensions(upstreamDims);
                connections.setInputDimensions(upstreamDims);
            }
            else if (parentRegion != null && parentNetwork != null
                  && parentRegion.equals(parentNetwork.getSensorRegion()) && encoder == null && spatialPooler != null)
            {
                Layer curr = this;
                while ((curr = curr.getPrevious()) != null)
                {
                    if (curr.getEncoder() != null)
                    {
                        int[] dims = (int[])curr.getParameters()[KEY.INPUT_DIMENSIONS];
                    this.parameters.setInputDimensions(dims);
                        connections.setInputDimensions(dims);
                    }
                }
            }

            // Let the SpatialPooler initialize the matrix with its requirements
            if (spatialPooler != null)
            {
                // The exact dimensions don't have to be the same but the number of
                // dimensions do!
                int inputLength, columnLength = 0;
                if ((inputLength = ((int[])this.parameters[KEY.INPUT_DIMENSIONS]).length) !=
                     (columnLength = ((int[])this.parameters[KEY.COLUMN_DIMENSIONS]).length)) {

                    LOGGER.error("The number of Input Dimensions (" + inputLength + ") is not same as the number of Column Dimensions " +
                        "(" + columnLength + ") in Parameters! - SpatialPooler not initialized!");

                    return this;
                }
                spatialPooler.init(connections);
            }

            // Let the TemporalMemory initialize the matrix with its requirements
            if (temporalMemory != null)
            {
                TemporalMemory.init(connections);
            }

            this.numColumns = connections.getNumColumns();

            this.isClosed = true;

            LOGGER.debug("Layer " + name + " content initialize mask = " + Integer.toBinaryString(algo_content_mask));

            return this;
        }
    }
}
