using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VideoLibrary;

namespace HTMVideoLearning
{
    class VideoLearning
    {
        public void Run()
        {
            Stopwatch sw = new Stopwatch();
            List<TimeSpan> RecordedTime = new();

            HelperFunction.WriteLineColor($"Hello NeoCortexApi! Conducting experiment {nameof(VideoLearning)} Noath2302");
            HelperFunction.WriteLineColor("Please insert or drag the folder that contains the training files: ", ConsoleColor.Blue);
            string trainingFolderPath = Console.ReadLine();

            sw.Start();
            // Define first the desired properties of the frames
            string outputFolder = "Output";
            string convertedVideoDir = $"{outputFolder}//ConvertedVideo";
            if (!Directory.Exists($"{convertedVideoDir}"))
            {
                Directory.CreateDirectory($"{convertedVideoDir}");
            }
            int frameWidth = 10;
            int frameHeight = 10;
            VideoLibrary.ColorMode colorMode = ColorMode.BLACKWHITE;
            double frameRate = 10;

            // Define HTM parameters
            int[] inputBits = { frameWidth * frameHeight };
            int[] numColumns = { 1024 };

            // Define Reader for Videos
            // Input videos are stored in different folders under TrainingVideos/
            // with their folder's names as label value. To get the paths of all folders:
            string[] videoSetPaths = HelperFunction.GetVideoSetPaths(trainingFolderPath);

            // A list of VideoSet object, each has the Videos and the name of the folder as Label, contains all the Data in TrainingVideos,
            // this List will be the core iterator in later learning and predicting
            List<VideoSet> videoData = new();

            // Because of the continous learning of the Frames in the Spatial Pooler
            // A list of tempInput is used to stored all the encoded frames from all Videos
            List<int[]> tempInput = new();

            // Iterate through every folder in TrainingVideos/ to create VideoSet: object that stores video of same folder/label
            foreach (string path in videoSetPaths)
            {
                VideoSet vs = new VideoSet(path, colorMode, frameWidth, frameHeight, frameRate);
                videoData.Add(vs);
                vs.CreateConvertedVideos(convertedVideoDir);
            }
            // Iterate through every folder in TrainingVideos/

            foreach (VideoSet set in videoData)
            {
                // Show Set Label/ Folder Name of each video set
                HelperFunction.WriteLineColor($"VIDEO SET LABEL: {set.VideoSetLabel}", ConsoleColor.Cyan);
                foreach (NVideo vid in set.nVideoList)
                {
                    // Show the name of each video
                    HelperFunction.WriteLineColor($"    VIDEO NAME: {vid.name}", ConsoleColor.DarkCyan);
                    foreach (NFrame frame in vid.nFrames)
                    {
                        // Show the encoded content of each frame in a video, these will be used as SP learning input
                        //Console.WriteLine($"      Frame : {frame.ArrToString()}");

                        // FOR RUNNING EXPERIMENT AT THIS POINT
                        // all frame encoded binary array are stored in tempInput
                        tempInput.Add(frame.EncodedBitArray);
                    }
                }
            }

            //Initiating HTM
            HtmConfig cfg = GetHTM(inputBits, numColumns);

            var mem = new Connections(cfg);

            HtmClassifier<int[], ComputeCycle> cls = new HtmClassifier<int[], ComputeCycle>();

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            TemporalMemory tm = new TemporalMemory();

            bool isInStableState = false;

            bool learn = true;

            int maxNumOfElementsInSequence = 0;

            foreach (VideoSet vd in videoData)
            {
                int currentSetFrameCount = vd.GetLongestFramesCountInSet();
                if (currentSetFrameCount > maxNumOfElementsInSequence)
                {
                    maxNumOfElementsInSequence = currentSetFrameCount;
                }
            }

            int maxCycles = 200;
            int newbornCycle = 0;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, maxNumOfElementsInSequence * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Console.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Console.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                // We are not learning in instable state.
                learn = isInStableState = isStable;

                // Clear all learned patterns in the classifier.
                cls.ClearState();

            }, numOfCyclesToWaitOnChange: 50);

            SpatialPoolerMT sp = new SpatialPoolerMT(hpa);
            sp.Init(mem);
            tm.Init(mem);
            layer1.HtmModules.Add("sp", sp);

