using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using InvariantLearning;
using Invariant.Entities;
using MnistDataGen;
using System.Collections.Concurrent;
using NeoCortexApi.Encoders;

namespace InvariantLearning
{
    public class InvariantLearning
    {
        public static void Main()
        {
            string experimentTime = DateTime.UtcNow.ToLongDateString().ToString().Replace(',',' ') +" "+DateTime.UtcNow.ToLongTimeString().ToString().Replace(':','_');
            //ExperimentPredictingWithFrameGrid();
            //ExperimentNormalImageClassification();
            //LocaDimensionTest();
            ExperimentEvaluatateImageClassification();
            // Invariant Learning Experiment
            InvariantRepresentation($"HtmInvariantLearning {experimentTime}");
            //SPCapacityTest();
        }

        private static void InvariantRepresentation(string experimentFolder)
        {
            Utility.CreateFolderIfNotExist(experimentFolder);

            // Get folder of MNIST images
            string sourceMNIST = Path.Combine(experimentFolder, "MnistSource");
            Utility.CreateFolderIfNotExist(sourceMNIST);
            Mnist.DataGen("MnistDataset", sourceMNIST, 20);

            // generate 32x32 MNISTDataSet
            int width = 32; int height = 32;
            DataSet sourceSet = new DataSet(sourceMNIST);
            DataSet sourceSet_32x32 = ScaleSet(experimentFolder, width, height, sourceSet);

            string MnistTestFolder = Path.Combine(experimentFolder, "TestSet");
            Utility.CreateFolderIfNotExist(MnistTestFolder);
            Mnist.TestDataGen("MnistDataSet", MnistTestFolder, 10);

            DataSet scaledTestSet = DataSet.CreateTestSet(sourceSet_32x32, 10, 100, 100, Path.Combine(experimentFolder,"testSet_32x32"));
            // write extracted/filtered frame from 32x32 dataset into 4x4 for SP to learn all pattern
            var listOfFrame = Frame.GetConvFrames(width, height, 4, 4, 8, 8);

            string extractedFrameFolder = Path.Combine(experimentFolder, "extractedFrame");
            int index = 0;
            List<string> frameDensityList = new List<string>();
            foreach (var image in sourceSet_32x32.Images)
            {
                foreach (var frame in listOfFrame)
                {
                    if (image.IsRegionOverBinarizedThreshold(frame, 255 / 2))
                    {
                        Utility.CreateFolderIfNotExist(Path.Combine(extractedFrameFolder, $"{index}"));
                        if (!ExistImageInDataSet(image, extractedFrameFolder, frame))
                        {
                            string savePath = Path.Combine(extractedFrameFolder, $"{index}", $"{index}.png");
                            string extractedFrameFolderBinarized = Path.Combine(experimentFolder, "extractedFrameBinarized");
                            Utility.CreateFolderIfNotExist(Path.Combine(extractedFrameFolderBinarized, $"{index}"));
                            string savePathOri = Path.Combine(extractedFrameFolderBinarized, $"{index}", $"{index}_ori.png");

                            image.SaveTo(savePath, frame, true);
                            image.SaveTo(savePathOri, frame);

                            frameDensityList.Add($"pattern {index}, Pixel Density {image.CalculateImageDensity(frame, 255 / 2) * 100}");
                            index += 1;
                        }
                    }
                }
            }
            File.WriteAllLines(Path.Combine(extractedFrameFolder, "PixelDensity.txt"), frameDensityList.ToArray());
            DataSet extractedFrameSet = new DataSet(extractedFrameFolder);

            // Learning the filtered frame set with SP
            LearningUnit spLayer1 = new LearningUnit(32, 2048);
            spLayer1.TrainingNewbornCycle(extractedFrameSet);
            // spLayer1.TrainingNormal(extractedFrameSet, 1);

            string extractedImageSource = Path.Combine(experimentFolder, "extractedSet");
            Utility.CreateFolderIfNotExist(extractedImageSource);

            // Saving representation/semantic array with its label in files
            Dictionary<string, List<int[]>> lib = new Dictionary<string, List<int[]>>();

            foreach (var image in sourceSet_32x32.Images)
            {
                string extractedFrameFolderofImage = Path.Combine(extractedImageSource, $"{image.label}_{Path.GetFileNameWithoutExtension(image.imagePath)}");
                Utility.CreateFolderIfNotExist(extractedFrameFolderofImage);
                if (!lib.ContainsKey(image.label))
                {
                    lib.Add(image.label, new List<int[]>());
                }
                int[] current = new int[spLayer1.columnDim];
                foreach (var frame in listOfFrame)
                {
                    string frameImage = Path.Combine(extractedFrameFolderofImage, $"{frame.tlX}-{frame.tlY}_{frame.brX}-{frame.brY}.png");
                    image.SaveTo(frameImage, frame, true);
                    int[] a = spLayer1.Predict(frameImage);
                    current = Utility.AddArray(current, a);
                }
                lib[image.label].Add(current);
            }

            foreach (var a in lib)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(extractedImageSource, $"{a.Key}.txt")))
                {
                    foreach (var s in a.Value)
                    {
                        sw.WriteLine(string.Join(',', s));
                    }
                }
            }

            // Testing section, caculate accuracy
            string testFolder = Path.Combine(experimentFolder, "Test");
            Utility.CreateFolderIfNotExist(testFolder);
            int match = 0;
            listOfFrame = Frame.GetConvFrames(100, 100, 4, 4, 25, 25);
            foreach (var testImage in scaledTestSet.Images)
            {
                string testImageFolder = Path.Combine(testFolder, $"{testImage.label}_{Path.GetFileNameWithoutExtension(testImage.imagePath)}");
                Utility.CreateFolderIfNotExist(testImageFolder);
                testImage.SaveTo(Path.Combine(testImageFolder, "origin.png"));

                int[] current = new int[spLayer1.columnDim];
                foreach (var frame in listOfFrame)
                {
                    string frameImage = Path.Combine(testImageFolder, $"{frame.tlX}-{frame.tlY}_{frame.brX}-{frame.brY}.png");
                    testImage.SaveTo(frameImage, frame, true);
                    current = Utility.AddArray(current, spLayer1.Predict(frameImage));
                }
                string actualLabel = testImage.label;
                string predictedLabel = "";
                double lowestMatch = 10000;
                foreach (var digitClass in lib)
                {
                    string currentLabel = digitClass.Key;
                    foreach (var entry in digitClass.Value)
                    {
                        double arrayGeometricDistance = Utility.CalArrayUnion(entry, current);
                        if (arrayGeometricDistance < lowestMatch)
                        {
                            predictedLabel = currentLabel;
                            lowestMatch = arrayGeometricDistance;
                        }
                    }
                }
                if (actualLabel == predictedLabel)
                {
                    match += 1;
                }
            }
            Debug.WriteLine($"accuracy equals {(double)(((double)match) / ((double)scaledTestSet.Count))}");
        }

        private static DataSet ScaleSet(string experimentFolder, int width, int height, DataSet sourceSet)
        {
            string sourceMNIST_32x32 = Path.Combine(experimentFolder, $"MnistSource_{width}x{height}");
            Utility.CreateFolderIfNotExist(sourceMNIST_32x32);
            foreach (var image in sourceSet.Images)
            {
                string digitLabelFolder = Path.Combine(sourceMNIST_32x32, image.label);
                Utility.CreateFolderIfNotExist(digitLabelFolder);
                image.SaveTo_Scaled(Path.Combine(digitLabelFolder, Path.GetFileName(image.imagePath)), width, height);
            }
            DataSet sourceSet_32x32 = new DataSet(sourceMNIST_32x32);
            return sourceSet_32x32;
        }

        private static bool ExistImageInDataSet(Picture image, string extractedFrameFolder, Frame frame)
        {
            foreach(var dir in Directory.GetDirectories(extractedFrameFolder))
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    Picture a = new Picture(file, "test");
                    if (Picture.CheckArrayEqual(Picture.Binarize(image.GetPixels(frame),255/2),Picture.Binarize(a.GetPixels(), 255 / 2)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void SPCapacityTest()
        {
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", 100},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", (double)600}
            };

            EncoderBase encoder = new ScalarEncoder(settings);

            bool isInStableState = false;
            HtmConfig htm = new HtmConfig(new int[] { 100 },new int[] { 1024 });
            Connections conn = new(htm);
            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(conn, 600 * 100, (isStable, numPatterns, actColAvg, seenInputs) =>
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
            }, numOfCyclesToWaitOnChange: 100);
            SpatialPooler sp = new SpatialPooler(hpc);
            sp.Init(conn);
            while (!isInStableState)
            {
                for (double i = 0; i < 600; i += 1)
                {
                    sp.Compute(encoder.Encode(i),true);
                }
            }
            hpc.TraceState();
        }

        private static void LocaDimensionTest()
        {
            List<LearningUnit> sps = new List<LearningUnit>();
            List<int> spDim = new List<int> { 28, 50};
            foreach(var dim in spDim)
            {
                sps.Add(new LearningUnit(dim, 1024));
            }


            Picture four = new Picture(Path.Combine("LocalDimensionTest","4.png"),"4");
            Picture nine = new Picture(Path.Combine("LocalDimensionTest", "9.png"), "9");

            DataSet training = new DataSet(new List<Picture> { four, nine });

            foreach (var sp in sps)
            {
                sp.TrainingNewbornCycle(training);
                sp.TrainingNormal(training, 50);

                string similarityPath = Path.Combine("LocalDimensionTest_Res", $"SimilaritiesCalc__{sp.inputDim}");
                Utility.CreateFolderIfNotExist(similarityPath);

                var a = sp.classifier.TraceCrossSimilarity("4", "9");
                var j = sp.classifier.TraceCrossSimilarity("4", "9");
            }
        }

        private static void ExperimentEvaluatateImageClassification()
        {
            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            Utility.CreateFolderIfNotExist(config.ExperimentFolder);
            string pathToTrainDataFolder = config.PathToTrainDataFolder;
            string pathToTestDataFolder = config.PathToTestDataFolder;

            Mnist.DataGen("MnistDataset", Path.Combine(config.ExperimentFolder, pathToTrainDataFolder), 10);

            Utility.CreateFolderIfNotExist(Path.Combine(config.ExperimentFolder, pathToTrainDataFolder));
            DataSet trainingData = new DataSet(Path.Combine(config.ExperimentFolder, pathToTrainDataFolder));

            Utility.CreateFolderIfNotExist(Path.Combine(config.ExperimentFolder, pathToTestDataFolder));
            DataSet testingData = trainingData.GetTestData(10);
            testingData.VisualizeSet(Path.Combine(config.ExperimentFolder, pathToTestDataFolder));

            LearningUnit sp = new(40, 1024);

            sp.TrainingNewbornCycle(trainingData);

            sp.TrainingNormal(trainingData, config.runParams.Epoch);

            var allResult = new List<Dictionary<string, string>>();

            foreach (var testingImage in testingData.Images)
            {
                Utility.CreateFolderIfNotExist("TestResult");
                var res = sp.PredictScaledImage(testingImage, Path.Combine(config.ExperimentFolder, "TestResult"));
                res.Add("fileName", $"{testingImage.label}_{Path.GetFileName(testingImage.imagePath)}");
                res.Add("CorrectLabel", testingImage.label);
                allResult.Add(res);
            }
            Utility.WriteListToCsv(Path.Combine(config.ExperimentFolder, "TestResult", "testOutput"), allResult);
            Utility.WriteListToOutputFile(Path.Combine(config.ExperimentFolder, "TestResult", "testOutput"), allResult);

            var a = sp.classifier.RenderCorrelationMatrixToCSVFormat();
            File.WriteAllLines(Path.Combine(config.ExperimentFolder, "correlationMat.csv"), a);

            string similarityPath = Path.Combine(config.ExperimentFolder, "SimilaritiesCalc");
            Utility.CreateFolderIfNotExist(similarityPath);

        }

        private static void ExperimentNormalImageClassification()
        {
            // reading Config from json
            var config = Utility.ReadConfig("experimentParams.json");
            string pathToTrainDataFolder = config.PathToTrainDataFolder;

            //Mnist.DataGenAll("MnistDataset", "TrainingFolder");
            Mnist.DataGen("MnistDataset", "TrainingFolder", 10);

            List<DataSet> testingData = new List<DataSet>();
            List<DataSet> trainingData = new List<DataSet>();

            DataSet originalTrainingDataSet = new DataSet(pathToTrainDataFolder);

            int k = 5;

            (trainingData, testingData) = originalTrainingDataSet.KFoldDataSetSplitEvenly(k);

            ConcurrentDictionary<string, double> foldValidationResult = new ConcurrentDictionary<string, double>();

            Parallel.For(0, k, (i) =>
            //for (int i = 0; i < k; i += 1)
            {
                // Visualizing data in k-fold scenarios
                string setPath = $"DatasetFold{i}";
                string trainSetPath = Path.Combine(setPath, "TrainingData");
                Utility.CreateFolderIfNotExist(trainSetPath);
                trainingData[i].VisualizeSet(trainSetPath);

                string testSetPath = Path.Combine(setPath, "TestingData");
                Utility.CreateFolderIfNotExist(trainSetPath);
                testingData[i].VisualizeSet(testSetPath);

                // passing the training data to the training experiment
                InvariantExperimentImageClassification experiment = new(trainingData[i], config.runParams);

                // train the network
                experiment.Train(false);

                // Prediction phase
                Utility.CreateFolderIfNotExist($"Predict_{i}");

                List<string> currentResList = new List<string>();

                Dictionary<string, List<Dictionary<string, string>>> allResult = new Dictionary<string, List<Dictionary<string, string>>>();

                foreach (var testImage in testingData[i].Images)
                {
                    var result = experiment.Predict(testImage, i.ToString());

                    string testImageID = $"{testImage.label}_{Path.GetFileNameWithoutExtension(testImage.imagePath)}";
                    UpdateResult(ref allResult, testImageID, result);
                }
                double foldValidationAccuracy = CalculateAccuracy(allResult);

                foreach (var sp in allResult)
                {
                    string path = Path.Combine($"Predict_{i}", sp.Key);
                    Utility.WriteListToCsv(path, allResult[sp.Key]);
                }

                foldValidationResult.TryAdd($"Fold_{i}_accuracy", foldValidationAccuracy);
            });
            Utility.WriteResultOfOneSP(new Dictionary<string, double>(foldValidationResult), $"KFold_{k}_Validation_Result");
        }

        /// <summary>
        /// Calculate by averaging similarity prediction of the correct label
        /// </summary>
        /// <param name="allResult"></param>
        /// <returns></returns>
        private static double CalculateAccuracy(Dictionary<string, List<Dictionary<string, string>>> allResult)
        {
            List<double> spAccuracy = new List<double>();

            foreach (var spResult in allResult.Values)
            {
                List<double> similarityList = new List<double>();
                foreach (var imagePredictResult in spResult)
                {
                    if (imagePredictResult.ContainsKey(imagePredictResult["CorrectLabel"]))
                    {
                        similarityList.Add(Double.Parse(imagePredictResult[imagePredictResult["CorrectLabel"]]));
                    }
                    else
                    {
                        similarityList.Add(0.0);
                    }
                }
                spAccuracy.Add(similarityList.Average());
            }
            return spAccuracy.Average();
        }

        private static void UpdateResult(ref Dictionary<string, List<Dictionary<string, string>>> allResult, string testImageID, Dictionary<string, Dictionary<string, string>> result)
        {
            foreach (var spKey in result.Keys)
            {
                if (!allResult.ContainsKey(spKey))
                {
                    allResult.Add(spKey, new List<Dictionary<string, string>>());
                }
            }

            foreach (var spKey in allResult.Keys)
            {
                Dictionary<string, string> resultEntryOfOneSP = new Dictionary<string, string>();
                resultEntryOfOneSP.Add("fileName", testImageID);
                foreach (var labelPred in result[spKey])
                {
                    resultEntryOfOneSP.Add(labelPred.Key, labelPred.Value);
                }
                allResult[spKey].Add(resultEntryOfOneSP);
            }
        }

        /*
private static void ExperimentPredictingWithFrameGrid()
{
   // populate the training and testing dataset with Mnist DataGen
   Mnist.DataGen("MnistDataset", "TrainingFolder", 5);
   Mnist.TestDataGen("MnistDataset", "TestingFolder", 5);

   // reading Config from json
   var config = Utility.ReadConfig("experimentParams.json");
   string pathToTrainDataFolder = config.PathToTrainDataFolder;
   string pathToTestDataFolder = config.PathToTestDataFolder;

   // generate the training data
   DataSet trainingSet = new DataSet(pathToTrainDataFolder);

   // generate the testing data
   DataSet testingSet = new DataSet(pathToTestDataFolder);

   // passing the training data to the training experiment
   InvariantExperimentImageClassification experiment = new(trainingSet, config.runParams);

   // train the network
   experiment.Train(true);


   // using predict to classify image from dataset
   Utility.CreateFolderIfNotExist("Predict");
   List<string> currentResList = new List<string>();
   /*
   CancellationToken cancelToken = new CancellationToken();
   while (true)
   {
       if (cancelToken.IsCancellationRequested)
       {
           return;
       }
       // This can be later changed to the validation test
       var result = experiment.Predict(testingSet.PickRandom());
       Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");


       double accuracy = Utility.AccuracyCal(currentResList);
       currentResList.Add($"{result.Item1}_{result.Item2}");
       Utility.WriteOutputToFile(Path.Combine("Predict", "PredictionOutput"),result);
   }



   foreach (var testImage in testingSet.Images)
   {
       var result = experiment.Predict(testImage);

       Debug.WriteLine($"predicted as {result.Item1}, correct label: {result.Item2}");

       double accuracy = Utility.AccuracyCal(currentResList);

       currentResList.Add($"{result.Item1}_{result.Item2}");

       Utility.WriteOutputToFile(Path.Combine("Predict", $"{Utility.GetHash()}_____PredictionOutput of testImage label {testImage.label}"), result);
   }

}
*/
    }
}