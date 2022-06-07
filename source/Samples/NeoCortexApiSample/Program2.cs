using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeoCortexApiSample
{
    class Program2
    {
        static Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>()
        {
            {"S1", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, }) }
        };
        static Connections connection = new Connections(htmConfig);
        static void Main(string[] args)
        {
            EncoderBase encoder = new ScalarEncoder(encoderSettings);
            encoder.Initialize(encoderSettings);

            SpatialPoolerMT sp = new SpatialPoolerMT(homeostaticPlasticityController);
            sp.Init(connection);

            TemporalMemory tm = new TemporalMemory();
            tm.Init(connection);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);
            layer1.HtmModules.Add("tm", tm);

            int maxCycles = 3500;

            foreach (var sequenceKeyPair in sequences)
            {
                for (int i = 0; i < maxCycles; i++)
                {
                    foreach (var input in sequenceKeyPair.Value)
                    {
                        var lyrOut = layer1.Compute(input, true) as ComputeCycle;
                    }
                }
            }
        }






















        
        static int inputBits = 100;
        static int numColumns = 1024;
        static double max = 20;

        static Dictionary<string, object> encoderSettings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };
        static HtmConfig htmConfig = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
        {
            Random = new ThreadSafeRandom(42),

            CellsPerColumn = 25,
            GlobalInhibition = true,
            LocalAreaDensity = -1,
            NumActiveColumnsPerInhArea = 0.02 * numColumns,
            PotentialRadius = (int)(0.15 * inputBits),
            //InhibitionRadius = 15,

            MaxBoost = 10.0,
            DutyCyclePeriod = 25,
            MinPctOverlapDutyCycles = 0.75,
            MaxSynapsesPerSegment = (int)(0.02 * numColumns),

            ActivationThreshold = 15,
            ConnectedPermanence = 0.5,

            // Learning is slower than forgetting in this case.
            PermanenceDecrement = 0.25,
            PermanenceIncrement = 0.15,

            // Used by punishing of segments.
            PredictedSegmentDecrement = 0.1
        };
        static int numUniqueInputs = 8;
        static bool isInStableState = false;

        static HomeostaticPlasticityController homeostaticPlasticityController = new HomeostaticPlasticityController(connection, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
        {
            if (isStable)
                // Event should be fired when entering the stable state.
                Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
            else
                // Ideal SP should never enter unstable state after stable state.
                Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

            // We are not learning in instable state.
            isInStableState = isStable;

            // Clear active and predictive cells.
            //tm.Reset(mem);
        }, numOfCyclesToWaitOnChange: 50);
    }
}
