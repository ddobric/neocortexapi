using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System.Diagnostics;
using HtmImageEncoder;
using NeoCortexApi.Classifiers;

using Invariant.Entities;

namespace InvariantLearning
{
    /// <summary>
    /// <br>Learning Unit is a Combination of Image Encoder, Spatial Pooler and Classifier in one Place</br>
    /// <br></br>
    /// </summary>
    public class LearningUnit
    {
        public string OutFolder;
        private CortexLayer<object, object> cortexLayer;

        private bool isInStableState;

        public int width;
        public int height;
        
        public int columnDim;
        
        public HtmClassifier<string, int[]> classifier;

        public string Id { get => $"_{width}x{height}_";}

        public LearningUnit(int width, int height, int numColumn, string outFolder)
        {
            this.width = width;
            this.height = height;
            this.columnDim = numColumn;
            this.OutFolder = outFolder;

            cortexLayer = new CortexLayer<object, object>(this.Id);

            classifier = new HtmClassifier<string, int[]>();
        }

        /// <summary>
        /// <br>start training in Newborn Cycle to reach stable state of SP with the training dataset.</br>
        /// <br>after the stable state, LearningUnit spends 2 more cycles to learn all labels for each images from the DataSet</br>
        /// </summary>
        /// <param name="trainingDataSet"></param>
        public void TrainAndLearn(DataSet trainingDataSet)
        {
            TrainingNewbornCycle(trainingDataSet);
            TrainingNormal(trainingDataSet, 2);
        }

        /// <summary>
        /// Training in newborn cycle
        /// </summary>
        /// <param name="trainingDataSet">the Training Dataset</param>
        public void TrainingNewbornCycle(DataSet trainingDataSet)
        {
            // HTM CONFIG
            HtmConfig config = new HtmConfig(new int[] { width * height }, new int[] { columnDim })
            {
                Random = new Random(15676),
                DutyCyclePeriod = 1000
            };

            // CONNECTIONS
            Connections conn = new Connections(config);

            // HPC
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(conn, trainingDataSet.Count * 10, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // Toggle Stability
                this.isInStableState = isStable;

            }, numOfCyclesToWaitOnChange: 50);

            // SPATIAL POOLER
            SpatialPooler sp = new SpatialPooler(hpc);
            sp.Init(conn);

            // IMAGE ENCODER
            ImageEncoder imgEncoder = new(new Daenet.ImageBinarizerLib.Entities.BinarizerParams()
            {
                Inverse = false,
                ImageHeight = height,
                ImageWidth = width,
                GreyScale = true,
            });

            // Building Cortex Layer
            cortexLayer.AddModule("encoder", imgEncoder);
            cortexLayer.AddModule("sp", sp);

            // Stable State Check
            isInStableState = false;

            // Training In New Born State
            int cycle = 0;
            int maxCycle = 500;
            while (cycle < maxCycle)
            {
                Debug.Write($"Cycle {cycle}: ");
                foreach (var sample in trainingDataSet.Images)
                {
                    Debug.Write(".");
                    cortexLayer.Compute(sample.ImagePath, true);
                }
                Debug.Write("\n");

                if (this.isInStableState)
                {
                    break;
                }
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
            if (epoch != 0)
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

        public void Learn(Image sample)
        {
            Debug.WriteLine($"Label: {sample.Label}___{Path.GetFileNameWithoutExtension(sample.ImagePath)}");

            // Claculates the SDR of Active Columns.
            cortexLayer.Compute(sample.ImagePath, true);

            var activeColumns = cortexLayer.GetResult("sp") as int[];

            // Uncomment this to learn the whole label + name
            // classifier.Learn($"{sample.label}_{Path.GetFileName(sample.imagePath)}", activeColumns);

            // Uncomment this to learn only the label
            classifier.Learn(sample.Label, activeColumns);
        }

        /// <summary>
        /// Predicting image without using frame
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Dictionary<string, string> PredictScaledImage(Image image, string predictOutputPath)
        {
            // Create the folder for the frame extracted by InvImage
            string spFolder = Path.Combine(predictOutputPath, OutFolder, $"SP of {width}x{height}");
            Utility.CreateFolderIfNotExist(spFolder);

            // dictionary for saving result
            Dictionary<string, string> result = new Dictionary<string, string>();

            var frameMatrix = Frame.GetConvFramesbyPixel(image.ImageWidth, image.ImageHeight, width, height, 5);

            int lastXIndex = image.ImageWidth - 1;
            int lastYIndex = image.ImageHeight - 1;
            Frame frame = new Frame(0, 0, lastXIndex, lastYIndex);
            if (image.IsRegionBelowDensity(frame, 0.05))
            {
                return result;
            }
            else
            {
                // Save frame to folder
                string outFile = Path.Combine(spFolder, $"testImage of label {image.Label} index {Path.GetFileNameWithoutExtension(image.ImagePath)}.png");
                Image.SaveTo(image.GetPixels(), outFile);

                // Compute the SDR
                var sdr = cortexLayer.Compute(outFile, false) as int[];

                // Get Predicted Labels
                var predictedLabel = classifier.GetPredictedInputValues(sdr, 10);

                // Check if there are Predicted Label ?
                if (predictedLabel.Count != 0)
                {
                    foreach (var a in predictedLabel)
                    {
                        Debug.WriteLine($"Predicting image: {image.ImagePath}");
                        Debug.WriteLine($"label predicted as : {a.PredictedInput}");
                        Debug.WriteLine($"similarity : {a.Similarity}");
                        Debug.WriteLine($"Number of Same Bits: {a.NumOfSameBits}");
                        result.Add(a.PredictedInput, a.Similarity.ToString());
                    }
                }
            }
            return result;
        }
        public Dictionary<string, double> PredictWithFrameGrid(Image image)
        {
            // Create the folder for the frame extracted by InvImage
            string spFolder = Path.Combine("Predict", OutFolder, $"SP of {width}x{height}");
            Utility.CreateFolderIfNotExist(spFolder);

            // dictionary for saving result
            Dictionary<string, double> result = new Dictionary<string, double>();
            Dictionary<string, string> allResultForEachFrame = new Dictionary<string, string>();

            var frameMatrix = Frame.GetConvFramesbyPixel(image.ImageWidth, image.ImageHeight, width, height, 5);


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
                    Image.SaveTo(image.GetPixels(frame), outFile);

                    // Compute the SDR
                    var sdr = cortexLayer.Compute(outFile, false) as int[];

                    // Get Predicted Labels
                    var predictedLabel = classifier.GetPredictedInputValues(sdr, 1);

                    // Check if there are Predicted Label ?
                    if (predictedLabel.Count != 0)
                    {
                        foreach (var a in predictedLabel)
                        {
                            Debug.WriteLine($"Predicting image: {image.ImagePath}");
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
            Utility.WriteResultOfOneSPDetailed(allResultForEachFrame, Path.Combine(spFolder, $"SP of {width}x{height} detailed.csv"));
            return result;
        }

        public int[] Predict(string imagePath)
        {
            cortexLayer.Compute(imagePath, false);

            var activeColumns = cortexLayer.GetResult("sp") as int[];
            if (activeColumns == null || activeColumns.Length == 0)
            {
                return new int[columnDim];
            }

            return ToSDRBinArray(activeColumns);
        }

        private int[] ToSDRBinArray(int[] activeColumns)
        {
            int[] res = new int[columnDim];

            for (int i = 0; i < activeColumns.Length; i += 1)
            {
                res[activeColumns[i]] = 1;
            }
            return res;
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