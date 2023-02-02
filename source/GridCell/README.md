# Grid Cell

## Folder Structure
1. encoder
    - Contains **GridCellEncoder.cs** that produces SDR for a given location position (x and y value).
2. GridCellModelTwistedTorous
   - Contains two different Gridcell hexagonal pattern simulation using the twisted torous module. 
     - One implementation depends on NumSharp library (a minimal C# implementation of python numpy).
     - An alternate implementation without any external libray dependence (Just pure C#).
3. hexagonal
   - It contains number of helper classes that converts a given circle to hexagonal and vice versa. This classes are used by the GridCellEncoder.cs
5. docs
   - Contains documentation materials related to Grid Cell


## How to run
The sample class on how to use the encoder is located at

[GridCellSamples.cs](../Samples/NeoCortexApiSample/GridCellSamples.cs)

Use NeoCortexApiSample project to call the run method of GridCellSamples.cs class
