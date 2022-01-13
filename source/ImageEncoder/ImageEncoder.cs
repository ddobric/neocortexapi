using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Daenet.ImageBinarizerLib.Entities;
using Daenet.ImageBinarizerLib;
using Newtonsoft.Json;
using System.Drawing;

namespace NeoCortexApi.Encoders
{
    internal class ImageEncoder : EncoderBase
    {
        #region NotImplementedParts
        public override int Width => throw new NotImplementedException();

        public override bool IsDelta => throw new NotImplementedException();
        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
        #endregion

        BinarizerParams parameters;
        ImageBinarizer imageBinarizer;

        #region Contructor
        public ImageEncoder() { }
        public ImageEncoder(Dictionary<string, object> encoderSettings)
        {
            if (encoderSettings == null)
            {
                return;
            }

            this.Initialize(encoderSettings);
            var json = JsonConvert.SerializeObject(Properties);
            parameters = JsonConvert.DeserializeObject<BinarizerParams>(json);

            if (parameters == null)
            {
                throw new System.Exception("Invalid encoder setting for ImageEncoder");
            }
        }
        #endregion

        /// <summary>
        /// Encoding an image with a given full path to a binary array
        /// </summary>
        /// <param name="inputFilePath">Input image's full path</param>
        /// <returns></returns>
        public override int[] Encode(object inputFilePath)
        {
            try
            {
                parameters.InputImagePath = (string)inputFilePath;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            imageBinarizer = new ImageBinarizer(parameters);
            return GetIntArray();
        }

        #region API for communicating with the library Daenet.ImageBinarizerLib

        /// <summary>
        /// Method to convert GetArrayBinary from data type double[,,] to int[]
        /// </summary>
        /// <returns></returns>
        private int[] GetIntArray()
        {
            var doubleArray = imageBinarizer.GetArrayBinary();
            var hg = doubleArray.GetLength(1);
            var wd = doubleArray.GetLength(0);
            var intArray = new int[hg * wd];
            for (int j = 0; j < hg; j++)
            {
                for (int i = 0; i < wd; i++)
                {
                    intArray[j * wd + i] = (int)doubleArray[i, j, 0];
                }
            }
            return intArray;
        }

        public void SaveBinarizedImage(string outputFilePath)
        {
            Bitmap bmp = new Bitmap(parameters.ImageWidth, parameters.ImageHeight);
            double[,,] binarizedImage = imageBinarizer.GetArrayBinary();
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int b = (int)binarizedImage[y, x, 0];
                    bmp.SetPixel(x, y, Color.FromArgb(255, 255*b, 255 * b, 255 * b));
                }
            }
            bmp.Save(outputFilePath);
        }

        #endregion
    }
}
