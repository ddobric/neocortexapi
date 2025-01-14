using Daenet.Binarizer.Entities;

namespace ProjectNeoCortexAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to our project ML 24/25-01");
            Console.WriteLine("Investigate Image Reconstruction by using Classifiers");
            Console.WriteLine("Created by:");
            Console.WriteLine("     Anoushka Piplai[1566664]");
            Console.WriteLine("     Avradip Mazumdar[1566651]");
            Console.WriteLine("     Raka Sarkar[1567153]");
            Console.WriteLine("     Somava Ganguly[1566916]\n\n");

            Run();
        }

        public static void Run()
        {
            Console.WriteLine("Processing all images in the input folder.");

            // Specify the input folder and output folder
            string inputFolder = "CommonFiles"; // Path to the folder containing input image

            // Get all image files from the input folder
            string[] imageFiles = Directory.GetFiles(inputFolder, "*.jpeg");

            if (imageFiles.Length == 0)
            {
                Console.WriteLine("No images found in the input folder.");
                return;
            }

            foreach (var imagePath in imageFiles)
            {
                // Construct the output file name based on the input file name
                string outputFileName = Path.GetFileNameWithoutExtension(imagePath) + "_Binarized.txt";

                // Define binarizer configuration for each image
                var config = new BinarizerParams
                {
                    InputImagePath = imagePath,
                    OutputImagePath = outputFileName,
                    ImageWidth = 40,
                    ImageHeight = 20
                };

                // Run the binarizer
                var img = new CustomImageBinarizer(config);
                img.Run();
            }

            Console.WriteLine("All images have been processed.");
        }
    }
}