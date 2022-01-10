# TraceColumnOverlap Method

TraceColumnOverlap Method presents various ways to view the column overlap values. After a stable case, the overlap of multiple columns are traced in an overlap array.
The data can be viewed in various formats through TraceColumnOverlap method. The overlapped values can be traced in a text format in the console output or can be imported to a excelsheet. These texts or the excelsheets can be used to view the corresponding values. The data can also be represented by a line graph which includes the threshold parameter additionally, to differentiate the overlapped values.


## Method Signature

```cs
TraceColumnsOverlap( List<double[,]> overlapArrays, int[] colDims, TraceFormat formatTypes,
double threshold, Color aboveThreshold, Color belowThreshold)
```

## Parameters

- `overlapArrays` int[]

overlapArrays: The list of arrays that contains overlaps of mini-columns to the set of input neurons. When initialized, the SpatialPooler automatically connects mini-columns to the input neurons. During the learning process, the SpatialPooler makes sure that active columns in the current learning step increase the permanence of synapses connected to the input cells currently set on '1' and decrease the permanence of synapses connected to input cells set on '0'. This leads to the dynamic change of the permanence value of synapses. Once the permanence reaches the configured threshold, it is declared as a connection. The sum of all mini-column connected synapses defines the column's overlap value.

***Example:***
Here in this case overlap array after being called in inmplementation is passed in the first parameter.
```cs
TraceColumnsOverlap(overlapArrays, colDims, TraceFormat.Excel, 24, Color.Blue, Color.Yellow);
```
https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/blob/60757a5b1388b04094ecbced86b659d8ae7f6ab8/MyProject/SDRrepresentation/SDRrepresentation/ImageSDRGeneration.cs#L165


- `colDims` int[]
Dimensions of the 2-D array.

***Example:***
Column Dimensions for example is set to 64 * 64 and is passed as the second parameter.

- `TraceFormat formatTypes` enum
formatTypes is a parameter of type enum (TraceFormat) which is an enum used to generate different types of Overlap representations.
formatTypes can take the following values: Text, Graph and Excel (enum values defined for TraceFormat).
Using a switch case with formatTypes as the parameter.
```cs
switch (formatTypes)
{
    case TraceFormat.Text:
        TraceInLineFormat(overlapArrays, colDims);
        break;
}
```
***Example:***
To view in excel format, trace format can be set to TraceFormat.Excel as the third parameter in the function call.
```cs
TraceColumnsOverlap(overlapArrays, colDims, TraceFormat.Excel, 24, Color.Blue, Color.Yellow);
```

- `threshold` double Threshold is the value that is set in order to separate the activecolumns (cells) from inactive ones.
All overlaps over this value are separately marked.

***Example:***
Here in this case the Threshold value is set to 24.
```cs
TraceColumnsOverlap(overlapArrays, colDims, TraceFormat.Excel, 24, Color.Blue, Color.Yellow);
```

- `aboveThreshold` Color
Assigns the color to columns above the threshold value.

- `belowThreshold` Color
Assigns the color to columns below the threshold value.

https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/blob/60757a5b1388b04094ecbced86b659d8ae7f6ab8/MyProject/SDRrepresentation/SDRrepresentation/ImageSDRGeneration.cs#L327

## Different formats to represent the overlaps as below:

LineTrace : The line trace can be seen in the console output of the visual studio
```cs
0,29
1,18
2,17
...
```


Excel :

Using ***GemBox.Spreadsheet*** library.

The generated excel file with name " ColumnOverlapTracing.xlsx " is saved in the below path:
se-cloud-2020-2021\MyProject\SDRrepresentation\SDRrepresentation\bin\Debug\net5.0

<img src="https://user-images.githubusercontent.com/74201238/125265516-35affe00-e305-11eb-8ce4-b1ce1647b1da.png" width=450></br>

Graph :
Complete information on how to plot graph for Column Overlap can be found in the below link:
https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/blob/ML20/21-5.8-Implement-SDR-representation-samples-IndieNeurons/MyProject/Documentation/PlottingGraphForColumnOverlaps.md

The graph representation below shows the column overlap:

<img src="https://user-images.githubusercontent.com/74201563/123655925-ba901780-d82f-11eb-894d-cdae8a67574c.png"></br>


## Example : Tracing to Excel file


~~~cs

double[,] overlapArray = new int[4, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
TraceColumnsOverlap( overlapArrays, 64, TraceFormat.Excel, thershold:24, Color.Blue, Color.Yellow);
~~~


URL for the location of the code :

https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/blob/4719565c12918681ab41a3969cbaeb2b7df8f145/MyProject/SDRrepresentation/SDRrepresentation/ImageSDRGeneration.cs#L323
