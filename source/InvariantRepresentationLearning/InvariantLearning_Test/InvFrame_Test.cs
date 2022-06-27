using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvariantLearning;
using dataSet;
namespace InvariantLearning_Test
{
    [TestClass]
    public class InvFrame_Test
    {
        /// <summary>
        /// Testing division frame for array
        /// </summary>
        [TestMethod]
        [TestCategory("Invariant Learning")]
        [TestCategory("Convolution Test")]
        [DataRow(0,19,5)]
        [DataRow(4,25,7)]
        [DataRow(10,100,21)]
        public void GetDivisionIndex_Test(int head, int tail, int No)
        {
            var results = Frame.GetIndexes(head, tail, No);
            foreach(var i in results)
            {
                // printing the divided index on pixels
                Debug.Write($" {i}");
            }
            Debug.WriteLine("");
        }
        /// <summary>
        /// Testing Convolution frame by saving frames as output images
        /// </summary>
        [TestMethod]
        [TestCategory("Invariant Learning")]
        [TestCategory("Convolution Test")]
        [DataRow(60,60,3,3)]
        [DataRow(40,40,2,3)]
        [DataRow(25,25,4,4)]
        public void GetConvFrames_Test(int frameWidth, int frameHeight, int NoX, int NoY)
        {
            #region in/out file config

            // file name declaration
            string folderName = $"TEST_{frameWidth}_{frameHeight}_{NoX}_{NoY}";
            string testName = "GetConvFrames_Test";
            string imagePath = Path.Combine("TEST_INPUT","apple.jpg");
            // string imagePath = "ascdScript.png";

            // folder creation
            Utility.CreateFolderIfNotExist(Path.Combine("TEST_OUTPUT", testName, folderName));
            #endregion

            Picture invImage = new Picture(imagePath, "test");
            Debug.WriteLine($"{invImage.label}");

            var a = invImage.GetPixels();
            Picture.SaveAsImage(a, "out_apple2.png");
            var b = Frame.GetConvFrames(invImage.imageWidth, invImage.imageHeight, frameWidth, frameHeight, NoX, NoY);
            int count = 0;
            foreach(var frame in b)
            {
                count += 1;
                string imgPath = Path.Combine("TEST_OUTPUT",testName,folderName,$"{Path.GetFileNameWithoutExtension(imagePath)}{frame.brX}{frame.brY}_i.png");
                Picture.SaveAsImage(invImage.GetPixels(frame),imgPath);
            }
        }
    }
}
