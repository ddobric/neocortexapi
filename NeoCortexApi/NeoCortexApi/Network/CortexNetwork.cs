using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    /**
 * <p>
 * A {@code Network} is the fundamental component of the HTM.java Network API.
 * It is comprised of {@link Region}s which are in turn comprised of {@link Layer}s;
 * each Layer directly containing one or more algorithm or computational components
 * such (i.e. {@link Sensor}, {@link MultiEncoder}, {@link SpatialPooler}, 
 * {@link TemporalMemory}, {@link CLAClassifier} etc.)
 * </p>
 * <p>
 * Networks in HTM.java are extremely easy to compose. For instance, here is an example 
 * of a network which contains everything:
 * <pre>
 * Parameters p = NetworkTestHarness.getParameters();
 * p = p.union(NetworkTestHarness.getNetworkDemoTestEncoderParams());
 * 
 * Network network = Network.create("test network", p)
 *     .add(Network.createRegion("r1")
 *         .add(Network.createLayer("1", p)
 *             .alterParameter(KEY.AUTO_CLASSIFY, Boolean.TRUE)
 *             .add(Anomaly.create())
 *             .add(new TemporalMemory())
 *             .add(new SpatialPooler())
 *             .add(Sensor.create(FileSensor::create, SensorParams.create(
 *                 Keys::path, "", ResourceLocator.path("rec-center-hourly.csv"))))));
 * </pre>
 *                 
 * <p>
 * As you can see, {@code Networks} can be composed in "fluent" style making their
 * declaration much more concise.
 * 
 * While the above Network contains only 1 Region and only 1 Layer, Networks with many Regions
 * and Layers within them; may be composed. For example:
 * <pre>
 * Connections cons = new Connections();
 * 
 * Network network = Network.create("test network", p)
 *     .add(Network.createRegion("r1")
 *         .add(Network.createLayer("1", p)
 *             .add(Anomaly.create())
 *             .add(new TemporalMemory())
 *             .add(new SpatialPooler())))
 *     .add(Network.createRegion("r2")
 *         .add(Network.createLayer("1", p)
 *             .using(cons)
 *             .alterParameter(KEY.AUTO_CLASSIFY, Boolean.TRUE)
 *             .add(new TemporalMemory()))                    
 *         .add(Network.createLayer("2", p)
 *             .using(cons)
 *             .add(new SpatialPooler())
 *             .add(Sensor.create(FileSensor::create, SensorParams.create(
 *                 Keys::path, "", ResourceLocator.path("rec-center-hourly.csv")))))
 *         .connect("1", "2")) // Tell the Region to connect the two layers {@link Region#connect(String, String)}.
 *     .connect("r1", "r2");   // Tell the Network to connect the two regions {@link Network#connect(String, String)}.
 * </pre>
 * As you can see above, a {@link Connections} object is being shared among Region "r2"'s inner layers 
 * via the use of the {@link Layer#using(Connections)} method. Additionally, {@link Parameters} may be
 * altered in place without changing the values in the original Parameters object, by calling {@link Layer#alterParameter(KEY, Object)}
 * as seen in the above examples.  
 * 
 * <p>
 * Networks can be "observed", meaning they can return an {@link Observable} object
 * which can be operated on in the map-reduce pattern of usage; and they can be 
 * subscribed to, to receive {@link Inference} objects as output from every process
 * cycle. For instance:
 * <pre>
 * network.observe().subscribe(new Subscriber&lt;Inference&gt;() {
 *      public void onCompleted() { System.out.println("Input Completed!"); }
 *      public void onError(Throwable e) { e.printStackTrace(); }
 *      public void onNext(Inference i) {
 *          System.out.println(String.format("%d: %s", i.getRecordNum(), Arrays.toString(i.getSDR())));
 *      }
 * });
 * </pre>
 * 
 * <p>
 * Likewise, each Region within a Network may be "observed" and/or subscribed to; and each
 * Layer may be observed and subscribed to as well - for those instances where you would like 
 * to obtain the processing from an individual component within the network for use outside 
 * the network in some other area of your application. To find and subscribe to individual
 * components try:
 * <pre>
 * Network network = ...
 * 
 * Region region = network.lookup("&lt;region name&gt;");
 * region.observe().subscribe(new Subscriber&lt;Inference&gt;() {
 *     public void onCompleted() { System.out.println("Input Completed!"); }
 *     public void onError(Throwable e) { e.printStackTrace(); }
 *     public void onNext(Inference i) {
 *         int[] sdr = i.getSDR();
 *         do something...
 *     }    
 * }
 * 
 * Layer l2_3 = region.lookup("&lt;layer name&gt;");
 * l2_3.observe().subscribe(new Subscriber&lt;Inference&gt;() {
 *     public void onCompleted() { System.out.println("Input Completed!"); }
 *     public void onError(Throwable e) { e.printStackTrace(); }
 *     public void onNext(Inference i) {
 *         int[] sdr = i.getSDR();
 *         do something...
 *     }    
 * }
 * </pre>
 * 
 * In addition there are many usage examples to be found in the {@link org.numenta.nupic.examples.napi.hotgym} package
 * where there are tests which may be examined for details, and the {@link NetworkAPIDemo}
 * 
 * @author David Ray
 * @see Region
 * @see Layer
 * @see Inference
 * @see ManualInput
 * @see NetworkAPIDemo
 */
    public class CortexNetwork
    {
        private ILogger logger;

        private String name;
        private object[] parameters;
        //private HTMSensor<?> sensor;
        //private MultiEncoder encoder;
        private Region head;
        private Region tail;
        private Region sensorRegion;
        //private volatile Publisher publisher;

        private volatile bool isLearn = true;
        private volatile bool isThreadRunning;

        private List<Region> regions = new List<Region>();

        public delegate byte[] CheckPointFnc(CortexNetwork net);

        /** Stored check pointer function */
        private CheckPointFnc checkPointFunction;

        bool shouldDoHalt = true;

        /// <summary>
        /// Sets a reference to the Region which contains the Sensor
        /// (if any).</summary>
        /// <param name="r"></param>
        public Region SensorRegion { get; set; }
        

        CortexNetwork() { }

        /**
         * Creates a new {@link Network}
         * 
         * Warning: name cannot be null or empty
         * 
         * @param name
         * @param parameters
         */
        public CortexNetwork(String name, object[] parameters)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("All Networks must have a name. " +
                    "Increases digestion, and overall happiness!");
            }
            this.name = name;
            this.parameters = parameters;
            if (parameters == null)
            {
                throw new ArgumentException("Network Parameters were null.");
            }
        }

        /**
         * Creates and returns an implementation of {@link Network}
         * 
         * Warning: name cannot be null or empty
         * 
         * @param name
         * @param parameters
         * @return
         */
        public static Network create(String name, object[] parameters)
        {
            return new Network(name, parameters);
        }

        /**
         * Creates and returns a child {@link Region} of this {@code Network}
         * 
         * @param   name    The String identifier for the specified {@link Region}
         * @return
         */
        public static Region createRegion(String name)
        {
            Network.CheckName(name);

            Region r = new Region(name, null);
            return r;
        }

        private static void CheckName(string name)
        {
            if (name.IndexOf(":") != -1)
            {
                throw new ArgumentException("\":\" is a reserved character.");
            }

        }

        /**
         * Creates a {@link Layer} to hold algorithmic components and returns
         * it.
         * 
         * @param name  the String identifier for the specified {@link Layer}
         * @param p     the {@link Parameters} to use for the specified {@link Layer}
         * @return
         */
        //@SuppressWarnings("rawtypes")
        public static Layer createLayer(String name, object[] p)
        {
            Network.CheckName(name);
            return new Layer(name, null, p);
        }

        /**
         * DO NOT CALL THIS METHOD! FOR INTERNAL USE ONLY!
         * {@inheritDoc}
         */
        //@SuppressWarnings("unchecked")
        //@Override
        //public Network preSerialize()
        //{
        //    if (shouldDoHalt && isThreadRunning)
        //    {
        //        halt();
        //    }
        //    else
        //    { // Make sure "close()" has been called on the Network
        //        if (regions.size() == 1)
        //        {
        //            this.tail = regions.get(0);
        //        }
        //        tail.close();
        //    }

        //    regions.stream().forEach(r->r.preSerialize());
        //    return this;
        //}

        /**
         * DO NOT CALL THIS METHOD! FOR INTERNAL USE ONLY!
         * {@inheritDoc}
         */
        //@SuppressWarnings("unchecked")
        //@Override
        //public Network postDeSerialize()
        //{
        //    regions.stream().forEach(r->r.setNetwork(this));
        //    regions.stream().forEach(r->r.postDeSerialize());

        //    // Connect Layer Observable chains (which are transient so we must 
        //    // rebuild them and their subscribers)
        //    if (isMultiRegion())
        //    {
        //        Region curr = head;
        //        Region nxt = curr.getUpstreamRegion();
        //        do
        //        {
        //            curr.connect(nxt);
        //        } while ((curr = nxt) != null && (nxt = nxt.getUpstreamRegion()) != null);
        //    }

        //    return this;
        //}

        /**
         * INTERNAL METHOD: DO NOT CALL
         * 
         * Called from {@link Layer} to execute a check point from within the scope of 
         * this {@link Network}
         * checkPointFunction
         * @return  the serialized {@code Network} in byte array form.
         */
        byte[] internalCheckPointOp()
        {
            shouldDoHalt = false;
            byte[] serializedBytes = (byte[])checkPointFunction.Invoke(this);
            shouldDoHalt = true;
            return serializedBytes;
        }

        /**
         * Sets the reference to the check point function.
         * @param f function which executes check point logic.
         */
        //@SuppressWarnings("unchecked")
        public void setCheckPointFunction<T, R>(CheckPointFnc fnc)
        {
            this.checkPointFunction = fnc;
        }

        /**
         * USED INTERNALLY, DO NOT CALL
         * Returns an {@link rx.Observable} operator that when subscribed to, invokes an operation
         * that stores the state of this {@code Network} while keeping the Network up and running.
         * The Network will be stored at the pre-configured location (in binary form only, not JSON).
         * 
         * @return  the {@link CheckPointOp} operator 
         */
        public ICheckPointOp<byte[]> getCheckPointOperator()
        {
            logger.LogDebug("Network [" + getName() + "] called checkPoint() at: " + (new DateTime()));

            if (regions.Count == 1)
            {
                this.tail = regions[0];
            }
            return tail.getCheckPointOperator();
        }

        /**
         * Restarts this {@code Network}. The network will run from the previous save point
         * of the stored Network.
         * 
         * @see {@link #restart(boolean)} for a start at "saved-index" behavior explanation. 
         */
        public void restart()
        {
            restart(true);
        }

        /**
         * Restarts this {@code Network}. If the "startAtIndex" flag is true, the Network
         * will start from the last record number (plus 1) at which the Network was saved -
         * continuing on from where it left off. The Network will achieve this by rebuilding
         * the underlying Stream (if necessary, i.e. not for {@link ObservableSensor}s) and skipping 
         * the number of records equal to the stored record number plus one, continuing from where it left off.
         * 
         * @param startAtIndex  flag indicating whether to start this {@code Network} from
         *                      its previous save point.
         */
        public void restart(bool startAtIndex)
        {
            if (regions.Count < 1)
            {
                throw new InvalidOperationException("Nothing to start - 0 regions");
            }

            Region tail = regions[0];
            Region upstream = tail;
            while ((upstream = upstream.getUpstreamRegion()) != null)
            {
                tail = upstream;
            }

            // Record thread start
            this.isThreadRunning = tail.restart(startAtIndex);
        }

        /**
         * <p>
         * DO NOT CALL THIS METHOD!
         * </p><p>
         * Called internally by an {@link ObservableSensor}'s factory method's creation of a new 
         * {@code ObservableSensor}. This would usually happen following a halt or
         * deserialization.
         * </p>
         * @param p  the new Publisher created upon reconstitution of a new ObservableSensor  
         */
        void setPublisher(Publisher p)
        {
            this.publisher = p;
            publisher.setNetwork(this);
        }

        /**
         * Returns the new {@link Publisher} created after halt or deserialization
         * of this {@code Network}, when a new Publisher must be created.
         * 
         * @return      the new Publisher created after deserialization or halt.
         * @see #getPublisherSupplier()
         */
        public Publisher getPublisher()
        {
            if (publisher == null)
            {
                throw new InvalidOperationException("A Supplier must be built first. " +
                    "please see Network.getPublisherSupplier()");
            }
            return publisher;
        }

        /**
         * Returns a flag indicating whether this {@code Network} contain multiple
         * {@link Region}s.
         * 
         * @return  true if so, false if not.
         */
        public bool isMultiRegion()
        {
            return regions.Count > 1;
        }

        /**
         * Returns the String identifier for this {@code Network}
         * @return
         */
        public String getName()
        {
            return name;
        }

        /**
         * Calling this 
         * method will start the main engine thread which pulls in data
         * from the connected {@link Sensor}(s).
         */
        public void start()
        {
            if (regions.Count < 1)
            {
                throw new InvalidOperationException("Nothing to start - 0 regions");
            }

            Region tail = regions[0];
            Region upstream = tail;
            while ((upstream = upstream.getUpstreamRegion()) != null)
            {
                tail = upstream;
            }

            // Record thread start
            this.isThreadRunning = tail.start();
        }

        /**
         * Returns a flag indicating that the {@code Network} has an {@link Observable}
         * running on a thread.
         * 
         * @return  a flag indicating if threaded.
         */
        public bool isThreadedOperation()
        {
            return this.isThreadRunning;
        }

        /**
         * Halts this {@code Network}, stopping all threads and closing
         * all {@link SensorFactory} connections to incoming data, freeing up 
         * any resources associated with the input connections.
         */
        public void halt()
        {
            // Call onComplete if using an ObservableSensor to complete the stream output.
            if (publisher != null)
            {
                publisher.onComplete();
            }

            if (regions.size() == 1)
            {
                this.tail = regions.get(0);
            }
            tail.halt();
        }

        /**
         * Returns a flag indicating whether this Network has a Region
         * whose tail (input {@link Layer}) is halted.
         * @return  true if so, false if not
         */
        public bool isHalted()
        {
            if (regions.Count == 1)
            {
                this.tail = regions.get(0);
            }
            return tail.isHalted();
        }

        /**
         * Returns the index of the last record processed.
         * 
         * @return  the last recordNum processed
         */
        public int getRecordNum()
        {
            if (regions.size() == 1)
            {
                this.tail = regions.get(0);
            }
            return tail.getTail().getRecordNum();
        }

        /**
         * Sets the learning mode.
         * @param isLearn
         */
        public void setLearn(bool isLearn)
        {
            this.isLearn = isLearn;
        for (Region r in regions)
            {
                r.setLearn(isLearn);
            }
        }

        /**
         * Returns the learning mode setting.
         * @return
         */
        public bool isLearn()
        {
            return isLearn;
        }

        /**
         * Finds any {@link Region} containing a {@link Layer} which contains a {@link TemporalMemory} 
         * and resets them.
         */
        public void reset()
        {
        for (Region r in regions)
            {
                r.reset();
            }
        }

        /**
         * Resets the recordNum in all {@link Region}s.
         */
        public void resetRecordNum()
        {
        for (Region r in regions)
            {
                r.resetRecordNum();
            }
        }

        /**
         * Returns an {@link Observable} capable of emitting {@link Inference}s
         * which contain the results of this {@code Network}'s processing chain.
         * @return
         */
        public Observable<Inference> observe()
        {
            if (regions.size() == 1)
            {
                this.head = regions.get(0);
            }
            return head.observe();
        }

        /**
         * Returns the top-most (last in execution order from
         * bottom to top) {@link Region} in this {@code Network}
         * 
         * @return
         */
        public Region getHead()
        {
            if (regions.size() == 1)
            {
                this.head = regions.get(0);
            }
            return this.head;
        }

        /**
         * Returns the bottom-most (first in execution order from
         * bottom to top) {@link Region} in this {@code Network}
         * 
         * @return
         */
        public Region getTail()
        {
            if (regions.size() == 1)
            {
                this.tail = regions.get(0);
            }
            return this.tail;
        }

        /**
         * For internal Use: Returns a boolean flag indicating whether
         * the specified {@link Layer} is the tail of the Network.
         * @param l     the layer to test   
         * @return  true if so, false if not
         */
        bool isTail(Layer l)
        {
            if (regions.Count == 1)
            {
                this.tail = regions.get(0);
            }
            return tail.getTail() == l;
        }

        /**
         * Returns a {@link Iterator} capable of walking the tree of regions
         * from the root {@link Region} down through all the child Regions. In turn,
         * a {@link Region} may be queried for a {@link Iterator} which will return
         * an iterator capable of traversing the Region's contained {@link Layer}s.
         * 
         * @return
         */
        public ICollection<Region> iterator()
        {
            return getRegions();
        }

        /**
         * Used to manually input data into a {@link Network}, the other way 
         * being the call to {@link Network#start()} for a Network that contains a
         * Region that contains a {@link Layer} which in turn contains a {@link Sensor} <em>-OR-</em>
         * subscribing a receiving Region to this Region's output Observable.
         * 
         * @param input One of (int[], String[], {@link ManualInput}, or Map&lt;String, Object&gt;)
         */
        public <T> void compute(T input)
        {
            if (tail == null && regions.size() == 1)
            {
                this.tail = regions.get(0);
            }

            if (head == null)
            {
                addDummySubscriber();
            }

            tail.compute(input);
        }

        /**
         * Used to manually input data into a {@link Network} in a synchronous way, the other way 
         * being the call to {@link Network#start()} for a Network that contains a
         * Region that contains a {@link Layer} which in turn contains a {@link Sensor} <em>-OR-</em>
         * subscribing a receiving Region to this Region's output Observable.
         * 
         * @param input One of (int[], String[], {@link ManualInput}, or Map&lt;String, Object&gt;)
         */
        public <T> Inference computeImmediate(T input)
        {
            if (isThreadRunning)
            {
                throw new IllegalStateException("Cannot call computeImmediate() when Network has been started.");
            }

            if (tail == null && regions.size() == 1)
            {
                this.tail = regions.get(0);
            }

            if (head == null)
            {
                addDummySubscriber();
            }

            tail.compute(input);
            return head.getHead().getInference();
        }

        /**
         * Added when a synchronous call is made and there is no subscriber. No
         * Subscriber leads to the observable chain not being constructed, therefore
         * we must always have at least one subscriber.
         */
        void addDummySubscriber()
        {
            observe().subscribe(new Subscriber<Inference>() {
            @Override public void onCompleted() { }
            @Override public void onError(Throwable e) { e.printStackTrace(); }
            @Override public void onNext(Inference i) { }
        });
    }

    /**
     * Connects the specified source to the specified sink (the order of
     * processing flows from source to sink, or lower level region to higher
     * level region). 
     * @param regionSink        the receiving end of the connection
     * @param regionSource      the source end of the connection
     * 
     * @return  this {@code Network}
     */
    public Network connect(String regionSink, String regionSource)
    {
        Region source = lookup(regionSource);
        if (source == null)
        {
            throw new IllegalArgumentException("Region with name: " + regionSource + " not added to Network.");
        }

        Region sink = lookup(regionSink);
        if (sink == null)
        {
            throw new IllegalArgumentException("Region with name: " + regionSink + " not added to Network.");
        }

        sink.connect(source);

        tail = tail == null ? source : tail;
        head = head == null ? sink : head;

        Region bottom = source;
        while ((bottom = bottom.getUpstreamRegion()) != null)
        {
            tail = bottom;
        }

        Region top = sink;
        while ((top = top.getDownstreamRegion()) != null)
        {
            head = top;
        }

        return this;
    }

    /**
     * Adds a {@link Region} to this {@code Network}
     * @param region
     * @return
     */
    public Network add(Region region)
    {
        regions.add(region);
        region.setNetwork(this);
        return this;
    }

    /**
     * Closes all the {@link Region} objects, in this {@link Network}
     */
    public Network close()
    {
        regions.forEach(region->region.close());
        return this;
    }

    /**
     * Returns a {@link List} view of the contained {@link Region}s.
     * @return
     */
    public ICollection<Region> getRegions()
    {
        return new List<Region>(regions);
    }

    /**
     * Sets a reference to the {@link Region} which contains the {@link Sensor}
     * (if any).
     * 
     * @param r
     */
    public void setSensorRegion(Region r)
    {
        this.sensorRegion = r;
    }

    /**
     * Returns a reference to the {@link Region} which contains the {@link Sensor}
     * 
     * @return  the Region which contains the Sensor
     */
    public Region getSensorRegion()
    {
        return sensorRegion;
    }

    /**
     * Returns the {@link Region} with the specified name
     * or null if it doesn't exist within this {@code Network}
     * @param regionName
     * @return
     */
    public Region lookup(String regionName)
    {
        for (Region r : regions)
        {
            if (r.getName().equals(regionName))
            {
                return r;
            }
        }
        return null;
    }

    /**
     * Returns the network-level {@link Parameters}.
     * @return
     */
    public Parameters getParameters()
    {
        return parameters;
    }

    /**
     * Sets the reference to this {@code Network}'s Sensor
     * @param sensor
     */
    public void setSensor(HTMSensor<?> sensor)
    {
        this.sensor = sensor;
        this.sensor.initEncoder(this.parameters);
    }

    /**
     * Returns the encoder present in one of this {@code Network}'s
     * {@link Sensor}s
     * 
     * @return
     */
    public HTMSensor<?> getSensor()
    {
        return sensor;
    }

    /**
     * Sets the {@link MultiEncoder} on this Network
     * @param e
     */
    public void setEncoder(MultiEncoder e)
    {
        this.encoder = e;
    }

    /**
     * Returns the {@link MultiEncoder} with which this Network is configured.
     * @return
     */
    public MultiEncoder getEncoder()
    {
        return this.encoder;
    }

    /**
     * Checks the name for suitability within a given network, 
     * checking for reserved characters and such.
     * 
     * @param name
     */
    private static void checkName(String name)
    {
        if (name.IndexOf(":") != -1)
        {
            throw new ArgumentException("\":\" is a reserved character.");
        }
    }

    /* (non-Javadoc)
     * @see java.lang.Object#hashCode()
     */
    //@Override
    public int hashCode()
    {
        final int prime = 31;
        int result = 1;
        result = prime * result + (isLearn ? 1231 : 1237);
        result = prime * result + ((name == null) ? 0 : name.hashCode());
        result = prime * result + ((parameters == null) ? 0 : parameters.hashCode());
        result = prime * result + ((regions == null) ? 0 : regions.hashCode());
        result = prime * result + ((sensor == null) ? 0 : sensor.hashCode());
        return result;
    }

    /* (non-Javadoc)
     * @see java.lang.Object#equals(java.lang.Object)
     */
    @Override
    public boolean equals(Object obj)
    {
        if (this == obj)
            return true;
        if (obj == null)
            return false;
        if (getClass() != obj.getClass())
            return false;
        Network other = (Network)obj;
        if (isLearn != other.isLearn)
            return false;
        if (name == null)
        {
            if (other.name != null)
                return false;
        }
        else if (!name.equals(other.name))
            return false;
        if (parameters == null)
        {
            if (other.parameters != null)
                return false;
        }
        else if (!parameters.equals(other.parameters))
            return false;
        if (regions == null)
        {
            if (other.regions != null)
                return false;
        }
        else if (!regions.equals(other.regions))
            return false;
        if (sensor == null)
        {
            if (other.sensor != null)
                return false;
        }
        else if (!sensor.equals(other.sensor))
            return false;
        return true;
    }
}
