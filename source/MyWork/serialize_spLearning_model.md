Spatial Pooler Learning Analysis

I. Train method

1. The method output is CortexLayer instance, inputs are Maxval for the encoder and the input set for training. 

    ```
		public CortexLayer<object, object> Train(double max, List<double> inputValues)
    ```

2. Create new instance of htm
    ```
        // Pre-defined parameters for htm configuration
		    double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;
		    int inputBits = 200;
		    int numColumns = 2048;
   
        // htm configuration parameters
    
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
                StimulusThreshold = 10,
            };

        // Create htm memory
            var mem = new Connections(cfg);
    ```

3. Create new instance of Encoder class

    ```       
        // Dictionary defines typical encoder parameters

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
    
        // Create new instance of ScalarEncoder

            EncoderBase encoder = new ScalarEncoder(settings);
    ```

4. Homeostatic Plasticity Controller 
    
    - HPC extends the default Spatial Pooler algorithm.
    - HPC is to set the Spatial Pooler is in the new-born stage at the begining of the training. 
    - In this stage, the boosting is very active, but the SP behaves instable. After this stage, the hpc will control the learning process of the SP.
    - Once the SDR generated for every input gets stable, the HPC will fire event that notifies your program.
         
    ```  
        // the Sp is first set to unstable 
            
            bool isInStableState = false;
     
        // Create new instance of HomeostaticPlasticityController
            
            hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
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
    ```

5. Create instance of Spatial Pooler Multithread version

    ```    
        SpatialPooler sp = new SpatialPooler(hpa);

        // Initializes the SP instance 

        sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });
     ```

6. Create instance of NeoCortexLayer 

     ``` 
        // All the algorithms will be performed within this layer

        CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

        // Add encoder as the very first module. This model is connected to the sensory input cells that receives the input
        // The encoder will receive the input and forward the encoded input to the next module.

        cortexLayer.HtmModules.Add("encoder", encoder);

        // The next module in the layer is Spatial Pooler. This module will receive the encoded signal which is the output of encoder module.

        cortexLayer.HtmModules.Add("sp", sp);
     ```

7. Convert input values from list to array

    ```
        double[] inputs = inputValues.ToArray();
    ```

8. SDR inputs and SDR similarities

    ```   
        // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

        // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();
        
        // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0); 
                prevActiveCols.Add(input, new int[0]);
            }
    ```

9. Learning process

    - At the end, the NeocortexLayer model is returned.

    ```
        // Learning process will take 1000 iterations (cycles)

            int maxSPLearningCycles = 1000;
    
        // SP learning cycles

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
                    break;
            }

            return cortexLayer;
        }
    ```