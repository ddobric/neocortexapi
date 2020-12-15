# Getting started

## HTM

The Hierarchical Temporal Memory Cortical Learning Algorithm (HTM CLA) is a theory and machine learning technology that aims to capture cortical algorithm of the neocortex.

HTM consists of 2 different components: Spatial Pooler and Temporal Memory. The concept of HTM is illustrated in the following image. Inside the algorithms, there are multiple mini columns act as synapses in our brain. These columns will be activated or deactivated depend on the input that is given. This is similar to the synapse activity. HTM, like many other machine learning algorithm, only deals with number. Therefore, it requires an encoder to transform the real world concept into digitized world of '0's and '1's.

<p align="center">
    <img src="./images/ColumnsWithSPAndTM.png" width=50%>
</p>

<!-- ![](./images/ColumnsWithSPAndTM.png) -->

First, input is given as a list.

```cs
            int inputBits = 100;

            double max = 20;
            int numColumns = 2048;

            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 });
            int numInputs = inputValues.Distinct().ToList().Count;

            var inputs = inputValues.ToArray();
```

## Encoder

Encoder is chosen according to the type of the inputs. There are some encoder available for popular input type:

- Scalar Encoder
- Datetime Encoder
- Boolean Encoder
- Category Encoder
- Geo-Spatial Encoder

In this example, ScalarEncoder is preferred as inputs are all numbers. The encoder is instantiated with predefined settings. The inputs will be encoded as series of '0's and '1's so that the spatial pooler will understand and proceed with its own computation.

```cs
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
```

See more: [Encoder](./tutorial.md#encoder)

## Spatial Pooler

Encoder produces output to be fed into Spatial Pooler algorithm. Type of Spatial Pooler (SP) that is used in this example is the multithreaded version that utilize multicore of the machine to run the spatial pooler algorithm.

```cs
            HtmConfig htmConfig = new HtmConfig
            {
                Random = new ThreadSafeRandom(42),
                InputDimensions = new int[] { inputBits },
                ColumnDimensions = new int[] { numColumns },
                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = 50,
                InhibitionRadius = 15,
                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxNewSynapseCount = (int)(0.02 * numColumns),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            Connections memory = new Connections(htmConfig);
```

Further explaination of parameters configuration can be found in [Readme.md](../../Readme.md)

The `HomeostaticPlasticityController` is included in the spatial pooler algorithm to implement the "new born" effect. This effect tracks the learning process of the SP and switches-off the boosting mechanism (new-born effect) after the SP has entered a stable state for all seen input patterns.

```cs
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(memory, numInputs * 55,
            (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
            }, numOfCyclesToWaitOnChange: 25);
```

Initialization of Spatial Pooler

```cs
            SpatialPoolerMT spatialPooler = new SpatialPoolerMT(hpa);
            spatialPooler.Init(memory, UnitTestHelpers.GetMemory());
```

An example SDR produced by Spatial Pooler algorithm is presented in the following image. The left side shows an MNIST image in binary format and the right side illustrates the SDR output from that image through the Spatial Pooler. In the SDR output, the yellow dots represent the active columns 

<img src="./images/SDRExample.png" height="300" align="center">

See more: [Spatial Pooler](./tutorial.md#spatial-pooler)  

**SDR result**

## Temporal Memory

The output of Spatial Pooler (SDR) is used as the input of Temporal Memory.

Initialization of Temporal Memory.

```cs
            TemporalMemory temporalMemory = new TemporalMemory();
            temporalMemory.Init(memory);
```

See more: [Temporal Memory](./tutorial.md#temporal-memory)

## Combine components

After initializing components, they are chained together in `CortexLayer` as HtmModule pipelines in the following order:

Input &#8594; Encoder &#8594; Spatial Pooler &#8594; Temporal Memory &#8594; Output (`ComputeCycle`)

```cs
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", spatialPooler);
            layer1.HtmModules.Add("tm", temporalMemory);

            bool learn = true;

            int maxCycles = 3500;

            for (int i = 0; i < maxCycles; i++)
            {
                foreach (var input in inputs)
                {
                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;
                }
            }
```
