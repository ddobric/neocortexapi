using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LearningFoundation;
using NeoCortexApi.Network;

namespace NeoCortexApi.Sensors
{
    public class CsvFileSensor : ISensor<int>
    {
        public DataDescriptor descriptor { get ; set; }

        public HeaderMetaData HeaderMetaData { get; set ; }


        public int[] Current => throw new NotImplementedException();

        object IEnumerator.Current => throw new NotImplementedException();

        public DataDescriptor DataDescriptor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CsvFileSensor(string fileName, DataDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }

        public void Dispose()
        {
           
        }

        public IMetaStream<int> getInputStream()
        {
            throw new NotImplementedException();
        }

        public SensorParameters getSensorParams()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
