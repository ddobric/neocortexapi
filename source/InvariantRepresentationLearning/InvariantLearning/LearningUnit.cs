using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Diagnostics;
using HtmImageEncoder;
using NeoCortexApi.Classifiers;

using Invariant.Entities;
using SkiaSharp;

namespace InvariantLearning
{
    public class LearningUnit
    {
        public string OutputPredictFolder = "";
        private CortexLayer<object, object> cortexLayer;
        private bool isInStableState;

        public int inputDim;
        private int columnDim;
        public HtmClassifier<string, int[]> classifier;


        public LearningUnit(int inputDim, int columnDim)
        {
            this.inputDim = inputDim;
            this.columnDim = columnDim;

            // CortexLayer
            cortexLayer = new CortexLayer<object, object>("Invariant");

            // HTM CLASSIFIER
            classifier = new HtmClassifier<string, int[]>();
        }

        /// <summary>
        /// Training with newborn cycle before Real Learning of DataSet
        /// </summary>
        /// <param name="trainingDataSet">the Training Dataset</param>
        public void TrainingNewbornCycle(DataSet trainingDataSet)
        {
            // HTM CONFIG
            HtmConfig config = new HtmConfig(new int[] { inputDim * inputDim }, new int[] { columnDim });

            // CONNECTIONS
            Connections conn = new Connections(config);

            // HPC
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(conn, trainingDataSet.Count * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
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
            cortexLayer.AddModule("encoder", imgEncoder);
            cortexLayer.AddModule("sp", sp);

            // STABLE STATE
            isInStableState = false;

            // New Born Cycle Loop
            int cycle = 0;
            while (!this.isInStableState)
            {
                Debug.Write($"Cycle {cycle}: ");
                foreach (var sample in trainingDataSet.Images)
                {
                    Debug.Write(".");
                    cortexLayer.Compute(sample.imagePath, true);
                }
                Debug.Write("\n");
                cycle++;
            }

            Console.WriteLine("Learning Unit reach stable state in newborn learning");
        }


        /// <summary>
        /// Training after newborncycle with a training dataset
        /// </summary>
        /// <param name="trainingDataSet"></param>
        /// <param name="epoch">number of training cycle</param>
        public void TrainingNormal(DataSet trainingDataSet, int epoch = 0)
        {
            if (epoch == 0)
            {
                // for loop with epochs
                for (int cycle = 1; cycle <= epoch; cycle += 1)
                {
                    // for loop with training:
                    foreach (var sample in trainingDataSet.Images)
                    {
                        this.Learn(sample);
                    }
                }
            }
            else
            {
                isInStableState = false;
                while (!isInStableState)
                {
                    // for loop with training:
                    foreach (var sample in trainingDataSet.Images)
                    {
                        this.Learn(sample);
                        if (isInStableState)
                        {
                            Console.WriteLine("Learning Unit reach stable state in normal learning");
                            break;
                        }
                    }
                }
            }
        }

        public void Learn(Picture sample)
        {
            Debug.WriteLine($"Label: {sample.label}___{Path.GetFileNameWithoutExtension(sample.imagePath)}");

            // Claculates the SDR of Active Columns.
            cortexLayer.Compute(sample.imagePath, true);

            var activeColumns = cortexLayer.GetResult("sp") as int[];

            // Uncomment this to learn the whole label + name
            // classifier.Learn($"{sample.label}_{Path.GetFileName(sample.imagePath)}", activeColumns);

            // Uncomment this to learn only the label
            classifier.Learn(sample.label, activeColumns);
        }

        /// <summary>
        /// Predicting image without using frame
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Dictionary<string, string> PredictScaledImage(Picture image, string predictOutputPath)
        {
            // Create the folder for the frame extracted by InvImage
            string spFolder = Path.Combine(predictOutputPath, OutputPredictFolder, $"SP of {inputDim}x{inputDim}");
            Utility.CreateFolderIfNotExist(spFolder);

            // dictionary for saving result
            Dictionary<string, string> result = new Dictionary<string, string>();

            var frameMatrix = Frame.GetConvFramesbyPixel(image.imageWidth, image.imageHeight, inputDim, inputDim, 5);

            int lastXIndex = image.imageWidth - 1;
            int lastYIndex = image.imageHeight - 1;
            Frame frame = new Frame(0, 0, lastXIndex, lastYIndex);
            if (image.IsRegionBelowDensity(frame, 0.05))
            {
                return result;
            }
            else
            {
                // Save frame to folder
                string outFile = Path.Combine(spFolder, $"testImage of label {image.label} index {Path.GetFileNameWithoutExtension(image.imagePath)}.png");
                Picture.SaveAsImage(image.GetPixels(), outFile);

                // Compute the SDR
                var sdr = cortexLayer.Compute(outFile, false) as int[];

                // Get Predicted Labels
                var predictedLabel = classifier.GetPredictedInputValues(sdr, 10);

                // Check if there are Predicted Label ?
                if (predictedLabel.Count != 0)
                {
                    foreach (var a in predictedLabel)
                    {
                        Debug.WriteLine($"Predicting image: {image.imagePath}");
                        Debug.WriteLine($"label predicted as : {a.PredictedInput}");
                        Debug.WriteLine($"similarity : {a.Similarity}");
                        Debug.WriteLine($"Number of Same Bits: {a.NumOfSameBits}");
                        result.Add(a.PredictedInput, a.Similarity.ToString());
                    }
                }
            }
            return result;
        }
        public Dictionary<string, double> PredictWithFrameGrid(Picture image)
        {
            // Create the folder for the frame extracted by InvImage
            string spFolder = Path.Combine("Predict", OutputPredictFolder, $"SP of {inputDim}x{inputDim}");
            Utility.CreateFolderIfNotExist(spFolder);

            // dictionary for saving result
            Dictionary<string, double> result = new Dictionary<string, double>();
            Dictionary<string, string> allResultForEachFrame = new Dictionary<string, string>();

            var frameMatrix = Frame.GetConvFramesbyPixel(image.imageWidth, image.imageHeight, inputDim, inputDim, 5);


            foreach (var frame in frameMatrix)
            {
                if (image.IsRegionBelowDensity(frame, 0.05))
                {
                    continue;
                }
                else
                {
                    // Save frame to folder
                    string outFile = Path.Combine(spFolder, $"frame__{frame.tlX}_{frame.tlY}.png");
                    Picture.SaveAsImage(image.GetPixels(frame), outFile);

                    // Compute the SDR
                    var sdr = cortexLayer.Compute(outFile, false) as int[];

                    // Get Predicted Labels
                    var predictedLabel = classifier.GetPredictedInputValues(sdr, 1);

                    // Check if there are Predicted Label ?
                    if (predictedLabel.Count != 0)
                    {
                        foreach (var a in predictedLabel)
                        {
                            Debug.WriteLine($"Predicting image: {image.imagePath}");
                            Debug.WriteLine($"label predicted as : {a.PredictedInput}");
                            Debug.WriteLine($"similarity : {a.Similarity}");
                            Debug.WriteLine($"Number of Same Bits: {a.NumOfSameBits}");
                        }
                    }

                    allResultForEachFrame.Add(outFile, GetStringFromResult(predictedLabel));
                    // Aggregate Label to Result
                    AddResult(ref result, predictedLabel, frameMatrix.Count);
                }
            }
            Utility.WriteResultOfOneSPDetailed(allResultForEachFrame, Path.Combine(spFolder, $"SP of {inputDim}x{inputDim} detailed.csv"));
            return result;
        }


        private string GetStringFromResult(List<ClassifierResult<string>> predictedLabel)
        {
            string resultString = "";
            foreach (ClassifierResult<string> result in predictedLabel)
            {
                resultString += $",{result.PredictedInput}__{result.NumOfSameBits}_{result.Similarity}";
            }
            return resultString;
        }

        private void AddResult(ref Dictionary<string, double> result, List<ClassifierResult<string>> predictedLabel, int frameCount)
        {
            Dictionary<string, double> res = new Dictionary<string, double>();
            foreach (var label in predictedLabel)
            {
                if (result.ContainsKey(label.PredictedInput))
                {
                    result[label.PredictedInput] += label.Similarity / frameCount;
                }
                else
                {
                    result.Add(label.PredictedInput, label.Similarity / frameCount);
                }
            }
        }
    }
}