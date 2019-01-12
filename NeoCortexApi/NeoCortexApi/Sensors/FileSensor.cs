using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Sensors
{
    public class FileSensor : ISensor<int>
    {
        private static readonly int HEADER_SIZE = 3;
        private static readonly int BATCH_SIZE = 20;
        // This is OFF until Encoders are made concurrency safe
        private static readonly bool DEFAULT_PARALLEL_MODE = false;
    
    private BatchedCsvStream<String[]> stream;
        private SensorParameters parameters;

        public List<List<object>> getInputStream()
        {
            throw new NotImplementedException();
        }

        public SensorParameters getSensorParams()
        {
            throw new NotImplementedException();
        }
    }
}
