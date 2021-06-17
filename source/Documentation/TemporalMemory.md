# Temporal Memory

Temporal memory is an algorithm which learns sequences of Sparse Distributed Representations (SDRs) formed by the Spatial Pooling algorithm, and makes predictions of what the next input SDR will be. ([reference](https://numenta.com/resources/biological-and-machine-intelligence/temporal-memory-algorithm/))

Temporal memory alogirithm's input is the active columns output of Spatial Pooler algorithm.

```cs
public void TemporalMemoryInit()
{
    HtmConfig htmConfig = Connections.GetHtmConfigDefaultParameters();
    Connections connections = new Connections(htmConfig);

    TemporalMemory temporalMemory = new TemporalMemory();

    temporalMemory.Init(connections);
}
```

To apply the algorithm, method `Compute()` is invoked. The result will then be stored as `ComputeCycle` object.

```cs
public ComputeCycle Compute(int[] activeColumns, bool learn)
{
    ...
}
```

Following is an example illustrates how to use `TemporalMemory` algorithm.

```cs
public void TestBurstUnpredictedColumns1()
{
    HtmConfig htmConfig = GetDefaultTMParameters();
    Connections cn = new Connections(htmConfig);

    TemporalMemory tm = new TemporalMemory();

    tm.Init(cn);

    int[] activeColumns = { 0 };
    ISet<Cell> burstingCells = cn.GetCellSet(new int[] { 0, 1, 2, 3 });

    ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

    Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
}
```

Further unit tests can be found here: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/UnitTestsProject/TemporalMemoryTests.cs
