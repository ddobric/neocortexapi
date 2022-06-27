using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Diagnostics;
using HtmImageEncoder;
using NeoCortexApi.Classifiers;
using Classifier;

namespace InvariantLearning
{
    internal class LearningUnit
    {
        public string OutputPredictFolder = "";
        private CortexLayer<object, object> cortexLayer;
        private bool isInStableState;
        Classifier<string> cls; 
        private int inputDim;

        public LearningUnit(int inputDim, int columnDim)
        {
            this.inputDim = inputDim;
            // HTM CONFIG
            HtmConfig config = new HtmConfig(new int[] { inputDim*inputDim }, new int[] { columnDim });

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
                Inverse = false,
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
            cls = new Classifier<string>();
        }

        internal void Learn(Picture sample)
        {
            // SPATIAL POOLER
            Debug.WriteLine($"Label: {sample.label}___{Path.GetFileNameWithoutExtension(sample.imagePath)}");
            var SDR = cortexLayer.Compute(sample.imagePath, true);

            if (isInStableState)
            {
                // SP CLASSIFIER
                var activeColumns = cortexLayer.GetResult("sp") as int[];
                cls.Learn(sample.label, activeColumns);
            }
        }

        internal Dictionary<string, double> Predict(Picture image)
        {
            // Create the folder for the frame extracted by InvImage
            string spFolder = Path.Combine("Predict", OutputPredictFolder, $"SP of {inputDim}x{inputDim}");
            Utility.CreateFolderIfNotExist(spFolder);

            // dictionary for saving result
            Dictionary<string, double> result = new Dictionary<string, double>();

            var frameMatrix = Frame.GetConvFramesbyPixel(image.imageWidth,image.imageHeight,inputDim,inputDim);

            foreach (var frame in frameMatrix) 
            {
                // Save frame to folder
                string outFile = Path.Combine(spFolder,$"frame__{frame.tlX}_{frame.tlY}.png");
                Picture.SaveAsImage(image.GetPixels(frame),outFile);

                // Compute the SDR
                var sdr = cortexLayer.Compute(outFile, false) as int[];

                // Get Predicted Labels
                var predictedLabel = cls.GetPredictedInputValues(sdr, 1);

                // Check if there are Predicted Label ?
                if (predictedLabel.Count!=0)
                {
                    foreach (var a in predictedLabel)
                    {
                        Debug.WriteLine($"Predicting image: {image.imagePath}");
                        Debug.WriteLine($"label predicted as : {a.PredictedInput}");
                        Debug.WriteLine($"similarity : {a.Similarity}");
                        Debug.WriteLine($"Number of Same Bits: {a.NumOfSameBits}");
                    }
                }
                // Aggregate Label to Result
                AddResult(ref result, predictedLabel, frameMatrix.Count);

            }
            return result;
        }

        private void AddResult(ref Dictionary<string, double> result, List<Classifier<string>.ClassifierResult<string>> predictedLabel, int frameCount)
        {
            Dictionary<string, double> res = new Dictionary<string, double>();
            foreach (var label in predictedLabel)
            {
                if (result.ContainsKey(label.PredictedInput))
                {
                    result[label.PredictedInput]+=label.NumOfSameBits/frameCount;
                }
                else
                {
                    result.Add(label.PredictedInput, label.NumOfSameBits/frameCount);
                }
            }
        }
    }
}