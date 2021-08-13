// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using NeoCortexApi.Network;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using NeoCortexApi.Utility;
using System.IO;
using System.Threading;
using NeoCortexApi.Classifiers;
using VideoLibrary;

namespace VideoLearningUnitTest
{
    /// <summary>
    /// This Experiment focus on learning and recognition of videos 
    /// The folders' name are used as the label of the dataset
    /// The videos are stored as short clip in label Folders in TrainingVideos/ 
    /// The class VideoSet is created to represent a set of videos which are contained in the same folder, using their folder's name as the label
    /// 
    /// The option currently available for encoding videos are:
    ///     reduction of FrameRate
    ///     Resize of Frame Resolution
    ///     Color Resolution:
    ///         BLACKWHITE,
    ///         BINARIZEDRGB,
    ///         PURE,
    ///         // see ColorMode in VideoSet.cs
    /// to faster read Videos
    /// 
    /// 
    /// </summary>
    [TestClass]
    public class VideosExperiment
    {
        readonly int[] inputBits = { 225 };
        readonly int[] numColumns = { 2048 };

        [TestMethod]
        [TestCategory("Experiment")]
        [Timeout(TestTimeout.Infinite)]
        public void VideosLearningExperiment()
        {
            // Experiment Variables
            ColorMode colorMode = ColorMode.BLACKWHITE;
            int frameWidth = 15;
            int frameHeight = 15;

            // Input videos are stored in different folders under SequenceLearningExperiments/TrainingVideos/
            // with their folder's names as key value. To get the paths of all folders:
            string[] videoSetPaths = GetVideoSetPaths();

            // A list of VideoSet object, each has the Videos and the name of the folder as Label, contains all the Data in TrainingVideos,
            // this List will be the core iterator in later learning and predicting
            List<VideoSet> videoData = new();

            // Because of the continous learning of the Frames in the Spatial Pooler
            // A list of tempInput is used to stored all the encoded frames from all Videos
            List<int[]> tempInput = new();

            // Iterate through every folder in TrainingVideos/ to create VideoSet: object that stores video of same folder/label
            foreach ( string path in videoSetPaths)
            {
                videoData.Add(new VideoSet(path, colorMode, frameWidth, frameHeight));
            }
            // Iterate through every folder in TrainingVideos/
            
            foreach (VideoSet set in videoData)
            {
                // Show Set Label/ Folder Name of each video set
                Debug.WriteLine($"VIDEO SET LABEL: {set.VideoSetLabel}");
                foreach (NVideo vid in set.nVideoList)
                {
                    // Show the name of each video
                    Debug.WriteLine($"  VIDEO NAME: {vid.name}");
                    foreach (int[] frame in vid.nFrames)
                    {
                        // Show the encoded content of each frame in a video, these will be used as SP learning input
                        Debug.WriteLine($"      Frame : {frame.ArrToString()}");

                        // FOR RUNNING EXPERIMENT AT THIS POINT
                        // all frame encoded binary array are stored in tempInput
                        tempInput.Add(frame);
                    }
                }
            }

            // Start the Experiment timer
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // initiating the CLA HTM mode
            HtmConfig htm = GetHTM();
            
            int maxMatchCnt = 0;
            bool learn = true;

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            var mem = new Connections(htm);

            bool isInStableState = false;

            //HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> cls = new();

            TemporalMemory tm1 = new();

            // Question about homeoplasticity controller 
            int numInputs = tempInput.Count;
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, numInputs * 55, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                Assert.IsTrue(numPatterns == numInputs);
                learn = isInStableState = isStable;
                cls.ClearState();

                tm1.Reset(mem);
            }, numOfCyclesToWaitOnChange: 25);
            

            SpatialPoolerMT sp1 = new(hpa);
            sp1.Init(mem);
            tm1.Init(mem);

            CortexLayer<object, object> layer1 = new("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("sp", sp1);
            layer1.HtmModules.Add("tm", tm1);

            int cycle = 0;
            int matches = 0;

            string lastPredictedValue = "";


            int maxCycles = 4200;
            int maxPrevInputs = tempInput.Count - 1;
            List<string> previousInputs = new()
            {
                //tempInput[0].ArrToString()
            };

            // Training SP to get stable. New-born stage.
            //
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                foreach (VideoSet set in videoData)
                {
                    // Show Set Label/ Folder Name of each video set
                    //Debug.WriteLine($"VIDEO SET LABEL: {set.setLabel}");
                    foreach (NVideo vid in set.nVideoList)
                    {
                        // Show the name of each video
                        //Debug.WriteLine($"  VIDEO NAME: {vid.name}");
                        foreach (int[] frame in vid.nFrames)
                        {
                            // Show the encoded content of each frame in a video, these will be used as SP learning input
                            Debug.WriteLine($"      Frame : {frame.ArrToString()}");
                            var lyrOut = sp1.Compute(frame, learn);
                            if (isInStableState)
                                Debug.WriteLine("Stable State reached");
                                break;
                        }
                    }
                }

                if (isInStableState)
                    Debug.WriteLine("Stable State reached");
                    break;   
            }


