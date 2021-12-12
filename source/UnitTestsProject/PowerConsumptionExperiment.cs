// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UnitTestsProject
{
    [TestClass]
    public class PowerConsumptionExperiment
    {
        private const int OutImgSize = 1024;


        [TestMethod]
        [TestCategory("LongRunning")]
        public void RunPowerPredictionExperiment()
        {
            const int inputBits = 300; /* without datetime component */ // 13420; /* with 4096 scalar bits */ // 10404 /* with 1024 bits */;

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 2048 });
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10 /* 50 */);
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.CONNECTED_PERMANENCE, 0.1);
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.setNumActiveColumnsPerInhArea(0.02 * 2048);
            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10 /*15*/);
            p.setInhibitionRadius(15);
            p.Set(KEY.MAX_BOOST, 0.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100000);
            p.setActivationThreshold(10);
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * 2048));
            p.setPermanenceIncrement(0.17);

            //p.Set(KEY.MAX_SYNAPSES_PER_SEGMENT, 32);
            //p.Set(KEY.MAX_SEGMENTS_PER_CELL, 128);
            //p.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 200);

            //p.Set(KEY.POTENTIAL_RADIUS, 700);
            //p.Set(KEY.POTENTIAL_PCT, 0.5);
            //p.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 42);

            //p.Set(KEY.LOCAL_AREA_DENSITY, -1);

            CortexRegion region0 = new CortexRegion("1st Region");

            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.Init(mem, UnitTestHelpers.GetMemory());
            tm1.Init(mem);

            Dictionary<string, object> settings = new Dictionary<string, object>();

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("sp", sp1);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Train(inputBits, layer1, cls, true); // New born mode.
            sw.Stop();

            Debug.WriteLine($"NewBorn stage duration: {sw.ElapsedMilliseconds / 1000} s");

            layer1.AddModule("tm", tm1);

            sw.Start();

            int hunderdAccCnt = 0;
            for (int i = 0; i < 1000; i++)
            {
                float acc = Train(inputBits, layer1, cls, false);

                Debug.WriteLine($"Accuracy = {acc}, Cycle = {i}");

                if (acc == 100.0)
                    hunderdAccCnt++;

                if (hunderdAccCnt >= 10)
                {
                    break;
                }
                //tm1.reset(mem);
            }

            if (hunderdAccCnt >= 10)
            {
                Debug.WriteLine($"EXPERIMENT SUCCESS. Accurracy 100% reached.");
            }
            else
            {
                Debug.WriteLine($"Experiment FAILED!. Accurracy 100% was not reached.");
            }

            cls.TraceState();

            sw.Stop();

            Debug.WriteLine($"Training duration: {sw.ElapsedMilliseconds / 1000} s");

        }

        double lastPredictedValue = 0.0;

        private float Train(int inpBits, IHtmModule<object, object> network, HtmClassifier<double, ComputeCycle> cls, bool isNewBornMode = true)
        {
            float accurracy;

            string outFolder = nameof(RunPowerPredictionExperiment);

            Directory.CreateDirectory(outFolder);

            CortexNetworkContext ctx = new CortexNetworkContext();

            Dictionary<string, object> scalarEncoderSettings = getScalarEncoderDefaultSettings(inpBits);
            //var dateTimeEncoderSettings = getFullDateTimeEncoderSettings();

            ScalarEncoder scalarEncoder = new ScalarEncoder(scalarEncoderSettings);
            //DateTimeEncoder dtEncoder = new DateTimeEncoder(dateTimeEncoderSettings, DateTimeEncoder.Precision.Hours);

            string fileName = "TestFiles\\rec-center-hourly-short.csv";

            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                int cnt = 0;
                int matches = 0;

                using (StreamWriter sw = new StreamWriter("out.csv"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        cnt++;

                        //if (isNewBornMode && cnt > 100) break;

                        bool x = false;
                        if (x)
                        {
                            break;
                        }
                        List<int> output = new List<int>();

                        string[] tokens = line.Split(",");

                        // Encode scalar value
                        var result = scalarEncoder.Encode(tokens[1]);

                        output.AddRange(result);

                        // This part adds datetime components to the input vector.
                        //output.AddRange(new int[scalarEncoder.Offset]);
                        //DateTime dt = DateTime.Parse(tokens[0], CultureInfo.InvariantCulture);
                        // Encode date/time/hour.
                        //result = dtEncoder.Encode(new DateTimeOffset(dt, TimeSpan.FromMilliseconds(0)));
                        //output.AddRange(result);

                        // This performs a padding to the inputBits = 10404 = 102*102.
                        output.AddRange(new int[inpBits - output.Count]);

                        var outArr = output.ToArray();

                        Debug.WriteLine($"-------------- {tokens[1]} --------------");

                        if (isNewBornMode)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                // Output here are active cells.
                                var res = network.Compute(output.ToArray(), true);

                                Debug.WriteLine(Helpers.StringifyVector(((int[])res)));
                            }
                        }
                        else
                        {
                            var lyrOut = network.Compute(output.ToArray(), true) as ComputeCycle; ;

                            double input = Convert.ToDouble(tokens[1], CultureInfo.InvariantCulture);

                            if (input == lastPredictedValue)
                            {
                                matches++;
                                Debug.WriteLine($"Match {input}");
                            }
                            else
                                Debug.WriteLine($"Missmatch Actual value: {input} - Predicted value: {lastPredictedValue}");

                            cls.Learn(input, lyrOut.ActiveCells.ToArray());

                            lastPredictedValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());

                            sw.WriteLine($"{tokens[0]};{input.ToString(CultureInfo.InvariantCulture)};{lastPredictedValue.ToString(CultureInfo.InvariantCulture)}");

                            Debug.WriteLine($"W: {Helpers.StringifyVector(lyrOut.WinnerCells.Select(c => c.Index).ToArray())}");
                            Debug.WriteLine($"P: {Helpers.StringifyVector(lyrOut.PredictiveCells.Select(c => c.Index).ToArray())}");
                            Debug.WriteLine($"Current input: {input} Predicted Input: {lastPredictedValue}");

                            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(outArr, (int)Math.Sqrt(outArr.Length), (int)Math.Sqrt(output.Count));
                            var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
                            NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{tokens[0].Replace("/", "-").Replace(":", "-")}.png", Color.Yellow, Color.Black, text: input.ToString());

                        }

                        Debug.WriteLine($"NewBorn stage: {isNewBornMode} - record: {cnt}");

                    }
                }

                accurracy = (float)matches / (float)cnt * (float)100.0;
            }

            return accurracy;
        }




        /// <summary>
        /// The getDefaultSettings
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, object}"/></returns>
        private static Dictionary<string, object> getScalarEncoderDefaultSettings(int inputBits)
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("W", 15 /*21*/);                       //the number of bits that are set to encode a single value -the "width" of the output signal 
                                                                       //restriction: w must be odd to avoid centering problems.
            encoderSettings.Add("N", inputBits /*4096*/);                     //The number of bits in the output. Must be greater than or equal to w
            encoderSettings.Add("MinVal", (double)0.0);         //The minimum value of the input signal.
            encoderSettings.Add("MaxVal", (double)60);       //The upper bound of the input signal
                                                             //encoderSettings.Add("Radius", (double)0);         //Two inputs separated by more than the radius have non-overlapping representations.
                                                             //Two inputs separated by less than the radius will in general overlap in at least some
                                                             //of their bits. You can think of this as the radius of the input.
                                                             //encoderSettings.Add("Resolution", (double)0.15);  // Two inputs separated by greater than, or equal to the resolution are guaranteed
                                                             //to have different representations.
            encoderSettings.Add("Periodic", (bool)false);        //If true, then the input value "wraps around" such that minval = maxval
                                                                 //For a periodic value, the input must be strictly less than maxval,
                                                                 //otherwise maxval is a true upper bound.
            encoderSettings.Add("ClipInput", (bool)false);       //if true, non-periodic inputs smaller than minval or greater than maxval 
                                                                 //will be clipped to minval/maxval

            encoderSettings.Add("Offset", 108);

            return encoderSettings;
        }



        /// <summary>
        /// Values for the radius and width will be passed here. For this unit test the width = 1 and radius = 1.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, object>> getFullDateTimeEncoderSettings()
        {
            Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();

            encoderSettings.Add("SeasonEncoder",
              new Dictionary<string, object>()
              {
                    { "W", 21},
                    { "N", 128},
                    //{ "Radius", 365/4},
                    { "MinVal", 1.0},
                    { "MaxVal", 367.0},
                    { "Periodic", true},
                    { "Name", "SeasonEncoder"},
                    { "ClipInput", true},
                    { "Offset", 50},
              }
              );

            encoderSettings.Add("DayOfWeekEncoder",
                new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 128},
                    { "MinVal", 0.0},
                    { "MaxVal", 7.0},
                    { "Periodic", false},
                    { "Name", "DayOfWeekEncoder"},
                    { "ClipInput", false},
                    { "Offset", 50},
                });

            encoderSettings.Add("WeekendEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 42},
                    { "MinVal", 0.0},
                    { "MaxVal", 1.0},
                    { "Periodic", false},
                    { "Name", "WeekendEncoder"},
                    { "ClipInput", true},
                    { "Offset", 50},
                });


            encoderSettings.Add("DateTimeEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 8640},
                     // This means 8640 hours.
                    { "MinVal", new DateTimeOffset(new DateTime(2010, 1, 1), TimeSpan.FromHours(0))},
                    { "MaxVal", new DateTimeOffset(new DateTime(2011, 1, 1), TimeSpan.FromHours(0))},
                    { "Periodic", false},
                    { "Name", "DateTimeEncoder"},
                    { "ClipInput", false},
                    { "Offset", 128},
                });

            return encoderSettings;
        }
    }

}
