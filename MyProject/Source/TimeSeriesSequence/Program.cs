using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using static TimeSeriesSequence.Entity.HelperClasses;
using static TimeSeriesSequence.MultisequenceLearningTest;

namespace TimeSeriesSequence
{
    class program
    {
        /// <summary>
        /// This sample shows a typical experiment code for SP and TM.
        /// You must start this code in debugger to follow the trace.
        /// and TM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            MultiSequenceTaxiPassanger learning_TaxiPassanger = new MultiSequenceTaxiPassanger();

            /// Prediction of taxi passangers based on data set
            learning_TaxiPassanger.RunPassangerTimeSeriesSequenceExperiment();
        } 



    }
}
