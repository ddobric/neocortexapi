using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using UnitTestsProject;

namespace MyUnitTest
{
    [TestClass]
    public class SinusoidalNoiseExperiment
    {
        /// <summary>
        /// This experiment aims at investigating Spatial Pooler's noise robustness and specificity (as defined in the report) under common settings.
        /// Before running the experiment, please create a new folder with the name "MyEncoderInput" in the output folder of this project and then copy all the input files to that folder.
        /// For the Robustness test, the final output file is the CSV file containing Hamming distances between Spatial Pooler's output with respect to the training input file and the testing input file
        /// For the Specificity test, the final output file is the CSV file containing Hamming distances between Spatial Pooler's output with respect to every neighboring value pair in the specificity testing file
        /// (Notice: please remove the REPAIR_STABILITY feature (if it is not yet removed) in "SpatialPooler.cs" in "NeoCortexApi" library before running the experiment)
        /// </summary>
        [TestMethod]
        public void MyTestMethod()
        {
            //DIRECTORIES TO STORE INPUT AND OUTPUT FILES OF THE EXPERIMENT

            //Encoder
            string E_inFolder = "NoiseExperiments/Input"; //Encoder's raw input file directory
            //Directory.CreateDirectory(E_inFolder);

            //----------------INPUT FILE PATH-----------------
            string E_inFile_train = $"{E_inFolder}\\sinusoidal.csv"; //Encoder's input file in "Training mode"
            //<Robustness>
            string E_inFile_robustness = $"{E_inFolder}/Noisy_N-0-2_sinusoidal.csv"; // Encoder's input file in "Testing mode - Robustness" - All the files with name of the form "Noisy_*.csv"
            //</Robustness>
            //< Specificity >
            string E_inFile_specificity = $"{E_inFolder}/sinusoidal-specificity.csv"; // Encoder's input file in "Testing mode - Specificity"
            //</ Specificity >
            //------------------------------------------------

            string E_outFolder = "NoiseExperiments/MyEncoderOutput"; //Encoder's graphical output (PNG format) during "Testing mode" will be created here
            Directory.CreateDirectory(E_outFolder);
            string E_outFolder_train = $"{E_outFolder}/train"; // Encoder's graphical output (PNG format) during "Training mode" will be created here
            Directory.CreateDirectory(E_outFolder_train);

            //Spatial Pooler
            string SP_inFolder = "NoiseExperiments/MySPInput"; //Spatial Pooler's input file (Encoder's output (CSV format)) directory
            Directory.CreateDirectory(SP_inFolder);
            string SP_inFile_train = $"{SP_inFolder}/MyEncoderOut_train.csv"; //Spatial Pooler's input file during "Training mode"
            string SP_inFile_robustness = $"{SP_inFolder}/MyEncoderOut_robustness.csv"; //Spatial Pooler's input file during "Testing mode - robustness"
            string SP_inFile_specificity = $"{SP_inFolder}/MyEncoderOut_specificity.csv"; //Spatial Pooler's input file during "Testing mode - specificity"
            string SP_outFolder = "MySPOutput"; //Spatial Pooler's graphical output (PNG format) will be stored here
            Directory.CreateDirectory(SP_outFolder);

            string SP_outFolder_compare = $"{SP_outFolder}/compare_445"; //This folder containing CSV files, which show Hamming distance between Spatial Pooler's output in "Training mode" and "Testing Mode"
            Directory.CreateDirectory(SP_outFolder_compare);

            //--------------------OUTPUT FILE PATH-------------------------
            //<Robustness>
            string SP_outFile_robustness = $"{SP_outFolder_compare}/compare_N-0-2.csv"; //The final output file in "Robustness test"
            //</Robustness>
            //<Specificity>
            string SP_outFile_specificity = $"{SP_outFolder_compare}/compare_specificity.csv"; //The final output file in "Specificity test"
            //</Specificity>
            //-------------------------------------------------------------


            //-------------------------------------------------------
            //|                    HTM PARAMETERS                   |
            //-------------------------------------------------------

            const int E_outBits = 445; //Number of Scalar Encoder's output bits
            const int columnsNumber = 2048; //Number of Spatial Pooler's output columns

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));

