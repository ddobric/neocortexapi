using NeoCortexApi;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApiPersistenceSample
{
    public static class HTMModuleExtension
    {
        public static CortexLayer<object, object> Train(this CortexLayer<object, object> model, List<double> inputs, int maxCycle, string spName)
        {
            var spatialPooler = (SpatialPooler)model.HtmModules[spName];
            bool isInStableState = false;
            spatialPooler.SetOnStableStatusChanged(((isStable, numPatterns, actColAvg, seenInputs) =>
            {
                // Event should only be fired when entering the stable state.
                // Ideal SP should never enter unstable state after stable state.
                if (isStable == false)
                {
                    Debug.WriteLine($"INSTABLE STATE");
                    // This should usually not happen.
                    isInStableState = false;
                }
                else
                {
                    Debug.WriteLine($"STABLE STATE");
                    // Here you can perform any action if required.
                    isInStableState = true;
                }
            }));

            // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            //
            // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            for (int cycle = 0; cycle < maxCycle; cycle++)
            {
                Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

                //
                // This trains the layer on input pattern.
                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    // 
                    var lyrOut = model.Compute((object)input, true) as int[];

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = model.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    Debug.WriteLine($"[cycle={cycle:D4}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }
                
                if (isInStableState)
                    break;
            }
            return model;
        }
    }
}
