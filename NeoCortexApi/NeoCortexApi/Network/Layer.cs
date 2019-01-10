using Microsoft.Extensions.Logging;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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
        private ILogger logger;

        private bool isClosed;

        public Network ParentNetwork { get; set; }
        public string Name { get; private set; }

        protected Parameters parameters;

        protected SensorParameters sensorParams;

        protected Connections connections;

        public void setNetwork(Network network)
        {
            this.ParentNetwork = network;
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
                sensor.initEncoder(params);
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
                    if (params.get(KEY.FIELD_ENCODING_MAP) == null || ((Map<String, Map<String, Object>>)params.get(KEY.FIELD_ENCODING_MAP)).size() < 1) {
                        LOGGER.error("No field encoding map found for specified MultiEncoder");
                        throw new IllegalStateException("No field encoding map found for specified MultiEncoder");
                    }

                    encoder.addMultipleEncoders((Map<String, Map<String, Object>>)params.get(KEY.FIELD_ENCODING_MAP));
                }

                // Make the declared column dimensions match the actual input
                // dimensions retrieved from the encoder
                int product = 0, inputLength = 0, columnLength = 0;
                if (((inputLength = ((int[])params.get(KEY.INPUT_DIMENSIONS)).length) != (columnLength = ((int[])params.get(KEY.COLUMN_DIMENSIONS)).length))
                            || encoder.getWidth() != (product = ArrayUtils.product((int[])params.get(KEY.INPUT_DIMENSIONS)))) {

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

            autoCreateClassifiers = autoCreateClassifiers != null && (autoCreateClassifiers | (Boolean)params.get(KEY.AUTO_CLASSIFY));

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
                Layer <?> curr = this;
                while ((curr = curr.getPrevious()) != null)
                {
                    if (curr.getEncoder() != null)
                    {
                        int[] dims = (int[])curr.getParameters().get(KEY.INPUT_DIMENSIONS);
                    params.setInputDimensions(dims);
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
                if ((inputLength = ((int[])params.get(KEY.INPUT_DIMENSIONS)).length) !=
                     (columnLength = ((int[])params.get(KEY.COLUMN_DIMENSIONS)).length)) {

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
