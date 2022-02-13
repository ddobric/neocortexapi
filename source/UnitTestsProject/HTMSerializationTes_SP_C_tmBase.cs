using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Diagnostics;

namespace UnitTestsProject
{
    [TestClass]
    public class HTMSerializationTes_SP_C_tmBase
    {
        [TestMethod]
        [TestCategory("Serialization")]
        public void PartialConnection()

        {
            var partialmemory = GetDefaultPartialmemory();

            partialmemory.setInputDimensions(new int[] { 35 });
            partialmemory.setColumnDimensions(new int[] { 136 });
            partialmemory.setNumActiveColumnsPerInhArea(0.02 * 128);

            var spartial = new SpatialPooler();

            var mem = new Connections();
            partialmemory.apply(mem);
            spartial.Init(mem);

            int[] activeArray = new int[128];

            int[] inputVector = Helpers.GetRandomVector(32, partialmemory.Get<Random>(KEY.RANDOM));

            for (int i = 0; i < 100; i++)
            {
                spartial.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str);
            }
        }
    }
}