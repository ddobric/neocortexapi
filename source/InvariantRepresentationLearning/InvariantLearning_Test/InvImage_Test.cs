using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using InvariantLearning;
using Invariant.Entities;

namespace InvariantLearning_Test
{
    [TestClass]
    public class InvImage_Test
    {
        [TestMethod]
        [TestCategory("Invariant Learning")]
        public void GetPixels_Test()
        {
            #region in/out file config

            // file name declaration
            string folderName = $"TEST";
            string testName = "GetPixels_Test";
            string imagePath = Path.Combine("TEST_INPUT", "apple.jpg");

            // folder creation
            Utility.CreateFolderIfNotExist(Path.Combine("TEST_OUTPUT", testName, folderName));
            #endregion

            Picture invImage = new Picture(imagePath, "test");
            Debug.WriteLine($"{invImage.label}");

            var a = invImage.GetPixels();
            string outImg = Path.Combine("TEST_OUTPUT", testName, folderName, "out_apple.png");
            Picture.SaveAsImage(a, outImg);
        }
        /// <summary>
        /// Taking the frame out from the image
        /// </summary>
        /// <param name="tlX">top left Coord X</param>
        /// <param name="tlY">top left Coord Y</param>
        /// <param name="brX">bottom right Coord X</param>
        /// <param name="brY">bottom right Coord Y</param>
        [TestMethod]
        [TestCategory("Invariant Learning")]
        [DataRow(0,30,60,60)]
        public void GetPixels_Frame_Test(int tlX,int tlY,int brX, int brY)
        {
            #region in/out file config

            // file name declaration
            string folderName = $"TEST_{tlX}_{tlY}_{brX}_{brY}";
            string testName = "GetPixels_Frame_Test";
            string imagePath = Path.Combine("TEST_INPUT", "apple.jpg");

            // folder creation
            Utility.CreateFolderIfNotExist(Path.Combine("TEST_OUTPUT", testName, folderName));
            #endregion
            Frame frame = new Frame(tlX, tlY, brX, brY);
            Picture invImage = new Picture(imagePath, "test");
            Debug.WriteLine($"{invImage.label}");

            var a = invImage.GetPixels(frame);

            string outImg = Path.Combine("TEST_OUTPUT",testName,folderName,"out_apple2.png");
            Picture.SaveAsImage(a, outImg);
        }
    }
}