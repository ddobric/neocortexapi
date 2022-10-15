using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to HtmConfig.
    /// </summary>
    [TestClass]
    public class HtmConfigTests
    {
        [TestMethod]
        public void CompareHtmConfigs()
        {
            int[] inputDims1 = { 1, 2, 3 };
            int[] inputDims2 = { 10, 12, 14 };
            int[] columnDims1 = { 512, 1024 };
            int[] columnDims2 = { 10, 100, 1000 };

            HtmConfig config1 = new HtmConfig(inputDims1, columnDims1);
            HtmConfig config2 = new HtmConfig(inputDims1, columnDims1);
            HtmConfig config3 = new HtmConfig(inputDims2, columnDims1);
            HtmConfig config4 = new HtmConfig(inputDims2, columnDims2);
            HtmConfig config5 = new HtmConfig(inputDims2, columnDims2);
            HtmConfig config6 = new HtmConfig(inputDims2, columnDims2);

            //config4 parameters
            config4.CellsPerColumn = 32;
            config4.ActivationThreshold = 10;
            config4.LearningRadius = 10;
            config4.MinThreshold = 9;
            config4.MaxNewSynapseCount = 20;
            config4.MaxSynapsesPerSegment = 225;
            config4.MaxSegmentsPerCell = 225;
            config4.InitialPermanence = 0.21;
            config4.ConnectedPermanence = 0.5;
            config4.PermanenceIncrement = 0.10;
            config4.PermanenceDecrement = 0.10;
            config4.PredictedSegmentDecrement = 0.1;
            config4.PotentialRadius = 15;
            config4.PotentialPct = 0.75;
            config4.GlobalInhibition = true;
            config4.LocalAreaDensity = -1.0;
            config4.NumActiveColumnsPerInhArea = 0.02 * 2048;
            config4.StimulusThreshold = 5.0;
            config4.SynPermInactiveDec = 0.008;
            config4.SynPermActiveInc = 0.05;
            config4.SynPermConnected = 0.1;
            config4.SynPermBelowStimulusInc = 0.01;
            config4.SynPermTrimThreshold = 0.05;
            config4.MinPctOverlapDutyCycles = 0.001;
            config4.MinPctActiveDutyCycles = 0.001;
            config4.DutyCyclePeriod = 1000;
            config4.MaxBoost = 10.0;
            config4.WrapAround = true;
            config4.Random = new ThreadSafeRandom(42);

            //config5 parameters
            config5.CellsPerColumn = 32;
            config5.ActivationThreshold = 10;
            config5.LearningRadius = 10;
            config5.MinThreshold = 9;
            config5.MaxNewSynapseCount = 20;
            config5.MaxSynapsesPerSegment = 225;
            config5.MaxSegmentsPerCell = 225;
            config5.InitialPermanence = 0.21;
            config5.ConnectedPermanence = 0.5;
            config5.PermanenceIncrement = 0.10;
            config5.PermanenceDecrement = 0.10;
            config5.PredictedSegmentDecrement = 0.1;
            config5.PotentialRadius = 15;
            config5.PotentialPct = 0.75;
            config5.GlobalInhibition = true;
            config5.LocalAreaDensity = -1.0;
            config5.NumActiveColumnsPerInhArea = 0.02 * 2048;
            config5.StimulusThreshold = 5.0;
            config5.SynPermInactiveDec = 0.008;
            config5.SynPermActiveInc = 0.05;
            config5.SynPermConnected = 0.1;
            config5.SynPermBelowStimulusInc = 0.01;
            config5.SynPermTrimThreshold = 0.05;
            config5.MinPctOverlapDutyCycles = 0.001;
            config5.MinPctActiveDutyCycles = 0.001;
            config5.DutyCyclePeriod = 1000;
            config5.MaxBoost = 10.0;
            config5.WrapAround = true;
            config5.Random = new ThreadSafeRandom(42);

            //config6 parameters
            config6.CellsPerColumn = 128;
            config6.ActivationThreshold = 100;
            config6.LearningRadius = 15;
            config6.MinThreshold = 9;
            config6.MaxNewSynapseCount = 20;
            config6.MaxSynapsesPerSegment = 225;
            config6.MaxSegmentsPerCell = 225;
            config6.InitialPermanence = 0.21;
            config6.ConnectedPermanence = 0.5;
            config6.PermanenceIncrement = 0.10;
            config6.PermanenceDecrement = 0.10;
            config6.PredictedSegmentDecrement = 0.1;
            config6.PotentialRadius = 15;
            config6.PotentialPct = 0.75;
            config6.GlobalInhibition = true;
            config6.LocalAreaDensity = -1.0;
            config6.NumActiveColumnsPerInhArea = 0.02 * 2048;
            config6.StimulusThreshold = 5.0;
            config6.SynPermInactiveDec = 0.008;
            config6.SynPermActiveInc = 0.05;
            config6.SynPermConnected = 0.1;
            config6.SynPermBelowStimulusInc = 0.01;
            config6.SynPermTrimThreshold = 0.05;
            config6.MinPctOverlapDutyCycles = 0.001;
            config6.MinPctActiveDutyCycles = 0.001;
            config6.DutyCyclePeriod = 100;
            config6.MaxBoost = 10.0;
            config6.WrapAround = true;
            config6.Random = new ThreadSafeRandom(42);

            //Not same by reference
            Assert.IsFalse(config1 == config2);

            //config1 and config2 are same by value
            Assert.IsTrue(config1.Equals(config2));

            //config1 and config3 are NOT same by value
            Assert.IsFalse(config1.Equals(config3));

            //config4 and config5 are same by value
            Assert.IsTrue(config4.Equals(config5));

            //config4 and config6 are NOT same by value
            Assert.IsFalse(config4.Equals(config6));
        }
    }
}
