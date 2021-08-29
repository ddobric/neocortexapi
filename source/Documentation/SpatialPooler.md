# Spatial Pooler

The HTM spatial pooler represents a neurally inspired algorithm for learning sparse representations from noisy data streams in an online fashion. ([reference](https://www.frontiersin.org/articles/10.3389/fncom.2017.00111/full))

Right now, three versions of SP are implemented and considered:

- Spatial Pooler single threaded original version without algorithm specific changes.
- SP-MT multithreaded version, which supports multiple cores on a single machine and
- SP-Parallel, which supports multicore and multimode calculus of spatial pooler.

Spatial Pooler algorithm requires 2 steps.

1. Parameters configuration

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
           NumActiveColumnsPerInhArea = 10.0,
           StimulusThreshold = 0.0,
           SynPermInactiveDec = 0.008,
           SynPermActiveInc = 0.05,
           SynPermConnected = 0.10,
           MinPctOverlapDutyCycles = 0.001,
           MinPctActiveDutyCycles = 0.001,
           DutyCyclePeriod = 1000,
           MaxBoost = 10.0,
           RandomGenSeed = 42,
           Random = new ThreadSafeRandom(42)
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
       parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
       parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
       parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
       parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
       parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
       parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
       parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
       parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
       parameters.Set(KEY.MAX_BOOST, 10.0);
       parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));

       Connnections mem = new Connections();
       parameters.apply(mem);

       SpatialPooler sp = new SpatialPooler();
       sp.Init(mem);
   }

   ```

2. Invocation of `Compute()`

   ```csharp
   public void TestSpatialPoolerCompute()
   {
       // parameters configuration
       ...

       // Invoke Compute()
       int[] outputArray = sp.Compute(inputArray, learn: true);
   }
   ```

3. How SP learn
 Following is an example illustrates how to use `SpatialPooler` algorithm.

```csharp
public void testCompute1_1()
{
    HtmConfig htmConfig = SetupHtmConfigParameters();
    htmConfig.InputDimensions = new int[] { 9 };
    htmConfig.ColumnDimensions = new int[] { 5 };
    htmConfig.PotentialRadius = 5;

    // This is 0.3 in Python version due to use of dense
    // permanence instead of sparse (as it should be)
    htmConfig.PotentialPct = 0.5;

    htmConfig.GlobalInhibition = false;
    htmConfig.LocalAreaDensity = -1.0;
    htmConfig.NumActiveColumnsPerInhArea = 3;
    htmConfig.StimulusThreshold = 1;
    htmConfig.SynPermInactiveDec = 0.01;
    htmConfig.SynPermActiveInc = 0.1;
    htmConfig.MinPctOverlapDutyCycles = 0.1;
    htmConfig.MinPctActiveDutyCycles = 0.1;
    htmConfig.DutyCyclePeriod = 10;
    htmConfig.MaxBoost = 10;
    htmConfig.SynPermTrimThreshold = 0;

    // This is 0.5 in Python version due to use of dense
    // permanence instead of sparse (as it should be)
    htmConfig.PotentialPct = 1;

    htmConfig.SynPermConnected = 0.1;

    mem = new Connections(htmConfig);

    SpatialPoolerMock mock = new SpatialPoolerMock(new int[] { 0, 1, 2, 3, 4 });
    mock.Init(mem);

    int[] inputVector = new int[] { 1, 0, 1, 0, 1, 0, 0, 1, 1 }; // output of encoder
    int[] activeArray = new int[] { 0, 0, 0, 0, 0 };
    for (int i = 0; i < 20; i++)
    {
        mock.compute(inputVector, activeArray, true);
    }

    for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
    {
        int[] permanences = ArrayUtils.ToIntArray(mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs));

        Assert.IsTrue(inputVector.SequenceEqual(permanences));
    }
}
```
Further unit tests can be found [here](../UnitTestsProject/SpatialPoolerTests.cs)