using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SPTestForInvLearning
{
    [TestClass]
    public class SPConsistencyTest
    {
        private bool isInStableState1 = false;
        private bool isInStableState2 = false;

        /// <summary>
        /// This is test for Spatial Pooler consistency. 
        /// 2 Spatial Pooler will undergo same learning for 1 pattern, to see if they behave the same in case of same random seed
        /// The output learned SDRs of 2 SP for a same pattern is saved under OutputFilePath/sp1.csv and sp2.csv.    
        /// </summary>
        [TestMethod]
        [TestCategory("Invariant Learning")]
        public void ConsistencyTest()
        {
            // Output file path
            string outputFolder = "OutputFilePath";
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            string path1 = Path.Combine(outputFolder, "sp1.csv");
            string path2 = Path.Combine(outputFolder, "sp2.csv");

            if (!File.Exists(path1))
            {
                using (StreamWriter sw = File.CreateText(path1))
                {
                }
            }
            if (!File.Exists(path2))
            {
                using (StreamWriter sw = File.CreateText(path2))
                {
                }
            }

            // Declaring input sequence
            int[] input = { 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1 };
            int inputLength = input.Length;

            // Declaring input dimension
            int[] inputDim = { inputLength };
            int[] columnDim = { 1024 };

            // Initialize HtmConfig
            HtmConfig config1 = GetHtmConfig(inputDim, columnDim);
            HtmConfig config2 = GetHtmConfig(inputDim, columnDim);

            // Initialize Connections
            Connections mem1 = new Connections(config1);
            Connections mem2 = new Connections(config2);

            // Initialize hpc
            HomeostaticPlasticityController hpc1 = getHpc(mem1, 1);
            HomeostaticPlasticityController hpc2 = getHpc(mem2, 2);


            //Initialize Spatial Pooler
            SpatialPooler sp1 = new SpatialPoolerMT(hpc1);
            SpatialPooler sp2 = new SpatialPoolerMT(hpc2);

            sp1.Init(mem1);
            sp2.Init(mem2);

            var SDRlist1 = RecordOutput(sp1, input, ref this.isInStableState1, path1);
            var SDRlist2 = RecordOutput(sp2, input, ref this.isInStableState2, path2);

            CheckSame(SDRlist1, SDRlist2, outputFolder);
        }

        /// <summary>
        /// check if the 2 SDR lists are the same via means of correlation
        /// </summary>
        /// <param name="sDRlist1"></param>
        /// <param name="sDRlist2"></param>
        /// <param name="outputFolder"></param>
        private void CheckSame(List<int[]> sDRlist1, List<int[]> sDRlist2, string outputFolder)
        {
            Debug.WriteLine($"length of SDRlist 1: {sDRlist1.Count}, length of SDRlist 2: {sDRlist2}");
            for (int i = 0; i < ((sDRlist1.Count <= sDRlist2.Count) ? sDRlist1.Count : sDRlist2.Count); i += 1)
            {
                double correlation = MathHelpers.CalcArraySimilarity(sDRlist1[i], sDRlist2[i]);
                Debug.WriteLine($"pair number {i}: {correlation}");
                double[] temp = new double[] { correlation };
                WriteArray<double>(temp, Path.Combine(outputFolder, "correlation.txt"));
            }
        }

        /// <summary>
        /// Spatial Pooler learn an input pattern till it gets stable or 1000 cycles.
        /// During the learning, every output SDR of the input is recorded in a csv file
        /// </summary>
        /// <param name="sp">the spatial pooler to train</param>
        /// <param name="input">the input bit array to train</param>
        /// <param name="isInStableState">passing from class to function, as there are 2 SPs</param>
        /// <param name="path">the csv file to record the SDRs</param>
        private List<int[]> RecordOutput(SpatialPooler sp, int[] input, ref bool isInStableState, string path)
        {
            // Experiment parameter
            int MaxCycle = 1000;
            int currentCycle = 0;

            // List of unique SDRs
            List<int[]> output = new List<int[]>();

            // While loop
            while (!isInStableState)
            {
                // learning
                var lyrOut = sp.Compute(input, true);

                // Displaying on output and record
                WriteArray<int>(lyrOut, path);

                // saving SDRs list
                output.Add(lyrOut);

                // Outing 
                if (currentCycle >= MaxCycle)
                {
                    Debug.WriteLine($"exp ends by reaching MAXCYCLE = {MaxCycle}");
                    break;
                }
            }
            return output;
        }

        /// <summary>
        /// GetHTM Config
        /// </summary>
        /// <param name="inputDim"></param>
        /// <param name="columnDim"></param>
        /// <returns></returns>
        private HtmConfig GetHtmConfig(int[] inputDim, int[] columnDim)
        {
            HtmConfig cfg = new HtmConfig(inputDim, columnDim)
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * columnDim[0],
                PotentialRadius = (int)(0.15 * inputDim[0]),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * columnDim[0]),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
            };
            return cfg;
        }

        /// <summary>
        /// Append the SDR to the file
        /// </summary>
        /// <param name="lyrOut">SDR to be written</param>
        /// <param name="filepath">csv file</param>
        private void WriteArray<T>(T[] lyrOut, string filepath)
        {
            string toWrite = string.Join(',', lyrOut);
            // Writing to debug
            Debug.WriteLine(toWrite);

            // Writing to File
            using (StreamWriter sw = File.AppendText(filepath))
            {
                sw.WriteLine(toWrite);
            }
        }

        private HomeostaticPlasticityController getHpc(Connections mem, int index)
        {

            // Homeostatic Plasticity Controller
            HomeostaticPlasticityController hpc1 = new HomeostaticPlasticityController(mem, 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                if (index == 1)
                {
                    isInStableState1 = isStable;
                }
                else
                {
                    isInStableState2 = isStable;
                }

            }, numOfCyclesToWaitOnChange: 50);
            return hpc1;
        }
    }
}