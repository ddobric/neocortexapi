using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace NeoCortexApiSample
{
    /// <summary>
    /// Implements an experiment that demonstrates how to learn spatial patterns.
    /// SP will learn every presented input in multiple iterations.
    /// </summary>
    public class SpatialPatternLearning
    {
        public void Run()
        {
            Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(SpatialPatternLearning)}");

            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represent an input vector (pattern).
            int inputBits = 200;

            // We will build a slice of the cortex with the given number of mini-columns
            int numColumns = 1024;

            //
            // This is a set of configuration parameters used in the experiment.
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,
                
                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold=10,
            };

            double max = 100;

            //
            // This dictionary defines a set of typical encoder parameters.
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };


            EncoderBase encoder = new ScalarEncoder(settings);

            //
            // We create here 100 random input values.
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            var sp = RunExperiment(cfg, encoder, inputValues);

            //RunRustructuringExperiment(sp, encoder, inputValues);
        }

       

        /// <summary>
        /// Implements the experiment.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="encoder"></param>
        /// <param name="inputValues"></param>
        /// <returns>The trained bersion of the SP.</returns>
        private static SpatialPooler RunExperiment(HtmConfig cfg, EncoderBase encoder, List<double> inputValues)
        {
            // Creates the htm memory.
            var mem = new Connections(cfg);

            bool isInStableState = false;

            //
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPooler(hpa);
            //sp = new SpatialPoolerMT(hpa);

            // Initializes the 
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // mem.TraceProximalDendritePotential(true);

            // It creates the instance of the neo-cortex layer.
            // Algorithm will be performed inside of that layer.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Add encoder as the very first module. This model is connected to the sensory input cells
            // that receive the input. Encoder will receive the input and forward the encoded signal
            // to the next module.
            cortexLayer.HtmModules.Add("encoder", encoder);

            // The next module in the layer is Spatial Pooler. This module will receive the output of the
            // encoder.
            cortexLayer.HtmModules.Add("sp", sp);

            double[] inputs = inputValues.ToArray();

            // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            //
            // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 1000;

            int numStableCycles = 0;

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

                //
                // This trains the layer on input pattern.
                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    // 
                    var lyrOut = cortexLayer.Compute((object)input, true) as int[];

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }

                if (isInStableState)
                {
                    numStableCycles++;
                }

                if (numStableCycles > 5)
                    break;
            }

            return sp;
        }
        /// <summary>
        /// Executes an experiment to analyze and visualize the behavior of a Spatial Pooler (SP) in response to a sequence of encoded input values. 
        /// This method systematically encodes each input value into a Sparse Distributed Representation (SDR) using the specified encoder (Scaler Encoder), 
        /// then processes these SDRs through the SP (Saptial Pooler Algorithm) to identify active columns. It reconstructs permanence values for these active columns, 
        /// normalizes them against a predefined threshold, and aggregates this data to generate visual heatmaps. These heatmaps illustrate 
        /// how the SP's internal representations of inputs evolve over time, enabling a deeper understanding of its learning and memory processes.
        /// Additionally, the method assesses the SP's ability to adapt its synaptic connections (permanences) in response to the inputs, 
        /// thereby effectively 'training' the SP through exposure to the dataset. The experiment aims to shed light on the dynamics of synaptic 
        /// plasticity within the SP framework, offering insights that could guide the tuning of its parameters for improved performance in specific tasks.
        /// </summary>
        /// <param name="sp">The Spatial Pooler instance to be used for the experiment. It processes input SDRs to simulate neural activity and synaptic plasticity.</param>
        /// <param name="encoder">The encoder used for converting raw input values into SDRs. The quality of encoding directly influences the SP's performance and the experiment's outcomes.</param>
        /// <param name="inputValues">A list of input values to be encoded and processed through the SP. These values serve as the experimental dataset, exposing the SP to various patterns and contexts.</param>
        /// <remarks>
        /// <para>The "maxInput" is the Maximum number of inputs to consider for reconstruction according to the size of Encoded Inputs.</para>
        /// <para>The "thresholdValue" is Threshold value for permanence normalization to convert them as 0 and 1 Like the encoded Inputs.</para>
        /// <para>Initializes lists to store heatmap data and normalized permanence values.</para>
        /// <para>Iterates over each input value, encoding it, computing active columns, and reconstructing permanence values.</para>
        /// <para>Populates a dictionary with all reconstructed permanence values and ensures representation up to a maximum input index.</para>
        /// <para>Converts permanence values to a list and adds it to the heatmap data.</para>
        /// <para>Outputs debug information about input values and corresponding Sparse Distributed Representation (SDR).</para>
        /// <para>Defines a threshold for permanence normalization and normalizes permanences based on this threshold.</para>
        /// <para>Adds normalized permanences to a list.</para>
        /// <para>Calls a method to generate 1D heatmaps using the heatmap data and normalized permanences.</para>
        /// </remarks>



        // Define a method to run the restructuring experiment, which takes a spatial pooler, an encoder, and a list of input values as arguments.
        private void RunRustructuringExperiment(SpatialPooler sp, EncoderBase encoder, List<double> inputValues)
        {
            // Initialize a list to get heatmap data for all input values.
            List<List<double>> heatmapData = new List<List<double>>();

            // Initialize a list to get normalized permanence values.
            List<int[]> normalizedPermanence = new List<int[]>();

            // Initialize a list to get normalized permanence values.
            List<int[]> encodedInputs = new List<int[]>();

            // Initialize a list to measure the similarities.
            List<double[]> similarityList = new List<double[]>();

            // Loop through each input value in the list of input values.
            foreach (var input in inputValues)
            {
                // Encode the current input value using the provided encoder, resulting in an SDR
                var inpSdr = encoder.Encode(input);

                // Compute the active columns in the spatial pooler for the given input SDR, without learning.
                var actCols = sp.Compute(inpSdr, false);

                // Reconstruct the permanence values for the active columns.
                Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actCols);

                // Define the maximum number of inputs (Same size of encoded Inputs) to consider.
                int maxInput = inpSdr.Length;

                // Initialize a dictionary to hold all permanence values, including those not reconstructed becuase of Inactive columns.
                Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();

                // Populate the all permanence dictionary with reconstructed permanence values.
                foreach (var kvp in reconstructedPermanence)
                {
                    int inputIndex = kvp.Key;

                    double probability = kvp.Value;

                    allPermanenceDictionary[inputIndex] = probability;

                }
                // Ensure that all input indices up to the maximum are represented in the dictionary, even if their permanence is 0.
                for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
                {

                    if (!reconstructedPermanence.ContainsKey(inputIndex))
                    {

                        allPermanenceDictionary[inputIndex] = 0.0;
                    }
                }
                // Sort the dictionary by keys
                var sortedAllPermanenceDictionary = allPermanenceDictionary.OrderBy(kvp => kvp.Key);

                // Convert the sorted dictionary of all permanences to a list
                List<double> permanenceValuesList = sortedAllPermanenceDictionary.Select(kvp => kvp.Value).ToList();

                heatmapData.Add(permanenceValuesList);

                // Output debug information showing the input value and its corresponding SDR as a string.
                Debug.WriteLine($"Input: {input} SDR: {Helpers.StringifyVector(actCols)}");

                // Define a threshold value for normalizing permanences, this value provides best Reconstructed Input
                var ThresholdValue = 8.3;

                // Normalize permanences (0 and 1) based on the threshold value and convert them to a list of integers.
                List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);

                // Add the normalized permanences to the list of all normalized permanences.
                normalizedPermanence.Add(normalizePermanenceList.ToArray());

                // Add the encoded bits to the list of all original encoded Inputs.
                encodedInputs.Add(inpSdr);

                //Calling JaccardSimilarityofBinaryArrays function to measure the similarities
                var similarity = MathHelpers.JaccardSimilarityofBinaryArrays(inpSdr, normalizePermanenceList.ToArray());
                double[] similarityArray = new double[] { similarity };
                // Add the Similarity Arrays to the list.
                similarityList.Add(similarityArray);

            }
            // Generate 1D heatmaps using the heatmap data and the normalized permanences To plot Heatmap, Encoded Inputs and Normalize Image combined.
            Generate1DHeatmaps(heatmapData, normalizedPermanence, encodedInputs);
            // Plotting Graphs to Visualize Smililarities of Encoded Inputs and Reconstructed Inputs
            DrawSimilarityPlots(similarityList);
        }

        /// <summary>
        /// Generates 1D heatmaps based on the provided heatmap data and normalized permanence values (Combined Image), and saves them to local Drive.
        /// </summary>
        /// <param name="heatmapData">A list containing the heatmap data for each input value.</param>
        /// <param name="normalizedPermanence">A list containing the normalized permanence values for each input value.</param>
        private void Generate1DHeatmaps(List<List<double>> heatmapData, List<int[]> normalizedPermanence, List<int[]> encodedInputs)
        {
            int i = 1;

            foreach (var values in heatmapData)
            {
                // Define the folder path from current Directory
                string folderPath = Path.Combine(Environment.CurrentDirectory, "1DHeatMap");

                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Define the file path with the folder path
                string filePath = Path.Combine(folderPath, $"heatmap_{i}.png");
                //Debugging the Filepath
                Debug.WriteLine($"FilePath: {filePath}");

                // Convert the Values to a 1D array using ToArray
                double[] array1D = values.ToArray();

                // Call the Draw1DHeatmap function with the dynamically generated file path
                NeoCortexUtils.Draw1dHeatmap(new List<double[]>() { array1D }, new List<int[]>() { normalizedPermanence[i - 1] }, new List<int[]>() { encodedInputs[i - 1] }, filePath, 200, 12, 9, 4, 0, 30);

                //Debugging the Message
                Debug.WriteLine("Heatmap generated and saved successfully.");
                i++;
            }
        }

        /// <summary>
        /// Draws a combined similarity plot based on the list of similarity arrays.
        /// </summary>
        /// <param name="similaritiesList">The list of arrays containing similarity values to be combined and plotted.</param>
        /// <remarks>
        /// The method combines all similarity values from the list of arrays and generates a plot representing the combined data.
        /// It creates a folder named "SimilarityPlots" in the current directory if it doesn't exist and saves the plot as a PNG image file named "combined_similarity_plot.png" within this folder.
        /// Debugging information, including the generated file path and successful plot generation confirmation, is output using Debug.WriteLine.
        /// </remarks>

        public static void DrawSimilarityPlots(List<double[]> similaritiesList)
        {
            // Combine all similarities from the list of arrays
            List<double> combinedSimilarities = new List<double>();
            foreach (var similarities in similaritiesList)
            {
                combinedSimilarities.AddRange(similarities);
            }

            // Define the folder path based on the current directory
            string folderPath = Path.Combine(Environment.CurrentDirectory, "SimilarityPlots");

            // Create the folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Define the file name
            string fileName = "combined_similarity_plot.png";

            // Define the file path with the folder path and file name
            string filePath = Path.Combine(folderPath, fileName);

            // Draw the combined similarity plot
            NeoCortexUtils.DrawCombinedSimilarityPlot(combinedSimilarities, filePath, 4500, 1100);
            //Debugging the Filepath
            Debug.WriteLine($"FilePath: {filePath}");

            Debug.WriteLine($"Combined similarity plot generated and saved successfully.");
        }
    }
}
