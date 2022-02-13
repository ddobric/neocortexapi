using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;


namespace UnitTestsProject
{
    [TestClass]
    public class HTMSerialization_Deserialization
    {
        [TestMethod]
        [TestCategory("DeSerialization")]
        public void DeserializeValueTest()
        {
            HtmDeserializer2 htm = new HtmDeserializer2();

            using (StreamWriter sw = new StreamWriter("ser.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                htm.SerializeValue(12, sw);
                htm.SerializeValue(15.34, sw);
                htm.SerializeValue(8765421, sw);
                htm.SerializeValue("olleH", sw);
                htm.SerializeValue(true, sw);
                htm.SerializeEnd("UnitTest", sw);
            }

        }

}
