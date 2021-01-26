using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;

namespace UnitTests.Parallel
{
    [TestClass]
    public class SPParallelTests
    {
        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Parallel")]
        public void SPInitTest()
        {
            //Thread.Sleep(2000);

            int numOfColsInDim = 12;
            int numInputs = 128;

            Parameters parameters = Parameters.getAllDefaultParameters();

            parameters.Set(KEY.POTENTIAL_RADIUS, 5);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            parameters.Set(KEY.INPUT_DIMENSIONS, new int[] { numInputs, numInputs });
            parameters.Set(KEY.COLUMN_DIMENSIONS, new int[] { numOfColsInDim, numOfColsInDim });
            parameters.setPotentialRadius(5);

            //This is 0.3 in Python version due to use of dense 
            // permanence instead of sparse (as it should be)
            parameters.setPotentialPct(0.5);

            parameters.setGlobalInhibition(false);
            parameters.setLocalAreaDensity(-1.0);
            parameters.setNumActiveColumnsPerInhArea(3);
            parameters.setStimulusThreshold(1);
            parameters.setSynPermInactiveDec(0.01);
            parameters.setSynPermActiveInc(0.1);
            parameters.setMinPctOverlapDutyCycles(0.1);
            parameters.setMinPctActiveDutyCycles(0.1);
            parameters.setDutyCyclePeriod(10);
            parameters.setMaxBoost(10);
            parameters.setSynPermTrimThreshold(0);

            //This is 0.5 in Python version due to use of dense 
            // permanence instead of sparse (as it should be)
            parameters.setPotentialPct(1);

            parameters.setSynPermConnected(0.1);

            //SpatialPooler sp = UnitTestHelpers.CreatePooler(poolerMode) ;            
            var sp = new SpatialPoolerParallel();
            var mem = new Connections();
            parameters.apply(mem);

            sp.Init(mem, UnitTestHelpers.GetDistributedDictionary(mem.HtmConfig));

            //sp.init(mem);

            //int[] inputVector = new int[] { 1, 0, 1, 0, 1, 0, 0, 1, 1 };
            //int[] activeArray = new int[] { 0, 0, 0, 0, 0 };
            //for (int i = 0; i < 20; i++)
            //{
            //    sp.compute(mem, inputVector, activeArray, true);
            //}           
        }


    }
}
