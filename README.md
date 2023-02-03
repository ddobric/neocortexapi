[![license](https://img.shields.io/github/license/mashape/apistatus.svg?maxAge=2592000)](https://github.com/ddobric/htmdotnet/blob/master/LICENSE)
[![buildStatus](https://github.com/ddobric/neocortexapi/workflows/.NET%20Core/badge.svg)](https://github.com/ddobric/neocortexapi/actions?query=workflow%3A%22.NET+Core%22)

# Introduction
This repository is the open source implementation of the Hierarchical Temporal Memory in C#/.NET Core. This repository contains set of libraries around **NeoCortext** API .NET Core library. **NeoCortex** API focuses implementation of _Hierarchical Temporal Memory Cortical Learning Algorithm_. Current version is first implementation of this algorithm on .NET platform. It includes the **Spatial Pooler**, **Temporal Pooler**, various encoders and **CorticalNetwork**  algorithms. Implementation of this library aligns to existing Python and JAVA implementation of HTM. Due similarities between JAVA and C#, current API of SpatialPooler in C# is very similar to JAVA API. However the implementation of future versions will include some API changes to API style, which is additionally more aligned to C# community.
This repository also cotains first experimental implementation of distributed highly scalable HTM CLA based on Actor Programming Model.
The code published here is experimental code implemented during my research at daenet and Frankfurt University of Applied Sciences. 

## Getting started
To get started, please see <a href="https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/gettingStarted.md">this document.</a>

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

Dobric, Pech, Ghita, Wennekers 2020. 2020 International Journal of Artificial Intelligence and Applications. Scaling the HTM Spatial Pooler. doi:10.5121/ijaia .2020.11407

#### AIS 2020 - 6th International Conference on Artificial Intelligence and Soft Computing (AIS 2020), Helsinki
The Parallel HTM Spatial Pooler with Actor Model

Dobric, Pech, Ghita, Wennekers 2020. 2020 AIS 2020 - 6th International Conference on Artificial Intelligence and Soft Computing, Helsinki. The Parallel HTM Spatial Pooler with Actor Model. https://aircconline.com/csit/csit1006.pdf, doi:10.5121/csit.2020.100606

#### Symposium on Pattern Recognition and Applications - Rome, Italy
On the Relationship Between Input Sparsity and Noise Robustness in Hierarchical Temporal Memory Spatial Pooler 

Dobric, Pech, Ghita, Wennekers 2020. 2020 Symposium on Pattern Recognition and Applications. On the Relationship Between Input Sparsity and Noise Robustness in Hierarchical Temporal Memory Spatial Pooler. https://dl.acm.org/doi/10.1145/3393822.3432317. doi:10.1145/3393822.3432317

#### International Conference on Pattern Recognition Applications and Methods - ICPRAM 2021
Improved HTM Spatial Pooler with Homeostatic Plasticity Control (Awarded with: *Best Industrial Paper*)

Dobric, Pech, Ghita, Wennekers 2021. ICPRAM Vienna Improved HTM Spatial Pooler with Homeostatic Plasticity control. doi:10.5220/0010314200980106

#### Springer Nature - Computer Sciences
On the Importance of the Newborn Stage When Learning Patterns with the Spatial Pooler

Dobric, Pech, Ghita, Wennekers 2022. Springer Nature Computer Science Journal
On the Importance of the Newborn Stage When Learning Patterns with the Spatial Pooler. https://rdcu.be/cIcoc. doi:10.1007/s42979-022-01066-4

# Contribute
If your want to contribute on this project please contact us by opening an issue. 


