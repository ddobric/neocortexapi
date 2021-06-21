// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Diagnostics;
using NeoCortexEntities.NeuroVisualizer;
using WebSocketNeuroVisualizer;
using NeoCortexApi.Utility;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using NeoCortexApi.Classifiers;
using System.Drawing;
using System.Drawing.Imaging;

namespace UnitTestsProject.SequenceLearningExperiments
{
    [TestClass]
    public class VideosExperiment
    {
        int inputBits = 225;
        int numColumns = 2048;
        [TestMethod]
        [TestCategory("Experiment")]
        [Timeout(TestTimeout.Infinite)]
        public void VideosLearningExperiment()
        {
            //===============================================================
            // Experiment designed to learn the rule of a simple video:
            // A ball bouncing in a square box
            //===============================================================

            //================= preparing the parameters ====================
            Parameters p = GetParameters();
            double max = 20;


            //===================== input collection ========================
            //input are stored on different folders with their folder's names
            //as the settings value. However it is only for isolating the ini
            //conditions of the ball, how it will move. The generation of the 
            //data was done using a seperate python code.

            string[] imageDirList = fetchImagesDirList("SequenceLearningExperiments");
            List<ImageSet> InputVideos = fetchImagesfromFolders(imageDirList);
            //InputVideos[0].checkInstance();

            //====================== getting number of different input for HomeostaticPlasticityController ===============
            List<int[]> tempInput = new();
            foreach (ImageSet vid in InputVideos)
            {
                Debug.WriteLine($"============= initiate learning on {vid.IdName} ==============");
                foreach (int[] imageBinary in vid.ImageBinValue)
                {
                    if (!tempInput.Contains(imageBinary))
                    {
                        tempInput.Add(imageBinary);
                    }
                }
            }
            var numInputs = tempInput.Distinct<int[]>().ToList().Count;
            Debug.WriteLine($"No of different input: {numInputs}");

            //==================================== initiating the CLA HTM model ===================================
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;
            bool learn = true;

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            var mem = new Connections();

            p.apply(mem);

            bool isInStableState = false;

            //HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            TemporalMemory tm1 = new TemporalMemory();

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


            SpatialPoolerMT sp1 = new SpatialPoolerMT(hpa);
            sp1.Init(mem, UnitTestHelpers.GetMemory());
            tm1.Init(mem);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("sp", sp1);
            layer1.HtmModules.Add("tm", tm1);

            int cycle = 0;
            int matches = 0;

            string lastPredictedValue = "0";


            int maxCycles = 4200;
            int maxPrevInputs = tempInput.Count - 1;
            List<string> previousInputs = new List<string>();
            previousInputs.Add(tempInput[0].ArrToString());

            // Training SP to get stable. New-born stage.
            //
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Newborn Cycle {cycle} ---------------");

                for (int j = 0; j < tempInput.Count;j+=1)
                {
                    Debug.WriteLine($" -- {InputVideos[0].ImageNames[j]} -- {tempInput[j].ArrToString()} --");

                    var lyrOut = sp1.Compute(tempInput[j], learn);
                    Debug.WriteLine($"SDR:  {lyrOut.ArrToString()}");
                    if (isInStableState)
                        break;
                }

                if (isInStableState)
                    break;
            }


