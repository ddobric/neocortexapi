// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;


namespace NeoCortexApi.Experiments
{
    /// <summary>
    /// Investigation of Hierarchical Temporal Memory Spatial Pooler’s Noise Robustness against Gaussian noise
    /// Sang Nguyen phuocsangnguyen97 @gmail.com
    /// Duy Nguyen ngthanhduy7 @gmail.com
    /// https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2019-2020/tree/VNGroup/Source/MyProject
    /// Check out student paper in the following URL: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/ML-19-20_20-5.12_SpatialPooler_NoiseRobustness.pdf
    /// </summary>
    [TestClass]

    [TestCategory("Not Working")]
    public class GaussianNoiseExperiment
    {
        [TestMethod]
        public void RunGaussianNoiseExperiment()
        {
            const int E_outBits = 423;
            const int columnsNumber = 2048;
            int[] SP_Result = null;
            int[] SP_NoisyResult = null;
            List<int[]> SP_Result_List = new List<int[]>();
            List<int[]> SP_NoisyResult_List = new List<int[]>();
            double[] ham_total = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] ham_avg = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double ham_upper;
            double ham_lower;
            int number_training_set = 41; // Integer numbers range from -20 to 20 with step of 1.
            int number_testing_set = 401; // Decimal numbers range from -20 to 20 with step of 0.1.

            string experimentFolder = nameof(GaussianNoiseExperiment);
            // string SP_noisyinFolder = nameof(GoussianNoiseExperiment);
            Directory.CreateDirectory(experimentFolder);
            // Directory.CreateDirectory(SP_noisyinFolder);
            string SP_inFile = $"{experimentFolder}\\MyEncoderOut.csv";
            string SP_noisyinFile = $"{experimentFolder}\\MyNoisyEncoderOut.csv";
            //string SP_outFolder = "MySPOutput";
            //string SP_noisyoutFolder = "MyNoisySPOutput";
            //Directory.CreateDirectory(SP_outFolder);
            //Directory.CreateDirectory(SP_noisyoutFolder);
            string SP_outFile = $"{experimentFolder}\\MySPOut.csv";
            string SP_noisyoutFile = $"{experimentFolder}\\MyNoisySPOut.csv";

            //string testFolder = "MyDraftFoler";
            //Directory.CreateDirectory(testFolder);

