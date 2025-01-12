using Daenet.Binarizer.Entities;
using Daenet.Binarizer;

/// <summary>
/// Provides methods to binarize an image.
/// </summary>
public class ImgBinarizer
{

    /// <summary>
    /// Binarizes an image and returns the binarized image as a 1D array of integers.
    /// </summary>
    /// <returns>The binarized image as a 1D array of integers.</returns>
    public static int[] ProcessImagesAsSingleArray(string folderPath)
    {
        // Dictionary to hold image paths and their binary data.
        Dictionary<string, int[]> binarizedImages = new Dictionary<string, int[]>();

        // Iterate through all images in the folder.
        foreach (var imagePath in Directory.GetFiles(folderPath, ".", SearchOption.AllDirectories)
                        .Where(file => file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)))
        {
            // Define binarization parameters for each image.
            var parameters = new BinarizerParams
            {
                InputImagePath = imagePath,
                ImageHeight = 28,
                ImageWidth = 28,
            };

            // Create an ImageBinarizer instance and get the binary array.
            ImageBinarizer bizer = new ImageBinarizer(parameters);
            var doubleArray = bizer.GetArrayBinary();

            // Flatten 3D double array to a 1D int array.
            int height = doubleArray.GetLength(1);
            int width = doubleArray.GetLength(0);
            int[] intArray = new int[height * width];

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    intArray[j * width + i] = (int)doubleArray[i, j, 0];
                }
            }

            // Add the result to the dictionary.
            binarizedImages[imagePath] = intArray;
        }

        // Combine all binary data into a single array.
        var combinedBinaryData = binarizedImages.Values.SelectMany(x => x).ToArray();

        // Return the combined binary data as a single 1D array.
        return combinedBinaryData;

    }      
}
