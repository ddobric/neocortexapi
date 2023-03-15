using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Net.Http.Headers;
using Naa = NeoCortexApi.NeuralAssociationAlgorithm;
using System.Diagnostics;


namespace UnitTestsProject
{
    /// <summary>
    /// UnitTests for the Cell.
    /// </summary>
    [TestClass]
    public class SynapseTests
    {
        [TestMethod]
        [TestCategory("Prod")]
        public void SynapseCompareTest()
        {
            Cell c1 = new Cell(0, 0);
            Cell c2 = new Cell(0, 1);

            Synapse s1 = new Synapse(c1, 0, 0, 0, "a1", 1.0);

            Synapse s2 = new Synapse(c1, 0, 0, 0, "a1", 1.0);

            Synapse s3 = new Synapse(c1, 0, 0, 0, "a2", 1.0);

            Synapse s4 = new Synapse(c1, 0, 1, 0, "a1", 1.0);

            Synapse s5 = new Synapse(c1, 0, 0, 1, "a1", 1.0);

            Synapse s6 = new Synapse(c1, 0, 0, 0, "a1", 0.2);

            Assert.AreEqual(s2, s2);

            Assert.AreNotEqual(s2, s3);

            Assert.AreNotEqual(s1, s4);

            Assert.AreNotEqual(s1, s5);

            Assert.AreNotEqual(s1, s6);
        }

        [TestMethod]
        // [TestCategory("Prod")]
        public void SynapseConnectionTest()
        {
            Cell c0 = new Cell(0, 0);

            List<Cell> population = new List<Cell> { /* c10 */new Cell(1, 0), /* c11 */ new Cell(1, 1), /* c12 */ new Cell(1, 2) };

            ApicalDendrite s10 = new ApicalDendrite(population[0], 0, 0, 0, 0.5, -1);
            ApicalDendrite s11 = new ApicalDendrite(population[1], 1, 0, 1, 0.5, -1);
            ApicalDendrite s12 = new ApicalDendrite(population[2], 2, 0, 2, 0.5, -1);

            s10.ParentCell.ApicalDendrites.Add(s10);
            s11.ParentCell.ApicalDendrites.Add(s11);
            s12.ParentCell.ApicalDendrites.Add(s12);

            //
            // Connects the c0 with c10
            s10.Synapses.Add(new Synapse(c0, 0, s10.SegmentIndex, s10.ParentCell.Index, "Y", 0.6));
            
            c0.ReceptorSynapses.Add(s10.Synapses[0]);

            var connectedCells = Helpers.GetDistalConnectedCells(c0, population);
            Assert.AreEqual(0, connectedCells.Count);

            connectedCells = Helpers.GetApicalConnectedCells(c0, population);
            Assert.AreEqual(1, connectedCells.Count);
            Assert.IsTrue(connectedCells[0].Equals(population[0]));
        }
    }
}
