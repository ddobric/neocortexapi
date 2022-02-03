using Daenet.ImageBinarizerLib.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace NeoCortexApi.Encoders
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
        public void LearningInLayerTest()
        {

        }


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
    }
}