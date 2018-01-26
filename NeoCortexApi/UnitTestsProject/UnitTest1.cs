using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
