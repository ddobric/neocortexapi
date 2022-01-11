# Plotting the graph to represent the Column Overlap (Using C#)


## graph.DrawLines(Pen, Point[] { new Point(), new Point() })

The overlap is represented in the form of graph using `graph.DrawLines()` , which is implemented in ***TraceInGraphFormat()*** method.

***graph.DrawLines()*** is present in `System.Drawing.Common` library.

Refer the below URL to know the method description of ***graph.Drawlines()*** :
https://developers.de/2018/01/22/how-to-use-system-drawing-in-net-core/

Method Signature :

```cs
graph.DrawLines(pen, new Point[] { new Point(row, (int)entry[i, j]), new Point(row, 0) });
```

## Parameters

- `pen` Pen

  sets the colour of the lines to be plotted as graph

- `Point[] { new Point(row, (int)entry[i, j])` int

  sets the x and y coordinates

- `Point[] { new Point(row, 0)}` int

  sets the x coordinate only

## Implementation

```cs
public void TraceInGraphFormat(List<double[,]> overlapArrays, int[] colDims, double threshold, Color aboveThreshold, Color belowThreshold)
{
    int row = 0;

    Image image = new Bitmap(500, 524);

    Graphics graph = Graphics.FromImage(image);

    graph.Clear(Color.Azure);
    Pen pen = new Pen(belowThreshold);

    foreach (var entry in overlapArrays)
    {
        for (int i = 0; i < colDims[0]; ++i)
        {
            for (int j = 0; j < colDims[1]; ++j)
            {
                var overlapValue = (entry[i, j]);
                if (overlapValue > threshold)
                {
                    Pen penthreshold = new Pen(aboveThreshold);
                    graph.DrawLines(penthreshold, new Point[] { new Point(row, (int)entry[i, j]), new Point(row, 0) });
                }
                else
                {
                    graph.DrawLines(pen, new Point[] { new Point(row, (int)entry[i, j]), new Point(row, 0) });
                }
                row++;
            }
        }
    }
    image.Save("trace_col_overlap_graph.jpeg", System.Drawing.Imaging.ImageFormat.Png);
}

```
### In the graph:
- For X-axis: The indexes of the 2-D array are represented as integer values (example: for index [0][1] -> 1).
- For Y-axis: The overlap value for each [row][column] is represented.


## URL for the code (implementing graph.Drawlines()) :

URL for the location of the code at line number 345 within the ***TraceInGraphFormat()*** method :

https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/blob/762ec726b21d5337f234b4f22956253e32f0382a/MyProject/SDRrepresentation/SDRrepresentation/ImageSDRGeneration.cs#L369

## Example:
The output for overlap array of the Image (Alphabet L) can be viewed in the form of a graph representation in png format:

Output:

<img src="https://user-images.githubusercontent.com/74201563/121677365-0e6fd200-cab6-11eb-802a-d44b63f72c9d.png" width="450"><br />



