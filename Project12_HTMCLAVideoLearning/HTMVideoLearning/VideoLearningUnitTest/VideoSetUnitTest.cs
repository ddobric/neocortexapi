using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLearningUnitTest
{
    [TestClass]
    public class VideoSetUnitTest
    {
        [TestMethod, TestCategory("static method"), TestCategory("Video")]
        [DataRow(42, 0, 0, 1, 0, 1, 0, 1, 0 )]
        public void ColorChannelToBinList_testing(int pixelChannelColor, int b1, int b2, int b3, int b4, int b5, int b6, int b7, int b8)
        {
            byte pixelChannelColorValue = (byte)pixelChannelColor;
            Debug.WriteLine("Testing ColorChannelToBinList method =============");
            List<int> expectedOutput = new() { b1, b2, b3, b4, b5, b6, b7, b8 };
            PrintArray(expectedOutput.ToArray(),"expected output");
            List<int> outputFromMethod = VideoLibrary.Video.ColorChannelToBinList(pixelChannelColorValue);
            PrintArray(outputFromMethod.ToArray(), "from method");
            Assert.IsTrue(expectedOutput == outputFromMethod);
        }
        private static void PrintArray(int[] a, string b)
        {
            Debug.Write($"{b}: ");
            foreach( int i in a)
            {
                Debug.Write($"{i}_");
            }
            Debug.WriteLine("");
        }
    }

}
