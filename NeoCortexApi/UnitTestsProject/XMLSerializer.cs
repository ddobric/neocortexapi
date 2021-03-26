using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using NeoCortexApi.Utility;
//using NeoCortex;
using Newtonsoft.Json;

using Newtonsoft.Json.Serialization;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerSerializeTests
    {
        private static Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 80.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);
            parameters.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true);


            return parameters;
        }


        public class mySP : SpatialPooler
        {
            public mySP() : base(null)
            {
            }
        }
        [TestMethod]
        public void SerializationTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 16 * 16 });
            parameters.setColumnDimensions(new int[] { 32 * 32 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 32 * 32);
            parameters.setGlobalInhibition(true);

            SpatialPooler sp = new mySP();
            var mem = new Connections();
            parameters.apply(mem);
            sp.Init(mem);

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(sp.GetType());
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "//SerializationOverview.xml";
            System.IO.FileStream wfile = System.IO.File.Create(path);
            writer.Serialize(wfile, sp);
            wfile.Close();

            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(sp.GetType());
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            SpatialPooler sp2 = (mySP)reader.Deserialize(file);
            file.Close();
        }

    }

}
