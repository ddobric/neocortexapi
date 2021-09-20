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
using VideoLibrary;

namespace NeoCortexApiSample
{
    class VideoLearning
    {
        // Run1:
        // Training and Learning Video with HTMClassifier with key as a frame key
        // Testing Procedure:
        // Read the Training dataset
        // Preprocessing the frame into smaller resolution, lower frame rate, color to binarized value
        // Learn the patterns(frames) with SP till reaching newborn stable state
        // Learn the patterns(frames) with SP+TM to generate sequential relation of adjacent frames,
        //      The learning ends when average accuracy is more than 90% and stays for 40 cycles or reaching maxcycles
        //      Calculating Average accuracy:
        //          Get the Predicted cells of the current frames SDR through TM
        //          Use the Predicted cells in HTMClassifier to see if there are learned framekey
        //          If the key of the next frame is found, count increase 1.
        //          The average accuracy is calculated by average of videoset accuracy, 
        //          videoset accuracy is calculated by average of all video accuracy in that set.
        // Testing session start:
        // Drag an Image as input, The trained layer will try to predict the next Frame, then uses the next frame as input to continue 
        // as long as there are predicted cells.
        // The predicted series of Frame after the input frame are made into videos under Run1Experiment/TEST/
        public void Run1()
        {

            Stopwatch sw = new Stopwatch();
            List<TimeSpan> RecordedTime = new List<TimeSpan>();

            HelperFunction.WriteLineColor($"Hello NeoCortexApi! Conducting experiment {nameof(VideoLearning)} Noath2302");
            HelperFunction.WriteLineColor("Please insert or drag the folder that contains the training files: ", ConsoleColor.Blue);
            string trainingFolderPath = Console.ReadLine();

            sw.Start();
            // Define first the desired properties of the frames
            string outputFolder = "Run1ExperimentOutput";
            if (!Directory.Exists($"{outputFolder}"))
            {
                Directory.CreateDirectory($"{outputFolder}");
            }
            string convertedVideoDir = $"{outputFolder}" + @"\" + "Converted";
            if (!Directory.Exists($"{convertedVideoDir}"))
            {
                Directory.CreateDirectory($"{convertedVideoDir}");
            }
            string testOutputFolder = $"{outputFolder}" + @"\" + "TEST";
            if (!Directory.Exists(testOutputFolder))
            {
                Directory.CreateDirectory(testOutputFolder);
            }
            int frameWidth = 18;
            int frameHeight = 18;
            ColorMode colorMode = ColorMode.BLACKWHITE;
            double frameRate = 12;
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
            List<VideoSet> videoData = new List<VideoSet>();

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

            int maxCycles = 10;
            int newbornCycle = 0;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, maxNumOfElementsInSequence * 150 * 3, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

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
            ///*
            //for (int i = 0; i < maxCycles; i++)
            while (isInStableState == false)
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
            long SP_TrainingTimeElapsed = sw.ElapsedMilliseconds;
            sw.Reset();
            sw.Start();
            for (int i = 0; i < maxCycles; i++)
            {
                List<double> setAccuracy = new List<double>();
                HelperFunction.WriteLineColor($"------------- Cycle {i} -------------", ConsoleColor.Green);
                // Iterating through every video set
                foreach (VideoSet vs in videoData)
                {
                    List<double> videoAccuracy = new List<double>();
                    // Iterating through every video in a VideoSet
                    foreach (NVideo nv in vs.nVideoList)
                    {
                        List<NFrame> trainingVideo = nv.nFrames;
                        learn = true;

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
                                /*
                                foreach (Cell nCell in cellArray)
                                {
                                    HelperFunction.WriteLineColor(nCell.ToString(), ConsoleColor.Yellow);

                                }
                                */
                                // HTMClassifier used Predicted Cells to infer learned frame key 
                                var predictedFrames = cls.GetPredictedInputValues(cellArray, 5);
                                HelperFunction.WriteLineColor("Predicted next Frame's Label:", ConsoleColor.Yellow);
                                foreach (var item in predictedFrames)
                                {
                                    //Console.WriteLine($"Current Input: {currentFrame.FrameKey} \t| Predicted Input: {item.PredictedInput}");
                                    Console.WriteLine($"{item.PredictedInput} -- similarity{item.Similarity} -- NumberOfSameBit {item.NumOfSameBits}");
                                }
                            }
                            else
                            {
                                // If No Cells is predicted
                                //HelperFunction.WriteLineColor($"CURRENT FRAME: {currentFrame.FrameKey}", ConsoleColor.Red);
                                HelperFunction.WriteLineColor("NO CELLS PREDICTED  FOR THIS FRAME", ConsoleColor.Red);
                            }
                        }
                        // Inferring Mode
                        // 
                        learn = false;
                        List<List<string>> possibleOutcomeSerie = new List<List<string>>();
                        possibleOutcomeSerie.Add(new List<string> { trainingVideo[0].FrameKey });
                        List<string> possibleOutcome = new List<string>();

                        foreach (NFrame currentFrame in trainingVideo)
                        {
                            // Inferring the current frame encoded bit array with learned SP
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;
                            var nextFramePossibilities = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 1);
                            foreach (var predictedOutput in nextFramePossibilities)
                            {
                                possibleOutcome.Add(predictedOutput.PredictedInput);
                            }

                            possibleOutcomeSerie.Add(new List<string>(possibleOutcome));
                            possibleOutcome.Clear();
                        }

                        int correctlyPredictedFrame = 0;

                        List<string> resultToWrite = new List<string>();
                        if (!Directory.Exists($"{outputFolder}" + @"\" + $"ResultLog"))
                        {
                            Directory.CreateDirectory($"{outputFolder}" + @"\" + $"ResultLog");
                        }
                        string resultFileName = $"{outputFolder}" + @"\" + $"ResultLog" + @"\" + $"{nv.label}_{nv.name}_Cycle{i}";

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
                        double accuracy = correctlyPredictedFrame / ((double)trainingVideo.Count - 1);
                        videoAccuracy.Add(accuracy);

                        if (accuracy > 0.9)
                        {
                            RecordResult(resultToWrite, resultFileName);
                        }
                        resultToWrite.Clear();
                        // Enter training phase again
                        learn = true;
                        tm.Reset(mem);
                    }
                    double currentSetAccuracy = videoAccuracy.Average();
                    HelperFunction.WriteLineColor($"Video Set of Label: {vs.VideoSetLabel} reachs accuracy: {currentSetAccuracy * 100}%", ConsoleColor.Cyan);
                    setAccuracy.Add(currentSetAccuracy);
                }
                cycleAccuracy = setAccuracy.Average();
                HelperFunction.WriteLineColor($"Accuracy in Cycle {i}: {cycleAccuracy * 100}%");
                // Check if accuracy is stable
                if (lastCycleAccuracy == cycleAccuracy)
                {
                    stableAccuracyCount += 1;
                }
                else
                {
                    stableAccuracyCount = 0;
                }
                if (stableAccuracyCount >= 40 && cycleAccuracy > 0.9)
                {
                    List<string> outputLog = new List<string>();
                    if (!Directory.Exists($"{outputFolder}" + @"\" + "TEST"))
                    {
                        Directory.CreateDirectory($"{outputFolder}" + @"\" + "TEST");
                    }
                    string fileName = $"{outputFolder}" + @"\" + "TEST" + @"\" + $"saturatedAccuracyLog_Run1";
                    outputLog.Add($"Result Log for reaching saturated accuracy at cycleAccuracy {cycleAccuracy}");

                    outputLog.Add($"reaching stable after enter newborn cycle {newbornCycle}.");
                    outputLog.Add($"Elapsed time: {SP_TrainingTimeElapsed / 1000 / 60} min.");

                    for (int j = 0; j < videoData.Count; i += 1)
                    {
                        outputLog.Add($"{videoData[j].VideoSetLabel} reach average Accuracy {setAccuracy[j]}");
                    }
                    outputLog.Add($"Stop SP+TM after {i} cycles");
                    outputLog.Add($"Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");

                    RecordResult(outputLog, fileName);
                    break;
                }
                else if (i == maxCycles - 1)
                {
                    List<string> outputLog = new List<string>();
                    if (!Directory.Exists($"{outputFolder}" + @"\" + "TEST"))
                    {
                        Directory.CreateDirectory($"{outputFolder}" + @"\" + "TEST");
                    }
                    string fileName = $"{outputFolder}" + @"\" + "TEST" + @"\" + $"MaxCycleReached";
                    outputLog.Add($"Result Log for stopping experiment with accuracy at cycleAccuracy {cycleAccuracy}");

                    outputLog.Add($"reaching stable after enter newborn cycle {newbornCycle}.");
                    outputLog.Add($"Elapsed time: {SP_TrainingTimeElapsed / 1000 / 60} min.");

                    for (int j = 0; j < videoData.Count; j += 1)
                    {
                        outputLog.Add($"{videoData[j].VideoSetLabel} reach average Accuracy {setAccuracy[j]}");
                    }
                    outputLog.Add($"Stop SP+TM after {i} cycles");
                    outputLog.Add($"Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                    RecordResult(outputLog, fileName);
                    break;
                }
                lastCycleAccuracy = cycleAccuracy;
            }
            // Testing Section
            string userInput;

            HelperFunction.WriteLineColor("Drag a Frame(Picture) to recall the learned videos : ", ConsoleColor.Cyan);
            int testNo = 0;

            do
            {
                userInput = "";
                while (userInput == "")
                {
                    userInput = Console.ReadLine().Replace("\"", "");
                }
                testNo += 1;
                NFrame inputFrame = new NFrame(new Bitmap(userInput), "TEST", "test", 0, frameWidth, frameHeight, colorMode);
                // Computing user input frame with trained layer 
                var lyrOut = layer1.Compute(inputFrame.EncodedBitArray, false) as ComputeCycle;
                var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 5);
                foreach (var possibleFrame in predictedInputValue)
                {
                    bool nextPredictedFrameExists = true;
                    List<NFrame> frameSequence = new List<NFrame>();
                    frameSequence.Add(inputFrame);
                    string currentFrameKey = possibleFrame.PredictedInput;

                    NFrame currentFrame = null;
                    while (nextPredictedFrameExists && frameSequence.Count < 42)
                    {
                        foreach (var vs in videoData)
                        {
                            currentFrame = vs.GetNFrameFromFrameKey(currentFrameKey);
                            if (currentFrame != null)
                            {
                                frameSequence.Add(currentFrame);
                                break;
                            }
                        }
                        HelperFunction.WriteLineColor($"Predicted nextFrame: {currentFrameKey}", ConsoleColor.Green);

                        var computedSDR = layer1.Compute(currentFrame.EncodedBitArray, false) as ComputeCycle;
                        var predictedNext = cls.GetPredictedInputValues(computedSDR.PredictiveCells.ToArray(), 3);

                        // Check for end of Frame sequence
                        if (predictedNext.Count == 0)
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
                        $"{dir}" + @"\" + $"testNo_{testNo}_FirstPossibility_{possibleFrame.Similarity}_FirstLabel_{possibleFrame.PredictedInput}",
                        (int)(videoData[0].nVideoList[0].frameRate),
                        new Size((int)videoData[0].nVideoList[0].frameWidth, (int)videoData[0].nVideoList[0].frameHeight),
                        true);
                }
            }
            while (userInput != "Q");
        }
        // Run2:
        // Training and Learning Video with HTMClassifier with key as a serie of framekey
        // Testing Procedure:
        // Read the Training dataset
        // Preprocessing the frame into smaller resolution, lower frame rate, color to binarized value
        // Learn the patterns(frames) with SP till reaching newborn stable state
        // Learn the patterns(serie of frames) with SP+TM,
        // The serie of frames add each framekey respectively untill it reached the videos' framecount lengths:30
        // Then key - serie of frames with current frame as last frame is learned with the Cells index of the current frame.
        //      e.g. current frame circle_vd1_3's cell will be associate with key "circle_vd1_4-circle_vd1_5-circle_vd1_6-...-circle_vd1_29-circle_vd1_0-circle_vd1_1-circle_vd1_2-circle_vd1_3"
        //      through each iteration of frames in a video, the key will be framekey-shifted
        //      a List of Last Predicted Values is saved every frame iteration to be used in the next as validation.
        //          if LastPredictedValue of previous Frame contains the current frame's key, then match increase 1
        //          Accuracy is calculated each iteration of each Videos.
        //          The training ends when accuracy surpasses 80% more than 30 times or reaching max cycle
        // Testing session start:
        // Drag an Image as input, The trained layer will try to predict the next Frame, then uses the next frame label - framekey series
        // to recreate the video under Run2Experiment/TEST/
        public void Run2()
        {
            Stopwatch sw = new Stopwatch();
            List<TimeSpan> RecordedTime = new List<TimeSpan>();

            HelperFunction.WriteLineColor($"Hello NeoCortexApi! Conducting experiment {nameof(VideoLearning)} Toan Truong");

            // The current training Folder is located in HTMVideoLlearning/
            // SmallTrainingSet ; Training Videos ; oneVideoTrainingSet
            HelperFunction.WriteLineColor("Please drag the folder that contains the training files to the Console Window: ", ConsoleColor.Blue);
            string trainingFolderPath = Console.ReadLine();

            // Starting experiment
            sw.Start();

            // Output folder initiation
            string outputFolder = "Run2ExperimentOutput";
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
            List<VideoSet> videoData = new List<VideoSet>();

            // Iterate through every folder in TrainingVideos/ to create VideoSet: object that stores video of same folder/label
            foreach (string path in videoSetDirectories)
            {
                VideoSet vs = new VideoSet(path, colorMode, frameWidth, frameHeight, frameRate);
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

            int maxCycles = 1000;
            int newbornCycle = 0;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, 30 * 150 * 3, (isStable, numPatterns, actColAvg, seenInputs) =>
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
                //cls.ClearState();

            }, numOfCyclesToWaitOnChange: 50);

            SpatialPoolerMT sp = new SpatialPoolerMT(hpa);
            sp.Init(mem);
            tm.Init(mem);
            layer1.HtmModules.Add("sp", sp);

            //
            // Training SP to get stable. New-born stage.
            //
            ///*
            //for (int i = 0; i < maxCycles; i++)
            while (isInStableState == false)
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
            List<int[]> stableAreas = new List<int[]>();

            int cycle = 0;
            int matches = 0;

            List<string> lastPredictedValue = new List<string>();

            foreach (VideoSet vd in videoData)
            {
                foreach (NVideo nv in vd.nVideoList)
                {
                    int maxPrevInputs = nv.nFrames.Count - 1;
                    List<string> previousInputs = new List<string>();
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
                    double lastCycleAccuracy = 0;
                    int saturatedAccuracyCount = 0;
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

                        double accuracy;

                        accuracy = (double)matches / ((double)nv.nFrames.Count - 1.0) * 100.0; // Use if with reset
                        //accuracy = (double)matches / (double)nv.nFrames.Count * 100.0; // Use if without reset

                        Console.WriteLine($"Cycle: {cycle}\tMatches={matches} of {nv.nFrames.Count}\t {accuracy}%");
                        if (accuracy == lastCycleAccuracy)
                        {
                            // The learning may result in saturated accuracy
                            // Unable to learn to higher accuracy, Exit
                            saturatedAccuracyCount += 1;
                            if (saturatedAccuracyCount >= 50 && lastCycleAccuracy > 80)
                            {
                                List<string> outputLog = new List<string>();
                                if (!Directory.Exists($"{outputFolder}" + @"\" + "TEST"))
                                {
                                    Directory.CreateDirectory($"{outputFolder}" + @"\" + "TEST");
                                }
                                string fileName = $"{outputFolder}" + @"\" + "TEST" + @"\" + $"saturatedAccuracyLog_{nv.label}_{nv.name}";
                                outputLog.Add($"Result Log for reaching saturated accuracy at {accuracy}");
                                outputLog.Add($"Label: {nv.label}");
                                outputLog.Add($"Video Name: {nv.name}");
                                outputLog.Add($"Stop after {cycle} cycles");
                                outputLog.Add($"Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                outputLog.Add($"reaching stable after enter newborn cycle {newbornCycle}.");
                                RecordResult(outputLog, fileName);
                                break;
                            }
                        }
                        lastCycleAccuracy = accuracy;
                        //learn = true;
                        // Reset Temporal memory after learning 1 time the video/sequence
                        tm.Reset(mem);
                    }

                    Console.WriteLine("------------ END ------------");
                    previousInputs.Clear();
                }
            }
            //Testing Section
            string userInput;
            string testOutputFolder = $"{outputFolder}" + @"\" + "TEST";
            if (!Directory.Exists(testOutputFolder))
            {
                Directory.CreateDirectory(testOutputFolder);
            }
            HelperFunction.WriteLineColor("Drag an image as input to recall the learned Video: ");
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
                NFrame inputFrame = new NFrame(new Bitmap(userInput), "TEST", "test", 0, frameWidth, frameHeight, colorMode);
                inputFrame.SaveFrame(Outputdir + @"\" + $"Converted_{Path.GetFileName(userInput)}");
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
                    List<NFrame> outputNFrameList = new List<NFrame>();
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

                CellsPerColumn = 30,
                GlobalInhibition = true,
                //LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns[0],
                PotentialRadius = (int)(0.15 * inputBits[0]),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                //DutyCyclePeriod = 25,
                //MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns[0]),

                //ActivationThreshold = 15,
                //ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                //PermanenceDecrement = 0.15,
                //PermanenceIncrement = 0.15,

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
