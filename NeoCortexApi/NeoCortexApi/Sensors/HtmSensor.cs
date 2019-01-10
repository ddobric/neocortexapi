using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Utility;

namespace NeoCortexApi.Sensors
{
    public class HTMSensor<T> : ISensor<T>, IEquatable<HtmSensor<T>>
    {
        private bool encodersInitted;
        private ISensor<T> sensor;
        private SensorParameters sensorParams;
        //private Header header;
        private Parameters localParameters;
        //private MultiEncoder encoder;
        private List<int[]> outputStream;
        private List<int[]> output;
        //private InputMap inputMap;

        private Dictionary<int, EncoderBase<T>> indexToEncoderMap;
        private Dictionary<String, object> indexFieldMap = new Dictionary<string, object>();


    
        public HTMSensor(ISensor<T> sensor)
        {
            this.sensor = sensor;
            this.sensorParams = sensor.getSensorParams();
            header = new Header(sensor.getInputStream().getMeta());
            if (header == null || header.size() < 3)
            {
                throw new IllegalStateException("Header must always be present; and have 3 lines.");
            }
            createEncoder();
        }


        public SensorParameters getSensorParams()
        {
            throw new NotImplementedException();
        }

        public int GetHashCode()
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

      
    }
}