            //
            // Now training with SP+TM. SP is pretrained on the given input pattern set.
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                for (int j = 0; j < tempInput.Count;j+=1)
                {
                    Debug.WriteLine($"-------------- {InputVideos[0].ImageNames[j]} ---------------");

                    var lyrOut = layer1.Compute(tempInput[j], learn) as ComputeCycle;

                    // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.
                    //if (isInStableState && lyrOut != null)
                    {
                        var activeColumns = layer1.GetResult("sp") as int[];

                        //layer2.Compute(lyrOut.WinnerCells, true);
                        //activeColumnsLst[input].Add(activeColumns.ToList());

                        previousInputs.Add(InputVideos[0].ImageNames[j]);
                        if (previousInputs.Count > (maxPrevInputs + 1))
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, InputVideos[0].ImageNames[j]);

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
                            var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                            Debug.WriteLine($"Current Input: {InputVideos[0].ImageNames[j]} \t| Predicted Input: {predictedInputValue}");

                            lastPredictedValue = predictedInputValue;
                        }
                        else
                        {
                            Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValue = String.Empty;
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
        private class ImageSet
        {
            public List<int[]> ImageBinValue;
            public List<string> ImageNames;
            public string IdName;
            public ImageSet(string dir)
            {
                this.IdName = Path.GetFileName(dir);
                ReadImages(dir);
            }
            private void ReadImages(string dir)
            {
                ImageBinValue = new();
                ImageNames = new();
                foreach (string file in sortFile(dir))
                {
                    ImageBinValue.Add(ImageToBin(file));
                    string fname = Path.GetFileNameWithoutExtension(file.ToString());
                    Debug.WriteLine(fname);
                    ImageNames.Add(fname);
                }
            }
            private List<string> sortFile(string dir)
            {
                List<string> retDir = new();
                for(int i = 0;i<Directory.GetFiles(dir).Length;i+=1)
                {
                    retDir.Add($"{dir}\\{i}.jpg");
                }
                return retDir;
            }
            private int[] ImageToBin(string file)
            {
                var image = new Bitmap(file);
                Bitmap img = ResizeBitmap(image, 15, 15);
                int length = img.Width * img.Height;
                int[] imageBinary = new int[length];

                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        Color pixel = img.GetPixel(i, j);
                        if (pixel.R < 100)
                        {
                            imageBinary[j + i * img.Height] = 1;
                        }
                        else
                        {
                            imageBinary[j + i * img.Height] = 0;
                        }
                    }
                }
                return imageBinary;
            }
            private Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
            {
                Bitmap result = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(bmp, 0, 0, width, height);
                }
                return result;
            }
            public void checkInstance()
            {
                Debug.WriteLine(IdName);
                foreach (int[] imags in ImageBinValue)
                {
                    printSerie(imags);
                }
            }
            private void printSerie(int[] ba)
            {
                foreach (int b in ba)
                {
                    Debug.Write(b);
                }
                Debug.Write("\n");
            }
        }
        private Parameters GetParameters()
        {
            Parameters p = Parameters.getAllDefaultParameters();

            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.CELLS_PER_COLUMN, 25);

            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.

            //p.setNumActiveColumnsPerInhArea(10);
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.Set(KEY.POTENTIAL_RADIUS, 50);
            p.setInhibitionRadius(15);

            //
            // Activates the high bumping/boosting of inactive columns.
            // This exeperiment uses HomeostaticPlasticityActivator, which will deactivate boosting and bumping.
            p.Set(KEY.MAX_BOOST, 10.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 25);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.75);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));

            // If learning process does not generate active segments, this value should be decreased. You can notice this with continious burtsing. look in trace for 'B.B.B'
            // If invalid patterns are predicted then this value should be increased.
            p.setActivationThreshold(15);
            p.setConnectedPermanence(0.5);

            // Learning is slower than forgetting in this case.
            p.setPermanenceDecrement(0.25);
            p.setPermanenceIncrement(0.15);

            // Used by punishing of segments.
            p.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.1);
            return p;
        }
        private string[] fetchImagesDirList(string testName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            //================ up dir one level ==========================
            currentDir = $"{ Directory.GetParent(currentDir)}";
            //================ up dir one level ==========================
            currentDir = $"{ Directory.GetParent(currentDir)}";
            //================ up dir one level ==========================
            currentDir = $"{ Directory.GetParent(currentDir)}";
            //================ test directory ============================

            string testDir = $"{currentDir}\\{testName}";
            string testDataDir = $"{testDir}\\VideosData";
            string[] a = Directory.GetDirectories(testDataDir, "*", SearchOption.TopDirectoryOnly);
            // a is a string list of all directories inside VideosData/
            return a;
        }
        private List<ImageSet> fetchImagesfromFolders(string[] imageDirList)
        {
            List<ImageSet> inputImagesList = new();
            foreach (string dir in imageDirList)
            {
                ImageSet temp = new ImageSet(dir);
                inputImagesList.Add(temp);
            }
            return inputImagesList;
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
    }
}
