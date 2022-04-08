# Cortex

The human cerebral cortex is divided into multiple layers, according to neuroscience. NeoCortexApi, a model based on biology, also introduces CortexLayer, which works as one layer of the cortex. The layer is made up of multiple IHtmModules that are implemented by Encoder, Spatial Pooler, and Temporal Memory. The addition of this component makes the code more presentable and opens up new possibilities for combining many layers together for more complicated applications. The layer combination is implemented using CortexRegion. When all cortical areas are combined, we have a CortexNetwork that resembles the whole cerebral cortex of the human brain.

The Cortex Layers is a pipeline that combines all the components of HTM which are encoder, spatial pooler and temporal memory. All the components in the pipeline are chained together in the added sequence and run one after another. This provide an intuitive way to run the HTM algorithm. Also, it helps to separate the preparation region and execution region.

The following code snippet illustrates the usage of `CortexNetwork`, `CortexRegion` and `CortexLayer`.

```cs
CortexNetwork net = new CortexNetwork("my cortex");

CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

// the modules are all initialized and configured
EncoderBase encoder = new ScalarEncoder(settings);
SpatialPooler sp = new SpatialPoolerMT(hpa);
TemporalMemory tm = new TemporalMemory();

layer1.AddModule("encoder", encoder);
layer1.AddModule("sp", sp);
layer1.AddModule("tm", tm);

region0.AddLayer(layer1);
net.AddRegion(region0);

```

`CortexLayer` also allows to add modules by chaining the method `AddModule()`.

```cs
CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

layer1.AddModule("encoder", encoder)
    .AddModule("spatialPooler", spatialPooler)
    .AddModule("temporalMemory", temporalMemory);
```

It is simple to run the Network, Region, or Layer by using the `Compute` method on the component. Specifically, the `CortexLayer` provides a method to get the result of each IHtmModule using `GetResult(string moduleName)`, allowing freedom in utilizing the component while being presentable.

```cs
layer1.Compute(input, learn: true);
int[] spatialPoolerResult = layer1.GetResult("sp") as int[];
```

