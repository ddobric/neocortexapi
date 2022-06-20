using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    /// <summary>
    /// Compares the similarity of multiple SDRs.
    /// </summary>
    [TestClass]
    public class TraceSDRsTest
    {
        /// <summary>
        /// Compares the similarity of multiple SDRs.
        /// <see cref=""/>
        /// </summary>
        [TestMethod]
        [TestCategory("Trace SDR")]
        public void StringifyTest()
        {

            //First SDR vector
            var list = new int[] { 51, 76, 87 };
            //Second SDR vector
            var list1 = new int[] { 51, 76, 113 };
            //Stores the two SDRs
            var output = Helpers.StringifySdr(new List<int[]> { list, list1 });

            /// <summary>
            /// This result stores the SDR vectors same as output for the comparison. 
            /// </summary>
            /// <param name="expectedResult"></param>
            /// <returns></returns>
            var expectedResult = new StringBuilder();
            var sdr1 = new StringBuilder();
            sdr1.Append("51, 76, 87,    , ");

            var sdr2 = new StringBuilder();
            sdr2.Append("51, 76,   , 113, ");

            expectedResult.AppendLine(sdr1.ToString());
            expectedResult.AppendLine(sdr2.ToString());

            Console.WriteLine($"{output}");
            var expectedOutput = "51, 76, 87,    ,  \n51, 76,   , 113,";

            //Tests if both have same values 
            Assert.IsTrue(output == expectedResult.ToString());

            //Assert.IsTrue(output == expectedOutput);
            Debug.WriteLine($"{expectedOutput}");
        }

        /// <summary>
        /// This method implements the well formatted pattern of SDR values from the HTM. 
        /// </summary>
        [TestMethod]
        [TestCategory("Trace SDR")]
        public void LongerIntergerArray()
        {
            var list = new int[] { 51, 76, 87, 113, 116, 118, 122, 152, 156, 163, 179, 181, 183, 186, 188, 190, 195, 210, 214, 224, };
            var list1 = new int[] { 51, 76, 113, 116, 118, 156, 163, 179, 181, 182, 183, 186, 188, 190, 195, 197, 210, 214, 224, 243 };

            var output = Helpers.StringifySdr(new List<int[]> { list, list1 });
            Debug.WriteLine(output);
        }
    }
}
