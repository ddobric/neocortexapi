[![license](https://img.shields.io/github/license/mashape/apistatus.svg?maxAge=2592000)](https://github.com/ddobric/htmdotnet/blob/master/LICENSE)
[![buildStatus](https://github.com/ddobric/neocortexapi/workflows/.NET%20Core/badge.svg)](https://github.com/ddobric/neocortexapi/actions?query=workflow%3A%22.NET+Core%22)

# Introduction
This repository is the open source implementation of the Hierarchical Temporal Memory in C#/.NET Core. This repository contains set of libraries around **NeoCortext** API .NET Core library. **NeoCortex** API focuses implementation of _Hierarchical Temporal Memory Cortical Learning Algorithm_. Current version is first implementation of this algorithm on .NET platform. It includes the **Spatial Pooler**, **Temporal Pooler**, various encoders and **CorticalNetwork**  algorithms. Implementation of this library aligns to existing Python and JAVA implementation of HTM. Due similarities between JAVA and C#, current API of SpatialPooler in C# is very similar to JAVA API. However the implementation of future versions will include some API changes to API style, which is additionally more aligned to C# community.
This repository also cotains first experimental implementation of distributed highly scalable HTM CLA based on Actor Programming Model.
The code published here is experimental code implemented during my research at daenet and Frankfurt University of Applied Sciences. 

## Getting started
NeoCortexApi is today built as .NET Standard 2.2 library. It contains all artefacts required to run **SpatialPooler**, **TemoralPooler** and few encoders.
First important step in using of any algorithm is initialization of various parameters, which are defined by class `Parameters`. Following method shows how to setup parameters:
 
```csharp
private static Parameters GetDefaultParams()
        {
            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 80.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);
          
            return parameters;
        }
```
Parameters can also be set by using of property setters as typically used in JAVA.

```
            parameters.setPotentialPct(0.5);
            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(3);
            parameters.setStimulusThreshold(1);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.1);
            parameters.setMinPctActiveDutyCycles(0.1);
            parameters.setDutyCyclePeriod(10);
            parameters.setMaxBoost(10);
            parameters.setSynPermTrimThreshold(0);
```
In the future version of **NeoCortexApi** this will be changed by using properties as commonly used in C#.

Chosing right parameters for experiments is very complex task and it will not be further described in this guide. For example, following method will set  **SpatilaPooler** to use local inhibition with very long period for boosting (DUTY_CYCLE_PERIOD) to ensure stable representation. It will use 10 columns as potential radius (POTENTIAL_RADIUS), which means that every column (cell, which represents column) will see 10 input cells. This is called column receptive field.
3/4 of columns in receptive field will be connected to column (POTENTIAL_PCT).
Moreover, **SpatilaPooler** will use 1/4 of columns as inhibition inside of  inhibition (INHIBITION_RADIUS), and 50% of columns inside of inhibition radius will be used as so called "winner columns". This set of parameter defines sparsity of algorithm

This code snippet shows how to set described parameters:

```
var parameters = GetDefaultParams();
parameters.Set(KEY.POTENTIAL_RADIUS, 10);
parameters.Set(KEY.POTENTIAL_PCT, 0.75);
parameters.Set(KEY.INHIBITION_RADIUS, (int)0.25 * imageSize[parameters.Set(KEY.LOCAL_AREA_DENSITY, 0.5);
parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, -1);
parameters.Set(KEY.DUTY_CYCLE_PERIOD, 1000000);
parameters.Set(KEY.MAX_BOOST, 5);
```

|Parameter Name  |  Meaning|
|--|--|
|POTENTIAL_RADIUS  | Defines the radius in number of input cells visible to column cells. It is important to choose this value, so every input neuron is connected to at least a single column. For example, if the input has 50000 bits and the column topology is 500, then you must choose some value larger than 50000/500 > 100.  |
|POTENTIAL_PCT  | Defines the percent of of inputs withing potential radius, which can/should be connected to the column. |
|GLOBAL_INHIBITION  | If TRUE global inhibition algorithm will be used. If FALSE local inhibition algorithm will be used. |
|GLOBAL_INHIBITION  | If TRUE global inhibition algorithm will be used. If FALSE local inhibition algorithm will be used. |
|INHIBITION_RADIUS  | Defines neighbourhood radius of a column. |
|LOCAL_AREA_DENSITY  | Density of active columns inside of local inhibition radius. If set on value < 0, explicit number of active columns (NUM_ACTIVE_COLUMNS_PER_INH_AREA) will be used.|
|NUM_ACTIVE_COLUMNS_PER_INH_AREA | An alternate way to control the density of the active columns. If this value is specified then LOCAL_AREA_DENSITY must be less than 0, and vice versa.
|STIMULUS_THRESHOLD| One mini-column is active if its overlap exceeds _overlap threshold_ **θo** of connected synapses.  |
|SYN_PERM_INACTIVE_DEC| Decrement step of synapse permanence value withing every inactive cycle. It defines how fats the NeoCortex will forget learned patterns.|
|SYN_PERM_ACTIVE_INC| Increment step of connected synapse during learning process  |
|SYN_PERM_CONNECTED| Defines _Connected Permanence Threshold_ **θp**, which is a float value, which must be exceeded to declare synapse as connected.  |
|DUTY_CYCLE_PERIOD| Number of iterations. The period used to calculate duty cycles. Higher values make it take longer to respond to changes in boost. Shorter values make it more unstable and likely to oscillate.  |
|MAX_BOOST| Maximum boost factor of a column.  |


Please note, for full list of parameters and their meaning are not a part of this document.

As next **SpatialPooler** instance hast to be created. Following code shows how to do this:
  
```csharp
// Creates an empty pooler.
SpatialPooler sp = new SpatialPooler();

// Creates the cell-space
Connections mem = new Connections();

// Initialize cell-space with set of required parameters.
parameters.apply(mem);

// Initializes the `SpatialPooler`.
sp.init(mem);
```

Before **SpatialPooler** can be used, it is required to create instance of _Connection_ class. This class builds internally cell-space and all required connections. For example, instance of this class will hold definition of a cortical-column with set of mini-columns and dendrite segments. It will also hold synaptic connections between cells. This instance is used as cortex memory inside of **SpatialPooler** (in later implementation TemporalPooler too).
To initialize this class with required parameters, following code is used:

`parameters.apply(mem);`

As next pooler has to be initialized:

`sp.init(mem);`

After initialization **SpatialPooler** can start learning patterns. Learning is done by invoking of method `compute`.

`public void compute(Connections c, int[] inputVector, int[] activeArray, bool learn)`

The method gets as first argument the instance of _Connection_ class. This is required, because algorithm operates on _Connection_ class. Remember,  this class holds all cells, mini-columns and synapses. 

Following method puts all together and demonstrates how to use **SpatialPooler**. It prepares sensory input of 32 cells and uses cortical-column with 128 mini-columns with inhibition of 2%. Note inhibition of 2% aligns to sparse encoding in neocortex. 

        
```csharp
public void CollSynapsesToInput()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32 });
            parameters.setColumnDimensions(new int[] { 128 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 128);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[128];

            int[] inputVector = Helpers.GetRandomVector(32, parameters.Get<Random>(KEY.RANDOM));

            for (int i = 0; i < 100; i++)
            {
                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str);
            }
        }

        private static Parameters GetDefaultParams()
        {
            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 80.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);
          
            return parameters;
        }
```

Following images show how **SpatialPooler** creates (encodes) Sparse Distributed Representation of MNIST images.

SDR code of digit '3' by using of local inhibition and various receptive field (radius)
![image.png](/.attachments/image-494af819-a46e-43ef-bf88-d39a2d8e8ca6.png)

Same example by using of global inhibition mechanism:
![image.png](/.attachments/image-6bb495b4-84a7-45dc-9199-37fc629b8e55.png)

Following example shows encoding of different representations of digit '1' by using same set of parameters shown in code snippet above.
![image.png](/.attachments/image-da7ddc5c-ff0a-493a-a0d7-54b765b0aaa1.png)

# References

HTM School:
https://www.youtube.com/playlist?list=PL3yXMgtrZmDqhsFQzwUC9V8MeeVOQ7eZ9&app=desktop

HTM Overview:
https://en.wikipedia.org/wiki/Hierarchical_temporal_memory

A Machine Learning Guide to HTM:
https://numenta.com/blog/2019/10/24/machine-learning-guide-to-htm

Numenta on Github:
https://github.com/numenta

HTM Community:
https://numenta.org/

A deep dive in HTM Temporal Memory algorithm:
https://numenta.com/assets/pdf/temporal-memory-algorithm/Temporal-Memory-Algorithm-Details.pdf

Continious Online Sequence Learning with HTM:
https://www.mitpressjournals.org/doi/full/10.1162/NECO_a_00893#.WMBBGBLytE6

# Papers and conference proceedings
#### International Journal of Artificial Intelligence and Applications
Scaling the HTM Spatial Pooler
https://aircconline.com/abstract/ijaia/v11n4/11420ijaia07.html

#### AIS 2020 - 6th International Conference on Artificial Intelligence and Soft Computing (AIS 2020), Helsinki
The Parallel HTM Spatial Pooler with Actor Model
https://aircconline.com/csit/csit1006.pdf

#### Symposium on Pattern Recognition and Applications - Rome, Italy
On the Relationship Between Input Sparsity and Noise Robustness in Hierarchical Temporal Memory Spatial Pooler 
https://doi.org/10.1145/3393822.3432317

#### International Conference on Pattern Recognition Applications and Methods - ICPRAM 2021

Improved HTM Spatial Pooler with Homeostatic Plasticity Control (Awarded with: *Best Industrial Paper*)
https://www.insticc.org/node/TechnicalProgram/icpram/2021/presentationDetails/103142

#### Springer Nature - Computer Sciences
On the Importance of the Newborn Stage When Learning Patterns with the Spatial Pooler
https://rdcu.be/cIcoc

# Contribute
If your want to contribute on this project please contact us. 


