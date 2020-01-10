using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace UnitTestsProject.NoiseExperiments
{
    [TestClass]
    public class NoiseUnitTests
    {
        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Experiment")]
        public void SpatialSequenceLearningExperiment()
        {
            var parameters = GetDefaultParams();
            parameters.Set(KEY.POTENTIAL_RADIUS, 64 * 64);
            parameters.Set(KEY.POTENTIAL_PCT, 1.0);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.5);
            parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * 64 * 64);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * 64 * 64);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
            parameters.Set(KEY.MAX_BOOST, 5);

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPoolerMT();
            var mem = new Connections();

            double[] inputSequence = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };

            var inputVectors = GetEncodedSequence(inputSequence, 0.0, 100.0);

            parameters.apply(mem);

            sp.init(mem, UnitTestHelpers.GetMemory());

            foreach (var inputVector in inputVectors)
            {
                for (int i = 0; i < 3; i++)
                {
                    var activeIndicies = sp.Compute(inputVector, true, true) as int[];
                    var activeArray = sp.Compute(inputVector, true, false) as int[];

                    Debug.WriteLine(Helpers.StringifyVector(activeArray));
                    Debug.WriteLine(Helpers.StringifyVector(activeIndicies));
                }
            }
        }


        public List<int[]> GetEncodedSequence(double[] inputSequence, double min, double max)
        {
            List<int[]> sdrList = new List<int[]>();

            string outFolder = nameof(NoiseUnitTests);

            Directory.CreateDirectory(outFolder);

            DateTime now = DateTime.Now;

            ScalarEncoder encoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 21},
                { "N", 1024},
                { "Radius", -1.0},
                { "MinVal", min},
                { "MaxVal", max},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
            });

            foreach (var i in inputSequence)
            {
                var result = encoder.Encode(i);

                sdrList.Add(result);

                int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, (int)Math.Sqrt(result.Length), (int)Math.Sqrt(result.Length));
                var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{i}.png", Color.Yellow, Color.Black, text: i.ToString());
            }

            return sdrList;
        }

        internal static Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 50.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);

            return parameters;
        }
    }
}
