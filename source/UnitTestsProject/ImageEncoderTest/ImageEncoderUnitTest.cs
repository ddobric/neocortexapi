using Daenet.ImageBinarizerLib.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using HtmImageEncoder;
using NeoCortexApi;

namespace ImageEncoderTest
{
    [TestClass]
    public class ImageEncoderUnitTest
    {
        [TestMethod]
        [TestCategory("without learning")]
        [DataRow(500,500,"digit7.png")]
        [DataRow(500,400,"digit1.png")]
        [DataRow(400,500,"digit7a.png")]
        public void ImageEncoderTest(int width, int height, string imageName)
        {
            var outFolder = EnsureFolderExist(nameof(ImageEncoderTest));

            string inputImage = Path.Combine("TestFiles",imageName);

            ImageEncoder encoder = new ImageEncoder(new BinarizerParams { ImageWidth = width, ImageHeight = height });

            int[] encodedValue = encoder.Encode(inputImage);

            encoder.EncodeAndSaveAsImage(inputImage, Path.Combine(outFolder, $"encodedImage_{imageName}"));

            encoder.EncodeAndSave(inputImage, Path.Combine(outFolder, $"encodedImage_{Path.GetFileNameWithoutExtension(imageName)}.txt")) ;
        }


        [TestMethod]
        [TestCategory("with learning")]
        [DataRow(500, 400, "digit1.png")]
        [DataRow(400, 500, "digit7a.png")]
        public void LearningInLayerTest(int width, int height, string imageName)
        {
            // Prepare test output folder
            var outFolder = EnsureFolderExist(nameof(ImageEncoderTest));

            // Prepare input file for test
            string inputImage = Path.Combine("TestFiles", imageName);

            // Initialize Image Encoder
            ImageEncoder encoder = new ImageEncoder(new BinarizerParams { ImageWidth = width, ImageHeight = height });

            // Initialize HTMModules 
            int inputBits = width* height;
            int numColumns = 1024;
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns });
            var mem = new Connections(cfg);

            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            //Test Compute method
            var computeResult = layer1.Compute(inputImage, true) as int[];
            var activeCellList = GetActiveCells(computeResult);
            Debug.WriteLine($"Active Cells computed from Image {inputImage}: {activeCellList}");
        }

        

        #region private help method
        /// <summary>
        /// <br>Check for existence of specified folder path</br>
        /// <br>If not, create a folder with that name</br>
        /// </summary>
        /// <param name="foldername">folder name</param>
        /// <returns></returns>
        private static string  EnsureFolderExist(string foldername)
        {   
            if (!Directory.Exists(foldername))
            {
                Directory.CreateDirectory(foldername);
            }

            while (!Directory.Exists(foldername))
            {
                Thread.Sleep(250);
            }

            return foldername;
        }

        /// <summary>
        /// Convert int array to string for better representation
        /// </summary>
        /// <param name="computeResult"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private string GetActiveCells(int[] computeResult)
        {
            string result = String.Join(",",computeResult);
            return result;
        }
        #endregion
    }
}