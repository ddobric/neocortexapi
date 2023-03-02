// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest_Syed_Mostain_Ahmed
    {
        private static bool areDisjoined<T>(ICollection<T> arr1, ICollection<T> arr2)
        {
            foreach (var item in arr1)
            {
                if (arr2.Contains(item))
                    return false;
            }

            return true;
        }

        private Parameters getDefaultParameters()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 4);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.21);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.10);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        private HtmConfig GetDefaultTMParameters()
        {
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 4,
                ActivationThreshold = 3,
                InitialPermanence = 0.21,
                ConnectedPermanence = 0.5,
                MinThreshold = 2,
                MaxNewSynapseCount = 3,
                PermanenceIncrement = 0.1,
                PermanenceDecrement = 0.1,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(42),
                RandomGenSeed = 42
            };

            return htmConfig;
        }


        private Parameters getDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters() : p;
            retVal.Set(key, value);

            return retVal;
        }

        [TestMethod]
        public void CreateSynapse_WhenMaxSynapsesIsNotExceeded_AddsSynapseAndIncrementsCounters()
        {
            var segment = new Mock<DistalDendrite>();
            var synapses = new List<Synapse>();
            segment.Setup(s => s.Synapses).Returns(synapses);
            var presynapticCell = new Mock<Cell>();
            var receptorSynapses = new List<Synapse>();
            presynapticCell.Setup(s => s.ReceptorSynapses).Returns(receptorSynapses);
            double permanence = 0.5;

            var htmConfig = new HtmConfig
            {
                MaxSynapsesPerSegment = 10
            };

            var mockHtm = new Mock<Htm>();
            mockHtm.SetupGet(s => s.HtmConfig).Returns(htmConfig);

            var synapseManager = new SynapseManager(mockHtm.Object);

            // Act
            var synapse = synapseManager.CreateSynapse(segment.Object, presynapticCell.Object, permanence);

            // Assert
            Assert.IsNotNull(synapse);
            Assert.AreEqual(synapse.Permanence, permanence);
            Assert.AreEqual(segment.Object.Synapses.Count, 1);
            Assert.AreEqual(segment.Object.Synapses[0], synapse);
            Assert.AreEqual(presynapticCell.Object.ReceptorSynapses.Count, 1);
            Assert.AreEqual(presynapticCell.Object.ReceptorSynapses[0], synapse);
            Assert.AreEqual(synapseManager.NextSynapseOrdinal, 1);
            Assert.AreEqual(synapseManager.NumSynapses, 1);
        }

    }
}
