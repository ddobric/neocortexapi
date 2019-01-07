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
    public class Layer
    {
    }
}
