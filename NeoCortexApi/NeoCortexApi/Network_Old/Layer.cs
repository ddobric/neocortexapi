using Microsoft.Extensions.Logging;
using NeoCortexApi.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Encoders;
using NeoCortexApi.Sensors;
using NeoCortexApi.Network;

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

        private CortexNetworkContext context;

        private ILogger logger;

        private bool isClosed;

        public CortexNetwork ParentNetwork { get; set; }
        public string Name { get; private set; }

        protected Parameters parameters;

        protected SensorParameters sensorParams;

        protected Connections connections;

        protected int numColumns;

        protected CortexNetwork parentNetwork;
        protected Region parentRegion;

        Dictionary<Type, IObservable<ManualInput>> observableDispatch = new Dictionary<Type, IObservable<ManualInput>>();

        //protected HTMSensor<T> sensor;
        protected ISensor<T> sensor;
        //protected MultiEncoder<T> encoder;
        protected SpatialPooler spatialPooler;
        protected TemporalMemory temporalMemory;
        private Anomaly anomalyComputer;

        private readonly List<IHtmModule> modules = new List<IHtmModule>();

        private bool? autoCreateClassifiers;

        private readonly List<IObserver<IInference>> subscribers = new List<IObserver<IInference>>();
        private PublisherSubject<IInference> publisher = null;
        private readonly IObservable<IInference> userObservable;
        private readonly Subscription<T> subscription;

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

        private Layer<IInference> nextLayer;

        public Layer<IInference> NextLayer { get => nextLayer; set => nextLayer = value; }

        private Layer<IInference> previousLayer;
        public Layer<IInference> PreviousLayer { get => previousLayer; set => previousLayer = value; }

        private readonly List<IObserver<IInference>> observers = new List<IObserver<IInference>>();
        private readonly ICheckPointOp<T> checkPointOp;
        private readonly List<IObserver<byte[]>> checkPointOpObservers = new List<IObserver<byte[]>>();

        ManualInput inference = new ManualInput();

        #endregion

        #region Properties

        /// <summary>
        /// Gets true if the HTM module of specified type in the list of modules.
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns></returns>
        public bool HasModule<TModule>() where TModule : IHtmModule
        {
            return this.modules.FirstOrDefault(m => m.GetType() == typeof(TModule)) != null;
        }

        /// <summary>
        /// Returns true if layer contains 
        /// </summary>
        public bool ContainsAlgorithm
        {
            get
            {
                return this.modules.OfType<IHtmAlgorithm>().Count() > 0;
            }
        }

        /// <summary>
        /// Gets list of modules of layer as descriptive text.
        /// </summary>
        private string LayerInfo
        {
            get
            {
                StringBuilder sb = new StringBuilder("Modules: ");

                foreach (var item in this.modules)
                {
                    sb.Append(item.GetType().Name);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        /**
* Returns a flag indicating whether we've connected the first observable in
* the sequence (which lazily does the input type of &lt;T&gt; to
* {@link Inference} transformation) to the Observables connecting the rest
* of the algorithm components.
* 
* @return flag indicating all observables connected. True if so, false if
*         not
*/
        private bool DispatchCompleted
        {
            get
            {
                return observableDispatch == null;
            }
        }




        #endregion


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
        public Layer(string name = null, CortexNetwork network = null, Parameters parameters = null, IHtmModule module = null, bool autoCreateClassifiers = false, CortexNetworkContext context = null) :
            this(name, network, parameters, new List<IHtmModule> { module }, autoCreateClassifiers, context)
        {

        }

        public Layer(string name = null, CortexNetwork network = null, Parameters parameters = null, ICollection<IHtmModule> modules = null, bool? autoCreateClassifiers = null, CortexNetworkContext context = null)
        {
            if (name == null)
                name = $"[Layer {DateTime.Now.Ticks}]";

            this.context = context;

            // Make sure we have a valid parameters object
            if (parameters == null || modules == null || modules.Count == 0)
            {
                throw new ArgumentException("No parameters specified.");
            }

            var mulEncoder = modules.FirstOrDefault(m => m.GetType() == typeof(MultiEncoder<>));
            // Check to see if the Parameters include the encoder configuration.
            if (parameters[KEY.FIELD_ENCODING_MAP] == null && modules != null)
            {
                throw new ArgumentException("The passed in Parameters must contain a field encoding map " +
                    "specified by org.numenta.nupic.Parameters.KEY.FIELD_ENCODING_MAP");
            }

            this.name = name;
            this.parameters = parameters;
            this.parentNetwork = network;

            foreach (var module in modules)
            {
                this.modules.Add(module);
            }

            //this.encoder = e;
            //this.spatialPooler = sp;
            //this.temporalMemory = tm;
            //this.anomalyComputer = a;

            this.autoCreateClassifiers = autoCreateClassifiers;

            connections = new Connections();
            //factory = new FunctionFactory();

            observableDispatch = createDispatchMap();

            //initializeMask();


            logger?.LogDebug($"Layer successfully created containing: {LayerInfo}");

            if (this.autoCreateClassifiers != null)
                logger?.LogDebug("Auto creating Classifiers for each input field.");
        }


        /// <summary>
        /// Finalizes the initialization in one method call so that side effect
        /// operations to share objects and other special initialization tasks can
        /// happen all at once in a central place for maintenance ease.
        /// </summary>
        /// <returns></returns>
        public Layer<T> CloseInit()
        {
            if (this.isClosed)
            {
                logger.LogWarning("Close called on Layer " + this.Name + " which is already closed.");
                return this;
            }

            parameters.apply(connections);

            if (sensor != null)
            {
                //encoder = encoder == null ? sensor.Encoder : encoder;
                //sensor.initEncoder(this.parameters);
                connections.NumInputs = this.sensor.InputWidth;

                if (parentNetwork != null && parentRegion != null)
                {
                    parentNetwork.SensorRegion = parentRegion;

                    Object supplier;
                    if ((supplier = sensor.getSensorParams()["ONSUB"]) != null)
                    {
                        if (supplier is  PublisherSupplier) {
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
                //if (encoder.getEncoders(encoder) == null || encoder.getEncoders(encoder).size() < 1)
                //{
                //    if (this.parameters[KEY.FIELD_ENCODING_MAP] == null || ((Dictionary<String, Dictionary<String, Object>>)parameters.get(KEY.FIELD_ENCODING_MAP)).size() < 1) {
                //        logger.LogError("No field encoding map found for specified MultiEncoder");
                //        throw new InvalidOperationException("No field encoding map found for specified MultiEncoder");
                //    }

                //    encoder.addMultipleEncoders((Dictionary<String, Dictionary<String, Object>>)this.parameters[KEY.FIELD_ENCODING_MAP]);
                //}

                // Make the declared column dimensions match the actual input
                // dimensions retrieved from the encoder
                int product = 0, inputLength = 0, columnLength = 0;
                if (((inputLength = ((int[])this.parameters[KEY.INPUT_DIMENSIONS]).Length) !=
                    (columnLength = ((int[])this.parameters[KEY.COLUMN_DIMENSIONS]).Length))
                            || encoder.getWidth() != (product = ArrayUtils.product((int[])this.parameters[KEY.INPUT_DIMENSIONS])))
                {

                    logger.LogWarning("The number of Input Dimensions (" + inputLength + ") != number of Column Dimensions " + "(" + columnLength + ") --OR-- Encoder width (" + encoder.getWidth()
                                    + ") != product of dimensions (" + product + ") -- now attempting to fix it.");

                    int[] inferredDims = inferInputDimensions(encoder.getWidth(), columnLength);
                    if (inferredDims != null && inferredDims.length > 0 && encoder.getWidth() == ArrayUtils.product(inferredDims))
                    {
                        this.logger.LogInformation("Input dimension fix successful!");
                        this.logger.LogInformation("Using calculated input dimensions: " + Arrays.toString(inferredDims));
                    }

                parameters.setInputDimensions(inferredDims);
                    connections.setInputDimensions(inferredDims);
                }
            }

            // TODO
            //autoCreateClassifiers = autoCreateClassifiers != null && (autoCreateClassifiers | (Boolean)this.parameters[KEY.AUTO_CLASSIFY]);

            //if (autoCreateClassifiers != null && autoCreateClassifiers.booleanValue() && (factory.inference.getClassifiers() == null || factory.inference.getClassifiers().size() < 1))
            //{
            //    factory.inference.classifiers(makeClassifiers(encoder == null ? parentNetwork.getEncoder() : encoder));

            //    // Note classifier addition by setting content mask
            //    algo_content_mask |= CLA_CLASSIFIER;
            //}

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
                Layer<T> curr = this;
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
                if ((inputLength = ((int[])this.parameters[KEY.INPUT_DIMENSIONS]).Length) !=
                     (columnLength = ((int[])this.parameters[KEY.COLUMN_DIMENSIONS]).Length))
                {

                    this.logger.LogError("The number of Input Dimensions (" + inputLength + ") is not same as the number of Column Dimensions " +
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

            this.numColumns = connections.getNumColumns;

            this.isClosed = true;

            this.logger?.LogDebug("Layer " + name + " content initialize mask = " + BitConverter.bin(algo_content_mask));

            return this;
        }

        /// <summary>
        /// Registers specified subscriber.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>

        public ISubscription<IInference> Subscribe(IObserver<IInference> subscriber)
        {
            // This will be called again after the Network is halted so we have to prepare
            // for rebuild of the Observer chain
            //if (isHalted)
            //{
            //    clearSubscriberObserverLists();
            //}

            if (subscriber == null)
            {
                throw new ArgumentException("Subscriber cannot be null.");
            }


            this.subscribers.Add(subscriber);

            return this.publisher.Subscribe(subscriber) as ISubscription<IInference>;
        }

     

        /// <summary>
        /// Notify all subscribers that processing has completed succesfully.
        /// </summary>
        public void NotifyComplete()
        {
            foreach (var subscriber in this.subscribers)
            {
                subscriber.OnCompleted();
            }

            foreach (var observer in this.observers)
            {
                observer.OnCompleted();
            }

            publisher.OnCompleted();
        }


        /// <summary>
        /// Increments the current record sequence number.
        /// </summary>
        /// <returns></returns>
        public Layer<T> increment()
        {
            if (skip > -1)
            {
                --skip;
            }
            else
            {
                ++recordNum;
            }
            return this;
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
                CloseInit();
            }

            increment();

            if (observableDispatch != null)
            {
                completeDispatch(t);
            }

            publisher.OnNext(t);
        }

        /**
        * We cannot create the {@link Observable} sequence all at once because the
        * first step is to transform the input type to the type the rest of the
        * sequence uses (Observable<b>&lt;Inference&gt;</b>). This can only happen
        * during the actual call to {@link #compute(Object)} which presents the
        * input type - so we create a map of all types of expected inputs, and then
        * connect the sequence at execution time; being careful to only incur the
        * cost of sequence assembly on the first call to {@link #compute(Object)}.
        * After the first call, we dispose of this map and its contents.
        * 
        * @return the map of input types to {@link Transformer}
*/

        private Dictionary<Type, IObservable<ManualInput>> createDispatchMap()
        {
            Dictionary<Type, IObservable<ManualInput>> observableDispatch = new Dictionary<Type, IObservable<ManualInput>>();

            this.publisher = new PublisherSubject<IInference>();
            var fDict = new Func<PublisherSubject<IInference>, object, IObservable<ManualInput>>(
                (pub, inp) => { return null; }
                );


            var fInt = new Func<PublisherSubject<IInference>, int[], IObservable<ManualInput>>(
              (pub, inp) =>
              {

                  inference.RecordNum = this.recordNum;
                  inference.LayerInput = inp;

                  return this.inference;
              }
              );


            observableDispatch.Add((Class<T>)Map.class, factory.createMultiMapFunc(publisher));
        observableDispatch.put((Class<T>) ManualInput.class, factory.createManualInputFunc(publisher));
        observableDispatch.put((Class<T>) String[].class, factory.createEncoderFunc(publisher));
        observableDispatch.put((Class<T>)int[].class, factory.createVectorFunc(publisher));

        return observableDispatch;
    }





    internal ICheckPointOp<byte[]> delegateCheckPointCall()
    {
        if (ParentNetwork != null)
        {
            return ParentNetwork.getCheckPointOperator();
        }
        return null;
    }



    public void setNetwork(Network network)
    {
        this.ParentNetwork = network;
    }

    private IHtmModule GetModule<TModule>()
    {
        return this.modules.FirstOrDefault(m => m.GetType() == typeof(TModule));
    }

}

}
