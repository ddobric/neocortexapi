using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
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
            string outputFolder = "output";
            string convertedVideoDir = $"{outputFolder}" + @"\" + "converted";
            if (!Directory.Exists($"{convertedVideoDir}"))
            {
                Directory.CreateDirectory($"{convertedVideoDir}");
            }
            int frameWidth = 18;
            int frameHeight = 18;
            ColorMode colorMode = ColorMode.BLACKWHITE;
            double frameRate = 10;
            // adding condition for 
            // Define HTM parameters
            int[] inputBits = { frameWidth * frameHeight * (int)colorMode};
            int[] numColumns = { 1024 };

            // Define Reader for Videos
            // Input videos are stored in different folders under TrainingVideos/
            // with their folder's names as label value. To get the paths of all folders:
            string[] videoSetPaths = HelperFunction.GetVideoSetPaths(trainingFolderPath);

            // A list of VideoSet object, each has the Videos and the name of the folder as Label, contains all the Data in TrainingVideos,
            // this List will be the core iterator in later learning and predicting
            List<VideoSet> videoData = new();

            // Iterate through every folder in TrainingVideos/ to create VideoSet: object that stores video of same folder/label
            foreach (string path in videoSetPaths)
            {
                VideoSet vs = new VideoSet(path, colorMode, frameWidth, frameHeight, frameRate);
                videoData.Add(vs);
                vs.CreateConvertedVideos(convertedVideoDir);
            }
            //Initiating HTM
            HtmConfig cfg = GetHTM(inputBits, numColumns);

            var mem = new Connections(cfg);

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

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

            int maxCycles = 100;
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

            SpatialPoolerMT sp = new(hpa);
            sp.Init(mem);
            tm.Init(mem);
            layer1.HtmModules.Add("sp", sp);
            
            //
            // Training SP to get stable. New-born stage.
            //
            ///*
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
                            //Console.WriteLine($" -- {frame.FrameKey} --");

                            var lyrOut = layer1.Compute(frame.EncodedBitArray, learn);

                            if (isInStableState)
                                break;
                        }
                    }
                }

                if (isInStableState)
                    break;
            }
            //*/
            

            layer1.HtmModules.Add("tm", tm);
            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;


            foreach (VideoSet vd in videoData) 
            {
                foreach (NVideo nv in vd.nVideoList)
                {
                    List<NFrame> inputVideo = nv.nFrames;
                    cycle = 0;
                    learn = true;
                    sw.Reset();
                    sw.Start();

                    //
                    // Now training with SP+TM. SP is pretrained on the given VideoSet.
                    // There is a little different between a input pattern set and an input video set,
                    // The reason is because a video consists of continously altering frame, not distinct values like the sequence learning of Scalar value.
                    // Thus Learning with sp alone was kept
                    for (int i = 0; i < maxCycles; i++)
                    {
                        matches = 0;

                        cycle++;

                        Console.WriteLine($"-------------- Cycle {cycle} ---------------");

                        foreach (var currentFrame in inputVideo)
                        {
                            Console.WriteLine($"-------------- {currentFrame.FrameKey} ---------------");
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;

                            Console.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));
                            // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.
                            var activeColumns = layer1.GetResult("sp") as int[];
                            Console.WriteLine(string.Join(',', activeColumns));

                            List<Cell> actCells;

                            HelperFunction.WriteLineColor($"WinnerCell Count: {lyrOut.WinnerCells.Count}", ConsoleColor.Cyan);
                            HelperFunction.WriteLineColor($"ActiveCell Count: {lyrOut.ActiveCells.Count}", ConsoleColor.Cyan);
                            if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                            {
                                actCells = lyrOut.ActiveCells;
                            }
                            else
                            {
                                actCells = lyrOut.WinnerCells;
                            }

                            cls.Learn(currentFrame.FrameKey, actCells.ToArray());
                            /*
                            if (lyrOut.PredictiveCells.Count > 0)
                            {
                                HelperFunction.WriteLineColor("Predicted Values for current frame: ", ConsoleColor.Yellow);
                                Cell[] cellArray = lyrOut.PredictiveCells.ToArray();
                                foreach(Cell nCell in cellArray)
                                {
                                    HelperFunction.WriteLineColor(nCell.ToString(),ConsoleColor.Yellow);
                                    
                                }
                                //string a = Console.ReadLine();
                                var predictedFrames = cls.GetPredictedInputValues(cellArray,5);
                                foreach (var item in predictedFrames)
                                {
                                    Debug.WriteLine($"Current Input: {currentFrame.FrameKey} \t| Predicted Input: {item.PredictedInput}");
                                    Console.WriteLine($"{item.PredictedInput} -- similarity{item.Similarity} -- NumberOfSameBit {item.NumOfSameBits}");
                                }
                                
                            }
                            else
                            {
                                HelperFunction.WriteLineColor($"CURRENT FRAME: {currentFrame.FrameKey}", ConsoleColor.Red);
                                HelperFunction.WriteLineColor("NO PREDICTED CELLS FOR THIS FRAME",ConsoleColor.Red);
                            }
                            */
                        }
                        // Inferring Mode
                        if (i>20)
                        {
                        learn = false;
                        List<List<string>> possibleOutcomeSerie = new();
                        possibleOutcomeSerie.Add(new List<string> { inputVideo[0].FrameKey });
                        List<string> possibleOutcome = new();
                        tm.Reset(mem);
                        foreach (NFrame currentFrame in inputVideo)
                        {
                            // Inferring the current frame encoded bit array with learned SP
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;
                            var nextFramePossibilities = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var nextFrame in nextFramePossibilities)
                            {
                                if (nextFrame.Similarity > 0)
                                {
                                    possibleOutcome.Add(nextFrame.PredictedInput);
                                }
                            }
                            possibleOutcomeSerie.Add(new(possibleOutcome));
                            possibleOutcome.Clear();
                        }
                        foreach (var outcome in possibleOutcomeSerie)
                        {
                            HelperFunction.WriteLineColor(string.Join("---", outcome), ConsoleColor.Green);
                        }
                        learn = true;
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
        private static HtmConfig GetHTM(int[] inputBits, int[] numColumns)
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
        /// <summary>
        /// Get the key for HTMClassifier learning stage.
        /// The key here is a serie of frames' keys, seperated by "-"
        /// </summary>
        /// <param name="prevInputs"></param>
        /// <returns></returns>
        private static string GetKey(List<string> prevInputs)
        {
            string key = string.Join("-", prevInputs);
            return key;
        }
    }
}
