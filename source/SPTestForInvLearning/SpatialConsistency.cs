using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;

namespace SPTestForInvLearning
{
    [TestClass]
    public class SpatialConsistency
    {
        private bool isInStableState1 = false;
        private bool isInStableState2 = false;

        /// <summary>
        /// This is test for Spatial Pooler consistency. 2 Spatial Pooler will undergo same learning for 1 pattern, to see if they behave the same in case of same random seed
        /// </summary>
        [TestMethod]
        [TestCategory("Invariant Learning")]
        public void SPConsistencyTest()
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
            SpatialPoolerMT sp1 = new SpatialPoolerMT(hpc1);
            SpatialPoolerMT sp2 = new SpatialPoolerMT(hpc2);

            sp1.Init(mem1);
            sp2.Init(mem2);

            RecordOutput(sp1, input, ref this.isInStableState1, path1);
            RecordOutput(sp2, input, ref this.isInStableState2, path2);

        }

        private void RecordOutput(SpatialPoolerMT sp, int[] input, ref bool isInStableState, string path)
        {
            // Experiment parameter
            int MaxCycle = 1000;
            int currentCycle = 0;



            // While loop
            while (!isInStableState)
            {
                var lyrOut = sp.Compute(input, true);
                WriteArray(lyrOut, path);

                // Outing 
                if (currentCycle >= MaxCycle)
                {
                    Debug.WriteLine($"exp ends by reaching MAXCYCLE = {MaxCycle}");
                    break;
                }
            }
        }
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
        private void WriteArray(int[] lyrOut, string filepath)
        {
            string toWrite = string.Join(",", lyrOut);
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