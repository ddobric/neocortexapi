using Daenet.Binarizer.Entities;
using System;
using System.Collections.Generic;
using System.IO;

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
            string inputFolder = "CommonFiles"; // Path to the folder containing input images
            string outputFolder = ".\\OutputFiles"; // Path to the folder where results will be saved

            // Ensure the output folder exists
            Directory.CreateDirectory(outputFolder);

            // Define supported image extensions
            string[] supportedExtensions = new[] { "*.jpeg", "*.jpg", "*.png", "*.bmp", "*.tiff" };

            // Get all supported image files from the input folder
            var imageFiles = new List<string>();
            foreach (var ext in supportedExtensions)
            {
                imageFiles.AddRange(Directory.GetFiles(inputFolder, ext));
            }

            if (imageFiles.Count == 0)
            {
                Console.WriteLine("No images found in the input folder.");
                return;
            }

            foreach (var imagePath in imageFiles)
            {
                try
                {
                    // Construct the output file name based on the input file name
                    string outputFileName = Path.GetFileNameWithoutExtension(imagePath) + "_Binarized.txt";
                    string outputPath = Path.Combine(outputFolder, outputFileName);

                    // Define binarizer configuration for each image
                    var config = new BinarizerParams
                    {
                        InputImagePath = imagePath,
                        OutputImagePath = outputPath,
                        ImageWidth = 40,
                        ImageHeight = 20
                    };

                    // Run the binarizer
                    var img = new CustomImageBinarizer(config);
                    img.Run();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {imagePath}: {ex.Message}");
                }
            }

            Console.WriteLine("All images have been processed.");
        }
    }
}