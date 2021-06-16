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



namespace UnitTestsProject.SequenceLearningExperiments
{
    [TestClass]
    public class VideosExperiment
    {
        int inputBits = 20 * 20;
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
            //Parameters p = GetParameters();
            double max = 20;


            //===================== input collection ========================
            //input are stored on different folders with their folder's names
            //as the settings value. However it is only for isolating the ini
            //conditions of the ball, how it will move. The generation of the 
            //data was done using a seperate python code.

            string testDir = getTestDir("SequenceLearningExperiments");
            string testDataDir = $"{testDir}\\VideosData";
            Debug.WriteLine(testDataDir);
            string[] a = Directory.GetDirectories(testDataDir, "*", SearchOption.TopDirectoryOnly);
            Debug.WriteLine(Path.GetFileName(a[0]));

            //List<ImageSet> InputVideos = ...;

            //string outFolder = $"{testOutputFolder}\\{digit}\\{topologies[topologyIndx]}x{topologies[topologyIndx]}";

            //Directory.CreateDirectory(outFolder);



        }
        private class ImageSet{
            public List<bool[]> ImageBinValue;
            private string IdName;
            public ImageSet(string IdName)
            {
                this.IdName = IdName;
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
        private string getTestDir(string testName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            //================ up dir one level ==========================
            currentDir = $"{ Directory.GetParent(currentDir)}";
            //================ up dir one level ==========================
            currentDir = $"{ Directory.GetParent(currentDir)}";
            //================ up dir one level ==========================
            currentDir = $"{ Directory.GetParent(currentDir)}";
            //================ test directory ============================
            return $"{currentDir}\\{testName}";
        }
    }
}
