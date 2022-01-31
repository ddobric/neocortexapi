using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Daenet.ImageBinarizerLib.Entities;
using Daenet.ImageBinarizerLib;
using Newtonsoft.Json;
using System.IO;
using SkiaSharp;

namespace NeoCortexApi.Encoders
{
    /// <summary>
    /// It encodes the image by using the binarization algorithm. The SDR produced by this encoder is a set of binary bits that represent pixels in the image.
    /// </summary>
    public class ImageEncoder : EncoderBase
    {
        #region NotImplementedParts
        public override int Width => throw new NotImplementedException();

        public override bool IsDelta => throw new NotImplementedException();

        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// The instance of the binarized to be used while encoding.
        /// </summary>
        ImageBinarizer imageBinarizer;

        BinarizerParams binarizerParams;

        #region Constructors and Initialization

        public ImageEncoder() { }

        /// <summary>
        /// Creates the instance of the image encoder.
        /// </summary>
        /// <param name="binarizerParmas"></param>
        /// <exception cref="ArgumentException"></exception>
        public ImageEncoder(BinarizerParams binarizerParmas)
        {
            if (binarizerParmas == null)
            {
                throw new ArgumentException("Invalid encoder setting for ImageEncoder");
            }

            this.binarizerParams = binarizerParmas;

            this.imageBinarizer = new ImageBinarizer(binarizerParmas);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Encoding an image with a given full path to a binary array
        /// </summary>
        /// <param name="inputFile">Input image's full path</param>
        /// <returns></returns>
        public override int[] Encode(object inputFile)
        {
            this.binarizerParams.InputImagePath = (string)inputFile;

            var binarizer = new ImageBinarizer(binarizerParams);

            return GetArray<int>(binarizer);
        }

        public void EncodeAndSave(string inputFile, string outputFile)
        {
            this.binarizerParams.InputImagePath = inputFile;
            this.binarizerParams.OutputImagePath = outputFile;
            this.imageBinarizer.Run();
        }

        /// <summary>
        /// <br>Encode the Input file Image, binarize it and save to an output file image</br>
        /// <br>Default image format: Png</br>
        /// </summary>
        /// <param name="inputFile">Input image path</param>
        /// <param name="outputFile">Output image path</param>
        /// <param name="encodingFormat">One of following formats is supported: Bmp, Gif, Ico, Jpeg, Png, Wbmp, Webp, Pkm, Ktx, Astc, Dng, Heif </param>
        public void EncodeAndSaveAsImage(string inputFile, string outputFile, string encodingFormat = "Png")
        {
            //
            // 1. Initiate the parameters
            this.binarizerParams.InputImagePath = inputFile;
            this.binarizerParams.OutputImagePath = outputFile;
            double[,,] binarizedImage = GetPixels();

            //
            // 2. Reading the inputFile image to type SKBitmap
            SKBitmap inputBitmap = SKBitmap.Decode(inputFile);
            SKBitmap outputBitmap = new SKBitmap(binarizerParams.ImageWidth, binarizerParams.ImageHeight);
            inputBitmap.ScalePixels(outputBitmap, SKFilterQuality.High);
            for (int y = 0; y < outputBitmap.Height; y++)
            {
                for (int x = 0; x < outputBitmap.Width; x++)
                {
                    int b = (int)binarizedImage[y, x, 0];
                    outputBitmap.SetPixel(x, y, new SKColor((byte)(255 * b), (byte)(255 * b), (byte)(255 * b)));
                }
            }

            //
            // 3. Reading a Picture file in pixels
            using (var image = SKImage.FromBitmap(outputBitmap))
            {
                SKEncodedImageFormat frm = (SKEncodedImageFormat)Enum.Parse(typeof(SKEncodedImageFormat), encodingFormat);

                using (var data = image.Encode(frm, 80))
                {
                    // save the data to a stream
                    using (var stream = File.OpenWrite($"{outputFile}"))
                    {
                        data.SaveTo(stream);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the image pixels.
        /// </summary>
        /// <returns>double 3D array of greyscale value</returns>
        public double[,,] GetPixels()
        {
            return imageBinarizer.GetArrayBinary();
        }



        #endregion

        #region Private Methods


        /// <summary>
        /// Method to convert GetArrayBinary from data type double[,,] to int[]
        /// </summary>
        /// <returns></returns>
        private static T[] GetArray<T>(ImageBinarizer imageBinarizer)
        {
            var doubleArray = imageBinarizer.GetArrayBinary();
            var hg = doubleArray.GetLength(1);
            var wd = doubleArray.GetLength(0);
            T[] intArray = new T[hg * wd];

            for (int j = 0; j < hg; j++)
            {
                for (int i = 0; i < wd; i++)
                {
                    intArray[j * wd + i] = (T)Convert.ChangeType(doubleArray[i, j, 0], typeof(int));
                }
            }
            return intArray;
        }
        #endregion
    }
}
