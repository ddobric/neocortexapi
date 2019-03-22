using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Sensors
{
    public interface ISensor<T> : IEnumerator<int[]>, IHtmModule
    {


        /**
         * <p>
         * Creates and returns the {@link Sensor} subtype indicated by the 
         * method reference passed in for the SensorFactory {@link FunctionalInterface}
         * argument. <br><br><b>Typical usage is as follows:</b>
         * </p>
         * <p>
         * <pre>
         * Sensor.create(FileSensor::create, SensorParams); //Can be URISensor, or ObservableSensor
         * </pre>
         * <p>
         * 
         * @param sf    the {@link SensorFactory} or method reference. SensorFactory is a {@link FunctionalInterface}
         * @param t     the {@link SensorParams} which hold the configuration and data source details.
         * @return
         */
        //public static <T> Sensor<T> create(SensorFactory<T> sf, SensorParams t)
        //{
        //    if (sf == null)
        //    {
        //        throw new IllegalArgumentException("Factory cannot be null");
        //    }
        //    if (t == null)
        //    {
        //        throw new IllegalArgumentException("Properties (i.e. \"SensorParams\") cannot be null");
        //    }

        //    return new HTMSensor<T>(sf.create(t));
        //}

        /**
         * Returns an instance of {@link SensorParams} used 
         * to initialize the different types of Sensors with
         * their resource location or source object.
         * 
         * @return a {@link SensorParams} object.
         */
        SensorParameters getSensorParams();

        /**
         * Returns the configured {@link Stream} if this is of
         * Type Stream, otherwise it throws an {@link UnsupportedOperationException}
         * 
         * @return the constructed Stream
         */
        IMetaStream<T> getInputStream();
        
        /// <summary>
        /// Descriptor, which describes the input.
        /// </summary>
        DataDescriptor DataDescriptor { get; set; }

        HeaderMetaData HeaderMetaData {get;set;}


        /// <summary>
        /// Number of bits of input produced by sensor.
        /// </summary>
        int InputWidth { get;  }

        /**
         * Returns the inner Stream's meta information.
         * @return
         */
        //public ValueList getMetaInfo();
    }
}
