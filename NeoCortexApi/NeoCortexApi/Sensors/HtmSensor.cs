using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Utility;
using System.IO;
using NeoCortexApi.Network;

namespace NeoCortexApi.Sensors
{
    public class HTMSensor<T> : ISensor<T>, IMetaStream<T>, IEquatable<HTMSensor<T>>
    {
        private bool encodersInitted;
        private CortexNetworkContext context;
        private ISensor<T> sensor;
        private SensorParameters sensorParams;
        private Header header;
        private Parameters localParameters;
        private MultiEncoder<T> encoder;
        private List<int[]> outputStream;
        private List<int[]> output;
        //private InputMap inputMap;

        private Dictionary<int, EncoderBase<T>> indexToEncoderMap;

        private Dictionary<String, object> indexFieldMap = new Dictionary<string, object>();

        /// <summary>
        /// Encoder attached to sensor.
        /// </summary>
        public MultiEncoder<T> Encoder { get => encoder; set => encoder = value; }


        /**
      * <p>
      * Main method by which this Sensor's information is retrieved.
      * </p><p>
      * This method returns a subclass of Stream ({@link MetaStream})
      * capable of returning a flag indicating whether a terminal operation
      * has been performed on the stream (i.e. see {@link MetaStream#isTerminal()});
      * in addition the MetaStream returned can return meta information (see
      * {@link MetaStream#getMeta()}.
      * </p>
      * @return  a {@link MetaStream} instance.
      */

        public IMetaStream<T> getInputStream()
        {
            return (IMetaStream<T>)sensor.getInputStream();
        }

        /**
   * Called internally during construction to build the encoders
   * needed to process the configured field types.
   */

