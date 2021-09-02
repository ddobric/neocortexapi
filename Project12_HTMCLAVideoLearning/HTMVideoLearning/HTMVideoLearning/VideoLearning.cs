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
            string outputFolder = "output";
            string convertedVideoDir = $"{outputFolder}" + @"\" + "converted";
            if (!Directory.Exists($"{convertedVideoDir}"))
            {
                Directory.CreateDirectory($"{convertedVideoDir}");
            }
            int frameWidth = 18;
            int frameHeight = 18;
            ColorMode colorMode = ColorMode.PURE;
            double frameRate = 10;
            // adding condition for 
            // Define HTM parameters
            int[] inputBits = { frameWidth * frameHeight * (int)colorMode };
            int[] numColumns = { 1024*8 };

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
                foreach (NVideo nv in vd.nVideoList)
                {
                    maxNumOfElementsInSequence += nv.nFrames.Count;
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
                            //var activeColumns = layer1.GetResult("sp") as int[];
                            //Console.WriteLine(string.Join(',', activeColumns));

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
                        if (i > 20)
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
                                var nextFramePossibilities = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 5);

                                foreach (var nextFrame in nextFramePossibilities)
                                {
                                    if (nextFrame.Similarity > 0)
                                    {
                                        possibleOutcome.Add($"{nextFrame.PredictedInput}|{nextFrame.Similarity}|{nextFrame.NumOfSameBits}");
                                    }
                                }
                                possibleOutcomeSerie.Add(new(possibleOutcome));
                                possibleOutcome.Clear();

                            }

                            double correctlyPredictedFrame = 0;

                            List<string> resultToWrite = new();
                            string resultFileName = $"Output" + @"\" + $"ResultLog"+@"\"+$"{nv.label}_{nv.name}_Cycle{i}";

                            for (int j = 0; j < possibleOutcomeSerie.Count - 1; j += 1)
                            {
                                string message = $"Expected : {inputVideo[j].FrameKey} ||| GOT {string.Join(" --- ", possibleOutcomeSerie[j])}";
                                if (possibleOutcomeSerie[j].Contains(inputVideo[j].FrameKey))
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
                            double accuracy = correctlyPredictedFrame / (double)inputVideo.Count;
                            if ( accuracy> 0.5)
                            {
                                RecordResult(resultToWrite, resultFileName);
                            }
                            resultToWrite.Clear();
                            learn = true;
                        }
                    }
                }
            }
        }
        public void Run2()
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
            ColorMode colorMode = ColorMode.BINARIZEDRGB;
            double frameRate = 10;
            // adding condition for 
            // Define HTM parameters
            int[] inputBits = { frameWidth * frameHeight * (int)colorMode };
            int[] numColumns = { 1024 * 4 };

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
                foreach (NVideo nv in vd.nVideoList)
                {
                    maxNumOfElementsInSequence += nv.nFrames.Count;
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
            List<int[]> stableAreas = new List<int[]>();
            int cycle = 0;
            int matches = 0;

            List<string> lastPredictedValue = new List<string>();
            foreach (VideoSet vd in videoData)
            {
                foreach (NVideo nv in vd.nVideoList)
                {
                    List<NFrame> inputVideo = nv.nFrames;
                    int maxPrevInputs = nv.nFrames.Count - 1;
                    List<string> previousInputs = new List<string>();
                    previousInputs.Add("-1");
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

                        foreach (var currentFrame in inputVideo)
                        {
                            Console.WriteLine($"-------------- {currentFrame.FrameKey} ---------------");
                            var lyrOut = layer1.Compute(currentFrame.EncodedBitArray, learn) as ComputeCycle;

                            Console.WriteLine(string.Join(',', lyrOut.ActivColumnIndicies));
                            // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.
                            //var activeColumns = layer1.GetResult("sp") as int[];
                            //Console.WriteLine(string.Join(',', activeColumns));

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

                            cls.Learn(key, actCells.ToArray());

                            if (learn == false)
                                Debug.WriteLine($"Inference mode");

                            Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                            Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                            if (lastPredictedValue.Contains(key))
                            {
                                matches++;
                                Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {key}");
                                lastPredictedValue.Clear();
                            }
                            else
                            {
                                Debug.WriteLine($"Mismatch! Actual value: {key} - Predicted values: {String.Join(',', lastPredictedValue)}");
                                lastPredictedValue.Clear();
                            }

                            if (lyrOut.PredictiveCells.Count > 0)
                            {
                                //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                                var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                                foreach (var item in predictedInputValues)
                                {
                                    Debug.WriteLine($"Current Input: {currentFrame.FrameKey} \t| Predicted Input: {item.PredictedInput}");
                                    lastPredictedValue.Add(item.PredictedInput);
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                                lastPredictedValue.Clear();
                            }
                        }
                        double accuracy;
                        tm.Reset(mem);
                        //previousInputs.Clear();
                        accuracy = (double)matches / ((double)nv.nFrames.Count - 1.0) * 100.0; // Use if with reset
                                                                                                            //accuracy = (double)matches / (double)inputBitsOfTheSequence.Length * 100.0; // Use if without reset

                        Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {nv.nFrames.Count}\t {accuracy}%");

                        if (accuracy == 100.0)
                        {
                            maxMatchCnt++;
                            Debug.WriteLine($"100% accuracy reached {maxMatchCnt} times.");
                            //
                            // Experiment is completed if we are 30 cycles long at the 100% accuracy.
                            //if (maxMatchCnt >= 30)
                            if (cycle == maxCycles)
                            {
                                stableAreas.Add(new int[] { cycle - maxMatchCnt, cycle });
                                sw.Stop();
                                Debug.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                Console.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                foreach (int[] stableArea in stableAreas)
                                {
                                    Debug.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Console.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Debug.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Console.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Debug.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Console.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Debug.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Console.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Debug.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Console.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                }
                                learn = false;
                                break;
                            }
                        }
                        else if (maxMatchCnt > 0)
                        {
                            Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                            stableAreas.Add(new int[] { cycle - maxMatchCnt, cycle - 1 });
                            if (cycle == maxCycles)
                            {
                                sw.Stop();
                                Debug.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                Console.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                                foreach (int[] stableArea in stableAreas)
                                {
                                    Debug.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Console.WriteLine($"----------------Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Debug.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Console.WriteLine($"Starting cycle: {stableArea.Min()}");
                                    Debug.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Console.WriteLine($"Ending cycle: {stableArea.Max()}");
                                    Debug.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Console.WriteLine($"Stable area's size: {stableArea.Max() - stableArea.Min()}");
                                    Debug.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                    Console.WriteLine($"----------------End of Stable area number: {stableAreas.IndexOf(stableArea)}----------------");
                                }
                                learn = false;
                                break;
                            }
                            maxMatchCnt = 0;
                        }
                    }
                    Debug.WriteLine("------------ END ------------");
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
                testNo += 1;
                NFrame inputFrame = new(new System.Drawing.Bitmap(userInput), "TEST", "test", 0, frameWidth, frameHeight, colorMode);
                var lyrOut = layer1.Compute(inputFrame.EncodedBitArray, false) as ComputeCycle;
                var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 5);
                foreach (var serie in predictedInputValue)
                {
                    HelperFunction.WriteLineColor($"Predicted Serie:", ConsoleColor.Red);
                    string s = serie.PredictedInput;
                    HelperFunction.WriteLineColor(s);
                    Console.WriteLine("\n");
                    List<NFrame> a = new();
                    List<string> b = s.Split("-").ToList();
                    foreach (string frameKey in b)
                    {
                        foreach (var vs in videoData)
                        {
                            foreach (var vd in vs.nVideoList)
                            {
                                foreach (var nf in vd.nFrames)
                                {
                                    if (nf.FrameKey == frameKey)
                                    {
                                        a.Add(nf);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    NVideo.NFrameListToVideo(
                        a, 
                        $"{testOutputFolder}"+@"\"+$"testNo_{testNo}.mp4",
                        (int)videoData[0].nVideoList[0].frameRate,
                        new Size((int)videoData[0].nVideoList[0].frameWidth, (int)videoData[0].nVideoList[0].frameHeight),
                        true);  
                }
                userInput = Console.ReadLine().Replace("\"", "");
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
                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                NumActiveColumnsPerInhArea = (int)(0.02 * numColumns[0]),
                MaxSynapsesPerSegment = (int)(0.02 * numColumns[0]),
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
