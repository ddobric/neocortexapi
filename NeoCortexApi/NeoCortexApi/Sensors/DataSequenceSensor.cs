using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi;
using NeoCortexApi.DataMappers;
using NeoCortexApi.Network;

namespace NeoCortexApi.Sensors
{
    public class DataSequenceSensor : ISensor<int>
    {
        private int currentPos;

        private object[][] data;

        private CortexNetworkContext context;

        public DataDescriptor DataDescriptor { get; set; }

        private DataMapper mapper;

        private int[] currentOutput;

        public HeaderMetaData HeaderMetaData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public int[] Current
        {
            get
            {
                return this.currentOutput;
            }
        }


        object IEnumerator.Current {

            get {
                return this.currentOutput;
            }
        }

        /// <summary>
        /// Length of input vector.
        /// </summary>
        public int InputWidth
        {

            get
            {
                return this.mapper.InputWidth;
            }
        }

        public IModuleData Output => throw new NotImplementedException();

        public IModuleData Input => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DataSequenceSensor(object[][] data, DataDescriptor descriptor, CortexNetworkContext context)
        {
            if (data == null)
                throw new ArgumentException("data argument cannot be null!");

            this.data = data;

            this.context = context;

            this.mapper = new DataMapper(descriptor, context);
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
            if (this.currentPos + 1 < data.Length)
            {
                this.currentPos++;

                var row = data[this.currentPos];

                currentOutput = this.mapper.Run(row);              

                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            this.currentPos = 0;
        }

        public IModuleData Compute(int[] input, bool learn)
        {
            throw new NotImplementedException();
        }
    }
}
