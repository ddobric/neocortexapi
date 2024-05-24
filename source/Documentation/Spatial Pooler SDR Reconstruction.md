# ML 23/24-04 Implementation of Spatial Pooler SDR Reconstruction


[![N|Logo](https://ddobric.github.io/neocortexapi/images/logo-NeoCortexAPI.svg )](https://ddobric.github.io/neocortexapi/)


In this Documentation we will describe about the SDR Reconstruction.
#### Instruction for Running the Project
- Clone the Repository and Run
- You will get the project here
[NeoCortexApiSample](https://github.com/ddobric/neocortexapi/tree/master/source/Samples/NeoCortexApiSample)
#### Two Experiments
- **`SpatialPatternLearning.cs`**: Numerical Inputs 
[SpatialPatternLearning.cs](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs)
- **`ImageBinarizerSpatialPattern.cs`**: Image Inputs 
[ImageBinarizerSpatialPattern.cs](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/ImageBinarizerSpatialPattern.cs)
###### Image Input sets are already uploaded here
- neocortexapi_team.bji\source\Samples\NeoCortexApiSample\bin\Debug\net8.0\Sample\TestFiles
###### Simply Change the Running commands here 
- **`Program.cs`**: Goto Program.cs file of NeoCortexApiSample
- Change the codes here Click the Link below and it will Redirect you.
[Program.cs](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/Program.cs#L26-L28) for ImageBinarizerSpatialPattern
###### All the output will be saved here
- neocortexapi\source\Samples\NeoCortexApiSample\bin\Debug\net8.0
#### Running Unit Test Experiments
Go to this File and Run Unit-Test Project for **`SdrReconstructionTests.cs`**
[SdrReconstructionTests.cs](https://github.com/ddobric/neocortexapi/blob/master/source/UnitTestsProject/SdrReconstructionTests.cs)
## Introduction
Our project focuses on exploring the Reconstruct() function within HTM's "NeoCortexAPI" to unlock its full potential. This function, serving as the inverse of SP, lies at the core of our investigation. Titled "Visualization of Reconstructed Permanence Values," our project aims to demonstrate how HTM's Spatial Pooler utilizes Reconstruct() for input sequence rebuilding. By employing images as input, we strive to elucidate the relationship between input and output in HTM. Through our exploration, we aim to contribute to a clearer understanding and effective utilization of HTM technology.
# Methodology
Our methodology encompasses two distinct approaches: one involving encoded numerical values and the other employing images as input.

For the encoded numerical values, our process begins with the provision of numerical data ranging from 0 to 99. These numerical inputs undergo transformation via an encoder, converting them into int[] arrays. Each array, consisting of 200 bits post-encoding, represents a sequence of 0s and 1s. These encoded arrays exclusively serve as input for our experiment, enabling us to evaluate the Reconstruct() function within HTM's "NeoCortexAPI" for input sequence rebuilding.

On the other hand, for image input, our methodology involves extracting visual information from images and preprocessing them for compatibility with HTM. Each image undergoes transformation into a numerical format, typically represented as arrays of pixel values. These pixel arrays are then encoded to align with HTM's processing framework. The resulting encoded representations of the images serve as input for our experiment, allowing us to investigate the application of the Reconstruct() function within HTM's Spatial Pooler for reconstructing image sequences.

**Fig: Methodology Flowchart**
![Methodology Flowchart](https://github.com/ddobric/neocortexapi/assets/64829519/170ad580-900c-4e66-96b7-045e54165a92)


## Hierarchical Temporal Memory (HTM) Spatial Pooler

The encoded int[] arrays undergo transformation using the HTM Spatial Pooler, generating Sparse Distributed Representations (SDRs). This pivotal step lays the groundwork for further exploration.

## Reconstruct() Method:

Utilizing the Neocortexapi's Reconstruct() method, we meticulously reverse the transformation of the encoded int[] arrays. The reconstructed representations are shaped by permanence values obtained from the Reconstruction method.
``` csharp
 public Dictionary<int, double> Reconstruct(int[] activeMiniColumns)
 {
     if (activeMiniColumns == null)
     {
         throw new ArgumentNullException(nameof(activeMiniColumns));
     }

     var cols = connections.GetColumnList(activeMiniColumns);

     Dictionary<int, double> permancences = new Dictionary<int, double>();

    
     foreach (var col in cols)
     {
         col.ProximalDendrite.Synapses.ForEach(s =>
         {
             double currPerm = 0.0;

             
             if (permancences.TryGetValue(s.InputIndex, out currPerm))
             {
               
                 permancences[s.InputIndex] = s.Permanence + currPerm;
             }
             else
             {
              
                 permancences[s.InputIndex] = s.Permanence;
             }
         });
     }

     return permancences;
 }
```
[Reconstruction in SP](https://github.com/ddobric/neocortexapi/blob/master/source/NeoCortexApi/SpatialPooler.cs#L1442) 

#### Reconstruct() Workflow:
- **Input Validation:** Thorough validation checks, throwing an `ArgumentNullException` if the input array of active mini-columns is null.
   
- **Column Retrieval:** Retrieve the list of columns associated with the active mini-columns from the connections.
   
- **Reconstruction Process:** Iterate through each column, accessing the synapses in its proximal dendrite.
   
- **Permanence Accumulation:** For each synapse, accumulate the permanence values for each input index in the reconstructed input dictionary.
   
- **Dictionary Update:** Update the reconstructed input dictionary, considering whether the input index already exists or needs to be added as a new key-value pair.
   
- **Result Return:** The method concludes by returning the reconstructed input as a dictionary, mapping input indices to their associated permanences.

# Running Reconstruct Method for Numerical Inputs
```csharp
     private void RunRustructuringExperiment(SpatialPooler sp, EncoderBase encoder, List<double> inputValues)
 {
     // Initialize a list to get heatmap data for all input values.
     List<List<double>> heatmapData = new List<List<double>>();

     // Initialize a list to get normalized permanence values.
     List<int[]> normalizedPermanence = new List<int[]>();

     // Initialize a list to get normalized permanence values.
     List<int[]> encodedInputs = new List<int[]>();

     // Initialize a list to measure the similarities.
     List<double[]> similarityList = new List<double[]>();

     // Loop through each input value in the list of input values.
     foreach (var input in inputValues)
     {
         // Encode the current input value using the provided encoder, resulting in an SDR
         var inpSdr = encoder.Encode(input);

         // Compute the active columns in the spatial pooler for the given input SDR, without learning.
         var actCols = sp.Compute(inpSdr, false);

         // Reconstruct the permanence values for the active columns.
         Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actCols);

         // Define the maximum number of inputs (Same size of encoded Inputs) to consider.
         int maxInput = inpSdr.Length;

         // Initialize a dictionary to hold all permanence values, including those not reconstructed becuase of Inactive columns.
         Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();

         // Populate the all permanence dictionary with reconstructed permanence values.
         foreach (var kvp in reconstructedPermanence)
         {
             int inputIndex = kvp.Key;

             double probability = kvp.Value;

             allPermanenceDictionary[inputIndex] = probability;

         }
         // Ensure that all input indices up to the maximum are represented in the dictionary, even if their permanence is 0.
         for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
         {

             if (!reconstructedPermanence.ContainsKey(inputIndex))
             {

                 allPermanenceDictionary[inputIndex] = 0.0;
             }
         }
         // Sort the dictionary by keys
         var sortedAllPermanenceDictionary = allPermanenceDictionary.OrderBy(kvp => kvp.Key);

         // Convert the sorted dictionary of all permanences to a list
         List<double> permanenceValuesList = sortedAllPermanenceDictionary.Select(kvp => kvp.Value).ToList();

         heatmapData.Add(permanenceValuesList);

         // Output debug information showing the input value and its corresponding SDR as a string.
         Debug.WriteLine($"Input: {input} SDR: {Helpers.StringifyVector(actCols)}");

         // Define a threshold value for normalizing permanences, this value provides best Reconstructed Input
         var ThresholdValue = 8.3;

         // Normalize permanences (0 and 1) based on the threshold value and convert them to a list of integers.
         List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);

         // Add the normalized permanences to the list of all normalized permanences.
         normalizedPermanence.Add(normalizePermanenceList.ToArray());

         // Add the encoded bits to the list of all original encoded Inputs.
         encodedInputs.Add(inpSdr);

         //Calling JaccardSimilarityofBinaryArrays function to measure the similarities
         var similarity = MathHelpers.JaccardSimilarityofBinaryArrays(inpSdr, normalizePermanenceList.ToArray());
         double[] similarityArray = new double[] { similarity };
         // Add the Similarity Arrays to the list.
         similarityList.Add(similarityArray);

     }
     // Generate 1D heatmaps using the heatmap data and the normalized permanences To plot Heatmap, Encoded Inputs and Normalize Image combined.
     Generate1DHeatmaps(heatmapData, normalizedPermanence, encodedInputs);
     // Plotting Graphs to Visualize Smililarities of Encoded Inputs and Reconstructed Inputs
     DrawSimilarityPlots(similarityList);
 }

```
[Running Reconstruct Method For Numeric Data](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs#L242-L328)
# Running Reconstruct Method for Image Inputs
```csharp
    private void RunRustructuringExperiment(SpatialPooler sp)
{
    // Path to the folder containing training images
    string trainingFolder = "Sample\\TestFiles";
    // Get all image files matching the specified prefix
    var trainingImages = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.png");
    // Size of the images
    int imgSize = 28;
    // Name for the test image
    string testName = "test_image";
    // Array to hold active columns
    int[] activeArray = new int[64 * 64];
    // List to store heatmap data
    List<List<double>> heatmapData = new List<List<double>>();
    // Initialize a list to get normalized permanence values.
    List<int[]> BinarizedencodedInputs = new List<int[]>();
    // List to store normalized permanence values
    List<int[]> normalizedPermanence = new List<int[]>();
    // List to store similarity values
    List<double[]> similarityList = new List<double[]>();
    foreach (var Image in trainingImages)
    {
        string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{Image}", imgSize, testName);

        // Read input csv file into array
        int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

        // Initialize arrays and lists for computations
        int[] oldArray = new int[activeArray.Length];
        List<double[,]> overlapArrays = new List<double[,]>();
        List<double[,]> bostArrays = new List<double[,]>();

        // Compute spatial pooling on the input vector
        sp.compute(inputVector, activeArray, true);
        var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

        Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(activeCols);

        int maxInput = inputVector.Length;

        // Create a new dictionary to store extended probabilities
        Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();
        // Iterate through all possible inputs using a foreach loop
        foreach (var kvp in reconstructedPermanence)
        {
            int inputIndex = kvp.Key;
            double probability = kvp.Value;

            // Use the existing probability
            allPermanenceDictionary[inputIndex] = probability;
        }

        //Assinginig the inactive columns Permanence 0
        for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
        {
            if (!reconstructedPermanence.ContainsKey(inputIndex))
            {
                // Key doesn't exist, set the probability to 0
                allPermanenceDictionary[inputIndex] = 0.0;
            }
        }

        // Sort the dictionary by keys
        var sortedAllPermanenceDictionary = allPermanenceDictionary.OrderBy(kvp => kvp.Key);
        // Convert the sorted dictionary of allpermanences to a list
        List<double> permanenceValuesList = sortedAllPermanenceDictionary.Select(kvp => kvp.Value).ToList();

        //Collecting Heatmap Data for Visualization
        heatmapData.Add(permanenceValuesList);

        //Collecting Encoded Data for Visualization
        BinarizedencodedInputs.Add(inputVector);

        //Normalizing Permanence Threshold
        var ThresholdValue = 30.5;

        // Normalize permanences (0 and 1) based on the threshold value and convert them to a list of integers.
        List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);

        //Collecting Normalized Permanence List for Visualizing
        normalizedPermanence.Add(normalizePermanenceList.ToArray());

        //Calculating Similarity with encoded Inputs and Reconstructed Inputs
        var similarity = MathHelpers.JaccardSimilarityofBinaryArrays(inputVector, normalizePermanenceList.ToArray());

        double[] similarityArray = new double[] { similarity };

        //Collecting Similarity Data for visualizing
        similarityList.Add(similarityArray);
    }
    // Generate the 1D heatmaps using the heatmapData list
    Generate1DHeatmaps(heatmapData, BinarizedencodedInputs, normalizedPermanence);
    // Generate the Similarity graph using the Similarity list
    DrawSimilarityPlots(similarityList);
}

```
[Running Reconstruct Method for Image Data](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/ImageBinarizerSpatialPattern.cs#L157-L251) 
### Implementation Details for both inputs Type():
###### Reconstruct permanence values from active columns using the Spatial Pooler
reconstructedPermanence = sp.Reconstruct(actCols)

###### Set the maximum input index
maxInput = lengthofinputvectors
###### Note: According to the size of Encoded Inputs (200 bits for numerical inputs)
###### Note: According to the size of Encoded Inputs (for image  inputs the output of encoded bits depends on the multiplication of height and width of the image )

###### Initialize a dictionary to store all input indices and their associated permanence probabilities
allPermanenceDictionary = new Dictionary<int, double>()

###### Storing Permanence in the dictionary with reconstructed permanence values
for each key-value pair (inputIndex, probability) in reconstructedPermanence
    allPermanenceDictionary[inputIndex] = probability

###### Handling Inactive Columns Permanence by assigning a default permanence value of 0.0
for inputIndex from 0 to maxInput
    if inputIndex not in reconstructedPermanence
        allPermanenceDictionary[inputIndex] = 0.0

###### Note: reconstructedPermanence is a subset contributing to the construction of allPermanenceDictionary

## Getting Data For Visualizing Results
```csharp
    //Getting The Heatmap data from Reconstructed Permanence as Double
     List<List<double>> heatmapData = new List<List<double>>();
     //Getting The encoded bits data
     List<int[]> encodedInputs = new List<int[]>();
    //Getting The Nomalize Permanence as int
     List<int[]> normalizedPermanence = new List<int[]>();
```
## Normalizing the Permanence Values for Numeric Input Data
```csharp
   //We used the Threshold values 8.3 to normalize the permanence
   var ThresholdValue = 8.3;
   //calling the function ThresholdingProbabilities from Helpers.cs
List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);
  //Converting normalizedPermanence into Array
normalizedPermanence.Add(normalizePermanenceList.ToArray());
```

###### Note: The Threshold Value 8.3 has the ability to Normalize The permanence with the most similiraty with Encoded Inputs. We tried multiple Threshold values and Debugged the output and compared with encoded inputs.
## Normalizing the Permanence Values for Image Input Data
```csharp
   //Normalizing Permanence Threshold
var ThresholdValue = 30.5;

// Normalize permanences (0 and 1) based on the threshold value and convert them to a list of integers.
List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);

//Collecting Normalized Permanence List for Visualizing
normalizedPermanence.Add(normalizePermanenceList.ToArray());
```
###### Note: The Threshold Value 30.5 has the ability to Normalize The permanence with the most similiraty with Encoded Inputs. We tried multiple Threshold values and Debugged the output and compared with encoded inputs.
## Normalizing Function (ThresholdingProbabilities)
```csharp
  public static List<int> ThresholdingProbabilities(IEnumerable<double> values, double threshold)
{
    if (values == null)
    {
        return null;
    }

    List<int> resultList = new List<int>();

    foreach (var numericValue in values)
    {
        int thresholdedValue = (numericValue >= threshold) ? 1 : 0;

        resultList.Add(thresholdedValue);
    }

    return resultList;
}
```
Here is the Function
[Helpers.cs
](https://github.com/ddobric/neocortexapi/blob/master/source/NeoCortexApi/Helpers.cs#L620-L637)
## Generate1DHeatmaps Function for both Input types
```csharp
   private void Generate1DHeatmaps(List<List<double>> heatmapData, List<int[]> encodedData, List<int[]> normalizedPermanence)
{
    int i = 1;

    foreach (var values in heatmapData)
    {
       
        string folderPath = Path.Combine(Environment.CurrentDirectory, "1DHeatMap");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, $"heatmap_{i}.png");
        Debug.WriteLine($"FilePath: {filePath}");
      
        double[] array1D = values.ToArray();
       
        NeoCortexUtils.Draw1DHeatmap(new List<double[]>() { array1D }, new List<int[]>() { normalizedPermanence[i - 1] }, new List<int[]>() { normalizedPermanence[i - 1] }, filePath, 200, 8, 9, 4, 0, 30);

        Debug.WriteLine("Heatmap generated and saved successfully.");
        i++;
    }
}
```
[GenarateHeatmap Function](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs#L311) - Lines (311 to 341)
###### Parameters
- `heatmapData`: A list of lists containing probability data for heatmap generation.
- `EncodedData`: A list of lists containing Encoded input Data.
- `normalizedPermanence`: A list of arrays containing normalized permanence values corresponding to the heatmap data.

###### Implementation
- The function iterates through each set of probabilities in `heatmapData`.

###### Folder and File Management:
- A folder path is defined based on the current environment, specifically within the "1DHeatMap" directory.
- If the folder does not exist, it is created to ensure proper organization.
- The file path for each heatmap is constructed dynamically using the folder path and an index (`i`).

###### 1D Array Conversion:
- The probabilities list is converted into a 1D array (`array1D`) using the `ToArray` method for compatibility with the subsequent heatmap generation process.

###### Heatmap Generation:
- The function calls a modified version of the `Draw1DHeatmapWithSeparatedValues` function from the `NeoCortexUtils` class.
- This function handles the visualization process, considering the 1D array of probabilities (`array1D`) and the corresponding normalized permanence values.
- Key parameters, such as file path, dimensions, and visualization settings, are dynamically adjusted for each iteration.
###### Note: Heatmap Generation Parameters

- **`filePath`**: File path where the heatmap image will be saved.
- **`width`**: 200 (pixels) - Width of the heatmap image.
- **`height`**: 8 (pixels) - Height of the heatmap image.
- **`mostHeatedColor`**: 9 - Value for the most heated color (Red represents 1).
- **`medianValue`**: 4 - Median value for color interpolation.
  - Example: Greater than 4 represents orange to red, less than 4 represents green to yellow.
- **`coldestColor`**: 0 - Coldest color representing 0 bits.
- **`enlargementFactor`**: 30 - Enlargement factor used to magnify the image for better visualization.


###### Debugging Information:
- Debugging information, including file paths and successful heatmap generation confirmation, is output using `Debug.WriteLine`.
## Calling HeatMap Function
```csharp
//Calling the HeatMap Function in RunRestructuringExperiment with two Perameters
Generate1DHeatmaps(heatmapData, normalizedPermanence);
```

## Combined Visualization: Heatmaps and int[] Sequences
We Applied this Function to Draw1DHeatmap
Click Below for More Details 
[Draw1dHeatmap](https://github.com/ddobric/neocortexapi/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L222-L351) - Lines (222 to 351)
**Outcomes:**
- HeatMap Image for all inputs as Image Visualization.
- Encoded Inputs as int []
- Reconstruced Input as int [] (Normalized Permanence)
- Combined Image.


**Results Example:**
**Fig: Final Outcome**
![Final Outcome](https://github.com/ddobric/neocortexapi/assets/64829519/28e6a62c-0c72-424a-9194-a3064a6abc7a)

## Similarity Calculation Using Jaccard Similarity Coefficient
```csharp
   public static double JaccardSimilarityofBinaryArrays(int[] arr1, int[] arr2)
{
    if (arr1.Length != arr2.Length)
    {
        throw new ArgumentException("Arrays must have the same length.");
    }

    int intersectionCount = 0;
    int unionCount = 0;

    for (int i = 0; i < arr1.Length; i++)
    {
        if (arr1[i] == 1 && arr2[i] == 1)
        {
            intersectionCount++;
        }
        if (arr1[i] == 1 || arr2[i] == 1)
        {
            unionCount++;
        }
    }

    return (double)intersectionCount / unionCount;
}
```
Here is the Function
[MathHelpers.cs](https://github.com/ddobric/neocortexapi/blob/master/source/NeoCortexApi/Utility/MathHelpers.cs#L182-L205) 
## Genarate Similarity Graph
- Note The calling Similarity Function is same like Drwaing Heatmap

We Applied this Function to DrawCombinedSimilarityplot
Click Below for More Details 
[DrawCombinedSimilarityPlot](https://github.com/ddobric/neocortexapi/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L428-L536) 
**Outcomes:**
- Bar graphs of similarity for each inputs


**Results Example:**
**Fig: Final Outcome for Image  input**
![Final Outcome](https://github.com/ddobric/neocortexapi/assets/64829519/055b1b32-242a-4e2b-9ab3-cbaf159907ed)

## Spatial Pooler Reconstruction Tests
## UnitTest of SdrReconstructionTests
We Tested the SdrReconstruction.cs with 9 Test cases and all Passed
This document provides an overview of the unit tests present in the project.
[SdrReconstructionTests](https://github.com/ddobric/neocortexapi/blob/master/source/UnitTestsProject/SdrReconstructionTests.cs)
### Reconstruct_ValidInput_ReturnsResult
- **Test Category:** SpatialPoolerReconstruction
- **Description:** Verifies whether the `Reconstruct` method in the `SPSdrReconstructor` class behaves correctly under valid input conditions. It ensures that the method returns a dictionary containing keys for all provided active mini-columns, with corresponding permanence values. Additionally, it confirms that the method properly handles the case where a key is not present in the dictionary.

### Reconstruct_NullInput_ThrowsArgumentNullException
- **Test Category:** ReconstructionExceptionHandling
- **Description:** Verifies that the `Reconstruct` method in the `SPSdrReconstructor` class throws an `ArgumentNullException` when invoked with a null input parameter.

### Reconstruct_EmptyInput_ReturnsEmptyResult
- **Test Category:** ReconstructionEdgeCases
- **Description:** Tests whether the `Reconstruct` method returns an empty dictionary when provided with an empty input.

## Reconstruction Tests for Various Scenarios

### Reconstruct_AllPositivePermanences_ReturnsExpectedValues
- **Test Category:** ReconstructionAllPositiveValues
- **Description:** Checks if the `Reconstruct` method in the `SPSdrReconstructor` class handles a scenario where all mini-column indices provided as input are positive integers and returns permanence values that are non-negative.

### Reconstruct_AddsKeyIfNotExists
- **Test Category:** ReconstructionAddingKeyIfNotExist
- **Description:** Ensures that the `Reconstruct` method adds a key to the dictionary if it doesn't already exist.

### Reconstruct_ReturnsValidDictionary
- **Test Category:** ReconstructionReturnsKvP
- **Description:** Validates whether the `Reconstruct` method returns a valid dictionary containing integer keys and double values.

### Reconstruct_NegativePermanences_ReturnsFalse
- **Test Category:** ReconstructedNegativePermanenceRetunsFalse
- **Description:** Tests the behavior of the `Reconstruct` method when encountering negative permanences and asserts that no negative permanences should be present in the reconstructed values.

### Reconstruct_AtLeastOneNegativePermanence_ReturnsFalse
- **Test Category:** ReconstructedNegativePermanenceRetunsFalse
- **Description:** Validates the behavior of the `Reconstruct` method when at least one permanence value is negative.

### Reconstruct_InvalidDictionary_ReturnsFalse
- **Test Category:** DataIntegrityValidation
- **Description:** Verifies if the `Reconstruct` method returns a valid dictionary by checking specific criteria such as NaN values and keys less than 0.

### IsDictionaryInvalid with Not a Number
- **Test Category:** DictionaryValidityTests
- **Description:** Determines whether a dictionary is considered invalid based on specific criteria like null reference, NaN values, and keys less than 0.
