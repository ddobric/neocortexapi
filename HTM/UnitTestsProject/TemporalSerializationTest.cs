using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;
using NeoCortexApi.Utility;
using System.Diagnostics;
using NeoCortex;

namespace UnitTestsProject
{

    [TestClass]
    public class TemporalMemorySerializationTest
    {
        private Parameters getDefaultParameters()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 4);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.21);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.10);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationDeSerializationBasicTest()
        {

            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

           
            Cell cell4 = cn.getCell(4);

            
            ISet<Cell> expectedActiveCells = new HashSet<Cell>(new Cell[] { cell4 });

       
            DistalDendrite activeSegment = cn.CreateDistalSegment(cell4);

         
            cn.createSynapse(activeSegment, cn.getCell(0), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(1), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(2), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(3), 0.5);
           
          
                ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
                Assert.IsTrue(cc.predictiveCells.SequenceEqual(expectedActiveCells));
           
            tm.Serializer("tmSerializeNew.json");
            var tm1 = TemporalMemory.Deserializer("tmSerializeNew.json");
         
          
                ComputeCycle cc2 = tm1.Compute(activeColumns, true) as ComputeCycle;
                Assert.IsTrue(cc2.ActiveCells.SequenceEqual(expectedActiveCells));
         
        }


        [TestMethod]
        [TestCategory("LongRunning")]
        public void SerializationDeSerializationTest()
        {
            TemporalMemory tm1 = new TemporalMemory();
            Connections cn1 = new Connections();
            Parameters p1 = getDefaultParameters();
            p1.apply(cn1);
            tm1.init(cn1);
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

            Cell cell4 = cn1.getCell(4);
            ISet<Cell> expectedActiveCells = new HashSet<Cell>(new Cell[] { cell4 });
            DistalDendrite activeSegment = cn1.CreateDistalSegment(cell4);
            cn1.createSynapse(activeSegment, cn1.getCell(0), 0.5);
            cn1.createSynapse(activeSegment, cn1.getCell(1), 0.5);
            cn1.createSynapse(activeSegment, cn1.getCell(2), 0.5);
            cn1.createSynapse(activeSegment, cn1.getCell(3), 0.5);
            ComputeCycle cc1 = new ComputeCycle();
            for (int i = 0; i < 5; i++)
            {
                cc1 = tm1.Compute(previousActiveColumns, true);
            }
            Assert.IsTrue(cc1.predictiveCells.SequenceEqual(expectedActiveCells));
      
         tm1.Serializer("tmTTrainSerialized.json");
         string ser1 = File.ReadAllText("tmTTrainSerialized.json");
            var tm2 = TemporalMemory.Deserializer("tmTTrainSerialized.json");
            ComputeCycle cc2 = new ComputeCycle();
           
       
              cc2=  tm2.Compute(activeColumns, true);
          
            Assert.IsTrue(cc2.ActiveCells.SequenceEqual(expectedActiveCells));
        }


    }
}