using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Diagnostics;
using HtmImageEncoder;
using NeoCortexApi.Classifiers;

namespace InvariantLearning
{
    internal class LearningUnit
    {

        private CortexLayer<object, object> cortexLayer;
        private bool isInStableState;
        HtmClassifier<string, ComputeCycle> cls;

        public LearningUnit(int inputDim, int columnDim)
        {
            // HTM CONFIG
            HtmConfig config = new HtmConfig(new int[] { inputDim*inputDim }, new int[]{columnDim});

            // CONNECTIONS
            Connections conn = new Connections(config);

            // HPC
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(conn, 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                this.isInStableState = isStable;

                // Clear active and predictive cells.
                //tm.Reset(mem);
            }, numOfCyclesToWaitOnChange: 50);

            // SPATIAL POOLER
            SpatialPooler sp = new SpatialPooler(hpc);
            sp.Init(conn);

            // IMAGE ENCODER
            ImageEncoder imgEncoder = new(new Daenet.ImageBinarizerLib.Entities.BinarizerParams()
            {
                Inverse = true,
                ImageHeight = inputDim,
                ImageWidth = inputDim,
                GreyScale = true,
            });  

            // CORTEX LAYER
            cortexLayer = new CortexLayer<object, object>("Invariant");
            cortexLayer.AddModule("encoder", imgEncoder);
            cortexLayer.AddModule("sp", sp);

            // STABLE STATE
            isInStableState = false;

            // HTM CLASSIFIER
            cls = new HtmClassifier<string, ComputeCycle>();
        }

        internal void Learn(InvImage sample)
        {
            // SPATIAL POOLER
            var SDR = cortexLayer.Compute(sample.imagePath, true) as ComputeCycle;

            if (isInStableState)
            {
                // HTM CLASSIFIER
                var activeColumns = cortexLayer.GetResult("sp") as int[];

                List<Cell> actCells;

                if (SDR.ActiveCells.Count == SDR.WinnerCells.Count)
                {
                    actCells = SDR.ActiveCells;
                }
                else
                {
                    actCells = SDR.WinnerCells;
                }

                cls.Learn(sample.label, actCells.ToArray());
            }
        }

        internal Dictionary<string, double> Predict(InvImage image)
        {
            throw new NotImplementedException();
        }
    }
}