            //
            // Training SP to get stable. New-born stage.
            //
            for (int i = 0; i < maxCycles; i++)
            {
                newbornCycle++;

                Console.WriteLine($"-------------- Newborn Cycle {newbornCycle} ---------------");

                foreach (VideoSet set in videoData)
                {
                    // Show Set Label/ Folder Name of each video set
                    HelperFunction.WriteLineColor($"VIDEO SET LABEL: {set.VideoSetLabel}", ConsoleColor.Cyan);
                    foreach (NVideo vid in set.nVideoList)
                    {
                        // Show the name of each video
                        HelperFunction.WriteLineColor($"    VIDEO NAME: {vid.name}", ConsoleColor.DarkCyan);
                        foreach (NFrame frame in vid.nFrames)
                        {
                            //reserved for displaying function
                            Console.WriteLine($" -- {frame.FrameKey} --");

                            var lyrOut = layer1.Compute(frame.EncodedBitArray, learn);

                            if (isInStableState)
                                break;
                        }
                    }
                }

                if (isInStableState)
                    break;
            }

            layer1.HtmModules.Add("tm", tm);

            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            HelperFunction.WriteLineColor("Please insert or drag the folder that contains the test Videos: ", ConsoleColor.Blue);
            string testVideosPath = Console.ReadLine();
            VideoSet testVideo = new(testVideosPath, colorMode, frameWidth, frameHeight, frameRate);

            foreach (NVideo nv in testVideo.nVideoList)
            {
                List<NFrame> inputVideo = nv.nFrames;
                List<int[]> stableAreas = new List<int[]>();

                List<string> lastPredictedValue = new List<string>();

                int maxPrevInputs = inputVideo.Count - 1;
                List<string> previousFrames = new List<string>();
                previousFrames.Add("_placeholder_");

                stableAreas.Clear();
                cycle = 0;
                learn = true;
                sw.Reset();
                sw.Start();
                int maxMatchCnt = 0;
                //
                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    matches = 0;

                    cycle++;

                    Console.WriteLine($"-------------- Cycle {cycle} ---------------");

                    foreach (var input in inputVideo)
                    {
                        Console.WriteLine($"-------------- {input.FrameKey} ---------------");

                        var lyrOut = layer1.Compute(input.EncodedBitArray, learn) as ComputeCycle;

                        // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.
                        var activeColumns = layer1.GetResult("sp") as int[];

                        previousFrames.Add(input.FrameKey);
                        if (previousFrames.Count > (maxPrevInputs + 1))
                            previousFrames.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousFrames.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousFrames);

                        List<Cell> actCells;

                        if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                        {
                            actCells = lyrOut.ActiveCells;
                        }
                        else
                        {
                            actCells = lyrOut.WinnerCells;
                        }

                        cls.Learn(nv.GetEncodedFrame(key), actCells.ToArray());

                        if (learn == false)
                            Console.WriteLine($"Inference mode");

                        Console.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Console.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        if (lastPredictedValue.Contains(key))
                        {
                            matches++;
                            Console.WriteLine($"Match. Actual value: {key} - Predicted value: {key}");
                            lastPredictedValue.Clear();
                        }
                        else
                        {
                            Console.WriteLine($"Mismatch! Actual value: {key} - Predicted values: {String.Join(',', lastPredictedValue)}");
                            lastPredictedValue.Clear();
                        }

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Console.WriteLine($"Current Input: {input} \t| Predicted Input: {item.PredictedInput.ArrToString()}");
                                lastPredictedValue.Add(item.PredictedInput.ArrToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValue.Clear();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// <para>Initiate the settings of HTM</para>
        /// </summary>
        /// <param name="inputBits">number of bit in input array</param>
        /// <param name="numColumns">number of columns in SDR</param>
        /// <returns></returns>
        private HtmConfig GetHTM(int[] inputBits, int[] numColumns)
        {
            HtmConfig htm = new HtmConfig(inputBits, numColumns)
            {
                CellsPerColumn = 25,

                MaxBoost = 10.0,
                NumActiveColumnsPerInhArea = (int)(0.02 * numColumns[0]),
                MaxSynapsesPerSegment = (int)(0.02 * numColumns[0]),
            };
            return htm;
        }
        private static string GetKey(List<string> prevInputs)
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
