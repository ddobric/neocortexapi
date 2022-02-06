using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class BoostingTests
    {
        /// <summary>
        /// Generates 50% activation amd makes sure that the boost result is 
        /// </summary>
        /// <param name="period"></param>
        [TestMethod]
        [TestCategory("Prod")]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(50000)]
        public void BoostTest50Pct(int period)
        { 
            int numCols = 1;
        
            double[] overallActivity = new double[numCols];
            double[] currentAtivity = new double[numCols];

            double max = 0.0;

            for (int i = 0; i < period*100; i++)
            {
                // Active in every cycle.
                currentAtivity[0] = currentAtivity[0] ==1? 0:1;

                // Calculate boost result by boosting formel.
                overallActivity = SpatialPooler.CalcEventFrequency(overallActivity, currentAtivity, period);

                if (overallActivity[0] > max)
                    max = overallActivity[0];

                //Trace.WriteLine(Helpers.StringifyVector(overallActivity));
            }

            Assert.IsTrue(max <= 0.51);
            Assert.IsTrue(max >= 0.49);
        }

        [TestMethod]
        [TestCategory("Prod")]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(50000)]
        public void BoostTest100Pct(int period)
        {
            int numCols = 1;

            double[] overallActivity = new double[numCols];
            double[] currentAtivity = new double[numCols];

            double max = 0.0;

            for (int i = 0; i < period*100; i++)
            {
                // Active in every cycle.
                currentAtivity[0] = 1;

                // Calculate boost result by boosting formel.
                overallActivity = SpatialPooler.CalcEventFrequency(overallActivity, currentAtivity, period);

                if (overallActivity[0] > max)
                    max = overallActivity[0];

                //Trace.WriteLine(Helpers.StringifyVector(overallActivity));
            }

            Assert.IsTrue(max <= 1.00);
            Assert.IsTrue(max >= 0.99);
        }
    }
}
