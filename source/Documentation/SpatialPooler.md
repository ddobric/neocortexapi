# Spatial Pooler

Spatial Pooler (SP) is a learning algorithm that is designed to replicate the neurons functionality of human brain. Essentially, if a brain sees one thing multiple times, it is going to strengthen the synapses that react to the specific input result in the recognition of the object. Similarly, if several similar SDRs are presented to the SP algorithm, it will reinforce the columns that are active according to the on bits in the SDRs. If the number of training iterations is big enough, the SP will be able to identify the objects by producing different set of active columns within the specified size of SDR for different objects.

The HTM spatial pooler represents a neurally inspired algorithm for learning sparse representations from noisy data streams in an online fashion. ([reference](https://www.frontiersin.org/articles/10.3389/fncom.2017.00111/full))

Right now, three versions of SP are implemented and considered:

- Spatial Pooler single threaded original version without algorithm specific changes.
- SP-MT multithreaded version, which supports multiple cores on a single machine and
- SP-Parallel, which supports multicore and multimode calculus of spatial pooler.

Spatial Pooler algorithm requires 2 steps.

## 1. Parameters configuration

   There are 2 ways to configure Spatial Pooler's parameters.

   1.1. Using `HtmConfig` (**Preferred** way to intialize `SpatialPooler` )

   ```csharp
   public void SpatialPoolerInit()
   {
       HtmConfig htmConfig = new HtmConfig()
       {
           InputDimensions = new int[] { 32, 32 },
           ColumnDimensions = new int[] { 64, 64 },
           PotentialRadius = 16,
           PotentialPct = 0.5,
           GlobalInhibition = false,
           LocalAreaDensity = -1.0,

           // other parameters
       };

       Connections connections = new Connections(htmConfig);

       SpatialPooler spatialPooler = new SpatialPoolerMT();
       spatialPooler.Init(connections);
   }
   ```

   1.2. Using `Parameters` (**Obsoleted** - this is for supporting the compatibility with the Java implementation)

   ```csharp
   public void setupParameters()
   {
       Parameters parameters = Parameters.getAllDefaultParameters();
       parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { 5 });
       parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { 5 });
       parameters.Set(KEY.POTENTIAL_RADIUS, 5);
       parameters.Set(KEY.POTENTIAL_PCT, 0.5);
       parameters.Set(KEY.GLOBAL_INHIBITION, false);
       parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);

       // other parameters

       Connnections mem = new Connections();
       parameters.apply(mem);

       SpatialPooler sp = new SpatialPooler();
       sp.Init(mem);
   }

   ```

### Parameter desription

| Parameter Name                  | Meaning                                                                                                                                                                                                                                                                                                          |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| POTENTIAL_RADIUS                | Defines the radius in number of input cells visible to column cells. It is important to choose this value, so every input neuron is connected to at least a single column. For example, if the input has 50000 bits and the column topology is 500, then you must choose some value larger than 50000/500 > 100. |
| POTENTIAL_PCT                   | Defines the percent of inputs withing potential radius, which can/should be connected to the column.                                                                                                                                                                                                             |
| GLOBAL_INHIBITION               | If TRUE global inhibition algorithm will be used. If FALSE local inhibition algorithm will be used.                                                                                                                                                                                                              |
| INHIBITION_RADIUS               | Defines neighbourhood radius of a column.                                                                                                                                                                                                                                                                        |
| LOCAL_AREA_DENSITY              | Density of active columns inside of local inhibition radius. If set on value < 0, explicit number of active columns (NUM_ACTIVE_COLUMNS_PER_INH_AREA) will be used.                                                                                                                                              |
| NUM_ACTIVE_COLUMNS_PER_INH_AREA | An alternate way to control the density of the active columns. If this value is specified then LOCAL_AREA_DENSITY must be less than 0, and vice versa.                                                                                                                                                           |
| STIMULUS_THRESHOLD              | One mini-column is active if its overlap exceeds overlap threshold  of connected synapses.                                                                                                                                                                                                                       |
| SYN_PERM_INACTIVE_DEC           | Decrement step of synapse permanence value withing every inactive cycle. It defines how fast the NeoCortex will forget learned patterns.                                                                                                                                                                         |
| SYN_PERM_ACTIVE_INC             | Increment step of connected synapse during learning process                                                                                                                                                                                                                                                      |
| SYN_PERM_CONNECTED              | Defines Connected Permanence Threshold  , which is a float value, which must be exceeded to declare synapse as connected.                                                                                                                                                                                        |
| DUTY_CYCLE_PERIOD               | Number of iterations. The period used to calculate duty cycles. Higher values make it take longer to respond to changes in boost. Shorter values make it more unstable and likely to oscillate.                                                                                                                  |
| MAX_BOOST                       | Maximum boost factor of a column.                                                                                                                                                                                                                                                                                |

## 2. Invocation of `Compute()`

   ```csharp
   public void TestSpatialPoolerCompute()
   {
       // parameters configuration
       ...

       // Invoke Compute()
       int[] outputArray = sp.Compute(inputArray, learn: true);
   }
   ```
The result of this method is the short form of the output SDR from the Spatial Pooler. The algorithm starts from an SDR with random distributed connections from each column to the input space. Each connection between an input bit and an output column is assigned a random permanence value between 0 and 1. The input bits connected to the given column are activated if their permanence values are greater than the connected permanence threshold θ<sub>p</sub> defined in the parameter initialization.  Each Output column will be active if a certain number of input bits connected to that column, are activated in the input space. With this connections, Spatial Pooler will be able to represent different inputs as SDRs but still be able to maintain the semantic information of the input. The similarity of information is measured by the overlap score of the output columns. The more overlap the two outputs have, the more similar they are. 

Only some columns that have the high overlap score will be chosen to be active and will be allowed to learn, other columns remain unchanged. The active columns will have its connections, which overlap the input, reinforced by increment the synaptic permanence value. Meanwhile, other connections which does not matched the input will have their permanence value diminished.

In practice, several inputs are presented to the Spatial Pooler with learning over a large number of iterations, which may be accomplished with the following code:
```cs
int maxSPLearningCycles = 1000;

for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
{
    //
    // This trains the layer on input pattern.
    foreach (var input in inputs)
    {
        // Learn the encoded SDR pattern.
        int[] activeColumns = sp.Compute(input, true) as int[];

        var actCols = activeColumns.OrderBy(c => c).ToArray();

        // Store the input/output in a dictionary and calculate the similarity with other output of the same input later on.
    }
}


```

## Boosting and Homeostatic plasticity controller

Normally, Spatial Pooler will have a limited number of active columns represent different inputs, or their active duty-cycles is close to 1. Other dormant columns will never be active during the whole learning phase. This mean the output SDR can describe less information about the set of input. Boosting mechanism will allow more columns to participate in expressing the input space.

Spatial Pooler’s boosting mechanism allows all columns to be consistently used throughout all patterns [6]. However, even if the columns have already learned a patterns, the boosting mechanism is still active and encourage other inactive columns to express themselves and cause the Spatial Pooler to forget the input. To overcome this issue, the new homeostatic plasticity controller is introduced to the Spatial Pooler to switch off boosting after the learning enter the stable state. The research shown that the output SDRs of the Spatial Pooler remain unchanged during its lifespan.

The Spatial Pooler is extended with the following code:
```cs
HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
    (isStable, numPatterns, actColAvg, seenInputs) =>
    {
        // Event should only be fired when entering the stable state.
        // Ideal SP should never enter unstable state after stable state.
        if (isStable == false)
        {
            // INSTABLE STATE
            // This should usually not happen.
            isInStableState = false;
        }
        else
        {
            // STABLE STATE
            // Here you can perform any action if required.
            isInStableState = true;
        }
    });
SpatialPooler sp = new SpatialPooler(hpa);

```

3. How SP learn
   Following is an example illustrates how to use `SpatialPooler` algorithm with a simple unit test.

```csharp
public void testZeroOverlap_NoStimulusThreshold_LocalInhibition()
{
    int inputSize = 10;
    int nColumns = 20;
    
    HtmConfig htmConfig = new HtmConfig(new int[] { inputSize }, new int[] { nColumns })
    {
        PotentialRadius = 5,
        GlobalInhibition = false,
        NumActiveColumnsPerInhArea = 1.0,
        StimulusThreshold = 0.0,
        Random = new ThreadSafeRandom(42),
        RandomGenSeed = 42
    };

    SpatialPooler sp = new SpatialPooler();
    Connections cn = new Connections(htmConfig);
    sp.Init(cn);

    // This exact number of active columns is determined by the inhibition
    // radius, which changes based on the random synapses (i.e. weird math).
    // Force it to a known number.
    cn.HtmConfig.InhibitionRadius = 2;

    int[] activeArray = new int[nColumns];
    sp.compute(new int[inputSize], activeArray, true);

    Assert.IsTrue(6 == activeArray.Count(i => i > 0));//, ArrayUtils.INT_GREATER_THAN_0).length);
}

```

Further unit tests can be found [here](../UnitTestsProject/SpatialPoolerTests.cs)
