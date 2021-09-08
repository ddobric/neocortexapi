using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            string convertedVideoDir = $"{outputFolder}" + @"\" + "Converted";
            if (!Directory.Exists($"{convertedVideoDir}"))
            {
                Directory.CreateDirectory($"{convertedVideoDir}");
            }
            int frameWidth = 24;
            int frameHeight = 24;
            ColorMode colorMode = ColorMode.BLACKWHITE;
            double frameRate = 10;
            // adding condition for 
            // Define HTM parameters
            int[] inputBits = { frameWidth * frameHeight * (int)colorMode };
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

            int maxNumOfElementsInSequence = videoData[0].GetLongestFramesCountInSet();

            int maxCycles = 500;
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

            // Accuracy Check
            double cycleAccuracy = 0;
            double lastCycleAccuracy = 0;
            int stableAccuracyCount = 0;

            for (int i = 0; i < maxCycles; i++)
            {
                List<double> setAccuracy = new();
                HelperFunction.WriteLineColor($"------------- Cycle {i} -------------", ConsoleColor.Green);
                // Iterating through every video set
                foreach (VideoSet vd in videoData)
                {
                    List<double> videoAccuracy = new();
                    // Iterating through every video in a VideoSet
                    foreach (NVideo nv in vd.nVideoList)
                    {
                        List<NFrame> trainingVideo = nv.nFrames;
                        learn = true;
                        sw.Reset();
                        sw.Start();

                        // Now training with SP+TM. SP is pretrained on the provided training videos.
                        // Learning each frame in a video
                        foreach (var currentFrame in trainingVideo)
                        {
                            Console.WriteLine($"--------------SP+TM {currentFrame.FrameKey} ---------------");

                            // Calculating SDR from the current Frame
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;

                            Console.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));
                            // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.

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
                            // Using HTMClassifier to assign the current frame key with the Collumns Indicies array
                            cls.Learn(currentFrame.FrameKey, actCells.ToArray());

                            // Checking Predicted Cells of the current frame
                            // From experiment the number of Predicted cells increase over cycles and reach stability later.
                            if (lyrOut.PredictiveCells.Count > 0)
                            {
                                HelperFunction.WriteLineColor("Predicted Values for current frame: ", ConsoleColor.Yellow);

                                // Checking the Predicted Cells by printing them out
                                Cell[] cellArray = lyrOut.PredictiveCells.ToArray();
                                foreach (Cell nCell in cellArray)
                                {
                                    HelperFunction.WriteLineColor(nCell.ToString(), ConsoleColor.Yellow);

                                }
                                // HTMClassifier used Predicted Cells to infer learned frame key 
                                var predictedFrames = cls.GetPredictedInputValues(cellArray, 5);
                                foreach (var item in predictedFrames)
                                {
                                    Console.WriteLine($"Current Input: {currentFrame.FrameKey} \t| Predicted Input: {item.PredictedInput}");
                                    Console.WriteLine($"{item.PredictedInput} -- similarity{item.Similarity} -- NumberOfSameBit {item.NumOfSameBits}");
                                }
                            }
                            else
                            {
                                // If No Cells is predicted
                                HelperFunction.WriteLineColor($"CURRENT FRAME: {currentFrame.FrameKey}", ConsoleColor.Red);
                                HelperFunction.WriteLineColor("NO CELLS PREDICTED  FOR THIS FRAME", ConsoleColor.Red);
                            }
                        }
                        // Inferring Mode
                        // 
                        learn = false;
                        List<List<string>> possibleOutcomeSerie = new();
                        possibleOutcomeSerie.Add(new List<string> { trainingVideo[0].FrameKey });
                        List<string> possibleOutcome = new();
                        tm.Reset(mem);
                        foreach (NFrame currentFrame in trainingVideo)
                        {
                            // Inferring the current frame encoded bit array with learned SP
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;
                            var nextFramePossibilities = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);
                            foreach (var predictedOutput in nextFramePossibilities)
                            {
                                possibleOutcome.Add(predictedOutput.PredictedInput);
                            }

                            possibleOutcomeSerie.Add(new(possibleOutcome));
                            possibleOutcome.Clear();
                        }

                        int correctlyPredictedFrame = 0;

                        List<string> resultToWrite = new();
                        string resultFileName = $"Output" + @"\" + $"ResultLog" + @"\" + $"{nv.label}_{nv.name}_Cycle{i}";

                        for (int j = 0; j < possibleOutcomeSerie.Count - 1; j += 1)
                        {
                            string message = $"Expected : {trainingVideo[j].FrameKey} ||| GOT {string.Join(" --- ", possibleOutcomeSerie[j])}";
                            if (possibleOutcomeSerie[j].Contains(trainingVideo[j].FrameKey))
                            {
                                correctlyPredictedFrame += 1;
                                HelperFunction.WriteLineColor(message, ConsoleColor.Green);
                                resultToWrite.Add($"FOUND:   {message}");
                            }
                            else
                            {
                                HelperFunction.WriteLineColor(message, ConsoleColor.Gray);
                                resultToWrite.Add($"NOTFOUND {message}");
                            }
                        }
                        double accuracy = correctlyPredictedFrame / (double)trainingVideo.Count;
                        videoAccuracy.Add(accuracy);
                        // Check for stability in predicting Sequence

                        if (accuracy > 0.8)
                        {
                            RecordResult(resultToWrite, resultFileName);
                        }
                        resultToWrite.Clear();
                        // Enter training phase again
                        learn = true;
                    }
                    setAccuracy.Add(videoAccuracy.Average());
                }
                cycleAccuracy = setAccuracy.Average();
                if(lastCycleAccuracy == cycleAccuracy)
                {
                    stableAccuracyCount += 1;
                }
                else
                {
                    stableAccuracyCount = 0;
                }
                if(stableAccuracyCount >= 40 && cycleAccuracy> 0.9)
                {
                    break;
                }
                lastCycleAccuracy = cycleAccuracy;
            }
            // Testing Section
            string userInput;
            string testOutputFolder = $"{outputFolder}" + @"\" + "TEST";
            HelperFunction.WriteLineColor("Drag a Frame(Picture) to recall the learned videos : ", ConsoleColor.Cyan);
            userInput = Console.ReadLine().Replace("\"", "");
            int testNo = 0;

            do
            {
                testNo += 1;
                NFrame inputFrame = new(new System.Drawing.Bitmap(userInput), "TEST", "test", 0, frameWidth, frameHeight, colorMode);
                //NFrame inputFrame = videoData[0].nVideoList[0].nFrames[-1+testNo];
                var lyrOut = layer1.Compute(inputFrame.EncodedBitArray, false) as ComputeCycle;
                var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 5);
                foreach (var possibleFrame in predictedInputValue)
                {
                    Boolean nextPredictedFrameExists = true;
                    List<NFrame> frameSequence = new();
                    string currentFrameKey = possibleFrame.PredictedInput;
                    string Label = "";
                    NVideo currentVid = null;
                    while (nextPredictedFrameExists)
                    {
                        foreach(var vs in videoData)
                        {
                            foreach(var nv in vs.nVideoList)
                            {
                                foreach(var nf in nv.nFrames)
                                {
                                    if(nf.FrameKey == currentFrameKey)
                                    {
                                        frameSequence.Add(nf);
                                        currentVid = nv;
                                    }
                                }
                            }
                        }
                        HelperFunction.WriteLineColor($"Predicted nextFrame: {currentFrameKey}", ConsoleColor.Green);

                        var computedSDR = layer1.Compute(currentVid.GetEncodedFrame(currentFrameKey), false) as ComputeCycle;
                        var predictedNext = cls.GetPredictedInputValues(computedSDR.PredictiveCells.ToArray(), 3);
                        // End sequence check
                        if(predictedNext.Count == 0)
                        {
                            nextPredictedFrameExists = false;
                        }
                        else
                        {
                            currentFrameKey = predictedNext[0].PredictedInput;
                        }
                    }
                    string dir = $"{testOutputFolder}" + @"\" + $"Predicted from {Path.GetFileNameWithoutExtension(userInput)}";
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    NVideo.NFrameListToVideo(
                        frameSequence,
                        $"{dir}" + @"\" + $"testNo_{testNo}_FirstPossibility_{possibleFrame.Similarity}_FirstLabel_{possibleFrame.PredictedInput}.mp4",
                        (int)videoData[0].nVideoList[0].frameRate,
                        new Size((int)videoData[0].nVideoList[0].frameWidth, (int)videoData[0].nVideoList[0].frameHeight),
                        true);
                }
                userInput = "";
                while (userInput == "")
                {
                    userInput = Console.ReadLine().Replace("\"", "");
                }
            }
            while (userInput != "Q");
        }
        public static void Run2()
        {
            Stopwatch sw = new Stopwatch();
            List<TimeSpan> RecordedTime = new();

            HelperFunction.WriteLineColor($"Hello NeoCortexApi! Conducting experiment {nameof(VideoLearning)} Toan Truong");

            // The current training Folder is located in HTMVideoLlearning/
            // SmallTrainingSet ; Training Videos ; oneVideoTrainingSet
            HelperFunction.WriteLineColor("Please drag the folder that contains the training files to the Console Window: ", ConsoleColor.Blue);
            string trainingFolderPath = Console.ReadLine();

            // Starting experiment
            sw.Start();

            // Output folder initiation
            string outputFolder = "Output";
            string convertedVideoDir = $"{outputFolder}" + @"\" + "Converted";
            if (!Directory.Exists($"{convertedVideoDir}"))
            {
                Directory.CreateDirectory($"{convertedVideoDir}");
            }

            // Video Parameter 
            int frameWidth = 18;
            int frameHeight = 18;
            ColorMode colorMode = ColorMode.BLACKWHITE;
            double frameRate = 10;
            
            // Define Reader for Videos
            // Input videos are stored in different folders under TrainingVideos/
            // with their folder's names as label value. To get the paths of all folders:
            string[] videoSetDirectories = HelperFunction.GetVideoSetPaths(trainingFolderPath);

            // A list of VideoSet object, each has the Videos and the name of the folder as Label, contains all the Data in TrainingVideos,
            // this List will be the core iterator in later learning and predicting
            List<VideoSet> videoData = new();

            // Iterate through every folder in TrainingVideos/ to create VideoSet: object that stores video of same folder/label
            foreach (string path in videoSetDirectories)
            {
                VideoSet vs = new(path, colorMode, frameWidth, frameHeight, frameRate);
                videoData.Add(vs);
                // Output converted Videos to Output/Converted/
                vs.CreateConvertedVideos(convertedVideoDir);
            }

            // Define HTM parameters
            int[] inputBits = { frameWidth * frameHeight * (int)colorMode };
            int[] numColumns = { 1024 };

            //Initiating HTM
            HtmConfig cfg = GetHTM(inputBits, numColumns);

            var mem = new Connections(cfg);

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

            TemporalMemory tm = new TemporalMemory();

            bool isInStableState = false;

            bool learn = true;

            int maxCycles = 2000;
            int newbornCycle = 0;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, 30 * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
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
            //for (int i = 0; i < maxCycles; i++)
            while(isInStableState == false)
            {
                newbornCycle++;
                Console.WriteLine($"-------------- Newborn Cycle {newbornCycle} ---------------");
                foreach (VideoSet set in videoData)
                {
                    // Show Set Label/ Folder Name of each video set
                    HelperFunction.WriteLineColor($"VIDEO SET LABEL: {set.VideoSetLabel}", ConsoleColor.Cyan);
                    foreach (NVideo vid in set.nVideoList)
                    {
                        // Name of the Video That is being trained 
                        HelperFunction.WriteLineColor($"    VIDEO NAME: {vid.name}", ConsoleColor.DarkCyan);
                        foreach (NFrame frame in vid.nFrames)
                        {
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
            List<int[]> stableAreas = new();

            int cycle = 0;
            int matches = 0;

            List<string> lastPredictedValue = new();

            foreach (VideoSet vd in videoData)
            {
                foreach (NVideo nv in vd.nVideoList)
                {
                    int maxPrevInputs = nv.nFrames.Count - 1;
                    List<string> previousInputs = new();
                    cycle = 0;
                    learn = true;

                    sw.Reset();
                    sw.Start();
                    int maxMatchCnt = 0;
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

                        foreach (var currentFrame in nv.nFrames)
                        {
                            Console.WriteLine($"-------------- {currentFrame.FrameKey} ---------------");
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;

                            Console.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));
                            // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.

                            previousInputs.Add(currentFrame.FrameKey);
                            if (previousInputs.Count > (maxPrevInputs + 1))
                                previousInputs.RemoveAt(0);

                            // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                            // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                            // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                            // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                            // memorized, it will match as the first one.
                            if (previousInputs.Count < maxPrevInputs)
                                continue;

                            string key = GetKey(previousInputs);
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

                            // Remember the key with corresponding SDR
                            HelperFunction.WriteLineColor($"Current learning Key: {key}", ConsoleColor.Magenta);
                            cls.Learn(key, actCells.ToArray());

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
                                    Console.WriteLine($"Current Input: {currentFrame.FrameKey} \t| Predicted Input: {item.PredictedInput}");
                                    lastPredictedValue.Add(item.PredictedInput);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"NO CELLS PREDICTED for next cycle.");
                                lastPredictedValue.Clear();
                            }
                        }
                        // Reset Temporal memory after learning 1 time the video/sequence
                        tm.Reset(mem);

                        double accuracy;
                        //previousInputs.Clear();
                        //accuracy = (double)matches / ((double)nv.nFrames.Count - 1.0) * 100.0; // Use if with reset
                        accuracy = (double)matches / (double)nv.nFrames.Count * 100.0; // Use if without reset

                        Console.WriteLine($"Cycle: {cycle}\tMatches={matches} of {nv.nFrames.Count}\t {accuracy}%");

                        if (accuracy >= 90.0)
                        {

                            maxMatchCnt++;
                            Console.WriteLine($"90% accuracy surpassed {maxMatchCnt} times.");
                            //
                            // Experiment is completed if we are 30 cycles long at the 100% accuracy.
                            if (maxMatchCnt >= 100)
                            //if (cycle == maxCycles)
                            {
                                stableAreas.Add(new int[] { cycle - maxMatchCnt, cycle });
                                sw.Stop();
                                Console.WriteLine($"Exit experiment in the stable state after 30 repeats with {accuracy}% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                Console.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                foreach (int[] stableArea in stableAreas)
                                {
                                    Console.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Debug.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Console.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Debug.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Console.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Debug.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Console.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Debug.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Console.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Debug.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                }
                                learn = false;
                                break;
                            }
                        }
                        else if (maxMatchCnt > 0)
                        {
                            Console.WriteLine($"At {accuracy}% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                            stableAreas.Add(new int[] { cycle - maxMatchCnt, cycle - 1 });
                            if (cycle == maxCycles)
                            {
                                sw.Stop();
                                Console.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                Console.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                foreach (int[] stableArea in stableAreas)
                                {
                                    Console.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Debug.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Console.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Debug.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Console.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Debug.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Console.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Debug.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Console.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Debug.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                }
                                learn = false;
                                break;
                            }
                            maxMatchCnt = 0;
                        }
                    }
                    Console.WriteLine("------------ END ------------");
                    previousInputs.Clear();
                }
            }
            //Testing Section
            string userInput;
            string testOutputFolder = $"{outputFolder}" + @"\" + "TEST";

            userInput = Console.ReadLine().Replace("\"", "");
            
            int testNo = 0;

            do
            {
                string Outputdir = $"{testOutputFolder}" + @"\" + $"Predicted from {Path.GetFileNameWithoutExtension(userInput)}";
                if (!Directory.Exists(Outputdir))
                {
                    Directory.CreateDirectory(Outputdir);
                }
                testNo += 1;
                // Save the input Frame as NFrame
                NFrame inputFrame = new(new Bitmap(userInput), "TEST", "test", 0, frameWidth, frameHeight, colorMode);
                inputFrame.SaveFrame(Outputdir+@"\"+$"Converted_{Path.GetFileName(userInput)}");
                // Compute the SDR of the Frame
                var lyrOut = layer1.Compute(inputFrame.EncodedBitArray, false) as ComputeCycle;

                // Use HTMClassifier to calculate 5 possible next Cells Arrays
                var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 5);


                foreach (var serie in predictedInputValue)
                {
                    HelperFunction.WriteLineColor($"Predicted Serie:", ConsoleColor.Green);
                    string s = serie.PredictedInput;
                    HelperFunction.WriteLineColor(s);
                    Console.WriteLine("\n");
                    //Create List of NFrame to write to Video
                    List<NFrame> outputNFrameList = new();
                    string Label = "";
                    List<string> frameKeyList = s.Split("-").ToList();
                    foreach (string frameKey in frameKeyList)
                    {
                        foreach (var vs in videoData)
                        {
                            foreach (var vd in vs.nVideoList)
                            {
                                foreach (var nf in vd.nFrames)
                                {
                                    if (nf.FrameKey == frameKey)
                                    {
                                        Label = nf.label;
                                        outputNFrameList.Add(nf);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                        
                    // Create output video
                    NVideo.NFrameListToVideo(
                        outputNFrameList,
                        $"{Outputdir}" + @"\" + $"testNo_{testNo}_Label{Label}_similarity{serie.Similarity}_No of same bit{serie.NumOfSameBits}.mp4",
                        (int)videoData[0].nVideoList[0].frameRate,
                        new Size((int)videoData[0].nVideoList[0].frameWidth, (int)videoData[0].nVideoList[0].frameHeight),
                        true);
                }
                // Enter loop for more input
                userInput = "";
                while (userInput == "")
                {
                    userInput = Console.ReadLine().Replace("\"", "");
                }
            }
            while (userInput != "Q");
        }
        /// <summary>
        /// Writing experiment result to write to a text file
        /// </summary>
        /// <param name="possibleOutcomeSerie"></param>
        /// <param name="inputVideo"></param>
        public static void RecordResult(List<string> result, string fileName)
        {
            File.WriteAllLines($"{fileName}.txt", result);
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
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 40,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns[0],
                PotentialRadius = (int)(0.15 * inputBits[0]),
                InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns[0]),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
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
