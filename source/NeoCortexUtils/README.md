# What is SDR?

The neocortex which is the seat of intelligent thought in the mammalian brain. High level vision, hearing, touch, movement, language, and planning are all performed by the neocortex. The activity of the neurons in the neocortex is sparse, meaning only a small percentage of neurons are spiking at any point in time. The sparsity might vary from less than one percent to several percent but is always sparse.

Hierarchical Temporal Memory (HTM) is a machine learning technology that aims to capture the structural and algorithmic properties of the neocortex. HTM systems require data in the form of Sparse Distributed Representations. “Sparse” means that only a small percentage of neurons are active at one time. “Distributed” means that the activations of many neurons are required in order to represent something. A single active neuron conveys some meaning but it must be interpreted within the context of a population of neurons to convey the full meaning.

Sparse Distributed Representations (SDRs) are binary representation i.e. an array consisting of large number of bits where small percentage are 1’s represents an active neuron and 0 an inactive one. Each bit typically has some meaning (such as the presence of an edge at a particular location and orientation). This means that if two vectors have 1s in the same position they are semantically similar in that attribute.

The first step of using an HTM system is to convert a data source into an SDR using an encoder. The encoder converts the native format of the data into an SDR that can be fed into an HTM system. The encoder is responsible for determining which output bits should be ones, and which should be zeros, for a given input value in such a way as to capture the important semantic characteristics of the data. Similar input values should produce highly overlapping SDRs.

## Table of content
1. [Create SDR Using Spatial Pooler](docs/create-sdr-using-spatial-pooler.md)
2. [Implement SDR representation samples](docs/implement-sdr-representation-samples.md)
3. [SDR Representation Using Encoders](docs/sdr-representation-using-encoders.md)
4. [Bitmap Representation of SDR](docs/bitmap-representation-of-sdr.md)
5. [SDR representation of Text](docs/sdr-representation-of-text.md)
6. [TraceColumnOverlap Method](docs/trace-column-overlap-method.md)
7. [Column Overlap representation through graph](docs/column-overlap-representation-graph.md)