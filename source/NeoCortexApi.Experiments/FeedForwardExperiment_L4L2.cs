
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
using System.IO;
using System.Linq;


namespace NeocortexApi.Experiments
{

    /// <summary>
    /// In the brain the Layer 4 has feed forforward connection with Layer 2 in CortexLayer.
    /// SIt feeds forward the result of L4 as input to the L2. Both L4 and L2 uses SP and TM.
    /// Discussion: https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/issues/70
    /// Check out student paper in the following URL: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/ML-20-21_20-5.2_HTM%20FeedForward_Network.pdf
    /// </summary>
    [TestClass]
    public class FeedForwardNetExperiment_L4L2
    {
        CortexLayer<object, object> layerL4, layerL2;
        Dictionary<double, int[]> L4_ActiveCell_sdr_log = new Dictionary<double, int[]>();
        Dictionary<double, int[]> L2_ActiveCell_sdr_log = new Dictionary<double, int[]>();

        TemporalMemory tm4, tm2;
        bool isSimilar_L4_active_cell_sdr = false;
        string key;

        [TestMethod]
        [TestCategory("Experiment")]
        public void FeedForwardNetTest()
        {
            int cellsPerColumnL4 = 20;
            int numColumnsL4 = 500;
            int cellsPerColumnL2 = 20;
            int numColumnsL2 = 500;
            int inputBits = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            double max = 20;

            HtmConfig htmConfig_L4 = new HtmConfig(new int[] { inputBits }, new int[] { numColumnsL4 })
            {
                Random = new ThreadSafeRandom(42),
                CellsPerColumn = cellsPerColumnL4,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumnsL4,
                PotentialRadius = inputBits,// Ever column is connected to 50 of 100 input cells.
                //InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.02 * numColumnsL4),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.10,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            // The HTM of the L2 is connected to cells of the HTM of L4.
            int inputsL2 = numColumnsL4 * cellsPerColumnL4;

            HtmConfig htmConfig_L2 = new HtmConfig(new int[] { inputsL2 }, new int[] { numColumnsL2 })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = cellsPerColumnL2,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.1 * numColumnsL2,
                PotentialRadius = inputsL2, // Every columns 
                //InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.05 * numColumnsL2),
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
            //List<double> inputValues = new List<double>(new double[] { 12, 12, 17, 17, 12 });
            // List<double> inputValues = new List<double>(new double[] { 7, 8, 9, 10, 11, 8, 9, 12 });
            //List<double> inputValues = new List<double>(new double[] { 7, 8, 9 });
            //List<double> inputValues = new List<double>(new double[] { 12345,12345,6783,6783,12345 });
            List<double> inputValues = new List<double>(new double[] { 2, 6, 6, 7, 6, 6, 8, 1 });

            RunExperiment(inputBits, htmConfig_L4, encoder, inputValues, htmConfig_L2);
        }

        private void RunExperiment(int inputBits, HtmConfig cfgL4, EncoderBase encoder, List<double> inputValues, HtmConfig cfgL2)
        {
            Stopwatch swL2 = new Stopwatch();

            int maxMatchCnt = 0;
            bool learn = true;
            bool isSP4Stable = false;
            bool isSP2STable = false;

            Connections memL4 = new Connections(cfgL4);
            Connections memL2 = new Connections(cfgL2);

            int numInputs = inputValues.Distinct<double>().ToList().Count;
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            layerL4 = new CortexLayer<object, object>("L4");
            layerL2 = new CortexLayer<object, object>("L2");
            //tm4 = new TemporalMemoryMT();
            //tm2 = new TemporalMemoryMT();
            tm4 = new TemporalMemory();
            tm2 = new TemporalMemory();

            //
            // HPC for Layer 4 SP
            HomeostaticPlasticityController hpa_sp_L4 = new HomeostaticPlasticityController(memL4, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    Debug.WriteLine($"SP L4 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    Debug.WriteLine($"SP L4 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                learn = isSP4Stable = isStable;
                cls.ClearState();

            }, numOfCyclesToWaitOnChange: 50);


            //
            // HPC for Layer 2 SP
            HomeostaticPlasticityController hpa_sp_L2 = new HomeostaticPlasticityController(memL2, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    Debug.WriteLine($"SP L2 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    Debug.WriteLine($"SP L2 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                learn = isSP2STable = isStable;
                cls.ClearState();
            }, numOfCyclesToWaitOnChange: 50);

            SpatialPooler sp4 = new SpatialPooler(hpa_sp_L4);

            SpatialPooler sp2 = new SpatialPooler(hpa_sp_L2);

            sp4.Init(memL4);
            sp2.Init(memL2);

            // memL2.TraceInputPotential();

            tm4.Init(memL4);
            tm2.Init(memL2);

            layerL4.HtmModules.Add("encoder", encoder);
            layerL4.HtmModules.Add("sp", sp4);
            layerL4.HtmModules.Add("tm", tm4);

            layerL2.HtmModules.Add("sp", sp2);
            layerL2.HtmModules.Add("tm", tm2);

            int[] inpCellsL4ToL2 = new int[cfgL4.CellsPerColumn * cfgL4.NumColumns];

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];
            int cycle = 0;
            int matches = 0;
            string lastPredictedValue = "0";
            int maxCycles = 3500;
            int maxPrevInputs = inputValues.Count - 1;
            List<string> previousInputs = new List<string>();


            //
            // Training SP at Layer 4 to get stable. New-born stage.
            //
            using (StreamWriter swL4Sdrs = new StreamWriter($"L4-SDRs-in_{cfgL2.NumInputs}-col_{cfgL2.NumColumns}-r_{cfgL2.PotentialRadius}.txt"))
            {
                using (StreamWriter sw = new StreamWriter($"in_{cfgL2.NumInputs}-col_{cfgL2.NumColumns}-r_{cfgL2.PotentialRadius}.txt"))
                {
                    for (int i = 0; i < maxCycles; i++)
                    {
                        matches = 0;
                        cycle = i;
                        Debug.WriteLine($"-------------- Newborn Cycle {cycle} at L4 SP region  ---------------");

                        foreach (double input in inputs)
                        {
                            Debug.WriteLine($" INPUT: '{input}'\t Cycle:{cycle}");
                            Debug.Write("L4: ");
                            object lyrOut = layerL4.Compute(input, learn);
                            int[] activeColumns = layerL4.GetResult("sp") as int[];
                            int[] cellSdrL4Indexes = memL4.ActiveCells.Select(c => c.Index).ToArray();
                            Debug.WriteLine($"L4out Active Coloumn for input: {input}: {Helpers.StringifyVector(activeColumns)}");
                            Debug.WriteLine($"L4out SDR for input: {input}: {Helpers.StringifyVector(cellSdrL4Indexes)}");


                            if (isSP4Stable)
                            {

                                /// <summary>
                                /// Checking Layer4 SP is giving similar or different 
                                /// Active Cell Sdr Indexes After it reaches to stable state.
                                /// This portion is actuallly to hold all acive cell sdr 
                                /// indexes after Layer 4 SP reaches at stable state via HPC.
                                /// 
                                /// But why we have done this?
                                /// 
                                /// Actually, In Necortex api we have obeserved during severel
                                /// experiments that whenvever Layer4 SP reaches to STABLE sate
                                /// it doesnt give us smilar pattern of Active Cell SDR Indexes
                                /// for each particular input data.
                                /// 
                                /// Instead active cell sdr patern of L4 varried  though it has reached
                                /// to stable sate!
                                /// 
                                /// So we want to obeserve whether Layer 4 SP is giving similar active cell sdr indexes 
                                /// or different acrive cell sdr indexes after it reaches to stable state.
                                /// 
                                /// If we receive similar acrive cell sdr indexes from Layer 4 sp after it reaches 
                                /// to stable state then do train Layer 2 sp by as usual process in NeocortexApi by
                                /// calling Layer4.Compute().
                                /// 
                                /// But if we receive different active cell sdr indexes then we will train Layer 2 sp
                                /// from L4_ActiveCell_sdr_log so that during training
                                /// Layer 2 SP gets similar stable active cell sdr indexes of layer4
                                /// from that dictionary. As a result, L2 SP will get similar sequence
                                /// of active cell sdr indexes during SP Tarining and reach to STABLE state
                                /// </summary>

                                Array.Sort(cellSdrL4Indexes);
                                if (!L4_ActiveCell_sdr_log.ContainsKey(input))
                                {
                                    L4_ActiveCell_sdr_log.Add(input, cellSdrL4Indexes);
                                }
                                else
                                {

                                    if (L4_ActiveCell_sdr_log[input].SequenceEqual(cellSdrL4Indexes))
                                    {
                                        Debug.WriteLine($"Layer4.Compute() is giving similar cell sdr indexes for input : {input} after reaching to stable state");
                                        isSimilar_L4_active_cell_sdr = true;
                                    }
                                    else
                                    {
                                        isSimilar_L4_active_cell_sdr = false;
                                        Debug.WriteLine($"Layer4.Compute() is giving different cell sdr indexes for input : {input} after reaching to stable state");
                                        Debug.WriteLine($"Sdr Mismatch with L4_ActiveCell_sdr_log after reaching to stable state");
                                        Debug.WriteLine($" L4_ActiveCell_sdr_log output for input {input}: { Helpers.StringifyVector(L4_ActiveCell_sdr_log[input])}");
                                        Debug.WriteLine($"L4 out sdr input:{input} {Helpers.StringifyVector(cellSdrL4Indexes)}");
                                        //Debug.WriteLine($"L4 out ac input: {input}: {Helpers.StringifyVector(activeColumns)}");
                                    }
                                }


                                if (!isSimilar_L4_active_cell_sdr)
                                {

                                    cellSdrL4Indexes = L4_ActiveCell_sdr_log[input];
                                }


                                //
                                // Training SP at Layer 2 to get stable. New-born stage.
                                //

                                // Write SDR as output of L4 and input of L2
                                // swL4Sdrs.WriteLine($"{input} - {Helpers.StringifyVector(cellSdrL4Indexes)}");
                                // Set the output active cell array

                                InitArray(inpCellsL4ToL2, 0);
                                ArrayUtils.SetIndexesTo(inpCellsL4ToL2, cellSdrL4Indexes, 1);
                                Debug.WriteLine($"L4 cell sdr to L2 SP Train for Input {input}: ");
                                layerL2.Compute(inpCellsL4ToL2, true);
                                int[] overlaps = ArrayUtils.IndexWhere(memL2.Overlaps, o => o > 0);
                                string strOverlaps = Helpers.StringifyVector(overlaps);
                                Debug.WriteLine($"Potential columns: {overlaps.Length}, overlaps: {strOverlaps}");


                            }

                        }


                        if (isSP4Stable && isSP2STable)
                            break;


                    }
                }
            }


            //
            // SP+TM at L2
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle = i;

                Debug.WriteLine($"-------------- L2 TM Train region Cycle {cycle} ---------------");

                foreach (double input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    // Reset tha array

                    //var cellSdrL4Indexes = L4_ActiveCell_sdr_log[input];
                    object layerL4Out = layerL4.Compute(input, learn);
                    previousInputs.Add(input.ToString());

                    if (previousInputs.Count > (maxPrevInputs + 1))
                        previousInputs.RemoveAt(0);
                    if (previousInputs.Count < maxPrevInputs)
                        continue;

                    key = GetKey(previousInputs, input);

                    List<Cell> actCells;
                    InitArray(inpCellsL4ToL2, 0);
                    int[] cellSdrL4Indexes;

                    if (!isSimilar_L4_active_cell_sdr)
                    {

                        cellSdrL4Indexes = L4_ActiveCell_sdr_log[input];
                    }
                    else
                    {
                        cellSdrL4Indexes = memL4.ActiveCells.Select(c => c.Index).ToArray();
                    }


                    // Set the output active cell array
                    ArrayUtils.SetIndexesTo(inpCellsL4ToL2, cellSdrL4Indexes, 1);

                    ComputeCycle layerL2Out = layerL2.Compute(inpCellsL4ToL2, true) as ComputeCycle;


                    if (layerL2Out.ActiveCells.Count == layerL2Out.WinnerCells.Count)
                    {
                        actCells = layerL2Out.ActiveCells;
                    }
                    else
                    {
                        actCells = layerL2Out.WinnerCells;
                    }

                    /// <summary>
                    /// HTM Classifier has added for Layer 2 
                    /// </summary>

                    cls.Learn(key, actCells.ToArray());

                    if (key == lastPredictedValue)
                    {
                        matches++;
                        Debug.WriteLine($"Match. Actual  Sequence: {key} - Last Predicted Sequence: {lastPredictedValue}");
                    }
                    else
                        Debug.WriteLine($"Missmatch! Actual Sequence: {key} - Last Predicted Sequence: {lastPredictedValue}");


                    /// <summary>
                    /// Classifier is taking Predictive Cells from Layer 2
                    /// </summary>

                    if (layerL2Out.PredictiveCells.Count > 0)
                    {
                        string predictedInputValue = cls.GetPredictedInputValue(layerL2Out.PredictiveCells.ToArray());

                        Debug.WriteLine($"Current Input: {input} \t| New Predicted Input Sequence: {predictedInputValue}");

                        lastPredictedValue = predictedInputValue;
                    }
                    else
                    {
                        Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                        lastPredictedValue = String.Empty;
                    }



                }

                double accuracy = (double)matches / (double)inputs.Length * 100.0;

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {accuracy}%");

                if (accuracy >= 100.0)
                {
                    maxMatchCnt++;
                    Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");
                    //
                    // Experiment is completed if we are 20 cycles long at the 100% accuracy.
                    if (maxMatchCnt >= 20)
                    {

                        Debug.WriteLine($"Exit experiment in the stable state after 20 repeats with 100% of accuracy.");
                        learn = false;
                        break;
                    }
                }
                else if (maxMatchCnt > 0)
                {
                    Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");

                }
            }



        }


        /// <summary>
        /// It will help us for making Sparce Density Array 
        /// from active cell SDR sequence form Layer 4 SP
        /// in runtime for a particular input in the sequebce
        /// during layer 2 SP train
        /// </summary>

        private static void InitArray(int[] array, int val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = val;
            }
        }

        private static string GetKey(List<string> prevInputs, double input)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return key;
        }
    }
}