            //-------------------------------------------------------
            //|                     PARAMETERS                      |
            //-------------------------------------------------------
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));

            //------------------SPATIAL POOLER PARAMETERS-----------------
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { E_outBits });
            p.Set(KEY.POTENTIAL_RADIUS, -1);
            p.Set(KEY.POTENTIAL_PCT, 0.75);
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.INHIBITION_RADIUS, 15);
            p.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            p.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * columnsNumber);
            p.Set(KEY.STIMULUS_THRESHOLD, 5);
            p.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            p.Set(KEY.SYN_PERM_ACTIVE_INC, 0.05);
            p.Set(KEY.SYN_PERM_CONNECTED, 0.10);
            p.Set(KEY.SYN_PERM_BELOW_STIMULUS_INC, 0.01);
            p.Set(KEY.SYN_PERM_TRIM_THRESHOLD, 0.05);

            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 1);
            p.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);

            // These values activate powerfull boosting.
            p.Set(KEY.MAX_BOOST, 5);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 1);

            p.Set(KEY.MAX_BOOST, 10);
            p.Set(KEY.WRAP_AROUND, true);
            p.Set(KEY.LEARN, true);

            //-------------------TEMPORAL MEMORY PARAMETERS----------------
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { columnsNumber });
            p.Set(KEY.CELLS_PER_COLUMN, 32);
            p.Set(KEY.ACTIVATION_THRESHOLD, 10);
            p.Set(KEY.LEARNING_RADIUS, 10);
            p.Set(KEY.MIN_THRESHOLD, 9);
            p.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 20);
            p.Set(KEY.MAX_SYNAPSES_PER_SEGMENT, 225);
            p.Set(KEY.MAX_SEGMENTS_PER_CELL, 225);
            p.Set(KEY.INITIAL_PERMANENCE, 0.21);
            p.Set(KEY.CONNECTED_PERMANENCE, 0.1);
            p.Set(KEY.PERMANENCE_INCREMENT, 0.10);
            p.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            p.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.1);
            p.Set(KEY.LEARN, true);

            //Initiating components of a Cortex Layer
            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.Init(mem, UnitTestHelpers.GetMemory());
            tm1.Init(mem);

            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();

            Encoding(E_outBits);

            // Can adjust the number of SP learning cycles below
            for (int cycle = 0; cycle < 320; cycle++)
            {
                if (cycle >= 300)
                {
                    // These activates ew-born effect which switch offs the boosting.
                    //mem.setMaxBoost(0.0);
                    mem.HtmConfig.MaxBoost = 0.0;
                    //mem.updateMinPctOverlapDutyCycles(0.0);
                    mem.HtmConfig.MinPctOverlapDutyCycles = 0.0;
                }

                using (StreamReader sr = new StreamReader(SP_inFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(",");
                        int[] SP_input = new int[E_outBits];
                        for (int i = 0; i < E_outBits; i++)
                        {
                            if (tokens[i + 1] == "0")
                                SP_input[i] = 0;
                            else
                                SP_input[i] = 1;
                        }
                        SP_Result = sp1.Compute(SP_input, true);
                    }
                }
            }

            using (StreamReader sr = new StreamReader(SP_inFile))
            {
                string line;
                int lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(",");
                    int[] SP_input = new int[E_outBits];
                    for (int i = 0; i < E_outBits; i++)
                    {
                        if (tokens[i + 1] == "0")
                            SP_input[i] = 0;
                        else
                            SP_input[i] = 1;
                    }
                    SP_Result = sp1.Compute(SP_input, false, false);
                    SP_Result_List.Add(SP_Result);
                    int[,] SP_twoDimenArray = ArrayUtils.Make2DArray(SP_Result, 32, 64);
                    var SP_twoDimArray = ArrayUtils.Transpose(SP_twoDimenArray);
                    NeoCortexUtils.DrawBitmap(SP_twoDimArray, 1024, 1024, $"{experimentFolder}\\{lineNumber}.png", Color.DimGray, Color.LawnGreen, text: tokens[0]);
                    lineNumber++;
                }
            }

            using (StreamReader sr = new StreamReader(SP_noisyinFile))
            {
                string line;
                int lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(",");
                    int[] SP_input = new int[E_outBits];
                    for (int i = 0; i < E_outBits; i++)
                    {
                        if (tokens[i + 1] == "0")
                            SP_input[i] = 0;
                        else
                            SP_input[i] = 1;
                    }
                    SP_NoisyResult = sp1.Compute(SP_input, false, false);
                    SP_NoisyResult_List.Add(SP_NoisyResult);
                    var ham = MathHelpers.GetHammingDistance(SP_Result_List[lineNumber], SP_NoisyResult_List[lineNumber], true);
                    Debug.WriteLine($"Noisy input: {tokens[0]} - Hamming NonZ: {ham}");
                    ham = MathHelpers.GetHammingDistance(SP_Result_List[lineNumber], SP_NoisyResult_List[lineNumber], false);
                    Debug.WriteLine($"Noisy input: {tokens[0]} - Hamming All: {ham}");
                    int[,] SP_twoDimenArray = ArrayUtils.Make2DArray(SP_NoisyResult, 32, 64);
                    var SP_twoDimArray = ArrayUtils.Transpose(SP_twoDimenArray);
                    NeoCortexUtils.DrawBitmap(SP_twoDimArray, 1024, 1024, $"{experimentFolder}\\{lineNumber}.png", Color.DimGray, Color.LawnGreen, text: tokens[0]);
                    lineNumber++;
                }
            }

            for (int i = 0; i < number_testing_set - 1; i += 10)
            {
                int count = 1;
                for (int j = i + 1; j < i + 1 + 9; j++)
                {
                    if (i != 0 && i != number_testing_set - 1)
                    {
                        ham_upper = MathHelpers.GetHammingDistance(SP_NoisyResult_List[i], SP_NoisyResult_List[j], true);
                        ham_lower = MathHelpers.GetHammingDistance(SP_NoisyResult_List[i], SP_NoisyResult_List[i - count], true);
                        ham_total[count - 1] += ham_upper + ham_lower;
                        count++;
                    }
                    else if (i == 0)
                    {
                        ham_upper = MathHelpers.GetHammingDistance(SP_NoisyResult_List[i], SP_NoisyResult_List[j], true);
                        ham_total[count - 1] += ham_upper;
                        count++;
                    }
                    else
                    {
                        ham_lower = MathHelpers.GetHammingDistance(SP_NoisyResult_List[i], SP_NoisyResult_List[i - count], true);
                        ham_total[count - 1] += ham_lower;
                        count++;
                    }
                }
            }
            for (int i = 0; i < 9; i++)
            {
                ham_avg[i] = ham_total[i] / (number_training_set * 2 - 2);
                Debug.WriteLine($"0.{i + 1} step avg hamming distance: {ham_avg[i]}");
            }
        }

        /// <summary>
        /// Creating CSV file containing encoded input for the SP
        /// </summary>
        /// <param name="E_outBits"></param>
        private void Encoding(int local_E_outBits)
        {
            string E_inFile = "TestFiles\\sequence.csv";
            string E_outFolder = "MyEncoderOutput";
            string E_noisyinFile = "TestFiles\\noisy_sequence.csv";
            string E_noisyoutFolder = "MyNoisyEncoderOutput";
            Directory.CreateDirectory(E_outFolder);
            Directory.CreateDirectory(E_noisyoutFolder);
            string E_outFile = $"{E_outFolder}\\MyEncoderOut.csv";
            string E_noisyoutFile = $"{E_noisyoutFolder}\\MyNoisyEncoderOut.csv";

            Dictionary<string, object> scalarEncoderSettings = getScalarEncoderDefaultSettings(local_E_outBits);
            ScalarEncoder encoder = new ScalarEncoder(scalarEncoderSettings);
            ScalarEncoder noisyencoder = new ScalarEncoder(scalarEncoderSettings);

            using (StreamReader sr = new StreamReader(E_inFile))
            {
                string line;
                using (StreamWriter sw = new StreamWriter(E_outFile))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        List<int> E_output = new List<int>();
                        string[] tokens = line.Split(",");
                        var E_result = encoder.Encode(tokens[1]);
                        E_output.AddRange(E_result);
                        E_output.AddRange(new int[local_E_outBits - E_output.Count]);
                        var outArr = E_output.ToArray();
                        Debug.WriteLine($"-------------- {tokens[1]} --------------");
                        int[,] E_twoDimenArray = ArrayUtils.Make2DArray(outArr, 9, 47);
                        var E_twoDimArray = ArrayUtils.Transpose(E_twoDimenArray);
                        NeoCortexUtils.DrawBitmap(E_twoDimArray, 1024, 1024, $"{E_outFolder}\\{tokens[0].Replace("/", "-").Replace(":", "-")}.png", Color.Yellow, Color.Black, text: tokens[1]);

                        sw.Write($"{tokens[1]},");
                        for (int i = 0; i < outArr.Length; i++)
                        {
                            sw.Write(outArr[i]);
                            if (i < outArr.Length - 1) sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }
            }

            using (StreamReader sr = new StreamReader(E_noisyinFile))
            {
                string line;
                using (StreamWriter sw = new StreamWriter(E_noisyoutFile))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        List<int> E_output = new List<int>();
                        string[] tokens = line.Split(",");
                        var E_result = noisyencoder.Encode(tokens[1]);
                        E_output.AddRange(E_result);
                        E_output.AddRange(new int[local_E_outBits - E_output.Count]);
                        var outArr = E_output.ToArray();
                        Debug.WriteLine($"-------------- {tokens[1]} --------------");
                        int[,] E_twoDimenArray = ArrayUtils.Make2DArray(outArr, 9, 47);
                        var E_twoDimArray = ArrayUtils.Transpose(E_twoDimenArray);
                        NeoCortexUtils.DrawBitmap(E_twoDimArray, 1024, 1024, $"{E_noisyoutFolder}\\{tokens[0].Replace("/", "-").Replace(":", "-")}.png", Color.Yellow, Color.Black, text: tokens[1]);

                        sw.Write($"{tokens[1]},");
                        for (int i = 0; i < outArr.Length; i++)
                        {
                            sw.Write(outArr[i]);
                            if (i < outArr.Length - 1) sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// The getDefaultSettings
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, object}"/></returns>
        private static Dictionary<string, object> getScalarEncoderDefaultSettings(int inputBits)
        {
            Dictionary<string, object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("W", 23/*21*/);                       //the number of bits that are set to encode a single value -the "width" of the output signal 
                                                                      //restriction: w must be odd to avoid centering problems.
            encoderSettings.Add("N", inputBits /*4096*/);                     //The number of bits in the output. Must be greater than or equal to w
            encoderSettings.Add("MinVal", (double)-20.0);         //The minimum value of the input signal.
            encoderSettings.Add("MaxVal", (double)20.0);       //The upper bound of the input signal
                                                               //encoderSettings.Add("Radius", (double)0);         //Two inputs separated by more than the radius have non-overlapping representations.
                                                               //Two inputs separated by less than the radius will in general overlap in at least some
                                                               //of their bits. You can think of this as the radius of the input.
                                                               //encoderSettings.Add("Resolution", (double)0.15);  // Two inputs separated by greater than, or equal to the resolution are guaranteed
                                                               //to have different representations.
            encoderSettings.Add("Periodic", false);        //If true, then the input value "wraps around" such that minval = maxval
                                                           //For a periodic value, the input must be strictly less than maxval,
                                                           //otherwise maxval is a true upper bound.
            encoderSettings.Add("ClipInput", true);       //if true, non-periodic inputs smaller than minval or greater than maxval 
                                                          //will be clipped to minval/maxval

            encoderSettings.Add("Offset", 108);

            return encoderSettings;
        }
    }
}