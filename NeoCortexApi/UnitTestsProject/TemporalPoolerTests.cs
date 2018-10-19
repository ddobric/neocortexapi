using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace UnitTestsProject
{
    [TestClass]
    public class TemporalPoolerTest
    {
        private Parameters getDefaultParameters()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.set(KEY.COLUMN_DIMENSIONS, new int[] { 32 });
            retVal.set(KEY.CELLS_PER_COLUMN, 4);
            retVal.set(KEY.ACTIVATION_THRESHOLD, 3);
            retVal.set(KEY.INITIAL_PERMANENCE, 0.21);
            retVal.set(KEY.CONNECTED_PERMANENCE, 0.5);
            retVal.set(KEY.MIN_THRESHOLD, 2);
            retVal.set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            retVal.set(KEY.PERMANENCE_INCREMENT, 0.10);
            retVal.set(KEY.PERMANENCE_DECREMENT, 0.10);
            retVal.set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.set(KEY.RANDOM, new Random(42));
            retVal.set(KEY.SEED, 42);

            return retVal;
        }


        private Parameters getDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters() : p;
            retVal.set(key, value);

            return retVal;
        }


        private T deepCopyPlain<T>(T obj)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return (T)formatter.Deserialize(stream);
            }
            
            //FSTConfiguration fastSerialConfig = FSTConfiguration.createDefaultConfiguration();
            //byte[] bytes = fastSerialConfig.asByteArray(t);
            //return (T)fastSerialConfig.asObject(bytes);
        }

        [TestMethod]
        public void testActivateCorrectlyPredictiveCells()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            TemporalMemory.init(cn);

            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

            // Cell4 belongs to column with index 1.
            Cell cell4 = cn.getCell(4);

            // ISet<Cell> expectedActiveCells = Stream.of(cell4).collect(Collectors.toSet());
            ISet<Cell> expectedActiveCells = new HashSet<Cell>(new Cell[] { cell4 });           

            // We add distal dentrite at column1.cell4
            DistalDendrite activeSegment = cn.createSegment(cell4);

            //
            // We add here synapses between column0.cells[0-3] and segment.
            cn.createSynapse(activeSegment, cn.getCell(0), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(1), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(2), 0.5);
            cn.createSynapse(activeSegment, cn.getCell(3), 0.5);

            ComputeCycle cc = tm.Compute(cn, previousActiveColumns, true);
            Assert.IsTrue(cc.predictiveCells.SequenceEqual(expectedActiveCells));

            ComputeCycle cc2 = tm.Compute(cn, activeColumns, true);
            Assert.IsTrue(cc2.activeCells.SequenceEqual(expectedActiveCells));
        }
    }
     

}
