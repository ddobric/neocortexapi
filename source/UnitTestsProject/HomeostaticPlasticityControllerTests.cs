using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestsProject
{
    /// <summary>
    /// Tests related to HomeostaticPlasticityControllerTests.
    /// </summary>
    [TestClass]
    public class HomeostaticPlasticityControllerTests
    {
        /// <summary>
        /// HomeostaticPlasticityControllerTests Equal method tests.
        /// </summary>
        [TestMethod]
        public void CompareHomestaticPlasticityControllers()
        {
            HtmConfig config = new HtmConfig(new int[] { 100, 100 }, new int[] { 10, 10 });
            HtmConfig config2 = new HtmConfig(new int[] { 10, 10 }, new int[] { 10, 10 });

            Connections htmMemory = new Connections(config);
            Connections htmMemory2 = new Connections(config2);

            int minCycles = 50;
            int minCycles2 = 70;

            int numOfCyclesToWaitOnChange = 50;
            int numOfCyclesToWaitOnChange2 = 52;

            double requiredSimilarityThreshold = 0.97;
            double requiredSimilarityThreshold2 = 0.98;

            Action<bool, int, double, int> onStabilityStatusChanged = (isStable, numPatterns, actColAvg, seenInputs) => { };            

            HomeostaticPlasticityController controller1 = new HomeostaticPlasticityController(htmMemory, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange, requiredSimilarityThreshold);
            HomeostaticPlasticityController controller2 = new HomeostaticPlasticityController(htmMemory, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange, requiredSimilarityThreshold);
            HomeostaticPlasticityController controller3 = new HomeostaticPlasticityController(htmMemory2, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange, requiredSimilarityThreshold);
            HomeostaticPlasticityController controller4 = new HomeostaticPlasticityController(htmMemory, minCycles2, onStabilityStatusChanged, numOfCyclesToWaitOnChange, requiredSimilarityThreshold);
            HomeostaticPlasticityController controller5 = new HomeostaticPlasticityController(htmMemory, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange2, requiredSimilarityThreshold);
            HomeostaticPlasticityController controller6 = new HomeostaticPlasticityController(htmMemory, minCycles, onStabilityStatusChanged, numOfCyclesToWaitOnChange, requiredSimilarityThreshold2);

            //Not same by reference
            Assert.IsFalse(controller1 == controller2);
            Assert.IsFalse(controller1 == controller3);
            Assert.IsFalse(controller1 == controller4);
            Assert.IsFalse(controller1 == controller5);
            Assert.IsFalse(controller1 == controller6);

            //controller1 and controller2 are same by value
            Assert.IsTrue(controller1.Equals(controller2));

            //controller1 and controller3 are NOT same by value (different htmMemory input)
            Assert.IsFalse(controller1.Equals(controller3));

            //controller1 and controller4 are NOT same by value (different minCycles input)
            Assert.IsFalse(controller1.Equals(controller4));

            //controller1 and controller5 are NOT same by value (different numOfCyclesToWaitOnChange input)
            Assert.IsFalse(controller1.Equals(controller5));

            //controller1 and controller6 are NOT same by value (different requiredSimilarityThreshold input)
            Assert.IsFalse(controller1.Equals(controller6));
        }
    }
}