            //
            // Now training with SP+TM. SP is pretrained on the given input pattern set.
            
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                foreach (VideoSet vs in videoData)

                    foreach (VideoLibrary.NVideo vid in vs.nVideoList)
                    {
                        for (int j = 0; j < vid.nFrames.Count; j++)
                        {
                            {
                                Debug.WriteLine($"-------------- currently playing {vid.name}, frame No. {j} ---------------");

                                var lyrOut = layer1.Compute(vid.nFrames[j], learn) as ComputeCycle;

                                // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.
                                //if (isInStableState && lyrOut != null)
                                {
                                    var activeColumns = layer1.GetResult("sp") as int[];

                                    //layer2.Compute(lyrOut.WinnerCells, true);
                                    //activeColumnsLst[input].Add(activeColumns.ToList());

                                    previousInputs.Add(vs.VideoSetLabel);
                                    if (previousInputs.Count > (maxPrevInputs + 1))
                                        previousInputs.RemoveAt(0);

                                    // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                                    // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                                    // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                                    // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                                    // memorized, it will match as the first one.
                                    if (previousInputs.Count < maxPrevInputs)
                                        continue;

                                    string key = GetKey(previousInputs, vs.VideoSetLabel);

                                    List<Cell> actCells;

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

                                    if (key == lastPredictedValue)
                                    {
                                        matches++;
                                        Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValue}");
                                    }
                                    else
                                        Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted value: {lastPredictedValue}");

                                    if (lyrOut.PredictiveCells.Count > 0)
                                    {
                                        //var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(),3);
                                        var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                                        Debug.WriteLine($"Current Input: {vid.nFrames[j]} \t| Predicted Input: {predictedInputValue}");

                                        lastPredictedValue = predictedInputValue;
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                                        lastPredictedValue = String.Empty;
                                    }
                                }
                            }
                        }
                    }

                // The brain does not do that this way, so we don't use it.
                // tm1.reset(mem);

                double accuracy = (double)matches / (double)tempInput.Count * 100.0;

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {tempInput.Count}\t {accuracy}%");

                if (accuracy == 100.0)
                {
                    maxMatchCnt++;
                    Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");
                    //
                    // Experiment is completed if we are 30 cycles long at the 100% accuracy.
                    if (maxMatchCnt >= 30)
                    {
                        sw.Stop();
                        Debug.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                        learn = false;
                        break;
                    }
                }
                else if (maxMatchCnt > 0)
                {
                    Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                    maxMatchCnt = 0;
                }
            }

            Debug.WriteLine("------------ END ------------");
            
        }

        /// <summary>
        /// <para>Initiate the settings of HTM</para>
        /// </summary>
        /// <returns></returns>
        private HtmConfig GetHTM()
        {
            HtmConfig htm = new HtmConfig(inputBits, numColumns);
            htm.SetHtmConfigDefaultParameters(inputBits, numColumns);
            htm.NumActiveColumnsPerInhArea = (int)(0.02 * numColumns[0]);
            htm.MaxSynapsesPerSegment = (int)(0.02 * numColumns[0]);
            return htm;
        }

        
        private static string GetKey(List<string> prevInputs, string input)
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

        /// <summary>
        /// <para>Get Folders under bin/TrainingVideos</para>
        /// <para>get all folders path in TrainingVideos for adding labels of VideoSet</para>
        /// </summary>
        /// <param name="experimentOutputFolder">Output folder name of the test, found under Debug/bin/</param>
        /// <returns>A String List of Label folder path</returns>
        private static string[] GetVideoSetPaths()
        {
            string currentDir = Directory.GetCurrentDirectory();

            // Get the root path of training videos.
            string testDir = $"{currentDir}\\TrainingVideos";

            // Get all the folders that contain video sets under TrainingVideos/
            string[] videoSetPaths = Directory.GetDirectories(testDir, "*", SearchOption.TopDirectoryOnly);

            // Return an array of video sets paths
            return videoSetPaths;
        }
    }
}