        public HTMSensor(ISensor<T> sensor, CortexNetworkContext context)
        {
            if (sensor == null || context == null)
                throw new ArgumentException("Sensor and context must be both provided.");

            this.context = context;

            this.sensor = sensor;
            this.sensorParams = sensor.getSensorParams();
            header = new Header(sensor.getInputStream().getMeta());
            if (header == null || header.Size < 3)
            {
                throw new InvalidOperationException("Header must always be present; and have 3 lines.");
            }

            createEncoder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoderSettings"></param>
        private void setupEncoders(Dictionary<String, Dictionary<String, Object>> encoderSettings)
        {
            //if (encoder is typeof(MultiEncoder<T>)) {
            if (encoderSettings == null || encoderSettings.Keys.Count == 0)
            {
                throw new ArgumentException(
                    "Cannot initialize this Sensor's MultiEncoder with a null settings");
            }
            //}

            var sortedFieldNames = encoderSettings.Keys.OrderBy(k => k);

            const string cFieldName = "fieldName";

            //MultiEncoderAssembler.assemble(Encoder, encoderSettings);
            foreach (String field in sortedFieldNames)
            {
                var prms = encoderSettings[field];

                if (!prms.ContainsKey(cFieldName)) {
                    throw new ArgumentException($"Missing fieldname for encoder {field}");
            }

            String fieldName = (String) prms[cFieldName];

            if (!prms.ContainsKey("encoderType")) {
                throw new ArgumentException($"Missing type for encoder {field}");
            }

                this.context.CreateEncoder<T>(encoderType, prms);

            String encoderType = (String) prms["encoderType"];
            Builder <?, ?> builder = ((MultiEncoder)encoder).getBuilder(encoderType);

            if (encoderType.equals("SDRCategoryEncoder"))
            {
                // Add mappings for category list
                configureCategoryBuilder((MultiEncoder)encoder, params, builder);
            }
            else if (encoderType.equals("DateEncoder"))
            {
                // Extract date specific mappings out of the map so that we can
                // pre-configure the DateEncoder with its needed directives.
                configureDateBuilder(encoder, encoderSettings, (DateEncoder.Builder)builder);
            }
            else if (encoderType.equals("GeospatialCoordinateEncoder"))
            {
                // Extract Geo specific mappings out of the map so that we can
                // pre-configure the GeospatialCoordinateEncoder with its needed directives.
                configureGeoBuilder(encoder, encoderSettings, (GeospatialCoordinateEncoder.Builder)builder);
            }
            else
            {
                for (String param : params.keySet())
                {
                    if (!param.equals("fieldName") && !param.equals("encoderType") &&
                        !param.equals("fieldType") && !param.equals("fieldEncodings"))
                    {

                        ((MultiEncoder)encoder).setValue(builder, param, params.get(param));
                    }
                }
            }

            encoder.addEncoder(fieldName, (Encoder <?>)builder.build());
        }
    }

        /**
     * Initializes this {@code HTMSensor}'s internal encoders if and 
     * only if the encoders have not been previously initialized.
     */


        public void initEncoder(Parameters encoderParams)
        {
            this.localParameters = encoderParams;

            Dictionary<String, Dictionary<String, Object>> encoderSettings;
            if ((encoderSettings = (Dictionary<String, Dictionary<String, Object>>)encoderParams[KEY.FIELD_ENCODING_MAP]) != null &&
                !encodersInitted)
            {

                setupEncoders(encoderSettings);

                makeIndexEncoderMap();

                encodersInitted = true;
            }
        }

        private void createEncoder()
        {
            Encoder = new MultiEncoder<T>();

            Dictionary<String, Dictionary<String, Object>> encoderSettings;
            if (localParameters != null &&
                (encoderSettings = (Dictionary<String, Dictionary<String, Object>>)localParameters[KEY.FIELD_ENCODING_MAP]) != null &&
                    encoderSettings.Keys.Count != 0)
            {

                setupEncoders(encoderSettings);
                makeIndexEncoderMap();
            }
        }


        /**
    * Returns the encoded output stream of the underlying {@link Stream}'s encoder.
    * 
    * @return      the encoded output stream.
    */
        public ICollection<int[]> getOutputStream()
        {
            if (this.isTerminal())
            {
                throw new InvalidOperationException("Stream is already \"terminal\" (operated upon or empty)");
            }

            MultiEncoder<T> encoder = (MultiEncoder<T>)getEncoder();
            if (encoder == null)
            {
                throw new InvalidOperationException("setLocalParameters(Parameters) must be called before calling this method.");
            }

            // Protect outputStream formation and creation of "fan out" also make sure
            // that no other thread is trying to update the fan out lists
            Stream<int[]> retVal = null;
            try
            {
                criticalAccessLock.lock () ;

                final String[] fieldNames = getFieldNames();
                final FieldMetaType[] fieldTypes = getFieldTypes();

                if (outputStream == null)
                {
                    if (indexFieldMap.isEmpty())
                    {
                        for (int i = 0; i < fieldNames.length; i++)
                        {
                            indexFieldMap.put(fieldNames[i], i);
                        }
                    }

                    // NOTE: The "inputMap" here is a special local implementation
                    //       of the "Map" interface, overridden so that we can access
                    //       the keys directly (without hashing). This map is only used
                    //       for this use case so it is ok to use this optimization as
                    //       a convenience.
                    if (inputMap == null)
                    {
                        inputMap = new InputMap();
                        inputMap.fTypes = fieldTypes;
                    }

                    final boolean isParallel = delegate.getInputStream().isParallel();

                    output = new ArrayList<>();

                    outputStream = delegate.getInputStream().map(l-> {
                        String[] arr = (String[])l;
                        inputMap.arr = arr;
                        return input(arr, fieldNames, fieldTypes, output, isParallel);
                    });

                    mainIterator = outputStream.iterator();
                }

                LinkedList<int[]> l = new LinkedList<int[]>();
                fanOuts.add(l);
                Copy copy = new Copy(l);

                retVal = StreamSupport.stream(Spliterators.spliteratorUnknownSize(copy,
                    Spliterator.ORDERED | Spliterator.NONNULL | Spliterator.IMMUTABLE), false);

            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
            finally
            {
                criticalAccessLock.unlock();
            }

            return retVal;
        }

        private void makeIndexEncoderMap()
        {
            indexToEncoderMap = new Dictionary<int, EncoderBase<T>>();

            for (int i = 0, size = header.Metadata.FieldNames.Count; i < size; i++)
            {
            }
        }

     



        public SensorParameters getSensorParams()
        {
            return this.sensor.getSensorParams();
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + ((indexFieldMap == null) ? 0 : indexFieldMap.GetHashCode());

            //result = prime * result + ((sensorParams == null) ? 0 : Arrays.deepHashCode(sensorParams.keys()));
            result = prime * result + ((sensorParams == null) ? 0 : sensorParams.GetHashCode());
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */

        public bool Equals(HTMSensor<T> obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            HTMSensor<T> other = (HTMSensor<T>)obj;
            if (indexFieldMap == null)
            {
                if (other.indexFieldMap != null)
                    return false;
            }
            else if (!ArrayUtils.AreEqual(indexFieldMap, other.indexFieldMap))
                return false;
            if (sensorParams == null)
            {
                if (other.sensorParams != null)
                    return false;
            }
            else if (!sensorParams.Keys.SequenceEqual(other.sensorParams.Keys))
                return false;
            return true;
        }

        public HeaderMetaData getMeta()
        {
            return this.sensor.getInputStream().getMeta();
        }

        public bool isTerminal()
        {
            return false;
        }
    }
}
