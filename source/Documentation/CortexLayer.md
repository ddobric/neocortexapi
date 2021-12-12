# Cortex Layer

The Cortex Layers is a pipeline that combines all the components of HTM which are encoder, spatial pooler and temporal memory. 
All the components in the pipeline are chained together in the added sequence and run one after another. This provide an intuitive way to run the HTM algorithm. Also, it helps to separate the preparation region and execution region.
The following code snippet illustrates the usage of `CortexLayer`.

```cs
CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

layer1.AddModule("encoder", encoder);
layer1.AddModule("spatialPooler", spatialPooler);
layer1.AddModule("temporalMemory", temporalMemory);

layer1.Compute(input, learn: true);
```

`CortexLayer` also allows to add modules by chaining the method `AddModule()`.

```
cs
CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");

layer1.AddModule("encoder", encoder)
    .AddModule("spatialPooler", spatialPooler)
    .AddModule("temporalMemory", temporalMemory);

layer1.Compute(input, learn: true);
```

The initialization of all components will be provided at the begining and then added to the `CortexLayer`. Then, 

