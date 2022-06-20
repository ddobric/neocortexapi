using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MnistDataGen
{
    [TestClass]
    public class MnistIOTest
    {
        [TestMethod]
        [TestCategory("Mnist Dataset")]
        public void DataGenTest()
        {
            Mnist.DataGen("MnistDataSet", "TrainingFolder", 7);
        }
    }
}