            //------------------SPATIAL POOLER PARAMETERS-----------------
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { E_outBits });
            p.Set(KEY.POTENTIAL_RADIUS, -1);
            p.Set(KEY.POTENTIAL_PCT, 1);
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.INHIBITION_RADIUS, 15);

            //Leave it
            p.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            p.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.02 * columnsNumber);

            p.Set(KEY.STIMULUS_THRESHOLD, 0.5);
            p.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.008);
            p.Set(KEY.SYN_PERM_ACTIVE_INC, 0.01);
            p.Set(KEY.SYN_PERM_CONNECTED, 0.10);

            //Leave it
            p.Set(KEY.SYN_PERM_BELOW_STIMULUS_INC, 0.01);
            p.Set(KEY.SYN_PERM_TRIM_THRESHOLD, 0.05);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            p.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);

            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            p.Set(KEY.MAX_BOOST, 10 /*10.0*/);
            p.Set(KEY.WRAP_AROUND, true);
            //p.Set(KEY.LEARN, true);


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
            p.Set(KEY.CONNECTED_PERMANENCE, 0.5);
            p.Set(KEY.PERMANENCE_INCREMENT, 0.10);
            p.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            p.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.1);
            //p.Set(KEY.LEARN, true);

            //---------------------------------------------------
            //|                    UNIT TEST                    |
            //---------------------------------------------------

            //Initiating HTM modules
            SpatialPoolerMT sp1 = new SpatialPoolerMT();
            TemporalMemory tm1 = new TemporalMemory();
            var mem = new Connections();
            p.apply(mem);
            sp1.Init(mem, UnitTestHelpers.GetMemory());
            tm1.Init(mem);
            HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();



            //-------------------ENCODING INPUTS----------------------
            Encoding(E_inFile_train, SP_inFile_train, E_outFolder_train, E_outBits);



            //--------------------TRAINING MODE---------------------

            //SP training
            for (int j = 0; j < 5; j++)
            {
                using (StreamReader sr = new StreamReader(SP_inFile_train))
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
                        for (int i = 0; i < 3; i++)
                            sp1.Compute(SP_input, true);
                    }
                }
            }

            Debug.WriteLine("-----------------------------------------------------");
            Debug.WriteLine("|-----------------FINISHED TRAINING-----------------|");
            Debug.WriteLine("-----------------------------------------------------");

            //TM + SP training
            double lastPredictedValue = 0.0;
            for (int j = 0; j < 20; j++)
            {
                using (StreamReader sr = new StreamReader(SP_inFile_train))
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
                        var SP_res = sp1.Compute(SP_input, true);
                        var TM_res = tm1.Compute(SP_res, true) as ComputeCycle;
                        double input = Convert.ToDouble(tokens[0], CultureInfo.InvariantCulture);
                        Debug.WriteLine($"Input: {input} - Predicted: {lastPredictedValue}");
                        //cls.Learn(input, TM_res.ActiveCells.ToArray(), TM_res.PredictiveCells.ToArray());
                        lastPredictedValue = cls.GetPredictedInputValue(TM_res.PredictiveCells.ToArray());

                    }
                }
            }

        }

        /// <summary>
        /// Creating CSV file containing encoded input for the SP
        /// </summary>
        /// <param name="E_inputFilePath">Input CSV file path</param>
        /// <param name="E_outputFilePath">Output CSV file path</param>
        /// <param name="local_E_outFolder">Folder to store graphical representation of the Encoder's output</param>
        /// <param name="local_E_outBits">Number of the Scalar Encoder's output bits</param>
        private void Encoding(string E_inputFilePath, string E_outputFilePath, string local_E_outFolder, int local_E_outBits)
        {
            Dictionary<string, object> scalarEncoderSettings = GetScalarEncoderDefaultSettings(local_E_outBits);
            ScalarEncoder encoder = new ScalarEncoder(scalarEncoderSettings);

            using (StreamReader sr = new StreamReader(E_inputFilePath))
            {
                string line;
                using (StreamWriter sw = new StreamWriter(E_outputFilePath))
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
                        //int[,] E_twoDimenArray = ArrayUtils.Make2DArray<int>(outArr, 15, 31);
                        //var E_twoDimArray = ArrayUtils.Transpose(E_twoDimenArray);
                        //NeoCortexUtils.DrawBitmap(E_twoDimArray, 1024, 1024, $"{local_E_outFolder}\\{tokens[0].Replace("/", "-").Replace(":", "-")}.png", Color.Yellow, Color.Black, text: tokens[1]);

                        sw.Write($"{tokens[1]},");
                        for (int i = 0; i < outArr.Length; i++)
                        {
                            sw.Write(outArr[i]);
                            if (i < (outArr.Length - 1)) sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Get the Scalar Encoder's settings
        /// </summary>
        /// <param name="inputBits">Total number of bits used to encode input data</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetScalarEncoderDefaultSettings(int inputBits)
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            //-----------------------SCALAR ENCODER PARAMETER---------------------------
            encoderSettings.Add("W", 45);                       //the number of bits that are set to encode a single value -the "width" of the output signal 
                                                                //restriction: w must be odd to avoid centering problems.
            encoderSettings.Add("N", inputBits);                     //The number of bits in the output. Must be greater than or equal to w
            encoderSettings.Add("MinVal", (double)-20.0);         //The minimum value of the input signal.
            encoderSettings.Add("MaxVal", (double)20.0);       //The upper bound of the input signal
                                                               //encoderSettings.Add("Radius", (double)0);         //Two inputs separated by more than the radius have non-overlapping representations.
                                                               //Two inputs separated by less than the radius will in general overlap in at least some
                                                               //of their bits. You can think of this as the radius of the input.
                                                               //encoderSettings.Add("Resolution", (double)0.15);  // Two inputs separated by greater than, or equal to the resolution are guaranteed
                                                               //to have different representations.
            encoderSettings.Add("Periodic", (bool)false);        //If true, then the input value "wraps around" such that minval = maxval
                                                                 //For a periodic value, the input must be strictly less than maxval,
                                                                 //otherwise maxval is a true upper bound.
            encoderSettings.Add("ClipInput", (bool)true);       //if true, non-periodic inputs smaller than minval or greater than maxval 
                                                                //will be clipped to minval/maxval

            encoderSettings.Add("Offset", 108);

            return encoderSettings;
        }
    }
}