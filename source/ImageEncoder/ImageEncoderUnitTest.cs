using Daenet.ImageBinarizerLib.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace NeoCortexApi.Encoders
{
    [TestClass]
    public class ImageEncoderUnitTest
    {
        [TestMethod]
        [TestCategory("without learning")]
        [DataRow(500,500,"1")]
        [DataRow(500,400,"2")]
        [DataRow(400,500,"3")]
        public void ImageEncoderTest(int width, int height, string testName)
        {
            string testFolder = InitTestFolder();
         
            string inputImagePath = "inputImage.jpg";
         
            ImageEncoder encoder = new ImageEncoder(new BinarizerParams {  ImageWidth = width, ImageHeight = height, OutputImagePath = Path.Combine(testFolder, inputImagePath) });
          
            int[] encodedValue = encoder.Encode(inputImagePath);

            encoder.EncodeAndSaveAsImage(Path.Combine(testFolder,$"encodedImage_{testName}.png"));
        }
        [TestMethod]
        public void LearningInLayerTest()
        {

        }


        static public string InitTestFolder()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            string testName = stackFrame.GetMethod().Name;
            if (!Directory.Exists($"{testName}/"))
            {
                Directory.CreateDirectory($"{testName}/");
            }
            return testName;
        }


    }
}