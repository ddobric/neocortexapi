using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Utility;

namespace UnitTestsProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [DataRow(new int[] { 2048, 6})]
        [DataRow(new int[] { 100, 20 })]
        public void TestMethod1(int[] data)
        {
            var res = initDimensionMultiples(data, 3);
        }

        protected int[] initDimensionMultiples(int[] dimensions, int numDimensions)
        {
            int holder = 1;
            int len = dimensions.Length;
            int[] dimensionMultiples = new int[numDimensions];
            for (int i = 0; i < len; i++)
            {
                holder *= (i == 0 ? 1 : dimensions[len - i]);
                dimensionMultiples[len - 1 - i] = holder;
            }
            return dimensionMultiples;
        }


        [TestMethod]
        public void TestMethod2()
        {
            Topology t = new Topology(new int[] { 2048, 40});
            int[] coords = new int[] { 200,10};
            var indx = t.indexFromCoordinates(coords);
        }


        [TestMethod]
        public void TestMethod3()
        {
            Topology t = new Topology(new int[] { 2048, 40 });
            int[] coords = new int[] { 200, 10 };
            var indx = t.indexFromCoordinates(coords);
            var coords2 = t.computeCoordinates(indx);

            Assert.AreEqual(coords[0], coords2[0]);
            Assert.AreEqual(coords[1], coords2[1]);
        }

    }
}
