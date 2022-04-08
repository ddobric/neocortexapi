// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System.Collections.Generic;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests for HomeostaticPlasticityActivator.
    /// </summary>
    [TestClass]
    public class HomeostaticPlasticityActivatorTests
    {
        [TestMethod]
        public void TestHashDictionary()
        {
            double minOctOverlapCycles = 1.0;
            int inputBits = 100;
            int numColumns = 2048;
            double maxBoost = 5.0;

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.MAX_BOOST, maxBoost);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minOctOverlapCycles);

            var mem = new Connections();

            List<int[]> inputs = new List<int[]>();
            List<int[]> outputs = new List<int[]>();

            // Create random vectors with 2% sparsity
            for (int i = 0; i < 10; i++)
            {
                var inp = NeoCortexUtils.CreateRandomVector(inputBits, 20);
                var outp = NeoCortexUtils.CreateRandomVector(numColumns, 40);

                inputs.Add(inp);
                outputs.Add(outp);
            }

            bool isBoostOff = true;

            int learningCycles = 100;

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputs.Count * learningCycles,
                (isStable, numPatterns, actColAvg, seenInputs) => { });

            for (int cycle = 0; cycle < 1000; cycle++)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    isBoostOff = hpa.Compute(inputs[i], outputs[i]);

                    if (isBoostOff)
                        Assert.IsTrue(cycle >= learningCycles);
                }
            }

            Assert.IsTrue(isBoostOff);
        }

        [TestMethod]
        public void TestCorrelation1()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));


            Assert.IsTrue(res == 1.0);
        }

        [TestMethod]
        public void TestCorrelation2()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 0, 0, 1 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));

            Assert.IsTrue(res == 0.8);
        }

        [TestMethod]
        public void TestCorrelation3()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 0, 0 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));

            Assert.IsTrue(res == 0.6);
        }

        [TestMethod]
        public void TestCorrelation4()
        {
            int[] arr1 = new int[] { 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0 };
            int[] arr2 = new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

            var res = HomeostaticPlasticityController.CalcArraySimilarity(ArrayUtils.IndexWhere(arr1, i => i == 1), ArrayUtils.IndexWhere(arr2, i => i == 1));

            Assert.IsTrue(res == 0.8);
        }
    }
}
