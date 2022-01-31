using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Daenet.ImageBinarizerLib.Entities;
using Daenet.ImageBinarizerLib;
using Newtonsoft.Json;
using System.IO;

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

        BinarizerParams binarizerParmas;

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

            this.binarizerParmas = binarizerParmas;

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
            this.binarizerParmas.InputImagePath = (string)inputFile;

            var binarizer = new ImageBinarizer(binarizerParmas);

            return GetArray<int>(binarizer);
        }

        public void EncodeAndSave(string inputFile, string outputFile)
        {
            this.binarizerParmas.InputImagePath = inputFile;
            this.binarizerParmas.OutputImagePath = outputFile;
            this.imageBinarizer.Run();
        }

        public void EncodeAndSaveAsImage(string inputFile, string outputFile)
        {
            // Bitmap does not work on Linux.
            // see https://developers.de/2022/01/14/bye-by-system-drawing-and-gdi/
            //    Bitmap bmp = new Bitmap(width, height);

            //    for (int y = 0; y < bmp.Height; y++)
            //    {
            //        for (int x = 0; x < bmp.Width; x++)
            //        {
            //            int b = (int)binarizedImage[y, x, 0];
            //            bmp.SetPixel(x, y, Color.FromArgb(255, 255 * b, 255 * b, 255 * b));
            //        }
            //    }
        }

        /// <summary>
        /// Gets the image pixels.
        /// </summary>
        /// <returns></returns>
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

        // see https://developers.de/2022/01/14/bye-by-system-drawing-and-gdi/
        // We should not use the bitmap here.
        //private void SaveBinarizedImage(string outputFilePath, int width, int height, double[,,] binarizedImage)
        //{
        //    Bitmap bmp = new Bitmap(width, height);

        //    for (int y = 0; y < bmp.Height; y++)
        //    {
        //        for (int x = 0; x < bmp.Width; x++)
        //        {
        //            int b = (int)binarizedImage[y, x, 0];
        //            bmp.SetPixel(x, y, Color.FromArgb(255, 255 * b, 255 * b, 255 * b));
        //        }
        //    }
        //    bmp.Save(outputFilePath);
        //}
        #endregion
    }
}
