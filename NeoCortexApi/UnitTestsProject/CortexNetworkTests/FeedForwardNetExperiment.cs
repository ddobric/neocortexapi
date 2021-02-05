using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject.CortexNetworkTests
{

    /// <summary>
    /// In Brain Layer 4 has feed forforward connection with Layer 2 in CortexLayer.
    /// So, instead of using layer name L1 we give it as L4
    /// </summary>
    [TestClass]
    public class FeedForwardNetExperiment
    {
        CortexLayer<object, object> layerL4, layerL2;

        TemporalMemory tm4, tm2;

        [TestMethod]
        public void FeedForwardNetTest()
        {
            int cellsPerColumn = 25;
            int numColumns = 2048;
            int inputBits = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            double max = 70000;

            HtmConfig htmConfig_L4 = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),
                CellsPerColumn = cellsPerColumn,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = 50,
                InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            // The HTM of the L2 is connected to cells of the HTM of L4.
            int inputsL2 = numColumns * cellsPerColumn;

            HtmConfig htmConfig_L2 = new HtmConfig(new int[] { inputsL2 }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = cellsPerColumn,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = 50,
                InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            Dictionary<string, object> settings = new Dictionary<string, object>()
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


            EncoderBase encoder = new ScalarEncoder(settings);
            List<double> inputValues = new List<double>(new double[] { 12345, 12345, 6783, 6783, 12345, 6783 });
            RunExperiment(inputBits, htmConfig_L4, encoder, inputValues, htmConfig_L2);
        }

        private void RunExperiment(int inputBits, HtmConfig cfgL4, EncoderBase encoder, List<double> inputValues, HtmConfig cfgL2)
        {
            //int maxMatchCnt = 0;
            bool learn = true;
            bool isInStableState = false;

            var memL4 = new Connections(cfgL4);
            var memL2 = new Connections(cfgL2);

            var numInputs = inputValues.Distinct<double>().ToList().Count;
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            layerL4 = new CortexLayer<object, object>("L4");
            layerL2 = new CortexLayer<object, object>("L2");

            tm4 = new TemporalMemory();
            tm2 = new TemporalMemory();

            // HPC for Layer 4 SP

            HomeostaticPlasticityController hpa_sp_L4 = new HomeostaticPlasticityController(memL4, numInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    Debug.WriteLine($"SP L4 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    Debug.WriteLine($"SP L4 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                learn = isInStableState = isStable;
                //cls.ClearState();
            }, numOfCyclesToWaitOnChange: 50);


            // HPC for Layer 2 SP

            HomeostaticPlasticityController hpa_sp_L2 = new HomeostaticPlasticityController(memL2, numInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    Debug.WriteLine($"SP L2 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    Debug.WriteLine($"SP L2 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                learn = isInStableState = isStable;
                //cls.ClearState();
            }, numOfCyclesToWaitOnChange: 50);

            SpatialPoolerMT sp4 = new SpatialPoolerMT(hpa_sp_L4);

            SpatialPoolerMT sp2 = new SpatialPoolerMT(hpa_sp_L2);

            sp4.Init(memL4);
            sp2.Init(memL2);

            tm4.Init(memL4);
            tm2.Init(memL2);

            layerL4.HtmModules.Add("encoder", encoder);

            layerL4.HtmModules.Add("sp", sp4);

            layerL2.HtmModules.Add("sp", sp2);

            int[] cellArray = new int[cfgL2.CellsPerColumn * cfgL2.NumColumns];

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];
            int cycle = 0;
            int matches = 0;
            // string lastPredictedValue = "0";
            int maxCycles = 3500;
            int maxPrevInputs = inputValues.Count - 1;
            List<string> previousInputs = new List<string>();

            //
            // Training SP at Layer 4 to get stable. New-born stage.
            //

            for (int i = 0; i < maxCycles; i++)
            {

                matches = 0;
                cycle++;
                Debug.WriteLine($"-------------- Newborn Cycle {cycle} at L4 SP region  ---------------");

                foreach (var input in inputs)
                {
                    Debug.WriteLine($" -- {input} --");

                    var lyrOut = layerL4.Compute(input, learn);

                    if (isInStableState)
                        break;
                }

                if (isInStableState)
                    break;
            }


            Debug.WriteLine($"-------------- L4 SP region is  {isInStableState} ---------------");

            layerL4.HtmModules.Add("tm", tm4);


            // SP+TM at L4

            for (int i = 0; i < maxCycles; i++)
            {
                matches -= 0;

                cycle++;

                Debug.WriteLine($"-------------- L4 TM Train region Cycle {cycle} ---------------");

                foreach (var input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    var lyrOut = layerL4.Compute(input, learn) as ComputeCycle;

                    previousInputs.Add(input.ToString());
                    if (previousInputs.Count > (maxPrevInputs + 1))
                        previousInputs.RemoveAt(0);

                    if (previousInputs.Count < maxPrevInputs)
                        continue;

                    if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                    {
                        /*var activeColumns = L4.GetResult("sp") as int[];
                        foreach (var item in activeColumns)
                        {
                            L2.Compute(item, true);
                        }*/

                        // Reset tha array
                        InitArray(cellArray, 0);

                        // Set the output active cell array
                        // ArrayUtils.SetIndexesTo(cellArray, memL2.ActiveCells.Select(c => c.Index).ToArray(), 1);
                        //Since our main target to pass active cells sdr from Layer 4 to Layer 2
                        ArrayUtils.SetIndexesTo(cellArray, memL4.ActiveCells.Select(c => c.Index).ToArray(), 1);
                        // 4102,25072, 25363, 25539, 25738, 25961, 26009, 26269, 26491, 26585, 26668, 26920, 26934, 27040, 27107, 27262, 27392, 27826, 27948, 28174, 28243, 28270, 28294, 28308, 28429, 28577, 28671, 29139, 29618, 29637, 29809, 29857, 29897, 29900, 29969, 30057, 30727, 31111, 49805, 49972, 
                        layerL2.Compute(cellArray, true);

                        /*foreach (var item in lyrOut.ActiveCells)
                        {
                            L2.Compute(item, true);
                        }*/

                        //var activeCell = Helpers.StringifyVector(lyrOut.ActiveCells.Select(c => c.Index).ToArray());
                        //L2.Compute(activeCell, true);


                    }


                }
            }

            layerL2.HtmModules.Add("tm2", tm2);
        }

        private static void InitArray(int[] array, int val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = val;
            }
        }
    }   
}
